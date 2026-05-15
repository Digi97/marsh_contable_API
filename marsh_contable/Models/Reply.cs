using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
namespace marsh_contable.Models
{
    public class Reply
       
    {
        public HttpStatusCode CodeStatus {get;set;}
    public object Data { get; set; }
    public string Message { get; set; }
}
}