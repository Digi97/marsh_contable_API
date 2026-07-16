using System;
using System.ComponentModel;
using System.Xml.Linq;
using Facturacion_C_Sharp.Utils;

namespace Facturacion_C_Sharp.Lib.DocumentoItems
{
    public class Item : IXMLGenerador
    {
        private int numeroLinea;
        private string codigoCabys;          // Obligatorio v4.4 (13 dígitos)
        private decimal cantidad;
        private string unidadMedida;
        private string detalle;
        private decimal precioUnitario;
        private decimal montoTotal;
        private decimal subTotal;
        private decimal baseImponible;       // Obligatorio cuando hay impuesto
        private decimal impuestoNeto;        // Obligatorio v4.4
        private decimal montoTotalLinea;

        // Opcionales
        private string[] codigosComerciales;
        private string tipoCodigoComercial;  // 01-04, 99
        private decimal descuento;
        private string codigoDescuento;      // 01-09, 99
        private string naturalezaDescuento;
        private Impuesto[] impuestos;
        private Exoneracion[] exoneraciones;
        private string unidadMedidaComercial;
        private string tipoTransaccion;      // 01-13
        private decimal impuestoAsumidoEmisorFabrica;

        public Item(int numeroLinea,
                    string codigoCabys,
                    decimal cantidad,
                    string unidadMedida,
                    string detalle,
                    decimal precioUnitario,
                    decimal montoTotal,
                    decimal subTotal,
                    decimal montoTotalLinea,
                    decimal baseImponible = 0,
                    decimal impuestoNeto = 0,
                    string[] codigosComerciales = null,
                    string tipoCodigoComercial = "04",
                    decimal descuento = 0,
                    string codigoDescuento = "07",
                    string naturalezaDescuento = "",
                    Impuesto[] impuestos = null,
                    Exoneracion[] exoneraciones = null,
                    string unidadMedidaComercial = "",
                    string tipoTransaccion = "", decimal impuestoAsumidoEmisorFabrica = 0)
        {
            this.numeroLinea = numeroLinea;
            this.codigoCabys = codigoCabys;
            this.cantidad = cantidad;
            this.unidadMedida = unidadMedida;
            this.detalle = detalle;
            this.precioUnitario = precioUnitario;
            this.montoTotal = montoTotal;
            this.subTotal = subTotal;
            this.montoTotalLinea = montoTotalLinea;
            this.baseImponible = baseImponible;
            this.impuestoNeto = impuestoNeto;
            this.codigosComerciales = codigosComerciales;
            this.tipoCodigoComercial = tipoCodigoComercial;
            this.descuento = descuento;
            this.codigoDescuento = codigoDescuento;
            this.naturalezaDescuento = naturalezaDescuento;
            this.impuestos = impuestos;
            this.exoneraciones = exoneraciones;
            this.unidadMedidaComercial = unidadMedidaComercial;
            this.tipoTransaccion = tipoTransaccion;
            this.impuestoAsumidoEmisorFabrica = impuestoAsumidoEmisorFabrica;
        }

        // Propiedades
        public int NumeroLinea { get => numeroLinea; set => numeroLinea = value; }
        public string CodigoCabys { get => codigoCabys; set => codigoCabys = value; }
        public string[] CodigosComerciales { get => codigosComerciales; set => codigosComerciales = value; }
        public string TipoCodigoComercial { get => tipoCodigoComercial; set => tipoCodigoComercial = value; }
        public decimal Cantidad { get => cantidad; set => cantidad = value; }
        public string UnidadMedida { get => unidadMedida; set => unidadMedida = value; }
        public string UnidadMedidaComercial { get => unidadMedidaComercial; set => unidadMedidaComercial = value; }
        public string TipoTransaccion { get => tipoTransaccion; set => tipoTransaccion = value; }
        public string Detalle { get => detalle; set => detalle = value; }
        public decimal PrecioUnitario { get => precioUnitario; set => precioUnitario = value; }
        public decimal MontoTotal { get => montoTotal; set => montoTotal = value; }
        public decimal Descuento { get => descuento; set => descuento = value; }
        public string CodigoDescuento { get => codigoDescuento; set => codigoDescuento = value; }
        public string NaturalezaDescuento { get => naturalezaDescuento; set => naturalezaDescuento = value; }
        public decimal SubTotal { get => subTotal; set => subTotal = value; }
        public decimal BaseImponible { get => baseImponible; set => baseImponible = value; }
        public decimal ImpuestoNeto { get => impuestoNeto; set => impuestoNeto = value; }
        public decimal MontoTotalLinea { get => montoTotalLinea; set => montoTotalLinea = value; }
        public Impuesto[] Impuestos { get => impuestos; set => impuestos = value; }
        public Exoneracion[] Exoneraciones { get => exoneraciones; set => exoneraciones = value; }

