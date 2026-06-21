using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Modulos
{
    public enum EstadoFactura
    {
        Borrador = 1,
        PendienteProcesarHacienda = 2,
        PendienteProcesarHaciendaPendientePago = 3,
        PendienteProcesarHaciendaPagado = 4,
        AceptadoPorHacienda = 5,
        RechazadoPorHacienda = 6,
        AceptadoPendientePago = 7,
        AceptadoPagado = 8,
            Error = 9,
            RecibidoHacienda = 10 
    }
}