using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Facturacion_C_Sharp.Utils;

namespace Facturacion_C_Sharp.Lib.DocumentoItems
{
    /// <summary>
    /// MedioPago del resumen (v4.4). Máximo 4 repeticiones.
    /// Orden XSD: TipoMedioPago, MedioPagoOtros, TotalMedioPago.
    /// </summary>
    public class MedioPagoResumen
    {
        /// <summary>01 Efectivo, 02 Tarjeta, 03 Cheque, 04 Transferencia, 05 Recaudado por terceros,
        /// 06 SINPE Móvil, 07 Plataforma Digital, 99 Otros.</summary>
        public string TipoMedioPago { get; set; }

        /// <summary>Obligatorio si TipoMedioPago = "99". Entre 3 y 100 caracteres.</summary>
        public string MedioPagoOtros { get; set; }

        /// <summary>Obligatorio cuando se declara más de un medio de pago.</summary>
        public decimal? TotalMedioPago { get; set; }

        public MedioPagoResumen() { }

        public MedioPagoResumen(string tipoMedioPago, decimal? totalMedioPago = null, string medioPagoOtros = null)
        {
            TipoMedioPago = tipoMedioPago;
            TotalMedioPago = totalMedioPago;
            MedioPagoOtros = medioPagoOtros;
        }
    }

    public class ResumenFactura : IXMLGenerador
    {
        /// <summary>Máximo de repeticiones de MedioPago según el XSD v4.4.</summary>
        private const int MAX_MEDIOS_PAGO = 4;

        /// <summary>fractionDigits de DecimalDineroType.</summary>
        private const int DECIMALES_XSD = 5;

        // Moneda (CodigoMonedaType: CodigoMoneda + TipoCambio, ambos obligatorios)
        private string codigoMoneda;
        private decimal tipoCambio;

        // Servicios
        private decimal totalServGravados;
        private decimal totalServExentos;
        private decimal totalServExonerado;
        private decimal totalServNoSujeto;

        // Mercancías
        private decimal totalMercanciasGravadas;
        private decimal totalMercanciasExentas;
        private decimal totalMercExonerada;
        private decimal totalMercNoSujeta;

        // Totales consolidados
        private decimal totalGravado;
        private decimal totalExento;
        private decimal totalExonerado;
        private decimal totalNoSujeto;
        private decimal totalVenta;
        private decimal totalDescuentos;
        private decimal totalVentaNeta;
        private decimal totalImpuesto;
        private decimal totalImpAsumEmisorFabrica;
        private decimal totalIVADevuelto;
        private decimal totalOtrosCargos;
        private decimal totalComprobante;

        // Desglose de impuestos (v4.4, hasta 1000)
        private List<DesgloseImpuestoResumen> desgloseImpuestos;

        // MedioPago dentro del resumen (v4.4, máx 4)
        private List<MedioPagoResumen> mediosPago;

        public ResumenFactura(
            string codigoMoneda = "CRC",
            decimal tipoCambio = 1,
            decimal totalServGravados = 0,
            decimal totalServExentos = 0,
            decimal totalServExonerado = 0,
            decimal totalServNoSujeto = 0,
            decimal totalMercanciasGravadas = 0,
            decimal totalMercanciasExentas = 0,
            decimal totalMercExonerada = 0,
            decimal totalMercNoSujeta = 0,
            decimal totalGravado = 0,
            decimal totalExento = 0,
            decimal totalExonerado = 0,
            decimal totalNoSujeto = 0,
            decimal totalVenta = 0,
            decimal totalDescuentos = 0,
            decimal totalVentaNeta = 0,
            decimal totalImpuesto = 0,
            decimal totalImpAsumEmisorFabrica = 0,
            decimal totalIVADevuelto = 0,
            decimal totalOtrosCargos = 0,
            decimal totalComprobante = 0,
            List<DesgloseImpuestoResumen> desgloseImpuestos = null,
            List<MedioPagoResumen> mediosPago = null)
        {
            this.codigoMoneda = codigoMoneda;
            this.tipoCambio = tipoCambio;
            this.totalServGravados = totalServGravados;
            this.totalServExentos = totalServExentos;
            this.totalServExonerado = totalServExonerado;
            this.totalServNoSujeto = totalServNoSujeto;
            this.totalMercanciasGravadas = totalMercanciasGravadas;
            this.totalMercanciasExentas = totalMercanciasExentas;
            this.totalMercExonerada = totalMercExonerada;
            this.totalMercNoSujeta = totalMercNoSujeta;
            this.totalGravado = totalGravado;
            this.totalExento = totalExento;
            this.totalExonerado = totalExonerado;
            this.totalNoSujeto = totalNoSujeto;
            this.totalVenta = totalVenta;
            this.totalDescuentos = totalDescuentos;
            this.totalVentaNeta = totalVentaNeta;
            this.totalImpuesto = totalImpuesto;
            this.totalImpAsumEmisorFabrica = totalImpAsumEmisorFabrica;
            this.totalIVADevuelto = totalIVADevuelto;
            this.totalOtrosCargos = totalOtrosCargos;
            this.totalComprobante = totalComprobante;
            this.desgloseImpuestos = desgloseImpuestos ?? new List<DesgloseImpuestoResumen>();
            this.mediosPago = mediosPago ?? new List<MedioPagoResumen>();
        }

