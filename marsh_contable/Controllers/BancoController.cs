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

                    // ── Buscar banco activo por categoría, tipo moneda y centro de costo
                    var banco = ctx.Bancos.FirstOrDefault(b =>
                       b.Tipo_moneda_id == tipoMonedaId && 
                        b.estado == 1);

                    if (banco == null)
                        throw new Exception("banco_not_found_for_tipo_moneda");

                    // ── Validar saldo suficiente
                    decimal montoDecimal = (decimal)montoGasto;
                    if (banco.saldo_actual < montoDecimal)
                        throw new Exception($"saldo_insuficiente_disponible_{banco.saldo_actual}_requerido_{montoDecimal}");

                    // ── Obtener símbolo de moneda
                    string simbolo = ctx.Tipo_moneda
                        .Where(t => t.id == tipoMonedaId)
                        .Select(t => t.Simbolo)
                        .FirstOrDefault() ?? "";

                    // ── Calcular saldos
                    decimal saldoAnterior = banco.saldo_actual;
                    decimal saldoPosterior = saldoAnterior - montoDecimal;

                    // ── Crear movimiento de egreso
                    Models.Bancos_Movimientos movimiento = new Models.Bancos_Movimientos()
                    {
                        Bancos_id = banco.id,
                        tipo_movimiento = (short) Tipo_Movimiento_Bancario.Egreso,
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
                        Tipo_moneda_id = tipoMonedaId,
                        Gastos_id = gastoId,
                        Ingresos_id = null,
                        Facturas_id = null,
                        Usuarios_Usuario_id = usuarioId,
                        Observaciones = $"Egreso automático por gasto | {simbolo} {montoGasto:N2}",
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
                        monto_gasto = montoGasto
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

                    var banco = ctx.Bancos.FirstOrDefault(b =>
                        b.Tipo_moneda_id == tipoMonedaId &&
                        b.estado == 1);

                    if (banco == null)
                        throw new Exception("banco_not_found_for_categoria_moneda_centro_costo");

                    string simbolo = ctx.Tipo_moneda
                        .Where(t => t.id == tipoMonedaId)
                        .Select(t => t.Simbolo)
                        .FirstOrDefault() ?? "";

                    decimal montoDecimal = (decimal)montoIngreso;
                    decimal saldoAnterior = banco.saldo_actual;
                    decimal saldoPosterior = saldoAnterior + montoDecimal; // Ingreso suma

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
                        Centro_Costos_id = centroCostosId,
                        Categoria_presupuestaria_id = categoriaPresupuestariaId,
                        Tipo_moneda_id = tipoMonedaId,
                        Gastos_id = null,
                        Ingresos_id = ingresoId,
                        Facturas_id = null,
                        Usuarios_Usuario_id = usuarioId,
                        Observaciones = $"Ingreso automático | {simbolo} {montoIngreso:N2}",
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
                        monto_ingreso = montoIngreso
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
                           if((totalBanco - totalTransaccion) <= 0)
                            {
                                throw new Exception("total_transaccion_exceed_the_banco_amount");
                            }
                        break;
                    }

                    return true;


                }


                }
            catch(Exception ex)
            {
                throw ex;
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

                    // ── Buscar banco activo por categoría, tipo moneda y centro de costo
                    var banco = ctx.Bancos.FirstOrDefault(b =>
                        b.Tipo_moneda_id == tipoMonedaId &&
                        b.estado == 1);

                    if (banco == null)
                        throw new Exception("banco_not_found_for_categoria_moneda_centro_costo");

                    // ── Validar saldo suficiente
                    decimal montoDecimal = (decimal)montoGasto;
                    if (banco.saldo_actual < montoDecimal)
                        throw new Exception($"saldo_insuficiente_disponible_{banco.saldo_actual}_requerido_{montoDecimal}");

                    // ── Obtener símbolo de moneda
                    string simbolo = ctx.Tipo_moneda
                        .Where(t => t.id == tipoMonedaId)
                        .Select(t => t.Simbolo)
                        .FirstOrDefault() ?? "";

                    // ── Calcular saldos
                    decimal saldoAnterior = banco.saldo_actual;
                    decimal saldoPosterior = saldoAnterior - montoDecimal;

                    // ── Crear movimiento de egreso


                    var movimiento = ctx.Bancos_Movimientos.FirstOrDefault(m =>
                        m.Gastos_id == gastoId && 
                        m.Centro_Costos_id == centroCostosId && 
                        m.Categoria_presupuestaria_id == categoriaPresupuestariaId
                
                    );

                    movimiento.saldo_anterior = saldoAnterior;
                    movimiento.saldo_posterior = saldoPosterior;
                    movimiento.Observaciones = $"Egreso editado automáticamente por gasto | {simbolo} {montoGasto:N2}";
                    movimiento.monto = montoDecimal;

                 //   ctx.Bancos_Movimientos.Add(movimiento);

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
                        monto_gasto = montoGasto
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


    }
}