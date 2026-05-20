using System;

namespace marsh_contable.Models
{
    public class GastosDetallesViewModel
    {
        public int id { get; set; }
        public double Subtotal { get; set; }
        public double Impuesto { get; set; }
        public double Total { get; set; }
        public int Cantidad { get; set; }
        public string Detalle { get; set; }
        public double Descuento { get; set; }
        public string codigo_comercial { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Ultima_fec_actualizacion { get; set; }
        public int Medio_pago_id { get; set; }
        public int Gastos_id { get; set; }

        public string Medio_pago { get; set; }
    }
}
