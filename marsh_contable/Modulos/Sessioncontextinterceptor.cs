using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace MarshAsprose.Infrastructure
{
    /// <summary>
    /// Contrato para resolver el usuario actual a partir del request HTTP.
    /// Se separa en una interfaz para no acoplar el interceptor de EF
    /// directamente a la implementación concreta (caché + JWT).
    /// </summary>
    public interface ICurrentUserAccessor
    {
        string ObtenerUsuarioIdActual();
    }

    /// <summary>
    /// Resuelve el Usuario_id del request actual siguiendo el mismo flujo que
    /// usa tu login:
    ///   1. Lee el header X-Session-ID.
    ///   2. Busca el JWT guardado en HttpRuntime.Cache bajo la clave
    ///      "session_{sessionId}" (la misma clave que usas al hacer login).
    ///   3. Decodifica el JWT y lee el claim "Usuario_id".
    ///
    /// NOTA: aquí solo se decodifica el JWT (ReadJwtToken), no se vuelve a
    /// validar firma/expiración. Se asume que eso ya lo hizo el handler/
    /// filtro de autenticación de la Web API antes de llegar a este punto.
    /// Este accessor es solo para fines de auditoría, no es un control de
    /// seguridad adicional.
    /// </summary>
    public class CacheCurrentUserAccessor : ICurrentUserAccessor
    {
        public string ObtenerUsuarioIdActual()
        {
            var request = HttpContext.Current?.Request;
            if (request == null)
                return null;

            var sessionId = request.Headers["X-Session-ID"];
            if (string.IsNullOrEmpty(sessionId))
                return null;

            var jwt = HttpRuntime.Cache[$"session_{sessionId}"] as string;
            if (string.IsNullOrEmpty(jwt))
                return null; // sesión expirada, inexistente o inválida

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);

                var usuarioIdClaim = token.Claims.FirstOrDefault(c => c.Type == "unique_name");
                if (!String.IsNullOrEmpty(usuarioIdClaim.Value))
                    return usuarioIdClaim.Value;
            }
            catch
            {
                // JWT corrupto/malformado: no interrumpe la operación de negocio,
                // la auditoría simplemente queda con Usuario_id NULL en este request.
            }

            return null;
        }
    }

    /// <summary>
    /// Al abrirse cualquier conexión SQL manejada por Entity Framework, este
    /// interceptor setea SESSION_CONTEXT('Usuario_id') con el id del usuario
    /// resuelto para el request actual. Los triggers de auditoría
    /// (trg_Auditoria_*) leen ese valor para llenar la columna Usuario_id
    /// en dbo.AuditaTabla.
    ///
    /// IMPORTANTE: EF6 clásico (System.Data.Entity, NO EF Core) NO tiene una
    /// clase base "DbConnectionInterceptor" con no-ops listos (eso solo existe
    /// en EF Core). Por eso aquí se implementa IDbConnectionInterceptor
    /// directamente. TODOS los métodos que no usamos deben quedar con cuerpo
    /// VACÍO, nunca con "throw new NotImplementedException()": EF dispara
    /// estos eventos en operaciones normales y muy frecuentes (leer
    /// connection.State, .ConnectionTimeout, .Database, etc.) apenas detecta
    /// que hay algún interceptor registrado, así que un throw ahí rompe la
    /// ejecución antes de llegar siquiera a Opened.
    /// </summary>
    public class SessionContextInterceptor : IDbConnectionInterceptor
    {
        private readonly ICurrentUserAccessor _currentUserAccessor;

        public SessionContextInterceptor(ICurrentUserAccessor currentUserAccessor)
        {
            _currentUserAccessor = currentUserAccessor;
        }

        // ---- Único método con lógica real ----
        public void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
        {
            var usuarioId = _currentUserAccessor.ObtenerUsuarioIdActual();
            if (usuarioId == null)
                return; // Ej: procesos batch/migraciones sin sesión HTTP -> Usuario_id queda NULL en la auditoría

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "EXEC sp_set_session_context @key = N'Usuario_id', @value = @UsuarioId;";

                var param = cmd.CreateParameter();
                param.ParameterName = "@UsuarioId";
                param.Value = usuarioId;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        // ---- Resto de la interfaz: no-op, cuerpo vacío a propósito ----
        public void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext) { }
        public void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext) { }
        public void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext) { }
        public void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext) { }
        public void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext) { }
        public void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext) { }
        public void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext) { }
        public void ConnectionStringSetting(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext) { }
        public void ConnectionStringSet(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext) { }
        public void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext) { }
        public void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext) { }
        public void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext) { }
        public void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext) { }
        public void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext) { }
        public void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext) { }
        public void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext) { }
        public void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext) { }
        public void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext) { }
        public void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext) { }
        public void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext) { }
        public void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext) { }
        public void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext) { }
        public void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext) { }
    }
}

/* ============================================================================
   REGISTRO (una sola vez, en Global.asax.cs -> Application_Start):

   protected void Application_Start()
   {
       System.Data.Entity.Infrastructure.Interception.DbInterception.Add(
           new MarshAsprose.Infrastructure.SessionContextInterceptor(
               new MarshAsprose.Infrastructure.CacheCurrentUserAccessor()));

       // ... resto de tu configuración existente (rutas, filtros, etc.)
   }

   VERIFICACIÓN SUGERIDA (en ese orden):
   1. Pon un breakpoint / Debug.WriteLine JUSTO en esa línea de DbInterception.Add
      dentro de Application_Start, y confirma que se ejecuta al arrancar la app.
      Si tu proyecto usa OWIN (Startup.cs con [assembly: OwinStartup] en vez de
      Global.asax), Application_Start puede no ser el punto de entrada real que
      esperas -> avísame si es tu caso.
   2. Si eso sí se ejecuta, confirma que tu DbContext se crea (y por lo tanto
      abre conexión) UNA VEZ POR REQUEST, no que reutiliza una conexión ya
      abierta desde el arranque de la app -> Opened solo dispara en la
      transición Closed -> Open.
   3. Ya con esta versión (sin throws), vuelve a poner el breakpoint en Opened
      y prueba.
============================================================================ */