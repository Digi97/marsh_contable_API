using System;
using System.ComponentModel;
using System.Xml.Linq;
using Facturacion_C_Sharp.Utils;

namespace Facturacion_C_Sharp.Lib.DocumentoItems
{
    public class Exoneracion : IXMLGenerador
    {
        public enum TipoDocumento
        {
            [Description("01")]
            Compras_autorizadas_por_la_Direccion_General_de_Tributacion,

            [Description("02")]
            Ventas_exentas_a_diplomaticos,

            [Description("03")]
            Autorizado_por_Ley_especial,

            [Description("04")]
            Exenciones_Direccion_General_de_Hacienda_Autorizacion_Local_Generica,

            [Description("05")]
            Exenciones_Direccion_General_de_Hacienda_Transitorio_V,

            [Description("06")]
            Servicios_turisticos_inscritos_ante_el_ICT,

            [Description("07")]
            Transitorio_XVII_Recoleccion_Clasificacion_Almacenamiento_de_Reciclaje,

            [Description("08")]
            Exoneracion_a_Zona_Franca,

            [Description("09")]
            Exoneracion_de_servicios_complementarios_para_la_exportacion_articulo_11_RLIVA,

            [Description("10")]
            Organo_de_las_corporaciones_municipales,

            [Description("11")]
            Exenciones_Direccion_General_de_Hacienda_Autorizacion_de_Impuesto_Local_Concreta
        }

        //attr_accessor :document_type, :document_number, :institution, :date, :total_tax, :percentage, :net_total

        //validates :document_type, presence: true, inclusion: DOCUMENT_TYPES.keys
        //validates :document_number, presence: true
        //validates :institution, presence: true
        //validates :date, presence: true
        //validates :total_tax,presence: true
        //validates :percentage, presence: true

        private TipoDocumento tipoDocumento;
        private String numeroDocumento;
        private String nombreInstitucion;
        private DateTime fechaEmision;
        private decimal montoImpuesto;
        private decimal totalNeto;
        private decimal montoExoneracion;


        //Dispensable
        private decimal porcentajeCompra;

        public TipoDocumento TipoDocumento1 { get => tipoDocumento; set => tipoDocumento = value; }
        public String NumeroDocumento { get => numeroDocumento; set => numeroDocumento = value; }
        public string NombreInstitucion { get => nombreInstitucion; set => nombreInstitucion = value; }
        public DateTime FechaEmision { get => fechaEmision; set => fechaEmision = value; }
        public decimal MontoImpuesto { get => montoImpuesto; set => montoImpuesto = value; }
        public decimal TotalNeto { get => totalNeto; set => totalNeto = value; }
        public decimal PorcentajeCompra { get => porcentajeCompra; set => porcentajeCompra = value; }

        public decimal MontoExoneracion { get => montoExoneracion; set => montoExoneracion = value; }


        public Exoneracion(TipoDocumento tipoDocumento, string numeroDocumento, string nombreInstitucion, DateTime fechaEmision, decimal montoImpuesto, decimal totalNeto, decimal montoExoneracion = 0)
        {
            this.tipoDocumento = tipoDocumento;
            this.numeroDocumento = numeroDocumento;
            this.nombreInstitucion = nombreInstitucion;
            this.fechaEmision = fechaEmision;
            this.montoImpuesto = montoImpuesto;
            this.totalNeto = totalNeto;
            this.montoExoneracion = montoExoneracion;

            this.porcentajeCompra = (montoImpuesto / totalNeto) * 100;
        }

        public XElement GenerarXML()
        {
            var exoneracion = new XElement("Exoneracion",
                                                  new XElement("TipoDocumentoEX1", tipoDocumento.ToDescriptionString()),
                                                  new XElement("NumeroDocumento", numeroDocumento),
                                                  new XElement("NombreInstitucion", nombreInstitucion),
                                           new XElement("FechaEmision", fechaEmision.ToRfc3339String()),
                                                  new XElement("MontoImpuesto", montoImpuesto),
                                                  new XElement("PorcentajeCompra", porcentajeCompra)
                                                 );
            return exoneracion;
        }
    }
}
