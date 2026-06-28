using System;

namespace marsh_contable.Models
{
    public class CuentaDetalleViewModel
    {
        public int id { get; set; }
        public int Cuenta_Encabezado_id { get; set; }
        public short Tipo_movimiento { get; set; }
        public decimal monto { get; set; }
        public decimal saldo_anterior { get; set; }
        public decimal saldo_posterior { get; set; }
        public DateTime fecha_movimiento { get; set; }
        public string referencia_pago { get; set; }
        public string Observaciones { get; set; }
        public short activo { get; set; }
        public int Usuarios_Usuario_id { get; set; }
        public int Medio_pago_id { get; set; }

        // Descripciones de joins
        public string Referencia_encabezado { get; set; }
        public string Medio_pago { get; set; }
        public string Usuario { get; set; }
        public string Tipo_movimiento_texto { get; set; }
    }
}
