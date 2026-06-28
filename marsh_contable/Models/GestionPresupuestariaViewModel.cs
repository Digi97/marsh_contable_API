using System;
using System.Collections.Generic;

namespace marsh_contable.Models
{
    public class GestionPresupuestariaViewModel
    {
        public int id { get; set; }
        public string codigo { get; set; }
        public string nombre { get; set; }
        public string Descripcion { get; set; }
        public string anio_presupuesto { get; set; }
        public DateTime periodo_inicio { get; set; }
        public DateTime periodo_fin { get; set; }
        public int Categoria_presupuestaria_id { get; set; }
        public double monto_aprobado { get; set; }
        public double monto_modificado { get; set; }
        public double monto_comprometido { get; set; }
        public double monto_ejecutado { get; set; }
        public short estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_actualizacion { get; set; }
        public int Usuarios_Usuario_id { get; set; }
        public int Centro_Costos_id { get; set; }

        public string Categoria_presupuestaria { get; set; }
        public string Centro_costo { get; set; }
        public string Usuario { get; set; }
        public string Formato { get; set; }

        public int Tipo_moneda_id { get; set; }
        public string tipo_moneda { get; set; }

        public int mesOrigen { get; set; }
        public int mesDestino { get; set; }

        public string anioOrigen { get; set; }
        public string anioDestino { get; set; }

        public List<DetalleGestionPViewModel> detalles { get; set; }
    }
}
