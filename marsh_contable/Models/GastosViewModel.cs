using System;

namespace marsh_contable.Models
{
    public class GastosViewModel
    {
        public int id { get; set; }
        public string Descripcion { get; set; }
        public int Categoria_gasto_id { get; set; }
        public double Subtotal { get; set; }
        public double Impuesto { get; set; }
        public double Total { get; set; }
        public string Doc_Referencia { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Ultima_Fec_Actualizacion { get; set; }
        public int Usuarios_Usuario_id { get; set; }
        public int Tipo_documento_id { get; set; }
        public int Medio_pago_id { get; set; }
        public int Proveedor_id { get; set; }

        // Descripciones
        public string Categoria_gasto { get; set; }
        public string Tipo_documento { get; set; }
        public string Medio_pago { get; set; }
        public string Proveedor { get; set; }
        public string Usuario { get; set; }
    }
}
