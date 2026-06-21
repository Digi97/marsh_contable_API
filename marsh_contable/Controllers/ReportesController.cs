using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Http;
using System.Net;
using marsh_contable.Models;
using System.Configuration;
using marsh_contable.Modulos;

namespace marsh_contable.Controllers
{
    public class ReportesController : ApiController
    {

        [HttpGet]
        [Authorize]
        [Route("api/v1/reportes/usuarios/filtro")]
        public Reply GetReporteUsuariosFiltrado(int? tipoPermiso = null, DateTime? fechaCreacionDesde =null , DateTime? fechaCreacionHasta = null,
            DateTime? fechaBloqueoDesde = null, DateTime? fechaBloqueoHasta = null
            )
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var query = from u in ctx.Usuarios
                                join r in ctx.Roles on u.Roles_id equals r.id
                                select new
                                {
                                    u.Usuario_id,
                                    u.Nombre,
                                    u.Apellido1,
                                    u.Apellido2,
                                    u.Correo,
                                    u.Id_Empleado,
                                    u.activo,
                                    u.Roles_id,
                                    Rol = r.Descripcion,
                                    Estado = u.activo == 1 ? "Activo" : "Inactivo",
                                    u.Fec_creacion,
                                    u.Fecha_bloqueo
                                };

                    // Filtro dinámico
                    if (tipoPermiso.HasValue && tipoPermiso.Value > 0)
                        query = query.Where(u => u.Roles_id == tipoPermiso.Value);


                    // Permisos del rol filtrado
                    var permisosQuery = from pxr in ctx.Permisos_x_rol
                                        join p in ctx.Permisos on pxr.Permisos_id equals p.id
                                        join r in ctx.Roles on pxr.Roles_id equals r.id
                                        select new
                                        {
                                            pxr.Roles_id,
                                            Rol = r.Descripcion,
                                            NombrePermiso = p.Nombre,
                                            Descripcion = p.Descripcion
                                        };

                    if (tipoPermiso.HasValue && tipoPermiso.Value > 0)
                        permisosQuery = permisosQuery.Where(p => p.Roles_id == tipoPermiso.Value);

                    if(fechaCreacionDesde.HasValue && fechaCreacionHasta.HasValue)
                    {
                        query = query.Where(u => u.Fec_creacion >= fechaCreacionDesde.Value && u.Fec_creacion <= fechaCreacionHasta.Value);
                    }

                    if (fechaBloqueoDesde.HasValue && fechaBloqueoHasta.HasValue)
                    {
                        query = query.Where(u => u.Fecha_bloqueo >= fechaBloqueoDesde.Value && u.Fecha_bloqueo <= fechaBloqueoHasta.Value);
                    }


                    var usuarios = query.OrderBy(u => u.Nombre).ToList();

                    var permisos = permisosQuery.ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        titulo = "Reporte de Usuarios",
                        fecha_generacion = DateTime.Now,
                        filtros = new { tipoPermiso },
                        total_registros = usuarios.Count,
                        usuarios = usuarios,
                        permisos_rol = permisos
                    };
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/reportes/clientes/filtro")]
        public Reply GetReporteClientesFiltrado([FromUri] int? estado = null, int? exonerado = null, DateTime? fechaCreacionDesde = null, DateTime? fechaCreacionHasta = null)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var query = from c in ctx.Clientes
                                join ti in ctx.tipo_identificacion on c.tipo_identificacion_id equals ti.id
                                join p in ctx.Provincia on c.Provincia_id equals p.id
                                join ca in ctx.codigo_actividad on c.codigo_actividad_id equals ca.id
                                select new
                                {
                                    c.id,
                                    c.identificacion,
                                    c.Nombre,
                                    c.Apellido1,
                                    c.Apellido2,
                                    NombreCompleto = c.Nombre + " " + c.Apellido1 + " " + c.Apellido2,
                                    c.correo,
                                    c.estado,
                                    c.exonerado,
                                    c.Provincia_id,
                                    c.fecha_creacion,
                                    Tipo_identificacion = ti.Nombre,
                                    Provincia = p.Nombre,
                                    Codigo_actividad = ca.codigo_actividad1,
                                //    Estado = c.estado == 1 ? "Activo" : "Inactivo",
                                 //   Exonerado = c.exonerado == 1 ? "Sí" : "No",
                              
                                };

