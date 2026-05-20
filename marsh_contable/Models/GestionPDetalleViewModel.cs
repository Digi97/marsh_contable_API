using System;

namespace marsh_contable.Models
{
    public class GestionPDetalleViewModel
    {
        public int id { get; set; }
        public double Monto { get; set; }
        public double Monto_aprobado { get; set; }
        public double Monto_modificado { get; set; }
        public double Monto_compometido { get; set; }
        public string detalle_presupuesto { get; set; }
        public int Gestion_Presupuestaria_id { get; set; }

        public string Gestion_presupuestaria_nombre { get; set; }
    }
}
