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
using System.Net.Http;

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
                   // pin_llave = u.pin_llave,
                    ruta_logo = u.ruta_logo,
                    terminal = u.terminal,
                    codigo_seguridad = u.codigo_seguridad,
                    identificacion = u.identificacion,
                    codigo_actividad_id =u.codigo_actividad_id,
                    tipo_identificacion_id = u.tipo_identificacion_id,
                    Impuesto_id = u.Impuesto_id,
                    Provincia_id= u.Provincia_id,
                    Canton_id = u.Canton_id,
                    Distrito_id = u.Distrito_id,
                    OtrasSenas_Emisor = u.OtrasSenas,
                            Codigo_Telefono = u.Codigo_telefono,
                            Telefono = u.Telefono,
                            Correo_smtp = u.Correo_smtp,
                          //  Contrasena_smtp = u.Contrasena_smtp,
                            Proveedor_SMTP = u.Proveedor_SMTP,
                            Puerto_SMTP = u.Puerto_SMTP,
                            Asunto_SMTP = u.Asunto_SMTP,
                            Usuario_hacienda = u.Usuario_hacienda,
                           // Contrasena_hacienda = u.Contrasena_hacienda


                        })
                        .FirstOrDefault();

                    if (empresa == null)
                    {
                        throw new Exception("empresa_not_found");
                    }

                 //   empresa.pin_llave = (empresa.pin_llave == String.Empty ? "" : tool.Desencriptar(empresa.pin_llave));
                    empresa.Ruta_llave_factura = string.IsNullOrEmpty(empresa.Ruta_llave_factura) ? "" : System.IO.Path.GetFileName(empresa.Ruta_llave_factura);

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

                if (!tool.validaNumeros(model.Telefono))
                {
                    throw new Exception("invalid_value_form_Telefono");
                }
                if (!tool.ValidaTexto(model.Codigo_Telefono))
                {
                    throw new Exception("invalid_value_form_codigo_telefono");
                }

                if (!tool.validaNumeros(model.Provincia_id.ToString()) || !tool.validaNumeros(model.Canton_id.ToString()) || !tool.validaNumeros(model.Distrito_id.ToString()))
                {
                    throw new Exception("invalid_value_form_provincia_canton_distrito");
                }

                if (!tool.validaNumeros(model.Provincia_id.ToString()) || !tool.validaNumeros(model.Canton_id.ToString()) || !tool.validaNumeros(model.Distrito_id.ToString()))
                {
                    throw new Exception("invalid_value_form_provincia_canton_distrito");
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
                    //empresaExistente.Ruta_llave_factura = model.Ruta_llave_factura;
                    empresaExistente.ruta_logo = model.ruta_logo;
                    empresaExistente.terminal = model.terminal;
                    empresaExistente.codigo_seguridad = model.codigo_seguridad;
                    empresaExistente.identificacion = model.identificacion;
                    empresaExistente.codigo_actividad_id = model.codigo_actividad_id;
                    empresaExistente.tipo_identificacion_id = model.tipo_identificacion_id;
                    empresaExistente.Impuesto_id = model.Impuesto_id;
                    empresaExistente.Provincia_id = model.Provincia_id;
                    empresaExistente.Canton_id = model.Canton_id;

                    empresaExistente.Distrito_id = model.Distrito_id;

                    empresaExistente.OtrasSenas = model.OtrasSenas_Emisor;

                    empresaExistente.Codigo_telefono = model.Codigo_Telefono;
                    empresaExistente.Telefono = model.Telefono;
                    empresaExistente.Correo_smtp = model.Correo_smtp;
                    empresaExistente.Proveedor_SMTP = model.Proveedor_SMTP;
                    empresaExistente.Puerto_SMTP = model.Puerto_SMTP;
                    empresaExistente.Asunto_SMTP = model.Asunto_SMTP;
                    empresaExistente.Usuario_hacienda = model.Usuario_hacienda;

                    // Pin de llave solo se actualiza si se envia un valor nuevo
                    if (!string.IsNullOrEmpty(model.pin_llave))
                    {
                        empresaExistente.pin_llave = tool.Encriptar(model.pin_llave);
                    }

                    if (!string.IsNullOrEmpty(model.Contrasena_hacienda))
                    {
                        empresaExistente.Contrasena_hacienda = tool.Encriptar(model.Contrasena_hacienda);
                    }

                    if (!string.IsNullOrEmpty(model.Contrasena_smtp))
                    {
                        empresaExistente.Contrasena_smtp = tool.Encriptar(model.Contrasena_smtp);
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


        [HttpPost]
        [Authorize]
        [Route("api/v1/empresa/upload-llave")]
        public Reply UploadLlaveFactura([FromBody] UploadLlaveViewModel model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            try
            {
                // ── Validar modelo
                if (model == null)
                    throw new Exception("invalid_model_request_missing");

                if (string.IsNullOrEmpty(model.file))
                    throw new Exception("invalid_file_not_found");

                if (string.IsNullOrEmpty(model.fileName))
                    throw new Exception("invalid_file_name_missing");

                // ── Validar extensión .p12
                string extension = System.IO.Path.GetExtension(model.fileName).ToLower();
                if (extension != ".p12")
                    throw new Exception("invalid_file_extension_must_be_p12");

                // ── Limpiar Base64 (quitar prefijo data:...;base64, si viene)
                string base64 = model.file;
                if (base64.Contains(","))
                    base64 = base64.Split(',')[1];

                // ── Convertir Base64 a bytes
                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(base64);
                }
                catch
                {
                    throw new Exception("invalid_file_base64_format");
                }

                if (fileBytes.Length == 0)
                    throw new Exception("invalid_file_empty");

                // ── Obtener la ruta NAS desde la BD
                string rutaNas;
                using (var ctx = new Models.EntitiesModel())
                {
                    ctx.Configuration.LazyLoadingEnabled = false;
                    ctx.Configuration.ProxyCreationEnabled = false;

                    var empresa = ctx.Empresa
                        .Where(e => e.Emp_id == 1)
                        .Select(e => new { e.Ruta_nas })
                        .FirstOrDefault();

                    if (empresa == null)
                        throw new Exception("empresa_not_found");

                    rutaNas = empresa.Ruta_nas;
                }

                if (string.IsNullOrEmpty(rutaNas))
                    throw new Exception("ruta_nas_not_configured");

                // ── Crear carpeta si no existe
                if (!System.IO.Directory.Exists(rutaNas))
                    System.IO.Directory.CreateDirectory(rutaNas);

                // ── Construir ruta completa y guardar
                string nombreArchivo = System.IO.Path.GetFileName(model.fileName);
                string rutaCompleta = System.IO.Path.Combine(rutaNas, nombreArchivo);

                System.IO.File.WriteAllBytes(rutaCompleta, fileBytes);

                // ── Actualizar Ruta_llave_factura en BD
                using (var ctx = new Models.EntitiesModel())
                {
                  //  ctx.Configuration.LazyLoadingEnabled = false;
                    //ctx.Configuration.ProxyCreationEnabled = false;

                    var empresaExistente = ctx.Empresa.FirstOrDefault(e => e.Emp_id == 1);
                    if (empresaExistente != null)
                    {
                        empresaExistente.Ruta_llave_factura = rutaCompleta;
                        ctx.SaveChanges();
                    }
                }

                oR.CodeStatus = HttpStatusCode.OK;
                oR.Data = new
                {
                    ruta_llave_factura = rutaCompleta,
                    nombre_archivo = nombreArchivo
                };
                return oR;
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