using System;
using System.ComponentModel;
using System.Xml.Linq;
using Facturacion_C_Sharp.Utils;

namespace Facturacion_C_Sharp.Lib.DocumentoItems
{
    public class Impuesto : IXMLGenerador
    {
        public enum CodigoImpuesto
        {
            [Description("01")]
            Impuesto_General_sobre_las_Ventas = 01,

            [Description("02")]
            Impuesto_Selectivo_de_Consumo = 02,

            [Description("03")]
            Impuesto_Único_a_los_combustibles = 03,

            [Description("04")]
            Impuesto_específico_de_bebidas_alcohólicas = 04,

            [Description("05")]
            Impuesto_Específico_sobre_las_bebidas_envasadas_sin_contenido_alcóholico_y_jabones_de_tocador = 05,

            [Description("06")]
            Impuesto_a_los_Productos_de_Tabaco = 06,

            [Description("07")]
            Servicio = 07,

            [Description("08")]
            Impuesto_General_sobre_las_ventas_diplomáticos = 08,

            [Description("09")]
            Impuesto_general_sobre_las_ventas_Compras_autorizadas = 09,

            [Description("10")]
            Impuesto_general_sobre_las_ventas_instituciones_públicas_y_otros_organismos = 10,

            [Description("11")]
            Impuesto_Selectivo_de_Consumo_Compras_Autorizadas = 11,

            [Description("12")]
            Impuesto_específico_al_cemento = 12,

            [Description("98")]
            Otros98 = 98,

            [Description("99")]
            Otros99 = 99
        }


        //El codigo siempre debe estar presente
        private CodigoImpuesto codigo;
        private decimal tarifa;
        private decimal monto;

        private Exoneracion exoneracion;

        public static CodigoImpuesto StringToCodigo(String codigo)
        {
            switch (codigo)
            {
                case "01": return CodigoImpuesto.Impuesto_General_sobre_las_Ventas;
                case "02": return CodigoImpuesto.Impuesto_Selectivo_de_Consumo;
                case "03": return CodigoImpuesto.Impuesto_Único_a_los_combustibles;
                case "04": return CodigoImpuesto.Impuesto_específico_de_bebidas_alcohólicas;
                case "05": return CodigoImpuesto.Impuesto_Específico_sobre_las_bebidas_envasadas_sin_contenido_alcóholico_y_jabones_de_tocador;
                case "06": return CodigoImpuesto.Impuesto_a_los_Productos_de_Tabaco;
                case "07": return CodigoImpuesto.Servicio;
                case "08": return CodigoImpuesto.Impuesto_General_sobre_las_ventas_diplomáticos;
                case "09": return CodigoImpuesto.Impuesto_general_sobre_las_ventas_Compras_autorizadas;
                case "10": return CodigoImpuesto.Impuesto_general_sobre_las_ventas_instituciones_públicas_y_otros_organismos;
                case "11": return CodigoImpuesto.Impuesto_Selectivo_de_Consumo_Compras_Autorizadas;
                case "12": return CodigoImpuesto.Impuesto_específico_al_cemento;
                case "98": return CodigoImpuesto.Otros98;
                case "99": return CodigoImpuesto.Otros99;
                default: return CodigoImpuesto.Impuesto_General_sobre_las_Ventas;
            }
        }

        public Impuesto(String codigo, decimal tarifa, decimal monto, Exoneracion exoneracion = null)
        {
            this.codigo = StringToCodigo( codigo);
            this.tarifa = tarifa;
            this.monto = monto;
            this.exoneracion = exoneracion;
        }

        public CodigoImpuesto Codigo { get => codigo; set => codigo = value; }
        public decimal Tarifa { get => tarifa; set => tarifa = value; }
        public decimal Monto { get => monto; set => monto = value; }
        public Exoneracion Exoneracion { get => exoneracion; set => exoneracion = value; }

        public XElement GenerarXML()
        {
            XElement impuesto;

            impuesto = new XElement("Impuesto",
                    new XElement("Codigo", codigo.ToDescriptionString()),
                    new XElement("Tarifa", tarifa),
                    new XElement("Monto", monto));

            if (exoneracion != null)
            {
                impuesto.Add(exoneracion.GenerarXML());
            }

            return impuesto;
        }
    }
}
