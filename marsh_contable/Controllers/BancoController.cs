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
    public class BancoController : ApiController
    {

        // ═══════════════════════════════════════════════════════════
        // GET ALL — Obtener todas las cuentas bancarias
        // ═══════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize]
        [Route("api/v1/bancos")]
        public Reply GetAllBancos()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var lista = (from b in ctx.Bancos
                                 join tm in ctx.Tipo_moneda on b.Tipo_moneda_id equals tm.id
                                 join u in ctx.Usuarios on b.Usuarios_Usuario_id equals u.Usuario_id

                                 orderby b.nombre_banco
                                 select new
                                 {
                                     b.id,
                                     b.nombre_banco,
                                     b.numero_cuenta,
                                     b.tipo_cuenta,
                                     b.saldo_inicial,
                                     b.saldo_actual,
                                     b.fecha_apertura,
                                     b.fecha_actualizacion,
                                     b.estado,
                                     b.Tipo_moneda_id,
                                     Tipo_moneda = tm.Nombre,
                                     Simbolo = tm.Simbolo,
                          
                                     Usuario = u.Nombre + " " + u.Apellido1,
                                     Estado_texto = b.estado == 1 ? "Activo" : "Inactivo"
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


        // ═══════════════════════════════════════════════════════════
        // GET BY ID — Obtener cuenta bancaria por ID
        // ═══════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize]
        [Route("api/v1/bancos/{id}")]
        public Reply GetBancoById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0)
                    throw new Exception("invalid_value_for_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var banco = (from b in ctx.Bancos
                                 join tm in ctx.Tipo_moneda on b.Tipo_moneda_id equals tm.id
                                 join u in ctx.Usuarios on b.Usuarios_Usuario_id equals u.Usuario_id

                                 where b.id == id
                                 select new
                                 {
                                     b.id,
                                     b.nombre_banco,
                                     b.numero_cuenta,
                                     b.tipo_cuenta,
                                     b.saldo_inicial,
                                     b.saldo_actual,
                                     b.fecha_apertura,
                                     b.fecha_actualizacion,
                                     b.estado,
                                     b.Tipo_moneda_id,
                                   
                                     b.Empresa_Emp_id,
                                     b.Usuarios_Usuario_id,
                                     Tipo_moneda = tm.Nombre,
                                     Simbolo = tm.Simbolo,
                                     Usuario = u.Nombre + " " + u.Apellido1
                                 }).FirstOrDefault();

                    if (banco == null)
                        throw new Exception("banco_not_found");

                    // Obtener últimos 20 movimientos de esta cuenta
                    var movimientos = (from m in ctx.Bancos_Movimientos
                                       join tm in ctx.Tipo_moneda on m.Tipo_moneda_id equals tm.id
                                       where m.Bancos_id == id && m.activo == 1
                                       orderby m.fecha_movimiento descending
                                       select new
                                       {
                                           m.id,
                                           m.tipo_movimiento,
                                           m.descripcion,
                                           m.monto,
                                           m.saldo_anterior,
                                           m.saldo_posterior,
                                           m.mes,
                                           m.anio,
                                           m.fecha_movimiento,
                                           m.referencia,
                                           m.Observaciones,
                                           Simbolo = tm.Simbolo
                                       }).Take(20).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        banco = banco,
                        movimientos = movimientos
                    };
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


        // ═══════════════════════════════════════════════════════════
        // POST — Crear cuenta bancaria
        // ═══════════════════════════════════════════════════════════

        [HttpPost]
        [Authorize]
        [Route("api/v1/bancos")]
        public Reply CreateBanco([FromBody] Models.Bancos model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null)
                    throw new Exception("invalid_model_request_missing");

                if (!tool.ValidaTexto(model.nombre_banco))
                    throw new Exception("invalid_string_form_nombre_banco");

                if (!tool.ValidaTexto(model.numero_cuenta))
                    throw new Exception("invalid_string_form_numero_cuenta");

                if (!tool.ValidaTexto(model.tipo_cuenta))
                    throw new Exception("invalid_string_form_tipo_cuenta");

                if (model.Tipo_moneda_id <= 0)
                    throw new Exception("invalid_value_form_Tipo_moneda_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    // Verificar que no exista la misma cuenta
                    bool cuentaExiste = ctx.Bancos
                        .Any(b => b.numero_cuenta == model.numero_cuenta &&
                                  b.estado == 1);

                    if (cuentaExiste)
                        throw new Exception("numero_cuenta_already_exists");

                    Models.Bancos banco = new Models.Bancos()
                    {
                        nombre_banco = model.nombre_banco,
                        numero_cuenta = model.numero_cuenta,
                        tipo_cuenta = model.tipo_cuenta,
                        moneda_simbolo = model.moneda_simbolo,
                        saldo_inicial = model.saldo_inicial,
                        saldo_actual = model.saldo_inicial, // Al crear, saldo actual = saldo inicial
                        fecha_apertura = DateTime.Now,
                        fecha_actualizacion = DateTime.Now,
                        estado = 1,
                        Tipo_moneda_id = model.Tipo_moneda_id,
                      
                        Empresa_Emp_id = 1,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id
                    };

                    ctx.Bancos.Add(banco);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = banco.id;
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;

                if (ex is System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    var errores = dbEx.EntityValidationErrors
                        .SelectMany(eve => eve.ValidationErrors)
                        .Select(ve => ve.ErrorMessage);

                    oR.Message = string.Join(" | ", errores);
                }
                else
                {
                    oR.Message = ex.Message;
                }

                return oR;
            }
        }


        // ═══════════════════════════════════════════════════════════
        // PUT — Actualizar cuenta bancaria
        // ═══════════════════════════════════════════════════════════

        [HttpPut]
        [Authorize]
        [Route("api/v1/bancos/{id}")]
        public Reply UpdateBanco(int id, [FromBody] Models.Bancos model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null)
                    throw new Exception("invalid_model_request_missing");

                if (id <= 0)
                    throw new Exception("invalid_value_for_id");

                if (!tool.ValidaTexto(model.nombre_banco))
                    throw new Exception("invalid_string_form_nombre_banco");

                if (!tool.ValidaTexto(model.numero_cuenta))
                    throw new Exception("invalid_string_form_numero_cuenta");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Bancos banco = ctx.Bancos.FirstOrDefault(b => b.id == id);

                    if (banco == null)
                        throw new Exception("banco_not_found");

                    // Verificar duplicado de cuenta (excluyendo el actual)
                    bool cuentaDuplicada = ctx.Bancos
                        .Any(b => b.numero_cuenta == model.numero_cuenta &&
                                  b.id != id &&
                                  b.estado == 1);

                    if (cuentaDuplicada)
                        throw new Exception("numero_cuenta_already_exists");

                    banco.nombre_banco = model.nombre_banco;
                    banco.numero_cuenta = model.numero_cuenta;
                    banco.tipo_cuenta = model.tipo_cuenta;
                    banco.moneda_simbolo = model.moneda_simbolo;
                    banco.Tipo_moneda_id = model.Tipo_moneda_id;
                    banco.fecha_actualizacion = DateTime.Now;
                    banco.estado = model.estado;

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = banco.id;
                    return oR;
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;

                if (ex is System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    var errores = dbEx.EntityValidationErrors
                        .SelectMany(eve => eve.ValidationErrors)
                        .Select(ve => ve.ErrorMessage);

                    oR.Message = string.Join(" | ", errores);
                }
                else
                {
                    oR.Message = ex.Message;
                }

                return oR;
            }
        }


        // ═══════════════════════════════════════════════════════════
        // GET MOVIMIENTOS — Obtener movimientos por cuenta y período
        // ═══════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize]
        [Route("api/v1/bancos/{bancoId}/movimientos")]
        public Reply GetMovimientosByBanco(int bancoId, [FromUri] int? mes = null, [FromUri] string anio = null)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (bancoId <= 0)
                    throw new Exception("invalid_value_for_banco_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var query = from m in ctx.Bancos_Movimientos
                                join tm in ctx.Tipo_moneda on m.Tipo_moneda_id equals tm.id
                                join u in ctx.Usuarios on m.Usuarios_Usuario_id equals u.Usuario_id

                                // LEFT JOIN Gastos
                                join g in ctx.Gastos on m.Gastos_id equals g.id into gastoGroup
                                from g in gastoGroup.DefaultIfEmpty()

                                    // LEFT JOIN Ingresos
                                join i in ctx.Ingresos on m.Ingresos_id equals i.id into ingresoGroup
                                from i in ingresoGroup.DefaultIfEmpty()

                                    // LEFT JOIN Facturas
                                join f in ctx.Facturas on m.Facturas_id equals f.id into facturaGroup
                                from f in facturaGroup.DefaultIfEmpty()

                                where m.Bancos_id == bancoId && m.activo == 1
                                select new
                                {
                                    m.id,
                                    m.tipo_movimiento,
                                    m.descripcion,
                                    m.monto,
                                    m.saldo_anterior,
                                    m.saldo_posterior,
                                    m.mes,
                                    m.anio,
                                    m.fecha_movimiento,
                                    m.referencia,
                                    m.Observaciones,
                                    m.Gastos_id,
                                    m.Ingresos_id,
                                    m.Facturas_id,
                                    Simbolo = tm.Simbolo,
                                    Usuario = u.Nombre + " " + u.Apellido1,
                                    Gasto_descripcion = g != null ? g.Descripcion : "",
                                    Ingreso_codigo = i != null ? i.Codigo : "",
                                    Factura_clave = f != null ? f.Clave : ""
                                };

                    if (mes.HasValue && mes.Value > 0)
                        query = query.Where(m => m.mes == mes.Value);

                    if (!string.IsNullOrEmpty(anio))
                        query = query.Where(m => m.anio == anio);

                    var movimientos = query.OrderByDescending(m => m.fecha_movimiento).ToList();

                    // Resumen
                    decimal totalIngresos = movimientos.Where(m => m.tipo_movimiento == (short)Tipo_Movimiento_Bancario.Ingreso).Sum(m => m.monto);
                    decimal totalEgresos = movimientos.Where(m => m.tipo_movimiento == (short)Tipo_Movimiento_Bancario.Egreso).Sum(m => m.monto);

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        total_movimientos = movimientos.Count,
                        total_ingresos = totalIngresos,
                        total_egresos = totalEgresos,
                        balance = totalIngresos - totalEgresos,
                        movimientos = movimientos
                    };
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


        // ═══════════════════════════════════════════════════════════
        // REGISTRAR MOVIMIENTO POR GASTO
        // Busca banco por categoria, tipo_moneda y centro_costo
        // y registra el egreso automáticamente
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Valida la existencia de un banco por categoría, tipo de moneda y centro de costo,
        /// y registra un movimiento de egreso al crear un gasto.
        /// Llamar desde GastosController después de guardar el gasto.
        /// </summary>
        public Reply RegistrarMovimientoPorGasto(
    int categoriaPresupuestariaId,
    int tipoMonedaId,
    int centroCostosId,
    int gastoId,
    double montoGasto,
    int usuarioId,
    string descripcionGasto,
    string referencia = "",
    int banco_id = 0
    )
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;
                    Bancos banco;

                    if (banco_id > 0)
                    {
                        banco = ctx.Bancos.FirstOrDefault(b => b.id == banco_id);
                    }
                    else
                    {
                        banco = ctx.Bancos.FirstOrDefault(b =>
                            b.Tipo_moneda_id == tipoMonedaId &&
                            b.estado == 1);
                    }

                    if (banco == null)
                        throw new Exception("banco_not_found_for_tipo_moneda");

                    decimal montoDecimal = (decimal)montoGasto;
                    decimal montoOriginal = montoDecimal;
                    decimal? tipoCambioAplicado = null;
                    string tipoCambioUsado = null;

                    // ── Validar si requiere conversión de moneda (moneda del gasto != moneda del banco)
                    if (tipoMonedaId != banco.Tipo_moneda_id)
                    {
                        var hoy = DateTime.Today;       // fecha de hoy a las 00:00:00
                        var manana = hoy.AddDays(1);    // mañana a las 00:00:00

                        var tipoCambioDia = ctx.Tipo_cambio
                            .Where(c => c.fecha >= hoy && c.fecha < manana)
                            .Select(c => new TipoCambioViewModel
                            {
                                id = c.id,
                                fecha = c.fecha,
                                compra = c.compra,
                                venta = c.venta,
                                Tipo_moneda_id = c.Tipo_moneda_id
                            })
                            .FirstOrDefault();

                        if (tipoCambioDia == null)
                            throw new Exception("tipo_cambio_no_disponible_para_hoy");

                        if (tipoMonedaId == 2 && banco.Tipo_moneda_id == 1)
                        {
                            // Gasto en dólares, banco en colones → convertir a colones (multiplicar por compra)
                            montoDecimal = montoDecimal * (decimal)tipoCambioDia.compra;
                            tipoCambioAplicado = (decimal)tipoCambioDia.compra;
                            tipoCambioUsado = "compra";
                        }
                        else if (tipoMonedaId == 1 && banco.Tipo_moneda_id == 2)
                        {
                            // Gasto en colones, banco en dólares → convertir a dólares (dividir entre venta)
                            montoDecimal = montoDecimal / (decimal)tipoCambioDia.venta;
                            tipoCambioAplicado = (decimal)tipoCambioDia.venta;
                            tipoCambioUsado = "venta";
                        }
                        else
                        {
                            // Combinación de monedas no contemplada
                            throw new Exception($"conversion_no_soportada_moneda_{tipoMonedaId}_banco_{banco.Tipo_moneda_id}");
                        }
                    }

                    // ── Validar saldo suficiente (ya en la moneda del banco)
                    if (banco.saldo_actual < montoDecimal)
                        throw new Exception($"saldo_insuficiente_disponible_{banco.saldo_actual}_requerido_{montoDecimal}");

                    // ── Obtener símbolo de moneda del banco (moneda en la que realmente se mueve el saldo)
                    string simbolo = ctx.Tipo_moneda
                        .Where(t => t.id == banco.Tipo_moneda_id)
                        .Select(t => t.Simbolo)
                        .FirstOrDefault() ?? "";

                    // ── Calcular saldos
                    decimal saldoAnterior = banco.saldo_actual;
                    decimal saldoPosterior = saldoAnterior - montoDecimal;

                    // ── Armar observación indicando si hubo conversión
                    string observaciones = tipoCambioAplicado.HasValue
                        ? $"Egreso automático por gasto | {simbolo} {montoDecimal:N2} (convertido de {montoOriginal:N2} usando tipo de {tipoCambioUsado} {tipoCambioAplicado:N2})"
                        : $"Egreso automático por gasto | {simbolo} {montoGasto:N2}";

                    // ── Crear movimiento de egreso
                    Models.Bancos_Movimientos movimiento = new Models.Bancos_Movimientos()
                    {
                        Bancos_id = banco.id,
                        tipo_movimiento = (short)Tipo_Movimiento_Bancario.Egreso,
                        descripcion = $"Gasto #{gastoId} - {descripcionGasto}",
                        monto = montoDecimal,
                        saldo_anterior = saldoAnterior,
                        saldo_posterior = saldoPosterior,
                        mes = (short)DateTime.Now.Month,
                        anio = DateTime.Now.Year.ToString(),
                        fecha_movimiento = DateTime.Now,
                        referencia = referencia,
                        Centro_Costos_id = centroCostosId,
                        Categoria_presupuestaria_id = categoriaPresupuestariaId,
                        Tipo_moneda_id = banco.Tipo_moneda_id, // moneda real del movimiento (la del banco)
                        Gastos_id = gastoId,
                        Ingresos_id = null,
                        Facturas_id = null,
                        Usuarios_Usuario_id = usuarioId,
                        Observaciones = observaciones,
                        activo = 1
                    };

                    ctx.Bancos_Movimientos.Add(movimiento);

                    // ── Actualizar saldo actual del banco
                    banco.saldo_actual = saldoPosterior;
                    banco.fecha_actualizacion = DateTime.Now;

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        movimiento_id = movimiento.id,
                        banco_id = banco.id,
                        banco_nombre = banco.nombre_banco,
                        saldo_anterior = saldoAnterior,
                        saldo_posterior = saldoPosterior,
                        monto_gasto_original = montoGasto,
                        monto_aplicado = montoDecimal,
                        tipo_cambio_aplicado = tipoCambioAplicado,
                        tipo_cambio_usado = tipoCambioUsado
                    };
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


        public Reply RegistrarMovimientoPorIngreso(
            int categoriaPresupuestariaId,
            int tipoMonedaId,
            int centroCostosId,
            int ingresoId,
            double montoIngreso,
            int usuarioId,
            string descripcionIngreso,
            string referencia = "",
             int banco_id = 0)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;
                    Bancos banco;
                    if (banco_id > 0)
                    {
                        banco = ctx.Bancos.FirstOrDefault(b =>
                      b.id == banco_id);
                    }
                    else
                    {
                        banco = ctx.Bancos.FirstOrDefault(b =>
            b.Tipo_moneda_id == tipoMonedaId &&
             b.estado == 1);
                    }
                    if (banco == null)
                        throw new Exception("banco_not_found_for_categoria_moneda_centro_costo");

                    decimal montoDecimal = (decimal)montoIngreso;
                    decimal montoOriginal = montoDecimal;
                    decimal? tipoCambioAplicado = null;
                    string tipoCambioUsado = null;

                    // ── Validar si requiere conversión de moneda (moneda del ingreso != moneda del banco)
                    if (tipoMonedaId != banco.Tipo_moneda_id)
                    {
                        var hoy = DateTime.Today;
                        var manana = hoy.AddDays(1);

                        var tipoCambioDia = ctx.Tipo_cambio
                            .Where(c => c.fecha >= hoy && c.fecha < manana)
                            .Select(c => new TipoCambioViewModel
                            {
                                id = c.id,
                                fecha = c.fecha,
                                compra = c.compra,
                                venta = c.venta,
                                Tipo_moneda_id = c.Tipo_moneda_id
                            })
                            .FirstOrDefault();

                        if (tipoCambioDia == null)
                            throw new Exception("tipo_cambio_no_disponible_para_hoy");

                        if (tipoMonedaId == 2 && banco.Tipo_moneda_id == 1)
                        {
                            // Ingreso en dólares, banco en colones → multiplicar por compra
                            montoDecimal = montoDecimal * (decimal)tipoCambioDia.compra;
                            tipoCambioAplicado = (decimal)tipoCambioDia.compra;
                            tipoCambioUsado = "compra";
                        }
                        else if (tipoMonedaId == 1 && banco.Tipo_moneda_id == 2)
                        {
                            // Ingreso en colones, banco en dólares → dividir entre venta
                            montoDecimal = montoDecimal / (decimal)tipoCambioDia.venta;
                            tipoCambioAplicado = (decimal)tipoCambioDia.venta;
                            tipoCambioUsado = "venta";
                        }
                        else
                        {
                            throw new Exception($"conversion_no_soportada_moneda_{tipoMonedaId}_banco_{banco.Tipo_moneda_id}");
                        }
                    }

                    // ── Símbolo según la moneda real del banco (moneda en la que se mueve el saldo)
                    string simbolo = ctx.Tipo_moneda
                        .Where(t => t.id == banco.Tipo_moneda_id)
                        .Select(t => t.Simbolo)
                        .FirstOrDefault() ?? "";

                    decimal saldoAnterior = banco.saldo_actual;
                    decimal saldoPosterior = saldoAnterior + montoDecimal; // Ingreso suma

                    string observaciones = tipoCambioAplicado.HasValue
                        ? $"Ingreso automático | {simbolo} {montoDecimal:N2} (convertido de {montoOriginal:N2} usando tipo de {tipoCambioUsado} {tipoCambioAplicado:N2})"
                        : $"Ingreso automático | {simbolo} {montoIngreso:N2}";

                    Models.Bancos_Movimientos movimiento = new Models.Bancos_Movimientos()
                    {
                        Bancos_id = banco.id,
                        tipo_movimiento = (short)Tipo_Movimiento_Bancario.Ingreso,
                        descripcion = $"Ingreso #{ingresoId} - {descripcionIngreso}",
                        monto = montoDecimal,
                        saldo_anterior = saldoAnterior,
                        saldo_posterior = saldoPosterior,
                        mes = (short)DateTime.Now.Month,
                        anio = DateTime.Now.Year.ToString(),
                        fecha_movimiento = DateTime.Now,
                        referencia = referencia,
                        Centro_Costos_id = null, //centroCostosId,
                        Categoria_presupuestaria_id = null,// categoriaPresupuestariaId,
                        Tipo_moneda_id = banco.Tipo_moneda_id, // moneda real del movimiento (la del banco)
                        Gastos_id = null,
                        Ingresos_id = ingresoId,
                        Facturas_id = null,
                        Usuarios_Usuario_id = usuarioId,
                        Observaciones = observaciones,
                        activo = 1
                    };
                    ctx.Bancos_Movimientos.Add(movimiento);
                    banco.saldo_actual = saldoPosterior;
                    banco.fecha_actualizacion = DateTime.Now;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        movimiento_id = movimiento.id,
                        banco_id = banco.id,
                        banco_nombre = banco.nombre_banco,
                        saldo_anterior = saldoAnterior,
                        saldo_posterior = saldoPosterior,
                        monto_ingreso_original = montoIngreso,
                        monto_aplicado = montoDecimal,
                        tipo_cambio_aplicado = tipoCambioAplicado,
                        tipo_cambio_usado = tipoCambioUsado
                    };
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

        public Reply EditarMovimientoPorGasto(
            int categoriaPresupuestariaId,
            int tipoMonedaId,
            int centroCostosId,
            int gastoId,
            double montoGastoNuevo,
            int usuarioId,
            string descripcionGasto,
            string referencia = "",
            int banco_id = 0)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    // ── Buscar movimiento existente por Gastos_id
                    var movimientoExistente = ctx.Bancos_Movimientos
                        .FirstOrDefault(m => m.Gastos_id == gastoId && m.activo == 1);

                    if (movimientoExistente == null)
                    {
                        //SI NO EXISTE LO CREAMOS 
                        var bmovimiento = RegistrarMovimientoPorGasto(categoriaPresupuestariaId, tipoMonedaId, centroCostosId, gastoId, montoGastoNuevo, usuarioId, "Registro de Gasto", banco_id: banco_id);

                        if (bmovimiento.CodeStatus != HttpStatusCode.OK)
                        {
                            throw new Exception(bmovimiento.Message);
                        }
                        return bmovimiento;
                    }

                    // ── Buscar banco
                    var banco = ctx.Bancos.FirstOrDefault(b => b.id == movimientoExistente.Bancos_id);
                    if (banco == null)
                        throw new Exception("banco_not_found");

                    // ── Revertir el movimiento anterior (devolver saldo). El monto guardado ya está en moneda del banco.
                    decimal montoAnterior = movimientoExistente.monto;
                    banco.saldo_actual = banco.saldo_actual + montoAnterior; // Revertir egreso

                    // ── Calcular nuevo monto y validar/aplicar conversión de moneda si aplica
                    decimal montoNuevo = (decimal)montoGastoNuevo;
                    decimal montoNuevoOriginal = montoNuevo;
                    decimal? tipoCambioAplicado = null;
                    string tipoCambioUsado = null;

                    if (tipoMonedaId != banco.Tipo_moneda_id)
                    {
                        var hoy = DateTime.Today;
                        var manana = hoy.AddDays(1);

                        var tipoCambioDia = ctx.Tipo_cambio
                            .Where(c => c.fecha >= hoy && c.fecha < manana)
                            .Select(c => new TipoCambioViewModel
                            {
                                id = c.id,
                                fecha = c.fecha,
                                compra = c.compra,
                                venta = c.venta,
                                Tipo_moneda_id = c.Tipo_moneda_id
                            })
                            .FirstOrDefault();

                        if (tipoCambioDia == null)
                            throw new Exception("tipo_cambio_no_disponible_para_hoy");

                        if (tipoMonedaId == 2 && banco.Tipo_moneda_id == 1)
                        {
                            // Gasto en dólares, banco en colones → multiplicar por compra
                            montoNuevo = montoNuevo * (decimal)tipoCambioDia.compra;
                            tipoCambioAplicado = (decimal)tipoCambioDia.compra;
                            tipoCambioUsado = "compra";
                        }
                        else if (tipoMonedaId == 1 && banco.Tipo_moneda_id == 2)
                        {
                            // Gasto en colones, banco en dólares → dividir entre venta
                            montoNuevo = montoNuevo / (decimal)tipoCambioDia.venta;
                            tipoCambioAplicado = (decimal)tipoCambioDia.venta;
                            tipoCambioUsado = "venta";
                        }
                        else
                        {
                            throw new Exception($"conversion_no_soportada_moneda_{tipoMonedaId}_banco_{banco.Tipo_moneda_id}");
                        }
                    }

                    if (banco.saldo_actual < montoNuevo)
                        throw new Exception($"saldo_insuficiente_disponible_{banco.saldo_actual}_requerido_{montoNuevo}");

                    decimal saldoAnterior = banco.saldo_actual;
                    decimal saldoPosterior = saldoAnterior - montoNuevo;

                    // ── Símbolo según la moneda real del banco (moneda en la que se mueve el saldo)
                    string simbolo = ctx.Tipo_moneda
                        .Where(t => t.id == banco.Tipo_moneda_id)
                        .Select(t => t.Simbolo)
                        .FirstOrDefault() ?? "";

                    string observaciones = tipoCambioAplicado.HasValue
                        ? $"Egreso editado | Anterior: {simbolo} {montoAnterior:N2} → Nuevo: {simbolo} {montoNuevo:N2} (convertido de {montoNuevoOriginal:N2} usando tipo de {tipoCambioUsado} {tipoCambioAplicado:N2})"
                        : $"Egreso editado | Anterior: {simbolo} {montoAnterior:N2} → Nuevo: {simbolo} {montoNuevo:N2}";

                    // ── Actualizar movimiento existente
                    movimientoExistente.monto = montoNuevo;
                    movimientoExistente.saldo_anterior = saldoAnterior;
                    movimientoExistente.saldo_posterior = saldoPosterior;
                    movimientoExistente.descripcion = $"Gasto #{gastoId} - {descripcionGasto} (Editado)";
                    movimientoExistente.fecha_movimiento = DateTime.Now;
                    movimientoExistente.referencia = referencia;
                    movimientoExistente.Centro_Costos_id = centroCostosId;
                    movimientoExistente.Categoria_presupuestaria_id = categoriaPresupuestariaId;
                    movimientoExistente.Tipo_moneda_id = banco.Tipo_moneda_id; // moneda real del movimiento
                    movimientoExistente.Usuarios_Usuario_id = usuarioId;
                    movimientoExistente.Observaciones = observaciones;

                    // ── Actualizar saldo del banco
                    banco.saldo_actual = saldoPosterior;
                    banco.fecha_actualizacion = DateTime.Now;

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        movimiento_id = movimientoExistente.id,
                        banco_id = banco.id,
                        banco_nombre = banco.nombre_banco,
                        monto_anterior = montoAnterior,
                        monto_nuevo_original = montoNuevoOriginal,
                        monto_nuevo_aplicado = montoNuevo,
                        saldo_anterior = saldoAnterior,
                        saldo_posterior = saldoPosterior,
                        tipo_cambio_aplicado = tipoCambioAplicado,
                        tipo_cambio_usado = tipoCambioUsado
                    };
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


        public Reply EditarMovimientoPorIngreso(
            int categoriaPresupuestariaId,
            int tipoMonedaId,
            int centroCostosId,
            int ingresoId,
            double montoIngresoNuevo,
            int usuarioId,
            string descripcionIngreso,
            string referencia = "",
            int banco_id = 0)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    // ── Buscar movimiento existente por Ingresos_id
                    var movimientoExistente = ctx.Bancos_Movimientos
                        .FirstOrDefault(m => m.Ingresos_id == ingresoId && m.activo == 1);

                    if (movimientoExistente == null)
                    {
                        //SI NO EXISTE LO CREAMOS 
                        var bmovimiento = RegistrarMovimientoPorGasto(categoriaPresupuestariaId, tipoMonedaId, centroCostosId, ingresoId, montoIngresoNuevo, usuarioId, "Registro de Ingreso", banco_id: banco_id);

                        if (bmovimiento.CodeStatus != HttpStatusCode.OK)
                        {
                            throw new Exception(bmovimiento.Message);
                        }
                        return bmovimiento;
                    }

                    // ── Buscar banco
                    var banco = ctx.Bancos.FirstOrDefault(b => b.id == movimientoExistente.Bancos_id);
                    if (banco == null)
                        throw new Exception("banco_not_found");

                    // ── Revertir el movimiento anterior (quitar ingreso). El monto guardado ya está en moneda del banco.
                    decimal montoAnterior = movimientoExistente.monto;
                    banco.saldo_actual = banco.saldo_actual - montoAnterior; // Revertir ingreso

                    // ── Calcular nuevo monto y validar/aplicar conversión de moneda si aplica
                    decimal montoNuevo = (decimal)montoIngresoNuevo;
                    decimal montoNuevoOriginal = montoNuevo;
                    decimal? tipoCambioAplicado = null;
                    string tipoCambioUsado = null;

                    if (tipoMonedaId != banco.Tipo_moneda_id)
                    {
                        var hoy = DateTime.Today;
                        var manana = hoy.AddDays(1);

                        var tipoCambioDia = ctx.Tipo_cambio
                            .Where(c => c.fecha >= hoy && c.fecha < manana)
                            .Select(c => new TipoCambioViewModel
                            {
                                id = c.id,
                                fecha = c.fecha,
                                compra = c.compra,
                                venta = c.venta,
                                Tipo_moneda_id = c.Tipo_moneda_id
                            })
                            .FirstOrDefault();

                        if (tipoCambioDia == null)
                            throw new Exception("tipo_cambio_no_disponible_para_hoy");

                        if (tipoMonedaId == 2 && banco.Tipo_moneda_id == 1)
                        {
                            // Ingreso en dólares, banco en colones → multiplicar por compra
                            montoNuevo = montoNuevo * (decimal)tipoCambioDia.compra;
                            tipoCambioAplicado = (decimal)tipoCambioDia.compra;
                            tipoCambioUsado = "compra";
                        }
                        else if (tipoMonedaId == 1 && banco.Tipo_moneda_id == 2)
                        {
                            // Ingreso en colones, banco en dólares → dividir entre venta
                            montoNuevo = montoNuevo / (decimal)tipoCambioDia.venta;
                            tipoCambioAplicado = (decimal)tipoCambioDia.venta;
                            tipoCambioUsado = "venta";
                        }
                        else
                        {
                            throw new Exception($"conversion_no_soportada_moneda_{tipoMonedaId}_banco_{banco.Tipo_moneda_id}");
                        }
                    }

                    decimal saldoAnterior = banco.saldo_actual;
                    decimal saldoPosterior = saldoAnterior + montoNuevo; // Ingreso suma

                    // ── Símbolo según la moneda real del banco
                    string simbolo = ctx.Tipo_moneda
                        .Where(t => t.id == banco.Tipo_moneda_id)
                        .Select(t => t.Simbolo)
                        .FirstOrDefault() ?? "";

                    string observaciones = tipoCambioAplicado.HasValue
                        ? $"Ingreso editado | Anterior: {simbolo} {montoAnterior:N2} → Nuevo: {simbolo} {montoNuevo:N2} (convertido de {montoNuevoOriginal:N2} usando tipo de {tipoCambioUsado} {tipoCambioAplicado:N2})"
                        : $"Ingreso editado | Anterior: {simbolo} {montoAnterior:N2} → Nuevo: {simbolo} {montoNuevo:N2}";

                    // ── Actualizar movimiento existente
                    movimientoExistente.monto = montoNuevo;
                    movimientoExistente.saldo_anterior = saldoAnterior;
                    movimientoExistente.saldo_posterior = saldoPosterior;
                    movimientoExistente.descripcion = $"Ingreso #{ingresoId} - {descripcionIngreso} (Editado)";
                    movimientoExistente.fecha_movimiento = DateTime.Now;
                    movimientoExistente.referencia = referencia;
                    movimientoExistente.Centro_Costos_id = centroCostosId;
                    movimientoExistente.Categoria_presupuestaria_id = categoriaPresupuestariaId;
                    movimientoExistente.Tipo_moneda_id = banco.Tipo_moneda_id; // moneda real del movimiento
                    movimientoExistente.Usuarios_Usuario_id = usuarioId;
                    movimientoExistente.Observaciones = observaciones;

                    // ── Actualizar saldo del banco
                    banco.saldo_actual = saldoPosterior;
                    banco.fecha_actualizacion = DateTime.Now;

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        movimiento_id = movimientoExistente.id,
                        banco_id = banco.id,
                        banco_nombre = banco.nombre_banco,
                        monto_anterior = montoAnterior,
                        monto_nuevo_original = montoNuevoOriginal,
                        monto_nuevo_aplicado = montoNuevo,
                        saldo_anterior = saldoAnterior,
                        saldo_posterior = saldoPosterior,
                        tipo_cambio_aplicado = tipoCambioAplicado,
                        tipo_cambio_usado = tipoCambioUsado
                    };
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


        public Reply EditarMovimientoPorGasto(
            int categoriaPresupuestariaId,
            int tipoMonedaId,
            int centroCostosId,
            int gastoId,
            double montoGasto,
            int usuarioId,
            string descripcionGasto,
            string referencia = "")
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    // ── Buscar banco activo por tipo moneda
                    var banco = ctx.Bancos.FirstOrDefault(b =>
                        b.Tipo_moneda_id == tipoMonedaId &&
                        b.estado == 1);

                    if (banco == null)
                        throw new Exception("banco_not_found_for_categoria_moneda_centro_costo");

                    // ── Calcular monto y validar/aplicar conversión de moneda si aplica
                    decimal montoDecimal = (decimal)montoGasto;
                    decimal montoOriginal = montoDecimal;
                    decimal? tipoCambioAplicado = null;
                    string tipoCambioUsado = null;

                    if (tipoMonedaId != banco.Tipo_moneda_id)
                    {
                        var hoy = DateTime.Today;
                        var manana = hoy.AddDays(1);

                        var tipoCambioDia = ctx.Tipo_cambio
                            .Where(c => c.fecha >= hoy && c.fecha < manana)
                            .Select(c => new TipoCambioViewModel
                            {
                                id = c.id,
                                fecha = c.fecha,
                                compra = c.compra,
                                venta = c.venta,
                                Tipo_moneda_id = c.Tipo_moneda_id
                            })
                            .FirstOrDefault();

                        if (tipoCambioDia == null)
                            throw new Exception("tipo_cambio_no_disponible_para_hoy");

                        if (tipoMonedaId == 2 && banco.Tipo_moneda_id == 1)
                        {
                            montoDecimal = montoDecimal * (decimal)tipoCambioDia.compra;
                            tipoCambioAplicado = (decimal)tipoCambioDia.compra;
                            tipoCambioUsado = "compra";
                        }
                        else if (tipoMonedaId == 1 && banco.Tipo_moneda_id == 2)
                        {
                            montoDecimal = montoDecimal / (decimal)tipoCambioDia.venta;
                            tipoCambioAplicado = (decimal)tipoCambioDia.venta;
                            tipoCambioUsado = "venta";
                        }
                        else
                        {
                            throw new Exception($"conversion_no_soportada_moneda_{tipoMonedaId}_banco_{banco.Tipo_moneda_id}");
                        }
                    }

                    // ── Validar saldo suficiente
                    if (banco.saldo_actual < montoDecimal)
                        throw new Exception($"saldo_insuficiente_disponible_{banco.saldo_actual}_requerido_{montoDecimal}");

                    // ── Obtener símbolo de moneda real del banco
                    string simbolo = ctx.Tipo_moneda
                        .Where(t => t.id == banco.Tipo_moneda_id)
                        .Select(t => t.Simbolo)
                        .FirstOrDefault() ?? "";

                    // ── Calcular saldos
                    decimal saldoAnterior = banco.saldo_actual;
                    decimal saldoPosterior = saldoAnterior - montoDecimal;

                    var movimiento = ctx.Bancos_Movimientos.FirstOrDefault(m =>
                        m.Gastos_id == gastoId &&
                        m.Centro_Costos_id == centroCostosId &&
                        m.Categoria_presupuestaria_id == categoriaPresupuestariaId
                    );

                    if (movimiento == null)
                        throw new Exception("movimiento_banco_not_found_for_gasto_id");

                    movimiento.saldo_anterior = saldoAnterior;
                    movimiento.saldo_posterior = saldoPosterior;
                    movimiento.Observaciones = tipoCambioAplicado.HasValue
                        ? $"Egreso editado automáticamente por gasto | {simbolo} {montoDecimal:N2} (convertido de {montoOriginal:N2} usando tipo de {tipoCambioUsado} {tipoCambioAplicado:N2})"
                        : $"Egreso editado automáticamente por gasto | {simbolo} {montoGasto:N2}";
                    movimiento.monto = montoDecimal;
                    movimiento.Tipo_moneda_id = banco.Tipo_moneda_id;

                    // ── Actualizar saldo actual del banco
                    banco.saldo_actual = saldoPosterior;
                    banco.fecha_actualizacion = DateTime.Now;

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        movimiento_id = movimiento.id,
                        banco_id = banco.id,
                        banco_nombre = banco.nombre_banco,
                        saldo_anterior = saldoAnterior,
                        saldo_posterior = saldoPosterior,
                        monto_gasto_original = montoGasto,
                        monto_aplicado = montoDecimal,
                        tipo_cambio_aplicado = tipoCambioAplicado,
                        tipo_cambio_usado = tipoCambioUsado
                    };
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
        [Route("api/v1/bancos/{id}")]
        public Reply DeleteBancoById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0)
                    throw new Exception("invalid_value_for_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Bancos banco = ctx.Bancos.FirstOrDefault(u => u.id == id);

                    if (banco == null)
                        throw new Exception("user_not_found");

                    try
                    {
                        // Intentar eliminar físicamente
                        ctx.Bancos.Remove(banco);
                        ctx.SaveChanges();

                        oR.CodeStatus = HttpStatusCode.OK;
                        oR.Message = "banco_deleted_successfully";
                        oR.Data = id;
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException)
                    {
                        // FK dependency — revertir el delete y desactivar
                        foreach (var entry in ctx.ChangeTracker.Entries())
                        {
                            entry.Reload();
                        }

                        banco.estado = 0;
                        banco.fecha_actualizacion = DateTime.Now;
                        ctx.SaveChanges();

                        oR.CodeStatus = HttpStatusCode.OK;
                        oR.Message = "banco_deactivated_due_to_dependencies";
                        oR.Data = id;
                    }
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
            }
            return oR;
        }

        public bool validaBanco(int categoriaPresupuestariaId, int tipoMonedaId, int centroCostosId, decimal totalTransaccion, Tipo_Movimiento_Bancario accion)
        {
            try
            {
                Reply oR = new Reply();
                oR.CodeStatus = 0;


                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    // ── Buscar banco activo por categoría, tipo moneda y centro de costo
                    var banco = ctx.Bancos.FirstOrDefault(b =>
                        b.Tipo_moneda_id == tipoMonedaId &&
                        b.estado == 1);

                    if (banco == null)
                        throw new Exception("banco_not_found_for_categoria_moneda_centro_costo");


                    decimal totalBanco = banco.saldo_actual;

                    switch (accion)
                    {
                        case Tipo_Movimiento_Bancario.Ingreso:
                            if ((totalBanco - totalTransaccion) <= 0)
                            {
                                throw new Exception("total_transaccion_exceed_the_banco_amount");
                            }
                            break;
                    }

                    return true;


                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}