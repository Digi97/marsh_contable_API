using System;

namespace marsh_contable.Models
{
    public class TelefonosViewModel
    {
        public int id { get; set; }
        public string Numero { get; set; }
        public string codigo_pais { get; set; }
        public int? Clientes_id { get; set; }
        public int? Proveedor_id { get; set; }
        public short telefono_principal { get; set; }

        // Descripciones
        public string Cliente { get; set; }
        public string Proveedor { get; set; }
    }
}
