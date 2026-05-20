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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class IngresosDetalleController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/ingresos_detalle")]
        public Reply CreateIngresoDetalle([FromBody] Models.Ingresos_Detalle model)
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
                if (!tool.validaNumeros(model.Ingresos_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Ingresos_id");
                }
                if (!tool.ValidaTexto(model.Detalle))
                {
                    throw new Exception("invalid_string_form_Detalle");
                }
                if (model.Cantidad <= 0)
                {
                    throw new Exception("invalid_value_form_Cantidad");
                }
                if (!tool.validaNumeros(model.Unidad_medida_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Unidad_medida_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Ingresos_Detalle d = new Models.Ingresos_Detalle()
                    {
                        Subtotal = model.Subtotal,
                        Impuesto = model.Impuesto,
                        Total = model.Total,
                        Cantidad = model.Cantidad,
                        Detalle = model.Detalle,
                        Descuento = model.Descuento,
                        codigo_comercial = model.codigo_comercial,
                        Unidad_medida_id = model.Unidad_medida_id,
                        Fecha = DateTime.Now,
                        Ultima_fec_actualizacion = DateTime.Now,
                        Ingresos_id = model.Ingresos_id
                    };
                    ctx.Ingresos_Detalle.Add(d);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = d.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }


        [HttpPut]
        [Authorize]
        [Route("api/v1/ingresos_detalle/{id}")]
        public Reply UpdateIngresoDetalle(int id, [FromBody] Models.Ingresos_Detalle model)
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
                    Models.Ingresos_Detalle d = ctx.Ingresos_Detalle.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("ingreso_detalle_not_found");
                    }
                    d.Subtotal = model.Subtotal;
                    d.Impuesto = model.Impuesto;
                    d.Total = model.Total;
                    d.Cantidad = model.Cantidad;
                    d.Detalle = model.Detalle;
                    d.Descuento = model.Descuento;
                    d.codigo_comercial = model.codigo_comercial;
                    d.Unidad_medida_id = model.Unidad_medida_id;
                    d.Ultima_fec_actualizacion = DateTime.Now;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = d.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
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
        [Route("api/v1/ingresos_detalle/ingreso/{ingresoId}")]
        public Reply GetDetallesByIngreso(int ingresoId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (ingresoId <= 0)
                {
                    throw new Exception("invalid_value_for_ingreso_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from d in ctx.Ingresos_Detalle
                                 join um in ctx.Unidad_medida on d.Unidad_medida_id equals um.id
                                 where d.Ingresos_id == ingresoId
                                 select new Models.IngresosDetalleViewModel
                                 {
                                     id = d.id,
                                     Subtotal = d.Subtotal,
                                     Impuesto = d.Impuesto,
                                     Total = d.Total,
                                     Cantidad = d.Cantidad,
                                     Detalle = d.Detalle,
                                     Descuento = d.Descuento,
                                     codigo_comercial = d.codigo_comercial,
                                     Unidad_medida_id = d.Unidad_medida_id,
                                     Fecha = d.Fecha,
                                     Ultima_fec_actualizacion = d.Ultima_fec_actualizacion,
                                     Ingresos_id = d.Ingresos_id,
                                     Unidad_medida = um.Nombre
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
        [Route("api/v1/ingresos_detalle/{id}")]
        public Reply GetIngresoDetalleById(int id)
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
                    var d = ctx.Ingresos_Detalle.Where(x => x.id == id)
                        .Select(x => new {
                            x.id,
                            x.Subtotal,
                            x.Impuesto,
                            x.Total,
                            x.Cantidad,
                            x.Detalle,
                            x.Descuento,
                            x.codigo_comercial,
                            x.Unidad_medida_id,
                            x.Fecha,
                            x.Ultima_fec_actualizacion,
                            x.Ingresos_id
                        }).FirstOrDefault();

                    if (d == null)
                    {
                        throw new Exception("ingreso_detalle_not_found");
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
        [Route("api/v1/ingresos_detalle/{id}")]
        public Reply DeleteIngresoDetalle(int id)
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
                    Models.Ingresos_Detalle d = ctx.Ingresos_Detalle.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("ingreso_detalle_not_found");
                    }
                    ctx.Ingresos_Detalle.Remove(d);
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
