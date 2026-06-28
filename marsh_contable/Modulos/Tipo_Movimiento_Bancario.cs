using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Modulos
{
    public enum Tipo_Movimiento_Bancario
    {
        Ingreso = 1,
        Egreso = 2,
        Transferencia = 3,
        Ajuste = 4,
        Conciliacion = 5
    }
}