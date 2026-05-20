using System;

namespace marsh_contable.Models
{
    public class DistritoViewModel
    {
        public int id { get; set; }
        public string codigo_canton { get; set; }
        public string codigo_distrito { get; set; }
        public string Nombre { get; set; }
        public int Canton_id { get; set; }
        public int Canton_Provincia_id { get; set; }

        public string Canton { get; set; }
        public string Provincia { get; set; }
    }
}
