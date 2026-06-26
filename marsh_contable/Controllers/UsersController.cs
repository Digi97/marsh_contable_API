using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Http;
using System.Net;
using marsh_contable.Models;
using System.Configuration;
using marsh_contable.Modulos;


namespace marsh_contable.Controllers
{

    public class UsersController : ApiController
    {

      

        [HttpPost]
        [Authorize]
        [Route("api/v1/users")]
        [RequierePermiso(PermisosAplica.AdministracionUsuarios)]
        public Reply CreateUser([FromBody] Models.Usuarios model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
          
           
            try
            {

                if(model==null)
                {
                    throw new Exception("invalid_model_request_missing");
                }
                // Seccion de validacion de datos 
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }
                if (!tool.ValidaTexto(model.Apellido1))
                {
                    throw new Exception("invalid_string_form_Apellido1");
                }

                if (!tool.ValidaTexto(model.Apellido2))
                {
                    throw new Exception("invalid_string_form_Apellido2");
                }

                if (!tool.ValidaCorreo(model.Correo))
                {
                    throw new Exception("invalid_string_form_Correo");
                }
               
                if (!tool.validaNumeros(model.Roles_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Roles_id");
                }


                if (!tool.validaPassword(model.Contrasena))
                {
                    throw new Exception("invalid_value_for_Contrasena");
                }
                //fin de validaciones


                String password = tool.Encriptar(model.Contrasena);

                using (var ctx = new Models.EntitiesModel())
                {
                    var userExist = ctx.Usuarios
                         .Where(u =>
                             u.Correo == model.Correo && 
                             u.activo == 1)
                         .Select(u => new Models.UsuariosViewModel
                         {
                             Usuario_id = u.Usuario_id,
                         }).FirstOrDefault();
                    // Usuario no encontrado
                    if (userExist != null)
                    {
                        throw new Exception("user_already_exist");
                    }

                    Models.Usuarios nuevoUsuario = new Models.Usuarios()
                    {
                        Nombre = model.Nombre,
                        Apellido1 = model.Apellido1,
                        Apellido2 = model.Apellido2,
                        Correo = model.Correo,
                        Contrasena = password,
                        Roles_id = model.Roles_id,
                        Id_Empleado = model.Id_Empleado,
                        activo = (Int16) model.activo,
                        Fec_Login = DateTime.Now,
                        Fec_Actualizacion = DateTime.Now,
                        Fec_creacion = DateTime.Now,
                        Empresa_Emp_id = 1

                    };
                    ctx.Usuarios.Add(nuevoUsuario);
                    ctx.SaveChanges();

            
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = nuevoUsuario.Usuario_id; // retorna el ID generado                 

                }

            }
            catch (Exception ex)

            {

                System.Data.Entity.Validation.DbEntityValidationException ex2 = new System.Data.Entity.Validation.DbEntityValidationException();
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                { 
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
       
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message +" "+ errorDB;
            }
            return oR;
        }


        [HttpPut]
        [Authorize]
        [Route("api/v1/users/{id}")]
        [RequierePermiso(PermisosAplica.AdministracionUsuarios)]
        public Reply UpdateUser(int id, [FromBody] Models.Usuarios model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();

            try
            {
                if (model == null)
                {
                    throw new Exception("invalid_model_request_missing");
                }
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }
                if (!tool.ValidaTexto(model.Apellido1))
                {
                    throw new Exception("invalid_string_form_Apellido1");
                }
                if (!tool.ValidaTexto(model.Apellido2))
                {
                    throw new Exception("invalid_string_form_Apellido2");
                }
                if (!tool.ValidaCorreo(model.Correo))
                {
                    throw new Exception("invalid_string_form_Correo");
                }
                if (!tool.validaNumeros(model.Roles_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Roles_id");
                }
                // fin de validaciones

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Usuarios usuarioExistente = ctx.Usuarios.FirstOrDefault(u => u.Usuario_id == id);

                    if (usuarioExistente == null)
                    {
                        throw new Exception("user_not_found");
                    }

                    usuarioExistente.Nombre = model.Nombre;
                    usuarioExistente.Apellido1 = model.Apellido1;
                    usuarioExistente.Apellido2 = model.Apellido2;
                    usuarioExistente.Correo = model.Correo;
                    usuarioExistente.Roles_id = model.Roles_id;
                    usuarioExistente.Id_Empleado = model.Id_Empleado;
                    usuarioExistente.activo = (Int16)model.activo;
                    usuarioExistente.Fec_Actualizacion = DateTime.Now;
                        /* Fec_Login = DateTime.Now,
                        Fec_Actualizacion = DateTime.Now,
                        Fec_creacion = DateTime.Now,*/

                    // Solo re-encripta la contraseña si se envía una nueva
                    if (!string.IsNullOrEmpty(model.Contrasena))
                    {
                        if (!tool.validaPassword(model.Contrasena))
                        {
                            throw new Exception("invalid_value_for_Contrasena");
                        }
                        usuarioExistente.Contrasena = tool.Encriptar(model.Contrasena);
                    }

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = usuarioExistente.Usuario_id;

                    return oR;
                }
            }

            catch(System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
           
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex)
            {
               
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }


        [HttpGet]
        [Authorize]
        [Route("api/v1/users")]
        public Reply GetAllUsers()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;

            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    List<Models.UsuariosViewModel> usuarios = (
                from u in ctx.Usuarios
                join r in ctx.Roles
                    on u.Roles_id equals r.id
                select new Models.UsuariosViewModel
                {
                    Usuario_id = u.Usuario_id,
                    Nombre = u.Nombre,
                    Apellido1 = u.Apellido1,
                    Apellido2 = u.Apellido2,
                    Correo = u.Correo,
                    Roles_id = u.Roles_id,
                    Id_Empleado = u.Id_Empleado,
                    activo = u.activo,
     
             Rol = r.Descripcion
 }
            ).OrderByDescending(x => x.Usuario_id).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = usuarios;
                }
            }
            catch (Exception ex)
            {
                System.Data.Entity.Validation.DbEntityValidationException ex2 = new System.Data.Entity.Validation.DbEntityValidationException();
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message + " " + errorDB;
            }
            return oR;
        }


        [HttpGet]
        [Authorize]
        [Route("api/v1/users/{id}")]
        public Reply GetUserById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;

            try
            {
                if (id <= 0)
                {
                    throw new Exception("invalid_value_for_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.UsuariosViewModel usuario = ctx.Usuarios
                        .Where(u => u.Usuario_id == id)
                        .Select(u => new Models.UsuariosViewModel
                        {
                            Usuario_id = u.Usuario_id,
                            Nombre = u.Nombre,
                            Apellido1 = u.Apellido1,
                            Apellido2 = u.Apellido2,
                            Correo = u.Correo,
                            Roles_id = u.Roles_id,
                            Id_Empleado = u.Id_Empleado,
                            activo = u.activo,
                            Fec_Actualizacion = (DateTime)u.Fec_Actualizacion,
                            Fec_Login = (DateTime)u.Fec_Login
                    // Contrasena se omite intencionalmente por seguridad
                })
                        .FirstOrDefault();

                    if (usuario == null)
                    {
                        throw new Exception("user_not_found");
                    }

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = usuario;
                }
            }
            catch (Exception ex)
            {
                System.Data.Entity.Validation.DbEntityValidationException ex2 = new System.Data.Entity.Validation.DbEntityValidationException();
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message + " " + errorDB;
            }
            return oR;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("api/v1/login")]
        public Reply Login([FromBody] Models.Usuarios model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();

            try
            {
                // Validar body null
                if (model == null)
                {
                    throw new Exception("invalid_model");
                }

                // Validar campos vacíos
                if (string.IsNullOrWhiteSpace(model.Correo) ||
                    string.IsNullOrWhiteSpace(model.Contrasena))
                {
                    throw new Exception("invalid_value_form_correo_or_contrasena");
                }

                if (!tool.ValidaCorreo(model.Correo))
                {
                    throw new Exception("invalid_value_form_Correo");
                }

                if (!tool.validaPassword(model.Contrasena))
                {
                    throw new Exception("invalid_value_form_Contrasena");
                }

                using (var ctx = new Models.EntitiesModel())
                {
    
                    Models.Usuarios usuarioDB = ctx.Usuarios
                        .FirstOrDefault(u => u.Correo == model.Correo && u.activo == 1);

            
                    if (usuarioDB == null)
                    {
                        throw new Exception("invalid_username_or_password");
                    }

                    if (usuarioDB.Intentos_fallidos >= 3 && usuarioDB.Fecha_bloqueo != null)
                    {
                        double minutosTranscurridos = (DateTime.Now - usuarioDB.Fecha_bloqueo.Value).TotalMinutes;

                        if (minutosTranscurridos < 10)
                        {
                            int minutosRestantes = (int)Math.Ceiling(10 - minutosTranscurridos);
                            throw new Exception($"user_blocked_{minutosRestantes}_minutes");
                        }

                        usuarioDB.Intentos_fallidos = 0;
                        usuarioDB.Fecha_bloqueo = null;
                        ctx.SaveChanges();
                    }

   
                    string password = tool.Encriptar(model.Contrasena);

                    if (usuarioDB.Contrasena != password)
                    {
                        usuarioDB.Intentos_fallidos += 1;

                        if (usuarioDB.Intentos_fallidos >= 3)
                        {
                            usuarioDB.Fecha_bloqueo = DateTime.Now;
                            ctx.SaveChanges();
                            throw new Exception("user_blocked_10_minutes");
                        }

                        ctx.SaveChanges();

                        int intentosRestantes = 3 - usuarioDB.Intentos_fallidos;
                        throw new Exception($"invalid_username_or_password_{intentosRestantes}_attempts_remaining");
                    }

                    usuarioDB.Intentos_fallidos = 0;
                    usuarioDB.Fecha_bloqueo = null;
                    usuarioDB.Fec_Login = DateTime.Now;
                    ctx.SaveChanges();

                    var empresa = ctx.Empresa.FirstOrDefault(u => u.Emp_id ==   1);



                    var usuarioResponse = new Models.UsuariosViewModel
                    {
                        Usuario_id = usuarioDB.Usuario_id,
                        Nombre = usuarioDB.Nombre + " " + usuarioDB.Apellido1 + " "+ usuarioDB.Apellido2,
                        Correo = usuarioDB.Correo,
                        Roles_id = usuarioDB.Roles_id,
                        FormatoFecha = empresa.Formato_fecha,
                        ImpuestoDefault = (int) empresa.Impuesto_id
                    };

                    var permisos = (from pxr in ctx.Permisos_x_rol
                                    join p in ctx.Permisos on pxr.Permisos_id equals p.id
                                    where pxr.Roles_id == usuarioDB.Roles_id
                                    select new
                                    {
                                        p.id
                                    }).ToList();
                    var permisosJWT = permisos.Select(p => p.id).ToList();
                    usuarioResponse.Permisos = permisosJWT; //predefinido para permisos 


                    string jwt = tool.GenerateTokenJWT(model.Correo, permisosJWT);

                    // ── Generar sessionId opaco para el cliente
                    string sessionId = Guid.NewGuid().ToString("N");

                    int expireMinutes = Convert.ToInt32(
                        ConfigurationManager.AppSettings["JWT_EXPIRE_MINUTES"]
                    );

                    // ── Guardar JWT en caché del servidor con el sessionId como clave
                    HttpRuntime.Cache.Insert(
                        key: $"session_{sessionId}",
                        value: jwt,
                        dependencies: null,
                        absoluteExpiration: DateTime.Now.AddMinutes(expireMinutes),
                        slidingExpiration: System.Web.Caching.Cache.NoSlidingExpiration
                    );

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Message = sessionId;// tool.GenerateTokenJWT(model.Correo, permisosJWT);
                    oR.Data = usuarioResponse;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorDB = "";
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;

                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
            }
            return oR;
        }


        [HttpPost]
        [Authorize]
        [Route("api/v1/logout")]
        public Reply Logout()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                string sessionId = null;

                if (Request.Headers.Contains("X-Session-Id"))
                    sessionId = Request.Headers.GetValues("X-Session-Id").FirstOrDefault();

                if (!string.IsNullOrEmpty(sessionId))
                    HttpRuntime.Cache.Remove($"session_{sessionId}");

                oR.CodeStatus = HttpStatusCode.OK;
                oR.Message = "logout_successful";
                return oR;
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("api/v1/login/recover")]
        public Reply RecoverPassword([FromBody] Models.Usuarios model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();

            try
            {
                // Validar body null
                if (model == null)
                {
                    throw new Exception("invalid_model");
                }

                // Validar correo vacío y formato
                if (string.IsNullOrWhiteSpace(model.Correo))
                {
                    throw new Exception("invalid_value_form_correo");
                }
                if (!tool.ValidaCorreo(model.Correo))
                {
                    throw new Exception("invalid_value_form_Correo");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    // Verificar existencia del correo en BD
                    Models.Usuarios usuario = ctx.Usuarios
                        .FirstOrDefault(u => u.Correo == model.Correo && u.activo == 1);

                    if (usuario == null)
                    {
                        throw new Exception("correo_not_found");
                    }

                    // Generar código de recuperación seguro de 6 dígitos
                    string codigoRecuperacion;
                    using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                    {
                        byte[] bytes = new byte[4];
                        rng.GetBytes(bytes);
                        int codigo = (Math.Abs(BitConverter.ToInt32(bytes, 0)) % 900000) + 100000;
                        codigoRecuperacion = codigo.ToString();
                    }

                    // Almacenar el código en BD
                    usuario.Fec_Actualizacion = DateTime.Now;//actualizamos la ultima fecha de actualizacion para hacer valido el tiempo de cambio de clave
                    usuario.Codigo_recupera_clave = codigoRecuperacion;
                    ctx.SaveChanges();

                    // Enviar correo con el código

                    string cuerpoHtml = $@"
        <html>
        <body style='font-family: Arial, sans-serif; color: #333;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                <h2 style='color: #1F4E79;'>Recuperación de contraseña</h2>
                <p>Hola <strong>{model.Correo}</strong>,</p>
                <p>Recibimos una solicitud para recuperar tu contraseña. 
                   Usá el siguiente código para continuar con el proceso:</p>
                <div style='text-align: center; margin: 30px 0;'>
                    <span style='
                        font-size: 36px;
                        font-weight: bold;
                        letter-spacing: 8px;
                        color: #1F4E79;
                        background-color: #DCE6F1;
                        padding: 15px 30px;
                        border-radius: 8px;
                        display: inline-block;'>
                        {codigoRecuperacion}
                    </span>
                </div>
                <p>Este código es válido por <strong>15 minutos</strong>. 
                   Si no solicitaste recuperar tu contraseña, ignorá este mensaje.</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                <p style='font-size: 12px; color: #999;'>
                    Este es un correo automático, por favor no respondas a este mensaje.
                </p>
            </div>
        </body>
        </html>";


                    tool.Send_Mail(usuario.Correo, "Código de recuperación de contraseña", cuerpoHtml);

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Message = "recovery_code_sent";
                    oR.Data = new { correo = tool.HideMail(usuario.Correo) };
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorDB = "";
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;

                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
            }
            return oR;
        }



        [HttpPost]
        [AllowAnonymous]
        [Route("api/v1/login/validate-code")]
        public Reply ValidateRecoveryCode([FromBody] Models.Usuarios model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();

            try
            {
                // Validar body null
                if (model == null)
                {
                    throw new Exception("invalid_model");
                }

                // Validar que el código no venga vacío
                if (string.IsNullOrWhiteSpace(model.Codigo_recupera_clave))
                {
                    throw new Exception("invalid_value_form_Codigo_recupera_clave");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    // Buscar usuario por código de recuperación
                    Models.Usuarios usuario = ctx.Usuarios
                        .FirstOrDefault(u =>
                            u.Correo == model.Correo &&
                            u.Codigo_recupera_clave == model.Codigo_recupera_clave &&
                            u.activo == 1);

                    // Código no existe en ningún usuario activo
                    if (usuario == null)
                    {
                        throw new Exception("invalid_recovery_code");
                    }

                    // Validar que Fec_Actualizacion tenga valor
                    if (usuario.Fec_Actualizacion == null)
                    {
                        throw new Exception("invalid_recovery_code");
                    }

                    // Calcular minutos transcurridos desde la solicitud
                    double minutosTranscurridos = (DateTime.Now - usuario.Fec_Actualizacion.Value).TotalMinutes;

                    if (minutosTranscurridos > 15)
                    {
                        // Limpiar el código vencido para que no pueda reutilizarse
                        usuario.Codigo_recupera_clave = null;
                        ctx.SaveChanges();

                        throw new Exception("recovery_code_expired");
                    }

                    // limpiar el código para que no pueda usarse de nuevo
                    usuario.Codigo_recupera_clave = null;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Message = "code_validated";
                    oR.Data = usuario.Usuario_id;
              
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorDB = "";
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;

                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
            }
            return oR;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("api/v1/login/confirm-change-password")]
        public Reply ConfirmChangePassword([FromBody] Models.ChangePasswordViewModel model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();

            try
            {
                // Validar body null
                if (model == null)
                {
                    throw new Exception("invalid_model");
                }

                // Validar campos vacíos
                if (string.IsNullOrWhiteSpace(model.Correo))
                {
                    throw new Exception("invalid_value_form_Correo");
                }
                if (string.IsNullOrWhiteSpace(model.Contrasena))
                {
                    throw new Exception("invalid_value_form_Contrasena");
                }
                if (string.IsNullOrWhiteSpace(model.Contrasena_confirma))
                {
                    throw new Exception("invalid_value_form_Contrasena_confirma");
                }

                // Validar formato de correo
                if (!tool.ValidaCorreo(model.Correo))
                {
                    throw new Exception("invalid_format_Correo");
                }

                // Validar formato de contraseña
                if (!tool.validaPassword(model.Contrasena))
                {
                    throw new Exception("invalid_format_Contrasena");
                }

                // Validar que ambas contraseñas sean iguales
                if (model.Contrasena != model.Contrasena_confirma)
                {
                    throw new Exception("password_and_confirmation_no_match");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    // Buscar usuario activo por correo
                    Models.Usuarios usuario = ctx.Usuarios
                        .FirstOrDefault(u =>
                            u.Correo == model.Correo &&
                            u.activo == 1);

                    if (usuario == null)
                    {
                        throw new Exception("correo_not_found");
                    }

                    // Encriptar y actualizar la nueva contraseña
                    string nuevaContrasenaEncriptada = tool.Encriptar(model.Contrasena);

                    usuario.Contrasena = nuevaContrasenaEncriptada;
                    usuario.Fec_Actualizacion = DateTime.Now;
                    ctx.SaveChanges();

                    // Construir correo de confirmación
                    string cuerpoHtml = $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #1F4E79;'>Contraseña actualizada</h2>
                        <p>Hola <strong>{usuario.Nombre} {usuario.Apellido1}</strong>,</p>
                        <p>Tu contraseña ha sido actualizada exitosamente.</p>
                        <p>Si no realizaste este cambio, contacte al equipo de soporte de inmediato.</p>
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                        <p style='font-size: 12px; color: #999;'>
                            Este es un correo automático, por favor no respondas a este mensaje.
                        </p>
                    </div>
                </body>
                </html>";

                    tool.Send_Mail(usuario.Correo, "Contraseña actualizada correctamente", cuerpoHtml);

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Message = "password_changed_successfully";
                    oR.Data = new { correo = tool.HideMail(usuario.Correo) };
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorDB = "";
                foreach (var eve in ex.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;

                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
            }
            return oR;
        }

    }
}