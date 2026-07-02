using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Modulos
{
    public class AceptaFacturaViewModel
    {
        public string tipoDocumento { get; set; }
        public string clave { get; set; }
        public string numeroConsecutivo { get; set; }
        public string fechaEmision { get; set; }
        public string condicionVenta { get; set; }

        public AceptaEmisorViewModel emisor { get; set; }
        public AceptaReceptorViewModel receptor { get; set; }
        public List<AceptaLineaViewModel> lineas { get; set; }
        public AceptaResumenViewModel resumen { get; set; }

        // Campos adicionales para el procesamiento
        public bool gastoRegistrado { get; set; }

        // Campos que se asignan desde el front-end
        public int Usuarios_Usuario_id { get; set; }
        public int Medio_pago_id { get; set; }
        public int Tipo_moneda_id { get; set; }
        public int dias_credito { get; set; }
        public string presupuesto_id { get; set; }
    }

    public class AceptaEmisorViewModel
    {
        public string nombre { get; set; }
        public string nombreComercial { get; set; }
        public string tipoIdentificacion { get; set; }
        public string numeroIdentificacion { get; set; }
        public string provincia { get; set; }
        public string canton { get; set; }
        public string distrito { get; set; }
        public string otrasSenas { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }
    }

    public class AceptaReceptorViewModel
    {
        public string nombre { get; set; }
        public string nombreComercial { get; set; }
        public string tipoIdentificacion { get; set; }
        public string numeroIdentificacion { get; set; }
        public string provincia { get; set; }
        public string canton { get; set; }
        public string distrito { get; set; }
        public string otrasSenas { get; set; }
        public string telefono { get; set; }
        public string correo { get; set; }
    }

    public class AceptaLineaViewModel
    {
        public string numeroLinea { get; set; }
        public string codigoCabys { get; set; }
        public int cantidad { get; set; }
        public string unidadMedida { get; set; }
        public string detalle { get; set; }
        public double precioUnitario { get; set; }
        public double subTotal { get; set; }
        public double baseImponible { get; set; }
        public double impuestoTarifa { get; set; }
        public double impuestoMonto { get; set; }
        public double montoTotalLinea { get; set; }
    }

    public class AceptaResumenViewModel
    {
        public string codigoMoneda { get; set; }
        public double tipoCambio { get; set; }
        public double totalGravado { get; set; }
        public double totalExento { get; set; }
        public double totalVenta { get; set; }
        public double totalDescuentos { get; set; }
        public double totalVentaNeta { get; set; }
        public double totalImpuesto { get; set; }
        public double totalOtrosCargos { get; set; }
        public double totalComprobante { get; set; }
    }
}