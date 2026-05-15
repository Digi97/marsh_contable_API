using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using marsh_contable.Models;
using System.Web.Http.Cors;


namespace marsh_contable.Controllers
{
    public class BaseController : ApiController
    {
    
            [EnableCors(origins: "*", headers:"*", methods:"*")]
            public bool Verify(string token)
        {
            //using (Usuarios db = new Usuarios())
            //{
            //    if(db.Usuario_id.where)
            //    return true;
            //}
            return false;
        }
    }
}