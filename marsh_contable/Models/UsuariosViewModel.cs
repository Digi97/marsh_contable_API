using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class UsuariosViewModel
    {
        public int Usuario_id { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public string correo { get; set; }
        public string contrasena { get; set; }
        public int roles_id { get; set; }
        public string id_empleado { get; set; }
        public int activo { get; set; }
    }
}