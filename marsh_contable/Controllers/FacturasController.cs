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
    // NOTA: El registro en cuentas por cobrar (Cuenta_Encabezado) y la aplicación al presupuesto
    // (Gestion_P_detalle) al crear una factura ya se realiza dentro de CreateFactura.
    //
    // NOTA: AceptaFactura ya crea el registro de tipo TipoDocumentoId.ConfirmacionAceptacionMensajeReceptor
    // (Factura + Gasto + CXP) y genera/valida su documento electrónico contra Hacienda mediante CreateDocument.
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


        /// <summary>
        /// "Elimina" una factura electrónica generando su respectiva Nota de Crédito Electrónica
        /// (anulación), copiando el encabezado y el detalle de la factura original y generando/
        /// validando el documento electrónico correspondiente contra Hacienda.
        /// La factura original NO se borra físicamente; queda referenciada por la nota de crédito
        /// y su estado se marca como anulada.
        /// </summary>
        [HttpDelete]
        [Authorize]
        [Route("api/v1/facturas/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioFacturacion)]
        public Reply DeleteFactura(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (id <= 0)
                {
                    throw new Exception("invalid_value_for_id");
                }

                Models.Facturas original;
                Models.Facturas notaCredito;
                List<Models.Factura_Detalles> detallesOriginal;

                using (var ctx = new Models.EntitiesModel())
                {
                    original = ctx.Facturas.FirstOrDefault(u => u.id == id);
                    if (original == null)
                    {
                        throw new Exception("factura_not_found");
                    }
                    if (original.Tipo_documento_id != (int)TipoDocumentoId.FacturaElectronica)
                    {
                        throw new Exception("solo_se_pueden_anular_facturas_electronicas");
                    }

                    detallesOriginal = ctx.Factura_Detalles.Where(d => d.Facturas_id == id).ToList();
                    if (detallesOriginal.Count == 0)
                    {
                        throw new Exception("factura_sin_detalle");
                    }

                    var llaves = ObtenerClaveYConsecutivo(ctx, TipoDocumentoId.NotaCreditoElectronica);

                    int siguienteConsecutivo = ctx.Facturas
                        .Where(x => x.Tipo_documento_id == (int)TipoDocumentoId.NotaCreditoElectronica)
                        .Select(x => x.consecutivo)
                        .DefaultIfEmpty(0)
                        .Max() + 1;

                    notaCredito = new Models.Facturas()
                    {
                        Clave = llaves.Clave,
                        Consecutivo_electronico = llaves.Consecutivo,
                        fecha = DateTime.Now,
                        consecutivo = siguienteConsecutivo,
                        Tipo_moneda_id = original.Tipo_moneda_id,
                        Estado_Factura_id = (int)EstadoFactura.Borrador,
                        Tipo_documento_id = (int)TipoDocumentoId.NotaCreditoElectronica,
                        Subtotal = original.Subtotal,
                        Impuesto = original.Impuesto,
                        Total = original.Total,
                        Descuento = original.Descuento,
                        cambio_venta = original.cambio_venta,
                        cambio_compra = original.cambio_compra,
                        Clientes_id = original.Clientes_id,
                        Condicion_venta_id = original.Condicion_venta_id,
                        Medio_pago_id = original.Medio_pago_id,
                        Usuarios_Usuario_id = original.Usuarios_Usuario_id
                    };
                    ctx.Facturas.Add(notaCredito);
                    ctx.SaveChanges();

                    FacturaDetallesController factDetalles = new FacturaDetallesController();
                    foreach (var d in detallesOriginal)
                    {
                        var copia = new Models.Factura_Detalles()
                        {
                            Facturas_id = notaCredito.id,
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
                            Fecha = DateTime.Now,
                            Ultima_fec_actualizacion = DateTime.Now
                        };
                        var result = factDetalles.CreateFacturaDetalle(copia);
                        if (result.CodeStatus != HttpStatusCode.OK)
                        {
                            throw new Exception(result.Message);
                        }
                    }

                    // Nota: el estado de Hacienda de la factura original se conserva tal cual;
                    // la anulación queda representada por la Nota de Crédito que la referencia.
                    ctx.SaveChanges();
                }

                var referencias = new[]
                {
                    new Facturacion_C_Sharp.Lib.DocumentoItems.Referencia(
                        Facturacion_C_Sharp.Lib.Documento.TipoDocumento.Factura_Electronica,
                        original.Clave,
                        original.fecha,
                        Facturacion_C_Sharp.Lib.DocumentoItems.Referencia.CodigoReferencia.Anula_Documento_de_referencia,
                        "Anulación de factura por eliminación")
                };

                CreateDocument(notaCredito.id, TipoDocumentoId.NotaCreditoElectronica, referencias);

                oR.CodeStatus = HttpStatusCode.OK;
                oR.Message = "nota_credito_generada";
                oR.Data = notaCredito.id;
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


                if ((model.Factura_DetalleAgregados == null || model.Factura_DetalleAgregados.Count == 0) &&
                    (model.Factura_DetalleEliminados == null || model.Factura_DetalleEliminados.Count == 0))
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
                    f.Estado_Factura_id = model.Estado_Factura_id == 0 ? 1 : model.Estado_Factura_id;
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
                  //  throw new Exception(response.Message);
                }

                int? notaCreditoId = null;
                int? notaDebitoId = null;

                // Líneas eliminadas de la factura -> Nota de Crédito Electrónica (rebaja lo facturado de más)
                if (model.Factura_DetalleEliminados != null && model.Factura_DetalleEliminados.Count > 0)
                {
                    notaCreditoId = CrearNotaAjuste(f, model.Factura_DetalleEliminados, TipoDocumentoId.NotaCreditoElectronica,
                        Facturacion_C_Sharp.Lib.DocumentoItems.Referencia.CodigoReferencia.Corrige_monto,
                        "Ajuste por líneas eliminadas de la factura", model.Usuarios_Usuario_id);
                }

                // Líneas nuevas agregadas a la factura -> Nota de Débito Electrónica (cobra lo facturado de menos)
                if (model.Factura_DetalleAgregados != null && model.Factura_DetalleAgregados.Count > 0)
                {
                    notaDebitoId = CrearNotaAjuste(f, model.Factura_DetalleAgregados, TipoDocumentoId.NotaDebitoElectronica,
                        Facturacion_C_Sharp.Lib.DocumentoItems.Referencia.CodigoReferencia.Corrige_monto,
                        "Ajuste por líneas agregadas a la factura", model.Usuarios_Usuario_id);
                }

                oR.CodeStatus = HttpStatusCode.OK;
                oR.Data = new { factura_id = id, nota_credito_id = notaCreditoId, nota_debito_id = notaDebitoId };
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


        /// <summary>
        /// Crea una Nota de Crédito o Nota de Débito Electrónica (según tipoDocumento) a partir de
        /// un conjunto de líneas de detalle (agregadas o eliminadas al actualizar una factura),
        /// referenciando la factura original, y genera/valida su documento electrónico con Hacienda.
        /// Devuelve el id de la nota creada.
        /// </summary>
        private int CrearNotaAjuste(Models.Facturas facturaOriginal, List<Models.FacturaDetallesViewModel> lineas,
            TipoDocumentoId tipoDocumento, Facturacion_C_Sharp.Lib.DocumentoItems.Referencia.CodigoReferencia codigoReferencia,
            string razonReferencia, int? usuarioId)
        {
            Models.Facturas nota;
            using (var ctx = new Models.EntitiesModel())
            {
                var llaves = ObtenerClaveYConsecutivo(ctx, tipoDocumento);

                int siguienteConsecutivo = ctx.Facturas
                    .Where(x => x.Tipo_documento_id == (int)tipoDocumento)
                    .Select(x => x.consecutivo)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                double subtotal = lineas.Sum(l => l.Subtotal);
                double impuesto = lineas.Sum(l => l.Impuesto);
                double total = lineas.Sum(l => l.Total);
                double descuento = lineas.Sum(l => l.Descuento);

                nota = new Models.Facturas()
                {
                    Clave = llaves.Clave,
                    Consecutivo_electronico = llaves.Consecutivo,
                    fecha = DateTime.Now,
                    consecutivo = siguienteConsecutivo,
                    Tipo_moneda_id = facturaOriginal.Tipo_moneda_id,
                    Estado_Factura_id = (int)EstadoFactura.Borrador,
                    Tipo_documento_id = (int)tipoDocumento,
                    Subtotal = subtotal,
                    Impuesto = impuesto,
                    Total = total,
                    Descuento = descuento,
                    cambio_venta = facturaOriginal.cambio_venta,
                    cambio_compra = facturaOriginal.cambio_compra,
                    Clientes_id = facturaOriginal.Clientes_id,
                    Condicion_venta_id = facturaOriginal.Condicion_venta_id,
                    Medio_pago_id = facturaOriginal.Medio_pago_id,
                    Usuarios_Usuario_id = usuarioId
                };
                ctx.Facturas.Add(nota);
                ctx.SaveChanges();

                FacturaDetallesController factDetalles = new FacturaDetallesController();
                foreach (var linea in lineas)
                {
                    var copia = new Models.Factura_Detalles()
                    {
                        Facturas_id = nota.id,
                        Subtotal = linea.Subtotal,
                        Impuesto = linea.Impuesto,
                        Total = linea.Total,
                        Cantidad = linea.Cantidad,
                        Detalle = linea.Detalle,
                        Codigos_cabys_id = linea.Codigos_cabys_id,
                        Codigos_cabys_codigo = linea.Codigos_cabys_codigo,
                        Codigos_cabys_Impuesto_id = linea.Codigos_cabys_Impuesto_id,
                        Descuento = linea.Descuento,
                        Unidad_medida_id = linea.Unidad_medida_id,
                        Codigo_comercial_id = linea.Codigo_comercial_id,
                        Fecha = DateTime.Now,
                        Ultima_fec_actualizacion = DateTime.Now
                    };
                    var result = factDetalles.CreateFacturaDetalle(copia);
                    if (result.CodeStatus != HttpStatusCode.OK)
                    {
                        throw new Exception(result.Message);
                    }
                }
            }

            var referencias = new[]
            {
                new Facturacion_C_Sharp.Lib.DocumentoItems.Referencia(
                    Facturacion_C_Sharp.Lib.Documento.TipoDocumento.Factura_Electronica,
                    facturaOriginal.Clave,
                    facturaOriginal.fecha,
                    codigoReferencia,
                    razonReferencia)
            };

            CreateDocument(nota.id, tipoDocumento, referencias);

            return nota.id;
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
                                     Medio_pago = mp.descripcion,
                                     Simbolo = tm.Simbolo
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
        [Route("api/v1/notacredito")]
        public Reply GetNotaCredito()
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
                                 where f.Tipo_documento_id == (int)TipoDocumentoId.NotaCreditoElectronica //filtramos solo las facturas
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
        [Route("api/v1/notadebito")]
        public Reply GetNotaDebito()
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
                                 where f.Tipo_documento_id == (int)TipoDocumentoId.NotaDebitoElectronica //filtramos solo las facturas
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

        
        internal ClaveViewModel ObtenerClaveYConsecutivo(Models.EntitiesModel ctx, TipoDocumentoId tipoDocumento)
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
  
    
        /// <summary>
        /// Consulta directamente contra Hacienda el estado actual de un documento electrónico dada
        /// su Clave numérica (53 dígitos), guarda la respuesta XML recibida y actualiza el estado
        /// de la factura/nota en la base de datos.
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("api/v1/facturas/estado/{clave}")]
        public Reply GetEstadoDocumentoHacienda(string clave)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (!tool.ValidaTexto(clave))
                {
                    throw new Exception("invalid_string_form_clave");
                }

                Models.Facturas f;
                Models.EmpresaViewModel empresaEmi;
                using (var ctx = new Models.EntitiesModel())
                {
                    f = ctx.Facturas.FirstOrDefault(x => x.Clave == clave);
                    if (f == null)
                    {
                        throw new Exception("factura_not_found_for_clave");
                    }

                    empresaEmi = ctx.Empresa
                        .Where(u => u.Emp_id == 1)
                        .Select(u => new Models.EmpresaViewModel
                        {
                            Nombre_empresa = u.Nombre_empresa,
                            Ruta_nas = u.Ruta_nas,
                            Ruta_llave_factura = u.Ruta_llave_factura,
                            pin_llave = u.pin_llave,
                            Usuario_hacienda = u.Usuario_hacienda,
                            Contrasena_hacienda = u.Contrasena_hacienda
                        }).FirstOrDefault();

                    if (empresaEmi == null)
                    {
                        throw new Exception("empresa_not_found");
                    }
                }

                var usuarioH = empresaEmi.Usuario_hacienda;
                var contraH = tool.Desencriptar(empresaEmi.Contrasena_hacienda);
                var config = new Configuracion(usuarioH, contraH, empresaEmi.Ruta_llave_factura, tool.Desencriptar(empresaEmi.pin_llave));
                var FH = new FacturacionHacienda(config);

                var estado = FH.EstadoDocumento(clave);

                // Guardamos la respuesta XML de Hacienda (cuando exista) y la registramos como adjunto
                if (estado.RepuestaXML != null)
                {
                    string rutaEstados = empresaEmi.Ruta_nas + "/Documentos_Electronicos/Estados/";
                    FH.GuardarXMLEstado(estado, rutaEstados);
                    string rutaArchivo = rutaEstados + clave + ".xml";
                    if (System.IO.File.Exists(rutaArchivo))
                    {
                        saveXMLFIle(rutaArchivo, f.id, f.Usuarios_Usuario_id ?? 0, TablasReferencia.Facturas);
                    }
                }

                int newStatus;
                switch (estado.EstadoEnHacienda)
                {
                    case Facturacion_C_Sharp.Lib.EstadoDocumento.ACEPTADO:
                        newStatus = (int)EstadoFactura.AceptadoPorHacienda;
                        break;
                    case Facturacion_C_Sharp.Lib.EstadoDocumento.PROCESANDO:
                        newStatus = (int)EstadoFactura.PendienteProcesarHacienda;
                        break;
                    case Facturacion_C_Sharp.Lib.EstadoDocumento.RECHAZADO:
                        newStatus = (int)EstadoFactura.RechazadoPorHacienda;
                        break;
                    case Facturacion_C_Sharp.Lib.EstadoDocumento.RECIBIDO:
                        newStatus = (int)EstadoFactura.RecibidoHacienda;
                        break;
                    default:
                        newStatus = (int)EstadoFactura.Error;
                        break;
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Facturas fupdate = ctx.Facturas.FirstOrDefault(x => x.Clave == clave);
                    if (fupdate != null)
                    {
                        fupdate.Estado_Factura_id = newStatus;
                        ctx.SaveChanges();
                    }
                }

                oR.CodeStatus = HttpStatusCode.OK;
                oR.Data = new
                {
                    clave = estado.ClaveNumerica,
                    estado_hacienda = estado.EstadoEnHacienda,
                    mensaje_hacienda = estado.MensajeHacienda,
                    fecha = estado.Fecha,
                    estado_factura_id = newStatus
                };
                return oR;
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }


        internal Boolean CreateDocument(int id, TipoDocumentoId tipoDocumento, Facturacion_C_Sharp.Lib.DocumentoItems.Referencia[] referencias = null)
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
                             Cliente_Correo = c.correo,
                             Usuarios_Usuario_id = (int)x.Usuarios_Usuario_id
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
                                      Canton_emisor = canton.codigo.ToString().Substring(canton.codigo.Length - 2),
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

                // ── Emisor
                var cedulaEmi = new DocumentoIdentificacion(empresaEmi.Tipo_Identificacion, empresaEmi.identificacion);
                var telefonoEmi = new TelefonoBase(empresaEmi.Codigo_Telefono, empresaEmi.Telefono);
                var ubicacionEmi = new Ubicacion(empresaEmi.Provincia_emisor, empresaEmi.Canton_emisor, empresaEmi.Distrito_emisor, empresaEmi.OtrasSenas_Emisor);
                var emisor = new Emisor(empresaEmi.Nombre_empresa, cedulaEmi, ubicacionEmi, empresaEmi.Correo_empresa, telefonoEmi);

                // ── Receptor
                var cedula = new DocumentoIdentificacion(f.Tipo_identificacion, f.Cliente_cedula);
                var telefono = new TelefonoBase(f.Telefono_codigo_pais, f.Telefono_numero);
                var ubicacion = new Ubicacion(f.Cliente_Provincia,
                    f.Cliente_Canton.ToString().Substring(f.Cliente_Canton.Length - 2),
                    f.Cliente_distrito,
                    f.Cliente_OtrasSenas);
                var receptor = new Receptor(f.Cliente, cedula, "", f.Cliente, ubicacion, telefono, null, f.Cliente_Correo);

                // ── Items con campos v4.4
                var items = new List<Item>();
                int index = 1;
                decimal totalImpuestoAcumulado = 0;

                foreach (var row in detalle)
                {
                    decimal precioUnitario = (decimal)(row.Subtotal / row.Cantidad);
                    decimal montoTotalLinea = (decimal)row.Subtotal;
                    decimal subTotalLinea = (decimal)row.Subtotal - (decimal)row.Descuento;
                    decimal baseImponible = subTotalLinea;
                    decimal montoImpuesto = (decimal)row.Impuesto;

                    var taxLine = row.Impuesto_detalle;

                    // Impuesto con CodigoTarifaIVA (v4.4)
                    var tax = new Facturacion_C_Sharp.Lib.DocumentoItems.Impuesto(
                        taxLine.codigo,                      
                        (decimal)taxLine.Porcentaje,
                        montoImpuesto,
                        codigoTarifaIVA: Facturacion_C_Sharp.Lib.DocumentoItems.Impuesto.TarifaToCodigoTarifaIVA((decimal)taxLine.Porcentaje)
                    );

                    // ImpuestoNeto = Monto impuesto - exoneraciones
                    decimal impuestoNeto = montoImpuesto;

                    totalImpuestoAcumulado += montoImpuesto;

                    // Item con CodigoCABYS y BaseImponible (v4.4)
                    items.Add(new Item(
                        numeroLinea: index,
                        codigoCabys: row.Codigos_cabys_codigo ?? "",
                        cantidad: row.Cantidad,
                        unidadMedida: row.Unidad_medida,
                        detalle: row.Detalle,
                        precioUnitario: precioUnitario,
                        montoTotal: montoTotalLinea,
                        subTotal: subTotalLinea,
                        montoTotalLinea: (decimal)row.Total,
                        baseImponible: baseImponible,
                        impuestoNeto: impuestoNeto,
                        codigosComerciales: !string.IsNullOrEmpty(row.Codigo_comercial_id.ToString()) && row.Codigo_comercial_id > 0
                            ? new string[] { row.Codigo_comercial_id.ToString() }
                            : null,
                        tipoCodigoComercial: "04",
                        descuento: (decimal)row.Descuento,
                        codigoDescuento: "07",
                        naturalezaDescuento: row.Descuento > 0 ? "Descuento comercial" : "",
                        impuestos: new Facturacion_C_Sharp.Lib.DocumentoItems.Impuesto[] { tax }
                    ));
                    index++;
                }
                // ── ResumenFactura con estructura v4.4 ──────────────────────────────
                decimal totalVenta = (decimal)(f.Subtotal);
                decimal totalDescuentos = (decimal)(f.Descuento );
                decimal totalVentaNeta = totalVenta - totalDescuentos;
                decimal totalImpuesto = (decimal)(f.Impuesto );
                decimal totalOtrosCargos = 0m;
                decimal totalIVADevuelto = 0m;

                // TotalComprobante = TotalVentaNeta + TotalImpuesto + TotalOtrosCargos - TotalIVADevuelto
                decimal totalComprobante = totalVentaNeta + totalImpuesto + totalOtrosCargos - totalIVADevuelto;

                // Moneda: CodigoTipoMoneda es obligatorio, nunca debe quedar vacío
                string moneda = string.IsNullOrWhiteSpace(f.Tipo_moneda) ? "CRC" : f.Tipo_moneda.Trim().ToUpper();
                decimal tipoCambio = moneda == "CRC"
                    ? 1m
                    : (f.cambio_compra > 0 ? (decimal)f.cambio_compra : 1m);

                // Desglose de impuestos (obligatorio de facto cuando TotalImpuesto > 0).
                // Codigo "01" = IVA; CodigoTarifaIVA "08" = tarifa general 13%.
                var desglose = new List<DesgloseImpuestoResumen>();
                if (totalImpuesto > 0)
                {
                    desglose.Add(new DesgloseImpuestoResumen(
                        codigo: "01",
                        totalMontoImpuesto: totalImpuesto,
                        codigoTarifaIVA: "08"));
                }

                // Medios de pago: máx 4; si son varios, TotalMedioPago es obligatorio
                var medios = new List<MedioPagoResumen>();
                if (!string.IsNullOrWhiteSpace(f.Medio_pago))
                    medios.Add(new MedioPagoResumen(f.Medio_pago.Trim().PadLeft(2, '0')));
                else
                    medios.Add(new MedioPagoResumen("01")); // Efectivo por defecto

                var resumenFac = new ResumenFactura(
                    codigoMoneda: moneda,
                    tipoCambio: tipoCambio,
                    totalMercanciasGravadas: totalVenta,
                    totalGravado: totalVenta,
                    totalVenta: totalVenta,
                    totalDescuentos: totalDescuentos,
                    totalVentaNeta: totalVentaNeta,
                    totalImpuesto: totalImpuesto,
                    totalOtrosCargos: totalOtrosCargos,
                    totalIVADevuelto: totalIVADevuelto,
                    totalComprobante: totalComprobante,
                    desgloseImpuestos: desglose,
                    mediosPago: medios
                );
                // ── Tipo de documento y ruta
                String rutaGuardado = empresaEmi.Ruta_nas;
                Documento.TipoDocumento tipoDoc;

                switch (tipoDocumento)
                {
                    case TipoDocumentoId.FacturaElectronica:
                        rutaGuardado += @"\Documentos_Electronicos\Facturas\";
                        tipoDoc = Documento.TipoDocumento.Factura_Electronica;
                        break;
                    case TipoDocumentoId.NotaCreditoElectronica:
                        rutaGuardado += @"\Documentos_Electronicos\Nota_Credito\";
                        tipoDoc = Documento.TipoDocumento.Nota_de_crédito;
                        break;
                    case TipoDocumentoId.NotaDebitoElectronica:
                        rutaGuardado += @"\Documentos_Electronicos\Nota_Debito\";
                        tipoDoc = Documento.TipoDocumento.Nota_de_débito;
                        break;
                    case TipoDocumentoId.FacturaElectronicaCompra:
                        rutaGuardado += @"\Documentos_Electronicos\FECompra\";
                        tipoDoc = Documento.TipoDocumento.Factura_Electronica_Compra;
                        break;
                    case TipoDocumentoId.ConfirmacionAceptacionMensajeReceptor:
                        rutaGuardado += @"\Documentos_Electronicos\Aceptacion\";
                        tipoDoc = Documento.TipoDocumento.Aceptación_del_comprobante_electrónico;
                        break;
                    default:
                        rutaGuardado += @"\Documentos_Electronicos\Facturas\";
                        tipoDoc = Documento.TipoDocumento.Factura_Electronica;
                        break;
                }

                // ── Crear documento con ProveedorSistemas y CodigoActividadEmisor (v4.4)
                var factura = new Documento(
                    DateTime.Now,
                    emisor,
                    Documento.CondicionVenta.Contado,
                    f.Medio_pago,
                    tipoDoc,
                    items.ToArray(),
                    resumenFac,
                    Documento.SituacionDocumento.Normal,
                    f.Clave,
                    f.Consecutivo_electronico,
                    receptor,
                    referencias: referencias
                    //codigoActividadEmisor: empresaEmi.codigo_actividad_id.ToString().PadLeft(6, '0')
                );

                // ── Firmar
                factura.FirmarDocumento(config);

                // ── Guardar XML
                FH.GuardarXMLEnviado(factura, rutaGuardado);
                saveXMLFIle(rutaGuardado + f.Clave + ".xml", id, f.Usuarios_Usuario_id, TablasReferencia.Facturas);

                // ── Enviar a Hacienda
                var esEnviado = FH.EnviarDocumento(factura);

                if (esEnviado)
                {
                    System.Threading.Thread.Sleep(2500);

                    var estado = FH.EstadoDocumento(f.Clave);

                    // Guardar XML de respuesta
                    string rutaXmlRespuesta = null;
                    if (estado.RepuestaXML != null)
                    {
                        string rutaEstados = rutaGuardado + "Respuestas/";
                        FH.GuardarXMLEstado(estado, rutaEstados);
                        rutaXmlRespuesta = rutaEstados + estado.ClaveNumerica + ".xml";
                        if (System.IO.File.Exists(rutaXmlRespuesta))
                        {
                            saveXMLFIle(rutaXmlRespuesta, id, f.Usuarios_Usuario_id, TablasReferencia.Facturas);
                        }
                    }

                    // Actualizar estado en BD
                    using (var ctx = new Models.EntitiesModel())
                    {
                        Models.Facturas fupdate = ctx.Facturas.FirstOrDefault(u => u.id == id);
                        if (fupdate == null)
                        {
                            throw new Exception("factura_not_found");
                        }

                        int newStatus;
                        switch (estado.EstadoEnHacienda)
                        {
                            case Facturacion_C_Sharp.Lib.EstadoDocumento.ACEPTADO:
                                newStatus = (int)EstadoFactura.AceptadoPorHacienda;
                                break;
                            case Facturacion_C_Sharp.Lib.EstadoDocumento.PROCESANDO:
                                newStatus = (int)EstadoFactura.PendienteProcesarHacienda;
                                break;
                            case Facturacion_C_Sharp.Lib.EstadoDocumento.RECHAZADO:
                                newStatus = (int)EstadoFactura.RechazadoPorHacienda;
                                break;
                            case Facturacion_C_Sharp.Lib.EstadoDocumento.RECIBIDO:
                                newStatus = (int)EstadoFactura.RecibidoHacienda;
                                break;
                            default:
                                newStatus = (int)EstadoFactura.Error;
                                break;
                        }
                        fupdate.Estado_Factura_id = newStatus;
                        ctx.SaveChanges();
                    }

                    // Generar PDF y enviar correo
                    if (tipoDocumento == TipoDocumentoId.FacturaElectronica ||
                        tipoDocumento == TipoDocumentoId.NotaCreditoElectronica ||
                        tipoDocumento == TipoDocumentoId.NotaDebitoElectronica)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(f.Cliente_Correo))
                            {
                                string rutaPdf = rutaGuardado + f.Clave + ".pdf";
                                GenerarPdfFactura(f, detalle, rutaPdf);

                                var adjuntos = new List<string> { rutaGuardado + f.Clave + ".xml", rutaPdf };
                                if (!string.IsNullOrEmpty(rutaXmlRespuesta))
                                {
                                    adjuntos.Add(rutaXmlRespuesta);
                                }

                                string tituloDoc = tipoDocumento == TipoDocumentoId.FacturaElectronica ? "Factura Electrónica"
                                                 : tipoDocumento == TipoDocumentoId.NotaCreditoElectronica ? "Nota de Crédito Electrónica"
                                                 : "Nota de Débito Electrónica";

                                string asunto = $"{tituloDoc} {f.Consecutivo_electronico} - {empresaEmi.Nombre_empresa}";
                                string cuerpo = $@"
                                <h2>{tituloDoc}</h2>
                                <p>Estimado(a) {f.Cliente},</p>
                                <p>Adjunto encontrará el comprobante electrónico generado por <strong>{empresaEmi.Nombre_empresa}</strong>.</p>
                                <table style='border-collapse:collapse; width:100%; max-width:450px;'>
                                    <tr style='background-color:#f8f9fa;'>
                                        <td style='padding:10px; border:1px solid #dee2e6;'><strong>Clave</strong></td>
                                        <td style='padding:10px; border:1px solid #dee2e6;'>{f.Clave}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding:10px; border:1px solid #dee2e6;'><strong>Consecutivo</strong></td>
                                        <td style='padding:10px; border:1px solid #dee2e6;'>{f.Consecutivo_electronico}</td>
                                    </tr>
                                    <tr style='background-color:#f8f9fa;'>
                                        <td style='padding:10px; border:1px solid #dee2e6;'><strong>Total</strong></td>
                                        <td style='padding:10px; border:1px solid #dee2e6;'>{f.Tipo_moneda} {f.Total:N2}</td>
                                    </tr>
                                </table>
                                <hr/>
                                <small style='color:#6c757d;'>Notificación automática - Marsh Asprose</small>";

                                tool.Send_Mail(f.Cliente_Correo, asunto, cuerpo, adjuntos);
                            }
                        }
                        catch
                        {
                            // No revertir documento electrónico por fallo de correo
                        }
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Genera un PDF simple y autocontenido (sin dependencias externas de terceros) con el
        /// resumen de la factura/nota electrónica, para adjuntarlo en el correo enviado al cliente.
        /// </summary>
        private void GenerarPdfFactura(FacturasViewModel f, List<FacturaDetallesViewModel> detalle, string rutaSalida)
        {
            var lineas = new List<string>();
            lineas.Add("Comprobante Electronico");
            lineas.Add("Clave: " + f.Clave);
            lineas.Add("Consecutivo: " + f.Consecutivo_electronico);
            lineas.Add("Fecha: " + f.fecha.ToString("dd/MM/yyyy HH:mm"));
            lineas.Add("Cliente: " + f.Cliente);
            lineas.Add("Cedula: " + f.Cliente_cedula);
            lineas.Add(" ");
            lineas.Add("Detalle:");
            foreach (var d in detalle)
            {
                lineas.Add(string.Format("  {0} x {1}  Subtotal: {2:N2}  Imp: {3:N2}  Total: {4:N2}",
                    d.Cantidad, d.Detalle, d.Subtotal, d.Impuesto, d.Total));
            }
            lineas.Add(" ");
            lineas.Add(string.Format("Subtotal: {0:N2}", f.Subtotal));
            lineas.Add(string.Format("Descuento: {0:N2}", f.Descuento));
            lineas.Add(string.Format("Impuesto: {0:N2}", f.Impuesto));
            lineas.Add(string.Format("Total: {0} {1:N2}", f.Tipo_moneda, f.Total));

            Modulos.PdfSimpleWriter.Generar(lineas, rutaSalida);
        }



        private bool saveXMLFIle(string rutaArchivo, int  id = 0, int uid = 1, TablasReferencia tabla = TablasReferencia.Facturas)
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

                        int codigo_actividad_id = ctx.codigo_actividad
    .Select(x => x.id)
    .Min();
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
                            codigo_actividad_id = codigo_actividad_id,
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

                        var cabys_info = ctx.Codigos_cabys.FirstOrDefault(x => x.codigo == linea.codigoCabys);

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
                            Ultima_fec_actualizacion = DateTime.Now,
                            Codigos_cabys_id = cabys_info.id,
                            Codigos_cabys_codigo = cabys_info.codigo,
                            Codigos_cabys_Impuesto_id = cabys_info.Impuesto_id,
                            Unidad_medida_id = 1, //default de UNIDAD
                            Codigo_comercial_id = 1 //default de Código del producto del vendedor
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

                            var resultGD = gastosDetalles.CreateGastoDetalle(gd);
                            if (resultGD.CodeStatus != HttpStatusCode.OK)
                                throw new Exception(resultGD.Message);
                        }

                        BancoController banco = new BancoController();
                        var bmovimiento = banco.RegistrarMovimientoPorGasto(cpid, tipoMonedaId, ccid, gastoId, g.Total, g.Usuarios_Usuario_id, "Registro de Gasto");

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
