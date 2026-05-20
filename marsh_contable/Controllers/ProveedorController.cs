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
    public class ProveedorController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/proveedor")]
        public Reply CreateProveedor([FromBody] Models.Proveedor model)
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
                if (!tool.ValidaTexto(model.identificacion))
                {
                    throw new Exception("invalid_string_form_identificacion");
                }
                if (!tool.validaNumeros(model.tipo_identificacion_id.ToString()))
                {
                    throw new Exception("invalid_value_form_tipo_identificacion_id");
                }
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }
                if (!tool.ValidaTexto(model.Apellido1))
                {
                    throw new Exception("invalid_string_form_Apellido1");
                }
                if (!tool.ValidaTexto(model.Apellido2))
                {
                    throw new Exception("invalid_string_form_Apellido2");
                }
                if (!tool.ValidaCorreo(model.correo))
                {
                    throw new Exception("invalid_string_form_correo");
                }
                if (!tool.validaNumeros(model.Provincia_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Provincia_id");
                }
                if (!tool.validaNumeros(model.Canton_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Canton_id");
                }
                if (!tool.validaNumeros(model.Distrito_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Distrito_id");
                }
                if (!tool.validaNumeros(model.codigo_actividad_id.ToString()))
                {
                    throw new Exception("invalid_value_form_codigo_actividad_id");
                }
                // fin de validaciones

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Proveedor nuevo = new Models.Proveedor()
                    {
                        identificacion = model.identificacion,
                        tipo_identificacion_id = model.tipo_identificacion_id,
                        Nombre = model.Nombre,
                        Apellido1 = model.Apellido1,
                        Apellido2 = model.Apellido2,
                        correo = model.correo,
                        Distrito_id = model.Distrito_id,
                        Canton_id = model.Canton_id,
                        Provincia_id = model.Provincia_id,
                        codigo_actividad_id = model.codigo_actividad_id,
                        estado = (Int16)model.estado,
                        exonerado = (Int16)model.exonerado,
                        OtrasSenas = model.OtrasSenas,
                        fecha_creacion = DateTime.Now,
                        fecha_actualizacion = DateTime.Now
                    };

                    ctx.Proveedor.Add(nuevo);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = nuevo.id;
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
        [Route("api/v1/proveedor/{id}")]
        public Reply UpdateProveedor(int id, [FromBody] Models.Proveedor model)
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
                if (!tool.ValidaTexto(model.identificacion))
                {
                    throw new Exception("invalid_string_form_identificacion");
                }
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }
                if (!tool.ValidaTexto(model.Apellido1))
                {
                    throw new Exception("invalid_string_form_Apellido1");
                }
                if (!tool.ValidaTexto(model.Apellido2))
                {
                    throw new Exception("invalid_string_form_Apellido2");
                }
                if (!tool.ValidaCorreo(model.correo))
                {
                    throw new Exception("invalid_string_form_correo");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Proveedor p = ctx.Proveedor.FirstOrDefault(u => u.id == id);

                    if (p == null)
                    {
                        throw new Exception("proveedor_not_found");
                    }

                    p.identificacion = model.identificacion;
                    p.tipo_identificacion_id = model.tipo_identificacion_id;
                    p.Nombre = model.Nombre;
                    p.Apellido1 = model.Apellido1;
                    p.Apellido2 = model.Apellido2;
                    p.correo = model.correo;
                    p.Distrito_id = model.Distrito_id;
                    p.Canton_id = model.Canton_id;
                    p.Provincia_id = model.Provincia_id;
                    p.codigo_actividad_id = model.codigo_actividad_id;
                    p.estado = (Int16)model.estado;
                    p.exonerado = (Int16)model.exonerado;
                    p.OtrasSenas = model.OtrasSenas;
                    p.fecha_actualizacion = DateTime.Now;

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
        [Route("api/v1/proveedor")]
        public Reply GetAllProveedores()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;

            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from p in ctx.Proveedor
                                 join ti in ctx.tipo_identificacion on p.tipo_identificacion_id equals ti.id
                                 join pr in ctx.Provincia on p.Provincia_id equals pr.id
                                 join ca in ctx.codigo_actividad on p.codigo_actividad_id equals ca.id
                                 select new Models.ProveedorViewModel
                                 {
                                     id = p.id,
                                     identificacion = p.identificacion,
                                     tipo_identificacion_id = p.tipo_identificacion_id,
                                     Nombre = p.Nombre,
                                     Apellido1 = p.Apellido1,
                                     Apellido2 = p.Apellido2,
                                     correo = p.correo,
                                     Distrito_id = p.Distrito_id,
                                     Canton_id = p.Canton_id,
                                     Provincia_id = p.Provincia_id,
                                     codigo_actividad_id = p.codigo_actividad_id,
                                     estado = p.estado,
                                     exonerado = p.exonerado,
                                     OtrasSenas = p.OtrasSenas,
                                     fecha_creacion = p.fecha_creacion,
                                     fecha_actualizacion = p.fecha_actualizacion,
                                     Tipo_identificacion = ti.Nombre,
                                     Provincia = pr.Nombre,
                                     Codigo_actividad = ca.codigo_actividad1,
                                     Nombre_actividad = ca.nombre_actividad
                                 }).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = lista;
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
        [Route("api/v1/proveedor/{id}")]
        public Reply GetProveedorById(int id)
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
                    var p = (from x in ctx.Proveedor
                             join ti in ctx.tipo_identificacion on x.tipo_identificacion_id equals ti.id
                             join pr in ctx.Provincia on x.Provincia_id equals pr.id
                             join ca in ctx.codigo_actividad on x.codigo_actividad_id equals ca.id
                             where x.id == id
                             select new Models.ProveedorViewModel
                             {
                                 id = x.id,
                                 identificacion = x.identificacion,
                                 tipo_identificacion_id = x.tipo_identificacion_id,
                                 Nombre = x.Nombre,
                                 Apellido1 = x.Apellido1,
                                 Apellido2 = x.Apellido2,
                                 correo = x.correo,
                                 Distrito_id = x.Distrito_id,
                                 Canton_id = x.Canton_id,
                                 Provincia_id = x.Provincia_id,
                                 codigo_actividad_id = x.codigo_actividad_id,
                                 estado = x.estado,
                                 exonerado = x.exonerado,
                                 OtrasSenas = x.OtrasSenas,
                                 fecha_creacion = x.fecha_creacion,
                                 fecha_actualizacion = x.fecha_actualizacion,
                                 Tipo_identificacion = ti.Nombre,
                                 Provincia = pr.Nombre,
                                 Codigo_actividad = ca.codigo_actividad1,
                                 Nombre_actividad = ca.nombre_actividad
                             }).FirstOrDefault();

                    if (p == null)
                    {
                        throw new Exception("proveedor_not_found");
                    }

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = p;
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
            }
            return oR;
        }


        [HttpDelete]
        [Authorize]
        [Route("api/v1/proveedor/{id}")]
        public Reply DeleteProveedor(int id)
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
                    Models.Proveedor p = ctx.Proveedor.FirstOrDefault(u => u.id == id);

                    if (p == null)
                    {
                        throw new Exception("proveedor_not_found");
                    }

                    p.estado = 0;
                    p.fecha_actualizacion = DateTime.Now;
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
    }
}