        public decimal ImpuestoAsumidoEmisorFabrica { get => impuestoAsumidoEmisorFabrica; set => impuestoAsumidoEmisorFabrica = value; }


        public XElement GenerarXML()
        {
            var baseXML = new XElement("LineaDetalle");

            // 1. NumeroLinea (obligatorio)
            baseXML.Add(new XElement("NumeroLinea", numeroLinea));

            // 2. CodigoCABYS (obligatorio v4.4 — 13 dígitos)
            if (!string.IsNullOrEmpty(codigoCabys))
            {
                baseXML.Add(new XElement("CodigoCABYS", codigoCabys));
            }

            // 3. CodigoComercial (opcional, max 5)
            if (codigosComerciales != null)
            {
                foreach (var cod in codigosComerciales)
                {
                    baseXML.Add(new XElement("CodigoComercial",
                        new XElement("Tipo", tipoCodigoComercial),
                        new XElement("Codigo", cod)));
                }
            }

            // 4. Cantidad (obligatorio)
            baseXML.Add(new XElement("Cantidad", cantidad));

            // 5. UnidadMedida (obligatorio)
            baseXML.Add(new XElement("UnidadMedida", unidadMedida));

            // 6. TipoTransaccion (opcional)
            if (!string.IsNullOrEmpty(tipoTransaccion))
            {
                baseXML.Add(new XElement("TipoTransaccion", tipoTransaccion));
            }

            // 7. UnidadMedidaComercial (opcional)
            if (!string.IsNullOrEmpty(unidadMedidaComercial))
            {
                baseXML.Add(new XElement("UnidadMedidaComercial", unidadMedidaComercial));
            }

            // 8. Detalle (obligatorio)
            baseXML.Add(new XElement("Detalle", detalle));

            // 9. PrecioUnitario (obligatorio)
            baseXML.Add(new XElement("PrecioUnitario", precioUnitario));

            // 10. MontoTotal (obligatorio)
            baseXML.Add(new XElement("MontoTotal", montoTotal));

            // 11. Descuento (opcional, estructura v4.4)
            if (descuento > 0)
            {
                var descuentoXml = new XElement("Descuento",
                    new XElement("MontoDescuento", descuento),
                    new XElement("CodigoDescuento", codigoDescuento));

                if (!string.IsNullOrEmpty(naturalezaDescuento))
                {
                    descuentoXml.Add(new XElement("NaturalezaDescuento", naturalezaDescuento));
                }

                baseXML.Add(descuentoXml);
            }

            // 12. SubTotal (obligatorio)
            baseXML.Add(new XElement("SubTotal", subTotal));

            // 13. BaseImponible (obligatorio cuando hay impuesto)
            if (impuestos != null && impuestos.Length > 0)
            {
                baseXML.Add(new XElement("BaseImponible", baseImponible > 0 ? baseImponible : subTotal));
            }

            // 14. Impuestos (con exoneración dentro de cada impuesto)
            if (impuestos != null)
            {
                foreach (var imp in impuestos)
                {
                    baseXML.Add(imp.GenerarXML());
                }
            }

            // 15. ImpuestoNeto (obligatorio v4.4)
            if (impuestos != null && impuestos.Length > 0)
            {
                decimal impNeto = impuestoNeto;
                if (impNeto == 0)
                {
                    // Calcular: suma de montos de impuestos - exoneraciones
                    foreach (var imp in impuestos)
                    {
                        impNeto += imp.Monto;
                        if (imp.Exoneracion != null)
                        {
                            impNeto -= imp.Exoneracion.MontoExoneracion;
                        }
                    }
                }
                baseXML.Add(new XElement("ImpuestoAsumidoEmisorFabrica", impuestoAsumidoEmisorFabrica));
                baseXML.Add(new XElement("ImpuestoNeto", impNeto));
            }

            // 16. MontoTotalLinea (obligatorio)
            baseXML.Add(new XElement("MontoTotalLinea", montoTotalLinea));

            return baseXML;
        }
    }
}
