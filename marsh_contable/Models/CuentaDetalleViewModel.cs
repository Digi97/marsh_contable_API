using System;

namespace marsh_contable.Models
{
    public class CuentaDetalleViewModel
    {
        public int id { get; set; }
        public double Total { get; set; }
        public double Monto_Proyeccion { get; set; }
        public double Fecha_creacion { get; set; }
        public short Estado { get; set; }
        public double Impuesto { get; set; }
        public double Subtotal { get; set; }
        public int Cuenta_Encabezado_id { get; set; }

        public string Referencia_encabezado { get; set; }
    }
}
