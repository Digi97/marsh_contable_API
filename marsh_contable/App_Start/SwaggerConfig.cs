using System.Linq;
using System.Web;
using System.Web.Http;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(marsh_contable.App_Start.SwaggerConfig), "Register")]

namespace marsh_contable.App_Start
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration
    .EnableSwagger(c =>
    {
        c.SingleApiVersion("v1", "Marsh Contable API")
         .Description("API del Sistema de Gestión Financiero Contable - Marsh Asprose");

        // ── Resolver rutas duplicadas
        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

        var xmlPath = System.AppDomain.CurrentDomain.BaseDirectory + @"bin\marsh_contable.xml";
        if (System.IO.File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath);

        c.ApiKey("X-Session-Id")
         .Description("Session ID para autenticación")
         .Name("X-Session-Id")
         .In("header");

        c.DescribeAllEnumsAsStrings();
    })
    .EnableSwaggerUi(c =>
    {
        c.DocumentTitle("Marsh Contable - API Docs");
        c.DocExpansion(DocExpansion.List);
        c.EnableApiKeySupport("X-Session-Id", "header");
    });
        }
    }
}