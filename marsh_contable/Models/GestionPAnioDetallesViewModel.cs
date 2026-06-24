using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class GestionPAnioDetallesViewModel
    {

        public string anio_presupuesto { get; set; }
        public int Gestion_Presupuestaria_id { get; set; }
        public decimal monto { get; set; }
        public int mes { get; set; }

    }
}