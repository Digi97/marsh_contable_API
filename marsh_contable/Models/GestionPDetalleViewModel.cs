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


        public Nullable<int> Gastos_id { get; set; }
        public Nullable<int> Ingresos_id { get; set; }
        public Nullable<int> Facturas_id { get; set; }
        public decimal Monto_ejecutado { get; set; }
        public int Usuarios_Usuario_id { get; set; }
        public System.DateTime Fecha_registro { get; set; }
        public string Observaciones { get; set; }
        public short activo { get; set; }
        public string categoria_presupuestaria { get; set; }

        public int Centro_Costos_id { get; set; }
        public int Categoria_presupuestaria_id { get; set; }

    }
}
