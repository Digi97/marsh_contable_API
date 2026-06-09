using System;

namespace marsh_contable.Models
{
    public class FacturaDetallesViewModel
    {
        public int id { get; set; }
        public int Facturas_id { get; set; }
        public double Subtotal { get; set; }
        public double Impuesto { get; set; }
        public double Total { get; set; }
        public int Cantidad { get; set; }
        public string Detalle { get; set; }
        public int Codigos_cabys_id { get; set; }
        public string Codigos_cabys_codigo { get; set; }
        public int Codigos_cabys_Impuesto_id { get; set; }
        public double Descuento { get; set; }
        public int Unidad_medida_id { get; set; }
        public int Codigo_comercial_id { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Ultima_fec_actualizacion { get; set; }

        // Descripciones
        public string Unidad_medida { get; set; }
        public string Codigo_comercial { get; set; }
        public string Codigo_cabys_descripcion { get; set; }
        public Impuesto Impuesto_detalle { get; set; }
    }
}
