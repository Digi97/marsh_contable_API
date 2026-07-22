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
                #region "Validaciones"
                if (model == null)
                    throw new Exception("invalid_model_request_missing");

                if (!tool.ValidaTexto(model.Referencia))
                    throw new Exception("invalid_string_form_Referencia");

                if (!tool.validaNumeros(model.Tipo_moneda_id.ToString()))
                    throw new Exception("invalid_value_form_Tipo_moneda_id");

                if (!tool.validaNumeros(model.Medio_pago_id.ToString()))
                    throw new Exception("invalid_value_form_Medio_pago_id");

                if (!tool.validaNumeros(model.Tipo_cuentas_id.ToString()))
                    throw new Exception("invalid_value_form_Tipo_cuentas_id");

                if (!tool.validaNumeros(model.Cuentas_Contables_id.ToString()))
                    throw new Exception("invalid_value_form_Cuentas_Contables_id");

                if (!tool.validaNumeros(model.Centro_Costos_id.ToString()))
                    throw new Exception("invalid_value_form_Centro_Costos_id");

                if (model.Vigencia_inicial >= model.Vigencia_final)
                    throw new Exception("vigencia_inicial_must_be_before_vigencia_final");

                // Validar según tipo de cuenta
                if (model.Tipo_cuentas_id == (int)TipoCuenta.CuentaPorCobrar && model.Clientes_id == null)
                    throw new Exception("clientes_id_required_for_cxc");

                if (model.Tipo_cuentas_id == (int)TipoCuenta.CuentaPorPagar && model.Proveedor_id == null)
                    throw new Exception("proveedor_id_required_for_cxp");

                #endregion
                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuenta_Encabezado ce = new Models.Cuenta_Encabezado()
                    {
                        Vigencia_inicial           = model.Vigencia_inicial,
                        Vigencia_final             = model.Vigencia_final,
                        Tipo_moneda_id             = model.Tipo_moneda_id,
                        Medio_pago_id              = model.Medio_pago_id,
                        Total                      = model.Total,
                        Monto_Proyeccion           = model.Monto_Proyeccion,
                        Fecha_creacion             = DateTime.Now,
                        Ultima_Fecha_actualizacion = DateTime.Now,
                        Usuarios_Usuario_id        = model.Usuarios_Usuario_id,
                        Facturas_id                = model.Facturas_id,
                        Referencia                 = model.Referencia,
                        Clientes_id                = model.Clientes_id,
                        impuesto                   = model.impuesto,
                        subtotal                   = model.subtotal,
                        Descuento                  = model.Descuento,
                        Estado                     = 1,
                        Tipo_cuentas_id            = model.Tipo_cuentas_id,
                        Cuentas_Contables_id       = model.Cuentas_Contables_id,
                        Centro_Costos_id           = model.Centro_Costos_id,
                        Gastos_id                  = model.Gastos_id,
                        Ingresos_id                = model.Ingresos_id,
                        Proveedor_id               = model.Proveedor_id
                    };
                    ctx.Cuenta_Encabezado.Add(ce);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = ce.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;

                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = errorDB;
                return oR;
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = ex.Message;
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
                    throw new Exception("invalid_model_request_missing");

                if (!tool.ValidaTexto(model.Referencia))
                    throw new Exception("invalid_string_form_Referencia");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuenta_Encabezado ce = ctx.Cuenta_Encabezado.FirstOrDefault(u => u.id == id);
                    if (ce == null)
                        throw new Exception("cuenta_encabezado_not_found");

                    ce.Vigencia_inicial           = model.Vigencia_inicial;
                    ce.Vigencia_final             = model.Vigencia_final;
                    ce.Tipo_moneda_id             = model.Tipo_moneda_id;
                    ce.Medio_pago_id              = model.Medio_pago_id;
                    ce.Total                      = model.Total;
                    ce.Monto_Proyeccion           = model.Monto_Proyeccion;
                    ce.Facturas_id                = model.Facturas_id;
                    ce.Referencia                 = model.Referencia;
                    ce.Clientes_id                = model.Clientes_id;
                    ce.impuesto                   = model.impuesto;
                    ce.subtotal                   = model.subtotal;
                    ce.Descuento                  = model.Descuento;
                    ce.Estado                     = model.Estado;
                    ce.Tipo_cuentas_id            = model.Tipo_cuentas_id;
                    ce.Cuentas_Contables_id       = model.Cuentas_Contables_id;
                    ce.Centro_Costos_id           = model.Centro_Costos_id;
                    ce.Gastos_id                  = model.Gastos_id;
                    ce.Ingresos_id                = model.Ingresos_id;
                    ce.Proveedor_id               = model.Proveedor_id;
                    ce.Ultima_Fecha_actualizacion = DateTime.Now;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = ce.id;
                    return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2)
            {
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                    foreach (var ve in eve.ValidationErrors)
                        errorDB += ve.ErrorMessage;

                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = errorDB;
                return oR;
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = ex.Message;
                return oR;
            }
        }


        [HttpGet]
        [Authorize]
        [Route("api/v1/cuenta_encabezado")]
        public Reply GetAllCuentaEncabezado([FromUri] int? tipoCuentaId = null)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled   = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var query = from ce  in ctx.Cuenta_Encabezado
                                join tm  in ctx.Tipo_moneda      on ce.Tipo_moneda_id       equals tm.id
                                join mp  in ctx.Medio_pago       on ce.Medio_pago_id        equals mp.id
                                join tc  in ctx.Tipo_cuentas     on ce.Tipo_cuentas_id      equals tc.id
                                join cco in ctx.Centro_Costos    on ce.Centro_Costos_id     equals cco.id
                                join u   in ctx.Usuarios         on ce.Usuarios_Usuario_id  equals u.Usuario_id

                                // LEFT JOIN Clientes (nullable para CXP)
                                join c in ctx.Clientes on ce.Clientes_id equals c.id into clienteGroup
                                from c in clienteGroup.DefaultIfEmpty()

                                // LEFT JOIN Proveedor (nullable para CXC)
                                join p in ctx.Proveedor on ce.Proveedor_id equals p.id into proveedorGroup
                                from p in proveedorGroup.DefaultIfEmpty()

                                // LEFT JOIN Facturas
                                join f in ctx.Facturas on ce.Facturas_id equals f.id into facturaGroup
                                from f in facturaGroup.DefaultIfEmpty()

                                // LEFT JOIN Gastos
                                join g in ctx.Gastos on ce.Gastos_id equals g.id into gastoGroup
                                from g in gastoGroup.DefaultIfEmpty()

                                // LEFT JOIN Ingresos
                                join i in ctx.Ingresos on ce.Ingresos_id equals i.id into ingresoGroup
                                from i in ingresoGroup.DefaultIfEmpty()

                                select new Models.CuentaEncabezadoViewModel
                                {
                                    id                         = ce.id,
                                    Vigencia_inicial           = ce.Vigencia_inicial,
                                    Vigencia_final             = ce.Vigencia_final,
                                    Tipo_moneda_id             = ce.Tipo_moneda_id,
                                    Medio_pago_id              = ce.Medio_pago_id,
                                    Total                      = ce.Total,
                                    Monto_Proyeccion           = ce.Monto_Proyeccion,
                                    Fecha_creacion             = ce.Fecha_creacion,
                                    Ultima_Fecha_actualizacion = ce.Ultima_Fecha_actualizacion,
                                    Usuarios_Usuario_id        = ce.Usuarios_Usuario_id,
                                    Facturas_id                = ce.Facturas_id,
                                    Referencia                 = ce.Referencia,
                                    Clientes_id                = ce.Clientes_id,
                                    impuesto                   = ce.impuesto,
                                    subtotal                   = ce.subtotal,
                                    Descuento                  = ce.Descuento,
                                    Estado                     = ce.Estado,
                                    Tipo_cuentas_id            = ce.Tipo_cuentas_id,
                                    Cuentas_Contables_id       = ce.Cuentas_Contables_id,
                                    Centro_Costos_id           = ce.Centro_Costos_id,
                                    Gastos_id                  = ce.Gastos_id,
                                    Ingresos_id                = ce.Ingresos_id,
                                    Proveedor_id               = ce.Proveedor_id,
                                    Tipo_moneda                = tm.Nombre,
                                    Simbolo                    = tm.Simbolo,
                                    Medio_pago                 = mp.descripcion,
                                    Cliente                    = c != null ? c.Nombre + " " + c.Apellido1 : "",
                                    Proveedor                  = p != null ? p.Nombre + " " + p.Apellido1 : "",
                                    Tipo_cuenta                = tc.Nombre,
                               
                                    Centro_costo               = cco.Nombre,
                                    Usuario                    = u.Nombre + " " + u.Apellido1,
                                    Estado_texto               = ce.Estado == 1 ? "Activo" : "Inactivo"
                                };

                    // Filtrar por tipo de cuenta si se especifica
                    if (tipoCuentaId.HasValue && tipoCuentaId.Value > 0)
                        query = query.Where(ce => ce.Tipo_cuentas_id == tipoCuentaId.Value);

                    var lista = query.OrderByDescending(ce => ce.id).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = lista;
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = ex.Message;
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
                    throw new Exception("invalid_value_for_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled   = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var ce = (from x   in ctx.Cuenta_Encabezado
                              join tm  in ctx.Tipo_moneda       on x.Tipo_moneda_id       equals tm.id
                              join mp  in ctx.Medio_pago        on x.Medio_pago_id        equals mp.id
                              join tc  in ctx.Tipo_cuentas      on x.Tipo_cuentas_id      equals tc.id
                           
                              join cco in ctx.Centro_Costos     on x.Centro_Costos_id     equals cco.id
                              join u   in ctx.Usuarios          on x.Usuarios_Usuario_id  equals u.Usuario_id

                              join c in ctx.Clientes on x.Clientes_id equals c.id into clienteGroup
                              from c in clienteGroup.DefaultIfEmpty()

                              join p in ctx.Proveedor on x.Proveedor_id equals p.id into proveedorGroup
                              from p in proveedorGroup.DefaultIfEmpty()

                              join f in ctx.Facturas on x.Facturas_id equals f.id into facturaGroup
                              from f in facturaGroup.DefaultIfEmpty()

                              join g in ctx.Gastos on x.Gastos_id equals g.id into gastoGroup
                              from g in gastoGroup.DefaultIfEmpty()

                              join i in ctx.Ingresos on x.Ingresos_id equals i.id into ingresoGroup
                              from i in ingresoGroup.DefaultIfEmpty()

                              where x.id == id
                              select new Models.CuentaEncabezadoViewModel
                              {
                                  id                         = x.id,
                                  Vigencia_inicial           = x.Vigencia_inicial,
                                  Vigencia_final             = x.Vigencia_final,
                                  Tipo_moneda_id             = x.Tipo_moneda_id,
                                  Medio_pago_id              = x.Medio_pago_id,
                                  Total                      = x.Total,
                                  Monto_Proyeccion           = x.Monto_Proyeccion,
                                  Fecha_creacion             = x.Fecha_creacion,
                                  Ultima_Fecha_actualizacion = x.Ultima_Fecha_actualizacion,
                                  Usuarios_Usuario_id        = x.Usuarios_Usuario_id,
                                  Facturas_id                = x.Facturas_id,
                                  Referencia                 = x.Referencia,
                                  Clientes_id                = x.Clientes_id,
                                  impuesto                   = x.impuesto,
                                  subtotal                   = x.subtotal,
                                  Descuento                  = x.Descuento,
                                  Estado                     = x.Estado,
                                  Tipo_cuentas_id            = x.Tipo_cuentas_id,
                                  Cuentas_Contables_id       = x.Cuentas_Contables_id,
                                  Centro_Costos_id           = x.Centro_Costos_id,
                                  Gastos_id                  = x.Gastos_id,
                                  Ingresos_id                = x.Ingresos_id,
                                  Proveedor_id               = x.Proveedor_id,
                                  Tipo_moneda                = tm.Nombre,
                                  Simbolo                    = tm.Simbolo,
                                  Medio_pago                 = mp.descripcion,
                                  Cliente                    = c != null ? c.Nombre + " " + c.Apellido1 : "",
                                  Proveedor                  = p != null ? p.Nombre + " " + p.Apellido1 : "",
                                  Tipo_cuenta                = tc.Nombre,
                                  Centro_costo               = cco.Nombre,
                                  Usuario                    = u.Nombre + " " + u.Apellido1,
                                  Estado_texto               = x.Estado == 1 ? "Activo" : "Inactivo",
                                  
                              }).FirstOrDefault();



                    if (ce == null)
                        throw new Exception("cuenta_encabezado_not_found");

                    // Cargar detalles (abonos/pagos)
                    ce.Detalles = (from d  in ctx.Cuenta_Detalle
                                   join mp in ctx.Medio_pago on d.Medio_pago_id equals mp.id
                                   join u  in ctx.Usuarios on d.Usuarios_Usuario_id equals u.Usuario_id
                                   where d.Cuenta_Encabezado_id == id && d.activo == 1
                                   orderby d.fecha_movimiento descending
                                   select new Models.CuentaDetalleViewModel
                                   {
                                       id                    = d.id,
                                       Cuenta_Encabezado_id  = d.Cuenta_Encabezado_id,
                                       Tipo_movimiento       = d.Tipo_movimiento,
                                       monto                 = d.monto,
                                       saldo_anterior        = d.saldo_anterior,
                                       saldo_posterior       = d.saldo_posterior,
                                       fecha_movimiento      = d.fecha_movimiento,
                                       referencia_pago       = d.referencia_pago,
                                       Observaciones         = d.Observaciones,
                                       activo                = d.activo,
                                       Usuarios_Usuario_id   = d.Usuarios_Usuario_id,
                                       Medio_pago_id         = d.Medio_pago_id,
                                       Medio_pago            = mp.descripcion,
                                       Usuario               = u.Nombre + " " + u.Apellido1,
                                       Tipo_movimiento_texto = d.Tipo_movimiento == 1 ? "Abono"
                                                             : d.Tipo_movimiento == 2 ? "Pago Total"
                                                             : d.Tipo_movimiento == 3 ? "Ajuste"
                                                             : d.Tipo_movimiento == 4 ? "Nota Crédito"
                                                             : "Otro"
                                   }).ToList();


                    if(ce.Gastos_id != null)
                    {
                        //OBTENEMOS LA GESTION del gasto
                        ce.gestion = (from gp in ctx.Gestion_Presupuestaria
                                         join gpd in ctx.Gestion_P_detalle
                                             on gp.id equals gpd.Gestion_Presupuestaria_id
                                         where gpd.Gastos_id == ce.Gastos_id
                                         select gp).FirstOrDefault();
                    }

                    // Calcular saldo pendiente
                    decimal totalAbonos = ce.Detalles.Sum(d => d.monto);
                    ce.Saldo_pendiente  = ce.Total - totalAbonos;

                    // Calcular días vencido
                    ce.DiasVencido = (DateTime.Now - ce.Vigencia_final).Days;
                    if (ce.DiasVencido < 0) ce.DiasVencido = 0;

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = ce;
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = ex.Message;
                return oR;
            }
        }


        /// <summary>
        /// Reporte de antigüedad de saldos CXC o CXP (0-120 días)
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("api/v1/cuenta_encabezado/antiguedad/{tipoCuentaId}")]
        public Reply GetAntiguedadSaldos(int tipoCuentaId)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled   = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var cuentas = (from ce in ctx.Cuenta_Encabezado
                                   join tm in ctx.Tipo_moneda on ce.Tipo_moneda_id equals tm.id

                                   join c in ctx.Clientes on ce.Clientes_id equals c.id into clienteGroup
                                   from c in clienteGroup.DefaultIfEmpty()

                                   join p in ctx.Proveedor on ce.Proveedor_id equals p.id into proveedorGroup
                                   from p in proveedorGroup.DefaultIfEmpty()

                                   where ce.Tipo_cuentas_id == tipoCuentaId
                                      && ce.Estado == 1
                                   select new
                                   {
                                       ce.id,
                                       ce.Referencia,
                                       ce.Total,
                                       ce.Vigencia_final,
                                       ce.Tipo_cuentas_id,
                                       Simbolo  = tm.Simbolo,
                                       Cliente  = c != null ? c.Nombre + " " + c.Apellido1 : "",
                                       Proveedor = p != null ? p.Nombre + " " + p.Apellido1 : "",
                                       TotalAbonos = ctx.Cuenta_Detalle
                                           .Where(d => d.Cuenta_Encabezado_id == ce.id && d.activo == 1)
                                           .Select(d => d.monto)
                                           .DefaultIfEmpty(0)
                                           .Sum()
                                   }).ToList();

                    var resultado = cuentas.Select(ce =>
                    {
                        decimal saldoPendiente = ce.Total - ce.TotalAbonos;
                        int diasVencido = (DateTime.Now - ce.Vigencia_final).Days;
                        if (diasVencido < 0) diasVencido = 0;

                        return new
                        {
                            ce.id,
                            ce.Referencia,
                            ce.Total,
                            ce.TotalAbonos,
                            SaldoPendiente = saldoPendiente,
                            DiasVencido    = diasVencido,
                            ce.Simbolo,
                            Entidad        = ce.Tipo_cuentas_id == 1 ? ce.Cliente : ce.Proveedor,
                            Rango_0_30     = diasVencido <= 30                      ? saldoPendiente : 0,
                            Rango_31_60    = diasVencido > 30  && diasVencido <= 60  ? saldoPendiente : 0,
                            Rango_61_90    = diasVencido > 60  && diasVencido <= 90  ? saldoPendiente : 0,
                            Rango_91_120   = diasVencido > 90  && diasVencido <= 120 ? saldoPendiente : 0,
                            Rango_120_mas  = diasVencido > 120                       ? saldoPendiente : 0
                        };
                    })
                    .Where(x => x.SaldoPendiente > 0)
                    .OrderByDescending(x => x.DiasVencido)
                    .ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = new
                    {
                        tipo        = tipoCuentaId == 1 ? "Cuentas por Cobrar" : "Cuentas por Pagar",
                        total_0_30  = resultado.Sum(x => x.Rango_0_30),
                        total_31_60 = resultado.Sum(x => x.Rango_31_60),
                        total_61_90 = resultado.Sum(x => x.Rango_61_90),
                        total_91_120 = resultado.Sum(x => x.Rango_91_120),
                        total_120_mas = resultado.Sum(x => x.Rango_120_mas),
                        total_general = resultado.Sum(x => x.SaldoPendiente),
                        cuentas       = resultado
                    };
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = ex.Message;
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
                    throw new Exception("invalid_value_for_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuenta_Encabezado ce = ctx.Cuenta_Encabezado.FirstOrDefault(u => u.id == id);
                    if (ce == null)
                        throw new Exception("cuenta_encabezado_not_found");

                    ce.Estado                     = 0;
                    ce.Ultima_Fecha_actualizacion = DateTime.Now;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data       = id;
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message    = ex.Message;
                return oR;
            }
        }
    }
}
