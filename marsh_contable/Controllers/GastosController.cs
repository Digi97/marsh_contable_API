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
   public class GastosController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/gastos")]
        [RequierePermiso(PermisosAplica.UsuarioGastosFacturas)]
        public Reply CreateGasto([FromBody] Models.Gastos model)
        {
            int id = 0;
            Models.Gestion_Presupuestaria gpExist;
            Models.Gastos g;
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null)
                {
                    throw new Exception("invalid_model_request_missing");
                }
                if (!tool.ValidaTexto(model.Descripcion))
                {
                    throw new Exception("invalid_string_form_Descripcion");
                }
                if (!tool.validaNumeros(model.Categoria_gasto_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Categoria_gasto_id");
                }
                if (!tool.validaNumeros(model.Tipo_documento_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Tipo_documento_id");
                }
                if (!tool.validaNumeros(model.Medio_pago_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Medio_pago_id");
                }
                if (!tool.validaNumeros(model.Proveedor_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Proveedor_id");
                }
                if (!tool.ValidaTexto(model.Doc_Referencia))
                {
                    throw new Exception("invalid_string_form_Doc_Referencia");
                }


                if (model.Gastos_Detalles.Count == 0)
                {
                    throw new Exception("detail_is_required");
                }


                if (model.Tipo_moneda_id == 0)
                {
                    throw new Exception("currency_is_required");
                }

                if( String.IsNullOrEmpty(model.presupuesto_id) )
                {
                    throw new Exception("presupuesto_not defined");
                }

       
                BancoController banco = new BancoController();

                string[] partes = model.presupuesto_id.Split('_'); // id = gp.id+"_"+gp.Categoria_presupuestaria_id+"_"+ gp.Centro_Costos_id,

                int pid = int.Parse(partes[0]);
                int cpid = int.Parse(partes[1]);
                int ccid = int.Parse(partes[2]);

                validacionPresupuesto(pid, model.Total, cpid, ccid); //validamos el presupuesto

                using (var ctx = new Models.EntitiesModel())
                {

                    DateTime currentDate = DateTime.Now;

                    gpExist = ctx.Gestion_Presupuestaria.FirstOrDefault(u => currentDate >= u.periodo_inicio && currentDate <= u.periodo_fin && u.id == pid);
                    if (gpExist == null)
                    {
                        throw new Exception("gestion_presupuestaria_for_current_period_dont_exist");
                    }


                    g = new Models.Gastos()
                    {
                        Descripcion = model.Descripcion,
                        Categoria_gasto_id = model.Categoria_gasto_id,
                        Subtotal = model.Subtotal,
                        Impuesto = model.Impuesto,
                        Total = model.Total,
                        Doc_Referencia = model.Doc_Referencia,
                        Fecha = DateTime.Now,
                        Ultima_Fec_Actualizacion = DateTime.Now,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Tipo_documento_id = model.Tipo_documento_id,
                        Medio_pago_id = model.Medio_pago_id,
                        Proveedor_id = model.Proveedor_id,
                        Descuento = model.Descuento,
                        Tipo_moneda_id = (int)model.Tipo_moneda_id
                    };
                    ctx.Gastos.Add(g);
                    ctx.SaveChanges();


                    gpExist.monto_ejecutado = gpExist.monto_ejecutado + model.Total;
                    ctx.SaveChanges();


                    if(model.Tipo_documento_id == (int)TipoDocumentoId.FacturaElectronicaCompra)
                    {
                        //TODO: CREAR REGISTRO EN FACTURA Y XML 
                    }
                    //actualizamos el monto ejecutado para los reportes
                    id = g.id;
                    GastosDetallesController gastosDetalles = new GastosDetallesController();
                    foreach (var detalles in model.Gastos_Detalles)
                    {
                        detalles.Gastos_id = id;
                        var result = gastosDetalles.CreateGastoDetalle(detalles, ctx);
                        if (result.CodeStatus != HttpStatusCode.OK)
                        {

                            throw new Exception(result.Message);
                        }

                    }



                } 

                Models.Gestion_P_detalle detalle = new Models.Gestion_P_detalle()
                {
                    Monto = g.Total,
                    Monto_aprobado = gpExist.monto_aprobado,
                    Monto_modificado = gpExist.monto_modificado,
                    Monto_compometido = gpExist.monto_comprometido,
                    Monto_ejecutado = (decimal)g.Total,
                    detalle_presupuesto = $"Gastos #{id}",
                    Gestion_Presupuestaria_id = gpExist.id, // ID del presupuesto activo
                    Categoria_presupuestaria_id = (int)Modulos.Categoria_presupuestaria.Gastos,
                    Gastos_id = id,
                    Ingresos_id = null,
                    Facturas_id = null,
                    Usuarios_Usuario_id = (int)model.Usuarios_Usuario_id,
                    Fecha_registro = DateTime.Now,
                    Observaciones = $"Id: {g.id} | Subtotal: {g.Subtotal} | Impuesto: {g.Impuesto} | Descuento: {g.Descuento}",
                    activo = 1
                };



           
                var bmovimiento = banco.RegistrarMovimientoPorGasto(cpid, (int) model.Tipo_moneda_id, ccid, id, g.Total, g.Usuarios_Usuario_id, "Registro de Gasto");

                if (bmovimiento.CodeStatus != HttpStatusCode.OK)
                {
                    throw new Exception(bmovimiento.Message);
                }



                GestionPDetalleController detalleGestion = new GestionPDetalleController();
                var response = detalleGestion.CreateGestionPDetalle(detalle);

                if (response.CodeStatus != HttpStatusCode.OK)
                {
                    throw new Exception(response.Message);
                }

                // Dentro de CreateGasto, si la condición es crédito
                if (model.Condicion_venta_id == (int)CondicionVenta.Credito)
                {
                    using (var ctx = new Models.EntitiesModel())
                    {
                        Models.Cuenta_Encabezado cxp = new Models.Cuenta_Encabezado()
                        {
                            Vigencia_inicial = DateTime.Now,
                            Vigencia_final = DateTime.Now.AddDays(model.dias_credito),
                            Tipo_moneda_id = (int)g.Tipo_moneda_id,
                            Medio_pago_id = g.Medio_pago_id,
                            Total = (decimal)g.Total,
                            Monto_Proyeccion = (decimal)g.Total,
                            subtotal = (decimal)g.Subtotal,
                            impuesto = (decimal)g.Impuesto,
                            Descuento = (decimal)g.Descuento,
                            Referencia = g.Doc_Referencia,
                            Fecha_creacion = DateTime.Now,
                            Ultima_Fecha_actualizacion = DateTime.Now,
                            Usuarios_Usuario_id = g.Usuarios_Usuario_id,
                            Clientes_id = null,
                            Facturas_id = null,
                            Proveedor_id = g.Proveedor_id,
                            Gastos_id = g.id,
                            Ingresos_id = null,
                            Estado = 1,
                            Tipo_cuentas_id = (int)TipoCuenta.CuentaPorPagar,
                            Categoria_presupuestaria_id = gpExist.Categoria_presupuestaria_id,
                            Centro_Costos_id = gpExist.Centro_Costos_id
                        };
                        ctx.Cuenta_Encabezado.Add(cxp);
                        ctx.SaveChanges();
                    }
                
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


        [HttpPut]
        [Authorize]
        [Route("api/v1/gastos/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioGastosFacturas)]
        public Reply UpdateGasto(int id, [FromBody] Models.Gastos model)
        {
            Models.Gestion_Presupuestaria gpExist;
            Models.Gastos g;
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null)
                {
                    throw new Exception("invalid_model_request_missing");
                }
                if (!tool.ValidaTexto(model.Descripcion))
                {
                    throw new Exception("invalid_string_form_Descripcion");
                }
                if (model.Tipo_moneda_id == 0)
                {
                    throw new Exception("currency_is_required");
                }

                if (String.IsNullOrEmpty(model.presupuesto_id))
                {
                    throw new Exception("presupuesto_not defined");
                }


                string[] partes = model.presupuesto_id.Split('_'); // id = gp.id+"_"+gp.Categoria_presupuestaria_id+"_"+ gp.Centro_Costos_id,

                int pid = int.Parse(partes[0]);
                int cpid = int.Parse(partes[1]);
                int ccid = int.Parse(partes[2]);
                validacionPresupuesto(pid, model.Total, cpid, ccid); //validamos el presupuesto


                using (var ctx = new Models.EntitiesModel())
                {


                    DateTime currentDate = DateTime.Now;
                    gpExist = ctx.Gestion_Presupuestaria.FirstOrDefault(u => currentDate >= u.periodo_inicio && currentDate <= u.periodo_fin);
                    if (gpExist == null)
                    {
                        throw new Exception("gestion_presupuestaria_for_current_period_dont_exist");
                    }

                     g = ctx.Gastos.FirstOrDefault(u => u.id == id);
                    if (g == null)
                    {
                        throw new Exception("gasto_not_found");
                    }
                    g.Descripcion = model.Descripcion;
                    g.Categoria_gasto_id = model.Categoria_gasto_id;
                    g.Subtotal = model.Subtotal;
                    g.Impuesto = model.Impuesto;
                    g.Total = model.Total;
                    g.Doc_Referencia = model.Doc_Referencia;
                    g.Tipo_documento_id = model.Tipo_documento_id;
                    g.Medio_pago_id = model.Medio_pago_id;
                    g.Proveedor_id = model.Proveedor_id;
                    g.Ultima_Fec_Actualizacion = DateTime.Now;
                    g.Descuento = model.Descuento;
                    g.Tipo_moneda_id = model.Tipo_moneda_id;
                    g.Condicion_venta_id = model.Condicion_venta_id;
                    
                    ctx.SaveChanges();
                }



                Models.Gestion_P_detalle detalle = new Models.Gestion_P_detalle()
                {
                    Monto = g.Total,
                    Monto_aprobado = gpExist.monto_aprobado,
                    Monto_modificado = gpExist.monto_modificado,
                    Monto_compometido = gpExist.monto_comprometido,
                    Monto_ejecutado = (decimal)g.Total,
                    detalle_presupuesto = $"Gastos #{id}",
                    Gestion_Presupuestaria_id = gpExist.id, // ID del presupuesto activo
                    Categoria_presupuestaria_id = (int)Modulos.Categoria_presupuestaria.Gastos,
                    Gastos_id = id,
                    Ingresos_id = null,
                    Facturas_id = null,
                    Usuarios_Usuario_id = (int)model.Usuarios_Usuario_id,
                    Fecha_registro = DateTime.Now,
                    Observaciones = $"Id: {g.id} | Subtotal: {g.Subtotal} | Impuesto: {g.Impuesto} | Descuento: {g.Descuento}",
                    activo = 1
                };

                BancoController banco = new BancoController();
                var bmovimiento = banco.EditarMovimientoPorGasto(model.Categoria_gasto_id, (int)model.Tipo_moneda_id, gpExist.Centro_Costos_id, id, g.Total, g.Usuarios_Usuario_id, "Registro de Gasto");

                if (bmovimiento.CodeStatus != HttpStatusCode.OK)
                {
                    throw new Exception(bmovimiento.Message);
                }


                GestionPDetalleController detalleGestion = new GestionPDetalleController();
                var response = detalleGestion.UpdateGestionPDetalle(id,detalle, 0);

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
        [Route("api/v1/gastos")]
        public Reply GetAllGastosPaged()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                // Leer query string crudo
                var q = System.Web.HttpContext.Current.Request.QueryString;

                var request = new Models.DataTableRequest
                {
                    Draw = int.TryParse(q["draw"], out var d) ? d : 1,
                    Start = int.TryParse(q["start"], out var s) ? s : 0,
                    Length = int.TryParse(q["length"], out var l) ? l : 25,
                    SearchValue = q["search[value]"],
                    SortDirection = q["order[0][dir]"]
                };

                // El índice de la columna ordenada -> nombre real de la columna
                if (int.TryParse(q["order[0][column]"], out var colIdx))
                {
                    // columns[colIdx][data] trae el nombre que mandó el front (id, codigo, nombre...)
                    request.SortColumn = q[$"columns[{colIdx}][data]"];
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    var query = ctx.Gastos.AsQueryable();

                    if (!string.IsNullOrEmpty(request.SearchValue))
                    {
                        string search = request.SearchValue.ToLower();
                        query = query.Where(x =>
                            x.Descripcion.ToLower().Contains(search) ||
                            x.Doc_Referencia.ToLower().Contains(search) ||
                            x.Total.ToString().Contains(search) ||
                            x.Proveedor.Nombre.ToLower().Contains(search)||
                            x.Proveedor.Apellido1.ToLower().Contains(search) ||
                            x.Proveedor.Apellido2.ToLower().Contains(search) ||
                            x.Categoria_gasto.Nombre.ToLower().Contains(search) ||
                            x.Usuarios.Nombre.ToLower().Contains(search) ||
                            x.Usuarios.Apellido1.ToLower().Contains(search) ||
                            x.Usuarios.Apellido2.ToLower().Contains(search)
                        );
                    }

                    int totalRecords = ctx.Gastos.Count();
                    int totalFiltered = query.Count();

                    switch (request.SortColumn?.ToLower())
                    {
                        case "descripcion":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Descripcion)
                                : query.OrderByDescending(x => x.Descripcion);
                            break;
                        case "total":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Total)
                                : query.OrderByDescending(x => x.Total);
                            break;
                        case "subtotal":   
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Subtotal)
                                : query.OrderByDescending(x => x.Subtotal);
                            break;
                        case "doc_referencia":  
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Doc_Referencia)
                                : query.OrderByDescending(x => x.Doc_Referencia);
                            break;
                        case "fecha":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(x => x.Fecha)
                                : query.OrderByDescending(x => x.Fecha);
                            break;
                       
                        default:
                            query = query.OrderBy(x => x.id);
                            break;
                    }


                    var queryJoined = (from g in ctx.Gastos
                                 join cg in ctx.Categoria_gasto on g.Categoria_gasto_id equals cg.id
                                 join td in ctx.Tipo_documento on g.Tipo_documento_id equals td.id
                                 join mp in ctx.Medio_pago on g.Medio_pago_id equals mp.id
                                 join p in ctx.Proveedor on g.Proveedor_id equals p.id
                                 join u in ctx.Usuarios on g.Usuarios_Usuario_id equals u.Usuario_id
                                 join m in ctx.Tipo_moneda on  g.Tipo_moneda_id equals m.id
                                 select new Models.GastosViewModel
                                 {
                                     id = g.id,
                                     Descripcion = g.Descripcion,
                                     Categoria_gasto_id = g.Categoria_gasto_id,
                                     Subtotal = g.Subtotal,
                                     Impuesto = g.Impuesto,
                                     Total = g.Total,
                                     Doc_Referencia = g.Doc_Referencia,
                                     Fecha = g.Fecha,
                                     Ultima_Fec_Actualizacion = g.Ultima_Fec_Actualizacion,
                                     Usuarios_Usuario_id = g.Usuarios_Usuario_id,
                                     Tipo_documento_id = g.Tipo_documento_id,
                                     Medio_pago_id = g.Medio_pago_id,
                                     Proveedor_id = g.Proveedor_id,
                                     Categoria_gasto = cg.Nombre,
                                     Tipo_documento = td.Nombre,
                                     Medio_pago = mp.descripcion,
                                     Proveedor = p.Nombre + " " + p.Apellido1 + " " +p.Apellido2,
                                     Usuario = u.Nombre + " " + u.Apellido1 + u.Apellido2,
                                     tipo_moneda = m.Simbolo,
                                    

                                 })
                                 .OrderByDescending(x => x.id).ToList();
                   
                    var data = queryJoined
                        .Skip(request.Start)
                        .Take(request.Length > 0 ? request.Length : totalFiltered)
                        .ToList();

                  

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        draw = request.Draw,
                        recordsTotal = totalRecords,
                        recordsFiltered = totalFiltered,
                        data = data
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
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }


        [HttpGet]
        [Authorize]
        [Route("api/v1/gastos/{id}")]
        public Reply GetGastoById(int id)
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
                    var g = (from x in ctx.Gastos
                             join cg in ctx.Categoria_gasto on x.Categoria_gasto_id equals cg.id
                             join td in ctx.Tipo_documento on x.Tipo_documento_id equals td.id
                             join mp in ctx.Medio_pago on x.Medio_pago_id equals mp.id
                             join p in ctx.Proveedor on x.Proveedor_id equals p.id
                             join u in ctx.Usuarios on x.Usuarios_Usuario_id equals u.Usuario_id
                             where x.id == id
                             select new Models.GastosViewModel
                             {
                                 id = x.id,
                                 Descripcion = x.Descripcion,
                                 Categoria_gasto_id = x.Categoria_gasto_id,
                                 Subtotal = x.Subtotal,
                                 Impuesto = x.Impuesto,
                                 Total = x.Total,
                                 Doc_Referencia = x.Doc_Referencia,
                                 Fecha = x.Fecha,
                                 Ultima_Fec_Actualizacion = x.Ultima_Fec_Actualizacion,
                                 Usuarios_Usuario_id = x.Usuarios_Usuario_id,
                                 Tipo_documento_id = x.Tipo_documento_id,
                                 Medio_pago_id = x.Medio_pago_id,
                                 Proveedor_id = x.Proveedor_id,
                                 Categoria_gasto = cg.Nombre,
                                 Tipo_documento = td.Nombre,
                                 Medio_pago = mp.descripcion,
                                 Proveedor = p.Nombre + " " + p.Apellido1,
                                 Usuario = u.Nombre + " " + u.Apellido1,
                                 Tipo_moneda_id = (int)x.Tipo_moneda_id,
                                 condicion_venta_id = (int)x.Condicion_venta_id

                                 
                             }).FirstOrDefault();

                    if (g == null)
                    {
                        throw new Exception("gasto_not_found");
                    }

                    if (g != null)
                    {
                        g.GastosDetalle = ctx.Gastos_Detalles
                            .Where(t => t.Gastos_id == id)
                            .Select(t => new Models.GastosDetallesViewModel
                            {
                                id = t.id,
                                Subtotal = t.Subtotal,
                                Impuesto = t.Impuesto,
                                Total = t.Total,
                                Cantidad = t.Cantidad,
                                Detalle = t.Detalle,
                                Descuento = t.Descuento,
                                codigo_comercial = t.codigo_comercial,

                            }).ToList();
                    }


                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = g;
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
        [Route("api/v1/gastos/proveedor/{proveedorId}")]
        public Reply GetGastosByProveedor(int proveedorId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (proveedorId <= 0)
                {
                    throw new Exception("invalid_value_for_proveedor_id");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var lista = ctx.Gastos.Where(g => g.Proveedor_id == proveedorId)
                        .Select(g => new {
                            g.id,
                            g.Descripcion,
                            g.Total,
                            g.Fecha,
                            g.Doc_Referencia
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


        private bool validacionPresupuesto(int pid = 0, double gtotal = 0, int cpid =0, int ccid = 0)
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

                        if(montoMensual == 0)
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


        /// <summary>
        /// Notifica a usuarios de Rol 1 cuando el presupuesto está entre 90-95% de uso.
        /// </summary>
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
