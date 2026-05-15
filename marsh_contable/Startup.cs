using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Owin;
using Microsoft.IdentityModel.Tokens;
using System.Text;
[assembly: OwinStartup(typeof(marsh_contable.Startup))]
namespace marsh_contable
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            string secretKey = "TU_SECRET_KEY";

            var key = Encoding.ASCII.GetBytes(secretKey);

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,

                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),

                    ValidateIssuer = false,
                    ValidateAudience = false,

                    ValidateLifetime = true
                }
            });
        }
    }
}