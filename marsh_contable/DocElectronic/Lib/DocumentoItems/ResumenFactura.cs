using System;
using System.ComponentModel;
using System.Xml.Linq;
using Facturacion_C_Sharp.Utils;

namespace Facturacion_C_Sharp.Lib.DocumentoItems
{
    public class ResumenFactura : IXMLGenerador
    {
        // Moneda (estructura compleja v4.4)
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

        // MedioPago dentro del resumen (v4.4, max 4)
        private string[] mediosPago;

        public ResumenFactura(
            string codigoMoneda = "",
            decimal tipoCambio = 0,
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
            string[] mediosPago = null)
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
            this.mediosPago = mediosPago;
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
        public string[] MediosPago { get => mediosPago; set => mediosPago = value; }

        public XElement GenerarXML()
        {
            var baseXML = new XElement("ResumenFactura");

            // 1. CodigoTipoMoneda (estructura compleja v4.4)
            if (!string.IsNullOrEmpty(codigoMoneda))
            {
                baseXML.Add(new XElement("CodigoTipoMoneda",
                    new XElement("CodigoMoneda", codigoMoneda),
                    new XElement("TipoCambio", tipoCambio > 0 ? tipoCambio : 1)));
            }

            // 2-5. Totales de servicios (solo si > 0, son minOccurs=0)
            if (totalServGravados > 0)
                baseXML.Add(new XElement("TotalServGravados", totalServGravados));
            if (totalServExentos > 0)
                baseXML.Add(new XElement("TotalServExentos", totalServExentos));
            if (totalServExonerado > 0)
                baseXML.Add(new XElement("TotalServExonerado", totalServExonerado));
            if (totalServNoSujeto > 0)
                baseXML.Add(new XElement("TotalServNoSujeto", totalServNoSujeto));

            // 6-9. Totales de mercancías
            if (totalMercanciasGravadas > 0)
                baseXML.Add(new XElement("TotalMercanciasGravadas", totalMercanciasGravadas));
            if (totalMercanciasExentas > 0)
                baseXML.Add(new XElement("TotalMercanciasExentas", totalMercanciasExentas));
            if (totalMercExonerada > 0)
                baseXML.Add(new XElement("TotalMercExonerada", totalMercExonerada));
            if (totalMercNoSujeta > 0)
                baseXML.Add(new XElement("TotalMercNoSujeta", totalMercNoSujeta));

            // 10-13. Totales consolidados
            if (totalGravado > 0)
                baseXML.Add(new XElement("TotalGravado", totalGravado));
            if (totalExento > 0)
                baseXML.Add(new XElement("TotalExento", totalExento));
            if (totalExonerado > 0)
                baseXML.Add(new XElement("TotalExonerado", totalExonerado));
            if (totalNoSujeto > 0)
                baseXML.Add(new XElement("TotalNoSujeto", totalNoSujeto));

            // 14. TotalVenta (obligatorio)
            baseXML.Add(new XElement("TotalVenta", totalVenta));

            // 15. TotalDescuentos
            if (totalDescuentos > 0)
                baseXML.Add(new XElement("TotalDescuentos", totalDescuentos));

            // 16. TotalVentaNeta (obligatorio)
            baseXML.Add(new XElement("TotalVentaNeta", totalVentaNeta));

            // 17. TotalImpuesto
            if (totalImpuesto > 0)
                baseXML.Add(new XElement("TotalImpuesto", totalImpuesto));

            // 18. TotalImpAsumEmisorFabrica
            if (totalImpAsumEmisorFabrica > 0)
                baseXML.Add(new XElement("TotalImpAsumEmisorFabrica", totalImpAsumEmisorFabrica));

            // 19. TotalIVADevuelto
            if (totalIVADevuelto > 0)
                baseXML.Add(new XElement("TotalIVADevuelto", totalIVADevuelto));

            // 20. TotalOtrosCargos
            if (totalOtrosCargos > 0)
                baseXML.Add(new XElement("TotalOtrosCargos", totalOtrosCargos));

            // 21. MedioPago (v4.4, max 4 repeticiones)
            if (mediosPago != null)
            {
                foreach (var mp in mediosPago)
                {
                    baseXML.Add(new XElement("MedioPago",
                        new XElement("TipoMedioPago", mp)));
                }
            }

            // 22. TotalComprobante (obligatorio)
            baseXML.Add(new XElement("TotalComprobante", totalComprobante));

            return baseXML;
        }
    }
}
