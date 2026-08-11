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

    // NOTA: El endpoint gestion_presupuestaria/filtro ahora también incluye un nodo
    // "reporte_formato_plantilla" que replica la estructura de Modulos/PRESUPUESTO EJEMPLO.xlsx
    // (secciones INGRESOS/EGRESOS, desglose mensual Ene-Dic, Ejecutado, Recursos Estimados,
    // Por Gastar y % de Ejecución), para que el frontend pueda exportarlo a Excel manteniendo
    // el mismo formato que usa la organización.
    public class ReportesController : ApiController
    {

        [HttpGet]
        [Authorize]
        [Route("api/v1/reportes/usuarios/filtro")]
        [RequierePermiso(PermisosAplica.UsuarioReporte)]

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
                                    u.Roles_id,
                                    Nombre_Rol = r.Descripcion,
                                    Estado = u.activo == 1 ? "Activo" : "Inactivo",
                                    Fecha_Creacion = u.Fec_creacion,
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
                        query = query.Where(u => u.Fecha_Creacion >= fechaCreacionDesde.Value && u.Fecha_Creacion <= fechaCreacionHasta.Value);
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
        [RequierePermiso(PermisosAplica.UsuarioReporte)]

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
                                join cant in ctx.Canton on c.Canton_id equals cant.id 
                                join dist in ctx.Distrito on c.Distrito_id equals dist.id
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
                                    c.fecha_creacion,
                                    Tipo_identificacion = ti.Nombre,
                                    Provincia = p.Nombre,
                                    Canton = cant.Nombre,
                                    Distrito = dist.Nombre,
                                    Codigo_actividad = ca.codigo_actividad1,
                                    Estado = c.estado == 1 ? "Activo" : "Inactivo",
                                    Exonerado = c.exonerado == 1 ? "Sí" : "No",
                              
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
                  //  var clientes = query.OrderBy(c => c.Nombre).ToList();

                    var clientes = query
                        .OrderBy(c => c.Nombre)
                        .Select(c => new
                        {
                            c.id,
                            c.identificacion,
                            c.Nombre,
                            c.Apellido1,
                            c.Apellido2,
                            c.NombreCompleto,
                            c.correo,
                            c.fecha_creacion,
                            c.Tipo_identificacion,
                            c.Provincia,
                            c.Canton,
                            c.Distrito,
                            c.Codigo_actividad,
                            c.Estado,
                            c.Exonerado,
                        })
                        .ToList();

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
        [RequierePermiso(PermisosAplica.UsuarioReporte)]

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
                                join cant in ctx.Canton on p.Canton_id equals cant.id
                                join dist in ctx.Distrito on p.Distrito_id equals dist.id

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
                                    p.fecha_creacion,
                                    Tipo_identificacion = ti.Nombre,
                                    Provincia = pr.Nombre,
                                    Canton = cant.Nombre,
                                    Distrito = dist.Nombre,
                                    Codigo_actividad = ca.codigo_actividad1,
                                    Estado = p.estado == 1 ? "Activo" : "Inactivo",
                                    
                                };

                    if (estado.HasValue)
                        query = query.Where(p => p.estado == estado.Value);

                    if (fechaCreacionDesde.HasValue && fechaCreacionHasta.HasValue)
                    {
                        query = query.Where(c => c.fecha_creacion >= fechaCreacionDesde.Value && c.fecha_creacion <= fechaCreacionHasta.Value);
                    }


                    //    var proveedores = query.OrderBy(p => p.Nombre).ToList();

                    var proveedores = query
                     .OrderBy(c => c.Nombre)
                     .Select(c => new
                     {
                         c.id,
                         c.identificacion,
                         c.Nombre,
                         c.Apellido1,
                         c.Apellido2,
                         c.NombreCompleto,
                         c.correo,
                         c.fecha_creacion,
                         c.Tipo_identificacion,
                         c.Provincia,
                         c.Canton,
                         c.Distrito,
                         c.Codigo_actividad,
                         c.Estado
                     })
                     .ToList();

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
        [RequierePermiso(PermisosAplica.UsuarioReporte)]

        public Reply GetReporteFacturasFiltrado(
            [FromUri] int? tipoDocumentoId = null,
            [FromUri] int? clienteId = null,
            [FromUri] DateTime? fechaCreacionDesde = null,
            [FromUri] DateTime? fechaCreacionHasta = null)
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
                                    Cliente = c.Nombre + " " + c.Apellido1 + " "+c.Apellido2,
                                    Tipo_moneda = tm.Nombre,
                                    Estado_factura = ef.Nombre,
                                    Tipo_documento = td.Nombre,
                                    Condicion_venta = cv.Descripcion,
                                    Medio_pago = mp.descripcion
                                };

              ;

                    if (tipoDocumentoId.HasValue && tipoDocumentoId.Value > 0)
                        query = query.Where(f => f.Tipo_documento_id == tipoDocumentoId.Value);

                    if (clienteId.HasValue && clienteId.Value > 0)
                        query = query.Where(f => f.Clientes_id == clienteId.Value);

                    if (fechaCreacionDesde.HasValue && fechaCreacionHasta.HasValue)
                        query = query.Where(f => f.fecha >= fechaCreacionDesde.Value && f.fecha <= fechaCreacionHasta.Value);

                    var facturas = query.OrderByDescending(f => f.id).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        titulo = "Reporte de Facturas",
                        fecha_generacion = DateTime.Now,
                        filtros = new {tipoDocumentoId, clienteId, fechaCreacionDesde, fechaCreacionHasta },
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
        [RequierePermiso(PermisosAplica.UsuarioReporte)]

        public Reply GetReporteGastosFiltrado(
            [FromUri] int? categoriaGastoId = null,
            [FromUri] int? proveedorId = null,
            [FromUri] int? medioPagoId = null,
            [FromUri] DateTime? fechaCreacionDesde = null,
            [FromUri] DateTime? fechaCreacionHasta = null)
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
                                    Usuario = u.Nombre + " " + u.Apellido1 + " " + u.Apellido2,
                                    Tipo_moneda = m.Simbolo
                                };

                    // Filtros dinámicos
                    if (categoriaGastoId.HasValue && categoriaGastoId.Value > 0)
                        query = query.Where(g => g.Categoria_gasto_id == categoriaGastoId.Value);

                    if (proveedorId.HasValue && proveedorId.Value > 0)
                        query = query.Where(g => g.Proveedor_id == proveedorId.Value);

                ;

                    if (medioPagoId.HasValue && medioPagoId.Value > 0)
                        query = query.Where(g => g.Medio_pago_id == medioPagoId.Value);

                    if (fechaCreacionDesde.HasValue && fechaCreacionHasta.HasValue)
                        query = query.Where(g => g.Fecha >= fechaCreacionDesde.Value && g.Fecha <= fechaCreacionHasta.Value);

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
                        filtros = new { categoriaGastoId, proveedorId, medioPagoId, fechaCreacionDesde, fechaCreacionHasta },
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
        [RequierePermiso(PermisosAplica.UsuarioReporte)]

        public Reply GetReporteGestionPresupuestariaFiltrado(
            [FromUri] DateTime? fechaCreacionDesde = null,
            [FromUri] DateTime? fechaCreacionHasta = null)
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

                    if (fechaCreacionDesde.HasValue && fechaCreacionHasta.HasValue)
                        query = query.Where(d => d.Fecha_registro >= fechaCreacionDesde.Value && d.Fecha_registro <= fechaCreacionHasta.Value);

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

                    var reportePlantilla = ConstruirReportePlantillaPresupuesto(ctx, fechaCreacionDesde, fechaCreacionHasta);

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        titulo = "Reporte de Gestión Presupuestaria - Detalle de Movimientos",
                        fecha_generacion = DateTime.Now,
                        filtros = new { fechaCreacionDesde, fechaCreacionHasta},
                        total_registros = detalles.Count,
                        total_monto_ejecutado = detalles.Sum(d => (double)d.Monto_ejecutado),
                        total_monto = detalles.Sum(d => d.Monto),
                        resumen_movimiento = resumenMovimiento,
                        resumen_categoria = resumenCategoria,
                        detalles = detalles,
                        reporte_formato_plantilla = reportePlantilla
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
        // Construye el reporte de presupuesto con el mismo formato que
        // Modulos/PRESUPUESTO EJEMPLO.xlsx: secciones INGRESOS y EGRESOS, cada una con sus
        // "cuentas" (categorías) desglosadas mes a mes (Ene-Dic), más Ejecutado, Recursos
        // Estimados (monto aprobado + modificado del/los presupuesto(s) vigentes en el período),
        // Por Gastar y % de Ejecución.
        //
        // NOTA: la plantilla original desglosa "recursos estimados" por cada cuenta contable
        // individual; el modelo de datos actual solo registra el monto aprobado/modificado a
        // nivel de Gestion_Presupuestaria (no por categoría de gasto/ingreso), por lo que esos
        // tres indicadores (Recursos Estimados, Por Gastar, % Ejecución) se calculan a nivel de
        // sección (INGRESOS/EGRESOS) en vez de por cuenta individual.
        // ═══════════════════════════════════════════════════════════
        private object ConstruirReportePlantillaPresupuesto(Models.EntitiesModel ctx, DateTime? desde, DateTime? hasta)
        {
            string[] nombresMeses = { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            // ── EGRESOS: agrupados por Categoria_gasto (equivalente a "cuenta contable" en la plantilla)
            var detallesGastos = from d in ctx.Gestion_P_detalle
                                  join g in ctx.Gastos on d.Gastos_id equals g.id
                                  join cg in ctx.Categoria_gasto on g.Categoria_gasto_id equals cg.id
                                  where d.Gastos_id != null
                                  select new { d.Fecha_registro, d.Monto_ejecutado, Cuenta = cg.Nombre };

            if (desde.HasValue && hasta.HasValue)
                detallesGastos = detallesGastos.Where(x => x.Fecha_registro >= desde.Value && x.Fecha_registro <= hasta.Value);

            var listaGastos = detallesGastos.ToList();

            var cuentasEgresos = listaGastos
                .GroupBy(x => x.Cuenta)
                .Select(grp =>
                {
                    var meses = new decimal[12];
                    foreach (var item in grp)
                    {
                        meses[item.Fecha_registro.Month - 1] += item.Monto_ejecutado;
                    }
                    return new
                    {
                        cuenta = grp.Key,
                        meses = meses,
                        ejecutado = grp.Sum(x => x.Monto_ejecutado)
                    };
                })
                .OrderBy(x => x.cuenta)
                .ToList();

            // ── INGRESOS: no existe una tabla de categorías para Ingresos en el esquema actual,
            // se agrupan bajo una única "cuenta" general (a diferencia de Egresos, que sí tiene
            // Categoria_gasto). Si a futuro se agrega una categoría de ingresos, se puede agrupar
            // de la misma forma que Egresos.
            var detallesIngresos = from d in ctx.Gestion_P_detalle
                                    where d.Ingresos_id != null
                                    select new { d.Fecha_registro, d.Monto_ejecutado };

            if (desde.HasValue && hasta.HasValue)
                detallesIngresos = detallesIngresos.Where(x => x.Fecha_registro >= desde.Value && x.Fecha_registro <= hasta.Value);

            var listaIngresos = detallesIngresos.ToList();
            var mesesIngresos = new decimal[12];
            foreach (var item in listaIngresos)
            {
                mesesIngresos[item.Fecha_registro.Month - 1] += item.Monto_ejecutado;
            }
            var cuentasIngresos = new List<object>
            {
                new { cuenta = "INGRESOS GENERALES", meses = mesesIngresos, ejecutado = listaIngresos.Sum(x => x.Monto_ejecutado) }
            };

            // ── Recursos estimados a nivel de presupuesto(s) vigente(s) en el período (Categoria_presupuestaria: Ingresos=2, Gastos=3)
            var presupuestos = ctx.Gestion_Presupuestaria.ToList();
            double recursosEstimadosIngresos = presupuestos
                .Where(p => p.Categoria_presupuestaria_id == (int)Modulos.Categoria_presupuestaria.Ingresos)
                .Sum(p => p.monto_aprobado + p.monto_modificado);
            double recursosEstimadosEgresos = presupuestos
                .Where(p => p.Categoria_presupuestaria_id == (int)Modulos.Categoria_presupuestaria.Gastos)
                .Sum(p => p.monto_aprobado + p.monto_modificado);

            decimal ejecutadoIngresos = listaIngresos.Sum(x => x.Monto_ejecutado);
            decimal ejecutadoEgresos = listaGastos.Sum(x => x.Monto_ejecutado);

            object SeccionResumen(string nombre, object cuentas, decimal ejecutado, double recursosEstimados) => new
            {
                nombre,
                cuentas,
                ejecutado,
                recursos_estimados = recursosEstimados,
                por_gastar = recursosEstimados - (double)ejecutado,
                ejecucion_pct = recursosEstimados > 0 ? Math.Round((double)ejecutado / recursosEstimados * 100, 2) : 0
            };

            return new
            {
                nombres_meses = nombresMeses,
                secciones = new object[]
                {
                    SeccionResumen("INGRESOS", cuentasIngresos, ejecutadoIngresos, recursosEstimadosIngresos),
                    SeccionResumen("EGRESOS", cuentasEgresos, ejecutadoEgresos, recursosEstimadosEgresos)
                }
            };
        }

    }
}