        // Propiedades
        public string CodigoMoneda { get => codigoMoneda; set => codigoMoneda = value; }
        public decimal TipoCambio { get => tipoCambio; set => tipoCambio = value; }
        public decimal TotalServGravados { get => totalServGravados; set => totalServGravados = value; }
        public decimal TotalServExentos { get => totalServExentos; set => totalServExentos = value; }
        public decimal TotalServExonerado { get => totalServExonerado; set => totalServExonerado = value; }
        public decimal TotalServNoSujeto { get => totalServNoSujeto; set => totalServNoSujeto = value; }
        public decimal TotalMercanciasGravadas { get => totalMercanciasGravadas; set => totalMercanciasGravadas = value; }
        public decimal TotalMercanciasExentas { get => totalMercanciasExentas; set => totalMercanciasExentas = value; }
        public decimal TotalMercExonerada { get => totalMercExonerada; set => totalMercExonerada = value; }
        public decimal TotalMercNoSujeta { get => totalMercNoSujeta; set => totalMercNoSujeta = value; }
        public decimal TotalGravado { get => totalGravado; set => totalGravado = value; }
        public decimal TotalExento { get => totalExento; set => totalExento = value; }
        public decimal TotalExonerado { get => totalExonerado; set => totalExonerado = value; }
        public decimal TotalNoSujeto { get => totalNoSujeto; set => totalNoSujeto = value; }
        public decimal TotalVenta { get => totalVenta; set => totalVenta = value; }
        public decimal TotalDescuentos { get => totalDescuentos; set => totalDescuentos = value; }
        public decimal TotalVentaNeta { get => totalVentaNeta; set => totalVentaNeta = value; }
        public decimal TotalImpuesto { get => totalImpuesto; set => totalImpuesto = value; }
        public decimal TotalImpAsumEmisorFabrica { get => totalImpAsumEmisorFabrica; set => totalImpAsumEmisorFabrica = value; }
        public decimal TotalIVADevuelto { get => totalIVADevuelto; set => totalIVADevuelto = value; }
        public decimal TotalOtrosCargos { get => totalOtrosCargos; set => totalOtrosCargos = value; }
        public decimal TotalComprobante { get => totalComprobante; set => totalComprobante = value; }
        public List<DesgloseImpuestoResumen> DesgloseImpuestos { get => desgloseImpuestos; set => desgloseImpuestos = value; }
        public List<MedioPagoResumen> MediosPago { get => mediosPago; set => mediosPago = value; }

        /// <summary>
        /// Redondea a la escala permitida por DecimalDineroType (5 decimales)
        /// y serializa con formato invariante, evitando que la cultura del servidor
        /// introduzca coma decimal.
        /// </summary>
        private static string Dinero(decimal valor)
        {
            return XmlConvert.ToString(Math.Round(valor, DECIMALES_XSD, MidpointRounding.AwayFromZero));
        }

        private static XElement Monto(string nombre, decimal valor)
        {
            return new XElement(nombre, Dinero(valor));
        }

        /// <summary>Agrega el elemento únicamente si el monto redondeado es mayor a cero.</summary>
        private static void AgregarSiPositivo(XElement padre, string nombre, decimal valor)
        {
            if (Math.Round(valor, DECIMALES_XSD, MidpointRounding.AwayFromZero) > 0)
                padre.Add(Monto(nombre, valor));
        }

