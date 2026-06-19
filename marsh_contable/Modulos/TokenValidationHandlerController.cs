using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace marsh_contable.Modulos
{
    public class TokenValidationHandlerController : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var rutasPublicas = new List<string>
        {
            "api/v1/login",
            "api/v1/auth/salt",
            "api/v1/login/recover",
            "api/v1/login/validate-code",
            "api/v1/login/confirm-change-password"
        };

            string requestUri = request.RequestUri.AbsolutePath.ToLower().TrimStart('/');
            bool esPublica = rutasPublicas.Any(r => requestUri.Contains(r));

            if (esPublica)
                return await base.SendAsync(request, cancellationToken);

            // ── PASO 1: Leer sessionId del header
            string sessionId = null;

            if (request.Headers.Contains("X-Session-Id"))
                sessionId = request.Headers.GetValues("X-Session-Id").FirstOrDefault();

            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized("session_id_missing");

            // ── PASO 2: Recuperar JWT del caché usando el sessionId
            string jwt = HttpRuntime.Cache[$"session_{sessionId}"] as string;

            if (string.IsNullOrEmpty(jwt))
                return Unauthorized("session_expired_or_invalid");

            // ── PASO 3: Validar el JWT internamente
            try
            {
                var secretKey = ConfigurationManager.AppSettings["JWT_SECRET_KEY"];
                var audienceToken = ConfigurationManager.AppSettings["JWT_AUDIENCE_TOKEN"];
                var issuerToken = ConfigurationManager.AppSettings["JWT_ISSUER_TOKEN"];

                var securityKey = new SymmetricSecurityKey(
                    System.Text.Encoding.Default.GetBytes(secretKey)
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParams = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = issuerToken,
                    ValidateAudience = true,
                    ValidAudience = audienceToken,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(
                    jwt,
                    validationParams,
                    out validatedToken
                );

                // ── PASO 4: Renovar caché si quedan menos de 10 minutos
                var jwtToken = validatedToken as JwtSecurityToken;
                var minutosRestantes = (jwtToken.ValidTo - DateTime.UtcNow).TotalMinutes;

                if (minutosRestantes < 10)
                {
                    int expireMinutes = Convert.ToInt32(
                        ConfigurationManager.AppSettings["JWT_EXPIRE_MINUTES"]
                    );

                    HttpRuntime.Cache.Insert(
                        key: $"session_{sessionId}",
                        value: jwt,
                        dependencies: null,
                        absoluteExpiration: DateTime.Now.AddMinutes(expireMinutes),
                        slidingExpiration: System.Web.Caching.Cache.NoSlidingExpiration
                    );
                }

                // ── PASO 5: Inyectar principal al contexto
                Thread.CurrentPrincipal = principal;
                if (HttpContext.Current != null)
                    HttpContext.Current.User = principal;

                // Pasar el sessionId al contexto para usarlo en los controllers
                request.Properties["sessionId"] = sessionId;

                return await base.SendAsync(request, cancellationToken);
            }
            catch (SecurityTokenExpiredException)
            {
                HttpRuntime.Cache.Remove($"session_{sessionId}");
                return Unauthorized("session_expired");
            }
            catch (Exception)
            {
                return Unauthorized("session_invalid");
            }
        }

        private HttpResponseMessage Unauthorized(string mensaje)
        {


            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {

                Content = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        codeStatus = 401,
                        message = mensaje
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };

        }
    }
}