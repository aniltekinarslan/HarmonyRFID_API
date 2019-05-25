using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;

namespace HarmonyRFID_API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // JSON RESULT FORMATTER
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            ////////////////////////

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
