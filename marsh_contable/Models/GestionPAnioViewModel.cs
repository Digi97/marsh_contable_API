using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class GestionPAnioViewModel
    {
        public string anio_presupuesto { get; set; }
        public int Gestion_Presupuestaria_id { get; set; }
        public List<GestionPAnioDetallesViewModel> detalles { get; set; }

    }
}