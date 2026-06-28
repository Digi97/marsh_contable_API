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
    public class IngresosController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/ingresos")]
        public Reply CreateIngreso([FromBody] Models.Ingresos model)
        {

            int id = 0;
            Models.Gestion_Presupuestaria gpExist;
            Models.Ingresos i;
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

                if (model.Ingresos_Detalle == null || model.Ingresos_Detalle.Count == 0)
                {
                    throw new Exception("detail_is_required");
                }

                if (model.Tipo_moneda_id == 0)
                {
                    throw new Exception("currency_is_required");
                }

                using (var ctx = new Models.EntitiesModel())
                {

                    DateTime currentDate = DateTime.Now;

                    gpExist = ctx.Gestion_Presupuestaria
       .FirstOrDefault(u => currentDate >= u.periodo_inicio && currentDate <= u.periodo_fin);
                    
                    if (gpExist == null)
                    {
                        throw new Exception("gestion_presupuestaria_for_current_period_dont_exist");
                    }
                     i = new Models.Ingresos()
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
                        //Facturas_id = null//model.Facturas_id
                    };
                    ctx.Ingresos.Add(i);
                    ctx.SaveChanges();

                    // Guardar detalles usando el mismo contexto
                    IngresosDetalleController ingresosDetalle = new IngresosDetalleController();
                    foreach (var detalle in model.Ingresos_Detalle)
                    {
                        id = i.id;
                        detalle.Ingresos_id = i.id;
                        var result = ingresosDetalle.CreateIngresoDetalle(detalle, ctx);
                        if (result.CodeStatus != HttpStatusCode.OK)
                        {
                            throw new Exception(result.Message);
                        }
                    }

          
                }


                Models.Gestion_P_detalle detalleP = new Models.Gestion_P_detalle()
                {
                    Monto = i.Total,
                    Monto_aprobado = gpExist.monto_aprobado,
                    Monto_modificado = gpExist.monto_modificado,
                    Monto_compometido = gpExist.monto_comprometido,
                    Monto_ejecutado = (decimal)i.Total,
                    detalle_presupuesto = $"Ingresos #{id}",
                    Gestion_Presupuestaria_id = gpExist.id, // ID del presupuesto activo
                    Categoria_presupuestaria_id = (int)Modulos.Categoria_presupuestaria.Ingresos,
                    Gastos_id = null,
                    Ingresos_id = id,
                    Facturas_id = null,
                    Usuarios_Usuario_id = (int)model.Usuarios_Usuario_id,
                    Fecha_registro = DateTime.Now,
                    Observaciones = $"Id: {i.id} | Subtotal: {i.Subtotal} | Impuesto: {i.Impuesto} | Descuento: {i.Descuento}",
                    activo = 1
                };


                GestionPDetalleController detalleGestion = new GestionPDetalleController();
                var response = detalleGestion.CreateGestionPDetalle(detalleP);

                if (response.CodeStatus != HttpStatusCode.OK)
                {
                    throw new Exception(response.Message);
                }

                oR.CodeStatus = HttpStatusCode.OK;
                oR.Data = i.id;
                return oR;
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
            Models.Gestion_Presupuestaria gpExist;
            Models.Ingresos i;
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
                if (model.Tipo_moneda_id == 0)
                {
                    throw new Exception("currency_is_required");
                }

                using (var ctx = new Models.EntitiesModel())
                {

                    DateTime currentDate = DateTime.Now;
                    gpExist = ctx.Gestion_Presupuestaria
      .FirstOrDefault(u => currentDate >= u.periodo_inicio && currentDate <= u.periodo_fin);
                    
                    if (gpExist == null)
                    {
                        throw new Exception("gestion_presupuestaria_for_current_period_dont_exist");
                    }
                    
                    i = ctx.Ingresos.FirstOrDefault(u => u.id == id);
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

                }



                Models.Gestion_P_detalle detalle = new Models.Gestion_P_detalle()
                {
                    Monto = i.Total,
                    Monto_aprobado = gpExist.monto_aprobado,
                    Monto_modificado = gpExist.monto_modificado,
                    Monto_compometido = gpExist.monto_comprometido,
                    Monto_ejecutado = (decimal)i.Total,
                    detalle_presupuesto = $"Gastos #{id}",
                    Gestion_Presupuestaria_id = gpExist.id, // ID del presupuesto activo
                    Categoria_presupuestaria_id = (int)Modulos.Categoria_presupuestaria.Ingresos,
                    Gastos_id = null,
                    Ingresos_id = id,
                    Facturas_id = null,
                    Usuarios_Usuario_id = (int)model.Usuarios_Usuario_id,
                    Fecha_registro = DateTime.Now,
                    Observaciones = $"Id: {i.id} | Subtotal: {i.Subtotal} | Impuesto: {i.Impuesto} | Descuento: {i.Descuento}",
                    activo = 1
                };


                GestionPDetalleController detalleGestion = new GestionPDetalleController();
                var response = detalleGestion.UpdateGestionPDetalle(id, detalle, 1);

                if (response.CodeStatus != HttpStatusCode.OK)
                {
                    throw new Exception(response.Message);
                }


                oR.CodeStatus = HttpStatusCode.OK;
                oR.Data = i.id;
                return oR;
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
                                     Tipo_moneda = tm.Simbolo,
                                     Estado_factura = ef.Nombre,
                                     Medio_pago = mp.descripcion,
                                     Usuario = u.Nombre + " " + u.Apellido1
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

                    // Cargar detalles del ingreso
                    if (i != null)
                    {
                        i.IngresosDetalle = ctx.Ingresos_Detalle
                            .Where(t => t.Ingresos_id == id)
                            .Select(t => new Models.IngresosDetalleViewModel
                            {
                                id = t.id,
                                Subtotal = t.Subtotal,
                                Impuesto = t.Impuesto,
                                Total = t.Total,
                                Cantidad = t.Cantidad,
                                Detalle = t.Detalle,
                                Descuento = t.Descuento,
                                codigo_comercial = t.codigo_comercial,
                                //codigo_comercial_id = t.codigo_comercial_id,
                                Ingresos_id = t.Ingresos_id
                            }).ToList();
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
                        .Select(i => new
                        {
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


        private bool validacionPresupuesto(int pid = 0, double gtotal = 0)
        {
            General tool = new General();
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    DateTime currentDate = DateTime.Now;

                    // Buscar presupuesto vigente
                    Models.Gestion_Presupuestaria gpExist = ctx.Gestion_Presupuestaria
                        .FirstOrDefault(u => currentDate >= u.periodo_inicio &&
                                             currentDate <= u.periodo_fin &&
                                             u.id == pid);

                    if (gpExist == null)
                        throw new Exception("gestion_presupuestaria_for_current_period_dont_exist");

                    string Symbol = ctx.Tipo_moneda
                        .Where(t => t.id == gpExist.Tipo_moneda_id)
                        .Select(t => t.Simbolo)
                        .FirstOrDefault() ?? "₡";


                    // Sumar montos ya ejecutados en gestion_p_detalle

                    int anioActual = currentDate.Year;
                    int mesActual = currentDate.Month;

                    double montoEjecutado = (from d in ctx.Gestion_P_detalle
                                             join gp in ctx.Gestion_Presupuestaria
                                                 on d.Gestion_Presupuestaria_id equals gp.id
                                             where d.Gestion_Presupuestaria_id == pid
                                                && d.activo == 1
                                                && gp.anio_presupuesto == anioActual.ToString()
                                                && currentDate >= gp.periodo_inicio
                                                && currentDate <= gp.periodo_fin
                                                && d.Fecha_registro.Month == mesActual
                                                && d.Fecha_registro.Year == anioActual
                                             select d.Monto)
                                             .DefaultIfEmpty(0)
                                             .Sum();


                    decimal montoMensual = ctx.Gestion_P_Anio
                        .Where(d => d.Gestion_Presupuestaria_id == pid && d.anio_presupuesto == currentDate.Year.ToString() && d.mes == currentDate.Month)
                        .Select(d => d.monto)
                        .DefaultIfEmpty(0)
                        .Sum();



                    double montoAprobado = (double)montoMensual; //MONTO APROBADO PARA EL MES ACTUAL //gpExist.monto_aprobado;
                    double montoConNuevoGasto = montoEjecutado + gtotal;

                    // Validar que el nuevo gasto no exceda el presupuesto
                    if (montoConNuevoGasto >= montoAprobado)
                    {
                        double disponible = montoAprobado - montoEjecutado;
                        throw new Exception(
                            $"presupuesto_excedido_monto_aprobado_{montoAprobado}_ejecutado_{montoEjecutado}_disponible_{disponible}"
                        );
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
