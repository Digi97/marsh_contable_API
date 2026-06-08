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
using Facturacion_C_Sharp;
using Facturacion_C_Sharp.Lib;

namespace marsh_contable.Controllers
{
  
    public class FacturasController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/facturas")]
        [RequierePermiso(PermisosAplica.UsuarioFacturacion)]
        public Reply CreateFactura([FromBody] Models.Facturas model)
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
                if (!tool.ValidaTexto(model.Clave))
                {
                    throw new Exception("invalid_string_form_Clave");
                }
                if (!tool.ValidaTexto(model.Consecutivo_electronico))
                {
                    throw new Exception("invalid_string_form_Consecutivo_electronico");
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
                if (!tool.validaNumeros(model.Tipo_documento_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Tipo_documento_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {

                    int siguienteConsecutivo = ctx.Facturas
                    .Where(x => x.Tipo_documento_id == (int)TipoDocumentoId.FacturaElectronica)
                    .Select(x => x.consecutivo)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                    Models.Facturas f = new Models.Facturas()
                    {
                        Clave = model.Clave,
                        Consecutivo_electronico = model.Consecutivo_electronico,
                        fecha = DateTime.Now,
                        consecutivo = siguienteConsecutivo,
                        Tipo_moneda_id = model.Tipo_moneda_id,
                        Estado_Factura_id = model.Estado_Factura_id,
                        Tipo_documento_id = model.Tipo_documento_id,
                        Subtotal = model.Subtotal,
                        Impuesto = model.Impuesto,
                        Total = model.Total,
                        Descuento = model.Descuento,
                        cambio_venta = model.cambio_venta,
                        cambio_compra = model.cambio_compra,
                        Clientes_id = model.Clientes_id,
                        Condicion_venta_id = model.Condicion_venta_id,
                        Medio_pago_id = model.Medio_pago_id
                    };
                    ctx.Facturas.Add(f);
                    ctx.SaveChanges();

                    FacturaDetallesController factDetalles = new FacturaDetallesController();
                    foreach (var detalles in model.Factura_Detalles)
                    {
                        detalles.Facturas_id = f.id;
                        var result = factDetalles.CreateFacturaDetalle(detalles, ctx);
                        if (result.CodeStatus != HttpStatusCode.OK)
                        {

                            throw new Exception(result.Message);
                        }

                    }

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = f.id;
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
        [Route("api/v1/facturas/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioFacturacion)]
        public Reply UpdateFactura(int id, [FromBody] Models.Facturas model)
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
                if (!tool.ValidaTexto(model.Clave))
                {
                    throw new Exception("invalid_string_form_Clave");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Facturas f = ctx.Facturas.FirstOrDefault(u => u.id == id);
                    if (f == null)
                    {
                        throw new Exception("factura_not_found");
                    }
                    f.Clave = model.Clave;
                    f.Consecutivo_electronico = model.Consecutivo_electronico;
                    f.consecutivo = model.consecutivo;
                    f.Tipo_moneda_id = model.Tipo_moneda_id;
                    f.Estado_Factura_id = model.Estado_Factura_id;
                    f.Tipo_documento_id = model.Tipo_documento_id;
                    f.Subtotal = model.Subtotal;
                    f.Impuesto = model.Impuesto;
                    f.Total = model.Total;
                    f.Descuento = model.Descuento;
                 
                    f.cambio_venta = model.cambio_venta;
                    f.cambio_compra = model.cambio_compra;
                    f.Clientes_id = model.Clientes_id;
                    f.Condicion_venta_id = model.Condicion_venta_id;
                    f.Medio_pago_id = model.Medio_pago_id;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = f.id;
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
        [Route("api/v1/facturas")]
        public Reply GetAllFacturas()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = (from f in ctx.Facturas
                                 join c in ctx.Clientes on f.Clientes_id equals c.id
                                 join tm in ctx.Tipo_moneda on f.Tipo_moneda_id equals tm.id
                                 join ef in ctx.Estado_Factura on f.Estado_Factura_id equals ef.id
                                 join td in ctx.Tipo_documento on f.Tipo_documento_id equals td.id
                                 join cv in ctx.Condicion_venta on f.Condicion_venta_id equals cv.id
                                 join mp in ctx.Medio_pago on f.Medio_pago_id equals mp.id
                                 select new Models.FacturasViewModel
                                 {
                                     id = f.id,
                                     Clave = f.Clave,
                                     Consecutivo_electronico = f.Consecutivo_electronico,
                                     fecha = f.fecha,
                                     consecutivo = f.consecutivo,
                                     Tipo_moneda_id = f.Tipo_moneda_id,
                                     Estado_Factura_id = f.Estado_Factura_id,
                                     Tipo_documento_id = f.Tipo_documento_id,
                                     Subtotal = f.Subtotal,
                                     Impuesto = f.Impuesto,
                                     Total = f.Total,
                                     Descuento = f.Descuento,
                           
                                     cambio_venta = f.cambio_venta,
                                     cambio_compra = f.cambio_compra,
                                     Clientes_id = f.Clientes_id,
                                     Condicion_venta_id = f.Condicion_venta_id,
                                     Medio_pago_id = f.Medio_pago_id,
                                     Cliente = c.Nombre + " " + c.Apellido1,
                                     Tipo_moneda = tm.Nombre,
                                     Estado_factura = ef.Nombre,
                                     Tipo_documento = td.Nombre,
                                     Condicion_venta = cv.Descripcion,
                                     Medio_pago = mp.descripcion
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
        [Route("api/v1/facturas/{id}")]
        public Reply GetFacturaById(int id)
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
                    var f = (from x in ctx.Facturas
                             join c in ctx.Clientes on x.Clientes_id equals c.id
                             join tm in ctx.Tipo_moneda on x.Tipo_moneda_id equals tm.id
                             join ef in ctx.Estado_Factura on x.Estado_Factura_id equals ef.id
                             join td in ctx.Tipo_documento on x.Tipo_documento_id equals td.id
                             join cv in ctx.Condicion_venta on x.Condicion_venta_id equals cv.id
                             join mp in ctx.Medio_pago on x.Medio_pago_id equals mp.id
                             where x.id == id
                             select new Models.FacturasViewModel
                             {
                                 id = x.id,
                                 Clave = x.Clave,
                                 Consecutivo_electronico = x.Consecutivo_electronico,
                                 fecha = x.fecha,
                                 consecutivo = x.consecutivo,
                                 Tipo_moneda_id = x.Tipo_moneda_id,
                                 Estado_Factura_id = x.Estado_Factura_id,
                                 Tipo_documento_id = x.Tipo_documento_id,
                                 Subtotal = x.Subtotal,
                                 Impuesto = x.Impuesto,
                                 Total = x.Total,
                                 Descuento = x.Descuento,
                         
                                 cambio_venta = x.cambio_venta,
                                 cambio_compra = x.cambio_compra,
                                 Clientes_id = x.Clientes_id,
                                 Condicion_venta_id = x.Condicion_venta_id,
                                 Medio_pago_id = x.Medio_pago_id,
                                 Cliente = c.Nombre + " " + c.Apellido1,
                                 Tipo_moneda = tm.Nombre,
                                 Estado_factura = ef.Nombre,
                                 Tipo_documento = td.Nombre,
                                 Condicion_venta = cv.Descripcion,
                                 Medio_pago = mp.descripcion
                             }).FirstOrDefault();

                    if (f == null)
                    {
                        throw new Exception("factura_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = f;
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
        [Route("api/v1/facturas/clave")]
        public Reply GetKeyConsecutive()
        {
            Reply oR = new Reply();
            General tool = new General();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    // Obtener el consecutivo: MAX(consecutivo) + 1 donde Tipo_documento_id = 1
                    int siguienteConsecutivo = ctx.Facturas
                        .Where(x => x.Tipo_documento_id == (int)TipoDocumentoId.FacturaElectronica)
                        .Select(x => x.consecutivo)
                        .DefaultIfEmpty(0)
                        .Max() + 1;

                    Models.EmpresaViewModel empresa = ctx.Empresa
                                            .Where(u => u.Emp_id == 1)
                                            .Select(u => new Models.EmpresaViewModel
                                            {
                                              
                                                Nombre_empresa = u.Nombre_empresa,
                                                Correo_empresa = u.Correo_empresa,
                                                Ruta_nas = u.Ruta_nas,
                                                Numero_sucursal = u.Numero_sucursal,
                                                Ruta_llave_factura = u.Ruta_llave_factura,
                                                pin_llave = u.pin_llave,
                                                terminal = u.terminal,
                                                codigo_seguridad = u.codigo_seguridad,
                                                identificacion = u.identificacion,
                                                codigo_actividad_id = u.codigo_actividad_id,
                                                tipo_identificacion_id = u.tipo_identificacion_id,
                                                Sede = (int)u.sede
                                              })
                                            .FirstOrDefault();

                    if (empresa == null)
                    {
                        throw new Exception("empresa_not_found");
                    }

                    empresa.pin_llave = (empresa.pin_llave == String.Empty ? "" : tool.Desencriptar(empresa.pin_llave));



                    var consecutivo = tool.NumeroConsecutivo(tool.FormatearSede(empresa.Numero_sucursal), tool.FormatearTerminal(empresa.terminal), tool.FormatearTipoDocumento(TipoDocumentoId.FacturaElectronica), siguienteConsecutivo.ToString());
                    var claveNumerica = tool.ClaveNumerica("506", empresa.identificacion, consecutivo, "1", empresa.codigo_seguridad);

                    if (siguienteConsecutivo == 0)
                    {
                        throw new Exception("factura_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new ClaveViewModel { Consecutivo = consecutivo, Clave = claveNumerica};
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




        // Facturas filtradas por cliente
        [HttpGet]
        [Authorize]
        [Route("api/v1/facturas/cliente/{clienteId}")]
        public Reply GetFacturasByCliente(int clienteId)
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
                    var lista = ctx.Facturas
                        .Where(f => f.Clientes_id == clienteId)
                        .Select(f => new {
                            f.id,
                            f.Clave,
                            f.Consecutivo_electronico,
                            f.fecha,
                            f.consecutivo,
                            f.Total,
                            f.Estado_Factura_id
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
