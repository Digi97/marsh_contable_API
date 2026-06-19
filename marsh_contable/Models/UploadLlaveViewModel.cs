using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class UploadLlaveViewModel
    {
        public string file { get; set; } // Base64 del archivo
        public string fileName { get; set; } // Nombre del archivo
    }
}