using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using marsh_contable.Controllers;
using System.Web.Http.Cors;

namespace marsh_contable
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuración y servicios de API web

            // Rutas de API web

            //var cors = new EnableCorsAttribute(
            //    origins: "http://localhost:3000 , http://192.168.10.182:3000",
            //    headers: "*",
            //    methods: "*"
            //);
            config.EnableCors();


          //  config.EnableCors();

            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Add(new TokenValidationHandlerController());


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var format = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            format.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        }
    }
}
