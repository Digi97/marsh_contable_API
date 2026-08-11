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
    public class CuentaDetalleController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/cuenta_detalle")]
        public Reply CreateCuentaDetalle([FromBody] Models.Cuenta_Detalle model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                #region "Validaciones"
                if (model == null)
                    throw new Exception("invalid_model_request_missing");

                if (!tool.validaNumeros(model.Cuenta_Encabezado_id.ToString()))
                    throw new Exception("invalid_value_form_Cuenta_Encabezado_id");

                if (model.monto <= 0)
                    throw new Exception("monto_must_be_greater_than_zero");

                if (!tool.validaNumeros(model.Medio_pago_id.ToString()))
                    throw new Exception("invalid_value_form_Medio_pago_id");
                #endregion

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled   = false;
                    ctx.Configuration.ProxyCreationEnabled = false;
                    // Obtener el encabezado para calcular saldos
                    var encabezado = ctx.Cuenta_Encabezado
                        .FirstOrDefault(ce => ce.id == model.Cuenta_Encabezado_id);
                    if (encabezado == null)
                        throw new Exception("cuenta_encabezado_not_found");

                    // Calcular saldo pendiente actual
                    decimal totalAbonos = ctx.Cuenta_Detalle
                        .Where(cd => cd.Cuenta_Encabezado_id == model.Cuenta_Encabezado_id &&
                                    cd.activo == 1)
                        .Select(cd => cd.monto)
                        .DefaultIfEmpty(0)
                        .Sum();

                    decimal saldoAnterior = encabezado.Total - totalAbonos;

                    if (model.monto > saldoAnterior)
                        throw new Exception($"monto_excede_saldo_pendiente_{saldoAnterior}");

                    decimal saldoPosterior = saldoAnterior - model.monto;

                    Models.Cuenta_Detalle d = new Models.Cuenta_Detalle()
                    {
                        Cuenta_Encabezado_id = model.Cuenta_Encabezado_id,
                        Tipo_movimiento      = model.Tipo_movimiento,
                        monto                = model.monto,
                        saldo_anterior       = saldoAnterior,
                        saldo_posterior      = saldoPosterior,
                        fecha_movimiento     = DateTime.Now,
                        referencia_pago      = model.referencia_pago ?? "",
                        Observaciones        = model.Observaciones ?? "",
                        activo               = 1,
                        Usuarios_Usuario_id  = model.Usuarios_Usuario_id,
                        Medio_pago_id        = model.Medio_pago_id
                    };
                    ctx.Cuenta_Detalle.Add(d);

                    // Si el saldo posterior es 0, marcar la cuenta como pagada
                    if (saldoPosterior == 0)
                    {
                        encabezado.Estado                     = 2; // Pagada
                        encabezado.Ultima_Fecha_actualizacion = DateTime.Now;
                    }

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = new
                    {
                        detalle_id      = d.id,
                        saldo_anterior  = saldoAnterior,
                        saldo_posterior = saldoPosterior,
                        cuenta_pagada   = saldoPosterior == 0
                    };
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;

                if (ex is System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    var errores = dbEx.EntityValidationErrors
                        .SelectMany(eve => eve.ValidationErrors)
                        .Select(ve => ve.ErrorMessage);

                    oR.Message = string.Join(" | ", errores);
                }
                else
                {
                    oR.Message = ex.Message;
                }

                return oR;
            }
        }


        [HttpPut]
        [Authorize]
        [Route("api/v1/cuenta_detalle/{id}")]
        public Reply UpdateCuentaDetalle(int id, [FromBody] Models.Cuenta_Detalle model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (model == null)
                    throw new Exception("invalid_model_request_missing");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuenta_Detalle d = ctx.Cuenta_Detalle.FirstOrDefault(u => u.id == id);
                    if (d == null)
                        throw new Exception("cuenta_detalle_not_found");

                    d.Tipo_movimiento     = model.Tipo_movimiento;
                    d.monto               = model.monto;
                    d.referencia_pago     = model.referencia_pago ?? "";
                    d.Observaciones       = model.Observaciones ?? "";
                    d.Medio_pago_id       = model.Medio_pago_id;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = d.id;
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;

                if (ex is System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    var errores = dbEx.EntityValidationErrors
                        .SelectMany(eve => eve.ValidationErrors)
                        .Select(ve => ve.ErrorMessage);

                    oR.Message = string.Join(" | ", errores);
                }
                else
                {
                    oR.Message = ex.Message;
                }

                return oR;
            }
        }


        [HttpGet]
        [Authorize]
        [Route("api/v1/cuenta_detalle/encabezado/{encabezadoId}")]
        public Reply GetDetallesByEncabezado(int encabezadoId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (encabezadoId <= 0)
                    throw new Exception("invalid_value_for_encabezado_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled   = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var lista = (from d  in ctx.Cuenta_Detalle
                                 join ce in ctx.Cuenta_Encabezado on d.Cuenta_Encabezado_id equals ce.id
                                 join mp in ctx.Medio_pago on d.Medio_pago_id equals mp.id
                                 join u  in ctx.Usuarios on d.Usuarios_Usuario_id equals u.Usuario_id
                                 where d.Cuenta_Encabezado_id == encabezadoId
                                    && d.activo == 1
                                 orderby d.fecha_movimiento descending
                                 select new Models.CuentaDetalleViewModel
                                 {
                                     id                    = d.id,
                                     Cuenta_Encabezado_id  = d.Cuenta_Encabezado_id,
                                     Tipo_movimiento       = d.Tipo_movimiento,
                                     monto                 = d.monto,
                                     saldo_anterior        = d.saldo_anterior,
                                     saldo_posterior       = d.saldo_posterior,
                                     fecha_movimiento      = d.fecha_movimiento,
                                     referencia_pago       = d.referencia_pago,
                                     Observaciones         = d.Observaciones,
                                     activo                = d.activo,
                                     Usuarios_Usuario_id   = d.Usuarios_Usuario_id,
                                     Medio_pago_id         = d.Medio_pago_id,
                                     Referencia_encabezado = ce.Referencia,
                                     Medio_pago            = mp.descripcion,
                                     Usuario               = u.Nombre + " " + u.Apellido1,
                                     Tipo_movimiento_texto = d.Tipo_movimiento == 1 ? "Abono"
                                                           : d.Tipo_movimiento == 2 ? "Pago Total"
                                                           : d.Tipo_movimiento == 3 ? "Ajuste"
                                                           : d.Tipo_movimiento == 4 ? "Nota Crédito"
                                                           : "Otro"
                                 }).ToList();

                    // Calcular totales
                    decimal totalAbonos = lista.Sum(d => d.monto);

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = new
                    {
                        total_abonos    = totalAbonos,
                        total_registros = lista.Count,
                        detalles        = lista
                    };
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = ex.Message;
                return oR;
            }
        }


        [HttpGet]
        [Authorize]
        [Route("api/v1/cuenta_detalle/{id}")]
        public Reply GetCuentaDetalleById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0)
                    throw new Exception("invalid_value_for_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled   = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var d = (from det in ctx.Cuenta_Detalle
                             join mp  in ctx.Medio_pago on det.Medio_pago_id equals mp.id
                             join u   in ctx.Usuarios on det.Usuarios_Usuario_id equals u.Usuario_id
                             join ce  in ctx.Cuenta_Encabezado on det.Cuenta_Encabezado_id equals ce.id
                             where det.id == id
                             select new Models.CuentaDetalleViewModel
                             {
                                 id                    = det.id,
                                 Cuenta_Encabezado_id  = det.Cuenta_Encabezado_id,
                                 Tipo_movimiento       = det.Tipo_movimiento,
                                 monto                 = det.monto,
                                 saldo_anterior        = det.saldo_anterior,
                                 saldo_posterior       = det.saldo_posterior,
                                 fecha_movimiento      = det.fecha_movimiento,
                                 referencia_pago       = det.referencia_pago,
                                 Observaciones         = det.Observaciones,
                                 activo                = det.activo,
                                 Usuarios_Usuario_id   = det.Usuarios_Usuario_id,
                                 Medio_pago_id         = det.Medio_pago_id,
                                 Referencia_encabezado = ce.Referencia,
                                 Medio_pago            = mp.descripcion,
                                 Usuario               = u.Nombre + " " + u.Apellido1,
                                 Tipo_movimiento_texto = det.Tipo_movimiento == 1 ? "Abono"
                                                       : det.Tipo_movimiento == 2 ? "Pago Total"
                                                       : det.Tipo_movimiento == 3 ? "Ajuste"
                                                       : det.Tipo_movimiento == 4 ? "Nota Crédito"
                                                       : "Otro"
                             }).FirstOrDefault();

                    if (d == null)
                        throw new Exception("cuenta_detalle_not_found");

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = d;
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = ex.Message;
                return oR;
            }
        }


        [HttpDelete]
        [Authorize]
        [Route("api/v1/cuenta_detalle/{id}")]
        public Reply DeleteCuentaDetalle(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0)
                    throw new Exception("invalid_value_for_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuenta_Detalle d = ctx.Cuenta_Detalle.FirstOrDefault(u => u.id == id);
                    if (d == null)
                        throw new Exception("cuenta_detalle_not_found");

                    d.activo = 0;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = id;
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = ex.Message;
                return oR;
            }
        }
    }
}
