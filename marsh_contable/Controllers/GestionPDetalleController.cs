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

    public class GestionPDetalleController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/gestion_p_detalle")]
        public Reply CreateGestionPDetalle([FromBody] Models.Gestion_P_detalle model)
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
                if (!tool.validaNumeros(model.Gestion_Presupuestaria_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Gestion_Presupuestaria_id");
                }
                if (!tool.ValidaTexto(model.detalle_presupuesto))
                {
                    throw new Exception("invalid_string_form_detalle_presupuesto");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Gestion_P_detalle d = new Models.Gestion_P_detalle()
                    {
                        Monto = model.Monto,
                        Monto_aprobado = model.Monto_aprobado,
                        Monto_modificado = model.Monto_modificado,
                        Monto_compometido = model.Monto_compometido,
                        detalle_presupuesto = model.detalle_presupuesto,
                        Gestion_Presupuestaria_id = model.Gestion_Presupuestaria_id
                    };
                    ctx.Gestion_P_detalle.Add(d);
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
        [Route("api/v1/gestion_p_detalle/{id}")]
        public Reply UpdateGestionPDetalle(int id, [FromBody] Models.Gestion_P_detalle model)
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
                if (!tool.ValidaTexto(model.detalle_presupuesto))
                {
                    throw new Exception("invalid_string_form_detalle_presupuesto");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Gestion_P_detalle d = ctx.Gestion_P_detalle.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("gestion_p_detalle_not_found");
                    }
                    d.Monto = model.Monto;
                    d.Monto_aprobado = model.Monto_aprobado;
                    d.Monto_modificado = model.Monto_modificado;
                    d.Monto_compometido = model.Monto_compometido;
                    d.detalle_presupuesto = model.detalle_presupuesto;
                    d.Gestion_Presupuestaria_id = model.Gestion_Presupuestaria_id;
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
        [Route("api/v1/gestion_p_detalle/gestion/{gestionId}")]
        public Reply GetDetallesByGestion(int gestionId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (gestionId <= 0)
                {
                    throw new Exception("invalid_value_for_gestion_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from d in ctx.Gestion_P_detalle
                                 join gp in ctx.Gestion_Presupuestaria on d.Gestion_Presupuestaria_id equals gp.id
                                 where d.Gestion_Presupuestaria_id == gestionId
                                 select new Models.GestionPDetalleViewModel
                                 {
                                     id = d.id,
                                     Monto = d.Monto,
                                     Monto_aprobado = d.Monto_aprobado,
                                     Monto_modificado = d.Monto_modificado,
                                     Monto_compometido = d.Monto_compometido,
                                     detalle_presupuesto = d.detalle_presupuesto,
                                     Gestion_Presupuestaria_id = d.Gestion_Presupuestaria_id,
                                     Gestion_presupuestaria_nombre = gp.nombre
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
        [Route("api/v1/gestion_p_detalle/{id}")]
        public Reply GetGestionPDetalleById(int id)
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
                    var d = ctx.Gestion_P_detalle.Where(x => x.id == id)
                        .Select(x => new {
                            x.id,
                            x.Monto,
                            x.Monto_aprobado,
                            x.Monto_modificado,
                            x.Monto_compometido,
                            x.detalle_presupuesto,
                            x.Gestion_Presupuestaria_id
                        }).FirstOrDefault();

                    if (d == null)
                    {
                        throw new Exception("gestion_p_detalle_not_found");
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
        [Route("api/v1/gestion_p_detalle/{id}")]
        public Reply DeleteGestionPDetalle(int id)
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
                    Models.Gestion_P_detalle d = ctx.Gestion_P_detalle.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("gestion_p_detalle_not_found");
                    }
                    ctx.Gestion_P_detalle.Remove(d);
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
