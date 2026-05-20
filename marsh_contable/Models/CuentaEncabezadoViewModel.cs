using System;

namespace marsh_contable.Models
{
    public class CuentaEncabezadoViewModel
    {
        public int id { get; set; }
        public DateTime Vigencia_inicial { get; set; }
        public DateTime Vigencia_final { get; set; }
        public int Tipo_moneda_id { get; set; }
        public int Medio_pago_id { get; set; }
        public double Total { get; set; }
        public double Monto_Proyeccion { get; set; }
        public DateTime Fecha_creacion { get; set; }
        public DateTime Ultima_Fecha_actualizacion { get; set; }
        public int Usuarios_Usuario_id { get; set; }
        public int Facturas_id { get; set; }
        public string Referencia { get; set; }
        public int Clientes_id { get; set; }
        public double impuesto { get; set; }
        public double subtotal { get; set; }
        public short Estado { get; set; }
        public int Tipo_cuentas_id { get; set; }
        public int Cuentas_Contables_id { get; set; }
        public int Centro_Costos_id { get; set; }
        public int Gastos_id { get; set; }

        // Descripciones
        public string Tipo_moneda { get; set; }
        public string Medio_pago { get; set; }
        public string Cliente { get; set; }
        public string Tipo_cuenta { get; set; }
        public string Cuenta_contable { get; set; }
        public string Centro_costo { get; set; }
        public string Usuario { get; set; }
    }
}
