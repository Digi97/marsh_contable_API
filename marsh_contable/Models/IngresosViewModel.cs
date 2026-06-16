using System;
using System.Collections.Generic;

namespace marsh_contable.Models
{
    public class IngresosViewModel
    {
        public int id { get; set; }
        public string Codigo { get; set; }
        public DateTime fecha { get; set; }
        public int Tipo_moneda_id { get; set; }
        public int Estado_Factura_id { get; set; }
        public double Subtotal { get; set; }
        public double Impuesto { get; set; }
        public double Total { get; set; }
        public double Descuento { get; set; }
        public double cambio_venta { get; set; }
        public double cambio_compra { get; set; }
        public int Clientes_id { get; set; }
        public int Usuarios_Usuario_id { get; set; }
        public int Medio_pago_id { get; set; }
        public int? Facturas_id { get; set; }
        public string Cliente { get; set; }
        public string Tipo_moneda { get; set; }
        public string Estado_factura { get; set; }
        public string Medio_pago { get; set; }
        public string Usuario { get; set; }

        // Lista de detalles
        public List<IngresosDetalleViewModel> IngresosDetalle { get; set; }
    }
}
