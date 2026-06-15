using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using marsh_contable.Models;
using marsh_contable.Modulos;


namespace marsh_contable.Controllers
{
    public class HomeController : ApiController
    {

        // ─────────────────────────────────────────────
        // Total de usuarios del sistema
        // ─────────────────────────────────────────────
        [HttpGet]
        [Authorize]
        [Route("api/v1/home/total_usuarios")]
        public Reply GetTotalUsuarios()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    int total = ctx.Usuarios.Count();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new { total_usuarios = total };
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

        // ─────────────────────────────────────────────
        // Total de clientes del sistema
        // ─────────────────────────────────────────────
        [HttpGet]
        [Authorize]
        [Route("api/v1/home/total_clientes")]
        public Reply GetTotalClientes()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    int total = ctx.Clientes.Count();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new { total_clientes = total };
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

        // ─────────────────────────────────────────────
        // Total de facturas electrónicas (Tipo_documento_id = 1)
        // ─────────────────────────────────────────────
        [HttpGet]
        [Authorize]
        [Route("api/v1/home/total_facturas")]
        public Reply GetTotalFacturas()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    int total = ctx.Facturas
                        .Count(f => f.Tipo_documento_id == (int)TipoDocumentoId.FacturaElectronica);

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new { total_facturas = total };
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

        // ─────────────────────────────────────────────
        // Total de ganancias del mes (sum de Total en Facturas)
        // ─────────────────────────────────────────────
        [HttpGet]
        [Authorize]
        [Route("api/v1/home/total_ganancias_mes")]
        public Reply GetTotalGananciasMes()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    int mes = DateTime.Now.Month;
                    int anio = DateTime.Now.Year;

                    double total = ctx.Facturas
                        .Where(f => f.fecha.Month == mes &&
                                    f.fecha.Year == anio)
                        .Select(f => f.Total)
                        .DefaultIfEmpty(0)
                        .Sum();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        total_ganancias_mes = total,
                        mes = mes,
                        anio = anio
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

        // ─────────────────────────────────────────────
        // Total de gastos del mes (sum de Total en Gastos)
        // ─────────────────────────────────────────────
        [HttpGet]
        [Authorize]
        [Route("api/v1/home/total_gastos_mes")]
        public Reply GetTotalGastosMes()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    int mes = DateTime.Now.Month;
                    int anio = DateTime.Now.Year;

                    double total = ctx.Gastos
                        .Where(g => g.Fecha.Month == mes &&
                                    g.Fecha.Year == anio)
                        .Select(g => g.Total)
                        .DefaultIfEmpty(0)
                        .Sum();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        total_gastos_mes = total,
                        mes = mes,
                        anio = anio
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
        [Route("api/v1/home/dashboard")]
        public Reply GetDashboard()
        {
            Reply oR = new Reply();
              General tool = new General();
            var hoy = DateTime.Today; // fecha de hoy a las 00:00:00
            var manana = hoy.AddDays(1); // mañana a las 00:00:00
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    int mes = DateTime.Now.Month;

                    int anio = DateTime.Now.Year;

                    int totalUsuarios = ctx.Usuarios.Count(u => u.activo == 1); //buscar solo los activos
                    int totalClientes = ctx.Clientes.Count(c => c.estado == 1);
                    int totalClientesMesActual = ctx.Clientes.Count(c => c.estado == 1 && (c.fecha_creacion.Month == mes && c.fecha_creacion.Year == anio));
                    int totalFacturas = ctx.Facturas.Count(f => f.Tipo_documento_id == (int)TipoDocumentoId.FacturaElectronica);
                    double totalGanancias = ctx.Facturas
                                              .Where(f => f.fecha.Month == mes && f.fecha.Year == anio)
                                              .Select(f => f.Total)
                                              .DefaultIfEmpty(0)
                                              .Sum();
                    double totalGastos = ctx.Gastos
                                              .Where(g => g.Fecha.Month == mes && g.Fecha.Year == anio)
                                              .Select(g => g.Total)
                                              .DefaultIfEmpty(0)
                                              .Sum();

                    var totalPorTipoDocumento = (from td in ctx.Tipo_documento
                                                 join f in ctx.Facturas
                                                     on td.id equals f.Tipo_documento_id
                                                     into facturaGroup
                                                 from f in facturaGroup.DefaultIfEmpty()
                                                 where f == null ||
                                                       (f.fecha.Month == mes && f.fecha.Year == anio)
                                                 group f by new { td.id, td.Codigo_doc, td.Nombre } into g
                                                 orderby g.Key.id
                                                 select new
                                                 {
                                                   
                                                     title = g.Key.Nombre,
                                                     value = g.Count(x => x != null),
                                                  
                                                 }).ToList();


                    var tipoCambioDia = ctx.Tipo_cambio
                                 .Where(c => c.fecha >= hoy && c.fecha < manana)
                                 .Select(c => new TipoCambioViewModel
                                 {
                                     id = c.id,
                                     fecha = c.fecha,
                                     compra = c.compra,
                                     venta = c.venta,
                                     Tipo_moneda_id = c.Tipo_moneda_id
                                 })
                                 .FirstOrDefault();


                    if (tipoCambioDia == null)
                    {
                        var TC = tool.ActualizarTipoCambio();
                        if (TC != null)
                        {
                            tipoCambioDia = TC;
                        }
                    }


                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        total_usuarios = totalUsuarios,
                        total_clientes = totalClientes,
                        total_facturas = totalFacturas,
                        total_ganancias_mes = totalGanancias,
                        total_gastos_mes = totalGastos,
                        mes = mes,
                        anio = anio,
                        facturas_por_tipo_documento = totalPorTipoDocumento,
                        totalClientesMesActual = totalClientesMesActual,
                        tipo_cambio = tipoCambioDia
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