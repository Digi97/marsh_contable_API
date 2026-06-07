using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Modulos
{
    public enum TipoDocumento
    {
        FacturaElectronica = 1,
        NotaDebitoElectronica = 2,
        NotaCreditoElectronica = 3,
        FacturaElectronicaPuntoVenta = 4,
        FacturaElectronicaExportacion = 5,
        FacturaElectronicaCompra = 6,
        ConfirmacionAceptacionMensajeReceptor = 7,
        DepositoGarantia = 8,
        MultasPenalizaciones = 9,
        InteresMoratorio = 10,
        ReciboElectronicoPago = 11,
        Gasto = 12
    }   
}