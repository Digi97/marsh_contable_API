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
  
    public class GastosDetallesController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/gastos_detalles")]
        public Reply CreateGastoDetalle([FromBody] Models.Gastos_Detalles model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null)
                {
                    throw new Exception("invalid_model_request_missing");
                }
                if (!tool.validaNumeros(model.Gastos_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Gastos_id");
                }
                if (!tool.ValidaTexto(model.Detalle))
                {
                    throw new Exception("invalid_string_form_Detalle");
                }
                if (model.Cantidad <= 0)
                {
                    throw new Exception("invalid_value_form_Cantidad");
                }

                using (var ctx = new Models.EntitiesModel())
                {

                    
                    Models.Gastos_Detalles d = new Models.Gastos_Detalles()
                    {
                        Subtotal = model.Subtotal,
                        Impuesto = model.Impuesto,
                        Total = model.Total,
                        Cantidad = model.Cantidad,
                        Detalle = model.Detalle,
                        Descuento = model.Descuento,
                        codigo_comercial = model.codigo_comercial == null ? "01" : model.codigo_comercial,
                        Fecha = DateTime.Now,
                        Ultima_fec_actualizacion = DateTime.Now,
                        Gastos_id = model.Gastos_id
                    };
                    ctx.Gastos_Detalles.Add(d);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = d.id;
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
        [Route("api/v1/gastos_detalles/{id}")]
        public Reply UpdateGastoDetalle(int id, [FromBody] Models.Gastos_Detalles model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null)
                {
                    throw new Exception("invalid_model_request_missing");
                }
                if (!tool.ValidaTexto(model.Detalle))
                {
                    throw new Exception("invalid_string_form_Detalle");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Gastos_Detalles d = ctx.Gastos_Detalles.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("gasto_detalle_not_found");
                    }
                    d.Subtotal = model.Subtotal;
                    d.Impuesto = model.Impuesto;
                    d.Total = model.Total;
                    d.Cantidad = model.Cantidad;
                    d.Detalle = model.Detalle;
                    d.Descuento = model.Descuento;
                    d.codigo_comercial = model.codigo_comercial;
         
                    d.Ultima_fec_actualizacion = DateTime.Now;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = d.id;
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
        [Route("api/v1/gastos_detalles/gasto/{gastoId}")]
        public Reply GetDetallesByGasto(int gastoId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (gastoId <= 0)
                {
                    throw new Exception("invalid_value_for_gasto_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from d in ctx.Gastos_Detalles
                                 
                                 where d.Gastos_id == gastoId
                                 select new Models.GastosDetallesViewModel
                                 {
                                     id = d.id,
                                     Subtotal = d.Subtotal,
                                     Impuesto = d.Impuesto,
                                     Total = d.Total,
                                     Cantidad = d.Cantidad,
                                     Detalle = d.Detalle,
                                     Descuento = d.Descuento,
                                     codigo_comercial = d.codigo_comercial,
                                     Fecha = d.Fecha,
                                     Ultima_fec_actualizacion = d.Ultima_fec_actualizacion,
                                 
                                     Gastos_id = d.Gastos_id,
                                  
                                 }).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = lista;
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
        [Route("api/v1/gastos_detalles/{id}")]
        public Reply GetGastoDetalleById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0)
                {
                    throw new Exception("invalid_value_for_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var d = ctx.Gastos_Detalles.Where(x => x.id == id)
                        .Select(x => new {
                            x.id,
                            x.Subtotal,
                            x.Impuesto,
                            x.Total,
                            x.Cantidad,
                            x.Detalle,
                            x.Descuento,
                            x.codigo_comercial,
                            x.Fecha,
                            x.Ultima_fec_actualizacion,
                          
                            x.Gastos_id
                        }).FirstOrDefault();

                    if (d == null)
                    {
                        throw new Exception("gasto_detalle_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = d;
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


        [HttpDelete]
        [Authorize]
        [Route("api/v1/gastos_detalles/{id}")]
        public Reply DeleteGastoDetalle(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0)
                {
                    throw new Exception("invalid_value_for_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Gastos_Detalles d = ctx.Gastos_Detalles.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("gasto_detalle_not_found");
                    }
                    ctx.Gastos_Detalles.Remove(d);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = id;
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
