using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Facturacion_C_Sharp.Utils;

namespace Facturacion_C_Sharp.Lib.DocumentoItems
{
    /// <summary>
    /// Representa un nodo TotalDesgloseImpuesto del ResumenFactura (XSD v4.4).
    ///
    /// Definición del esquema:
    ///   TotalDesgloseImpuesto  minOccurs=0  maxOccurs=1000
    ///     ├─ Codigo             CodigoImpuestoType    (obligatorio)
    ///     ├─ CodigoTarifaIVA    CodigoTarifaIVAType   (opcional)
    ///     └─ TotalMontoImpuesto DecimalDineroType     (obligatorio)
    ///
    /// Este nodo se ubica entre TotalVentaNeta y TotalImpuesto dentro del ResumenFactura.
    /// La suma de todos los TotalMontoImpuesto debe coincidir con el TotalImpuesto del resumen.
    /// </summary>
    public class DesgloseImpuestoResumen : IXMLGenerador
    {
        /// <summary>fractionDigits de DecimalDineroType.</summary>
        private const int DECIMALES_XSD = 5;

        // ── Catálogos del XSD v4.4 ──────────────────────────────────────────

        /// <summary>Valores válidos de CodigoImpuestoType.</summary>
        public static readonly string[] CODIGOS_IMPUESTO =
            { "01", "02", "03", "04", "05", "06", "07", "08", "12", "99" };

        /// <summary>Valores válidos de CodigoTarifaIVAType.</summary>
        public static readonly string[] CODIGOS_TARIFA_IVA =
            { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11" };

        // Constantes de uso frecuente
        public const string IMPUESTO_IVA = "01";
        public const string IMPUESTO_SELECTIVO_CONSUMO = "02";
        public const string IMPUESTO_OTROS = "99";

        public const string TARIFA_0 = "01";           // Tarifa 0% (Art. 32, num 1, RLIVA)
        public const string TARIFA_REDUCIDA_1 = "02";  // 1%
        public const string TARIFA_REDUCIDA_2 = "03";  // 2%
        public const string TARIFA_REDUCIDA_4 = "04";  // 4%
        public const string TARIFA_TRANS_0 = "05";     // Transitorio 0%
        public const string TARIFA_TRANS_4 = "06";     // Transitorio 4%
        public const string TARIFA_TRANS_8 = "07";     // Transitorio 8%
        public const string TARIFA_GENERAL_13 = "08";  // Tarifa general 13%
        public const string TARIFA_REDUCIDA_05 = "09"; // 0.5%
        public const string TARIFA_EXENTA = "10";      // Tarifa exenta
        public const string TARIFA_0_SIN_CREDITO = "11";

        // ── Campos ──────────────────────────────────────────────────────────

        private string codigo;
        private string codigoTarifaIVA;
        private decimal totalMontoImpuesto;

        // ── Constructores ───────────────────────────────────────────────────

        public DesgloseImpuestoResumen() : this(IMPUESTO_IVA, 0, null) { }

        public DesgloseImpuestoResumen(
            string codigo,
            decimal totalMontoImpuesto,
            string codigoTarifaIVA = null)
        {
            this.codigo = codigo;
            this.totalMontoImpuesto = totalMontoImpuesto;
            this.codigoTarifaIVA = codigoTarifaIVA;
        }

        // ── Propiedades ─────────────────────────────────────────────────────

        /// <summary>
        /// Código de impuesto (CodigoImpuestoType). Obligatorio.
        /// 01 IVA, 02 Selectivo de Consumo, 03 Combustibles, 04 Bebidas alcohólicas,
        /// 05 Bebidas envasadas y jabones, 06 Tabaco, 07 IVA cálculo especial,
        /// 08 IVA bienes usados (factor), 12 Cemento, 99 Otros.
        /// </summary>
        public string Codigo { get => codigo; set => codigo = value; }

        /// <summary>Código de tarifa del IVA (CodigoTarifaIVAType). Opcional en el XSD,
        /// pero obligatorio de hecho cuando el Codigo corresponde a IVA.</summary>
        public string CodigoTarifaIVA { get => codigoTarifaIVA; set => codigoTarifaIVA = value; }

        /// <summary>Monto acumulado del impuesto para este código y tarifa.</summary>
        public decimal TotalMontoImpuesto { get => totalMontoImpuesto; set => totalMontoImpuesto = value; }

        // ── Validación ──────────────────────────────────────────────────────

        /// <summary>
        /// Verifica que los valores respeten las enumeraciones y restricciones del XSD.
        /// Devuelve la lista de errores encontrados; vacía si el nodo es válido.
        /// </summary>
        public List<string> Validar()
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                errores.Add("TotalDesgloseImpuesto.Codigo es obligatorio.");
            }
            else if (!CODIGOS_IMPUESTO.Contains(codigo.Trim()))
            {
                errores.Add(string.Format(
                    "TotalDesgloseImpuesto.Codigo '{0}' no pertenece a CodigoImpuestoType. Valores válidos: {1}.",
                    codigo, string.Join(", ", CODIGOS_IMPUESTO)));
            }

