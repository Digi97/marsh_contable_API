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
  
    public class EmpresaController: ApiController
    {

        [HttpGet]
        [Authorize]
        [Route("api/v1/empresa/{id}")]
        public Reply GetEmpresaById(int id)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            id = 1; //empresa por default tendra valor de 1
            General tool = new General();
            try
            {
                if (id <= 0)
                {
                    throw new Exception("invalid_value_for_id");
                }

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.EmpresaViewModel empresa = ctx.Empresa
                        .Where(u => u.Emp_id == id)
                        .Select(u => new Models.EmpresaViewModel
                        {
                    Emp_id = u.Emp_id,
                    Nombre_empresa = u.Nombre_empresa,
                    Correo_empresa =u.Correo_empresa,
                    Ruta_nas = u.Ruta_nas,
                    Numero_sucursal = u.Numero_sucursal,
                    Formato_fecha = u.Formato_fecha,
                    Ruta_llave_factura = u.Ruta_llave_factura,
                    pin_llave = u.pin_llave,
                    ruta_logo = u.ruta_logo,
                    terminal = u.terminal,
                    codigo_seguridad = u.codigo_seguridad,
                    identificacion = u.identificacion,
                    codigo_actividad_id =u.codigo_actividad_id,
                    tipo_identificacion_id = u.tipo_identificacion_id,
                    Impuesto_id = u.Impuesto_id
                        })
                        .FirstOrDefault();

                    if (empresa == null)
                    {
                        throw new Exception("empresa_not_found");
                    }

                    empresa.pin_llave = (empresa.pin_llave == String.Empty ? "" : tool.Desencriptar(empresa.pin_llave));

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = empresa;
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


        [HttpPut]
        [Route("api/v1/empresa/{id}")]
        [RequierePermiso(PermisosAplica.UsuarioMantenimiento)]
        public Reply UpdateEmpresa(int id, [FromBody] Models.EmpresaViewModel model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
            id = 1; // empresa por default tendra valor de 1

            try
            {
                if (model == null)
                {
                    throw new Exception("invalid_model_request_missing");
                }
                if (id <= 0)
                {
                    throw new Exception("invalid_value_for_id");
                }

                // Seccion de validacion de datos
                if (!tool.ValidaTexto(model.Nombre_empresa))
                {
                    throw new Exception("invalid_string_form_Nombre_empresa");
                }
                if (!tool.ValidaCorreo(model.Correo_empresa))
                {
                    throw new Exception("invalid_string_form_Correo_empresa");
                }
                if (!tool.ValidaRuta(model.Ruta_nas))
                {
                    throw new Exception("invalid_string_form_Ruta_nas");
                }
                //if (!tool.ValidaTexto(model.Ruta_llave_factura))
                //{
                //    throw new Exception("invalid_string_form_Ruta_llave_factura");
                //}
                if (!tool.ValidaTexto(model.identificacion))
                {
                    throw new Exception("invalid_string_form_identificacion");
                }
                if (!tool.validaNumeros(model.codigo_actividad_id.ToString()))
                {
                    throw new Exception("invalid_value_form_codigo_actividad_id");
                }
                if (!tool.validaNumeros(model.tipo_identificacion_id.ToString()))
                {
                    throw new Exception("invalid_value_form_tipo_identificacion_id");
                }
                if (!tool.validaNumeros(model.Impuesto_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Impuesto_id");
                }
                // fin de validaciones

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Empresa empresaExistente = ctx.Empresa.FirstOrDefault(e => e.Emp_id == id);

                    if (empresaExistente == null)
                    {
                        throw new Exception("empresa_not_found");
                    }

                    // Actualizar campos
                    empresaExistente.Nombre_empresa = model.Nombre_empresa;
                    empresaExistente.Correo_empresa = model.Correo_empresa;
                    empresaExistente.Ruta_nas = model.Ruta_nas;
                    empresaExistente.Numero_sucursal = model.Numero_sucursal;
                    empresaExistente.Formato_fecha = model.Formato_fecha;
                    empresaExistente.Ruta_llave_factura = model.Ruta_llave_factura;
                    empresaExistente.ruta_logo = model.ruta_logo;
                    empresaExistente.terminal = model.terminal;
                    empresaExistente.codigo_seguridad = model.codigo_seguridad;
                    empresaExistente.identificacion = model.identificacion;
                    empresaExistente.codigo_actividad_id = model.codigo_actividad_id;
                    empresaExistente.tipo_identificacion_id = model.tipo_identificacion_id;
                    empresaExistente.Impuesto_id = model.Impuesto_id;

                    // Pin de llave solo se actualiza si se envia un valor nuevo
                    if (!string.IsNullOrEmpty(model.pin_llave))
                    {
                        empresaExistente.pin_llave = tool.Encriptar(model.pin_llave);
                    }

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = empresaExistente.Emp_id;
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

    }
}