using System;
using System.Collections.Generic;

namespace marsh_contable.Models
{
    public class PermisosXRolViewModel
    {
        public int id { get; set; }
        public int Permisos_id { get; set; }
        public int Roles_id { get; set; }

        public string NombrePermiso { get; set; }
        public string Rol { get; set; }

    }
}
