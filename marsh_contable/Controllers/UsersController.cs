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
    public class UsersController : ApiController
    {

      

        [HttpPost]
        [Authorize]
        [Route("api/v1/users")]
        public Reply CreateUser([FromBody] Models.Usuarios model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
          
           
            try
            {

                if(model==null)
                {
                    throw new Exception("invalid_model_request_missing");
                }
                // Seccion de validacion de datos 
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }
                if (!tool.ValidaTexto(model.Apellido1))
                {
                    throw new Exception("invalid_string_form_Apellido1");
                }

                if (!tool.ValidaTexto(model.Apellido2))
                {
                    throw new Exception("invalid_string_form_Apellido2");
                }

                if (!tool.ValidaCorreo(model.Correo))
                {
                    throw new Exception("invalid_string_form_Correo");
                }
               
                if (!tool.validaNumeros(model.Roles_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Roles_id");
                }


                if (!tool.validaPassword(model.Contrasena))
                {
                    throw new Exception("invalid_value_for_Contrasena");
                }
                //fin de validaciones


                String password = tool.Encriptar(model.Contrasena);

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Usuarios nuevoUsuario = new Models.Usuarios()
                    {
                        Nombre = model.Nombre,
                        Apellido1 = model.Apellido1,
                        Apellido2 = model.Apellido2,
                        Correo = model.Correo,
                        Contrasena = password,
                        Roles_id = model.Roles_id,
                        Id_Empleado = model.Id_Empleado,
                        activo = (Int16) model.activo,
                        Empresa_Emp_id = 1

                    };

                    ctx.Usuarios.Add(nuevoUsuario);
                    ctx.SaveChanges();

            
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = nuevoUsuario.Usuario_id; // retorna el ID generado                 

                }

            }
            catch (Exception ex)

            {

                System.Data.Entity.Validation.DbEntityValidationException ex2 = new System.Data.Entity.Validation.DbEntityValidationException();
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                { 
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
       
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message +" "+ errorDB;
            }
            return oR;
        }


        [HttpPut]
        [Authorize]
        [Route("api/v1/users/{id}")]
        public Reply UpdateUser(int id, [FromBody] Models.Usuarios model)
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
                if (!tool.ValidaTexto(model.Nombre))
                {
                    throw new Exception("invalid_string_form_Nombre");
                }
                if (!tool.ValidaTexto(model.Apellido1))
                {
                    throw new Exception("invalid_string_form_Apellido1");
                }
                if (!tool.ValidaTexto(model.Apellido2))
                {
                    throw new Exception("invalid_string_form_Apellido2");
                }
                if (!tool.ValidaCorreo(model.Correo))
                {
                    throw new Exception("invalid_string_form_Correo");
                }
                if (!tool.validaNumeros(model.Roles_id.ToString()))
                {
                    throw new Exception("invalid_value_form_Roles_id");
                }
                // fin de validaciones

                using (var ctx = new Models.EntitiesModel())
                {
                    Models.Usuarios usuarioExistente = ctx.Usuarios.FirstOrDefault(u => u.Usuario_id == id);

                    if (usuarioExistente == null)
                    {
                        throw new Exception("user_not_found");
                    }

                    usuarioExistente.Nombre = model.Nombre;
                    usuarioExistente.Apellido1 = model.Apellido1;
                    usuarioExistente.Apellido2 = model.Apellido2;
                    usuarioExistente.Correo = model.Correo;
                    usuarioExistente.Roles_id = model.Roles_id;
                    usuarioExistente.Id_Empleado = model.Id_Empleado;
                    usuarioExistente.activo = (Int16)model.activo;

                    // Solo re-encripta la contraseña si se envía una nueva
                    if (!string.IsNullOrEmpty(model.Contrasena))
                    {
                        if (!tool.validaPassword(model.Contrasena))
                        {
                            throw new Exception("invalid_value_for_Contrasena");
                        }
                        usuarioExistente.Contrasena = tool.Encriptar(model.Contrasena);
                    }

                    ctx.SaveChanges();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = usuarioExistente.Usuario_id;
                }
            }
            catch (Exception ex)
            {
                System.Data.Entity.Validation.DbEntityValidationException ex2 = new System.Data.Entity.Validation.DbEntityValidationException();
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message + " " + errorDB;
            }
            return oR;
        }


        [HttpGet]
        [Authorize]
        [Route("api/v1/users")]
        public Reply GetAllUsers()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;

            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    List<Models.UsuariosViewModel> usuarios = ctx.Usuarios
                        .Select(u => new Models.UsuariosViewModel
                        {
                            Usuario_id = u.Usuario_id,
                            Nombre = u.Nombre,
                            Apellido1 = u.Apellido1,
                            Apellido2 = u.Apellido2,
                            Correo = u.Correo,
                            Roles_id = u.Roles_id,
                            Id_Empleado = u.Id_Empleado,
                            activo = u.activo
                    // Contrasena se omite intencionalmente por seguridad
                })
                        .ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = usuarios;
                }
            }
            catch (Exception ex)
            {
                System.Data.Entity.Validation.DbEntityValidationException ex2 = new System.Data.Entity.Validation.DbEntityValidationException();
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message + " " + errorDB;
            }
            return oR;
        }


        [HttpGet]
        [Authorize]
        [Route("api/v1/users/{id}")]
        public Reply GetUserById(int id)
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
                    Models.UsuariosViewModel usuario = ctx.Usuarios
                        .Where(u => u.Usuario_id == id)
                        .Select(u => new Models.UsuariosViewModel
                        {
                            Usuario_id = u.Usuario_id,
                            Nombre = u.Nombre,
                            Apellido1 = u.Apellido1,
                            Apellido2 = u.Apellido2,
                            Correo = u.Correo,
                            Roles_id = u.Roles_id,
                            Id_Empleado = u.Id_Empleado,
                            activo = u.activo,
                            Fec_Actualizacion = (DateTime)u.Fec_Actualizacion,
                            Fec_Login = (DateTime)u.Fec_Login
                    // Contrasena se omite intencionalmente por seguridad
                })
                        .FirstOrDefault();

                    if (usuario == null)
                    {
                        throw new Exception("user_not_found");
                    }

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = usuario;
                }
            }
            catch (Exception ex)
            {
                System.Data.Entity.Validation.DbEntityValidationException ex2 = new System.Data.Entity.Validation.DbEntityValidationException();
                String errorDB = "";
                foreach (var eve in ex2.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage;
                    }
                }
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message + " " + errorDB;
            }
            return oR;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("api/v1/login")]
        public Reply Login ([FromBody] Models.Usuarios model)
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;
            General tool = new General();
          
            try
            {
                // Validar body null
                if (model == null)
                {
                    throw new Exception("invalid_model");
                }
             
                // Validar campos vacíos
                if (string.IsNullOrWhiteSpace(model.Correo) ||
                    string.IsNullOrWhiteSpace(model.Contrasena))
                {
                    throw new Exception("invalid_value_form_correo_or_contrasena");   
                }


                if (!tool.ValidaCorreo(model.Correo))
                {
                    throw new Exception("invalid_value_form_Correo");
                }

                String password = tool.Encriptar(model.Contrasena);

                using (var ctx = new Models.EntitiesModel())
                {
                    var usuario = ctx.Usuarios
                        .Where(u =>
                            u.Correo == model.Correo &&
                            u.Contrasena == password &&
                            u.activo == 1)
                        .Select(u => new Models.UsuariosViewModel
                        {
                            Usuario_id = u.Usuario_id,
                            Nombre = u.Nombre,
                            Apellido1 = u.Apellido1,
                            Apellido2 = u.Apellido2,
                            Correo = u.Correo,
                            Roles_id = u.Roles_id,
                            Id_Empleado = u.Id_Empleado,
                            activo = u.activo
                        }).FirstOrDefault();

                    // Usuario no encontrado
                    if (usuario == null)
                    {
                        throw new Exception("invalid_username_or_password");                      
                    }
                    Models.Usuarios usuarioActualiza = ctx.Usuarios.FirstOrDefault(u => u.Usuario_id == usuario.Usuario_id);

                    //actualizamos la ultima fecha de login                   
                    usuarioActualiza.Fec_Login = DateTime.Now;
                    usuario.Fec_Login = DateTime.Now;
                    ctx.SaveChanges();

                    // Login exitoso
                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Message = tool.GenerateTokenJWT(model.Correo);
                    oR.Data = usuario;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string errorDB = "";
                foreach (var eve in ex.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errorDB += ve.ErrorMessage + "";
                    }
                }

                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = errorDB;
            }
            catch (Exception ex)
            {
              

                if (ex is System.Data.Entity.Validation.DbEntityValidationException ex2)
                {
                
                }

                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
            }

            return oR;
        }


    }
}