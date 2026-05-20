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
                if (model == null)
                {
                    throw new Exception("invalid_model_request_missing");
                }
                if (!tool.validaNumeros(model.Cuenta_Encabezado_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Cuenta_Encabezado_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    // Nota: campo Fecha_creacion definido como Double en el EDMX (se respeta tal cual)
                    Models.Cuenta_Detalle d = new Models.Cuenta_Detalle()
                    {
                        Total = model.Total,
                        Monto_Proyeccion = model.Monto_Proyeccion,
                        Fecha_creacion = model.Fecha_creacion,
                        Estado = (Int16)model.Estado,
                        Impuesto = model.Impuesto,
                        Subtotal = model.Subtotal,
                        Cuenta_Encabezado_id = model.Cuenta_Encabezado_id
                    };
                    ctx.Cuenta_Detalle.Add(d);
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
        [Route("api/v1/cuenta_detalle/{id}")]
        public Reply UpdateCuentaDetalle(int id, [FromBody] Models.Cuenta_Detalle model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (model == null)
                {
                    throw new Exception("invalid_model_request_missing");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuenta_Detalle d = ctx.Cuenta_Detalle.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("cuenta_detalle_not_found");
                    }
                    d.Total = model.Total;
                    d.Monto_Proyeccion = model.Monto_Proyeccion;
                    d.Fecha_creacion = model.Fecha_creacion;
                    d.Estado = (Int16)model.Estado;
                    d.Impuesto = model.Impuesto;
                    d.Subtotal = model.Subtotal;
                    d.Cuenta_Encabezado_id = model.Cuenta_Encabezado_id;
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
        [Route("api/v1/cuenta_detalle/encabezado/{encabezadoId}")]
        public Reply GetDetallesByEncabezado(int encabezadoId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (encabezadoId <= 0)
                {
                    throw new Exception("invalid_value_for_encabezado_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from d in ctx.Cuenta_Detalle
                                 join ce in ctx.Cuenta_Encabezado on d.Cuenta_Encabezado_id equals ce.id
                                 where d.Cuenta_Encabezado_id == encabezadoId
                                 select new Models.CuentaDetalleViewModel
                                 {
                                     id = d.id,
                                     Total = d.Total,
                                     Monto_Proyeccion = d.Monto_Proyeccion,
                                     Fecha_creacion = d.Fecha_creacion,
                                     Estado = d.Estado,
                                     Impuesto = d.Impuesto,
                                     Subtotal = d.Subtotal,
                                     Cuenta_Encabezado_id = d.Cuenta_Encabezado_id,
                                     Referencia_encabezado = ce.Referencia
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
        [Route("api/v1/cuenta_detalle/{id}")]
        public Reply GetCuentaDetalleById(int id)
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
                    var d = ctx.Cuenta_Detalle.Where(x => x.id == id)
                        .Select(x => new {
                            x.id,
                            x.Total,
                            x.Monto_Proyeccion,
                            x.Fecha_creacion,
                            x.Estado,
                            x.Impuesto,
                            x.Subtotal,
                            x.Cuenta_Encabezado_id
                        }).FirstOrDefault();

                    if (d == null)
                    {
                        throw new Exception("cuenta_detalle_not_found");
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
        [Route("api/v1/cuenta_detalle/{id}")]
        public Reply DeleteCuentaDetalle(int id)
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
                    Models.Cuenta_Detalle d = ctx.Cuenta_Detalle.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("cuenta_detalle_not_found");
                    }
                    // Borrado lógico
                    d.Estado = 0;
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
