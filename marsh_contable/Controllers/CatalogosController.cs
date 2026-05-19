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
    public class CatalogosController : ApiController
    {

        #region "Codigo actividad"



        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/codigo_actividad")]
        public Reply CreateCodigoActividad([FromBody] Models.codigo_actividad model)
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
                if (!tool.validaNumeros(model.codigo_actividad1))
                {
                    throw new Exception("invalid_string_form_codigo_actividad");
                }
                if (!tool.ValidaTexto(model.nombre_actividad))
                {
                    throw new Exception("invalid_string_form_nombre_actividad");
                }


                var codigo_actividad = tool.FormatearCodigoActividad(model.codigo_actividad1);


                using (var ctx = new Models.EntitiesModel())
                {
                    Models.codigo_actividad ca = new Models.codigo_actividad()
                    {
                       
                        codigo_actividad1 = codigo_actividad,
                        nombre_actividad = model.nombre_actividad,
                 
                    };

                    ctx.codigo_actividad.Add(ca);
                    ctx.SaveChanges();


                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ca.id; // retorna el ID generado                 
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
        [Route("api/v1/catalogos/codigo_actividad/{id}")]
        public Reply UpdateCodigoActividad(int id, [FromBody] Models.codigo_actividad model)
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
                if (!tool.validaNumeros(model.codigo_actividad1))
                {
                    throw new Exception("invalid_string_form_codigo_actividad");
                }
                if (!tool.ValidaTexto(model.nombre_actividad))
                {
                    throw new Exception("invalid_string_form_nombre_actividad");
                }
         
     
                using (var ctx = new Models.EntitiesModel())
                {
                    Models.codigo_actividad ca = ctx.codigo_actividad.FirstOrDefault(u => u.id == id);

                    if (ca == null)
                    {
                        throw new Exception("codigo_actividad_not_found");
                    }

                ca.codigo_actividad1 = model.codigo_actividad1;
                ca.nombre_actividad = model.nombre_actividad;
                   

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ca.id;

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
        [Route("api/v1/catalogos/codigo_actividad")]
        public Reply GetAllCodigo_actividad()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;

            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
            
                    var ca = ctx.codigo_actividad
              .Select(x => new {
                  x.id,
                  x.codigo_actividad1,
                  x.nombre_actividad
              }).ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ca;
                    return oR;
                }
            }

            catch(System.Data.Entity.Validation.DbEntityValidationException ex2)
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
        [Route("api/v1/catalogos/codigo_actividad/{id}")]
        public Reply GetCodigoActividadById(int id)
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

                    var ca = ctx.codigo_actividad
                       .Where(x => x.id == id)
                        .Select(x => new {
                        x.id,
                        x.codigo_actividad1,
                        x.nombre_actividad
                        }).ToList();
        

                    if (ca == null)
                    {
                        throw new Exception("codigo_actividad_not_found");
                    }

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = ca;
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
            }
            return oR;
        }
        #endregion

        #region "Cuentas_Contables"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/cuentas_contables")]
        public Reply CreateCuentasContables([FromBody] Models.Cuentas_Contables model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                if (!tool.validaNumeros(model.Tipo_Cuenta_Contable_id.ToString())) throw new Exception("invalid_value_form_Tipo_Cuenta_Contable_id");
                if (!tool.validaNumeros(model.Usuarios_Usuario_id.ToString())) throw new Exception("invalid_value_form_Usuarios_Usuario_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuentas_Contables cc = new Models.Cuentas_Contables()
                    {
                        Codigo = model.Codigo,
                        Nombre = model.Nombre,
                        Tipo_Cuenta_Contable_id = model.Tipo_Cuenta_Contable_id,
                        Usuarios_Usuario_id = model.Usuarios_Usuario_id,
                        Estado = model.Estado,
                        Saldo_inicial = model.Saldo_inicial,
                        Saldo_actual = model.Saldo_actual,
                        Fecha_Creacion = DateTime.Now,
                        Fecha_actualizacion = DateTime.Now
                    };
                    ctx.Cuentas_Contables.Add(cc);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/cuentas_contables/{id}")]
        public Reply UpdateCuentasContables(int id, [FromBody] Models.Cuentas_Contables model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                if (!tool.validaNumeros(model.Tipo_Cuenta_Contable_id.ToString())) throw new Exception("invalid_value_form_Tipo_Cuenta_Contable_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Cuentas_Contables cc = ctx.Cuentas_Contables.FirstOrDefault(x => x.id == id);
                    if (cc == null) throw new Exception("cuentas_contables_not_found");

                    cc.Codigo = model.Codigo;
                    cc.Nombre = model.Nombre;
                    cc.Tipo_Cuenta_Contable_id = model.Tipo_Cuenta_Contable_id;
                    cc.Estado = model.Estado;
                    cc.Saldo_inicial = model.Saldo_inicial;
                    cc.Saldo_actual = model.Saldo_actual;
                    cc.Fecha_actualizacion = DateTime.Now;

                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/cuentas_contables")]
        public Reply GetAllCuentasContables()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Cuentas_Contables.Select(x => new {
                        x.id,
                        x.Codigo,
                        x.Nombre,
                        x.Tipo_Cuenta_Contable_id,
                        x.Usuarios_Usuario_id,
                        x.Estado,
                        x.Saldo_inicial,
                        x.Saldo_actual,
                        x.Fecha_Creacion,
                        x.Fecha_actualizacion
                    }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/cuentas_contables/{id}")]
        public Reply GetCuentasContablesById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Cuentas_Contables.Where(x => x.id == id).Select(x => new {
                        x.id,
                        x.Codigo,
                        x.Nombre,
                        x.Tipo_Cuenta_Contable_id,
                        x.Usuarios_Usuario_id,
                        x.Estado,
                        x.Saldo_inicial,
                        x.Saldo_actual,
                        x.Fecha_Creacion,
                        x.Fecha_actualizacion
                    }).FirstOrDefault();
                    if (data == null) throw new Exception("cuentas_contables_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        #region "Codigos_cabys"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/codigos_cabys")]
        public Reply CreateCodigosCabys([FromBody] Models.Codigos_cabys model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                if (!tool.validaNumeros(model.Impuesto_id.ToString())) throw new Exception("invalid_value_form_Impuesto_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Codigos_cabys cc = new Models.Codigos_cabys()
                    {
                        codigo = model.codigo,
                        Nombre = model.Nombre,
                        Impuesto_id = model.Impuesto_id
                    };
                    ctx.Codigos_cabys.Add(cc);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/codigos_cabys/{id}")]
        public Reply UpdateCodigosCabys(int id, [FromBody] Models.Codigos_cabys model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                if (!tool.validaNumeros(model.Impuesto_id.ToString())) throw new Exception("invalid_value_form_Impuesto_id");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Codigos_cabys cc = ctx.Codigos_cabys.FirstOrDefault(x => x.id == id);
                    if (cc == null) throw new Exception("codigos_cabys_not_found");
                    cc.codigo = model.codigo;
                    cc.Nombre = model.Nombre;
                    cc.Impuesto_id = model.Impuesto_id;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/codigos_cabys")]
        public Reply GetAllCodigosCabys()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Codigos_cabys.Select(x => new { x.id, x.codigo, x.Nombre, x.Impuesto_id }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/codigos_cabys/{id}")]
        public Reply GetCodigosCabysById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Codigos_cabys.Where(x => x.id == id)
                        .Select(x => new { x.id, x.codigo, x.Nombre, x.Impuesto_id }).FirstOrDefault();
                    if (data == null) throw new Exception("codigos_cabys_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        #region "Codigo_comercial"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/codigo_comercial")]
        public Reply CreateCodigoComercial([FromBody] Models.Codigo_comercial model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Codigo_comercial cc = new Models.Codigo_comercial() { Codigo = model.Codigo, Nombre = model.Nombre };
                    ctx.Codigo_comercial.Add(cc);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/codigo_comercial/{id}")]
        public Reply UpdateCodigoComercial(int id, [FromBody] Models.Codigo_comercial model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Codigo_comercial cc = ctx.Codigo_comercial.FirstOrDefault(x => x.id == id);
                    if (cc == null) throw new Exception("codigo_comercial_not_found");
                    cc.Codigo = model.Codigo;
                    cc.Nombre = model.Nombre;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/codigo_comercial")]
        public Reply GetAllCodigoComercial()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Codigo_comercial.Select(x => new { x.id, x.Codigo, x.Nombre }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/codigo_comercial/{id}")]
        public Reply GetCodigoComercialById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Codigo_comercial.Where(x => x.id == id)
                        .Select(x => new { x.id, x.Codigo, x.Nombre }).FirstOrDefault();
                    if (data == null) throw new Exception("codigo_comercial_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        #region "Centro_Costos"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/centro_costos")]
        public Reply CreateCentroCostos([FromBody] Models.Centro_Costos model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                if (!tool.ValidaTexto(model.Seudonimo)) throw new Exception("invalid_string_form_Seudonimo");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Centro_Costos cc = new Models.Centro_Costos()
                    {
                        codigo = model.codigo,
                        Nombre = model.Nombre,
                        Seudonimo = model.Seudonimo
                    };
                    ctx.Centro_Costos.Add(cc);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/centro_costos/{id}")]
        public Reply UpdateCentroCostos(int id, [FromBody] Models.Centro_Costos model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                if (!tool.ValidaTexto(model.Seudonimo)) throw new Exception("invalid_string_form_Seudonimo");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Centro_Costos cc = ctx.Centro_Costos.FirstOrDefault(x => x.id == id);
                    if (cc == null) throw new Exception("centro_costos_not_found");
                    cc.codigo = model.codigo;
                    cc.Nombre = model.Nombre;
                    cc.Seudonimo = model.Seudonimo;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = cc.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/centro_costos")]
        public Reply GetAllCentroCostos()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Centro_Costos.Select(x => new { x.id, x.codigo, x.Nombre, x.Seudonimo }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/centro_costos/{id}")]
        public Reply GetCentroCostosById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Centro_Costos.Where(x => x.id == id)
                        .Select(x => new { x.id, x.codigo, x.Nombre, x.Seudonimo }).FirstOrDefault();
                    if (data == null) throw new Exception("centro_costos_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        #region "Tipo_moneda"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/tipo_moneda")]
        public Reply CreateTipoMoneda([FromBody] Models.Tipo_moneda model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo_moneda)) throw new Exception("invalid_string_form_codigo_moneda");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Tipo_moneda tm = new Models.Tipo_moneda() { codigo_moneda = model.codigo_moneda, Nombre = model.Nombre };
                    ctx.Tipo_moneda.Add(tm);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = tm.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/tipo_moneda/{id}")]
        public Reply UpdateTipoMoneda(int id, [FromBody] Models.Tipo_moneda model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.codigo_moneda)) throw new Exception("invalid_string_form_codigo_moneda");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Tipo_moneda tm = ctx.Tipo_moneda.FirstOrDefault(x => x.id == id);
                    if (tm == null) throw new Exception("tipo_moneda_not_found");
                    tm.codigo_moneda = model.codigo_moneda;
                    tm.Nombre = model.Nombre;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = tm.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_moneda")]
        public Reply GetAllTipoMoneda()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Tipo_moneda.Select(x => new { x.id, x.codigo_moneda, x.Nombre }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_moneda/{id}")]
        public Reply GetTipoMonedaById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Tipo_moneda.Where(x => x.id == id)
                        .Select(x => new { x.id, x.codigo_moneda, x.Nombre }).FirstOrDefault();
                    if (data == null) throw new Exception("tipo_moneda_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        #region "Tipo_documento"

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/tipo_documento")]
        public Reply CreateTipoDocumento([FromBody] Models.Tipo_documento model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo_doc)) throw new Exception("invalid_string_form_Codigo_doc");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Tipo_documento td = new Models.Tipo_documento() { Codigo_doc = model.Codigo_doc, Nombre = model.Nombre };
                    ctx.Tipo_documento.Add(td);
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = td.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/tipo_documento/{id}")]
        public Reply UpdateTipoDocumento(int id, [FromBody] Models.Tipo_documento model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Codigo_doc)) throw new Exception("invalid_string_form_Codigo_doc");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Tipo_documento td = ctx.Tipo_documento.FirstOrDefault(x => x.id == id);
                    if (td == null) throw new Exception("tipo_documento_not_found");
                    td.Codigo_doc = model.Codigo_doc;
                    td.Nombre = model.Nombre;
                    ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = td.id;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_documento")]
        public Reply GetAllTipoDocumento()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Tipo_documento.Select(x => new { x.id, x.Codigo_doc, x.Nombre }).ToList();
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_documento/{id}")]
        public Reply GetTipoDocumentoById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                if (id <= 0) throw new Exception("invalid_value_for_id");
                using (var ctx = new Models.EntitiesModel())
                {
                    var data = ctx.Tipo_documento.Where(x => x.id == id)
                        .Select(x => new { x.id, x.Codigo_doc, x.Nombre }).FirstOrDefault();
                    if (data == null) throw new Exception("tipo_documento_not_found");
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = data;
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
                oR.Message = errorDB;
                return oR;
            }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }
        #endregion

        // Los siguientes catálogos (Categoria_gasto, Tipo_archivo, Estado_Factura, Unidad_medida,
        // Medio_pago, Permisos, Categoria_presupuestaria, Roles, Condicion_venta, Impuesto,
        // Tipo_Cuenta_Contable) siguen exactamente el mismo patrón. Se listan sus rutas y campos:

        #region "Categoria_gasto"
        // POST   api/v1/catalogos/categoria_gasto          → Nombre
        // PUT    api/v1/catalogos/categoria_gasto/{id}     → Nombre
        // GET    api/v1/catalogos/categoria_gasto          → id, Nombre
        // GET    api/v1/catalogos/categoria_gasto/{id}     → id, Nombre

        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/categoria_gasto")]
        public Reply CreateCategoriaGasto([FromBody] Models.Categoria_gasto model)
        {
            Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Categoria_gasto e = new Models.Categoria_gasto() { Nombre = model.Nombre };
                    ctx.Categoria_gasto.Add(e); ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2) { String errorDB = ""; foreach (var eve in ex2.EntityValidationErrors) foreach (var ve in eve.ValidationErrors) errorDB += ve.ErrorMessage; oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = errorDB; return oR; }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/categoria_gasto/{id}")]
        public Reply UpdateCategoriaGasto(int id, [FromBody] Models.Categoria_gasto model)
        {
            Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General();
            try
            {
                if (model == null) throw new Exception("invalid_model_request_missing");
                if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre");
                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Categoria_gasto e = ctx.Categoria_gasto.FirstOrDefault(x => x.id == id);
                    if (e == null) throw new Exception("categoria_gasto_not_found");
                    e.Nombre = model.Nombre; ctx.SaveChanges();
                    oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex2) { String errorDB = ""; foreach (var eve in ex2.EntityValidationErrors) foreach (var ve in eve.ValidationErrors) errorDB += ve.ErrorMessage; oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = errorDB; return oR; }
            catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; }
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/categoria_gasto")]
        public Reply GetAllCategoriaGasto() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Categoria_gasto.Select(x => new { x.id, x.Nombre }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/categoria_gasto/{id}")]
        public Reply GetCategoriaGastoById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Categoria_gasto.Where(x => x.id == id).Select(x => new { x.id, x.Nombre }).FirstOrDefault(); if (data == null) throw new Exception("categoria_gasto_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Tipo_archivo"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/tipo_archivo")]
        public Reply CreateTipoArchivo([FromBody] Models.Tipo_archivo model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Tipo_archivo e = new Models.Tipo_archivo() { Nombre = model.Nombre }; ctx.Tipo_archivo.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/tipo_archivo/{id}")]
        public Reply UpdateTipoArchivo(int id, [FromBody] Models.Tipo_archivo model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Tipo_archivo e = ctx.Tipo_archivo.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("tipo_archivo_not_found"); e.Nombre = model.Nombre; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_archivo")]
        public Reply GetAllTipoArchivo() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Tipo_archivo.Select(x => new { x.id, x.Nombre }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_archivo/{id}")]
        public Reply GetTipoArchivoById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Tipo_archivo.Where(x => x.id == id).Select(x => new { x.id, x.Nombre }).FirstOrDefault(); if (data == null) throw new Exception("tipo_archivo_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Estado_Factura"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/estado_factura")]
        public Reply CreateEstadoFactura([FromBody] Models.Estado_Factura model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Estado_Factura e = new Models.Estado_Factura() { Nombre = model.Nombre }; ctx.Estado_Factura.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/estado_factura/{id}")]
        public Reply UpdateEstadoFactura(int id, [FromBody] Models.Estado_Factura model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Estado_Factura e = ctx.Estado_Factura.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("estado_factura_not_found"); e.Nombre = model.Nombre; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/estado_factura")]
        public Reply GetAllEstadoFactura() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Estado_Factura.Select(x => new { x.id, x.Nombre }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/estado_factura/{id}")]
        public Reply GetEstadoFacturaById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Estado_Factura.Where(x => x.id == id).Select(x => new { x.id, x.Nombre }).FirstOrDefault(); if (data == null) throw new Exception("estado_factura_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Unidad_medida"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/unidad_medida")]
        public Reply CreateUnidadMedida([FromBody] Models.Unidad_medida model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Unidad_medida e = new Models.Unidad_medida() { Codigo = model.Codigo, Nombre = model.Nombre }; ctx.Unidad_medida.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/unidad_medida/{id}")]
        public Reply UpdateUnidadMedida(int id, [FromBody] Models.Unidad_medida model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); using (var ctx = new Models.EntitiesModel()) { Models.Unidad_medida e = ctx.Unidad_medida.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("unidad_medida_not_found"); e.Codigo = model.Codigo; e.Nombre = model.Nombre; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/unidad_medida")]
        public Reply GetAllUnidadMedida() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Unidad_medida.Select(x => new { x.id, x.Codigo, x.Nombre }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/unidad_medida/{id}")]
        public Reply GetUnidadMedidaById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Unidad_medida.Where(x => x.id == id).Select(x => new { x.id, x.Codigo, x.Nombre }).FirstOrDefault(); if (data == null) throw new Exception("unidad_medida_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Medio_pago"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/medio_pago")]
        public Reply CreateMedioPago([FromBody] Models.Medio_pago model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo"); if (!tool.ValidaTexto(model.descripcion)) throw new Exception("invalid_string_form_descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Medio_pago e = new Models.Medio_pago() { codigo = model.codigo, descripcion = model.descripcion }; ctx.Medio_pago.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/medio_pago/{id}")]
        public Reply UpdateMedioPago(int id, [FromBody] Models.Medio_pago model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo"); if (!tool.ValidaTexto(model.descripcion)) throw new Exception("invalid_string_form_descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Medio_pago e = ctx.Medio_pago.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("medio_pago_not_found"); e.codigo = model.codigo; e.descripcion = model.descripcion; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/medio_pago")]
        public Reply GetAllMedioPago() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Medio_pago.Select(x => new { x.id, x.codigo, x.descripcion }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/medio_pago/{id}")]
        public Reply GetMedioPagoById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Medio_pago.Where(x => x.id == id).Select(x => new { x.id, x.codigo, x.descripcion }).FirstOrDefault(); if (data == null) throw new Exception("medio_pago_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Permisos"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/permisos")]
        public Reply CreatePermisos([FromBody] Models.Permisos model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Permisos e = new Models.Permisos() { Nombre = model.Nombre, Descripcion = model.Descripcion }; ctx.Permisos.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/permisos/{id}")]
        public Reply UpdatePermisos(int id, [FromBody] Models.Permisos model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Permisos e = ctx.Permisos.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("permisos_not_found"); e.Nombre = model.Nombre; e.Descripcion = model.Descripcion; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/permisos")]
        public Reply GetAllPermisos() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Permisos.Select(x => new { x.id, x.Nombre, x.Descripcion }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/permisos/{id}")]
        public Reply GetPermisosById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Permisos.Where(x => x.id == id).Select(x => new { x.id, x.Nombre, x.Descripcion }).FirstOrDefault(); if (data == null) throw new Exception("permisos_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Categoria_presupuestaria"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/categoria_presupuestaria")]
        public Reply CreateCategoriaPresupuestaria([FromBody] Models.Categoria_presupuestaria model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.nombre)) throw new Exception("invalid_string_form_nombre"); if (!tool.ValidaTexto(model.tipo_categoria)) throw new Exception("invalid_string_form_tipo_categoria"); using (var ctx = new Models.EntitiesModel()) { Models.Categoria_presupuestaria e = new Models.Categoria_presupuestaria() { nombre = model.nombre, tipo_categoria = model.tipo_categoria }; ctx.Categoria_presupuestaria.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/categoria_presupuestaria/{id}")]
        public Reply UpdateCategoriaPresupuestaria(int id, [FromBody] Models.Categoria_presupuestaria model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.nombre)) throw new Exception("invalid_string_form_nombre"); if (!tool.ValidaTexto(model.tipo_categoria)) throw new Exception("invalid_string_form_tipo_categoria"); using (var ctx = new Models.EntitiesModel()) { Models.Categoria_presupuestaria e = ctx.Categoria_presupuestaria.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("categoria_presupuestaria_not_found"); e.nombre = model.nombre; e.tipo_categoria = model.tipo_categoria; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/categoria_presupuestaria")]
        public Reply GetAllCategoriaPresupuestaria() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Categoria_presupuestaria.Select(x => new { x.id, x.nombre, x.tipo_categoria }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/categoria_presupuestaria/{id}")]
        public Reply GetCategoriaPresupuestariaById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Categoria_presupuestaria.Where(x => x.id == id).Select(x => new { x.id, x.nombre, x.tipo_categoria }).FirstOrDefault(); if (data == null) throw new Exception("categoria_presupuestaria_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Roles"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/roles")]
        public Reply CreateRoles([FromBody] Models.Roles model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Roles e = new Models.Roles() { Descripcion = model.Descripcion }; ctx.Roles.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/roles/{id}")]
        public Reply UpdateRoles(int id, [FromBody] Models.Roles model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Roles e = ctx.Roles.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("roles_not_found"); e.Descripcion = model.Descripcion; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/roles")]
        public Reply GetAllRoles() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Roles.Select(x => new { x.id, x.Descripcion }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/roles/{id}")]
        public Reply GetRolesById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Roles.Where(x => x.id == id).Select(x => new { x.id, x.Descripcion }).FirstOrDefault(); if (data == null) throw new Exception("roles_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Condicion_venta"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/condicion_venta")]
        public Reply CreateCondicionVenta([FromBody] Models.Condicion_venta model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Condicion_venta e = new Models.Condicion_venta() { Codigo = model.Codigo, Descripcion = model.Descripcion }; ctx.Condicion_venta.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/condicion_venta/{id}")]
        public Reply UpdateCondicionVenta(int id, [FromBody] Models.Condicion_venta model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Codigo)) throw new Exception("invalid_string_form_Codigo"); if (!tool.ValidaTexto(model.Descripcion)) throw new Exception("invalid_string_form_Descripcion"); using (var ctx = new Models.EntitiesModel()) { Models.Condicion_venta e = ctx.Condicion_venta.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("condicion_venta_not_found"); e.Codigo = model.Codigo; e.Descripcion = model.Descripcion; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/condicion_venta")]
        public Reply GetAllCondicionVenta() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Condicion_venta.Select(x => new { x.id, x.Codigo, x.Descripcion }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/condicion_venta/{id}")]
        public Reply GetCondicionVentaById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Condicion_venta.Where(x => x.id == id).Select(x => new { x.id, x.Codigo, x.Descripcion }).FirstOrDefault(); if (data == null) throw new Exception("condicion_venta_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Impuesto"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/impuesto")]
        public Reply CreateImpuesto([FromBody] Models.Impuesto model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.validaNumeros(model.Porcentaje.ToString())) throw new Exception("invalid_value_form_Porcentaje"); if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo"); using (var ctx = new Models.EntitiesModel()) { Models.Impuesto e = new Models.Impuesto() { Nombre = model.Nombre, Porcentaje = model.Porcentaje, codigo = model.codigo }; ctx.Impuesto.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/impuesto/{id}")]
        public Reply UpdateImpuesto(int id, [FromBody] Models.Impuesto model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.validaNumeros(model.Porcentaje.ToString())) throw new Exception("invalid_value_form_Porcentaje"); if (!tool.ValidaTexto(model.codigo)) throw new Exception("invalid_string_form_codigo"); using (var ctx = new Models.EntitiesModel()) { Models.Impuesto e = ctx.Impuesto.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("impuesto_not_found"); e.Nombre = model.Nombre; e.Porcentaje = model.Porcentaje; e.codigo = model.codigo; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/impuesto")]
        public Reply GetAllImpuesto() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Impuesto.Select(x => new { x.id, x.Nombre, x.Porcentaje, x.codigo }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/impuesto/{id}")]
        public Reply GetImpuestoById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Impuesto.Where(x => x.id == id).Select(x => new { x.id, x.Nombre, x.Porcentaje, x.codigo }).FirstOrDefault(); if (data == null) throw new Exception("impuesto_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

        #region "Tipo_Cuenta_Contable"
        [HttpPost]
        [Authorize]
        [Route("api/v1/catalogos/tipo_cuenta_contable")]
        public Reply CreateTipoCuentaContable([FromBody] Models.Tipo_Cuenta_Contable model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.ValidaTexto(model.Naturaleza)) throw new Exception("invalid_string_form_Naturaleza"); using (var ctx = new Models.EntitiesModel()) { Models.Tipo_Cuenta_Contable e = new Models.Tipo_Cuenta_Contable() { Nombre = model.Nombre, Naturaleza = model.Naturaleza }; ctx.Tipo_Cuenta_Contable.Add(e); ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpPut]
        [Authorize]
        [Route("api/v1/catalogos/tipo_cuenta_contable/{id}")]
        public Reply UpdateTipoCuentaContable(int id, [FromBody] Models.Tipo_Cuenta_Contable model) { Reply oR = new Reply(); oR.CodeStatus = 0; General tool = new General(); try { if (model == null) throw new Exception("invalid_model_request_missing"); if (!tool.ValidaTexto(model.Nombre)) throw new Exception("invalid_string_form_Nombre"); if (!tool.ValidaTexto(model.Naturaleza)) throw new Exception("invalid_string_form_Naturaleza"); using (var ctx = new Models.EntitiesModel()) { Models.Tipo_Cuenta_Contable e = ctx.Tipo_Cuenta_Contable.FirstOrDefault(x => x.id == id); if (e == null) throw new Exception("tipo_cuenta_contable_not_found"); e.Nombre = model.Nombre; e.Naturaleza = model.Naturaleza; ctx.SaveChanges(); oR.CodeStatus = HttpStatusCode.OK; oR.Data = e.id; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_cuenta_contable")]
        public Reply GetAllTipoCuentaContable() { Reply oR = new Reply(); oR.CodeStatus = 0; try { using (var ctx = new Models.EntitiesModel()) { oR.CodeStatus = HttpStatusCode.OK; oR.Data = ctx.Tipo_Cuenta_Contable.Select(x => new { x.id, x.Nombre, x.Naturaleza }).ToList(); return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }

        [HttpGet]
        [Authorize]
        [Route("api/v1/catalogos/tipo_cuenta_contable/{id}")]
        public Reply GetTipoCuentaContableById(int id) { Reply oR = new Reply(); oR.CodeStatus = 0; try { if (id <= 0) throw new Exception("invalid_value_for_id"); using (var ctx = new Models.EntitiesModel()) { var data = ctx.Tipo_Cuenta_Contable.Where(x => x.id == id).Select(x => new { x.id, x.Nombre, x.Naturaleza }).FirstOrDefault(); if (data == null) throw new Exception("tipo_cuenta_contable_not_found"); oR.CodeStatus = HttpStatusCode.OK; oR.Data = data; return oR; } } catch (Exception ex) { oR.CodeStatus = HttpStatusCode.InternalServerError; oR.Message = ex.Message; return oR; } }
        #endregion

    }
}