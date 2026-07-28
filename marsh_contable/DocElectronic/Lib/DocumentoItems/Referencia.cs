using System;
using System.ComponentModel;
using System.Xml.Linq;
using Facturacion_C_Sharp.Utils;

namespace Facturacion_C_Sharp.Lib.DocumentoItems
{
    public class Referencia :IXMLGenerador
    {
        public enum CodigoReferencia
        {
            [Description("01")]
            Anula_Documento_de_Referencia = 01,
            [Description("02")]
            Corrige_Monto = 02,
            [Description("04")]
            Referencia_a_Otro_Documento = 04,
            [Description("05")]
            Sustituye_Comprobante_Provisional_por_Contingencia = 05,
            [Description("06")]
            Devolucion_de_Mercancia = 06,
            [Description("07")]
            Sustituye_Comprobante_Electronico = 07,
            [Description("08")]
            Factura_Endosada = 08,
            [Description("09")]
            Nota_de_Credito_Financiera = 09,
            [Description("10")]
            Nota_de_Debito_Financiera = 10,
            [Description("11")]
            Proveedor_No_Domiciliado = 11,
            [Description("12")]
            Nota_de_Credito_Financiera_por_Exoneracion_Posterior_a_la_Facturacion = 12,
            [Description("13")]
            Anula_Documento_de_Referencia_por_Error_Material = 13,
            [Description("14")]
            Corrige_Monto_por_Error_Material = 14,
            [Description("15")]
            Sustituye_Comprobante_Electronico_por_Error_Material = 15,
            [Description("16")]
            Sustituye_Comprobante_Electronico_Rechazado = 16,
            [Description("17")]
            Pago_a_Comprobante_Electronico = 17,
            [Description("99")]
            Otros = 99
        }

        public static CodigoReferencia StringToCodigoReferencia(String codigo)
        {
            switch (codigo)
            {
                case "01":
                    return CodigoReferencia.Anula_Documento_de_Referencia;
                case "02":
                    return CodigoReferencia.Corrige_Monto;
                case "04":
                    return CodigoReferencia.Referencia_a_Otro_Documento;
                case "05":
                    return CodigoReferencia.Sustituye_Comprobante_Provisional_por_Contingencia;
                case "06":
                    return CodigoReferencia.Devolucion_de_Mercancia;
                case "07":
                    return CodigoReferencia.Sustituye_Comprobante_Electronico;
                case "08":
                    return CodigoReferencia.Factura_Endosada;
                case "09":
                    return CodigoReferencia.Nota_de_Credito_Financiera;
                case "10":
                    return CodigoReferencia.Nota_de_Debito_Financiera;
                case "11":
                    return CodigoReferencia.Proveedor_No_Domiciliado;
                case "12":
                    return CodigoReferencia.Nota_de_Credito_Financiera_por_Exoneracion_Posterior_a_la_Facturacion;
                case "13":
                    return CodigoReferencia.Anula_Documento_de_Referencia_por_Error_Material;
                case "14":
                    return CodigoReferencia.Corrige_Monto_por_Error_Material;
                case "15":
                    return CodigoReferencia.Sustituye_Comprobante_Electronico_por_Error_Material;
                case "16":
                    return CodigoReferencia.Sustituye_Comprobante_Electronico_Rechazado;
                case "17":
                    return CodigoReferencia.Pago_a_Comprobante_Electronico;
                case "99":
                default:
                    return CodigoReferencia.Otros;
            }
        }

        //      validates :document_type, presence: true, inclusion: FE::Document::DOCUMENT_TYPES.keys
        //      validates :number, presence: true, length: {maximum: 50}
        //      validates :date, presence: true
        //      validates :code, presence: true, length: {is: 2}, inclusion: REFERENCE_CODES.keys
        //      validates :reason, presence: true, length: {maximum: 180}

        private Documento.TipoDocumento tipoDoc;
        //Clave Numerica
        private String numero;
        //DD-MM-YYYY HH:MM:SS
        private DateTime fechaEmision;
        private CodigoReferencia codigo;
        private String razon;

        public Referencia ( Documento.TipoDocumento tipoDoc, string numero, DateTime fechaEmision, CodigoReferencia codigo, string razon )
        {
            this.tipoDoc = tipoDoc;
            this.numero = numero;
            this.fechaEmision = fechaEmision;
            this.codigo = codigo;
            this.razon = razon;
        }

        public Documento.TipoDocumento TipoDoc
        {
            get => tipoDoc; set => tipoDoc = value;
        }
        public string Numero
        {
            get => numero; set => numero = value;
        }
        public DateTime FechaEmision
        {
            get => fechaEmision; set => fechaEmision = value;
        }
        public CodigoReferencia Codigo
        {
            get => codigo; set => codigo = value;
        }
        public string Razon
        {
            get => razon; set => razon = value;
        }

        public XElement GenerarXML ( )
        {
            return new XElement( "InformacionReferencia",
                                       new XElement("TipoDocIR", tipoDoc.ToDescriptionString( ) ),
                                       new XElement( "Numero", numero ),
                                       new XElement( "FechaEmisionIR", fechaEmision.ToRfc3339String( ) ),
                                       new XElement( "Codigo", codigo.ToDescriptionString( ) ),
                                       new XElement( "Razon", razon ) );
        }
    }
}
