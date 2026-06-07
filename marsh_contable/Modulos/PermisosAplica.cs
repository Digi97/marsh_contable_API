using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Modulos
{
    public enum PermisosAplica
    {
        AdministracionUsuarios = 1,
        UsuarioFacturacion = 2,
        UsuarioAceptacionFacturas = 3,
        UsuarioDescarteFacturas = 4,
        UsuarioAgregaFacturas = 5,
        UsuarioGastosFacturas = 6,
        UsuarioMantenimientoClientes = 7,
        UsuarioMantenimientoProveedores = 8,
        UsuarioPresupuestos = 9,
        UsuarioMantenimiento = 10
    }
}