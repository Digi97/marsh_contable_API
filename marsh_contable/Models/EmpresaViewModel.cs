using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class EmpresaViewModel
    {
        public int Emp_id { get; set; }
        public string Nombre_empresa { get; set; }
        public string Correo_empresa { get; set; }
        public string Ruta_nas { get; set; }
        public int Numero_sucursal { get; set; }
        public string Formato_fecha { get; set; }
        public string Ruta_llave_factura { get; set; }
        public string pin_llave { get; set; }
        public string ruta_logo { get; set; }
        public int terminal { get; set; }
        public string codigo_seguridad { get; set; }
        public string identificacion { get; set; }
        public Nullable<int> codigo_actividad_id { get; set; }
        public int tipo_identificacion_id { get; set; }
        public Nullable<int> Impuesto_id { get; set; }
        public string Tipo_Identificacion { get; set; }

        public string Provincia_emisor { get; set; }
        public string Canton_emisor { get; set; }
        public string Distrito_emisor { get; set; }
        public string OtrasSenas_Emisor { get; set; }



        public int? Provincia_id{ get; set; }
        public int? Canton_id { get; set; }
        public int? Distrito_id { get; set; }
      

        public string Telefono { get; set; }
        public string Codigo_Telefono { get; set; }

        public string Correo_smtp { get; set; }
        public string Contrasena_smtp { get; set; }
        public string Proveedor_SMTP { get; set; }
        public string Puerto_SMTP { get; set; }
        public string Asunto_SMTP { get; set; }
        public string Usuario_hacienda { get; set; }
        public string Contrasena_hacienda { get; set; }








    }
}