            if (!string.IsNullOrWhiteSpace(codigoTarifaIVA)
                && !CODIGOS_TARIFA_IVA.Contains(codigoTarifaIVA.Trim()))
            {
                errores.Add(string.Format(
                    "TotalDesgloseImpuesto.CodigoTarifaIVA '{0}' no pertenece a CodigoTarifaIVAType. Valores válidos: {1}.",
                    codigoTarifaIVA, string.Join(", ", CODIGOS_TARIFA_IVA)));
            }

            var monto = Redondear(totalMontoImpuesto);

            if (monto < 0)
                errores.Add("TotalDesgloseImpuesto.TotalMontoImpuesto no puede ser negativo (minInclusive = 0).");

            if (monto > 9999999999999.99999m)
                errores.Add("TotalDesgloseImpuesto.TotalMontoImpuesto excede el máximo permitido por DecimalDineroType.");

            return errores;
        }

        public bool EsValido()
        {
            return Validar().Count == 0;
        }

        // ── Utilidades ──────────────────────────────────────────────────────

        private static decimal Redondear(decimal valor)
        {
            return Math.Round(valor, DECIMALES_XSD, MidpointRounding.AwayFromZero);
        }

        private static string Dinero(decimal valor)
        {
            return XmlConvert.ToString(Redondear(valor));
        }

        /// <summary>
        /// Agrupa una colección de desgloses por Codigo + CodigoTarifaIVA y suma los montos.
        /// Útil para consolidar las líneas de detalle antes de armar el resumen,
        /// ya que el XSD espera un nodo por combinación y no uno por línea.
        /// </summary>
        public static List<DesgloseImpuestoResumen> Consolidar(
            IEnumerable<DesgloseImpuestoResumen> desgloses)
        {
            if (desgloses == null)
                return new List<DesgloseImpuestoResumen>();

            return desgloses
                .Where(d => d != null && !string.IsNullOrWhiteSpace(d.Codigo))
                .GroupBy(d => new
                {
                    Codigo = d.Codigo.Trim(),
                    Tarifa = string.IsNullOrWhiteSpace(d.CodigoTarifaIVA) ? null : d.CodigoTarifaIVA.Trim()
                })
                .Select(g => new DesgloseImpuestoResumen(
                    g.Key.Codigo,
                    g.Sum(x => x.TotalMontoImpuesto),
                    g.Key.Tarifa))
                .OrderBy(d => d.Codigo)
                .ThenBy(d => d.CodigoTarifaIVA)
                .Take(1000)
                .ToList();
        }

        // ── Generación del XML ──────────────────────────────────────────────

        /// <summary>
        /// Genera el nodo TotalDesgloseImpuesto respetando el orden de la secuencia del XSD.
        /// </summary>
        public XElement GenerarXML()
        {
            var baseXML = new XElement("TotalDesgloseImpuesto");

            // 1. Codigo (obligatorio)
            baseXML.Add(new XElement("Codigo",
                string.IsNullOrWhiteSpace(codigo) ? IMPUESTO_IVA : codigo.Trim()));

            // 2. CodigoTarifaIVA (opcional)
            if (!string.IsNullOrWhiteSpace(codigoTarifaIVA))
                baseXML.Add(new XElement("CodigoTarifaIVA", codigoTarifaIVA.Trim()));

            // 3. TotalMontoImpuesto (obligatorio)
            baseXML.Add(new XElement("TotalMontoImpuesto", Dinero(totalMontoImpuesto)));

            return baseXML;
        }

        public override string ToString()
        {
            return string.Format("Impuesto {0}{1}: {2}",
                codigo,
                string.IsNullOrWhiteSpace(codigoTarifaIVA) ? "" : " / tarifa " + codigoTarifaIVA,
                Redondear(totalMontoImpuesto));
        }
    }
}