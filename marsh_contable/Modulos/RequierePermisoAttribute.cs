using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace marsh_contable.Modulos
{
    public class RequierePermisoAttribute : AuthorizeAttribute
    {
        private readonly int _permisoId;

        public RequierePermisoAttribute(PermisosAplica permiso)
        {
            _permisoId = (int)permiso;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!base.IsAuthorized(actionContext))
                return false;

            var identity = actionContext.RequestContext.Principal?.Identity as ClaimsIdentity;
            if (identity == null) return false;

            // Comparar por ID numérico del permiso
            return identity.Claims.Any(c =>
                c.Type == "permiso" &&
                c.Value == _permisoId.ToString()
            );
        }
    }
}