                    if (estado.HasValue)
                    {
                        query = query.Where(c => c.estado == estado.Value);
                    }

                    if (exonerado.HasValue)
                    {
                        query = query.Where(c => c.exonerado == exonerado.Value);
                    }
                    if (fechaCreacionDesde.HasValue && fechaCreacionHasta.HasValue)
                    {
                        query = query.Where(c => c.fecha_creacion >= fechaCreacionDesde.Value && c.fecha_creacion <= fechaCreacionHasta.Value);
                    }
                    var clientes = query.OrderBy(c => c.Nombre).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        titulo = "Reporte de Clientes",
                        fecha_generacion = DateTime.Now,
                        filtros = new { estado, exonerado },
                        total_registros = clientes.Count,
                        clientes = clientes
                    };
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/reportes/proveedores/filtro")]
        public Reply GetReporteProveedoresFiltrado([FromUri] int? estado = null, int? exonerado = null, DateTime? fechaCreacionDesde = null, DateTime? fechaCreacionHasta = null)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var query = from p in ctx.Proveedor
                                join ti in ctx.tipo_identificacion on p.tipo_identificacion_id equals ti.id
                                join pr in ctx.Provincia on p.Provincia_id equals pr.id
                                join ca in ctx.codigo_actividad on p.codigo_actividad_id equals ca.id
                                select new
                                {
                                    p.id,
                                    p.identificacion,
                                    p.Nombre,
                                    p.Apellido1,
                                    p.Apellido2,
                                    NombreCompleto = p.Nombre + " " + p.Apellido1 + " " + p.Apellido2,
                                    p.correo,
                                    p.estado,
                                    p.Provincia_id,
                                    p.fecha_creacion,
                                    Tipo_identificacion = ti.Nombre,
                                    Provincia = pr.Nombre,
                                    Codigo_actividad = ca.codigo_actividad1,
                                    Activo = p.estado == 1 ? "Activo" : "Inactivo",
                                    
                                };

                    if (estado.HasValue)
                        query = query.Where(p => p.estado == estado.Value);

                    if (fechaCreacionDesde.HasValue && fechaCreacionHasta.HasValue)
                    {
                        query = query.Where(c => c.fecha_creacion >= fechaCreacionDesde.Value && c.fecha_creacion <= fechaCreacionHasta.Value);
                    }


                    var proveedores = query.OrderBy(p => p.Nombre).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        titulo = "Reporte de Proveedores",
                        fecha_generacion = DateTime.Now,
                        filtros = new { estado, fechaCreacionDesde, fechaCreacionHasta },
                        total_registros = proveedores.Count,
                        proveedores = proveedores
                    };
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // REPORTE DE FACTURAS
        // ═══════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize]
        [Route("api/v1/reportes/facturas/filtro")]
        public Reply GetReporteFacturasFiltrado(
            [FromUri] int? estadoFacturaId = null,
            [FromUri] int? tipoDocumentoId = null,
            [FromUri] int? clienteId = null,
            [FromUri] DateTime? fechaDesde = null,
            [FromUri] DateTime? fechaHasta = null)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var query = from f in ctx.Facturas
                                join c in ctx.Clientes on f.Clientes_id equals c.id
                                join tm in ctx.Tipo_moneda on f.Tipo_moneda_id equals tm.id
                                join ef in ctx.Estado_Factura on f.Estado_Factura_id equals ef.id
                                join td in ctx.Tipo_documento on f.Tipo_documento_id equals td.id
                                join cv in ctx.Condicion_venta on f.Condicion_venta_id equals cv.id
                                join mp in ctx.Medio_pago on f.Medio_pago_id equals mp.id
                                select new
                                {
                                    f.id,
                                    f.Clave,
                                    f.Consecutivo_electronico,
                                    f.fecha,
                                    f.consecutivo,
                                    f.Subtotal,
                                    f.Impuesto,
                                    f.Total,
                                    f.Descuento,
                                    f.cambio_venta,
                                    f.cambio_compra,
                                    f.Clientes_id,
                                    f.Estado_Factura_id,
                                    f.Tipo_documento_id,
                                    Cliente = c.Nombre + " " + c.Apellido1,
                                    Tipo_moneda = tm.Nombre,
                                    Estado_factura = ef.Nombre,
                                    Tipo_documento = td.Nombre,
                                    Condicion_venta = cv.Descripcion,
                                    Medio_pago = mp.descripcion
                                };

                    // Filtros dinámicos
                    if (estadoFacturaId.HasValue && estadoFacturaId.Value > 0)
                        query = query.Where(f => f.Estado_Factura_id == estadoFacturaId.Value);

                    if (tipoDocumentoId.HasValue && tipoDocumentoId.Value > 0)
                        query = query.Where(f => f.Tipo_documento_id == tipoDocumentoId.Value);

                    if (clienteId.HasValue && clienteId.Value > 0)
                        query = query.Where(f => f.Clientes_id == clienteId.Value);

                    if (fechaDesde.HasValue && fechaHasta.HasValue)
                        query = query.Where(f => f.fecha >= fechaDesde.Value && f.fecha <= fechaHasta.Value);

                    var facturas = query.OrderByDescending(f => f.id).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        titulo = "Reporte de Facturas",
                        fecha_generacion = DateTime.Now,
                        filtros = new { estadoFacturaId, tipoDocumentoId, clienteId, fechaDesde, fechaHasta },
                        total_registros = facturas.Count,
                        total_subtotal = facturas.Sum(f => f.Subtotal),
                        total_impuesto = facturas.Sum(f => f.Impuesto),
                        total_descuento = facturas.Sum(f => f.Descuento),
                        total_general = facturas.Sum(f => f.Total),
                        facturas = facturas
                    };
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }


        // ═══════════════════════════════════════════════════════════
        // REPORTE DE GASTOS
        // ═══════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize]
        [Route("api/v1/reportes/gastos/filtro")]
        public Reply GetReporteGastosFiltrado(
            [FromUri] int? categoriaGastoId = null,
            [FromUri] int? proveedorId = null,
            [FromUri] int? tipoDocumentoId = null,
            [FromUri] int? medioPagoId = null,
            [FromUri] DateTime? fechaDesde = null,
            [FromUri] DateTime? fechaHasta = null)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var query = from g in ctx.Gastos
                                join cg in ctx.Categoria_gasto on g.Categoria_gasto_id equals cg.id
                                join td in ctx.Tipo_documento on g.Tipo_documento_id equals td.id
                                join mp in ctx.Medio_pago on g.Medio_pago_id equals mp.id
                                join p in ctx.Proveedor on g.Proveedor_id equals p.id
                                join u in ctx.Usuarios on g.Usuarios_Usuario_id equals u.Usuario_id
                                join m in ctx.Tipo_moneda on g.Tipo_moneda_id equals m.id
                                select new
                                {
                                    g.id,
                                    g.Descripcion,
                                    g.Doc_Referencia,
                                    g.Fecha,
                                    g.Subtotal,
                                    g.Impuesto,
                                    g.Total,
                                    g.Descuento,
                                    g.Categoria_gasto_id,
                                    g.Proveedor_id,
                                    g.Tipo_documento_id,
                                    g.Medio_pago_id,
                                    Categoria_gasto = cg.Nombre,
                                    Tipo_documento = td.Nombre,
                                    Medio_pago = mp.descripcion,
                                    Proveedor = p.Nombre + " " + p.Apellido1 + " " + p.Apellido2,
                                    Usuario = u.Nombre + " " + u.Apellido1,
                                    Tipo_moneda = m.Simbolo
                                };

                    // Filtros dinámicos
                    if (categoriaGastoId.HasValue && categoriaGastoId.Value > 0)
                        query = query.Where(g => g.Categoria_gasto_id == categoriaGastoId.Value);

                    if (proveedorId.HasValue && proveedorId.Value > 0)
                        query = query.Where(g => g.Proveedor_id == proveedorId.Value);

                    if (tipoDocumentoId.HasValue && tipoDocumentoId.Value > 0)
                        query = query.Where(g => g.Tipo_documento_id == tipoDocumentoId.Value);

                    if (medioPagoId.HasValue && medioPagoId.Value > 0)
                        query = query.Where(g => g.Medio_pago_id == medioPagoId.Value);

                    if (fechaDesde.HasValue && fechaHasta.HasValue)
                        query = query.Where(g => g.Fecha >= fechaDesde.Value && g.Fecha <= fechaHasta.Value);

                    var gastos = query.OrderByDescending(g => g.id).ToList();

                    // Agrupar por categoría para resumen
                    var resumenCategoria = gastos
                        .GroupBy(g => g.Categoria_gasto)
                        .Select(grp => new
                        {
                            categoria = grp.Key,
                            cantidad = grp.Count(),
                            total_monto = grp.Sum(g => g.Total)
                        }).ToList();

                    // Agrupar por proveedor para resumen
                    var resumenProveedor = gastos
                        .GroupBy(g => g.Proveedor)
                        .Select(grp => new
                        {
                            proveedor = grp.Key,
                            cantidad = grp.Count(),
                            total_monto = grp.Sum(g => g.Total)
                        }).OrderByDescending(x => x.total_monto).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        titulo = "Reporte de Gastos",
                        fecha_generacion = DateTime.Now,
                        filtros = new { categoriaGastoId, proveedorId, tipoDocumentoId, medioPagoId, fechaDesde, fechaHasta },
                        total_registros = gastos.Count,
                        total_subtotal = gastos.Sum(g => g.Subtotal),
                        total_impuesto = gastos.Sum(g => g.Impuesto),
                        total_descuento = gastos.Sum(g => g.Descuento),
                        total_general = gastos.Sum(g => g.Total),
                        resumen_categoria = resumenCategoria,
                        resumen_proveedor = resumenProveedor,
                        gastos = gastos
                    };
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }


        // ═══════════════════════════════════════════════════════════
        // REPORTE DE GESTIÓN PRESUPUESTARIA DETALLE
        // ═══════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize]
        [Route("api/v1/reportes/gestion_presupuestaria/filtro")]
        public Reply GetReporteGestionPresupuestariaFiltrado(
            [FromUri] int? gestionId = null,
            [FromUri] int? categoriaId = null,
            [FromUri] int? usuarioId = null,
            [FromUri] DateTime? fechaDesde = null,
            [FromUri] DateTime? fechaHasta = null)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var query = from d in ctx.Gestion_P_detalle
                                join gp in ctx.Gestion_Presupuestaria on d.Gestion_Presupuestaria_id equals gp.id
                                join cp in ctx.Categoria_presupuestaria on d.Categoria_presupuestaria_id equals cp.id
                                join u in ctx.Usuarios on d.Usuarios_Usuario_id equals u.Usuario_id

                                // LEFT JOIN Facturas
                                join f in ctx.Facturas on d.Facturas_id equals f.id into facturaGroup
                                from f in facturaGroup.DefaultIfEmpty()

                                    // LEFT JOIN Gastos
                                join g in ctx.Gastos on d.Gastos_id equals g.id into gastoGroup
                                from g in gastoGroup.DefaultIfEmpty()

                                    // LEFT JOIN Ingresos
                                join i in ctx.Ingresos on d.Ingresos_id equals i.id into ingresoGroup
                                from i in ingresoGroup.DefaultIfEmpty()

                                select new
                                {
                                    d.id,
                                    d.Monto,
                                    d.Monto_aprobado,
                                    d.Monto_modificado,
                                    d.Monto_compometido,
                                    d.Monto_ejecutado,
                                    d.detalle_presupuesto,
                                    d.Observaciones,
                                    d.Fecha_registro,
                                    d.activo,
                                    d.Gestion_Presupuestaria_id,
                                    d.Categoria_presupuestaria_id,
                                    d.Usuarios_Usuario_id,
                                    d.Gastos_id,
                                    d.Facturas_id,
                                    d.Ingresos_id,
                                    Gestion_nombre = gp.nombre,
                                    Gestion_anio = gp.anio_presupuesto,
                                    Categoria = cp.nombre,
                                    Usuario = u.Nombre + " " + u.Apellido1,
                                    Factura_clave = f != null ? f.Clave : "",
                                    Factura_total = f != null ? f.Total : 0,
                                    Gasto_descripcion = g != null ? g.Descripcion : "",
                                    Gasto_total = g != null ? g.Total : 0,
                                    Ingreso_codigo = i != null ? i.Codigo : "",
                                    Ingreso_total = i != null ? i.Total : 0,
                                    Tipo_movimiento = d.Facturas_id != null ? "Factura"
                                                        : d.Gastos_id != null ? "Gasto"
                                                        : d.Ingresos_id != null ? "Ingreso"
                                                        : "Manual"
                                };

                    // Filtros dinámicos
                    if (gestionId.HasValue && gestionId.Value > 0)
                        query = query.Where(d => d.Gestion_Presupuestaria_id == gestionId.Value);

                    if (categoriaId.HasValue && categoriaId.Value > 0)
                        query = query.Where(d => d.Categoria_presupuestaria_id == categoriaId.Value);

                    if (usuarioId.HasValue && usuarioId.Value > 0)
                        query = query.Where(d => d.Usuarios_Usuario_id == usuarioId.Value);

                    if (fechaDesde.HasValue && fechaHasta.HasValue)
                        query = query.Where(d => d.Fecha_registro >= fechaDesde.Value && d.Fecha_registro <= fechaHasta.Value);

                    var detalles = query.OrderByDescending(d => d.id).ToList();

                    // Resumen por tipo de movimiento
                    var resumenMovimiento = detalles
                        .GroupBy(d => d.Tipo_movimiento)
                        .Select(grp => new
                        {
                            tipo_movimiento = grp.Key,
                            cantidad = grp.Count(),
                            total_ejecutado = grp.Sum(d => (double)d.Monto_ejecutado)
                        }).ToList();

                    // Resumen por categoría presupuestaria
                    var resumenCategoria = detalles
                        .GroupBy(d => d.Categoria)
                        .Select(grp => new
                        {
                            categoria = grp.Key,
                            cantidad = grp.Count(),
                            total_ejecutado = grp.Sum(d => (double)d.Monto_ejecutado)
                        }).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        titulo = "Reporte de Gestión Presupuestaria - Detalle de Movimientos",
                        fecha_generacion = DateTime.Now,
                        filtros = new { gestionId, categoriaId, usuarioId, fechaDesde, fechaHasta },
                        total_registros = detalles.Count,
                        total_monto_ejecutado = detalles.Sum(d => (double)d.Monto_ejecutado),
                        total_monto = detalles.Sum(d => d.Monto),
                        resumen_movimiento = resumenMovimiento,
                        resumen_categoria = resumenCategoria,
                        detalles = detalles
                    };
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }

    }
}