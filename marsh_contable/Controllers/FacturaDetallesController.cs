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

    public class FacturaDetallesController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/factura_detalles")]
        [RequierePermiso(PermisosAplica.UsuarioFacturacion)]
        public Reply CreateFacturaDetalle([FromBody] Models.Factura_Detalles model)
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
                if (!tool.validaNumeros(model.Facturas_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Facturas_id");
                }
                if (!tool.ValidaTexto(model.Detalle))
                {
                    throw new Exception("invalid_string_form_Detalle");
                }
                if (model.Cantidad <= 0)
                {
                    throw new Exception("invalid_value_form_Cantidad");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Factura_Detalles d = new Models.Factura_Detalles()
                    {
                        Facturas_id = model.Facturas_id,
                        Subtotal = model.Subtotal,
                        Impuesto = model.Impuesto,
                        Total = model.Total,
                        Cantidad = model.Cantidad,
                        Detalle = model.Detalle,
                        Codigos_cabys_id = model.Codigos_cabys_id,
                        Codigos_cabys_codigo = model.Codigos_cabys_codigo,
                        Codigos_cabys_Impuesto_id = model.Codigos_cabys_Impuesto_id,
                        Descuento = model.Descuento,
                        Unidad_medida_id = model.Unidad_medida_id,
                        Codigo_comercial_id = model.Codigo_comercial_id,
                        Fecha = DateTime.Now,
                        Ultima_fec_actualizacion = DateTime.Now,
                        Impuesto_id = model.Impuesto_id,
                    };
                    ctx.Factura_Detalles.Add(d);
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
        [Route("api/v1/factura_detalles/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioFacturacion)]
        public Reply UpdateFacturaDetalle(int id, [FromBody] Models.Factura_Detalles model)
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
                if (!tool.ValidaTexto(model.Detalle))
                {
                    throw new Exception("invalid_string_form_Detalle");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Factura_Detalles d = ctx.Factura_Detalles.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("factura_detalle_not_found");
                    }
                    d.Subtotal = model.Subtotal;
                    d.Impuesto = model.Impuesto;
                    d.Total = model.Total;
                    d.Cantidad = model.Cantidad;
                    d.Detalle = model.Detalle;
                    d.Codigos_cabys_id = model.Codigos_cabys_id;
                    d.Codigos_cabys_codigo = model.Codigos_cabys_codigo;
                    d.Codigos_cabys_Impuesto_id = model.Codigos_cabys_Impuesto_id;
                    d.Descuento = model.Descuento;
                    d.Unidad_medida_id = model.Unidad_medida_id;
                    d.Codigo_comercial_id = model.Codigo_comercial_id;
                    d.Ultima_fec_actualizacion = DateTime.Now;
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
        [Route("api/v1/factura_detalles/factura/{facturaId}")]
        public Reply GetDetallesByFactura(int facturaId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (facturaId <= 0)
                {
                    throw new Exception("invalid_value_for_factura_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from d in ctx.Factura_Detalles
                                 join um in ctx.Unidad_medida on d.Unidad_medida_id equals um.id
                                 where d.Facturas_id == facturaId
                                 select new Models.FacturaDetallesViewModel
                                 {
                                     id = d.id,
                                     Facturas_id = d.Facturas_id,
                                     Subtotal = d.Subtotal,
                                     Impuesto = d.Impuesto,
                                     Total = d.Total,
                                     Cantidad = d.Cantidad,
                                     Detalle = d.Detalle,
                                     Codigos_cabys_id = d.Codigos_cabys_id,
                                     Codigos_cabys_codigo = d.Codigos_cabys_codigo,
                                     Codigos_cabys_Impuesto_id = d.Codigos_cabys_Impuesto_id,
                                     Descuento = d.Descuento,
                                     Unidad_medida_id = d.Unidad_medida_id,
                                     Codigo_comercial_id = d.Codigo_comercial_id,
                                     Fecha = d.Fecha,
                                     Ultima_fec_actualizacion = d.Ultima_fec_actualizacion,
                                     Unidad_medida = um.Nombre
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
        [Route("api/v1/factura_detalles/{id}")]
        public Reply GetFacturaDetalleById(int id)
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
                    var d = ctx.Factura_Detalles.Where(x => x.id == id)
                        .Select(x => new {
                            x.id,
                            x.Facturas_id,
                            x.Subtotal,
                            x.Impuesto,
                            x.Total,
                            x.Cantidad,
                            x.Detalle,
                            x.Codigos_cabys_id,
                            x.Codigos_cabys_codigo,
                            x.Codigos_cabys_Impuesto_id,
                            x.Descuento,
                            x.Unidad_medida_id,
                            x.Codigo_comercial_id,
                            x.Fecha,
                            x.Ultima_fec_actualizacion
                        }).FirstOrDefault();

                    if (d == null)
                    {
                        throw new Exception("factura_detalle_not_found");
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
        [Route("api/v1/factura_detalles/{id}")]
        public Reply DeleteFacturaDetalle(int id)
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
                    Models.Factura_Detalles d = ctx.Factura_Detalles.FirstOrDefault(u => u.id == id);
                    if (d == null)
                    {
                        throw new Exception("factura_detalle_not_found");
                    }
                    ctx.Factura_Detalles.Remove(d);
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
