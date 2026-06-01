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

    public class CuentaEncabezadoController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/cuenta_encabezado")]
        public Reply CreateCuentaEncabezado([FromBody] Models.Cuenta_Encabezado model)
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
                if (!tool.ValidaTexto(model.Referencia))
                {
                    throw new Exception("invalid_string_form_Referencia");
                }
                if (!tool.validaNumeros(model.Tipo_moneda_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Tipo_moneda_id");
                }
                if (!tool.validaNumeros(model.Medio_pago_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Medio_pago_id");
                }
                if (!tool.validaNumeros(model.Clientes_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Clientes_id");
                }
                if (!tool.validaNumeros(model.Tipo_cuentas_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Tipo_cuentas_id");
                }
                if (!tool.validaNumeros(model.Cuentas_Contables_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Cuentas_Contables_id");
                }
                if (!tool.validaNumeros(model.Centro_Costos_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Centro_Costos_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuenta_Encabezado ce = new Models.Cuenta_Encabezado()
                    {
                        Vigencia_inicial = model.Vigencia_inicial,
                        Vigencia_final = model.Vigencia_final,
                        Tipo_moneda_id = model.Tipo_moneda_id,
                        Medio_pago_id = model.Medio_pago_id,
                        Total = model.Total,
                        Monto_Proyeccion = model.Monto_Proyeccion,
                        Fecha_creacion = DateTime.Now,
                        Ultima_Fecha_actualizacion = DateTime.Now,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Facturas_id = model.Facturas_id,
                        Referencia = model.Referencia,
                        Clientes_id = model.Clientes_id,
                        impuesto = model.impuesto,
                        subtotal = model.subtotal,
                        Estado = (Int16)model.Estado,
                        Tipo_cuentas_id = model.Tipo_cuentas_id,
                        Cuentas_Contables_id = model.Cuentas_Contables_id,
                        Centro_Costos_id = model.Centro_Costos_id,
                        Gastos_id = model.Gastos_id
                    };
                    ctx.Cuenta_Encabezado.Add(ce);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ce.id;
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
        [Route("api/v1/cuenta_encabezado/{id}")]
        public Reply UpdateCuentaEncabezado(int id, [FromBody] Models.Cuenta_Encabezado model)
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
                if (!tool.ValidaTexto(model.Referencia))
                {
                    throw new Exception("invalid_string_form_Referencia");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuenta_Encabezado ce = ctx.Cuenta_Encabezado.FirstOrDefault(u => u.id == id);
                    if (ce == null)
                    {
                        throw new Exception("cuenta_encabezado_not_found");
                    }
                    ce.Vigencia_inicial = model.Vigencia_inicial;
                    ce.Vigencia_final = model.Vigencia_final;
                    ce.Tipo_moneda_id = model.Tipo_moneda_id;
                    ce.Medio_pago_id = model.Medio_pago_id;
                    ce.Total = model.Total;
                    ce.Monto_Proyeccion = model.Monto_Proyeccion;
                    ce.Facturas_id = model.Facturas_id;
                    ce.Referencia = model.Referencia;
                    ce.Clientes_id = model.Clientes_id;
                    ce.impuesto = model.impuesto;
                    ce.subtotal = model.subtotal;
                    ce.Estado = (Int16)model.Estado;
                    ce.Tipo_cuentas_id = model.Tipo_cuentas_id;
                    ce.Cuentas_Contables_id = model.Cuentas_Contables_id;
                    ce.Centro_Costos_id = model.Centro_Costos_id;
                    ce.Gastos_id = model.Gastos_id;
                    ce.Ultima_Fecha_actualizacion = DateTime.Now;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ce.id;
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
        [Route("api/v1/cuenta_encabezado")]
        public Reply GetAllCuentaEncabezado()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from ce in ctx.Cuenta_Encabezado
                                 join tm in ctx.Tipo_moneda on ce.Tipo_moneda_id equals tm.id
                                 join mp in ctx.Medio_pago on ce.Medio_pago_id equals mp.id
                                 join c in ctx.Clientes on ce.Clientes_id equals c.id
                                 join tc in ctx.Tipo_cuentas on ce.Tipo_cuentas_id equals tc.id
                                 join cc in ctx.Cuentas_Contables on ce.Cuentas_Contables_id equals cc.id
                                 join cco in ctx.Centro_Costos on ce.Centro_Costos_id equals cco.id
                                 join u in ctx.Usuarios on ce.Usuarios_Usuario_id equals u.Usuario_id
                                 select new Models.CuentaEncabezadoViewModel
                                 {
                                     id = ce.id,
                                     Vigencia_inicial = ce.Vigencia_inicial,
                                     Vigencia_final = ce.Vigencia_final,
                                     Tipo_moneda_id = ce.Tipo_moneda_id,
                                     Medio_pago_id = ce.Medio_pago_id,
                                     Total = ce.Total,
                                     Monto_Proyeccion = ce.Monto_Proyeccion,
                                     Fecha_creacion = ce.Fecha_creacion,
                                     Ultima_Fecha_actualizacion = ce.Ultima_Fecha_actualizacion,
                                     Usuarios_Usuario_id = ce.Usuarios_Usuario_id,
                                     Facturas_id = ce.Facturas_id,
                                     Referencia = ce.Referencia,
                                     Clientes_id = ce.Clientes_id,
                                     impuesto = ce.impuesto,
                                     subtotal = ce.subtotal,
                                     Estado = ce.Estado,
                                     Tipo_cuentas_id = ce.Tipo_cuentas_id,
                                     Cuentas_Contables_id = ce.Cuentas_Contables_id,
                                     Centro_Costos_id = ce.Centro_Costos_id,
                                     Gastos_id = ce.Gastos_id,
                                     Tipo_moneda = tm.Nombre,
                                     Medio_pago = mp.descripcion,
                                     Cliente = c.Nombre + " " + c.Apellido1,
                                     Tipo_cuenta = tc.Nombre,
                                     Cuenta_contable = cc.Nombre,
                                     Centro_costo = cco.Nombre,
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
        [Route("api/v1/cuenta_encabezado/{id}")]
        public Reply GetCuentaEncabezadoById(int id)
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
                    var ce = (from x in ctx.Cuenta_Encabezado
                              join tm in ctx.Tipo_moneda on x.Tipo_moneda_id equals tm.id
                              join mp in ctx.Medio_pago on x.Medio_pago_id equals mp.id
                              join c in ctx.Clientes on x.Clientes_id equals c.id
                              join tc in ctx.Tipo_cuentas on x.Tipo_cuentas_id equals tc.id
                              join cc in ctx.Cuentas_Contables on x.Cuentas_Contables_id equals cc.id
                              join cco in ctx.Centro_Costos on x.Centro_Costos_id equals cco.id
                              join u in ctx.Usuarios on x.Usuarios_Usuario_id equals u.Usuario_id
                              where x.id == id
                              select new Models.CuentaEncabezadoViewModel
                              {
                                  id = x.id,
                                  Vigencia_inicial = x.Vigencia_inicial,
                                  Vigencia_final = x.Vigencia_final,
                                  Tipo_moneda_id = x.Tipo_moneda_id,
                                  Medio_pago_id = x.Medio_pago_id,
                                  Total = x.Total,
                                  Monto_Proyeccion = x.Monto_Proyeccion,
                                  Fecha_creacion = x.Fecha_creacion,
                                  Ultima_Fecha_actualizacion = x.Ultima_Fecha_actualizacion,
                                  Usuarios_Usuario_id = x.Usuarios_Usuario_id,
                                  Facturas_id = x.Facturas_id,
                                  Referencia = x.Referencia,
                                  Clientes_id = x.Clientes_id,
                                  impuesto = x.impuesto,
                                  subtotal = x.subtotal,
                                  Estado = x.Estado,
                                  Tipo_cuentas_id = x.Tipo_cuentas_id,
                                  Cuentas_Contables_id = x.Cuentas_Contables_id,
                                  Centro_Costos_id = x.Centro_Costos_id,
                                  Gastos_id = x.Gastos_id,
                                  Tipo_moneda = tm.Nombre,
                                  Medio_pago = mp.descripcion,
                                  Cliente = c.Nombre + " " + c.Apellido1,
                                  Tipo_cuenta = tc.Nombre,
                                  Cuenta_contable = cc.Nombre,
                                  Centro_costo = cco.Nombre,
                                  Usuario = u.Nombre + " " + u.Apellido1
                              }).FirstOrDefault();

                    if (ce == null)
                    {
                        throw new Exception("cuenta_encabezado_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ce;
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
        [Route("api/v1/cuenta_encabezado/{id}")]
        public Reply DeleteCuentaEncabezado(int id)
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
                    Models.Cuenta_Encabezado ce = ctx.Cuenta_Encabezado.FirstOrDefault(u => u.id == id);
                    if (ce == null)
                    {
                        throw new Exception("cuenta_encabezado_not_found");
                    }
                    // Borrado lógico
                    ce.Estado = 0;
                    ce.Ultima_Fecha_actualizacion = DateTime.Now;
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
