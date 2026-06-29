using marsh_contable.Models;
using marsh_contable.Modulos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;

namespace marsh_contable.Controllers
{

    public class CatalogosController : ApiController
    {

        #region "Codigo actividad"



        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/codigo_actividad")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]

        public Reply CreateCodigoActividad([FromBody] Models.codigo_actividad model)
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
                if (!tool.validaNumeros(model.codigo_actividad1))
                {
                    throw new Exception("invalid_string_form_codigo_actividad");
                }
                if (!tool.ValidaTexto(model.nombre_actividad))
                {
                    throw new Exception("invalid_string_form_nombre_actividad");
                }


                var codigo_actividad = tool.FormatearCodigoActividad(model.codigo_actividad1);


                using (var ctx = new Models.EntitiesModel())
                {
                    Models.codigo_actividad ca = new Models.codigo_actividad()
                    {

                        codigo_actividad1 = codigo_actividad,
                        nombre_actividad = model.nombre_actividad,

                    };

                    ctx.codigo_actividad.Add(ca);
                    ctx.SaveChanges();


                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ca.id; // retorna el ID generado                 
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
        [Route("api/v1/catalogos/codigo_actividad/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateCodigoActividad(int id, [FromBody] Models.codigo_actividad model)
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
                if (!tool.validaNumeros(model.codigo_actividad1))
                {
                    throw new Exception("invalid_string_form_codigo_actividad");
                }
                if (!tool.ValidaTexto(model.nombre_actividad))
                {
                    throw new Exception("invalid_string_form_nombre_actividad");
                }


                using (var ctx = new Models.EntitiesModel())
                {
                    Models.codigo_actividad ca = ctx.codigo_actividad.FirstOrDefault(u => u.id == id);

                    if (ca == null)
                    {
                        throw new Exception("codigo_actividad_not_found");
                    }

                    ca.codigo_actividad1 = model.codigo_actividad1;
                    ca.nombre_actividad = model.nombre_actividad;


                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ca.id;

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
        [Route("api/v1/catalogos/codigo_actividad")]
        public Reply GetAllCodigo_actividad()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;

            try
            {
                using (var ctx = new Models.EntitiesModel())
                {

                    var ca = ctx.codigo_actividad
              .Select(x => new {
                  x.id,
                  x.codigo_actividad1,
                  x.nombre_actividad
              }).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ca;
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
        [Route("api/v1/catalogos/codigo_actividad/{id}")]
        public Reply GetCodigoActividadById(int id)
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

                    var ca = ctx.codigo_actividad
                       .Where(x => x.id == id)
                        .Select(x => new {
                            x.id,
                            x.codigo_actividad1,
                            x.nombre_actividad
                        }).ToList();


                    if (ca == null)
                    {
                        throw new Exception("codigo_actividad_not_found");
                    }

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ca;
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
        #endregion

        #region "Cuentas_Contables"

        //[HttpPost]
        //[Authorize]
        //[Route("api/v1/catalogos/cuentas_contables")]
        //[RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        //public Reply CreateCuentasContables([FromBody] Models.Cuentas_Contables model)
        //{
        //    Reply oR = new Reply();
        //    oR.CodeStatus = 0;
        //    General tool = new General();
        //    try
        //    {
        //        if (model == null) throw new Exception("invalid_model_request_missing");
        //        if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo");
        //        if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
        //        if (!tool.validaNumeros(model.Tipo_Cuenta_Contable_id.ToString())) throw new Exception("invalid_value_form_Tipo_Cuenta_Contable_id");
        //        if (!tool.validaNumeros(model.Usuarios_Usuario_id.ToString())) throw new Exception("invalid_value_form_Usuarios_Usuario_id");

        //        using (var ctx = new Models.EntitiesModel())
        //        {
        //            Models.Cuentas_Contables cc = new Models.Cuentas_Contables()
        //            {
        //                Codigo = model.Codigo,
        //                Nombre = model.Nombre,
        //                Tipo_Cuenta_Contable_id = model.Tipo_Cuenta_Contable_id,
        //                Usuarios_Usuario_id = model.Usuarios_Usuario_id,
        //                Estado = model.Estado,
        //                Saldo_inicial = model.Saldo_inicial,
        //                Saldo_actual = model.Saldo_actual,
        //                Fecha_Creacion = DateTime.Now,
        //                Fecha_actualizacion = DateTime.Now,
        //                Tipo_moneda_id = model.Tipo_moneda_id
        //            };
        //            ctx.Cuentas_Contables.Add(cc);
        //            ctx.SaveChanges();
        //            oR.CodeStatus = HttpStatusCode.OK;
        //            oR.Data = cc.id;
        //            return oR;
        //        }
        //    }
        //    catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
        //    {
        //        String errorDB = "";
        //        foreach (var eve in ex2.EntityValidationErrors)
        //            foreach (var ve in eve.ValidationErrors)
        //                errorDB += ve.ErrorMessage;
        //        oR.CodeStatus = HttpStatusCode.InternalServerError;
        //        oR.Message = errorDB;
        //        return oR;
        //    }
        //    catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        //}

        //[HttpPut]
        //[Authorize]
        //[Route("api/v1/catalogos/cuentas_contables/{id}")]
        //[RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        //public Reply UpdateCuentasContables(int id, [FromBody] Models.Cuentas_Contables model)
        //{
        //    Reply oR = new Reply();
        //    oR.CodeStatus = 0;
        //    General tool = new General();
        //    try
        //    {
        //        if (model == null) throw new Exception("invalid_model_request_missing");
        //        if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo");
        //        if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
        //        if (!tool.validaNumeros(model.Tipo_Cuenta_Contable_id.ToString())) throw new Exception("invalid_value_form_Tipo_Cuenta_Contable_id");

        //        using (var ctx = new Models.EntitiesModel())
        //        {
        //            Models.Cuentas_Contables cc = ctx.Cuentas_Contables.FirstOrDefault(x => x.id == id);
        //            if (cc == null) throw new Exception("cuentas_contables_not_found");

        //            cc.Codigo = model.Codigo;
        //            cc.Nombre = model.Nombre;
        //            cc.Tipo_Cuenta_Contable_id = model.Tipo_Cuenta_Contable_id;
        //            cc.Estado = model.Estado;
        //            cc.Saldo_inicial = model.Saldo_inicial;
        //            cc.Saldo_actual = model.Saldo_actual;
        //            cc.Fecha_actualizacion = DateTime.Now;
        //            cc.Tipo_moneda_id = model.Tipo_moneda_id;

        //            ctx.SaveChanges();
        //            oR.CodeStatus = HttpStatusCode.OK;
        //            oR.Data = cc.id;
        //            return oR;
        //        }
        //    }
        //    catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
        //    {
        //        String errorDB = "";
        //        foreach (var eve in ex2.EntityValidationErrors)
        //            foreach (var ve in eve.ValidationErrors)
        //                errorDB += ve.ErrorMessage;
        //        oR.CodeStatus = HttpStatusCode.InternalServerError;
        //        oR.Message = errorDB;
        //        return oR;
        //    }
        //    catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        //}

        //[HttpGet]
        //[Authorize]
        //[Route("api/v1/catalogos/cuentas_contables")]
        //public Reply GetAllCuentasContables()
        //{
        //    Reply oR = new Reply();
        //    oR.CodeStatus = 0;
        //    try
        //    {
        //        using (var ctx = new Models.EntitiesModel())
        //        {
        //            var data = ctx.Cuentas_Contables.Select(x => new {
        //                x.id,
        //                x.Codigo,
        //                x.Nombre,
        //                x.Tipo_Cuenta_Contable_id,
        //                NombreTipoCuenta = x.Tipo_Cuenta_Contable.Nombre,
        //                x.Usuarios_Usuario_id,
        //                x.Estado,
        //                x.Saldo_inicial,
        //                x.Saldo_actual,
        //                x.Fecha_Creacion,
        //                x.Fecha_actualizacion,
        //                x.Tipo_moneda_id,
        //                NombreTipoMoneda = x.Tipo_moneda.Nombre
        //            }).ToList();
        //            oR.CodeStatus = HttpStatusCode.OK;
        //            oR.Data = data;
        //            return oR;
        //        }
        //    }
        //    catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
        //    {
        //        String errorDB = "";
        //        foreach (var eve in ex2.EntityValidationErrors)
        //            foreach (var ve in eve.ValidationErrors)
        //                errorDB += ve.ErrorMessage;
        //        oR.CodeStatus = HttpStatusCode.InternalServerError;
        //        oR.Message = errorDB;
        //        return oR;
        //    }
        //    catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        //}

        //[HttpGet]
        //[Authorize]
        //[Route("api/v1/catalogos/cuentas_contables/{id}")]
        //public Reply GetCuentasContablesById(int id)
        //{
        //    Reply oR = new Reply();
        //    oR.CodeStatus = 0;
        //    try
        //    {
        //        if (id <= 0) throw new Exception("invalid_value_for_id");
        //        using (var ctx = new Models.EntitiesModel())
        //        {
        //            var data = ctx.Cuentas_Contables.Where(x => x.id == id).Select(x => new {
        //                x.id,
        //                x.Codigo,
        //                x.Nombre,
        //                x.Tipo_Cuenta_Contable_id,
        //                x.Usuarios_Usuario_id,
        //                x.Estado,
        //                x.Saldo_inicial,
        //                x.Saldo_actual,
        //                x.Fecha_Creacion,
        //                x.Fecha_actualizacion,
        //                x.Tipo_moneda_id
        //            }).FirstOrDefault();
        //            if (data == null) throw new Exception("cuentas_contables_not_found");
        //            oR.CodeStatus = HttpStatusCode.OK;
        //            oR.Data = data;
        //            return oR;
        //        }
        //    }
        //    catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
        //    {
        //        String errorDB = "";
        //        foreach (var eve in ex2.EntityValidationErrors)
        //            foreach (var ve in eve.ValidationErrors)
        //                errorDB += ve.ErrorMessage;
        //        oR.CodeStatus = HttpStatusCode.InternalServerError;
        //        oR.Message = errorDB;
        //        return oR;
        //    }
        //    catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        //}
        #endregion

        #region "Codigos_cabys"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/codigos_cabys")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateCodigosCabys([FromBody] Models.Codigos_cabys model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                if (!tool.validaNumeros(model.Impuesto_id.ToString())) throw new Exception("invalid_value_form_Impuesto_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Codigos_cabys cc = new Models.Codigos_cabys()
                    {
                        codigo = model.codigo,
                        Nombre = model.Nombre,
                        Impuesto_id = model.Impuesto_id
                    };
                    ctx.Codigos_cabys.Add(cc);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/codigos_cabys/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateCodigosCabys(int id, [FromBody] Models.Codigos_cabys model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                if (!tool.validaNumeros(model.Impuesto_id.ToString())) throw new Exception("invalid_value_form_Impuesto_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Codigos_cabys cc = ctx.Codigos_cabys.FirstOrDefault(x => x.id == id);
                    if (cc == null) throw new Exception("codigos_cabys_not_found");
                    cc.codigo = model.codigo;
                    cc.Nombre = model.Nombre;
                    cc.Impuesto_id = model.Impuesto_id;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
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
        [Route("api/v1/catalogos/codigos_cabys")]

        public Reply GetAllCodigosCabysPaged()
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
                    var query = ctx.Codigos_cabys.AsQueryable();

                    if (!string.IsNullOrEmpty(request.SearchValue))
                    {
                        string search = request.SearchValue.ToLower();
                        query = query.Where(x =>
                            x.codigo.ToLower().Contains(search) ||
                            x.Nombre.ToLower().Contains(search) ||
                            x.Impuesto.Nombre.ToLower().Contains(search)
                        );
                    }

                    int totalRecords = ctx.Codigos_cabys.Count();
                    int totalFiltered = query.Count();

                    switch (request.SortColumn?.ToLower())
                    {
                        case "codigo":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.codigo)
                                : query.OrderByDescending(x => x.codigo);
                            break;
                        case "nombre":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Nombre)
                                : query.OrderByDescending(x => x.Nombre);
                            break;
                        case "nombreimpuesto":   // ver nota abajo sobre el nombre
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Impuesto.Nombre)
                                : query.OrderByDescending(x => x.Impuesto.Nombre);
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
                            x.codigo,
                            x.Nombre,
                            x.Impuesto_id,
                            NombreImpuesto = x.Impuesto.Nombre
                        })
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
        //public Reply GetAllCodigosCabys()
        //{
        //    Reply oR = new Reply();
        //    oR.CodeStatus = 0;
        //    try
        //    {
        //        using (var ctx = new Models.EntitiesModel())
        //        {
        //            var data = ctx.Codigos_cabys.Select(x => new { x.id, x.codigo, x.Nombre, x.Impuesto_id, NombreImpuesto = x.Impuesto.Nombre }).ToList();
        //            oR.CodeStatus = HttpStatusCode.OK;
        //            oR.Data = data;
        //            return oR;
        //        }
        //    }
        //    catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
        //    {
        //        String errorDB = "";
        //        foreach (var eve in ex2.EntityValidationErrors)
        //            foreach (var ve in eve.ValidationErrors)
        //                errorDB += ve.ErrorMessage;
        //        oR.CodeStatus = HttpStatusCode.InternalServerError;
        //        oR.Message = errorDB;
        //        return oR;
        //    }
        //    catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        //}

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/codigos_cabys/{id}")]
        public Reply GetCodigosCabysById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Codigos_cabys.Where(x => x.id == id)
                        .Select(x => new { x.id, x.codigo, x.Nombre, x.Impuesto_id }).FirstOrDefault();
                    if (data == null) throw new Exception("codigos_cabys_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        #region "Codigo_comercial"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/codigo_comercial")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateCodigoComercial([FromBody] Models.Codigo_comercial model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Codigo_comercial cc = new Models.Codigo_comercial() { Codigo = model.Codigo, Nombre = model.Nombre };
                    ctx.Codigo_comercial.Add(cc);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/codigo_comercial/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateCodigoComercial(int id, [FromBody] Models.Codigo_comercial model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Codigo_comercial cc = ctx.Codigo_comercial.FirstOrDefault(x => x.id == id);
                    if (cc == null) throw new Exception("codigo_comercial_not_found");
                    cc.Codigo = model.Codigo;
                    cc.Nombre = model.Nombre;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
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
        [Route("api/v1/catalogos/codigo_comercial")]
        public Reply GetAllCodigoComercial()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Codigo_comercial.Select(x => new { x.id, x.Codigo, x.Nombre }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
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
        [Route("api/v1/catalogos/codigo_comercial/{id}")]
        public Reply GetCodigoComercialById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Codigo_comercial.Where(x => x.id == id)
                        .Select(x => new { x.id, x.Codigo, x.Nombre }).FirstOrDefault();
                    if (data == null) throw new Exception("codigo_comercial_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        #region "Centro_Costos"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/centro_costos")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateCentroCostos([FromBody] Models.Centro_Costos model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
               // if (!tool.ValidaTexto(model.Seudonimo)) throw new Exception("invalid_string_form_Seudonimo");
                if (!tool.validaNumeros(model.Monto_presupuesto_anual.ToString())) throw new Exception("invalid_format_monto_presupuesto_anual");


                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Centro_Costos cc = new Models.Centro_Costos()
                    {
                        codigo = model.codigo,
                        Nombre = model.Nombre,
                        Seudonimo = model.codigo,//seudonimo igual a codigo
                        Monto_presupuesto_anual = model.Monto_presupuesto_anual,
                        Tipo_moneda_id = model.Tipo_moneda_id
                    };
                    ctx.Centro_Costos.Add(cc);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/centro_costos/{id}")]
        public Reply UpdateCentroCostos(int id, [FromBody] Models.Centro_Costos model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
              //  if (!tool.ValidaTexto(model.Seudonimo)) throw new Exception("invalid_string_form_Seudonimo");
                if (!tool.validaNumeros(model.Monto_presupuesto_anual.ToString())) throw new Exception("invalid_format_monto_presupuesto_anual");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Centro_Costos cc = ctx.Centro_Costos.FirstOrDefault(x => x.id == id);
                    if (cc == null) throw new Exception("centro_costos_not_found");
                    cc.codigo = model.codigo;
                    cc.Nombre = model.Nombre;
                    cc.Seudonimo = model.codigo;
                    cc.Monto_presupuesto_anual = model.Monto_presupuesto_anual;
                    cc.Tipo_moneda_id = model.Tipo_moneda_id;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
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
        [Route("api/v1/catalogos/centro_costos")]
        public Reply GetAllCentroCostos()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Centro_Costos.Select(x => new { x.id, x.codigo, x.Nombre, x.Seudonimo, x.Monto_presupuesto_anual, x.Tipo_moneda_id }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
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
        [Route("api/v1/catalogos/centro_costos/{id}")]
        public Reply GetCentroCostosById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Centro_Costos.Where(x => x.id == id)
                        .Select(x => new { x.id, x.codigo, x.Nombre, x.Seudonimo, x.Monto_presupuesto_anual, x.Tipo_moneda_id }).FirstOrDefault();
                    if (data == null) throw new Exception("centro_costos_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }


        [HttpDelete]
        [Authorize]
        [Route("api/v1/catalogos/centro_costos/{id}")]
        public Reply DeleteCentroCostos(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0)
                    throw new Exception("invalid_value_for_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    var item = ctx.Centro_Costos.FirstOrDefault(x => x.id == id);

                    if (item == null)
                        throw new Exception("centro_costos_presupuestaria_not_found");

                    // Verificar si tiene registros asociados
                    //bool tieneDetalle = ctx.Gestion_P_detalle
                    //    .Any(d => d.Ce == id);

                    bool tieneGestion = ctx.Gestion_Presupuestaria
                        .Any(d => d.Centro_Costos_id == id);

                    if ( tieneGestion)
                        throw new Exception("centro_costos_tiene_registros_asociados_no_se_puede_eliminar");

                    ctx.Centro_Costos.Remove(item);
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


        #endregion

        #region "Tipo_moneda"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/tipo_moneda")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateTipoMoneda([FromBody] Models.Tipo_moneda model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo_moneda)) throw new Exception("invalid_string_form_codigo_moneda");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Tipo_moneda tm = new Models.Tipo_moneda() { codigo_moneda = model.codigo_moneda, Nombre = model.Nombre, Simbolo = model.Simbolo };
                    ctx.Tipo_moneda.Add(tm);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = tm.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/tipo_moneda/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateTipoMoneda(int id, [FromBody] Models.Tipo_moneda model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo_moneda)) throw new Exception("invalid_string_form_codigo_moneda");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Tipo_moneda tm = ctx.Tipo_moneda.FirstOrDefault(x => x.id == id);
                    if (tm == null) throw new Exception("tipo_moneda_not_found");
                    tm.codigo_moneda = model.codigo_moneda;
                    tm.Nombre = model.Nombre;
                    tm.Simbolo = model.Simbolo;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = tm.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
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
        [Route("api/v1/catalogos/tipo_moneda")]
        public Reply GetAllTipoMoneda()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Tipo_moneda.Select(x => new { x.id, x.codigo_moneda, x.Nombre, x.Simbolo }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
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
        [Route("api/v1/catalogos/tipo_moneda/{id}")]
        public Reply GetTipoMonedaById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Tipo_moneda.Where(x => x.id == id)
                        .Select(x => new { x.id, x.codigo_moneda, x.Nombre, x.Simbolo }).FirstOrDefault();
                    if (data == null) throw new Exception("tipo_moneda_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        #region "Tipo_documento"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/tipo_documento")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateTipoDocumento([FromBody] Models.Tipo_documento model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo_doc)) throw new Exception("invalid_string_form_Codigo_doc");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Tipo_documento td = new Models.Tipo_documento() { Codigo_doc = model.Codigo_doc, Nombre = model.Nombre };
                    ctx.Tipo_documento.Add(td);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = td.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/tipo_documento/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateTipoDocumento(int id, [FromBody] Models.Tipo_documento model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo_doc)) throw new Exception("invalid_string_form_Codigo_doc");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Tipo_documento td = ctx.Tipo_documento.FirstOrDefault(x => x.id == id);
                    if (td == null) throw new Exception("tipo_documento_not_found");
                    td.Codigo_doc = model.Codigo_doc;
                    td.Nombre = model.Nombre;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = td.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
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
        [Route("api/v1/catalogos/tipo_documento")]
        public Reply GetAllTipoDocumento()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Tipo_documento.Select(x => new { x.id, x.Codigo_doc, x.Nombre }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
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
        [Route("api/v1/catalogos/tipo_documento/{id}")]
        public Reply GetTipoDocumentoById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Tipo_documento.Where(x => x.id == id)
                        .Select(x => new { x.id, x.Codigo_doc, x.Nombre }).FirstOrDefault();
                    if (data == null) throw new Exception("tipo_documento_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        // Los siguientes catálogos (Categoria_gasto, Tipo_archivo, Estado_Factura, Unidad_medida,
        // Medio_pago, Permisos, Categoria_presupuestaria, Roles, Condicion_venta, Impuesto,
        // Tipo_Cuenta_Contable) siguen exactamente el mismo patrón. Se listan sus rutas y campos:

        #region "Categoria_gasto"
        // POST   api/v1/catalogos/categoria_gasto          → Nombre
        // PUT    api/v1/catalogos/categoria_gasto/{id}     → Nombre
        // GET    api/v1/catalogos/categoria_gasto          → id, Nombre
        // GET    api/v1/catalogos/categoria_gasto/{id}     → id, Nombre

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/categoria_gasto")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateCategoriaGasto([FromBody] Models.Categoria_gasto model)
        {
            Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Categoria_gasto e = new Models.Categoria_gasto() { Nombre = model.Nombre };
                    ctx.Categoria_gasto.Add(e); ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2) { String errorDB = ""; foreach (var eve in ex2.EntityValidationErrors) foreach (var ve in eve.ValidationErrors) errorDB += ve.ErrorMessage; oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = errorDB; return oR; }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/categoria_gasto/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateCategoriaGasto(int id, [FromBody] Models.Categoria_gasto model)
        {
            Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Categoria_gasto e = ctx.Categoria_gasto.FirstOrDefault(x => x.id == id);
                    if (e == null) throw new Exception("categoria_gasto_not_found");
                    e.Nombre = model.Nombre; ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2) { String errorDB = ""; foreach (var eve in ex2.EntityValidationErrors) foreach (var ve in eve.ValidationErrors) errorDB += ve.ErrorMessage; oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = errorDB; return oR; }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/categoria_gasto")]
        public Reply GetAllCategoriaGasto() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Categoria_gasto.Select(x => new { x.id, x.Nombre }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/categoria_gasto/{id}")]
        public Reply GetCategoriaGastoById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Categoria_gasto.Where(x => x.id == id).Select(x => new { x.id, x.Nombre }).FirstOrDefault(); if (data == null) throw new Exception("categoria_gasto_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Tipo_archivo"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/tipo_archivo")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateTipoArchivo([FromBody] Models.Tipo_archivo model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Tipo_archivo e = new Models.Tipo_archivo() { Nombre = model.Nombre }; ctx.Tipo_archivo.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/tipo_archivo/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateTipoArchivo(int id, [FromBody] Models.Tipo_archivo model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Tipo_archivo e = ctx.Tipo_archivo.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("tipo_archivo_not_found"); e.Nombre = model.Nombre; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_archivo")]
        public Reply GetAllTipoArchivo() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Tipo_archivo.Select(x => new { x.id, x.Nombre }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_archivo/{id}")]
        public Reply GetTipoArchivoById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Tipo_archivo.Where(x => x.id == id).Select(x => new { x.id, x.Nombre }).FirstOrDefault(); if (data == null) throw new Exception("tipo_archivo_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Estado_Factura"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/estado_factura")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateEstadoFactura([FromBody] Models.Estado_Factura model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Estado_Factura e = new Models.Estado_Factura() { Nombre = model.Nombre }; ctx.Estado_Factura.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/estado_factura/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateEstadoFactura(int id, [FromBody] Models.Estado_Factura model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Estado_Factura e = ctx.Estado_Factura.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("estado_factura_not_found"); e.Nombre = model.Nombre; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/estado_factura")]
        public Reply GetAllEstadoFactura() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Estado_Factura.Select(x => new { x.id, x.Nombre }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/estado_factura/{id}")]
        public Reply GetEstadoFacturaById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Estado_Factura.Where(x => x.id == id).Select(x => new { x.id, x.Nombre }).FirstOrDefault(); if (data == null) throw new Exception("estado_factura_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Unidad_medida"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/unidad_medida")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateUnidadMedida([FromBody] Models.Unidad_medida model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Unidad_medida e = new Models.Unidad_medida() { Codigo = model.Codigo, Nombre = model.Nombre }; ctx.Unidad_medida.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/unidad_medida/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateUnidadMedida(int id, [FromBody] Models.Unidad_medida model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Unidad_medida e = ctx.Unidad_medida.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("unidad_medida_not_found"); e.Codigo = model.Codigo; e.Nombre = model.Nombre; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/unidad_medida")]
        public Reply GetAllUnidadMedida() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Unidad_medida.Select(x => new { x.id, x.Codigo, x.Nombre }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/unidad_medida/{id}")]
        public Reply GetUnidadMedidaById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Unidad_medida.Where(x => x.id == id).Select(x => new { x.id, x.Codigo, x.Nombre }).FirstOrDefault(); if (data == null) throw new Exception("unidad_medida_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Medio_pago"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/medio_pago")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateMedioPago([FromBody] Models.Medio_pago model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo"); if (!tool.ValidaTexto(model.descripcion)) throw new Exception("invalid_string_form_descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Medio_pago e = new Models.Medio_pago() { codigo = model.codigo, descripcion = model.descripcion }; ctx.Medio_pago.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/medio_pago/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateMedioPago(int id, [FromBody] Models.Medio_pago model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo"); if (!tool.ValidaTexto(model.descripcion)) throw new Exception("invalid_string_form_descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Medio_pago e = ctx.Medio_pago.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("medio_pago_not_found"); e.codigo = model.codigo; e.descripcion = model.descripcion; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/medio_pago")]
        public Reply GetAllMedioPago() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Medio_pago.Select(x => new { x.id, x.codigo, x.descripcion }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/medio_pago/{id}")]
        public Reply GetMedioPagoById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Medio_pago.Where(x => x.id == id).Select(x => new { x.id, x.codigo, x.descripcion }).FirstOrDefault(); if (data == null) throw new Exception("medio_pago_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Permisos"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/permisos")]
            [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreatePermisos([FromBody] Models.Permisos model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Permisos e = new Models.Permisos() { Nombre = model.Nombre, Descripcion = model.Descripcion }; ctx.Permisos.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/permisos/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdatePermisos(int id, [FromBody] Models.Permisos model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Permisos e = ctx.Permisos.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("permisos_not_found"); e.Nombre = model.Nombre; e.Descripcion = model.Descripcion; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/permisos")]
        public Reply GetAllPermisos() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Permisos.Select(x => new { x.id, x.Nombre, x.Descripcion }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/permisos/{id}")]
        public Reply GetPermisosById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Permisos.Where(x => x.id == id).Select(x => new { x.id, x.Nombre, x.Descripcion }).FirstOrDefault(); if (data == null) throw new Exception("permisos_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Categoria_presupuestaria"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/categoria_presupuestaria")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateCategoriaPresupuestaria([FromBody] Models.Categoria_presupuestaria model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.nombre)) throw new Exception("invalid_string_form_nombre"); if (!tool.ValidaTexto(model.tipo_categoria)) throw new Exception("invalid_string_form_tipo_categoria");

                if (!tool.validaNumeros(model.Monto_presupuestado.ToString())) 
                    throw new Exception("invalid_monto_presupuestado");


                using (var ctx = new Models.EntitiesModel()) { Models.Categoria_presupuestaria e = new Models.Categoria_presupuestaria() { nombre = model.nombre, tipo_categoria = model.tipo_categoria, Monto_presupuestado = model.Monto_presupuestado, Tipo_moneda_id = model.Tipo_moneda_id }; ctx.Categoria_presupuestaria.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/categoria_presupuestaria/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateCategoriaPresupuestaria(int id, [FromBody] Models.Categoria_presupuestaria model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.nombre)) throw new Exception("invalid_string_form_nombre");


                if (!tool.validaNumeros(model.Monto_presupuestado.ToString()))
                    throw new Exception("invalid_monto_presupuestado");

                if (!tool.ValidaTexto(model.tipo_categoria)) throw new Exception("invalid_string_form_tipo_categoria"); using (var ctx = new Models.EntitiesModel()) { Models.Categoria_presupuestaria e = ctx.Categoria_presupuestaria.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("categoria_presupuestaria_not_found"); e.nombre = model.nombre; e.tipo_categoria = model.tipo_categoria; e.Monto_presupuestado = model.Monto_presupuestado; e.Tipo_moneda_id = model.Tipo_moneda_id; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/categoria_presupuestaria")]
        public Reply GetAllCategoriaPresupuestaria() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Categoria_presupuestaria.Select(x => new { x.id, x.nombre, x.tipo_categoria, x.Monto_presupuestado, x.Tipo_moneda_id }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/categoria_presupuestaria/{id}")]
        public Reply GetCategoriaPresupuestariaById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Categoria_presupuestaria.Where(x => x.id == id).Select(x => new { x.id, x.nombre, x.tipo_categoria, x.Tipo_moneda_id }).FirstOrDefault(); if (data == null) throw new Exception("categoria_presupuestaria_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }


        [HttpDelete]
        [Authorize]
        [Route("api/v1/catalogos/categoria_presupuestaria/{id}")]
        public Reply DeleteCategoriaPresupuestaria(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0)
                    throw new Exception("invalid_value_for_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    var item = ctx.Categoria_presupuestaria.FirstOrDefault(x => x.id == id);

                    if (item == null)
                        throw new Exception("categoria_presupuestaria_not_found");

                    // Verificar si tiene registros asociados
                    bool tieneDetalle = ctx.Gestion_P_detalle
                        .Any(d => d.Categoria_presupuestaria_id == id);

                    bool tieneGestion = ctx.Gestion_Presupuestaria
                        .Any(d => d.Categoria_presupuestaria_id == id);

                    if (tieneDetalle || tieneGestion)
                        throw new Exception("categoria_tiene_registros_asociados_no_se_puede_eliminar");

                    ctx.Categoria_presupuestaria.Remove(item);
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


        #endregion

        #region "Roles"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/roles")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateRoles([FromBody] Models.Roles model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Roles e = new Models.Roles() { Descripcion = model.Descripcion }; ctx.Roles.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/roles/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateRoles(int id, [FromBody] Models.RolesViewModel model) {
            Reply oR = new Reply(); 
            oR.CodeStatus = 0; 
            General tool = new General(); 
            try 
            { 
                if (model == null) throw new Exception("invalid_model_request_missing"); 
                if (!tool.ValidaTexto(model.descripcion)) throw new Exception("invalid_string_form_Descripcion"); 
                if(model.PermisosRol.Count ==0) throw new Exception("invalid_permission_are_required");

                using (var ctx = new Models.EntitiesModel()) 
                { 
                    Models.Roles e = ctx.Roles.FirstOrDefault(x => x.id == id); 
                    if (e == null) throw new Exception("roles_not_found"); 
                    e.Descripcion = model.descripcion; 
                    ctx.SaveChanges();

                   var updatePermiso= UpdatePermisosxRol(id, model.PermisosRol);

                    if(updatePermiso.CodeStatus != HttpStatusCode.OK)
                    {
                        throw new Exception(updatePermiso.Message);
                    }

                    oR.CodeStatus = HttpStatusCode.OK; 
                    oR.Data = e.id; 
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
        [Route("api/v1/catalogos/roles")]
        public Reply GetAllRoles() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Roles.Select(x => new { x.id, x.Descripcion }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/roles/{id}")]
        public Reply GetRolesById(int id) { 
            Reply oR = new Reply(); 
            oR.CodeStatus = 0;
            try { 
                if (id <= 0) throw new Exception("invalid_value_for_id"); 
                using (var ctx = new Models.EntitiesModel()) 
                {


                    var data = (from r in ctx.Roles
                               where r.id == id
                               select new Models.RolesViewModel
                               {
                                   id = r.id,
                                   descripcion = r.Descripcion
                          
                               }).FirstOrDefault();

                    if (data != null)
                    {
                        data.PermisosRol = (from pxr in ctx.Permisos_x_rol
                                            join p in ctx.Permisos on pxr.Permisos_id equals p.id
                                            where pxr.Roles_id == id
                                            select new Models.PermisosXRolViewModel
                                            {
                                                id = pxr.id,
                                                Permisos_id = pxr.Permisos_id,
                                                Roles_id = pxr.Roles_id,
                                                NombrePermiso = p.Nombre
                                            }).ToList();
                    }
                if (data == null) throw new Exception("roles_not_found"); 


                    
                    oR.CodeStatus = HttpStatusCode.OK; 
                    oR.Data = data; return oR; } } 
            catch (Exception ex) { 
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message; 
                return oR; 
            }
        }
        #endregion

        #region "Condicion_venta"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/condicion_venta")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateCondicionVenta([FromBody] Models.Condicion_venta model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Condicion_venta e = new Models.Condicion_venta() { Codigo = model.Codigo, Descripcion = model.Descripcion }; ctx.Condicion_venta.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/condicion_venta/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateCondicionVenta(int id, [FromBody] Models.Condicion_venta model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Condicion_venta e = ctx.Condicion_venta.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("condicion_venta_not_found"); e.Codigo = model.Codigo; e.Descripcion = model.Descripcion; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/condicion_venta")]
        public Reply GetAllCondicionVenta() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Condicion_venta.Select(x => new { x.id, x.Codigo, x.Descripcion }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/condicion_venta/{id}")]
        public Reply GetCondicionVentaById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Condicion_venta.Where(x => x.id == id).Select(x => new { x.id, x.Codigo, x.Descripcion }).FirstOrDefault(); if (data == null) throw new Exception("condicion_venta_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Impuesto"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/impuesto")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreateImpuesto([FromBody] Models.Impuesto model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.validaNumeros(model.Porcentaje.ToString())) throw new Exception("invalid_value_form_Porcentaje"); if (!tool.validaNumeros(model.codigo)) throw new Exception("invalid_string_form_codigo"); using (var ctx = new Models.EntitiesModel()) { Models.Impuesto e = new Models.Impuesto() { Nombre = model.Nombre, Porcentaje = model.Porcentaje, codigo = model.codigo, TarifaIVACodigo = model.TarifaIVACodigo, TarifaIVANombre = model.TarifaIVANombre }; ctx.Impuesto.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/impuesto/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateImpuesto(int id, [FromBody] Models.Impuesto model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.validaNumeros(model.Porcentaje.ToString())) throw new Exception("invalid_value_form_Porcentaje"); if (!tool.validaNumeros(model.codigo)) throw new Exception("invalid_string_form_codigo"); using (var ctx = new Models.EntitiesModel()) { Models.Impuesto e = ctx.Impuesto.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("impuesto_not_found"); e.Nombre = model.Nombre; e.Porcentaje = model.Porcentaje; e.codigo = model.codigo; e.TarifaIVACodigo = model.TarifaIVACodigo; e.TarifaIVANombre = model.TarifaIVANombre; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/impuesto")]
        public Reply GetAllImpuesto() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Impuesto.Select(x => new { x.id, x.Nombre, x.Porcentaje, x.codigo, x.TarifaIVACodigo, x.TarifaIVANombre }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/impuesto/{id}")]
        public Reply GetImpuestoById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Impuesto.Where(x => x.id == id).Select(x => new { x.id, x.Nombre, x.Porcentaje, x.codigo, x.TarifaIVACodigo, x.TarifaIVANombre }).FirstOrDefault(); if (data == null) throw new Exception("impuesto_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Tipo_Cuenta_Contable"
        ////[HttpPost]
        ////[Authorize]
        ////[Route("api/v1/catalogos/tipo_cuenta_contable")]
        ////[RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        ////public Reply CreateTipoCuentaContable([FromBody] Models.Tipo_Cuenta_Contable model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.ValidaTexto(model.Naturaleza)) throw new Exception("invalid_string_form_Naturaleza"); using (var ctx = new Models.EntitiesModel()) { Models.Tipo_Cuenta_Contable e = new Models.Tipo_Cuenta_Contable() { Nombre = model.Nombre, Naturaleza = model.Naturaleza }; ctx.Tipo_Cuenta_Contable.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        ////[HttpPut]
        ////[Authorize]
        ////[Route("api/v1/catalogos/tipo_cuenta_contable/{id}")]
        ////[RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        ////public Reply UpdateTipoCuentaContable(int id, [FromBody] Models.Tipo_Cuenta_Contable model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.ValidaTexto(model.Naturaleza)) throw new Exception("invalid_string_form_Naturaleza"); using (var ctx = new Models.EntitiesModel()) { Models.Tipo_Cuenta_Contable e = ctx.Tipo_Cuenta_Contable.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("tipo_cuenta_contable_not_found"); e.Nombre = model.Nombre; e.Naturaleza = model.Naturaleza; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        ////[HttpGet]
        ////[Authorize]
        ////[Route("api/v1/catalogos/tipo_cuenta_contable")]
        ////public Reply GetAllTipoCuentaContable() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Tipo_Cuenta_Contable.Select(x => new { x.id, x.Nombre, x.Naturaleza }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        ////[HttpGet]
        ////[Authorize]
        ////[Route("api/v1/catalogos/tipo_cuenta_contable/{id}")]
        ////public Reply GetTipoCuentaContableById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Tipo_Cuenta_Contable.Where(x => x.id == id).Select(x => new { x.id, x.Nombre, x.Naturaleza }).FirstOrDefault(); if (data == null) throw new Exception("tipo_cuenta_contable_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion


        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/padron/{cedula}")]
        public Reply GetPadronByCedula(string cedula)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;

            try
            {
                if (cedula.ToString() == string.Empty)
                {
                    throw new Exception("invalid_value_for_cedula");
                }

                using (var ctx = new Models.EntitiesModel())
                {

                    var ca = ctx.Padron
                       .Where(x => x.cedula == cedula)
                        .Select(x => new {
                            x.cedula,
                            x.nombre,
                            x.apellido1,
                            x.apellido2
                        }).FirstOrDefault();


                    if (ca == null)
                    {
                        throw new Exception("cedula_not_found");
                    }

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ca;
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




        #region "Permisos_x_rol"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/permisos_x_rol")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply CreatePermisosxRol([FromBody] Models.Permisos_x_rol model) { 
            Reply oR = new Reply(); 
            oR.CodeStatus = 0; 
           
            try { if (model == null) throw new Exception("invalid_model_request_missing");
                if (model.Permisos_id == 0)
                { throw new Exception("invalid_string_form_Descripcion"); }
                
                using (var ctx = new Models.EntitiesModel()) { 
                    Models.Permisos_x_rol e = new Models.Permisos_x_rol() { Permisos_id = model.Permisos_id, Roles_id = model.Roles_id }; 
                    ctx.Permisos_x_rol.Add(e); 
                    ctx.SaveChanges(); 
                    oR.CodeStatus = HttpStatusCode.OK; 
                    oR.Data = e.id; return oR; } } 
            catch (Exception ex) { 
                oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/permisos_x_rol/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdatePermisosxRol(int id, [FromBody] List<PermisosXRolViewModel> model) { 
            
            Reply oR = new Reply(); 
            oR.CodeStatus = 0; 
            General tool = new General(); 
            try { 
                if (model == null) throw new Exception("invalid_model_request_missing");
              
                using (var ctx = new Models.EntitiesModel()) 
                {
                    Models.Permisos_x_rol e = ctx.Permisos_x_rol.FirstOrDefault(x => x.id == id);
                    var deleteResp = DeletePermisosXRol(id);

                    if(deleteResp.CodeStatus != HttpStatusCode.OK) throw new Exception(deleteResp.Message);

                    foreach (var nuevoPermisos in model)
                    {

                        Models.Permisos_x_rol create = new Models.Permisos_x_rol() { Permisos_id = nuevoPermisos.Permisos_id, Roles_id = nuevoPermisos.Roles_id };
                        ctx.Permisos_x_rol.Add(create);
                        ctx.SaveChanges();
                    }
                    oR.CodeStatus = HttpStatusCode.OK; 
                    oR.Data = id; 
                    return oR; 
                }
            } 
            catch (Exception ex) { 
                oR.CodeStatus = HttpStatusCode.InternalServerError; 
                oR.Message = ex.Message; 
                return oR; 
            } 
        }


        public Reply DeletePermisosXRol(int id)//borramos los permisos por ID de rol para su recreacion completa
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
                    List<Models.Permisos_x_rol> permisos_x_rol = ctx.Permisos_x_rol
                     .Where(u => u.Roles_id == id)
                     .ToList();

                    //if (!permisos_x_rol.Any())
                    //{
                    //    throw new Exception("permiso_x_rol_not_found");
                    //}

                    ctx.Permisos_x_rol.RemoveRange(permisos_x_rol);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = id;
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
        [Route("api/v1/catalogos/permisos_x_rol")]
        public Reply GetAllPermisosxRol()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = (from pxr in ctx.Permisos_x_rol
                                join p in ctx.Permisos on pxr.Permisos_id equals p.id
                                join r in ctx.Roles on pxr.Roles_id equals r.id
                                select new
                                {
                                    pxr.id,
                                    pxr.Permisos_id,
                                    NombrePermiso = p.Nombre,
                                    DescripcionPermiso = p.Descripcion,
                                    pxr.Roles_id,
                                    NombreRol = r.Descripcion
                                }).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
        [Route("api/v1/catalogos/permisos_x_rol/{id}")]
        public Reply GetPermisosxRolByRolId(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0)
                {
                    throw new Exception("invalid_value_for_rolId");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    var data = (from pxr in ctx.Permisos_x_rol
                                join p in ctx.Permisos on pxr.Permisos_id equals p.id
                                join r in ctx.Roles on pxr.Roles_id equals r.id
                                where pxr.Roles_id == id
                                select new
                                {
                                    pxr.id,
                                    pxr.Permisos_id,
                                    NombrePermiso = p.Nombre,
                                    DescripcionPermiso = p.Descripcion,
                                    pxr.Roles_id,
                                    NombreRol = r.Descripcion
                                }).ToList();

                    if (!data.Any())
                    {
                        throw new Exception("permisos_not_found_for_rol");
                    }

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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