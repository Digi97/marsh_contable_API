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
    public class UbicacionController : ApiController
    {

        #region "Provincia"

        [HttpPost]
        [Authorize]
        [Route("api/v1/ubicacion/provincia")]
        public Reply CreateProvincia([FromBody] Models.Provincia model)
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
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Provincia p = new Models.Provincia()
                    {
                        Nombre = model.Nombre
                    };
                    ctx.Provincia.Add(p);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = p.id;
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
        [Route("api/v1/ubicacion/provincia/{id}")]
        public Reply UpdateProvincia(int id, [FromBody] Models.Provincia model)
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
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Provincia p = ctx.Provincia.FirstOrDefault(u => u.id == id);
                    if (p == null)
                    {
                        throw new Exception("provincia_not_found");
                    }
                    p.Nombre = model.Nombre;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = p.id;
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
        [Route("api/v1/ubicacion/provincia")]
        public Reply GetAllProvincias()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = ctx.Provincia.Select(x => new {
                        x.id,
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
        [Route("api/v1/ubicacion/provincia/{id}")]
        public Reply GetProvinciaById(int id)
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
                    var p = ctx.Provincia.Where(x => x.id == id)
                        .Select(x => new { x.id, x.Nombre }).FirstOrDefault();
                    if (p == null)
                    {
                        throw new Exception("provincia_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = p;
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

        #endregion


        #region "Canton"

        [HttpPost]
        [Authorize]
        [Route("api/v1/ubicacion/canton")]
        public Reply CreateCanton([FromBody] Models.Canton model)
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
                if (!tool.ValidaTexto(model.codigo))
                {
                    throw new Exception("invalid_string_form_codigo");
                }
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }
                if (!tool.validaNumeros(model.Provincia_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Provincia_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Canton c = new Models.Canton()
                    {
                        codigo = model.codigo,
                        Nombre = model.Nombre,
                        Provincia_id = model.Provincia_id
                    };
                    ctx.Canton.Add(c);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = c.id;
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
        [Route("api/v1/ubicacion/canton/{id}")]
        public Reply UpdateCanton(int id, [FromBody] Models.Canton model)
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
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Canton c = ctx.Canton.FirstOrDefault(u => u.id == id);
                    if (c == null)
                    {
                        throw new Exception("canton_not_found");
                    }
                    c.codigo = model.codigo;
                    c.Nombre = model.Nombre;
                    c.Provincia_id = model.Provincia_id;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = c.id;
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
        [Route("api/v1/ubicacion/canton")]
        public Reply GetAllCantones()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from c in ctx.Canton
                                 join p in ctx.Provincia on c.Provincia_id equals p.id
                                 select new Models.CantonViewModel
                                 {
                                     id = c.id,
                                     codigo = c.codigo,
                                     Nombre = c.Nombre,
                                     Provincia_id = c.Provincia_id,
                                     Provincia = p.Nombre
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
        [Route("api/v1/ubicacion/canton/{id}")]
        public Reply GetCantonById(int id)
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
                    var c = (from x in ctx.Canton
                             join p in ctx.Provincia on x.Provincia_id equals p.id
                             where x.id == id
                             select new Models.CantonViewModel
                             {
                                 id = x.id,
                                 codigo = x.codigo,
                                 Nombre = x.Nombre,
                                 Provincia_id = x.Provincia_id,
                                 Provincia = p.Nombre
                             }).FirstOrDefault();

                    if (c == null)
                    {
                        throw new Exception("canton_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = c;
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


        // Cantones filtrados por provincia (uso típico en formularios)
        [HttpGet]
        [Authorize]
        [Route("api/v1/ubicacion/canton/provincia/{provinciaId}")]
        public Reply GetCantonesByProvincia(int provinciaId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (provinciaId <= 0)
                {
                    throw new Exception("invalid_value_for_provincia_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = ctx.Canton.Where(c => c.Provincia_id == provinciaId)
                        .Select(c => new
                        {
                            c.id,
                            c.codigo,
                            c.Nombre,
                            c.Provincia_id
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

        #endregion


        #region "Distrito"

        [HttpPost]
        [Authorize]
        [Route("api/v1/ubicacion/distrito")]
        public Reply CreateDistrito([FromBody] Models.Distrito model)
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
                if (!tool.ValidaTexto(model.codigo_canton))
                {
                    throw new Exception("invalid_string_form_codigo_canton");
                }
                if (!tool.ValidaTexto(model.codigo_distrito))
                {
                    throw new Exception("invalid_string_form_codigo_distrito");
                }
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }
                if (!tool.validaNumeros(model.Canton_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Canton_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Distrito d = new Models.Distrito()
                    {
                        codigo_canton = model.codigo_canton,
                        codigo_distrito = model.codigo_distrito,
                        Nombre = model.Nombre,
                        Canton_id = model.Canton_id,
                        Canton_Provincia_id = model.Canton_Provincia_id
                    };
                    ctx.Distrito.Add(d);
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
        [Route("api/v1/ubicacion/distrito/{id}")]
        public Reply UpdateDistrito(int id, [FromBody] Models.Distrito model)
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
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Distrito d = ctx.Distrito.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("distrito_not_found");
                    }
                    d.codigo_canton = model.codigo_canton;
                    d.codigo_distrito = model.codigo_distrito;
                    d.Nombre = model.Nombre;
                    d.Canton_id = model.Canton_id;
                    d.Canton_Provincia_id = model.Canton_Provincia_id;
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
        [Route("api/v1/ubicacion/distrito")]
        public Reply GetAllDistritos()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from d in ctx.Distrito
                                 join c in ctx.Canton on d.Canton_id equals c.id
                                 join p in ctx.Provincia on c.Provincia_id equals p.id
                                 select new Models.DistritoViewModel
                                 {
                                     id = d.id,
                                     codigo_canton = d.codigo_canton,
                                     codigo_distrito = d.codigo_distrito,
                                     Nombre = d.Nombre,
                                     Canton_id = d.Canton_id,
                                     Canton_Provincia_id = d.Canton_Provincia_id,
                                     Canton = c.Nombre,
                                     Provincia = p.Nombre
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
        [Route("api/v1/ubicacion/distrito/{id}")]
        public Reply GetDistritoById(int id)
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
                    var d = (from x in ctx.Distrito
                             join c in ctx.Canton on x.Canton_id equals c.id
                             join p in ctx.Provincia on c.Provincia_id equals p.id
                             where x.id == id
                             select new Models.DistritoViewModel
                             {
                                 id = x.id,
                                 codigo_canton = x.codigo_canton,
                                 codigo_distrito = x.codigo_distrito,
                                 Nombre = x.Nombre,
                                 Canton_id = x.Canton_id,
                                 Canton_Provincia_id = x.Canton_Provincia_id,
                                 Canton = c.Nombre,
                                 Provincia = p.Nombre
                             }).FirstOrDefault();

                    if (d == null)
                    {
                        throw new Exception("distrito_not_found");
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


        // Distritos filtrados por cantón
        [HttpGet]
        [Authorize]
        [Route("api/v1/ubicacion/distrito/canton/{cantonId}")]
        public Reply GetDistritosByCanton(int cantonId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (cantonId <= 0)
                {
                    throw new Exception("invalid_value_for_canton_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = ctx.Distrito.Where(x => x.Canton_id == cantonId)
                        .Select(x => new {
                            x.id,
                            x.codigo_canton,
                            x.codigo_distrito,
                            x.Nombre,
                            x.Canton_id
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

        #endregion

    }
}
