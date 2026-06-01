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

    public class AdjuntosController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/adjuntos")]
        public Reply CreateAdjunto([FromBody] Models.Adjuntos model)
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
                if (!tool.ValidaTexto(model.Nombre_Archivo))
                {
                    throw new Exception("invalid_string_form_Nombre_Archivo");
                }
                if (!tool.ValidaTexto(model.Ruta_Archivo))
                {
                    throw new Exception("invalid_string_form_Ruta_Archivo");
                }
                if (!tool.validaNumeros(model.Tipo_archivo_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Tipo_archivo_id");
                }
                if (!tool.validaNumeros(model.Tablas_referencia_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Tablas_referencia_id");
                }
                if (!tool.validaNumeros(model.referencia.ToString()))
                {
                    throw new Exception("invalid_value_form_referencia");
                }
                if (!tool.ValidaTexto(model.extension))
                {
                    throw new Exception("invalid_string_form_extension");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Adjuntos a = new Models.Adjuntos()
                    {
                        Nombre_Archivo = model.Nombre_Archivo,
                        Ruta_Archivo = model.Ruta_Archivo,
                        estado = (Int16)model.estado,
                        Tipo_archivo_id = model.Tipo_archivo_id,
                        Tamano = model.Tamano,
                        Descripcion = model.Descripcion,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        extension = model.extension,
                        referencia = model.referencia,
                        Tablas_referencia_id = model.Tablas_referencia_id,
                        fecha_ingreso = DateTime.Now,
                        fecha_actualizacion = DateTime.Now
                    };
                    ctx.Adjuntos.Add(a);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = a.id;
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
        [Route("api/v1/adjuntos/{id}")]
        public Reply UpdateAdjunto(int id, [FromBody] Models.Adjuntos model)
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
                if (!tool.ValidaTexto(model.Nombre_Archivo))
                {
                    throw new Exception("invalid_string_form_Nombre_Archivo");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Adjuntos a = ctx.Adjuntos.FirstOrDefault(u => u.id == id);
                    if (a == null)
                    {
                        throw new Exception("adjunto_not_found");
                    }
                    a.Nombre_Archivo = model.Nombre_Archivo;
                    a.Ruta_Archivo = model.Ruta_Archivo;
                    a.estado = (Int16)model.estado;
                    a.Tipo_archivo_id = model.Tipo_archivo_id;
                    a.Tamano = model.Tamano;
                    a.Descripcion = model.Descripcion;
                    a.extension = model.extension;
                    a.referencia = model.referencia;
                    a.Tablas_referencia_id = model.Tablas_referencia_id;
                    a.fecha_actualizacion = DateTime.Now;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = a.id;
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
        [Route("api/v1/adjuntos")]
        public Reply GetAllAdjuntos()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from a in ctx.Adjuntos
                                 join ta in ctx.Tipo_archivo on a.Tipo_archivo_id equals ta.id
                                 join tr in ctx.Tablas_referencia on a.Tablas_referencia_id equals tr.id
                                 join u in ctx.Usuarios on a.Usuarios_Usuario_id equals u.Usuario_id
                                 select new Models.AdjuntosViewModel
                                 {
                                     id = a.id,
                                     Nombre_Archivo = a.Nombre_Archivo,
                                     Ruta_Archivo = a.Ruta_Archivo,
                                     estado = a.estado,
                                     Tipo_archivo_id = a.Tipo_archivo_id,
                                     Tamano = a.Tamano,
                                     Descripcion = a.Descripcion,
                                     Usuarios_Usuario_id = a.Usuarios_Usuario_id,
                                     extension = a.extension,
                                     referencia = a.referencia,
                                     Tablas_referencia_id = a.Tablas_referencia_id,
                                     fecha_ingreso = a.fecha_ingreso,
                                     fecha_actualizacion = a.fecha_actualizacion,
                                     Tipo_archivo = ta.Nombre,
                                     Tabla_referencia = tr.Nombre_tabla,
                                     Usuario = u.Nombre + " " + u.Apellido1
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
        [Route("api/v1/adjuntos/{id}")]
        public Reply GetAdjuntoById(int id)
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
                    var a = (from x in ctx.Adjuntos
                             join ta in ctx.Tipo_archivo on x.Tipo_archivo_id equals ta.id
                             join tr in ctx.Tablas_referencia on x.Tablas_referencia_id equals tr.id
                             join u in ctx.Usuarios on x.Usuarios_Usuario_id equals u.Usuario_id
                             where x.id == id
                             select new Models.AdjuntosViewModel
                             {
                                 id = x.id,
                                 Nombre_Archivo = x.Nombre_Archivo,
                                 Ruta_Archivo = x.Ruta_Archivo,
                                 estado = x.estado,
                                 Tipo_archivo_id = x.Tipo_archivo_id,
                                 Tamano = x.Tamano,
                                 Descripcion = x.Descripcion,
                                 Usuarios_Usuario_id = x.Usuarios_Usuario_id,
                                 extension = x.extension,
                                 referencia = x.referencia,
                                 Tablas_referencia_id = x.Tablas_referencia_id,
                                 fecha_ingreso = x.fecha_ingreso,
                                 fecha_actualizacion = x.fecha_actualizacion,
                                 Tipo_archivo = ta.Nombre,
                                 Tabla_referencia = tr.Nombre_tabla,
                                 Usuario = u.Nombre + " " + u.Apellido1
                             }).FirstOrDefault();

                    if (a == null)
                    {
                        throw new Exception("adjunto_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = a;
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


        // Adjuntos por tabla y registro de referencia (caso común)
        [HttpGet]
        [Authorize]
        [Route("api/v1/adjuntos/referencia/{tablaId}/{referencia}")]
        public Reply GetAdjuntosByReferencia(int tablaId, int referencia)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (tablaId <= 0 || referencia <= 0)
                {
                    throw new Exception("invalid_value_for_parameters");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = ctx.Adjuntos
                        .Where(a => a.Tablas_referencia_id == tablaId && a.referencia == referencia && a.estado == 1)
                        .Select(a => new {
                            a.id,
                            a.Nombre_Archivo,
                            a.Ruta_Archivo,
                            a.extension,
                            a.Tamano,
                            a.Descripcion,
                            a.fecha_ingreso
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
        [Route("api/v1/adjuntos/{id}")]
        public Reply DeleteAdjunto(int id)
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
                    Models.Adjuntos a = ctx.Adjuntos.FirstOrDefault(u => u.id == id);
                    if (a == null)
                    {
                        throw new Exception("adjunto_not_found");
                    }
                    // Borrado lógico
                    a.estado = 0;
                    a.fecha_actualizacion = DateTime.Now;
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
