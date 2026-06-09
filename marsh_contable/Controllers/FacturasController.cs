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
using Facturacion_C_Sharp.Lib.DocumentoItems;

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

                    var llaves = ObtenerClaveYConsecutivo(ctx, TipoDocumentoId.FacturaElectronica); //generamos las llaves, de nuevo, esto como doble factor para que no se repita la clave

                    Models.Facturas f = new Models.Facturas()
                    {
                        Clave = llaves.Clave,
                        Consecutivo_electronico = llaves.Consecutivo,
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
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var result = ObtenerClaveYConsecutivo(ctx, TipoDocumentoId.FacturaElectronica);

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = result;
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

        
        private ClaveViewModel ObtenerClaveYConsecutivo(Models.EntitiesModel ctx, TipoDocumentoId tipoDocumento)
        {
            General tool = new General();

            // Obtener el siguiente consecutivo
            int siguienteConsecutivo = ctx.Facturas
                .Where(x => x.Tipo_documento_id == (int)tipoDocumento)
                .Select(x => x.consecutivo)
                .DefaultIfEmpty(0)
                .Max() + 1;

            // Obtener datos de la empresa
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
                   
                })
                .FirstOrDefault();

            if (empresa == null)
                throw new Exception("empresa_not_found");

            empresa.pin_llave = string.IsNullOrEmpty(empresa.pin_llave)
                ? ""
                : tool.Desencriptar(empresa.pin_llave);

            string consecutivo = tool.NumeroConsecutivo(
                tool.FormatearSede(empresa.Numero_sucursal),
                tool.FormatearTerminal(empresa.terminal),
                tool.FormatearTipoDocumento(tipoDocumento),
                siguienteConsecutivo.ToString()
            );

            string claveNumerica = tool.ClaveNumerica(
                "506",
                empresa.identificacion,
                consecutivo,
                "1",
                empresa.codigo_seguridad
            );

            return new ClaveViewModel
            {
                Consecutivo = consecutivo,
                Clave = claveNumerica
            };
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
  
    
        private Boolean CreateDocument(int id, Models.EntitiesModel ctx)
        {
            try
            {

                using (ctx)
                {
                    var f = (from x in ctx.Facturas
                             join c in ctx.Clientes on x.Clientes_id equals c.id
                             join ti in ctx.tipo_identificacion on c.tipo_identificacion_id equals ti.id
                             join tm in ctx.Tipo_moneda on x.Tipo_moneda_id equals tm.id
                             join ef in ctx.Estado_Factura on x.Estado_Factura_id equals ef.id
                             join td in ctx.Tipo_documento on x.Tipo_documento_id equals td.id
                             join cv in ctx.Condicion_venta on x.Condicion_venta_id equals cv.id
                             join mp in ctx.Medio_pago on x.Medio_pago_id equals mp.id
                             join tel in ctx.Telefonos on c.id equals tel.Clientes_id 
                             join canton in ctx.Canton on c.Canton_id equals canton.id
                             join dist in ctx.Distrito on c.Distrito_id equals dist.id 
                             where x.id == id && tel.telefono_principal == 1
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
                                 Tipo_moneda = tm.codigo_moneda,
                                 Estado_factura = ef.Nombre,
                                 Tipo_documento = td.Nombre,
                                 Condicion_venta = cv.Descripcion,
                                 Medio_pago = mp.codigo,
                                 Tipo_identificacion = ti.codigo_tipo_identificacion,
                                 Cliente_cedula = c.identificacion,
                                 Telefono_numero = tel.Numero,
                                 Telefono_codigo_pais = tel.codigo_pais,
                                 Cliente_Provincia = c.Provincia_id.ToString(),
                                 Cliente_Canton = canton.codigo,
                                 Cliente_distrito = dist.codigo_distrito,
                                 Cliente_OtrasSenas = c.OtrasSenas,
                                 Cliente_Correo = c.correo

                             }).FirstOrDefault();


                    var detalle = (from d in ctx.Factura_Detalles
                                   join um in ctx.Unidad_medida on d.Unidad_medida_id equals um.id
                                   join i in ctx.Impuesto on d.Impuesto_id equals i.id
                                   where d.Facturas_id == id
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
                                       Unidad_medida = um.Codigo,
                                       Impuesto_detalle = i
                                   }).ToList();


                    var empresaEmi = (from u in ctx.Empresa
                                   join ti in ctx.tipo_identificacion on u.tipo_identificacion_id equals ti.id
                                    join canton in ctx.Canton on u.Canton_id equals canton.id
                                      join dist in ctx.Distrito on u.Distrito_id equals dist.id
                                      where u.Emp_id == 1
                                   select new Models.EmpresaViewModel
                                   {
                                       Emp_id = u.Emp_id,
                                       Nombre_empresa = u.Nombre_empresa,
                                       Correo_empresa = u.Correo_empresa,
                                       Ruta_nas = u.Ruta_nas,
                                       Numero_sucursal = u.Numero_sucursal,
                                       Formato_fecha = u.Formato_fecha,
                                       Ruta_llave_factura = u.Ruta_llave_factura,
                                       pin_llave = u.pin_llave,
                                       ruta_logo = u.ruta_logo,
                                       terminal = u.terminal,
                                       codigo_seguridad = u.codigo_seguridad,
                                       identificacion = u.identificacion,
                                       codigo_actividad_id = u.codigo_actividad_id,
                                       tipo_identificacion_id = u.tipo_identificacion_id,
                                       Impuesto_id = u.Impuesto_id,
                                       Tipo_Identificacion = ti.codigo_tipo_identificacion,
                                       Provincia_emisor = u.Provincia_id.ToString(),
                                       Canton_emisor = canton.codigo,
                                       Distrito_emisor = dist.codigo_distrito,
                                       OtrasSenas_Emisor = u.OtrasSenas,
                                       Telefono_Emisor = u.Telefono,
                                       Codigo_Telefono_Emisor = u.Codigo_telefono
                                   }).FirstOrDefault();




                    var user = "**************";
                    var userPass = "**********";

                    var pin = "****";
                    var p12 = "C:\\Users\\$$$$$\\Desktop\\060339051236.p12";

                    var config = new Configuracion(user, userPass, p12, pin);
                    var FH = new FacturacionHacienda(config);

                    
                    
                    ////Emisor                   
                    var cedulaEmi = new DocumentoIdentificacion(empresaEmi.Tipo_Identificacion, empresaEmi.identificacion);
                   var telefonoEmi = new TelefonoBase( empresaEmi.Codigo_Telefono_Emisor, empresaEmi.Telefono_Emisor);
                    var ubicacionEmi = new Ubicacion(empresaEmi.Provincia_emisor, empresaEmi.Canton_emisor, empresaEmi.Distrito_emisor, empresaEmi.OtrasSenas_Emisor);
                    //var email = "*****@***.com";
                    var emisor = new Emisor("NOMBRE EMISOR", cedulaEmi, ubicacionEmi,empresaEmi.Correo_empresa);

                    //Informacion de cliente/receptor
                    var cedula = new DocumentoIdentificacion( f.Tipo_identificacion,f.Cliente_cedula);
                    var telefono = new TelefonoBase( f.Telefono_codigo_pais, f.Telefono_numero);
                    var ubicacion = new Ubicacion(f.Cliente_Provincia, f.Cliente_Canton, f.Cliente_distrito, f.Cliente_OtrasSenas);
                    var receptor = new Receptor("NOMBRE RECEPTOR", cedula,"", f.Cliente,ubicacion,telefono,null, f.Cliente_Correo);

                    var items = new List<Item>();

                    int index = 1;

                    foreach (var row in detalle)
                    {

                        decimal precio_unitario = (decimal)(row.Subtotal / row.Cantidad);
                        decimal montoTotal = (decimal)(row.Subtotal / row.Cantidad);

                        //    Item(int numeroLinea, decimal cantidad, string unidadMedida, string detalle, decimal precioUnitario, decimal montoTotal, decimal subTotal, decimal montoTotalLinea,
                        //string[] codigos = null, decimal descuento = 0,
                        //string naturalezaDescuento = "", Impuesto[] impuestos = null,
                        //Exoneracion[] exoneraciones = null)
                        var taxLine = row.Impuesto_detalle;
                        

                        var tax = new Facturacion_C_Sharp.Lib.DocumentoItems.Impuesto(taxLine.TarifaIVACodigo, (decimal)taxLine.Porcentaje, 
                            (decimal)row.Impuesto);

                        items.Add(new Item(index, row.Cantidad, row.Unidad_medida, row.Detalle, precio_unitario, (decimal)row.Subtotal, (decimal)row.Subtotal, (decimal)row.Total,
                            impuestos: new Facturacion_C_Sharp.Lib.DocumentoItems.Impuesto[] { tax }, descuento: (decimal)row.Descuento, naturalezaDescuento: "Descuento aplicado")); //agregf
                        index++;
                    }



                    var resumenFac = new ResumenFactura(codigoMoneda: f.Tipo_moneda, tipoCambio: (f.Tipo_moneda == "CRC" ? 1 : (decimal)f.cambio_compra), 
                        totalServExentos: 0, 
                        totalMercanciasGravadas: (decimal)f.Total, 
                        totalExento: 0, 
                        totalGravado: (decimal)f.Total, 
                        totalVenta: (decimal)f.Total, 
                        totalVentaNeta: (decimal)f.Total, 
                        totalImpuesto: (decimal)f.Impuesto, 
                        totalComprobante: (decimal)f.Total,
                        totalDescuentos: (decimal) f.Descuento
                        );

         

                    //DateTime fechaEmision,
                    //     Emisor emisor,
                    //     CondicionVenta condicionVenta,
                    //     String medioPago,                        
                    //     TipoDocumento tipoDocumento,
                    //     Item[] items,
                    //     ResumenFactura resumenFactura,
                    //     SituacionDocumento situacionDocumento,
                    //     string clave,
                    //     string consecutivo,
                    //     Receptor receptor = null,
                    //     Normativa normativa = null,
                    //     Referencia[] referencias = null,
                    //     string plazoCredito = ""
                    var factura = new Documento(DateTime.Now, emisor, Documento.CondicionVenta.Contado,
                                                f.Medio_pago, Documento.TipoDocumento.Factura_Electronica,
                                                items.ToArray(), 
                                                resumenFac, 
                                                Documento.SituacionDocumento.Normal,
                                                f.Clave,
                                                f.Consecutivo_electronico,
                                                receptor );




                }


                return true;

            }catch(Exception ex)
            {
                throw ex;
            }
        }
    
    }
}
