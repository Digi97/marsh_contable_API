using System;

namespace marsh_contable.Models
{
    public class AdjuntosViewModel
    {
        public int id { get; set; }
        public string Nombre_Archivo { get; set; }
        public string Ruta_Archivo { get; set; }
        public short estado { get; set; }
        public int Tipo_archivo_id { get; set; }
        public double Tamano { get; set; }
        public string Descripcion { get; set; }
        public int Usuarios_Usuario_id { get; set; }
        public string extension { get; set; }
        public int referencia { get; set; }
        public int Tablas_referencia_id { get; set; }
        public DateTime fecha_ingreso { get; set; }
        public DateTime fecha_actualizacion { get; set; }

        // Descripciones
        public string Tipo_archivo { get; set; }
        public string Tabla_referencia { get; set; }
        public string Usuario { get; set; }
    }
}
