using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class RolesViewModel
    {
        public int id { get; set; }
        public string descripcion { get; set; }
        public List<PermisosXRolViewModel> PermisosRol { get; set; }
    }
}