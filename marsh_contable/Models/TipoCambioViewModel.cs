using System;

namespace marsh_contable.Models
{
    public class TipoCambioViewModel
    {
        public int id { get; set; }
        public DateTime fecha { get; set; }
        public double compra { get; set; }
        public double venta { get; set; }
        public int Tipo_moneda_id { get; set; }
        public int Usuarios_Usuario_id { get; set; }

        public string Tipo_moneda { get; set; }
        public string Usuario { get; set; }
    }
}
