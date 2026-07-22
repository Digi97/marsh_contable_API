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
    public class GestionPresupuestariaController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/gestion_presupuestaria")]
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply CreateGestionPresupuestaria([FromBody] Models.GestionPresupuestariaViewModel model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                #region "validaciones"
                if (model == null)
                {
                    throw new Exception("invalid_model_request_missing");
                }

                if (!tool.ValidaTexto(model.nombre))
                {
                    throw new Exception("invalid_string_form_nombre");
                }
                if (!tool.ValidaTexto(model.Descripcion))
                {
                    throw new Exception("invalid_string_form_Descripcion");
                }
                if (!tool.validaNumeros(model.anio_presupuesto))
                {
                    throw new Exception("invalid_string_form_anio_presupuesto");
                }
                
                
                if (model.periodo_inicio >= model.periodo_fin)
                {
                    throw new Exception("periodo_should_be_minor_than_periodo_fin");
                }
               
                if (model.anio_presupuesto.Length != 4 || !int.TryParse(model.anio_presupuesto, out int anio))
                {
                    throw new Exception("invalid_format_anio_presupuesto");
                }

                if (anio < model.periodo_inicio.Year || anio > model.periodo_fin.Year)
                {
                    throw new Exception("anio_presupuesto_fuera_de_periodo");
                }



                if (model.detalles.Count == 0)
                {
                    throw new Exception("detalles_are_required");
                }
                #endregion
                using (var ctx = new Models.EntitiesModel())
                {
                    foreach(var detalle in model.detalles)
                    {
                        var codigo = (from cp in ctx.Categoria_presupuestaria
                                         from cc in ctx.Centro_Costos
                                         where cc.id == detalle.centro_Costos_id && cp.id == detalle.categoria_presupuestaria_id
                                         select cc.codigo + "-" + cp.tipo_categoria
                                 ).FirstOrDefault();
                        var tipo_moneda_id = (from cp in ctx.Categoria_presupuestaria select cp.Tipo_moneda_id ).FirstOrDefault();
                        Models.Gestion_Presupuestaria gp = new Models.Gestion_Presupuestaria()
                        {
                            codigo = codigo,
                            nombre = model.nombre,
                            Descripcion = model.Descripcion,
                            anio_presupuesto = model.anio_presupuesto,
                            periodo_inicio = model.periodo_inicio,
                            periodo_fin = model.periodo_fin,
                            Categoria_presupuestaria_id = detalle.categoria_presupuestaria_id,
                            monto_aprobado = detalle.monto,
                            monto_modificado = 0, //en creacion es cero
                            monto_comprometido = detalle.monto,
                            monto_ejecutado = 0, //en creacion es cero
                            estado = 1, //default activo en creacion
                            fecha_creacion = DateTime.Now,
                            fecha_actualizacion = DateTime.Now,
                            Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                            Centro_Costos_id = detalle.centro_Costos_id,
                            Tipo_moneda_id = tipo_moneda_id //LA MONEDA SE HEREDA DE LA CATEGORIA PRESUPUESTARIA
                        };
                        ctx.Gestion_Presupuestaria.Add(gp);
                        ctx.SaveChanges();
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = 1;
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
        [Route("api/v1/gestion_presupuestaria/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply UpdateGestionPresupuestaria(int id, [FromBody] Models.GestionPresupuestariaViewModel model)
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
                if (!tool.ValidaTexto(model.nombre))
                {
                    throw new Exception("invalid_string_form_nombre");
                }


                if (model.detalles.Count == 0)
                {
                    throw new Exception("detalles_are_required");
                }


                using (var ctx = new Models.EntitiesModel())
                {

                    foreach (var detalle in model.detalles) //actualizamos en base al detalle
                    {
                        Models.Gestion_Presupuestaria gp = ctx.Gestion_Presupuestaria.FirstOrDefault(u => u.id == detalle.id && u.Centro_Costos_id == detalle.centro_Costos_id && u.Categoria_presupuestaria_id == detalle.categoria_presupuestaria_id);
                        if (gp == null)
                        {
                            throw new Exception("gestion_presupuestaria_not_found");
                        }
                        gp.nombre = model.nombre;
                        gp.Descripcion = model.Descripcion;
                        gp.anio_presupuesto = model.anio_presupuesto;
                        gp.periodo_inicio = model.periodo_inicio;
                        gp.periodo_fin = model.periodo_fin;
                        gp.Categoria_presupuestaria_id = detalle.categoria_presupuestaria_id;
                        gp.monto_aprobado = detalle.monto;
                        gp.monto_modificado = model.monto_modificado;
                        gp.monto_comprometido = model.monto_comprometido;
                        gp.monto_ejecutado = model.monto_ejecutado;
                        gp.estado = (Int16)model.estado;
                        gp.Centro_Costos_id = detalle.centro_Costos_id;
                        gp.fecha_actualizacion = DateTime.Now;
                        ctx.SaveChanges();

                    }


                    // notificacion 
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = 1;//devolvemos 1 si todo ok 
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
        [Route("api/v1/gestion_presupuestaria")]
        public Reply GetAllGestionPresupuestaria()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {

           
            
                    var lista = (from gp in ctx.Gestion_Presupuestaria
                                     join tm in ctx.Tipo_moneda on gp.Tipo_moneda_id equals tm.id
                                     group new { gp, tm } by new
                                     {
                                         gp.nombre,
                                         gp.Descripcion,
                                         gp.anio_presupuesto,
                                         gp.periodo_inicio,
                                         gp.periodo_fin,
                                         tm.Simbolo,
                                         gp.estado
                                     } into g
                                     select new
                                     {
                                         nombre = g.Key.nombre,
                                         descripcion = g.Key.Descripcion,
                                         anio_presupuesto = g.Key.anio_presupuesto,
                                         periodo_inicio = g.Key.periodo_inicio,
                                         periodo_fin = g.Key.periodo_fin,
                                         monto = g.Sum(x => x.gp.monto_aprobado),
                                         simbolo = g.Key.Simbolo,
                                         estado = g.Key.estado
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
        [Route("api/v1/gestion_presupuestaria/{anio_presupuesto}")]
        public Reply GetGestionPresupuestariaByAnioPresupuesto(string anio_presupuesto)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (String.IsNullOrEmpty(anio_presupuesto))
                {
                    throw new Exception("invalid_value_for_anio_presupuesto");
                }
                using (var ctx = new Models.EntitiesModel())
                {
                    var gp = (from x in ctx.Gestion_Presupuestaria
                              join cp in ctx.Categoria_presupuestaria on x.Categoria_presupuestaria_id equals cp.id
                              join cc in ctx.Centro_Costos on x.Centro_Costos_id equals cc.id
                              join u in ctx.Usuarios on x.Usuarios_Usuario_id equals u.Usuario_id
                              join tm in ctx.Tipo_moneda on x.Tipo_moneda_id equals tm.id
                              where x.anio_presupuesto == anio_presupuesto
                              select new Models.GestionPresupuestariaViewModel
                              {
                                  id = x.id,
                                  codigo = x.codigo,
                                  nombre = x.nombre,
                                  Descripcion = x.Descripcion,
                                  anio_presupuesto = x.anio_presupuesto,
                                  periodo_inicio = x.periodo_inicio,
                                  periodo_fin = x.periodo_fin,
                                  Categoria_presupuestaria_id = x.Categoria_presupuestaria_id,
                                  monto_aprobado = x.monto_aprobado,
                                  monto_modificado = x.monto_modificado,
                                  monto_comprometido = x.monto_comprometido,
                                  monto_ejecutado = x.monto_ejecutado,
                                  estado = x.estado,
                                  fecha_creacion = x.fecha_creacion,
                                  fecha_actualizacion = x.fecha_actualizacion,
                                  Usuarios_Usuario_id = x.Usuarios_Usuario_id,
                                  Centro_Costos_id = x.Centro_Costos_id,
                                  Categoria_presupuestaria = cp.nombre,
                                  Centro_costo = cc.Nombre,
                                  Usuario = u.Nombre + " " + u.Apellido1,
                                  Tipo_moneda_id = tm.id,
                                  tipo_moneda = tm.Simbolo
                              }).ToList();

                    if (gp == null)
                    {
                        throw new Exception("gestion_presupuestaria_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = gp;
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
        [Route("api/v1/gestion_presupuestaria/{anio_presupuesto}")]
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply DeleteGestionPresupuestaria(string anio_presupuesto)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (String.IsNullOrEmpty(anio_presupuesto))
                {
                    throw new Exception("invalid_value_for_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    using (var tx = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            var lstGp = ctx.Gestion_Presupuestaria
                                           .Where(u => u.anio_presupuesto == anio_presupuesto)
                                           .ToList();

                            if (lstGp.Count == 0)
                            {
                                throw new Exception("gestion_presupuestaria_not_found");
                            }

                            var lstIds = lstGp.Select(x => x.id).ToList();

                            // ¿Existe movimiento asociado?
                            bool tieneDetalle = ctx.Gestion_P_detalle
                                                   .Any(d => lstIds.Contains(d.Gestion_Presupuestaria_id));

                            bool tieneAnio = ctx.Gestion_P_Anio
                                                .Any(a => lstIds.Contains(a.Gestion_Presupuestaria_id));

                            if (tieneDetalle || tieneAnio)
                            {
                                // Borrado lógico: se conserva la información histórica
                                foreach (var gp in lstGp)
                                {
                                    gp.estado = 0;
                                    gp.fecha_actualizacion = DateTime.Now;
                                }

                                ctx.SaveChanges();
                                oR.Message = "gestion_presupuestaria_inactivada";
                            }
                            else
                            {
                                // Sin movimiento: borrado físico
                                ctx.Gestion_Presupuestaria.RemoveRange(lstGp);
                                ctx.SaveChanges();
                                oR.Message = "gestion_presupuestaria_eliminada";
                            }

                            tx.Commit();

                            oR.CodeStatus = HttpStatusCode.OK;
                            oR.Data = anio_presupuesto;
                            return oR;
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return oR;
            }
        }


        #region "Gestion por año"

        [HttpPost]
        [Authorize]
        [Route("api/v1/gestion_por_anio")]
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply CreateGestionPorAnio([FromBody] Models.GestionPAnioViewModel model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                #region "validaciones"
                if (model == null)
                    throw new Exception("invalid_model_request_missing");

                if (!tool.validaNumeros(model.anio_presupuesto))
                    throw new Exception("invalid_string_form_anio_presupuesto");

                if (model.anio_presupuesto.Length != 4)
                    throw new Exception("invalid_format_anio_presupuesto");

                
                if (model.detalles == null || model.detalles.Count == 0)
                    throw new Exception("detalles_are_required");

                // Validar que los meses sean válidos (1-12)
                if (model.detalles.Any(d => d.mes < 1 || d.mes > 12))
                    throw new Exception("invalid_value_mes_must_be_between_1_and_12");
                #endregion

                using (var ctx = new Models.EntitiesModel())
                {
                  
                    // Verificar que no existan registros para ese año y gestión
                                   foreach (var detalle in model.detalles)
                    {

                        // Verificar que la gestión presupuestaria existe
                        var gpExist = ctx.Gestion_Presupuestaria
                            .FirstOrDefault(g => g.id == detalle.Gestion_Presupuestaria_id);

                        if (gpExist == null)
                            throw new Exception("gestion_presupuestaria_not_found");

                        bool yaExiste = ctx.Gestion_P_Anio
                             .Any(a => a.Gestion_Presupuestaria_id == detalle.Gestion_Presupuestaria_id &&
                             a.anio_presupuesto == detalle.anio_presupuesto && a.mes == detalle.mes);

                        if (yaExiste)
                            throw new Exception("gestion_por_anio_for_this_month_already_exists");



                        Models.Gestion_P_Anio gpa = new Models.Gestion_P_Anio()
                        {
                            Gestion_Presupuestaria_id = detalle.Gestion_Presupuestaria_id,
                            anio_presupuesto = detalle.anio_presupuesto,
                            monto = detalle.monto,
                            mes = detalle.mes
                        };
                        ctx.Gestion_P_Anio.Add(gpa);
                    }

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        Gestion_Presupuestaria_id = model.Gestion_Presupuestaria_id,
                        anio_presupuesto = model.anio_presupuesto,
                        registros_creados = model.detalles.Count
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


        [HttpGet]
        [Authorize]
        [Route("api/v1/gestion_por_anio/{anio}")]
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply GetGestionPorAnio(string anio)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (string.IsNullOrEmpty(anio) || anio.Length != 4)
                    throw new Exception("invalid_format_anio_presupuesto");

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var lista = (from gpa in ctx.Gestion_P_Anio
                                 join gp in ctx.Gestion_Presupuestaria
                                     on gpa.Gestion_Presupuestaria_id equals gp.id
                                 where gpa.anio_presupuesto == anio
                                 orderby gpa.mes
                                 select new
                                 {
                                     gpa.id,
                                     gpa.Gestion_Presupuestaria_id,
                                     gpa.anio_presupuesto,
                                     gpa.monto,
                                     gpa.mes,
                                     gestion_nombre = gp.nombre
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
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        [Route("api/v1/gestion_presupuestaria_dropdown/{anio_presupuesto}/{mes_presupuesto}")]
        public Reply GetGestionPDropDown(string anio_presupuesto, int mes_presupuesto = 0)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            DateTime currenDate = DateTime.Now;
            try
            {
                if (String.IsNullOrEmpty(anio_presupuesto))
                {
                    throw new Exception("invalid_value_for_anio_presupuesto");
                }

                if(mes_presupuesto == 0)
                {
                    mes_presupuesto = currenDate.Month;
                }



                using (var ctx = new Models.EntitiesModel())
                {

                 
                    var resultado = (from gp in ctx.Gestion_Presupuestaria
                                     join cc in ctx.Centro_Costos on gp.Centro_Costos_id equals cc.id
                                     join cp in ctx.Categoria_presupuestaria on gp.Categoria_presupuestaria_id equals cp.id
                                     join tm in ctx.Tipo_moneda on gp.Tipo_moneda_id equals tm.id
                                     join gpa in ctx.Gestion_P_Anio on new { id = gp.id, gp.anio_presupuesto } equals new { id = gpa.Gestion_Presupuestaria_id, gpa.anio_presupuesto }
                                     where gp.anio_presupuesto == anio_presupuesto && gpa.mes == mes_presupuesto
                                     select new
                                     {
                                         id = gp.id+"_"+gp.Categoria_presupuestaria_id+"_"+ gp.Centro_Costos_id,
                                         gp.nombre,
                                         descripcion = gp.codigo + " ( " + cp.nombre + "-" + cc.Nombre + " ) ",
                                         monto = gpa.monto,//gp.monto_aprobado,
                                         simbolo = tm.Simbolo
                                     }).ToList();

                    if (resultado == null)
                    {
                        throw new Exception("gestion_presupuestaria_not_found");
                    }
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = resultado;
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





        [HttpPut]
        [Authorize]
        [Route("api/v1/mover_gestion_presupuestaria/{idOrigen}/{idDestino}")]
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply MoverPresupuesto(string idOrigen, string idDestino, [FromBody] Models.GestionPresupuestariaViewModel model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
               
                if (!tool.validaNumeros(model.monto_modificado.ToString()))
                    throw new Exception("monto_is_invalid");

                if (model.monto_modificado <= 0)
                    throw new Exception("monto_modificado_must_be_greater_than_zero");


              

                if (idOrigen == idDestino)
                    throw new Exception("origen_and_destino_must_be_different");


                string[] partesOrigen = idOrigen.Split('_'); // id = gp.id+"_"+gp.Categoria_presupuestaria_id+"_"+ gp.Centro_Costos_id,

                int pidO = int.Parse(partesOrigen[0]);
                int cpidO = int.Parse(partesOrigen[1]);
                int ccidO = int.Parse(partesOrigen[2]);

                string[] partesDestino = idDestino.Split('_'); // id = gp.id+"_"+gp.Categoria_presupuestaria_id+"_"+ gp.Centro_Costos_id,

                int pidD = int.Parse(partesDestino[0]);
                int cpidD = int.Parse(partesDestino[1]);
                int ccidD = int.Parse(partesDestino[2]);

                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    int mesActual = DateTime.Now.Month;
                    int anioActual = DateTime.Now.Year;

                    // ── Validar que los parámetros vengan en el model
                    if (model.mesOrigen < 1 || model.mesOrigen > 12)
                        throw new Exception("invalid_value_mes_origen_must_be_between_1_and_12");

                    if (model.mesDestino < 1 || model.mesDestino > 12)
                        throw new Exception("invalid_value_mes_destino_must_be_between_1_and_12");

                    if (string.IsNullOrEmpty(model.anioOrigen) || model.anioOrigen.Length != 4)
                        throw new Exception("invalid_format_anio_origen");

                    if (string.IsNullOrEmpty(model.anioDestino) || model.anioDestino.Length != 4)
                        throw new Exception("invalid_format_anio_destino");

                    int anioOrigenInt = int.Parse(model.anioOrigen);
                    int anioDestinoInt = int.Parse(model.anioDestino);

                    // ── Validar que no sean períodos pasados
                    //if (anioOrigenInt < anioActual ||
                    //   (anioOrigenInt == anioActual && model.mesOrigen < mesActual))
                    //    throw new Exception("periodo_origen_no_puede_ser_pasado");

                    if (anioDestinoInt < anioActual ||
                       (anioDestinoInt == anioActual && model.mesDestino < mesActual))
                        throw new Exception("periodo_destino_no_puede_ser_pasado");

                    // ── Validar existencia de presupuestos
                    Models.Gestion_Presupuestaria gpOrigen = ctx.Gestion_Presupuestaria
                        .FirstOrDefault(u => u.id == pidO && u.Categoria_presupuestaria_id == cpidO && u.Centro_Costos_id == ccidO);

                    if (gpOrigen == null)
                        throw new Exception("gestion_presupuestaria_origin_dont_exist");

                    Models.Gestion_Presupuestaria gpDestino = ctx.Gestion_Presupuestaria
                        .FirstOrDefault(u => u.id == pidD && u.Categoria_presupuestaria_id == cpidD && u.Centro_Costos_id == ccidD);

                    if (gpDestino == null)
                        throw new Exception("gestion_presupuestaria_destino_dont_exist");

                    // ═══════════════════════════════════════════════════════
                    // PASO 1: Validar existencia en Gestion_P_Anio con parámetros del model
                    // ═══════════════════════════════════════════════════════

                    var gpAnioOrigen = ctx.Gestion_P_Anio
                        .FirstOrDefault(a => a.Gestion_Presupuestaria_id == pidO &&
                                             a.anio_presupuesto == model.anioOrigen &&
                                             a.mes == model.mesOrigen );

                    if (gpAnioOrigen == null)
                        throw new Exception($"no_existe_presupuesto_para_mes_{model.mesOrigen}_anio_{model.anioOrigen}_en_origen");

                    // ── Validar destino en Gestion_P_Anio
                    var gpAnioDestino = ctx.Gestion_P_Anio
                        .FirstOrDefault(a => a.Gestion_Presupuestaria_id == pidD &&
                                             a.anio_presupuesto == model.anioDestino &&
                                             a.mes == model.mesDestino);

                    if (gpAnioOrigen == null)
                        throw new Exception($"no_existe_presupuesto_anio_para_mes_{mesActual}_anio_{anioActual}_en_origen");

                    if (gpAnioOrigen.monto <= 0)
                        throw new Exception($"presupuesto_mes_{mesActual}_sin_monto_disponible_en_origen");

                    if (gpAnioOrigen.monto < (decimal)model.monto_modificado)
                        throw new Exception($"monto_a_trasladar_excede_presupuesto_mensual_disponible_{gpAnioOrigen.monto}_requerido_{model.monto_modificado}");

                    // ═══════════════════════════════════════════════════════
                    // PASO 2: Calcular monto ejecutado real en Gestion_P_detalle
                    //         Gastos = resta | Ingresos y Facturas = suma
                    // ═══════════════════════════════════════════════════════

                    // Sumar gastos (egresos)
                    double totalGastos = ctx.Gestion_P_detalle
                        .Where(d => d.Gestion_Presupuestaria_id == pidO &&
                                    d.activo == 1 &&
                                    d.Gastos_id != null)
                        .Select(d => (double)d.Monto_ejecutado)
                        .DefaultIfEmpty(0)
                        .Sum();

                    // Sumar ingresos
                    double totalIngresos = ctx.Gestion_P_detalle
                        .Where(d => d.Gestion_Presupuestaria_id == pidD &&
                                    d.activo == 1 &&
                                    d.Ingresos_id != null)
                        .Select(d => (double)d.Monto_ejecutado)
                        .DefaultIfEmpty(0)
                        .Sum();

                    // Sumar facturas
                    double totalFacturas = ctx.Gestion_P_detalle
                        .Where(d => d.Gestion_Presupuestaria_id == pidO &&
                                    d.activo == 1 &&
                                    d.Facturas_id != null)
                        .Select(d => (double)d.Monto_ejecutado)
                        .DefaultIfEmpty(0)
                        .Sum();

                    // Balance real: ingresos + facturas - gastos
                    double balanceEjecutado = (totalIngresos + totalFacturas) - totalGastos;

                    // Monto neto ejecutado (gastos netos)
                    double montoEjecutadoNeto = totalGastos - (totalIngresos + totalFacturas);

                    // ═══════════════════════════════════════════════════════
                    // PASO 3: Validar que el presupuesto no haya sido consumido
                    // ═══════════════════════════════════════════════════════

                    double montoAnioOrigen = (double)gpAnioOrigen.monto;

                    if (montoEjecutadoNeto >= montoAnioOrigen)
                        throw new Exception($"no_se_puede_trasladar_presupuesto_ya_fue_ejecutado_ejecutado_{montoEjecutadoNeto}_presupuesto_mes_{montoAnioOrigen}");

                    // ═══════════════════════════════════════════════════════
                    // PASO 4: Validar que el monto a mover no deje el origen
                    //         sin cobertura para lo ya ejecutado
                    // ═══════════════════════════════════════════════════════

                    double disponibleDespuesDeTraslado = montoAnioOrigen - model.monto_modificado;

                    if (disponibleDespuesDeTraslado < montoEjecutadoNeto)
                        throw new Exception($"traslado_dejaria_presupuesto_sin_cobertura_ejecutado_{montoEjecutadoNeto}_disponible_post_traslado_{disponibleDespuesDeTraslado}");

                    // ═══════════════════════════════════════════════════════
                    // PASO 5: Ejecutar el traslado
                    // ═══════════════════════════════════════════════════════

                    // ── Actualizar ORIGEN: restar montos
                    gpOrigen.monto_aprobado = gpOrigen.monto_aprobado - model.monto_modificado;
                    gpOrigen.monto_modificado = gpOrigen.monto_modificado - model.monto_modificado;
                    gpOrigen.fecha_actualizacion = DateTime.Now;

                    // ── Actualizar DESTINO: sumar montos
                    gpDestino.monto_aprobado = gpDestino.monto_aprobado + model.monto_modificado;
                    gpDestino.monto_modificado = gpDestino.monto_modificado + model.monto_modificado;
                    gpDestino.fecha_actualizacion = DateTime.Now;

                    // ── Actualizar Gestion_P_Anio del origen
                    gpAnioOrigen.monto = gpAnioOrigen.monto - (decimal)model.monto_modificado;

                    // ── Actualizar o crear Gestion_P_Anio del destino
                    var gpAnioDestinoActualiza = ctx.Gestion_P_Anio
                        .FirstOrDefault(a => a.Gestion_Presupuestaria_id == pidD &&
                                             a.anio_presupuesto == anioDestinoInt.ToString() &&
                                             a.mes == mesActual);

                    if (gpAnioDestinoActualiza != null)
                    {
                        gpAnioDestinoActualiza.monto = gpAnioDestinoActualiza.monto + (decimal)model.monto_modificado;
                    }
                    else
                    {
                        Models.Gestion_P_Anio nuevoAnioDestino = new Models.Gestion_P_Anio()
                        {
                            Gestion_Presupuestaria_id = pidD,
                            anio_presupuesto = model.anioDestino,
                            monto = (decimal)model.monto_modificado,
                            mes = model.mesDestino
                        };
                        ctx.Gestion_P_Anio.Add(nuevoAnioDestino);
                    }

                    // ── Registrar movimiento de traslado en detalle (origen - egreso)
                    Models.Gestion_P_detalle detalleOrigen = new Models.Gestion_P_detalle()
                    {
                        Monto = model.monto_modificado * -1, // Negativo porque sale
                        Monto_aprobado = gpOrigen.monto_aprobado,
                        Monto_modificado = gpOrigen.monto_modificado,
                        Monto_compometido = gpOrigen.monto_comprometido,
                        Monto_ejecutado = (decimal)(model.monto_modificado * -1),
                        detalle_presupuesto = $"Traslado hacia {gpDestino.nombre}",
                        Gestion_Presupuestaria_id = pidO,
                        Categoria_presupuestaria_id = gpOrigen.Categoria_presupuestaria_id,
                        Gastos_id = null,
                        Ingresos_id = null,
                        Facturas_id = null,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Fecha_registro = DateTime.Now,
                        Observaciones = $"Traslado presupuestario de {gpOrigen.nombre} a {gpDestino.nombre} | Monto: {model.monto_modificado:N2}",
                        activo = 1
                    };
                    ctx.Gestion_P_detalle.Add(detalleOrigen);

                    // ── Registrar movimiento de traslado en detalle (destino - ingreso)
                    Models.Gestion_P_detalle detalleDestino = new Models.Gestion_P_detalle()
                    {
                        Monto = model.monto_modificado, // Positivo porque entra
                        Monto_aprobado = gpDestino.monto_aprobado,
                        Monto_modificado = gpDestino.monto_modificado,
                        Monto_compometido = gpDestino.monto_comprometido,
                        Monto_ejecutado = (decimal)model.monto_modificado,
                        detalle_presupuesto = $"Traslado desde {gpOrigen.nombre}",
                        Gestion_Presupuestaria_id = pidD,
                        Categoria_presupuestaria_id = gpDestino.Categoria_presupuestaria_id,
                        Gastos_id = null,
                        Ingresos_id = null,
                        Facturas_id = null,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Fecha_registro = DateTime.Now,
                        Observaciones = $"Traslado presupuestario de {gpOrigen.nombre} a {gpDestino.nombre} | Monto: {model.monto_modificado:N2}",
                        activo = 1
                    };
                    ctx.Gestion_P_detalle.Add(detalleDestino);

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = new
                    {
                        origen = new
                        {
                            id = gpOrigen.id,
                            nombre = gpOrigen.nombre,
                            monto_aprobado = gpOrigen.monto_aprobado,
                            monto_modificado = gpOrigen.monto_modificado
                        },
                        destino = new
                        {
                            id = gpDestino.id,
                            nombre = gpDestino.nombre,
                            monto_aprobado = gpDestino.monto_aprobado,
                            monto_modificado = gpDestino.monto_modificado
                        },
                        monto_trasladado = model.monto_modificado,
                        mes = mesActual,
                        anio = anioActual
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

        #endregion
    }
}
