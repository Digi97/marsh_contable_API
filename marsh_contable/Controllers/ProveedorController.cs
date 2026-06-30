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

    public class ProveedorController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/proveedor")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimientoProveedores)]
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

                if(String.IsNullOrEmpty(model.CtaIBAN_Colones) == false) //si la cta iban viene con valor se valida
                {
                    if (!tool.ValidaIBAN(model.CtaIBAN_Colones))
                    {
                        throw new Exception("invalid_value_for_cta_iban_colones");
                    }
                }

                if (String.IsNullOrEmpty(model.CtaIBAN_Dolares) == false) //si la cta iban viene con valor se valida
                {
                    if (!tool.ValidaIBAN(model.CtaIBAN_Dolares))
                    {
                        throw new Exception("invalid_value_for_cta_iban_dolares");
                    }
                }

                if (model.Telefonos.Count == 0)
                {
                    throw new Exception("a_phone_is_required");
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
                        , CtaIBAN_Colones = model.CtaIBAN_Colones,
                        CtaIBAN_Dolares = model.CtaIBAN_Dolares
                    };

                    ctx.Proveedor.Add(nuevo);
                    ctx.SaveChanges();

                    TelefonosController Telefonos = new TelefonosController();
                    foreach (var telefono in model.Telefonos)
                    {
                        telefono.Proveedor_id = nuevo.id;
                        var result = Telefonos.CreateTelefono(telefono);

                    }

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
        [RequierePermiso(PermisosAplica.UsuarioMantenimientoProveedores)]
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

                if (model.Telefonos.Count == 0)
                {
                    throw new Exception("a_phone_is_required");
                }

                if (String.IsNullOrEmpty(model.CtaIBAN_Colones) == false) //si la cta iban viene con valor se valida
                {
                    if (!tool.ValidaIBAN(model.CtaIBAN_Colones))
                    {
                        throw new Exception("invalid_value_for_cta_iban_colones");
                    }
                }

                if (String.IsNullOrEmpty(model.CtaIBAN_Dolares) == false) //si la cta iban viene con valor se valida
                {
                    if (!tool.ValidaIBAN(model.CtaIBAN_Dolares))
                    {
                        throw new Exception("invalid_value_for_cta_iban_dolares");
                    }
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
                    p.CtaIBAN_Colones = model.CtaIBAN_Colones;
                    p.CtaIBAN_Dolares = model.CtaIBAN_Dolares;

                    ctx.SaveChanges();


                    TelefonosController Telefonos = new TelefonosController();
                    Telefonos.DeleteTelefono(p.id); //eliminamos todos los telefonos de ese cliente


                    foreach (var telefono in model.Telefonos)
                    {
                        telefono.Clientes_id = p.id; //creamos los telefonos nuevamente
                        var result = Telefonos.CreateTelefono(telefono);
                    }

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
        [Route("api/v1/proveedor_dp")]
        public Reply GetAllProveedorforDropDown()
        {
            Reply oR = new Reply();
            oR.CodeStatus = HttpStatusCode.OK;
            try
            {


                using (var ctx = new Models.EntitiesModel())
                {

                    var proveedores = (from p in ctx.Proveedor

                                    where p.estado == 1 //solo activos
                                    select new
                                    {
                                        p.id,
                                        Nombre = p.Nombre,
                                        Apellido1 = p.Apellido1
                                    }).OrderBy(x => x.Nombre).ToList();

                    oR.Data = proveedores;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                string errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }


        [HttpGet]
        [Authorize]
        [Route("api/v1/proveedor")]
        public Reply GetAllProveedorPaged()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                // Leer query string crudo
                var q = System.Web.HttpContext.Current.Request.QueryString;

                var request = new Models.DataTableRequest
                {
                    Draw = int.TryParse(q["draw"], out var d) ? d : 1,
                    Start = int.TryParse(q["start"], out var s) ? s : 0,
                    Length = int.TryParse(q["length"], out var l) ? l : 25,
                    SearchValue = q["search[value]"],
                    SortDirection = q["order[0][dir]"]
                };

                // El índice de la columna ordenada -> nombre real de la columna
                if (int.TryParse(q["order[0][column]"], out var colIdx))
                {
                    // columns[colIdx][data] trae el nombre que mandó el front (id, codigo, nombre...)
                    request.SortColumn = q[$"columns[{colIdx}][data]"];
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    var query = ctx.Proveedor.AsQueryable();

                    if (!string.IsNullOrEmpty(request.SearchValue))
                    {
                        string search = request.SearchValue.ToLower();
                        query = query.Where(x =>
                            x.Nombre.ToLower().Contains(search) ||
                            x.Apellido1.ToLower().Contains(search) ||
                            x.Apellido2.ToLower().Contains(search) ||
                            x.correo.ToLower().Contains(search)
                        );
                    }

                    int totalRecords = ctx.Clientes.Count();
                    int totalFiltered = query.Count();

                    switch (request.SortColumn?.ToLower())
                    {
                        case "nombre":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Nombre)
                                : query.OrderByDescending(x => x.Nombre);
                            break;
                        case "apellido1":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Apellido1)
                                : query.OrderByDescending(x => x.Apellido1);
                            break;
                        case "apellido2":   // ver nota abajo sobre el nombre
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Apellido2)
                                : query.OrderByDescending(x => x.Apellido2);
                            break;
                        default:
                            query = query.OrderBy(x => x.id);
                            break;
                    }

                    var data = query
                        .Skip(request.Start)
                        .Take(request.Length > 0 ? request.Length : totalFiltered)
                        .Select(x => new {
                            x.id,
                            x.Nombre,
                            x.Apellido1,
                            x.Apellido2,
                            tipo_identificacion = x.tipo_identificacion.Nombre,
                            x.identificacion,
                            x.correo,
                            x.estado
                        }).OrderByDescending(x => x.id)
                        .ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        draw = request.Draw,
                        recordsTotal = totalRecords,
                        recordsFiltered = totalFiltered,
                        data = data
                    };
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                string errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
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
                                 Nombre_actividad = ca.nombre_actividad,
                                 CtaIBAN_Dolares = x.CtaIBAN_Dolares,
                                 CtaIBAN_Colones = x.CtaIBAN_Colones
                             }).FirstOrDefault();

                    if (p == null)
                    {
                        throw new Exception("proveedor_not_found");
                    }


                    if (p != null)
                    {
                        p.Telefonos = ctx.Telefonos
                            .Where(t => t.Clientes_id == id)
                            .Select(t => new Models.TelefonosViewModel
                            {
                                id = t.id,
                                Numero = t.Numero,
                                codigo_pais = t.codigo_pais,
                                telefono_principal = t.telefono_principal,
                                Clientes_id = t.Clientes_id
                            }).ToList();
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
        [RequierePermiso(PermisosAplica.UsuarioMantenimientoProveedores)]
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
