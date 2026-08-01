using System;
using System.Collections.Generic;

namespace marsh_contable.Models
{
    public class FacturasViewModel
    {
        public int id { get; set; }
        public string Clave { get; set; }
        public string Consecutivo_electronico { get; set; }
        public DateTime fecha { get; set; }
        public int consecutivo { get; set; }
        public int Tipo_moneda_id { get; set; }
        public int Estado_Factura_id { get; set; }
        public int Tipo_documento_id { get; set; }
        public double Subtotal { get; set; }
        public double Impuesto { get; set; }
        public double Total { get; set; }
        public double Descuento { get; set; }
        public int Impuesto_id { get; set; }
        public double cambio_venta { get; set; }
        public double cambio_compra { get; set; }
        public int Clientes_id { get; set; }
        public int Condicion_venta_id { get; set; }
        public int Medio_pago_id { get; set; }
        public int Usuarios_Usuario_id { get; set; }

        // Descripciones
        public string Tipo_moneda { get; set; }
        public string Estado_factura { get; set; }
        public string Tipo_documento { get; set; }
        public string Cliente { get; set; }
        public string Condicion_venta { get; set; }
        public string Medio_pago { get; set; }

        public string Tipo_identificacion { get; set; }
        public string Cliente_cedula { get; set; }
        public String Telefono_numero { get; set; }
        public String Telefono_codigo_pais { get; set; }

        public String Cliente_Provincia {get;set ;}
        public String Cliente_Canton { get; set; }
        public String Cliente_distrito { get; set; }
        public String Cliente_OtrasSenas { get; set; }
        public String Cliente_Correo { get; set; }
        public List<FacturaDetallesViewModel> Factura_Detalles { get; set; }
        public List<FacturaDetallesViewModel> Factura_DetalleEliminados { get; set; }

        public List<FacturaDetallesViewModel> Factura_DetalleAgregados { get; set; }

        public string Simbolo { get; set; }

        public string Referencia { get; set; }


        public string TipoExoneracion { get; set; }
        public string CodigoExoneracion { get; set; }
        public string NombreInstitucionExo { get; set; }
        public Nullable<short> PorcentajeExo { get; set; }
        public System.DateTime FechaEmision { get; set; }
        public int exonerado { get; set; }
    }

}
// FECHA DE ENTEGA NOTAS: SIGUIENTE SEMANA SEMANA 5
//SEMANA 9 REUNIRSE CON NOSOTROS EL PROFE, Y EL PROTOTIPO DEBE ESTAR AL 100% / 14 DE JULIO