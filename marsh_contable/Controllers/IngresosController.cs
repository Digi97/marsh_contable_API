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
    public class IngresosController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/ingresos")]
        public Reply CreateIngreso([FromBody] Models.Ingresos model)
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
                if (!tool.ValidaTexto(model.Codigo))
                {
                    throw new Exception("invalid_string_form_Codigo");
                }
                if (!tool.validaNumeros(model.Clientes_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Clientes_id");
                }
                if (!tool.validaNumeros(model.Tipo_moneda_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Tipo_moneda_id");
                }
                if (!tool.validaNumeros(model.Estado_Factura_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Estado_Factura_id");
                }
                if (!tool.validaNumeros(model.Medio_pago_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Medio_pago_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Ingresos i = new Models.Ingresos()
                    {
                        Codigo = model.Codigo,
                        fecha = DateTime.Now,
                        Tipo_moneda_id = model.Tipo_moneda_id,
                        Estado_Factura_id = model.Estado_Factura_id,
                        Subtotal = model.Subtotal,
                        Impuesto = model.Impuesto,
                        Total = model.Total,
                        Descuento = model.Descuento,
                        cambio_venta = model.cambio_venta,
                        cambio_compra = model.cambio_compra,
                        Clientes_id = model.Clientes_id,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Medio_pago_id = model.Medio_pago_id,
                        Facturas_id = model.Facturas_id
                    };
                    ctx.Ingresos.Add(i);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = i.id;
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
        [Route("api/v1/ingresos/{id}")]
        public Reply UpdateIngreso(int id, [FromBody] Models.Ingresos model)
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
                if (!tool.ValidaTexto(model.Codigo))
                {
                    throw new Exception("invalid_string_form_Codigo");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Ingresos i = ctx.Ingresos.FirstOrDefault(u => u.id == id);
                    if (i == null)
                    {
                        throw new Exception("ingreso_not_found");
                    }
                    i.Codigo = model.Codigo;
                    i.Tipo_moneda_id = model.Tipo_moneda_id;
                    i.Estado_Factura_id = model.Estado_Factura_id;
                    i.Subtotal = model.Subtotal;
                    i.Impuesto = model.Impuesto;
                    i.Total = model.Total;
                    i.Descuento = model.Descuento;
                    i.cambio_venta = model.cambio_venta;
                    i.cambio_compra = model.cambio_compra;
                    i.Clientes_id = model.Clientes_id;
                    i.Medio_pago_id = model.Medio_pago_id;
                    i.Facturas_id = model.Facturas_id;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = i.id;
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
        [Route("api/v1/ingresos")]
        public Reply GetAllIngresos()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from i in ctx.Ingresos
                                 join c in ctx.Clientes on i.Clientes_id equals c.id
                                 join tm in ctx.Tipo_moneda on i.Tipo_moneda_id equals tm.id
                                 join ef in ctx.Estado_Factura on i.Estado_Factura_id equals ef.id
                                 join mp in ctx.Medio_pago on i.Medio_pago_id equals mp.id
                                 join u in ctx.Usuarios on i.Usuarios_Usuario_id equals u.Usuario_id
                                 select new Models.IngresosViewModel
                                 {
                                     id = i.id,
                                     Codigo = i.Codigo,
                                     fecha = i.fecha,
                                     Tipo_moneda_id = i.Tipo_moneda_id,
                                     Estado_Factura_id = i.Estado_Factura_id,
                                     Subtotal = i.Subtotal,
                                     Impuesto = i.Impuesto,
                                     Total = i.Total,
                                     Descuento = i.Descuento,
                                     cambio_venta = i.cambio_venta,
                                     cambio_compra = i.cambio_compra,
                                     Clientes_id = i.Clientes_id,
                                     Usuarios_Usuario_id = i.Usuarios_Usuario_id,
                                     Medio_pago_id = i.Medio_pago_id,
                                     Facturas_id = i.Facturas_id,
                                     Cliente = c.Nombre + " " + c.Apellido1,
                                     Tipo_moneda = tm.Nombre,
                                     Estado_factura = ef.Nombre,
                                     Medio_pago = mp.descripcion,
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
        [Route("api/v1/ingresos/{id}")]
        public Reply GetIngresoById(int id)
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
                    var i = (from x in ctx.Ingresos
                             join c in ctx.Clientes on x.Clientes_id equals c.id
                             join tm in ctx.Tipo_moneda on x.Tipo_moneda_id equals tm.id
                             join ef in ctx.Estado_Factura on x.Estado_Factura_id equals ef.id
                             join mp in ctx.Medio_pago on x.Medio_pago_id equals mp.id
                             join u in ctx.Usuarios on x.Usuarios_Usuario_id equals u.Usuario_id
                             where x.id == id
                             select new Models.IngresosViewModel
                             {
                                 id = x.id,
                                 Codigo = x.Codigo,
                                 fecha = x.fecha,
                                 Tipo_moneda_id = x.Tipo_moneda_id,
                                 Estado_Factura_id = x.Estado_Factura_id,
                                 Subtotal = x.Subtotal,
                                 Impuesto = x.Impuesto,
                                 Total = x.Total,
                                 Descuento = x.Descuento,
                                 cambio_venta = x.cambio_venta,
                                 cambio_compra = x.cambio_compra,
                                 Clientes_id = x.Clientes_id,
                                 Usuarios_Usuario_id = x.Usuarios_Usuario_id,
                                 Medio_pago_id = x.Medio_pago_id,
                                 Facturas_id = x.Facturas_id,
                                 Cliente = c.Nombre + " " + c.Apellido1,
                                 Tipo_moneda = tm.Nombre,
                                 Estado_factura = ef.Nombre,
                                 Medio_pago = mp.descripcion,
                                 Usuario = u.Nombre + " " + u.Apellido1
                             }).FirstOrDefault();

                    if (i == null)
                    {
                        throw new Exception("ingreso_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = i;
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
        [Route("api/v1/ingresos/cliente/{clienteId}")]
        public Reply GetIngresosByCliente(int clienteId)
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
                    var lista = ctx.Ingresos.Where(i => i.Clientes_id == clienteId)
                        .Select(i => new {
                            i.id,
                            i.Codigo,
                            i.fecha,
                            i.Total,
                            i.Estado_Factura_id
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
