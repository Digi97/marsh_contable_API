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


                using (var ctx = new Models.EntitiesModel())
                {

                    //Models.Gestion_Presupuestaria gpExist = ctx.Gestion_Presupuestaria.FirstOrDefault(u => u.periodo_inicio >=  model.periodo_inicio || u.periodo_fin <= model.periodo_fin);
                    //if (gpExist != null)
                    //{
                    //    throw new Exception("gestion_presupuestaria_for_period_exist");
                    //}



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
                            Tipo_moneda_id = model.Tipo_moneda_id
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
                                         tm.Simbolo
                                     } into g
                                     select new
                                     {
                                         nombre = g.Key.nombre,
                                         descripcion = g.Key.Descripcion,
                                         anio_presupuesto = g.Key.anio_presupuesto,
                                         periodo_inicio = g.Key.periodo_inicio,
                                         periodo_fin = g.Key.periodo_fin,
                                         monto = g.Sum(x => x.gp.monto_aprobado),
                                         simbolo = g.Key.Simbolo
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
        [Route("api/v1/gestion_presupuestaria/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply DeleteGestionPresupuestaria(int id)
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
                    Models.Gestion_Presupuestaria gp = ctx.Gestion_Presupuestaria.FirstOrDefault(u => u.id == id);
                    if (gp == null)
                    {
                        throw new Exception("gestion_presupuestaria_not_found");
                    }
                    gp.estado = 0;
                    gp.fecha_actualizacion = DateTime.Now;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = id;
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
        [Route("api/v1/gestion_presupuestaria_dropdown/{anio_presupuesto}")]
        public Reply GetGestionPDropDown(string anio_presupuesto)
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
                    var resultado = (from gp in ctx.Gestion_Presupuestaria
                                     join cc in ctx.Centro_Costos on gp.Centro_Costos_id equals cc.id
                                     join cp in ctx.Categoria_presupuestaria on gp.Categoria_presupuestaria_id equals cp.id
                                     join tm in ctx.Tipo_moneda on gp.Tipo_moneda_id equals tm.id
                                     where gp.anio_presupuesto == anio_presupuesto
                                     select new
                                     {
                                         id = gp.id+"_"+gp.Categoria_presupuestaria_id+"_"+ gp.Centro_Costos_id,
                                         gp.nombre,
                                         descripcion = gp.nombre + " ( " + cp.nombre + "-" + cc.Nombre + " ) ",
                                         monto = gp.monto_aprobado,
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



        #endregion
    }
}
