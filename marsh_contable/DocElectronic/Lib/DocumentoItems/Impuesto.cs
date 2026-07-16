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
            IVA = 01,

            [Description("02")]
            Impuesto_Selectivo_de_Consumo = 02,

            [Description("03")]
            Impuesto_Unico_Combustibles = 03,

            [Description("04")]
            Impuesto_Bebidas_Alcoholicas = 04,

            [Description("05")]
            Impuesto_Bebidas_No_Alcoholicas_Jabones = 05,

            [Description("06")]
            Impuesto_Productos_Tabaco = 06,

            [Description("07")]
            IVA_Calculo_Especial = 07,

            [Description("08")]
            IVA_Bienes_Usados = 08,

            [Description("12")]
            Impuesto_Especifico_Cemento = 12,

            [Description("99")]
            Otros = 99
        }

        // Códigos tarifa IVA según XSD v4.4
        public enum CodigoTarifaIVAEnum
        {
            [Description("01")]
            Tarifa_0_Porciento = 01,

            [Description("02")]
            Tarifa_Reducida_1 = 02,

            [Description("03")]
            Tarifa_Reducida_2 = 03,

            [Description("04")]
            Tarifa_Reducida_4 = 04,

            [Description("05")]
            Transitorio_0 = 05,

            [Description("06")]
            Transitorio_4 = 06,

            [Description("07")]
            Transitorio_8 = 07,

            [Description("08")]
            Tarifa_General_13 = 08,

            [Description("09")]
            Tarifa_Reducida_05 = 09,

            [Description("10")]
            Tarifa_Exenta = 10,

            [Description("11")]
            Tarifa_0_Sin_Credito = 11
        }

        private CodigoImpuesto codigo;
        private string codigoImpuestoOtro;     // Obligatorio si codigo = 99
        private string codigoTarifaIVA;        // Obligatorio si codigo = 01
        private decimal tarifa;
        private decimal monto;
        private decimal factorCalculoIVA;      // Para código 08 (Bienes Usados)
        private Exoneracion exoneracion;

        public static CodigoImpuesto StringToCodigo(String codigo)
        {
            switch (codigo)
            {
                case "01": return CodigoImpuesto.IVA;
                case "02": return CodigoImpuesto.Impuesto_Selectivo_de_Consumo;
                case "03": return CodigoImpuesto.Impuesto_Unico_Combustibles;
                case "04": return CodigoImpuesto.Impuesto_Bebidas_Alcoholicas;
                case "05": return CodigoImpuesto.Impuesto_Bebidas_No_Alcoholicas_Jabones;
                case "06": return CodigoImpuesto.Impuesto_Productos_Tabaco;
                case "07": return CodigoImpuesto.IVA_Calculo_Especial;
                case "08": return CodigoImpuesto.IVA_Bienes_Usados;
                case "12": return CodigoImpuesto.Impuesto_Especifico_Cemento;
                case "99": return CodigoImpuesto.Otros;
                default:   return CodigoImpuesto.IVA;
            }
        }

        /// <summary>
        /// Determina el CodigoTarifaIVA basado en el porcentaje de tarifa
        /// </summary>
        public static string TarifaToCodigoTarifaIVA(decimal tarifa)
        {
            if (tarifa == 0)    return "01";
            if (tarifa == 1)    return "02";
            if (tarifa == 2)    return "03";
            if (tarifa == 4)    return "04";
            if (tarifa == 8)    return "07";
            if (tarifa == 13)   return "08";
            if (tarifa == 0.5m) return "09";
            return "08"; // Default: tarifa general 13%
        }

        public Impuesto(String codigo, decimal tarifa, decimal monto,
                        Exoneracion exoneracion = null,
                        string codigoTarifaIVA = "",
                        decimal factorCalculoIVA = 0,
                        string codigoImpuestoOtro = "")
        {
            this.codigo = StringToCodigo(codigo);
            this.tarifa = tarifa;
            this.monto = monto;
            this.exoneracion = exoneracion;
            this.factorCalculoIVA = factorCalculoIVA;
            this.codigoImpuestoOtro = codigoImpuestoOtro;

            // Auto-asignar CodigoTarifaIVA si es IVA y no se proporcionó
            if (string.IsNullOrEmpty(codigoTarifaIVA) &&
                (this.codigo == CodigoImpuesto.IVA || this.codigo == CodigoImpuesto.IVA_Calculo_Especial))
            {
                this.codigoTarifaIVA = TarifaToCodigoTarifaIVA(tarifa);
            }
            else
            {
                this.codigoTarifaIVA = codigoTarifaIVA;
            }
        }

        public CodigoImpuesto Codigo { get => codigo; set => codigo = value; }
        public string CodigoImpuestoOtro { get => codigoImpuestoOtro; set => codigoImpuestoOtro = value; }
        public string CodigoTarifaIVA { get => codigoTarifaIVA; set => codigoTarifaIVA = value; }
        public decimal Tarifa { get => tarifa; set => tarifa = value; }
        public decimal Monto { get => monto; set => monto = value; }
        public decimal FactorCalculoIVA { get => factorCalculoIVA; set => factorCalculoIVA = value; }
        public Exoneracion Exoneracion { get => exoneracion; set => exoneracion = value; }

        public XElement GenerarXML()
        {
            var impuestoXml = new XElement("Impuesto");

            // 1. Codigo (obligatorio)
            impuestoXml.Add(new XElement("Codigo", codigo.ToDescriptionString()));

            // 2. CodigoImpuestoOTRO (obligatorio si codigo = 99)
            if (codigo == CodigoImpuesto.Otros && !string.IsNullOrEmpty(codigoImpuestoOtro))
            {
                impuestoXml.Add(new XElement("CodigoImpuestoOTRO", codigoImpuestoOtro));
            }

            // 3. CodigoTarifaIVA (obligatorio para IVA - código 01)
            if (!string.IsNullOrEmpty(codigoTarifaIVA) &&
                (codigo == CodigoImpuesto.IVA ||
                 codigo == CodigoImpuesto.IVA_Calculo_Especial))
            {
                impuestoXml.Add(new XElement("CodigoTarifaIVA", codigoTarifaIVA));
            }

            // 4. Tarifa (obligatorio cuando hay impuesto)
            impuestoXml.Add(new XElement("Tarifa", tarifa));

            // 5. FactorCalculoIVA (obligatorio para Bienes Usados - código 08)
            if (codigo == CodigoImpuesto.IVA_Bienes_Usados && factorCalculoIVA > 0)
            {
                impuestoXml.Add(new XElement("FactorCalculoIVA", factorCalculoIVA));
            }

            // 6. Monto (obligatorio)
            impuestoXml.Add(new XElement("Monto", monto));

            // 7. Exoneracion (opcional, dentro del impuesto)
            if (exoneracion != null)
            {
                impuestoXml.Add(exoneracion.GenerarXML());
            }

            return impuestoXml;
        }
    }
}
