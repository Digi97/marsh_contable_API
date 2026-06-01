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

    public class TipoIdentificacionController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/tipo_identificacion")]
        public Reply CreateTipoIdentificacion([FromBody] Models.tipo_identificacion model)
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
                if (!tool.ValidaTexto(model.codigo_tipo_identificacion))
                {
                    throw new Exception("invalid_string_form_codigo_tipo_identificacion");
                }
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.tipo_identificacion ti = new Models.tipo_identificacion()
                    {
                        codigo_tipo_identificacion = model.codigo_tipo_identificacion,
                        Nombre = model.Nombre
                    };
                    ctx.tipo_identificacion.Add(ti);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ti.id;
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
        [Route("api/v1/tipo_identificacion/{id}")]
        public Reply UpdateTipoIdentificacion(int id, [FromBody] Models.tipo_identificacion model)
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
                if (!tool.ValidaTexto(model.codigo_tipo_identificacion))
                {
                    throw new Exception("invalid_string_form_codigo_tipo_identificacion");
                }
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.tipo_identificacion ti = ctx.tipo_identificacion.FirstOrDefault(u => u.id == id);
                    if (ti == null)
                    {
                        throw new Exception("tipo_identificacion_not_found");
                    }
                    ti.codigo_tipo_identificacion = model.codigo_tipo_identificacion;
                    ti.Nombre = model.Nombre;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ti.id;
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
        [Route("api/v1/tipo_identificacion")]
        public Reply GetAllTipoIdentificacion()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = ctx.tipo_identificacion.Select(x => new {
                        x.id,
                        x.codigo_tipo_identificacion,
                        x.Nombre
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
        [Route("api/v1/tipo_identificacion/{id}")]
        public Reply GetTipoIdentificacionById(int id)
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
                    var ti = ctx.tipo_identificacion.Where(x => x.id == id)
                        .Select(x => new {
                            x.id,
                            x.codigo_tipo_identificacion,
                            x.Nombre
                        }).FirstOrDefault();

                    if (ti == null)
                    {
                        throw new Exception("tipo_identificacion_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ti;
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
