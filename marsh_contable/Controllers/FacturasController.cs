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
    //TODO: CREAR UNA FUNCION QUE RECIBA POR HTTPDELETE LA CUAL CREE UNA COPIA EN LA TABLA FACTURA QUE SEA DE TIPO TipoDocumentoId.NotaCreditoElectronica y cree el respectivo documento electronico


    //TODO: MODIFICAR LA  FUNCION UpdateFactura LA CUAL DEBERA RECIBIR TRES DETALLES, LOS QUE YA EXISTEN, NUEVOS Y ELIMINADOS, PARA LOS ELIMINADOS DEBERA CREAR
    // UNA FACTURA DE TIPO TipoDocumentoId.NotaCreditoElectronica y cree el respectivo documento electronico con los respectivos detalles
    //PARA EL ARRAY DE NUEVOS DEBERA CREAR UNA FACTURA DE TIPO TipoDocumentoId.NotaDebitoElectronica con los respectivos detalles

    //TODO: AL CREAR UNA FACTURA SE DEBE INGRESAR EN REGISTRO DE CUENTAS Y APLICACION A PRESUPUESTO


    //TODO: Modificar la funcion aceptafactura PARA CREAR EL REGISTRO DE UNA FACTURA INGRESADA DE TIPO TipoDocumentoId.ConfirmacionAceptacionMensajeReceptor Y SU RESPECTIVO DOCUMENTO ELECTRONICO Y VALIDARLO CON HACIENDA
    //
    //TODO_Manually: CONLCUIR VALIDACION CON HACIENDA

    public class FacturasController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/facturas")]
        [RequierePermiso(PermisosAplica.UsuarioFacturacion)]
        public Reply CreateFactura([FromBody] Models.Facturas model)
        {

            
            int id = 0;
            Models.Gestion_Presupuestaria gpExist;
            Models.Facturas f;
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

                      DateTime currentDate = DateTime.Now;

                    gpExist = ctx.Gestion_Presupuestaria
       .FirstOrDefault(u => currentDate >= u.periodo_inicio && currentDate <= u.periodo_fin);
                    if (gpExist == null)
                {
                    throw new Exception("gestion_presupuestaria_for_current_period_dont_exist");
                }

                    int siguienteConsecutivo = ctx.Facturas
                    .Where(x => x.Tipo_documento_id == (int)TipoDocumentoId.FacturaElectronica)
                    .Select(x => x.consecutivo)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                    var llaves = ObtenerClaveYConsecutivo(ctx, TipoDocumentoId.FacturaElectronica); //generamos las llaves, de nuevo, esto como doble factor para que no se repita la clave

                     f = new Models.Facturas()
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
                        Medio_pago_id = model.Medio_pago_id,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                                           };
                    ctx.Facturas.Add(f);
                    ctx.SaveChanges();
                    id = f.id;

                    FacturaDetallesController factDetalles = new FacturaDetallesController();
                    foreach (var detalles in model.Factura_Detalles)
                    {
                        detalles.Facturas_id = f.id;
                        var result = factDetalles.CreateFacturaDetalle(detalles);
                        if (result.CodeStatus != HttpStatusCode.OK)
                        {
                            throw new Exception(result.Message);
                        }
                    }
                    if (model.Condicion_venta_id == (int)CondicionVenta.Credito)
                    {
                        // Obtener días de crédito según condición de venta
                        var condicion = ctx.Condicion_venta.FirstOrDefault(c => c.id == model.Condicion_venta_id);
                        int diasCredito = condicion != null ? model.dias_credito : 30;

                        Models.Cuenta_Encabezado cxc = new Models.Cuenta_Encabezado()
                        {
                            Vigencia_inicial = DateTime.Now,
                            Vigencia_final = DateTime.Now.AddDays(diasCredito),
                            Tipo_moneda_id = f.Tipo_moneda_id,
                            Medio_pago_id = f.Medio_pago_id,
                            Total = (decimal)f.Total,
                            Monto_Proyeccion = (decimal)f.Total,
                            subtotal = (decimal)f.Subtotal,
                            impuesto = (decimal)f.Impuesto,
                            Descuento = (decimal)f.Descuento,
                            Referencia = f.Clave,
                            Fecha_creacion = DateTime.Now,
                            Ultima_Fecha_actualizacion = DateTime.Now,
                            Usuarios_Usuario_id = (int)model.Usuarios_Usuario_id,
                            Clientes_id = f.Clientes_id,
                            Facturas_id = f.id,
                            Proveedor_id = null,
                            Gastos_id = null,
                            Ingresos_id = null,
                            Estado = 1,
                            Tipo_cuentas_id = (int)TipoCuenta.CuentaPorCobrar,
                          
                            Centro_Costos_id = (int)gpExist.Centro_Costos_id,
                            Categoria_presupuestaria_id = (int) gpExist.Categoria_presupuestaria_id
                        };
                        ctx.Cuenta_Encabezado.Add(cxc);
                        ctx.SaveChanges();
                    }
                }
                Models.Gestion_P_detalle detalle = new Models.Gestion_P_detalle()
                {
                    Monto = f.Total,
                    Monto_aprobado = gpExist.monto_aprobado,
                    Monto_modificado = gpExist.monto_modificado,
                    Monto_compometido = gpExist.monto_comprometido,
                    Monto_ejecutado = (decimal)f.Total,
                    detalle_presupuesto = $"Factura #{id} - Clave: {f.Clave}",
                    Gestion_Presupuestaria_id = gpExist.id, // ID del presupuesto activo
                    Categoria_presupuestaria_id = (int)Modulos.Categoria_presupuestaria.Ingresos,
                    Gastos_id = null,
                    Ingresos_id = null,
                    Facturas_id = id,
                    Usuarios_Usuario_id = (int)model.Usuarios_Usuario_id,
                    Fecha_registro = DateTime.Now,
                    Observaciones = $"Consecutivo: {f.Consecutivo_electronico} | Subtotal: {f.Subtotal} | Impuesto: {f.Impuesto} | Descuento: {f.Descuento}",
                    activo = 1
                };


                GestionPDetalleController detalleGestion = new GestionPDetalleController();
                var response = detalleGestion.CreateGestionPDetalle(detalle);

                if(response.CodeStatus != HttpStatusCode.OK)
                {
                    throw new Exception(response.Message);
                }
                /// si todo se hace correctamente creamos el doc electronico
                CreateDocument(id, TipoDocumentoId.FacturaElectronica);

                oR.CodeStatus = HttpStatusCode.OK;
                oR.Data = id;
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
        [Route("api/v1/facturas/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioFacturacion)]
        public Reply UpdateFactura(int id, [FromBody] Models.FacturasViewModel model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            Models.Gestion_Presupuestaria gpExist;
            Models.Facturas f;
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

                if (!tool.ValidaTexto(model.Clave))
                {
                    throw new Exception("invalid_string_form_Clave");
                }


                if (model.Factura_DetalleAgregados.Count  == 0 || model.Factura_DetalleEliminados.Count == 0 )
                {
                    throw new Exception("nothing_changed_on_this_invoice");
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
                    f = ctx.Facturas.FirstOrDefault(u => u.id == id);
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
                }



                Models.Gestion_P_detalle detalle = new Models.Gestion_P_detalle()
                {
                    Monto =f.Total,
                    Monto_aprobado = gpExist.monto_aprobado,
                    Monto_modificado = gpExist.monto_modificado,
                    Monto_compometido = gpExist.monto_comprometido,
                    Monto_ejecutado = (decimal)f.Total,
                    detalle_presupuesto = $"Gastos #{id}",
                    Gestion_Presupuestaria_id = gpExist.id, // ID del presupuesto activo
                    Categoria_presupuestaria_id = (int)Modulos.Categoria_presupuestaria.Ingresos,
                    Gastos_id = null,
                    Ingresos_id = null,
                    Facturas_id = id,
                    Usuarios_Usuario_id = (int)model.Usuarios_Usuario_id,
                    Fecha_registro = DateTime.Now,
                    Observaciones = $"Id: {f.id} | Subtotal: {f.Subtotal} | Impuesto: {f.Impuesto} | Descuento: {f.Descuento}",
                    activo = 1
                };


                GestionPDetalleController detalleGestion = new GestionPDetalleController();
                var response = detalleGestion.UpdateGestionPDetalle(id, detalle, 2);

                if (response.CodeStatus != HttpStatusCode.OK)
                {
                    throw new Exception(response.Message);
                }


                oR.CodeStatus = HttpStatusCode.OK;
                oR.Data = id;
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
                                 where f.Tipo_documento_id == (int)TipoDocumentoId.FacturaElectronica //filtramos solo las facturas
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

                   


                    f.Factura_Detalles = (from d in ctx.Factura_Detalles
                                         join um in ctx.Unidad_medida on d.Unidad_medida_id equals um.id
                                         join cc in  ctx.Codigo_comercial on d.Codigo_comercial_id equals cc.id 
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
                                             Unidad_medida = um.Nombre,
                                             Codigo_comercial = cc.Nombre

                                         }).ToList();





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
                "1",//en linea
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
  
    
        private Boolean CreateDocument(int id, TipoDocumentoId tipoDocumento)
        {
            try
            {

                EmpresaViewModel empresaEmi;
                List<FacturaDetallesViewModel> detalle;
                FacturasViewModel f;
                using (var ctx = new Models.EntitiesModel())
                {
                     f = (from x in ctx.Facturas
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


                     detalle = (from d in ctx.Factura_Detalles
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


                     empresaEmi = (from u in ctx.Empresa
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
                                       Canton_emisor = canton.codigo.ToString().Substring(0, 1),
                                       Distrito_emisor = dist.codigo_distrito,
                                       OtrasSenas_Emisor = u.OtrasSenas,
                                       Telefono = u.Telefono,
                                       Codigo_Telefono = u.Codigo_telefono,
                                       Usuario_hacienda = u.Usuario_hacienda,
                                       Contrasena_hacienda = u.Contrasena_hacienda

                                   }).FirstOrDefault();
                }



                General tool = new General();


                var usuarioH = empresaEmi.Usuario_hacienda;
                var contraH = tool.Desencriptar(empresaEmi.Contrasena_hacienda);
                var config = new Configuracion(usuarioH, contraH, empresaEmi.Ruta_llave_factura, tool.Desencriptar(empresaEmi.pin_llave));
                var FH = new FacturacionHacienda(config);


                ////Emisor                   
                var cedulaEmi = new DocumentoIdentificacion(empresaEmi.Tipo_Identificacion, empresaEmi.identificacion);
                var telefonoEmi = new TelefonoBase(empresaEmi.Codigo_Telefono, empresaEmi.Telefono);
                var ubicacionEmi = new Ubicacion(empresaEmi.Provincia_emisor, empresaEmi.Canton_emisor, empresaEmi.Distrito_emisor, empresaEmi.OtrasSenas_Emisor);
              
                var emisor = new Emisor(empresaEmi.Nombre_empresa, cedulaEmi, ubicacionEmi, empresaEmi.Correo_empresa);

                //Informacion de cliente/receptor
                var cedula = new DocumentoIdentificacion(f.Tipo_identificacion, f.Cliente_cedula);
                var telefono = new TelefonoBase(f.Telefono_codigo_pais, f.Telefono_numero);
                var ubicacion = new Ubicacion(f.Cliente_Provincia, f.Cliente_Canton.ToString().Substring(0, 1), f.Cliente_distrito, f.Cliente_OtrasSenas);
                var receptor = new Receptor(f.Cliente, cedula, "", f.Cliente, ubicacion, telefono, null, f.Cliente_Correo);

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
                    totalDescuentos: (decimal)f.Descuento
                    );


                String rutaGuardado = empresaEmi.Ruta_nas;
                Documento.TipoDocumento tipoDoc = 0;
               

                switch (tipoDocumento)
                {
                    case TipoDocumentoId.FacturaElectronica:
                        rutaGuardado = rutaGuardado + "/Documentos_Electronicos/Facturas/";
                        tipoDoc = Documento.TipoDocumento.Factura_Electronica;
                     
                     break;
                    case TipoDocumentoId.NotaCreditoElectronica:

                        rutaGuardado = rutaGuardado + "/Documentos_Electronicos/Nota_Credito/";
                        tipoDoc = Documento.TipoDocumento.Nota_de_crédito;
                        break;
                    case TipoDocumentoId.NotaDebitoElectronica:
                        rutaGuardado = rutaGuardado + "/Documentos_Electronicos/Nota_Debito/";
                        tipoDoc = Documento.TipoDocumento.Nota_de_débito;
                        break;

                    case TipoDocumentoId.FacturaElectronicaCompra:
                        rutaGuardado = rutaGuardado + "/Documentos_Electronicos/FECompra/";
                        tipoDoc = Documento.TipoDocumento.Factura_Electronica_Compra;
                        break;

                    case TipoDocumentoId.ConfirmacionAceptacionMensajeReceptor:
                        rutaGuardado = rutaGuardado + "/Documentos_Electronicos/Aceptacion/";
                        tipoDoc = Documento.TipoDocumento.Aceptación_del_comprobante_electrónico;

                        break;
                    default:
                        rutaGuardado = rutaGuardado + "/Documentos_Electronicos/Facturas/";
                        tipoDoc = Documento.TipoDocumento.Factura_Electronica;
                        break;
                }    


                var factura = new Documento(DateTime.Now, emisor, Documento.CondicionVenta.Contado,
                                            f.Medio_pago, tipoDoc,//Documento.TipoDocumento.Factura_Electronica,
                                            items.ToArray(),
                                            resumenFac,
                                            Documento.SituacionDocumento.Normal,
                                            f.Clave,
                                            f.Consecutivo_electronico,
                                            receptor);




                factura.FirmarDocumento(config);//firmamos documento para guardarlo

                //var xmlFirmado = FirmadorXML.Firmar(factura, empresaEmi.Ruta_llave_factura, tool.Desencriptar(empresaEmi.pin_llave));
                FH.GuardarXMLEnviado(factura, rutaGuardado);
                saveXMLFIle(rutaGuardado + f.Clave + ".xml", id, f.Usuarios_Usuario_id, TablasReferencia.Facturas);

                //Enviar a Hacienda
                var esEnviado = FH.EnviarDocumento(factura);

                //Espera a Hacienda
                System.Threading.Thread.Sleep(2500);

                //Optener el estado de la factura
                var estado = FH.EstadoDocumento(f.Clave);

                using (var ctx = new Models.EntitiesModel())
                {

                    Models.Facturas fupdate = ctx.Facturas.FirstOrDefault(u => u.id == id);
                    if (f == null)
                    {
                        throw new Exception("factura_not_found");
                    }
                    int newStatus = 0;
                    switch(estado.EstadoEnHacienda)
                    {
                        case "aceptado":
                            newStatus = (int)EstadoFactura.AceptadoPorHacienda;
                        break;
                        case "procesando":
                            newStatus = (int)EstadoFactura.PendienteProcesarHacienda;
                            break;
                        case "rechazado":
                            newStatus = (int)EstadoFactura.RechazadoPorHacienda;
                            break;
                        case "recibido":
                            newStatus = (int)EstadoFactura.RecibidoHacienda;
                            break;
                        default:
                            newStatus = (int)EstadoFactura.Error;
                         break;
                    
                    }
                    fupdate.Estado_Factura_id = newStatus;

                    ctx.SaveChanges();
                }
                return true;

            }catch(Exception ex)
            {
                throw ex;
            }
        }



        private bool saveXMLFIle(string rutaArchivo, int  id = 0, int uid = 0, TablasReferencia tabla = TablasReferencia.Facturas)
        {
            try
            {


                bool existe = System.IO.File.Exists(rutaArchivo);

                // Obtener información completa
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(rutaArchivo);

                string nombreCompleto = fileInfo.Name;               // "llave_factura.p12"
                string nombreSinExt = fileInfo.Name.Replace(fileInfo.Extension, ""); // "llave_factura"
                string extension = fileInfo.Extension;           // ".p12"
                string directorio = fileInfo.DirectoryName;       // "C:\NAS\marsh\llaves"
                long tamanoBytes = fileInfo.Length;              // 1234
                double tamanoKB = fileInfo.Length / 1024.0;    // 1.2
                DateTime fechaCreacion = fileInfo.CreationTime;
                DateTime fechaModificacion = fileInfo.LastWriteTime;
                DateTime fechaAcceso = fileInfo.LastAccessTime;
                bool esReadOnly = fileInfo.IsReadOnly;

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Adjuntos a = new Models.Adjuntos()
                    {
                        Nombre_Archivo = nombreCompleto,
                        Ruta_Archivo = rutaArchivo,
                        estado = 1, //recien creado significa activo
                        Tipo_archivo_id = (int)TipoArchivo.XML,
                        Tamano = tamanoKB,
                        Descripcion = nombreCompleto,
                        Usuarios_Usuario_id = uid,//administrador por defecto
                        extension = extension,
                        referencia = id, //id de referencia
                        Tablas_referencia_id = (int)tabla, //Facturas
                        fecha_ingreso = DateTime.Now,
                        fecha_actualizacion = DateTime.Now
                    };
                    ctx.Adjuntos.Add(a);
                    ctx.SaveChanges();

                    return true;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        // ═══════════════════════════════════════════════════════════
        // ACEPTAR FACTURA — Recibe JSON normalizado del XML y lo
        // convierte en Factura + Detalles + Presupuesto + CXP
        // ═══════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize]
        [Route("api/v1/aceptafactura")]
        [RequierePermiso(PermisosAplica.UsuarioAceptacionFacturas)]
        public Reply AceptaFactura([FromBody] Modulos.AceptaFacturaViewModel model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            int facturaId = 0;
            int gastoId = 0;
            Models.Facturas f;
            Models.Gestion_Presupuestaria gpExist;

            try
            {
                // ═══════════════════════════════════════════════════════
                // VALIDACIONES
                // ═══════════════════════════════════════════════════════

                if (model == null)
                    throw new Exception("invalid_model_request_missing");

                if (string.IsNullOrEmpty(model.clave))
                    throw new Exception("invalid_string_form_clave");

                if (string.IsNullOrEmpty(model.numeroConsecutivo))
                    throw new Exception("invalid_string_form_numero_consecutivo");

                if (model.emisor == null)
                    throw new Exception("invalid_emisor_missing");

                if (string.IsNullOrEmpty(model.emisor.numeroIdentificacion))
                    throw new Exception("invalid_emisor_identificacion_missing");

                if (model.lineas == null || model.lineas.Count == 0)
                    throw new Exception("detail_lines_required");

                if (model.resumen == null)
                    throw new Exception("invalid_resumen_missing");

                if (String.IsNullOrEmpty(model.presupuesto_id))
                {
                    throw new Exception("presupuesto_not defined");
                }


                string[] partes = model.presupuesto_id.Split('_'); // id = gp.id+"_"+gp.Categoria_presupuestaria_id+"_"+ gp.Centro_Costos_id,

                int pid = int.Parse(partes[0]);
                int cpid = int.Parse(partes[1]);
                int ccid = int.Parse(partes[2]);


                validacionPresupuesto(pid, model.resumen.totalComprobante, cpid, ccid); //validamos el presupuesto

                // ═══════════════════════════════════════════════════════
                // NORMALIZAR TIPO DE DOCUMENTO
                // ═══════════════════════════════════════════════════════

                int tipoDocumentoId = NormalizarTipoDocumento(model.tipoDocumento);

                // ═══════════════════════════════════════════════════════
                // NORMALIZAR CONDICIÓN DE VENTA
                // ═══════════════════════════════════════════════════════

                int condicionVentaId = 1; // Default: Contado
                if (!string.IsNullOrEmpty(model.condicionVenta))
                {
                    int.TryParse(model.condicionVenta, out condicionVentaId);
                    if (condicionVentaId == 0) condicionVentaId = 1;
                }

                // ═══════════════════════════════════════════════════════
                // NORMALIZAR TIPO DE MONEDA
                // ═══════════════════════════════════════════════════════

                int tipoMonedaId = model.Tipo_moneda_id > 0
                    ? model.Tipo_moneda_id
                    : NormalizarTipoMoneda(model.resumen.codigoMoneda);

                // ═══════════════════════════════════════════════════════
                // PARSEAR FECHA DE EMISIÓN
                // ═══════════════════════════════════════════════════════

                DateTime fechaEmision = DateTime.Now;
                if (!string.IsNullOrEmpty(model.fechaEmision))
                {
                    DateTime.TryParse(model.fechaEmision, out fechaEmision);
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    DateTime currentDate = DateTime.Now;

                    // ═══════════════════════════════════════════════════
                    // VERIFICAR QUE NO EXISTA LA CLAVE YA REGISTRADA
                    // ═══════════════════════════════════════════════════

                    bool claveExiste = ctx.Facturas.Any(x => x.Clave == model.clave);
                    if (claveExiste)
                        throw new Exception("factura_clave_already_exists");

                    // ═══════════════════════════════════════════════════
                    // BUSCAR PRESUPUESTO VIGENTE
                    // ═══════════════════════════════════════════════════

                    gpExist = ctx.Gestion_Presupuestaria
                        .FirstOrDefault(u => currentDate >= u.periodo_inicio &&
                                             currentDate <= u.periodo_fin && u.id == pid);

                    if (gpExist == null)
                        throw new Exception("gestion_presupuestaria_for_current_period_dont_exist");

                    // ═══════════════════════════════════════════════════
                    // BUSCAR O CREAR PROVEEDOR (EMISOR)
                    // ═══════════════════════════════════════════════════

                    int proveedorId = 0;
                    var proveedor = ctx.Proveedor
                        .FirstOrDefault(p => p.identificacion == model.emisor.numeroIdentificacion);

                    if (proveedor != null)
                    {
                        proveedorId = proveedor.id;
                    }
                    else
                    {
                        int tipoIdentEmisor = 1;
                        int.TryParse(model.emisor.tipoIdentificacion, out tipoIdentEmisor);
                        if (tipoIdentEmisor == 0) tipoIdentEmisor = 1;

                        int provinciaEmisor = 1;
                        int.TryParse(model.emisor.provincia, out provinciaEmisor);
                        if (provinciaEmisor == 0) provinciaEmisor = 1;

                        var nuevoProveedor = new Models.Proveedor()
                        {
                            identificacion = model.emisor.numeroIdentificacion,
                            tipo_identificacion_id = tipoIdentEmisor,
                            Nombre = model.emisor.nombre ?? "",
                            Apellido1 = "",
                            Apellido2 = "",
                            correo = model.emisor.correo ?? "",
                            Distrito_id = 1,
                            Canton_id = 1,
                            Provincia_id = provinciaEmisor,
                            codigo_actividad_id = 1,
                            estado = 1,
                            fecha_creacion = DateTime.Now,
                            fecha_actualizacion = DateTime.Now,
                            exonerado = 0
                        };
                        ctx.Proveedor.Add(nuevoProveedor);
                        ctx.SaveChanges();
                        proveedorId = nuevoProveedor.id;
                    }

                    // ═══════════════════════════════════════════════════
                    // BUSCAR CLIENTE (RECEPTOR) — la empresa receptora
                    // ═══════════════════════════════════════════════════

                    int clienteId = 0;
                    if (!string.IsNullOrEmpty(model.receptor?.numeroIdentificacion))
                    {
                        var cliente = ctx.Clientes
                            .FirstOrDefault(c => c.identificacion == model.receptor.numeroIdentificacion);

                        if (cliente != null)
                            clienteId = cliente.id;
                    }

                    // ═══════════════════════════════════════════════════
                    // OBTENER CONSECUTIVO
                    // ═══════════════════════════════════════════════════

                    int siguienteConsecutivo = ctx.Facturas
                        .Where(x => x.Tipo_documento_id == tipoDocumentoId)
                        .Select(x => x.consecutivo)
                        .DefaultIfEmpty(0)
                        .Max() + 1;

                    // ═══════════════════════════════════════════════════
                    // CREAR FACTURA NORMALIZADA
                    // ═══════════════════════════════════════════════════

                    f = new Models.Facturas()
                    {
                        Clave = model.clave,
                        Consecutivo_electronico = model.numeroConsecutivo,
                        fecha = fechaEmision,
                        consecutivo = siguienteConsecutivo,
                        Tipo_moneda_id = tipoMonedaId,
                        Estado_Factura_id = (int)EstadoFactura.AceptadoPorHacienda,
                        Tipo_documento_id = tipoDocumentoId,
                        Subtotal = model.resumen.totalVenta,
                        Impuesto = model.resumen.totalImpuesto,
                        Total = model.resumen.totalComprobante,
                        Descuento = model.resumen.totalDescuentos,
                        cambio_venta = model.resumen.tipoCambio,
                        cambio_compra = model.resumen.tipoCambio,
                        Clientes_id = clienteId > 0 ? clienteId : 1,
                        Condicion_venta_id = condicionVentaId,
                        Medio_pago_id = model.Medio_pago_id > 0 ? model.Medio_pago_id : 1,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                       // dias_credito = model.dias_credito,
                       // presupuesto_id = model.presupuesto_id
                    };

                    ctx.Facturas.Add(f);
                    ctx.SaveChanges();
                    facturaId = f.id;

                    // ═══════════════════════════════════════════════════
                    // CREAR LÍNEAS DE DETALLE
                    // ═══════════════════════════════════════════════════

                    FacturaDetallesController factDetalles = new FacturaDetallesController();
                    foreach (var linea in model.lineas)
                    {
                        Models.Factura_Detalles detalle = new Models.Factura_Detalles()
                        {
                            Facturas_id = facturaId,
                            Subtotal = linea.subTotal,
                            Impuesto = linea.impuestoMonto,
                            Total = linea.montoTotalLinea,
                            Cantidad = linea.cantidad,
                            Detalle = linea.detalle ?? "",
                            Descuento = 0,
                            Fecha = DateTime.Now,
                            Ultima_fec_actualizacion = DateTime.Now
                        };

                        var result = factDetalles.CreateFacturaDetalle(detalle);
                        if (result.CodeStatus != HttpStatusCode.OK)
                            throw new Exception(result.Message);
                    }
                    // ═══════════════════════════════════════════════════
                    // CREAR GASTO — Si gastoRegistrado viene en false
                    // ═══════════════════════════════════════════════════

                    if (!model.gastoRegistrado)
                    {
                        Models.Gastos g = new Models.Gastos()
                        {
                            Descripcion = $"Factura {model.emisor.nombre} - {model.clave}",
                            Categoria_gasto_id = 1, // Default
                            Subtotal = model.resumen.totalVenta,
                            Impuesto = model.resumen.totalImpuesto,
                            Total = model.resumen.totalComprobante,
                            Doc_Referencia = model.clave,
                            Fecha = fechaEmision,
                            Ultima_Fec_Actualizacion = DateTime.Now,
                            Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                            Tipo_documento_id = tipoDocumentoId,
                            Medio_pago_id = model.Medio_pago_id > 0 ? model.Medio_pago_id : 1,
                            Proveedor_id = proveedorId,
                            Descuento = model.resumen.totalDescuentos,
                            Tipo_moneda_id = tipoMonedaId
                        };
                        ctx.Gastos.Add(g);
                        ctx.SaveChanges();
                        gastoId = g.id;

                        // Crear líneas de detalle del gasto
                        GastosDetallesController gastosDetalles = new GastosDetallesController();
                        foreach (var linea in model.lineas)
                        {
                            Models.Gastos_Detalles gd = new Models.Gastos_Detalles()
                            {
                                Subtotal = linea.subTotal,
                                Impuesto = linea.impuestoMonto,
                                Total = linea.montoTotalLinea,
                                Cantidad = linea.cantidad,
                                Detalle = linea.detalle ?? "",
                                Descuento = 0,
                                codigo_comercial = linea.codigoCabys ?? "",
                                Fecha = DateTime.Now,
                                Ultima_fec_actualizacion = DateTime.Now,
                                Gastos_id = gastoId
                            };

                            var resultGD = gastosDetalles.CreateGastoDetalle(gd, ctx);
                            if (resultGD.CodeStatus != HttpStatusCode.OK)
                                throw new Exception(resultGD.Message);
                        }

                        BancoController banco = new BancoController();
                        var bmovimiento = banco.RegistrarMovimientoPorGasto(cpid, (int)model.Tipo_moneda_id, ccid, gastoId, g.Total, g.Usuarios_Usuario_id, "Registro de Gasto");

                        if (bmovimiento.CodeStatus != HttpStatusCode.OK)
                        {
                            throw new Exception(bmovimiento.Message);
                        }
                    }



                    // ═══════════════════════════════════════════════════
                    // CXP — Si es crédito, crear Cuenta por Pagar
                    // ═══════════════════════════════════════════════════

                    if (condicionVentaId == (int)CondicionVenta.Credito)
                    {
                        int diasCredito = model.dias_credito > 0 ? model.dias_credito : 30;

                        Models.Cuenta_Encabezado cxp = new Models.Cuenta_Encabezado()
                        {
                            Vigencia_inicial = DateTime.Now,
                            Vigencia_final = DateTime.Now.AddDays(diasCredito),
                            Tipo_moneda_id = tipoMonedaId,
                            Medio_pago_id = f.Medio_pago_id,
                            Total = (decimal)f.Total,
                            Monto_Proyeccion = (decimal)f.Total,
                            subtotal = (decimal)f.Subtotal,
                            impuesto = (decimal)f.Impuesto,
                            Descuento = (decimal)f.Descuento,
                            Referencia = f.Clave,
                            Fecha_creacion = DateTime.Now,
                            Ultima_Fecha_actualizacion = DateTime.Now,
                            Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                            Clientes_id = null,
                            Facturas_id = facturaId,
                            Proveedor_id = proveedorId,
                            Gastos_id = gastoId > 0 ? (int?)gastoId : null,
                            Ingresos_id = null,
                            Estado = 1,
                            Tipo_cuentas_id = (int)TipoCuenta.CuentaPorPagar,
                            Centro_Costos_id = (int)gpExist.Centro_Costos_id,
                            Categoria_presupuestaria_id = (int)gpExist.Categoria_presupuestaria_id
                        };
                        ctx.Cuenta_Encabezado.Add(cxp);
                        ctx.SaveChanges();
                    }
                }

                // ═══════════════════════════════════════════════════
                // REGISTRAR EN GESTIÓN PRESUPUESTARIA (fuera del using)
                // ═══════════════════════════════════════════════════

                Models.Gestion_P_detalle detalleGP = new Models.Gestion_P_detalle()
                {
                    Monto = f.Total,
                    Monto_aprobado = gpExist.monto_aprobado,
                    Monto_modificado = gpExist.monto_modificado,
                    Monto_compometido = gpExist.monto_comprometido,
                    Monto_ejecutado = (decimal)f.Total,
                    detalle_presupuesto = $"Aceptación Factura #{facturaId} - Emisor: {model.emisor.nombre}",
                    Gestion_Presupuestaria_id = gpExist.id,
                    Categoria_presupuestaria_id = (int)Modulos.Categoria_presupuestaria.Gastos,
                    Gastos_id = gastoId > 0 ? (int?)gastoId : null,
                    Ingresos_id = null,
                    Facturas_id = facturaId,
                    Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                    Fecha_registro = DateTime.Now,
                    Observaciones = $"Clave: {model.clave} | Emisor: {model.emisor.nombre} ({model.emisor.numeroIdentificacion}) | Total: {model.resumen.totalComprobante}",
                    activo = 1
                };

                /// si todo se hace correctamente creamos el doc electronico
                CreateDocument(facturaId, TipoDocumentoId.ConfirmacionAceptacionMensajeReceptor);

                oR.CodeStatus = HttpStatusCode.OK;
                oR.Data = new
                {
                    factura_id = facturaId,
                    clave = model.clave,
                    consecutivo = model.numeroConsecutivo,
                    emisor_nombre = model.emisor.nombre,
                    emisor_identificacion = model.emisor.numeroIdentificacion,
                    total = model.resumen.totalComprobante,
                    impuesto = model.resumen.totalImpuesto,
                    descuento = model.resumen.totalDescuentos,
                    tipo_documento = model.tipoDocumento,
                    lineas_procesadas = model.lineas.Count
                };
                return oR;
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
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }


        // ═══════════════════════════════════════════════════════════
        // HELPERS DE NORMALIZACIÓN
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Convierte el nombre del tipo de documento del XML al ID de la BD.
        /// </summary>
        private int NormalizarTipoDocumento(string tipoDocumento)
        {
            if (string.IsNullOrEmpty(tipoDocumento))
                return (int)TipoDocumentoId.FacturaElectronica;

            switch (tipoDocumento.ToLower().Trim())
            {
                case "facturaelectronica":
                case "factura electrónica":
                case "factura electronica":
                    return (int)TipoDocumentoId.FacturaElectronica;

                case "notadebitoelectronica":
                case "nota de débito electrónica":
                case "nota debito electronica":
                    return (int)TipoDocumentoId.NotaDebitoElectronica;

                case "notacreditoelectronica":
                case "nota de crédito electrónica":
                case "nota credito electronica":
                    return (int)TipoDocumentoId.NotaCreditoElectronica;

                case "tiqueteelectronico":
                case "factura electrónica punto de venta":
                case "tiquete electronico":
                    return (int)TipoDocumentoId.FacturaElectronicaPuntoVenta;

                case "facturaelectronicaexportacion":
                case "factura electrónica de exportación":
                    return (int)TipoDocumentoId.FacturaElectronicaExportacion;

                case "facturaelectronicacompra":
                case "factura electrónica de compra":
                    return (int)TipoDocumentoId.FacturaElectronicaCompra;

                default:
                    return (int)TipoDocumentoId.FacturaElectronica;
            }
        }

        /// <summary>
        /// Convierte el código de moneda ISO al ID de la BD.
        /// </summary>
        private int NormalizarTipoMoneda(string codigoMoneda)
        {
            if (string.IsNullOrEmpty(codigoMoneda))
                return 1; // Default CRC

            switch (codigoMoneda.ToUpper().Trim())
            {
                case "CRC": return 1;
                case "USD": return 2;
                case "EUR": return 3;
                default: return 1;
            }
        }

        private bool validacionPresupuesto(int pid = 0, double gtotal = 0, int cpid = 0, int ccid = 0)
        {//  validacionPresupuesto(pid, model.Total, cpid, ccid
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
                                             u.id == pid && u.Categoria_presupuestaria_id == cpid
                                             && u.Centro_Costos_id == ccid);

                    if (gpExist == null)
                        throw new Exception("gestion_presupuestaria_for_current_period_dont_exist");

                    BancoController banco = new BancoController();
                    if (banco.validaBanco(cpid, (int)gpExist.Tipo_moneda_id, ccid, (decimal)gtotal, Tipo_Movimiento_Bancario.Egreso))
                    { //si el banco permite el movimiento proseguimos


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

                        if (montoMensual == 0)
                        {
                            throw new Exception(
                              $"no_presupuesto_defined_for_month_{currentDate.Month}_and_year_{currentDate.Year}"
                          );
                        }


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

                        //  Calcular porcentaje de uso con el nuevo gasto
                        double porcentajeUso = (montoConNuevoGasto / montoAprobado) * 100;
                        double porcentajeDisponible = 100 - porcentajeUso;

                        // Si queda entre 5% y 10% disponible, notificar por correo
                        if (porcentajeDisponible <= 10 && porcentajeDisponible >= 5)
                        {
                            NotificarPresupuestoBajo(ctx, gpExist, porcentajeUso, montoConNuevoGasto, montoAprobado, tool, Symbol);
                        }

                        //  Si queda menos de 5%, notificar con urgencia
                        if (porcentajeDisponible < 5 && porcentajeDisponible > 0)
                        {
                            NotificarPresupuestoCritico(ctx, gpExist, porcentajeUso, montoConNuevoGasto, montoAprobado, tool, Symbol);
                        }

                    }


                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private void NotificarPresupuestoBajo(
          Models.EntitiesModel ctx,
          Models.Gestion_Presupuestaria gp,
          double porcentajeUso,
          double montoEjecutado,
          double montoAprobado,
          General tool,
          string symbol
          )
        {
            try
            {
                // Obtener correos de usuarios con Rol 1 (Administración)
                var correosAdmin = ctx.Usuarios
                    .Where(u => u.Roles_id == 1 && u.activo == 1)
                    .Select(u => u.Correo)
                    .ToList();

                if (!correosAdmin.Any()) return;

                double disponible = montoAprobado - montoEjecutado;

                string asunto = $"Alerta: Presupuesto \"{gp.nombre}\" al {porcentajeUso:F1}% de uso";

                string cuerpo = $@"
            <h2 style='color:#d4a017;'> Alerta de Presupuesto - Uso Elevado</h2>
            <p>El presupuesto <strong>{gp.nombre}</strong> ha alcanzado un nivel de uso elevado.</p>
            <table style='border-collapse:collapse; width:100%; max-width:500px;'>
                <tr style='background-color:#f8f9fa;'>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Presupuesto</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'>{gp.nombre}</td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Año</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'>{gp.anio_presupuesto}</td>
                </tr>
                <tr style='background-color:#f8f9fa;'>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Monto Aprobado</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'>{symbol} {montoAprobado:N2}</td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Monto Ejecutado</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'>{symbol} {montoEjecutado:N2}</td>
                </tr>
                <tr style='background-color:#fff3cd;'>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Disponible</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'>{symbol} {disponible:N2}</td>
                </tr>
                <tr style='background-color:#fff3cd;'>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Porcentaje de Uso</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>{porcentajeUso:F1}%</strong></td>
                </tr>
            </table>
            <p style='color:#856404; margin-top:15px;'>
                El presupuesto se encuentra entre el <strong>90% y 95%</strong> de uso.
                Se recomienda tomar las previsiones necesarias.
            </p>
            <hr/>
            <small style='color:#6c757d;'>Notificación automática - Marsh Asprose</small>";

                foreach (var correo in correosAdmin)
                {
                    tool.Send_Mail(correo, asunto, cuerpo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Notifica a usuarios de Rol 1 cuando el presupuesto está al 95%+ de uso (crítico).
        /// </summary>
        private void NotificarPresupuestoCritico(
            Models.EntitiesModel ctx,
            Models.Gestion_Presupuestaria gp,
            double porcentajeUso,
            double montoEjecutado,
            double montoAprobado,
            General tool,
            string symbol
            )
        {
            try
            {
                var correosAdmin = ctx.Usuarios
                    .Where(u => u.Roles_id == 1 && u.activo == 1)
                    .Select(u => u.Correo)
                    .ToList();

                if (!correosAdmin.Any()) return;

                double disponible = montoAprobado - montoEjecutado;

                string asunto = $"URGENTE: Presupuesto \"{gp.nombre}\" al {porcentajeUso:F1}% de uso";

                string cuerpo = $@"
            <h2 style='color:#dc3545;'>Alerta Crítica de Presupuesto</h2>
            <p>El presupuesto <strong>{gp.nombre}</strong> está próximo a agotarse.</p>
            <table style='border-collapse:collapse; width:100%; max-width:500px;'>
                <tr style='background-color:#f8f9fa;'>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Presupuesto</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'>{gp.nombre}</td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Año</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'>{gp.anio_presupuesto}</td>
                </tr>
                <tr style='background-color:#f8f9fa;'>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Monto Aprobado</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'>{symbol} {montoAprobado:N2}</td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Monto Ejecutado</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6;'>{symbol} {montoEjecutado:N2}</td>
                </tr>
                <tr style='background-color:#f8d7da;'>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Disponible</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6; color:#dc3545;'>
                        <strong>₡ {disponible:N2}</strong>
                    </td>
                </tr>
                <tr style='background-color:#f8d7da;'>
                    <td style='padding:10px; border:1px solid #dee2e6;'><strong>Porcentaje de Uso</strong></td>
                    <td style='padding:10px; border:1px solid #dee2e6; color:#dc3545;'>
                        <strong>{porcentajeUso:F1}%</strong>
                    </td>
                </tr>
            </table>
            <p style='color:#dc3545; margin-top:15px;'>
                <strong>ATENCIÓN:</strong> El presupuesto tiene menos del <strong>5%</strong> disponible.
                Cualquier transacción adicional podría ser rechazada por exceder el monto aprobado.
            </p>
            <hr/>
            <small style='color:#6c757d;'>Notificación automática - Marsh Asprose</small>";

                foreach (var correo in correosAdmin)
                {
                    tool.Send_Mail(correo, asunto, cuerpo);
                }
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
    }
}