        public XElement GenerarXML()
        {
            var baseXML = new XElement("ResumenFactura");

            // 1. CodigoTipoMoneda — OBLIGATORIO en el XSD (sin minOccurs=0).
            //    Sus dos hijos también son obligatorios, por eso se aplican valores por defecto.
            var moneda = string.IsNullOrWhiteSpace(codigoMoneda) ? "CRC" : codigoMoneda.Trim().ToUpperInvariant();
            var cambio = tipoCambio > 0 ? tipoCambio : 1m;
            baseXML.Add(new XElement("CodigoTipoMoneda",
                new XElement("CodigoMoneda", moneda),
                new XElement("TipoCambio", Dinero(cambio))));

            // 2-5. Totales de servicios
            AgregarSiPositivo(baseXML, "TotalServGravados", totalServGravados);
            AgregarSiPositivo(baseXML, "TotalServExentos", totalServExentos);
            AgregarSiPositivo(baseXML, "TotalServExonerado", totalServExonerado);
            AgregarSiPositivo(baseXML, "TotalServNoSujeto", totalServNoSujeto);

            // 6-9. Totales de mercancías
            AgregarSiPositivo(baseXML, "TotalMercanciasGravadas", totalMercanciasGravadas);
            AgregarSiPositivo(baseXML, "TotalMercanciasExentas", totalMercanciasExentas);
            AgregarSiPositivo(baseXML, "TotalMercExonerada", totalMercExonerada);
            AgregarSiPositivo(baseXML, "TotalMercNoSujeta", totalMercNoSujeta);

            // 10-13. Totales consolidados
            AgregarSiPositivo(baseXML, "TotalGravado", totalGravado);
            AgregarSiPositivo(baseXML, "TotalExento", totalExento);
            AgregarSiPositivo(baseXML, "TotalExonerado", totalExonerado);
            AgregarSiPositivo(baseXML, "TotalNoSujeto", totalNoSujeto);

            // 14. TotalVenta (obligatorio)
            baseXML.Add(Monto("TotalVenta", totalVenta));

            // 15. TotalDescuentos
            AgregarSiPositivo(baseXML, "TotalDescuentos", totalDescuentos);

            // 16. TotalVentaNeta (obligatorio)
            baseXML.Add(Monto("TotalVentaNeta", totalVentaNeta));

            // 17. TotalDesgloseImpuesto (v4.4, hasta 1000) — va ANTES de TotalImpuesto.
            if (desgloseImpuestos != null)
            {
                foreach (var d in DesgloseImpuestoResumen.Consolidar(desgloseImpuestos))
                {
                    baseXML.Add(d.GenerarXML());
                }
            }

            // 18. TotalImpuesto
            AgregarSiPositivo(baseXML, "TotalImpuesto", totalImpuesto);

            // 19. TotalImpAsumEmisorFabrica
            AgregarSiPositivo(baseXML, "TotalImpAsumEmisorFabrica", totalImpAsumEmisorFabrica);

            // 20. TotalIVADevuelto
            AgregarSiPositivo(baseXML, "TotalIVADevuelto", totalIVADevuelto);

            // 21. TotalOtrosCargos
            AgregarSiPositivo(baseXML, "TotalOtrosCargos", totalOtrosCargos);

            // 22. MedioPago (máx 4). TotalMedioPago se vuelve obligatorio con más de un medio.
            if (mediosPago != null)
            {
                var lista = mediosPago
                                .Where(m => m != null && !string.IsNullOrWhiteSpace(m.TipoMedioPago))
                                .Take(MAX_MEDIOS_PAGO)
                                .ToList();

                bool requiereMonto = lista.Count > 1;

                foreach (var mp in lista)
                {
                    var nodo = new XElement("MedioPago",
                        new XElement("TipoMedioPago", mp.TipoMedioPago.Trim()));

                    // MedioPagoOtros es obligatorio cuando el tipo es 99 (Otros).
                    if (mp.TipoMedioPago.Trim() == "99")
                    {
                        var otros = string.IsNullOrWhiteSpace(mp.MedioPagoOtros)
                            ? "Otro medio de pago"
                            : mp.MedioPagoOtros.Trim();

                        if (otros.Length > 100) otros = otros.Substring(0, 100);
                        if (otros.Length < 3) otros = otros.PadRight(3, '.');

                        nodo.Add(new XElement("MedioPagoOtros", otros));
                    }
                    else if (!string.IsNullOrWhiteSpace(mp.MedioPagoOtros))
                    {
                        nodo.Add(new XElement("MedioPagoOtros", mp.MedioPagoOtros.Trim()));
                    }

                    if (mp.TotalMedioPago.HasValue)
                        nodo.Add(Monto("TotalMedioPago", mp.TotalMedioPago.Value));
                    else if (requiereMonto)
                        nodo.Add(Monto("TotalMedioPago", 0));

                    baseXML.Add(nodo);
                }
            }

            // 23. TotalComprobante (obligatorio)
            baseXML.Add(Monto("TotalComprobante", totalComprobante));

            return baseXML;
        }
    }
}