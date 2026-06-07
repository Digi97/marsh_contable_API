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
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public int Roles_id { get; set; }
        public string Id_Empleado { get; set; }
        public int activo { get; set; }

        public DateTime Fec_Actualizacion { get; set; }
        public DateTime Fec_Login { get; set; }

        public string Rol { get; set; }
        public string FormatoFecha { get; set; }//obtenemos de empresa
        public int ImpuestoDefault { get; set; } //obtenemos de empresa
                                                 // Lista de permisos del rol
        public List<int> Permisos { get; set; }
    }
}