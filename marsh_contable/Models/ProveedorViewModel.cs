using System;

namespace marsh_contable.Models
{
    public class ProveedorViewModel
    {
        public int id { get; set; }
        public string identificacion { get; set; }
        public int tipo_identificacion_id { get; set; }
        public string Nombre { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public string correo { get; set; }
        public int Distrito_id { get; set; }
        public int Canton_id { get; set; }
        public int Provincia_id { get; set; }
        public int codigo_actividad_id { get; set; }
        public short estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_actualizacion { get; set; }
        public short exonerado { get; set; }
        public string OtrasSenas { get; set; }

        // Descripciones de catálogos relacionados
        public string Tipo_identificacion { get; set; }
        public string Provincia { get; set; }
        public string Canton { get; set; }
        public string Distrito { get; set; }
        public string Codigo_actividad { get; set; }
        public string Nombre_actividad { get; set; }
    }
}
