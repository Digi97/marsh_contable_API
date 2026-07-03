using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace marsh_contable.Modulos
{
    /// <summary>
    /// Generador de PDF mínimo y autocontenido (sin dependencias de NuGet de terceros como
    /// iTextSharp/PdfSharp), pensado para producir un PDF de texto plano simple —por ejemplo,
    /// el resumen de una factura electrónica— que se pueda adjuntar a un correo.
    /// Usa fuente estándar Helvetica con codificación WinAnsi (soporta tildes/ñ en español).
    /// Si en el futuro se requiere un PDF con más formato (logos, tablas complejas, etc.) se
    /// recomienda migrar a una librería dedicada.
    /// </summary>
    public static class PdfSimpleWriter
    {
        public static void Generar(List<string> lineas, string rutaSalida)
        {
            string directorio = Path.GetDirectoryName(rutaSalida);
            if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            // Usamos ISO-8859-1 (Latin-1) para que cada caracter ocupe 1 byte y coincida
            // razonablemente con WinAnsiEncoding para caracteres latinos comunes (á é í ó ú ñ).
            Encoding enc = Encoding.GetEncoding("ISO-8859-1");

            const int lineHeight = 14;
            const int marginTop = 750;
            const int marginLeft = 50;
            const int pageHeight = 792;
            const int linesPerPage = (marginTop - 40) / lineHeight;

            var paginas = new List<List<string>>();
            for (int i = 0; i < lineas.Count; i += linesPerPage)
            {
                paginas.Add(lineas.Skip(i).Take(linesPerPage).ToList());
            }
            if (paginas.Count == 0)
            {
                paginas.Add(new List<string>());
            }

            var objetos = new List<byte[]>();

            // 1: Catalog, 2: Pages — se agregan al final una vez conocido el total de páginas
            objetos.Add(null); // placeholder objeto 1
            objetos.Add(null); // placeholder objeto 2

            var idsPaginas = new List<int>();
            var idsContenido = new List<int>();

            int fontObjNum = 3; // objeto de fuente compartido por todas las páginas

            int nextObjNum = 4;
            foreach (var pagina in paginas)
            {
                int pageObjNum = nextObjNum++;
                int contentObjNum = nextObjNum++;
                idsPaginas.Add(pageObjNum);
                idsContenido.Add(contentObjNum);
            }

            // Construir stream de contenido por página
            var contenidoPorPagina = new List<string>();
            foreach (var pagina in paginas)
            {
                var sb = new StringBuilder();
                sb.Append("BT /F1 11 Tf ");
                int y = marginTop;
                bool primera = true;
                foreach (var lineaRaw in pagina)
                {
                    string linea = EscapePdfText(lineaRaw ?? "");
                    if (primera)
                    {
                        sb.Append(marginLeft + " " + y + " Td (" + linea + ") Tj ");
                        primera = false;
                    }
                    else
                    {
                        sb.Append("0 -" + lineHeight + " Td (" + linea + ") Tj ");
                    }
                }
                sb.Append("ET");
                contenidoPorPagina.Add(sb.ToString());
            }

            using (var ms = new MemoryStream())
            {
                var offsets = new Dictionary<int, long>();

                void EscribirObjeto(int numObj, string contenido)
                {
                    offsets[numObj] = ms.Position;
                    byte[] bytes = enc.GetBytes(numObj + " 0 obj\n" + contenido + "\nendobj\n");
                    ms.Write(bytes, 0, bytes.Length);
                }

                // Header
                byte[] header = enc.GetBytes("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");
                ms.Write(header, 0, header.Length);

                // Objeto 3: Fuente
                EscribirObjeto(fontObjNum,
                    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");

                // Objetos de páginas y su contenido
                for (int i = 0; i < paginas.Count; i++)
                {
                    int pageObjNum = idsPaginas[i];
                    int contentObjNum = idsContenido[i];
                    string contenido = contenidoPorPagina[i];
                    byte[] contenidoBytes = enc.GetBytes(contenido);

                    EscribirObjeto(pageObjNum,
                        "<< /Type /Page /Parent 2 0 R /Resources << /Font << /F1 " + fontObjNum + " 0 R >> >> " +
                        "/MediaBox [0 0 612 " + pageHeight + "] /Contents " + contentObjNum + " 0 R >>");

                    EscribirObjeto(contentObjNum,
                        "<< /Length " + contenidoBytes.Length + " >>\nstream\n" + contenido + "\nendstream");
                }

                // Objeto 2: Pages (kids)
                string kids = string.Join(" ", idsPaginas.Select(n => n + " 0 R"));
                EscribirObjeto(2, "<< /Type /Pages /Kids [" + kids + "] /Count " + idsPaginas.Count + " >>");

                // Objeto 1: Catalog
                EscribirObjeto(1, "<< /Type /Catalog /Pages 2 0 R >>");

                // xref
                int totalObjetos = 1 + idsPaginas.Count * 2 + 2; // fuente + (page+content) * n + pages + catalog... (aprox, se recalcula abajo)
                var todosLosNumeros = new List<int> { 1, 2, fontObjNum };
                todosLosNumeros.AddRange(idsPaginas);
                todosLosNumeros.AddRange(idsContenido);
                int maxObjNum = todosLosNumeros.Max();

                long xrefStart = ms.Position;
                var sbXref = new StringBuilder();
                sbXref.Append("xref\n0 " + (maxObjNum + 1) + "\n");
                sbXref.Append("0000000000 65535 f \n");
                for (int n = 1; n <= maxObjNum; n++)
                {
                    if (offsets.ContainsKey(n))
                    {
                        sbXref.Append(offsets[n].ToString("D10") + " 00000 n \n");
                    }
                    else
                    {
                        sbXref.Append("0000000000 00000 f \n");
                    }
                }
                sbXref.Append("trailer\n<< /Size " + (maxObjNum + 1) + " /Root 1 0 R >>\nstartxref\n" + xrefStart + "\n%%EOF");

                byte[] xrefBytes = enc.GetBytes(sbXref.ToString());
                ms.Write(xrefBytes, 0, xrefBytes.Length);

                File.WriteAllBytes(rutaSalida, ms.ToArray());
            }
        }

        private static string EscapePdfText(string texto)
        {
            return texto.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }
    }
}
