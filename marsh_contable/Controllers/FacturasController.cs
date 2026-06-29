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
                CreateDocument(id);

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
  
    
        private Boolean CreateDocument(int id)
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


                var factura = new Documento(DateTime.Now, emisor, Documento.CondicionVenta.Contado,
                                            f.Medio_pago, Documento.TipoDocumento.Factura_Electronica,
                                            items.ToArray(),
                                            resumenFac,
                                            Documento.SituacionDocumento.Normal,
                                            f.Clave,
                                            f.Consecutivo_electronico,
                                            receptor);


                factura.FirmarDocumento(config);//firmamos documento para guardarlo

                //var xmlFirmado = FirmadorXML.Firmar(factura, empresaEmi.Ruta_llave_factura, tool.Desencriptar(empresaEmi.pin_llave));
                FH.GuardarXMLEnviado(factura, empresaEmi.Ruta_nas + "/Documentos_Electronicos/");
                saveXMLFIle(empresaEmi.Ruta_nas + "/Documentos_Electronicos/" + f.Clave + ".xml", id);

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



        private bool saveXMLFIle(string rutaArchivo, int  id = 0)
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
                        Usuarios_Usuario_id = 1,//administrador por defecto
                        extension = extension,
                        referencia = id, //id de referencia
                        Tablas_referencia_id = 1, //Facturas
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


        [HttpPost]
        [Authorize]
        [Route("api/v1/aceptafactura")]
        [RequierePermiso(PermisosAplica.UsuarioAceptacionFacturas)]
        public Reply AceptaFactura([FromBody] AceptaFacturaViewModel model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null)
                    throw new Exception("invalid_model_request_missing");

                if (string.IsNullOrEmpty(model.base64Factura))
                    throw new Exception("invalid_base64_factura_missing");

                // ── PASO 1: Decodificar Base64 a texto XML
                string xmlTexto;
                try
                {
                    // Limpiar prefijo data:...;base64, si viene
                    string base64 = model.base64Factura;
                    if (base64.Contains(","))
                        base64 = base64.Split(',')[1];

                    byte[] xmlBytes = Convert.FromBase64String(base64);
                    xmlTexto = System.Text.Encoding.UTF8.GetString(xmlBytes);
                }
                catch
                {
                    throw new Exception("invalid_base64_format");
                }

                // ── PASO 2: Parsear XML
                System.Xml.Linq.XDocument xmlDoc;
                try
                {
                    xmlDoc = System.Xml.Linq.XDocument.Parse(xmlTexto);
                }
                catch
                {
                    throw new Exception("invalid_xml_format");
                }

                // ── PASO 3: Namespace de FacturaElectronica v4.4
                System.Xml.Linq.XNamespace ns =
                    "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica";

                var root = xmlDoc.Root;
                if (root == null || root.Name.LocalName != "FacturaElectronica")
                    throw new Exception("invalid_xml_not_factura_electronica");

                // ── PASO 4: Extraer datos del XML
                string clave = root.Element(ns + "Clave")?.Value ?? "";
                string consecutivo = root.Element(ns + "NumeroConsecutivo")?.Value ?? "";
                string fechaEmision = root.Element(ns + "FechaEmision")?.Value ?? "";

                // Emisor (proveedor)
                var emisor = root.Element(ns + "Emisor");
                string emisorNombre = emisor?.Element(ns + "Nombre")?.Value ?? "";
                string emisorTipo = emisor?.Element(ns + "Identificacion")
                                            ?.Element(ns + "Tipo")?.Value ?? "";
                string emisorNumero = emisor?.Element(ns + "Identificacion")
                                            ?.Element(ns + "Numero")?.Value ?? "";

                // Receptor (la empresa)
                var receptor = root.Element(ns + "Receptor");
                string receptorNombre = receptor?.Element(ns + "Nombre")?.Value ?? "";

                // Resumen
                var resumen = root.Element(ns + "ResumenFactura");
                double totalVenta = double.Parse(resumen?.Element(ns + "TotalVenta")?.Value ?? "0",
                    System.Globalization.CultureInfo.InvariantCulture);
                double totalDescuentos = double.Parse(resumen?.Element(ns + "TotalDescuentos")?.Value ?? "0",
                    System.Globalization.CultureInfo.InvariantCulture);
                double totalImpuesto = double.Parse(resumen?.Element(ns + "TotalImpuesto")?.Value ?? "0",
                    System.Globalization.CultureInfo.InvariantCulture);
                double totalComprobante = double.Parse(resumen?.Element(ns + "TotalComprobante")?.Value ?? "0",
                    System.Globalization.CultureInfo.InvariantCulture);

                // Validaciones básicas
                if (string.IsNullOrEmpty(clave))
                    throw new Exception("xml_missing_clave");
                if (string.IsNullOrEmpty(emisorNombre))
                    throw new Exception("xml_missing_emisor");

                // ── PASO 5: Buscar o crear proveedor por identificación
                int proveedorId = 0;
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var proveedor = ctx.Proveedor
                        .FirstOrDefault(p => p.identificacion == emisorNumero);

                    if (proveedor != null)
                    {
                        proveedorId = proveedor.id;
                    }
                    else
                    {
                        // Crear proveedor automáticamente desde datos del XML
                        var nuevoProveedor = new Models.Proveedor()
                        {
                            identificacion = emisorNumero,
                            tipo_identificacion_id = int.Parse(emisorTipo),
                            Nombre = emisorNombre,
                            Apellido1 = "",
                            Apellido2 = "",
                            correo = emisor?.Element(ns + "CorreoElectronico")?.Value ?? "",
                            Distrito_id = 1,
                            Canton_id = 1,
                            Provincia_id = 1,
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
                }

                // ── PASO 6: Guardar como Gasto con detalles
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    Models.Gastos g = new Models.Gastos()
                    {
                        Descripcion = $"Factura {emisorNombre} - Clave: {clave}",
                        Categoria_gasto_id = model.Categoria_gasto_id,
                        Subtotal = totalVenta,
                        Impuesto = totalImpuesto,
                        Total = totalComprobante,
                        Doc_Referencia = clave,
                        Fecha = DateTime.Now,
                        Ultima_Fec_Actualizacion = DateTime.Now,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Tipo_documento_id = (int)TipoDocumentoId.FacturaElectronicaCompra,
                        Medio_pago_id = model.Medio_pago_id,
                        Proveedor_id = proveedorId,
                        Descuento = totalDescuentos,
                        Tipo_moneda_id = model.Tipo_moneda_id
                    };
                    ctx.Gastos.Add(g);
                    ctx.SaveChanges();

                    // ── Extraer líneas de detalle del XML
                    var detalleServicio = root.Element(ns + "DetalleServicio");
                    if (detalleServicio != null)
                    {
                        var lineas = detalleServicio.Elements(ns + "LineaDetalle");
                        GastosDetallesController gastosDetalles = new GastosDetallesController();

                        foreach (var linea in lineas)
                        {
                            double precioUnit = double.Parse(
                                linea.Element(ns + "PrecioUnitario")?.Value ?? "0",
                                System.Globalization.CultureInfo.InvariantCulture);
                            double montoTotal = double.Parse(
                                linea.Element(ns + "MontoTotal")?.Value ?? "0",
                                System.Globalization.CultureInfo.InvariantCulture);
                            double subTotal = double.Parse(
                                linea.Element(ns + "SubTotal")?.Value ?? "0",
                                System.Globalization.CultureInfo.InvariantCulture);
                            double montoTotalLinea = double.Parse(
                                linea.Element(ns + "MontoTotalLinea")?.Value ?? "0",
                                System.Globalization.CultureInfo.InvariantCulture);
                            int cantidad = int.Parse(
                                linea.Element(ns + "Cantidad")?.Value ?? "1");
                            string detalle = linea.Element(ns + "Detalle")?.Value ?? "";

                            // Extraer impuesto de la línea
                            var impuestoNode = linea.Element(ns + "Impuesto");
                            double montoImpuesto = 0;
                            if (impuestoNode != null)
                            {
                                montoImpuesto = double.Parse(
                                    impuestoNode.Element(ns + "Monto")?.Value ?? "0",
                                    System.Globalization.CultureInfo.InvariantCulture);
                            }

                            // Extraer descuento de la línea
                            var descuentoNode = linea.Element(ns + "Descuento");
                            double montoDescuento = 0;
                            if (descuentoNode != null)
                            {
                                montoDescuento = double.Parse(
                                    descuentoNode.Element(ns + "MontoDescuento")?.Value ?? "0",
                                    System.Globalization.CultureInfo.InvariantCulture);
                            }

                            // Código comercial
                            string codigoComercial = "";
                            var codigoComNode = linea.Element(ns + "CodigoComercial");
                            if (codigoComNode != null)
                            {
                                codigoComercial = codigoComNode.Element(ns + "Codigo")?.Value ?? "";
                            }

                            Models.Gastos_Detalles gd = new Models.Gastos_Detalles()
                            {
                                Subtotal = subTotal,
                                Impuesto = montoImpuesto,
                                Total = montoTotalLinea,
                                Cantidad = cantidad,
                                Detalle = detalle,
                                Descuento = montoDescuento,
                                codigo_comercial = codigoComercial,
                                Fecha = DateTime.Now,
                                Ultima_fec_actualizacion = DateTime.Now,
                                Gastos_id = g.id
                            };

                            var result = gastosDetalles.CreateGastoDetalle(gd, ctx);
                            if (result.CodeStatus != HttpStatusCode.OK)
                                throw new Exception(result.Message);
                        }
                    }

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        gasto_id = g.id,
                        clave = clave,
                        consecutivo = consecutivo,
                        emisor_nombre = emisorNombre,
                        emisor_identificacion = emisorNumero,
                        total = totalComprobante,
                        impuesto = totalImpuesto,
                        descuento = totalDescuentos
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
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }

    }
}
