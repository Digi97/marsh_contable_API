using System;

namespace marsh_contable.Models
{
    public class CantonViewModel
    {
        public int id { get; set; }
        public string codigo { get; set; }
        public string Nombre { get; set; }
        public int Provincia_id { get; set; }

        public string Provincia { get; set; }
    }
}
