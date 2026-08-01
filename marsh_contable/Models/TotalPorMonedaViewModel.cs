using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class TotalPorMonedaViewModel
    {
        public int Tipo_Moneda_id { get; set; }
        public string Simbolo { get; set; }
        public double Total { get; set; }
    }
}