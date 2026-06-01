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

    public class RolesController : ApiController
    {



        [HttpGet]
        [Authorize]
        [Route("api/v1/roles")]
        public IHttpActionResult GetAllUsers()
        {
            Reply oR = new Reply();
            oR.CodeStatus = 0;

            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    List<Models.RolesViewModel> usuarios = ctx.Roles
                  .Select(u => new Models.RolesViewModel
                  {
                      id = u.id,
                      descripcion = u.Descripcion,
                        })
                  .ToList();

                    oR.CodeStatus = HttpStatusCode.OK;
                    oR.Data = usuarios;
                    return Ok(oR);
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
                return Content(HttpStatusCode.InternalServerError, oR);
            }
            catch (Exception ex)
            {
                
                oR.CodeStatus = HttpStatusCode.InternalServerError;
                oR.Message = ex.Message;
                return Content(HttpStatusCode.InternalServerError, oR);
            }
        }


        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}