using System;

namespace marsh_contable.Models
{
    public class DevolucionesViewModel
    {
        public int id { get; set; }
        public string Motivo { get; set; }
        public double Monto { get; set; }
        public int Ingresos_id { get; set; }

        public string Ingreso_codigo { get; set; }
    }
}
