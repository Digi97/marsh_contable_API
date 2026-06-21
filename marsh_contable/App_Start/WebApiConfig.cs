using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using marsh_contable.Controllers;
using System.Web.Http.Cors;
using System.Configuration;

namespace marsh_contable
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {


            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Add(new Modulos.TokenValidationHandlerController());


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
