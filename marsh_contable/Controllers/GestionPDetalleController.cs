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
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
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
                        Gestion_Presupuestaria_id = model.Gestion_Presupuestaria_id,
                        Categoria_presupuestaria_id = model.Categoria_presupuestaria_id,
                        Gastos_id = model.Gastos_id,
                        Ingresos_id = model.Ingresos_id,
                        Facturas_id = model.Facturas_id,
                        Monto_ejecutado = model.Monto_ejecutado,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Fecha_registro = DateTime.Now,
                        Observaciones = "",
                        activo = 1
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
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply UpdateGestionPDetalle(int id, [FromBody] Models.Gestion_P_detalle model, int typeDoc = 0)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            Models.Gestion_P_detalle d;
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
                    
                    switch(typeDoc)
                    {
                        case 0:// Gastos_id
                            d = ctx.Gestion_P_detalle.FirstOrDefault(u => u.Gastos_id == id);
                           break;
                        case 1: //Ingresos
                            d = ctx.Gestion_P_detalle.FirstOrDefault(u => u.Ingresos_id == id);

                            break;

                        case 2: //Facturas
                            d = ctx.Gestion_P_detalle.FirstOrDefault(u => u.Facturas_id == id);
                        break;
                        default:
                            d = null;
                                break;
                    }

                   
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
                    d.Categoria_presupuestaria_id = model.Categoria_presupuestaria_id;
                    d.Gastos_id = model.Gastos_id;
                    d.Ingresos_id = model.Ingresos_id;
                    d.Facturas_id = model.Facturas_id;
                    d.Monto_ejecutado = model.Monto_ejecutado;
                    d.Usuarios_Usuario_id = model.Usuarios_Usuario_id;
                    d.Fecha_registro = model.Fecha_registro;
                    d.Observaciones = model.Observaciones;
                    d.activo = model.activo;
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
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply GetDetallesByGestion(string gestionId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (String.IsNullOrEmpty(gestionId))
                {
                    throw new Exception("invalid_value_for_gestion_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from d in ctx.Gestion_P_detalle
                                 join gp in ctx.Gestion_Presupuestaria on d.Gestion_Presupuestaria_id equals gp.id
                                 join cp in ctx.Categoria_presupuestaria on d.Categoria_presupuestaria_id equals cp.id
                                 join f in ctx.Facturas on d.Facturas_id equals f.id into facturaGroup
                                 from f in facturaGroup.DefaultIfEmpty()
                                 join g in ctx.Gastos on d.Gastos_id equals g.id into gastoGroup
                                 from g in gastoGroup.DefaultIfEmpty()
                                 join i in ctx.Ingresos on d.Ingresos_id equals i.id into ingresoGroup
                                 from i in ingresoGroup.DefaultIfEmpty()

                                 where gp.anio_presupuesto == gestionId //se filtra por ano
                                 select new Models.GestionPDetalleViewModel
                                 {
                                     id = d.id,
                                     Monto = d.Monto,
                                     Monto_aprobado = d.Monto_aprobado,
                                     Monto_modificado = d.Monto_modificado,
                                     Monto_compometido = d.Monto_compometido,
                                     detalle_presupuesto = d.detalle_presupuesto,
                                     Gestion_Presupuestaria_id = d.Gestion_Presupuestaria_id,
                                     Gestion_presupuestaria_nombre = gp.nombre,
                                     Gastos_id = d.Gastos_id,
                                     Facturas_id = d.Facturas_id,
                                     Ingresos_id = d.Ingresos_id,
                                     Monto_ejecutado = d.Monto_ejecutado,
                                     Fecha_registro = d.Fecha_registro,
                                     categoria_presupuestaria = cp.nombre,
                                     Observaciones = d.Observaciones

                                 }).OrderByDescending(x => x.id).ToList();

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
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
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
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
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
