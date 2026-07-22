using System;
using System.Collections.Generic;

namespace marsh_contable.Models
{
    public class CuentaEncabezadoViewModel
    {
        public int id { get; set; }
        public DateTime Vigencia_inicial { get; set; }
        public DateTime Vigencia_final { get; set; }
        public int Tipo_moneda_id { get; set; }
        public int Medio_pago_id { get; set; }
        public decimal Total { get; set; }
        public decimal Monto_Proyeccion { get; set; }
        public DateTime Fecha_creacion { get; set; }
        public DateTime Ultima_Fecha_actualizacion { get; set; }
        public int Usuarios_Usuario_id { get; set; }
        public int? Facturas_id { get; set; }
        public string Referencia { get; set; }
        public int? Clientes_id { get; set; }
        public decimal impuesto { get; set; }
        public decimal subtotal { get; set; }
        public decimal Descuento { get; set; }
        public short Estado { get; set; }
        public int Tipo_cuentas_id { get; set; }
        public int Cuentas_Contables_id { get; set; }
        public int Centro_Costos_id { get; set; }
        public int? Gastos_id { get; set; }
        public int? Ingresos_id { get; set; }
        public int? Proveedor_id { get; set; }

        // Descripciones de joins
        public string Tipo_moneda { get; set; }
        public string Simbolo { get; set; }
        public string Medio_pago { get; set; }
        public string Cliente { get; set; }
        public string Proveedor { get; set; }
        public string Tipo_cuenta { get; set; }
        public string Cuenta_contable { get; set; }
        public string Centro_costo { get; set; }
        public string Usuario { get; set; }
        public string Estado_texto { get; set; }

        // Calculados
        public int DiasVencido { get; set; }
        public decimal Saldo_pendiente { get; set; }

        // Detalle
        public List<CuentaDetalleViewModel> Detalles { get; set; }

        // Presupuesto relacionado
        public Gestion_Presupuestaria gestion { get; set; }
    }
}
