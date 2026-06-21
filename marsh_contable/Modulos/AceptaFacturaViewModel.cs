using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Modulos
{
    public class AceptaFacturaViewModel
    {
        public string base64Factura { get; set; }
        public int Categoria_gasto_id { get; set; }
        public int Medio_pago_id { get; set; }
        public int Usuarios_Usuario_id { get; set; }
        public int Tipo_moneda_id { get; set; }
    }
}