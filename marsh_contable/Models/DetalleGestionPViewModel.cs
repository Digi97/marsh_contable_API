using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class DetalleGestionPViewModel
    {
        public int categoria_presupuestaria_id { get; set; }
        public int centro_Costos_id { get; set; }
        public double monto { get; set; }
        public int id { get; set; }
    }
}