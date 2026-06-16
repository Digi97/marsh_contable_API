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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class GestionPresupuestariaController : ApiController
    {

        [HttpPost]
        [Authorize]
        [Route("api/v1/gestion_presupuestaria")]
        [RequierePermiso(PermisosAplica.UsuarioPresupuestos)]
        public Reply CreateGestionPresupuestaria([FromBody] Models.Gestion_Presupuestaria model)
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
                if (!tool.ValidaTexto(model.codigo))
                {
                    throw new Exception("invalid_string_form_codigo");
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
                if (!tool.validaNumeros(model.Categoria_presupuestaria_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Categoria_presupuestaria_id");
                }
                if (!tool.validaNumeros(model.Centro_Costos_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Centro_Costos_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Gestion_Presupuestaria gp = new Models.Gestion_Presupuestaria()
                    {
                        codigo = model.codigo,
                        nombre = model.nombre,
                        Descripcion = model.Descripcion,
                        anio_presupuesto = model.anio_presupuesto,
                        periodo_inicio = model.periodo_inicio,
                        periodo_fin = model.periodo_fin,
                        Categoria_presupuestaria_id = model.Categoria_presupuestaria_id,
                        monto_aprobado = model.monto_aprobado,
                        monto_modificado = model.monto_modificado,
                        monto_comprometido = model.monto_comprometido,
                        monto_ejecutado = model.monto_ejecutado,
                        estado = (Int16)model.estado,
                        fecha_creacion = DateTime.Now,
                        fecha_actualizacion = DateTime.Now,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Centro_Costos_id = model.Centro_Costos_id,
                        Tipo_moneda_id = model.Tipo_moneda_id
                    };
                    ctx.Gestion_Presupuestaria.Add(gp);
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = gp.id;
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
        public Reply UpdateGestionPresupuestaria(int id, [FromBody] Models.Gestion_Presupuestaria model)
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
                if (!tool.ValidaTexto(model.codigo))
                {
                    throw new Exception("invalid_string_form_codigo");
                }
                if (!tool.ValidaTexto(model.nombre))
                {
                    throw new Exception("invalid_string_form_nombre");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Gestion_Presupuestaria gp = ctx.Gestion_Presupuestaria.FirstOrDefault(u => u.id == id);
                    if (gp == null)
                    {
                        throw new Exception("gestion_presupuestaria_not_found");
                    }
                    gp.codigo = model.codigo;
                    gp.nombre = model.nombre;
                    gp.Descripcion = model.Descripcion;
                    gp.anio_presupuesto = model.anio_presupuesto;
                    gp.periodo_inicio = model.periodo_inicio;
                    gp.periodo_fin = model.periodo_fin;
                    gp.Categoria_presupuestaria_id = model.Categoria_presupuestaria_id;
                    gp.monto_aprobado = model.monto_aprobado;
                    gp.monto_modificado = model.monto_modificado;
                    gp.monto_comprometido = model.monto_comprometido;
                    gp.monto_ejecutado = model.monto_ejecutado;
                    gp.estado = (Int16)model.estado;
                    gp.Centro_Costos_id = model.Centro_Costos_id;
                    gp.fecha_actualizacion = DateTime.Now;
                    gp.Tipo_moneda_id = model.Tipo_moneda_id;
                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = gp.id;
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

                    var formato = ctx.Empresa
    .Where(e => e.Emp_id == 1)
    .Select(e => e.Formato_fecha)
    .FirstOrDefault();
                    var lista = (from gp in ctx.Gestion_Presupuestaria
                                 join cp in ctx.Categoria_presupuestaria on gp.Categoria_presupuestaria_id equals cp.id
                                 join cc in ctx.Centro_Costos on gp.Centro_Costos_id equals cc.id
                                 join u in ctx.Usuarios on gp.Usuarios_Usuario_id equals u.Usuario_id
                                 join tm in ctx.Tipo_moneda on gp.Tipo_moneda_id equals tm.id
                                 select new Models.GestionPresupuestariaViewModel
                                 {
                                     id = gp.id,
                                     codigo = gp.codigo,
                                     nombre = gp.nombre,
                                     Descripcion = gp.Descripcion,
                                     anio_presupuesto = gp.anio_presupuesto,
                                     periodo_inicio = gp.periodo_inicio,
                                     periodo_fin = gp.periodo_fin,
                                     Categoria_presupuestaria_id = gp.Categoria_presupuestaria_id,
                                     monto_aprobado = gp.monto_aprobado,
                                     monto_modificado = gp.monto_modificado,
                                     monto_comprometido = gp.monto_comprometido,
                                     monto_ejecutado = gp.monto_ejecutado,
                                     estado = gp.estado,
                                     fecha_creacion = gp.fecha_creacion,
                                     fecha_actualizacion = gp.fecha_actualizacion,
                                     Usuarios_Usuario_id = gp.Usuarios_Usuario_id,
                                     Centro_Costos_id = gp.Centro_Costos_id,
                                     Categoria_presupuestaria = cp.nombre,
                                     Centro_costo = cc.Nombre,
                                     Usuario = u.Nombre + " " + u.Apellido1,
                                     Formato = formato.ToUpper(),
                                     tipo_moneda_id = tm.id,
                                     tipo_moneda = tm.Simbolo
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
        [Route("api/v1/gestion_presupuestaria/{id}")]
        public Reply GetGestionPresupuestariaById(int id)
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
                    var gp = (from x in ctx.Gestion_Presupuestaria
                              join cp in ctx.Categoria_presupuestaria on x.Categoria_presupuestaria_id equals cp.id
                              join cc in ctx.Centro_Costos on x.Centro_Costos_id equals cc.id
                              join u in ctx.Usuarios on x.Usuarios_Usuario_id equals u.Usuario_id
                              join tm in ctx.Tipo_moneda on x.Tipo_moneda_id equals tm.id
                              where x.id == id
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
                                  tipo_moneda_id = tm.id,
                                  tipo_moneda = tm.Simbolo
                              }).FirstOrDefault();

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
    }
}
