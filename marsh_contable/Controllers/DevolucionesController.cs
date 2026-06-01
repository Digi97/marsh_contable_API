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

    public class DevolucionesController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/devoluciones")]
        public Reply CreateDevolucion([FromBody] Models.Devoluciones model)
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
                if (!tool.ValidaTexto(model.Motivo))
                {
                    throw new Exception("invalid_string_form_Motivo");
                }
                if (!tool.validaNumeros(model.Ingresos_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Ingresos_id");
                }
                if (model.Monto <= 0)
                {
                    throw new Exception("invalid_value_form_Monto");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Devoluciones d = new Models.Devoluciones()
                    {
                        Motivo = model.Motivo,
                        Monto = model.Monto,
                        Ingresos_id = model.Ingresos_id
                    };
                    ctx.Devoluciones.Add(d);
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
        [Route("api/v1/devoluciones/{id}")]
        public Reply UpdateDevolucion(int id, [FromBody] Models.Devoluciones model)
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
                if (!tool.ValidaTexto(model.Motivo))
                {
                    throw new Exception("invalid_string_form_Motivo");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Devoluciones d = ctx.Devoluciones.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("devolucion_not_found");
                    }
                    d.Motivo = model.Motivo;
                    d.Monto = model.Monto;
                    d.Ingresos_id = model.Ingresos_id;
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
        [Route("api/v1/devoluciones")]
        public Reply GetAllDevoluciones()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from d in ctx.Devoluciones
                                 join i in ctx.Ingresos on d.Ingresos_id equals i.id
                                 select new Models.DevolucionesViewModel
                                 {
                                     id = d.id,
                                     Motivo = d.Motivo,
                                     Monto = d.Monto,
                                     Ingresos_id = d.Ingresos_id,
                                     Ingreso_codigo = i.Codigo
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
        [Route("api/v1/devoluciones/{id}")]
        public Reply GetDevolucionById(int id)
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
                    var d = (from x in ctx.Devoluciones
                             join i in ctx.Ingresos on x.Ingresos_id equals i.id
                             where x.id == id
                             select new Models.DevolucionesViewModel
                             {
                                 id = x.id,
                                 Motivo = x.Motivo,
                                 Monto = x.Monto,
                                 Ingresos_id = x.Ingresos_id,
                                 Ingreso_codigo = i.Codigo
                             }).FirstOrDefault();

                    if (d == null)
                    {
                        throw new Exception("devolucion_not_found");
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


        [HttpGet]
        [Authorize]
        [Route("api/v1/devoluciones/ingreso/{ingresoId}")]
        public Reply GetDevolucionesByIngreso(int ingresoId)
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
                    var lista = ctx.Devoluciones.Where(d => d.Ingresos_id == ingresoId)
                        .Select(d => new {
                            d.id,
                            d.Motivo,
                            d.Monto,
                            d.Ingresos_id
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


        [HttpDelete]
        [Authorize]
        [Route("api/v1/devoluciones/{id}")]
        public Reply DeleteDevolucion(int id)
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
                    Models.Devoluciones d = ctx.Devoluciones.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("devolucion_not_found");
                    }
                    ctx.Devoluciones.Remove(d);
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
