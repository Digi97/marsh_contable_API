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
   public class GastosController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/gastos")]
        public Reply CreateGasto([FromBody] Models.Gastos model)
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
                if (!tool.ValidaTexto(model.Descripcion))
                {
                    throw new Exception("invalid_string_form_Descripcion");
                }
                if (!tool.validaNumeros(model.Categoria_gasto_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Categoria_gasto_id");
                }
                if (!tool.validaNumeros(model.Tipo_documento_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Tipo_documento_id");
                }
                if (!tool.validaNumeros(model.Medio_pago_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Medio_pago_id");
                }
                if (!tool.validaNumeros(model.Proveedor_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Proveedor_id");
                }
                if (!tool.ValidaTexto(model.Doc_Referencia))
                {
                    throw new Exception("invalid_string_form_Doc_Referencia");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Gastos g = new Models.Gastos()
                    {
                        Descripcion = model.Descripcion,
                        Categoria_gasto_id = model.Categoria_gasto_id,
                        Subtotal = model.Subtotal,
                        Impuesto = model.Impuesto,
                        Total = model.Total,
                        Doc_Referencia = model.Doc_Referencia,
                        Fecha = DateTime.Now,
                        Ultima_Fec_Actualizacion = DateTime.Now,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Tipo_documento_id = model.Tipo_documento_id,
                        Medio_pago_id = model.Medio_pago_id,
                        Proveedor_id = model.Proveedor_id
                    };
                    ctx.Gastos.Add(g);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = g.id;
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
        [Route("api/v1/gastos/{id}")]
        public Reply UpdateGasto(int id, [FromBody] Models.Gastos model)
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
                if (!tool.ValidaTexto(model.Descripcion))
                {
                    throw new Exception("invalid_string_form_Descripcion");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Gastos g = ctx.Gastos.FirstOrDefault(u => u.id == id);
                    if (g == null)
                    {
                        throw new Exception("gasto_not_found");
                    }
                    g.Descripcion = model.Descripcion;
                    g.Categoria_gasto_id = model.Categoria_gasto_id;
                    g.Subtotal = model.Subtotal;
                    g.Impuesto = model.Impuesto;
                    g.Total = model.Total;
                    g.Doc_Referencia = model.Doc_Referencia;
                    g.Tipo_documento_id = model.Tipo_documento_id;
                    g.Medio_pago_id = model.Medio_pago_id;
                    g.Proveedor_id = model.Proveedor_id;
                    g.Ultima_Fec_Actualizacion = DateTime.Now;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = g.id;
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
        [Route("api/v1/gastos")]
        public Reply GetAllClientesPaged()
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
                    var query = ctx.Gastos.AsQueryable();

                    if (!string.IsNullOrEmpty(request.SearchValue))
                    {
                        string search = request.SearchValue.ToLower();
                        query = query.Where(x =>
                            x.Descripcion.ToLower().Contains(search) ||
                            x.Doc_Referencia.ToLower().Contains(search) ||
                            x.Total.ToString().Contains(search) ||
                            x.Proveedor.Nombre.ToLower().Contains(search)||
                            x.Proveedor.Apellido1.ToLower().Contains(search) ||
                            x.Proveedor.Apellido2.ToLower().Contains(search) ||
                            x.Categoria_gasto.Nombre.ToLower().Contains(search) ||
                            x.Usuarios.Nombre.ToLower().Contains(search) ||
                            x.Usuarios.Apellido1.ToLower().Contains(search) ||
                            x.Usuarios.Apellido2.ToLower().Contains(search)
                        );
                    }

                    int totalRecords = ctx.Gastos.Count();
                    int totalFiltered = query.Count();

                    switch (request.SortColumn?.ToLower())
                    {
                        case "descripcion":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Descripcion)
                                : query.OrderByDescending(x => x.Descripcion);
                            break;
                        case "total":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Total)
                                : query.OrderByDescending(x => x.Total);
                            break;
                        case "subtotal":   
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Subtotal)
                                : query.OrderByDescending(x => x.Subtotal);
                            break;
                        case "doc_referencia":  
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Doc_Referencia)
                                : query.OrderByDescending(x => x.Doc_Referencia);
                            break;
                        case "fecha":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Fecha)
                                : query.OrderByDescending(x => x.Fecha);
                            break;
                       
                        default:
                            query = query.OrderBy(x => x.id);
                            break;
                    }


                    var queryJoined = (from g in ctx.Gastos
                                 join cg in ctx.Categoria_gasto on g.Categoria_gasto_id equals cg.id
                                 join td in ctx.Tipo_documento on g.Tipo_documento_id equals td.id
                                 join mp in ctx.Medio_pago on g.Medio_pago_id equals mp.id
                                 join p in ctx.Proveedor on g.Proveedor_id equals p.id
                                 join u in ctx.Usuarios on g.Usuarios_Usuario_id equals u.Usuario_id
                                 select new Models.GastosViewModel
                                 {
                                     id = g.id,
                                     Descripcion = g.Descripcion,
                                     Categoria_gasto_id = g.Categoria_gasto_id,
                                     Subtotal = g.Subtotal,
                                     Impuesto = g.Impuesto,
                                     Total = g.Total,
                                     Doc_Referencia = g.Doc_Referencia,
                                     Fecha = g.Fecha,
                                     Ultima_Fec_Actualizacion = g.Ultima_Fec_Actualizacion,
                                     Usuarios_Usuario_id = g.Usuarios_Usuario_id,
                                     Tipo_documento_id = g.Tipo_documento_id,
                                     Medio_pago_id = g.Medio_pago_id,
                                     Proveedor_id = g.Proveedor_id,
                                     Categoria_gasto = cg.Nombre,
                                     Tipo_documento = td.Nombre,
                                     Medio_pago = mp.descripcion,
                                     Proveedor = p.Nombre + " " + p.Apellido1,
                                     Usuario = u.Nombre + " " + u.Apellido1
                                 }).ToList();
                   
                    var data = queryJoined
                        .Skip(request.Start)
                        .Take(request.Length > 0 ? request.Length : totalFiltered)
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
        //public Reply GetAllGastos()
        //{
        //    Reply oR = new Reply();
        //    oR.CodeStatus = 0;
        //    try
        //    {
        //        using (var ctx = new Models.EntitiesModel())
        //        {
        //            var lista = (from g in ctx.Gastos
        //                         join cg in ctx.Categoria_gasto on g.Categoria_gasto_id equals cg.id
        //                         join td in ctx.Tipo_documento on g.Tipo_documento_id equals td.id
        //                         join mp in ctx.Medio_pago on g.Medio_pago_id equals mp.id
        //                         join p in ctx.Proveedor on g.Proveedor_id equals p.id
        //                         join u in ctx.Usuarios on g.Usuarios_Usuario_id equals u.Usuario_id
        //                         select new Models.GastosViewModel
        //                         {
        //                             id = g.id,
        //                             Descripcion = g.Descripcion,
        //                             Categoria_gasto_id = g.Categoria_gasto_id,
        //                             Subtotal = g.Subtotal,
        //                             Impuesto = g.Impuesto,
        //                             Total = g.Total,
        //                             Doc_Referencia = g.Doc_Referencia,
        //                             Fecha = g.Fecha,
        //                             Ultima_Fec_Actualizacion = g.Ultima_Fec_Actualizacion,
        //                             Usuarios_Usuario_id = g.Usuarios_Usuario_id,
        //                             Tipo_documento_id = g.Tipo_documento_id,
        //                             Medio_pago_id = g.Medio_pago_id,
        //                             Proveedor_id = g.Proveedor_id,
        //                             Categoria_gasto = cg.Nombre,
        //                             Tipo_documento = td.Nombre,
        //                             Medio_pago = mp.descripcion,
        //                             Proveedor = p.Nombre + " " + p.Apellido1,
        //                             Usuario = u.Nombre + " " + u.Apellido1
        //                         }).ToList();

        //            oR.CodeStatus = HttpStatusCode.OK;
        //            oR.Data = lista;
        //            return oR;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        oR.CodeStatus = HttpStatusCode.InternalServerError;
        //        oR.Message = ex.Message;
        //        return oR;
        //    }
        //}


        [HttpGet]
        [Authorize]
        [Route("api/v1/gastos/{id}")]
        public Reply GetGastoById(int id)
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
                    var g = (from x in ctx.Gastos
                             join cg in ctx.Categoria_gasto on x.Categoria_gasto_id equals cg.id
                             join td in ctx.Tipo_documento on x.Tipo_documento_id equals td.id
                             join mp in ctx.Medio_pago on x.Medio_pago_id equals mp.id
                             join p in ctx.Proveedor on x.Proveedor_id equals p.id
                             join u in ctx.Usuarios on x.Usuarios_Usuario_id equals u.Usuario_id
                             where x.id == id
                             select new Models.GastosViewModel
                             {
                                 id = x.id,
                                 Descripcion = x.Descripcion,
                                 Categoria_gasto_id = x.Categoria_gasto_id,
                                 Subtotal = x.Subtotal,
                                 Impuesto = x.Impuesto,
                                 Total = x.Total,
                                 Doc_Referencia = x.Doc_Referencia,
                                 Fecha = x.Fecha,
                                 Ultima_Fec_Actualizacion = x.Ultima_Fec_Actualizacion,
                                 Usuarios_Usuario_id = x.Usuarios_Usuario_id,
                                 Tipo_documento_id = x.Tipo_documento_id,
                                 Medio_pago_id = x.Medio_pago_id,
                                 Proveedor_id = x.Proveedor_id,
                                 Categoria_gasto = cg.Nombre,
                                 Tipo_documento = td.Nombre,
                                 Medio_pago = mp.descripcion,
                                 Proveedor = p.Nombre + " " + p.Apellido1,
                                 Usuario = u.Nombre + " " + u.Apellido1
                             }).FirstOrDefault();

                    if (g == null)
                    {
                        throw new Exception("gasto_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = g;
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
        [Route("api/v1/gastos/proveedor/{proveedorId}")]
        public Reply GetGastosByProveedor(int proveedorId)
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
                    var lista = ctx.Gastos.Where(g => g.Proveedor_id == proveedorId)
                        .Select(g => new {
                            g.id,
                            g.Descripcion,
                            g.Total,
                            g.Fecha,
                            g.Doc_Referencia
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
    }
}
