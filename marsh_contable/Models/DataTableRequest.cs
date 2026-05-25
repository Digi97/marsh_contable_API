using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace marsh_contable.Models
{
    public class DataTableRequest
    {
        // Enviado por DataTables para sincronizar la respuesta
        public int Draw { get; set; }

        // Índice del primer registro (paginación)
        public int Start { get; set; }

        // Cantidad de registros por página
        public int Length { get; set; }

        // Texto de búsqueda global
        public string SearchValue { get; set; }

        // Columna por la que se ordena
        public string SortColumn { get; set; }

        // Dirección del orden: "asc" o "desc"
        public string SortDirection { get; set; }
    }
}