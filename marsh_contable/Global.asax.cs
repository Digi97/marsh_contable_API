using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace marsh_contable
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {

            Exception ex = Server.GetLastError();

            // Ignorar error de ruta raíz
            if (ex?.Message?.Contains("la ruta de acceso '/'") == true ||
                ex?.Message?.Contains("does not implement IController") == true)
            {
                Server.ClearError();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== CRASH: {ex?.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"=== MESSAGE: {ex?.Message}");
            System.Diagnostics.Debug.WriteLine($"=== STACK: {ex?.StackTrace}");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            string origin = context.Request.Headers["Origin"];

            string path = context.Request.Path.ToLower();

            // No aplicar CORS a Swagger
            if (path.Contains("/swagger"))
                return;

            // Leer orígenes permitidos del Web.config
            string[] origenesPermitidos = (ConfigurationManager.AppSettings["CorsOrigins"] ?? "*")
                .Split(',');

            // Verificar si el origen está en la lista
            bool origenPermitido = origenesPermitidos.Contains("*") ||
                                   origenesPermitidos.Any(o => o.Trim() == origin);

            if (origenPermitido && !string.IsNullOrEmpty(origin))
                context.Response.AddHeader("Access-Control-Allow-Origin", origin);
            else
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");

            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Session-Id, Authorization, X-Requested-With");
            context.Response.AddHeader("Access-Control-Max-Age", "86400");

            // Responder inmediatamente al preflight OPTIONS
            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.StatusCode = 200;
                context.Response.Flush();
                // context.Response.End();
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }
    }
}