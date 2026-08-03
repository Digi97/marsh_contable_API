using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class AuditaTablaViewModel
    {
        public int id { get; set; }
        public string CamposKey { get; set; }
        public string NombreTabla { get; set; }
        public string CamposValores { get; set; }

        public DateTime Fecha { get; set; }

        public string Accion { get; set; }

        public string NombreColumna { get; set; }

        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public string Usuario_id { get; set; }



    }
}