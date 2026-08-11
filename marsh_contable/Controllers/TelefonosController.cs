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
 
    public class TelefonosController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/telefonos")]
        public Reply CreateTelefono([FromBody] Models.Telefonos model)
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
                if (!tool.validaNumeros(model.Numero))
                {
                    throw new Exception("invalid_string_form_Numero");
                }
                if (!tool.ValidaTexto(model.codigo_pais))
                {
                    throw new Exception("invalid_string_form_codigo_pais");
                }
                if (model.Clientes_id == null && model.Proveedor_id == null)
                {
                    throw new Exception("invalid_value_form_owner_required");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Telefonos t = new Models.Telefonos()
                    {
                        Numero = model.Numero,
                        codigo_pais = model.codigo_pais,
                        Clientes_id = model.Clientes_id,
                        Proveedor_id = model.Proveedor_id,
                        telefono_principal = (Int16)model.telefono_principal
                    };
                    ctx.Telefonos.Add(t);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = t.id;
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
        [Route("api/v1/telefonos/{id}")]
        public Reply UpdateTelefono(int id, [FromBody] Models.Telefonos model)
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
                if (!tool.validaNumeros(model.Numero))
                {
                    throw new Exception("invalid_string_form_Numero");
                }
                if (!tool.ValidaTexto(model.codigo_pais))
                {
                    throw new Exception("invalid_string_form_codigo_pais");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Telefonos t = ctx.Telefonos.FirstOrDefault(u => u.id == id);
                    if (t == null)
                    {
                        throw new Exception("telefono_not_found");
                    }
                    t.Numero = model.Numero;
                    t.codigo_pais = model.codigo_pais;
                    t.Clientes_id = model.Clientes_id;
                    t.Proveedor_id = model.Proveedor_id;
                    t.telefono_principal = (Int16)model.telefono_principal;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = t.id;
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
        [Route("api/v1/telefonos")]
        public Reply GetAllTelefonos()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = ctx.Telefonos.Select(t => new {
                        t.id,
                        t.Numero,
                        t.codigo_pais,
                        t.Clientes_id,
                        t.Proveedor_id,
                        t.telefono_principal
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
        [Route("api/v1/telefonos/{id}")]
        public Reply GetTelefonoById(int id)
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
                    var t = ctx.Telefonos.Where(x => x.id == id)
                        .Select(x => new {
                            x.id,
                            x.Numero,
                            x.codigo_pais,
                            x.Clientes_id,
                            x.Proveedor_id,
                            x.telefono_principal
                        }).FirstOrDefault();

                    if (t == null)
                    {
                        throw new Exception("telefono_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = t;
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
        [Route("api/v1/telefonos/cliente/{clienteId}")]
        public Reply GetTelefonosByCliente(int clienteId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (clienteId <= 0)
                {
                    throw new Exception("invalid_value_for_cliente_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = ctx.Telefonos.Where(t => t.Clientes_id == clienteId)
                        .Select(t => new {
                            t.id,
                            t.Numero,
                            t.codigo_pais,
                            t.telefono_principal
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
        [Route("api/v1/telefonos/proveedor/{proveedorId}")]
        public Reply GetTelefonosByProveedor(int proveedorId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (proveedorId <= 0)
                {
                    throw new Exception("invalid_value_for_proveedor_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = ctx.Telefonos.Where(t => t.Proveedor_id == proveedorId)
                        .Select(t => new {
                            t.id,
                            t.Numero,
                            t.codigo_pais,
                            t.telefono_principal
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
        [Route("api/v1/telefonos/{id}")]
        public Reply DeleteTelefono(int id)//borramos los telefonos por ID de cliente para su recreacion completa
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
                    List<Models.Telefonos> telefonos = ctx.Telefonos
         .Where(u => u.Clientes_id == id)
         .ToList();

                    if (!telefonos.Any())
                    {
                        throw new Exception("telefonos_not_found");
                    }

                    ctx.Telefonos.RemoveRange(telefonos);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = id;
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

    }
}
