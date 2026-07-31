using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace marsh_contable.Modulos
{
    public enum PdfAlineacion
    {
        Izquierda,
        Centro,
        Derecha
    }

    /// <summary>
    /// Generador de PDF mínimo y autocontenido (sin dependencias de NuGet de terceros como
    /// iTextSharp/PdfSharp), pensado para producir documentos simples —por ejemplo, el resumen
    /// de una factura electrónica— que se puedan adjuntar a un correo.
    /// Usa fuentes estándar Helvetica / Helvetica-Bold con codificación WinAnsi (soporta
    /// tildes/ñ en español).
    /// Si en el futuro se requiere un PDF con más formato (logos, imágenes, tablas con
    /// bordes complejos, etc.) se recomienda migrar a una librería dedicada.
    /// </summary>
    public static class PdfSimpleWriter
    {
        private const float MarginLeft = 50;
        private const float MarginRight = 50;
        private const float MarginTop = 792 - 50;   // 742
        private const float MarginBottom = 50;
        private const float PageWidth = 612;
        private const float PageHeight = 792;

        /// <summary>
        /// Compatibilidad con el uso anterior: recibe una lista de líneas de texto plano
        /// y las escribe una debajo de otra con tamaño 11 sin negrita.
        /// </summary>
        public static void Generar(List<string> lineas, string rutaSalida)
        {
            var doc = new PdfDocumento();
            foreach (var linea in lineas)
            {
                if (string.IsNullOrEmpty(linea) || linea.Trim().Length == 0)
                {
                    doc.Espacio(14);
                }
                else
                {
                    doc.Texto(linea, 11, false);
                }
            }
            doc.Generar(rutaSalida);
        }

        // ═══════════════════════════════════════════════════════════
        // API con estilo: construir un documento por elementos
        // ═══════════════════════════════════════════════════════════

        public static Celda CeldaAux(string texto, float x, float ancho, bool negrita = false,
            PdfAlineacion alineacion = PdfAlineacion.Izquierda, int tamano = 11)
        {
            return new Celda
            {
                Texto = texto ?? "",
                X = x,
                Ancho = ancho,
                Negrita = negrita,
                Alineacion = alineacion,
                Tamano = tamano
            };
        }

        public class Celda
        {
            public string Texto;
            public float X;
            public float Ancho;
            public bool Negrita;
            public PdfAlineacion Alineacion = PdfAlineacion.Izquierda;
            public int Tamano = 11;
        }

        private interface IPdfElemento { }

        private class ElTexto : IPdfElemento
        {
            public string Texto;
            public int Tamano = 11;
            public bool Negrita;
            public PdfAlineacion Alineacion = PdfAlineacion.Izquierda;
        }

        private class ElEspacio : IPdfElemento
        {
            public float Alto;
        }

        private class ElLinea : IPdfElemento
        {
            public float Grosor = 0.6f;
        }

        private class ElFila : IPdfElemento
        {
            public List<Celda> Celdas;
        }

        public class PdfDocumento
        {
            private readonly List<IPdfElemento> _elementos = new List<IPdfElemento>();

            public void Titulo(string texto, int tamano = 16)
            {
                _elementos.Add(new ElTexto { Texto = texto, Tamano = tamano, Negrita = true });
            }

            public void Texto(string texto, int tamano = 11, bool negrita = false,
                PdfAlineacion alineacion = PdfAlineacion.Izquierda)
            {
                _elementos.Add(new ElTexto { Texto = texto, Tamano = tamano, Negrita = negrita, Alineacion = alineacion });
            }

            public void Espacio(float alto = 8)
            {
                _elementos.Add(new ElEspacio { Alto = alto });
            }

            public void Linea(float grosor = 0.6f)
            {
                _elementos.Add(new ElLinea { Grosor = grosor });
            }

            public void FilaTabla(params Celda[] celdas)
            {
                _elementos.Add(new ElFila { Celdas = celdas.ToList() });
            }

            /// <summary>Fila rápida de "Etiqueta: valor" (etiqueta en negrita).</summary>
            public void FilaEtiquetaValor(string etiqueta, string valor, float xEtiqueta = MarginLeft,
                float anchoEtiqueta = 110, float xValor = MarginLeft + 115)
            {
                FilaTabla(
                    PdfSimpleWriter.CeldaAux(etiqueta, xEtiqueta, anchoEtiqueta, negrita: true),
                    PdfSimpleWriter.CeldaAux(valor, xValor, PageWidth - MarginRight - xValor)
                );
            }

            public void Generar(string rutaSalida)
            {
                string directorio = Path.GetDirectoryName(rutaSalida);
                if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                Encoding enc = Encoding.GetEncoding("ISO-8859-1");

                var paginas = new List<string>();
                var sb = new StringBuilder();
                sb.Append("BT ");
                float y = MarginTop;
                string fuenteActual = "";
                int tamanoActual = 0;

                void EscribirFuente(string fuente, int tamano)
                {
                    if (fuente != fuenteActual || tamano != tamanoActual)
                    {
                        sb.Append("/" + fuente + " " + tamano.ToString(CultureInfo.InvariantCulture) + " Tf ");
                        fuenteActual = fuente;
                        tamanoActual = tamano;
                    }
                }

                void EscribirTexto(string texto, float x, float yPos, int tamano, bool negrita)
                {
                    EscribirFuente(negrita ? "F2" : "F1", tamano);
                    sb.Append("1 0 0 1 " + F(x) + " " + F(yPos) + " Tm (" + EscapePdfText(texto) + ") Tj ");
                }

                void NuevaPagina()
                {
                    sb.Append("ET");
                    paginas.Add(sb.ToString());
                    sb = new StringBuilder();
                    sb.Append("BT ");
                    y = MarginTop;
                    fuenteActual = "";
                    tamanoActual = 0;
                }

                void AsegurarEspacio(float alto)
                {
                    if (y - alto < MarginBottom)
                    {
                        NuevaPagina();
                    }
                }

                foreach (var el in _elementos)
                {
                    switch (el)
                    {
                        case ElTexto t:
                            {
                                float altoLinea = t.Tamano + 4;
                                AsegurarEspacio(altoLinea);
                                float x = MarginLeft;
                                float anchoDisponible = PageWidth - MarginLeft - MarginRight;
                                if (t.Alineacion == PdfAlineacion.Derecha)
                                {
                                    x = MarginLeft + anchoDisponible - AnchoTexto(t.Texto, t.Tamano, t.Negrita);
                                }
                                else if (t.Alineacion == PdfAlineacion.Centro)
                                {
                                    x = MarginLeft + (anchoDisponible - AnchoTexto(t.Texto, t.Tamano, t.Negrita)) / 2f;
                                }
                                EscribirTexto(t.Texto, x, y, t.Tamano, t.Negrita);
                                y -= altoLinea;
                                break;
                            }
                        case ElEspacio e:
                            AsegurarEspacio(e.Alto);
                            y -= e.Alto;
                            break;
                        case ElLinea l:
                            {
                                AsegurarEspacio(10);
                                sb.Append("ET ");
                                float yLinea = y - 3;
                                sb.Append(F(l.Grosor) + " w " + F(MarginLeft) + " " + F(yLinea) + " m " +
                                          F(PageWidth - MarginRight) + " " + F(yLinea) + " l S ");
                                sb.Append("BT ");
                                fuenteActual = "";
                                tamanoActual = 0;
                                y -= 10;
                                break;
                            }
                        case ElFila fila:
                            {
                                int tamanoFila = fila.Celdas.Count > 0 ? fila.Celdas.Max(c => c.Tamano) : 11;
                                float altoFila = tamanoFila + 4;
                                AsegurarEspacio(altoFila);
                                foreach (var celda in fila.Celdas)
                                {
                                    float anchoTexto = AnchoTexto(celda.Texto, celda.Tamano, celda.Negrita);
                                    float xTexto;
                                    if (celda.Alineacion == PdfAlineacion.Derecha)
                                    {
                                        xTexto = celda.X + celda.Ancho - anchoTexto;
                                    }
                                    else if (celda.Alineacion == PdfAlineacion.Centro)
                                    {
                                        xTexto = celda.X + (celda.Ancho - anchoTexto) / 2f;
                                    }
                                    else
                                    {
                                        xTexto = celda.X;
                                    }
                                    EscribirTexto(celda.Texto, xTexto, y, celda.Tamano, celda.Negrita);
                                }
                                y -= altoFila;
                                break;
                            }
                    }
                }
                sb.Append("ET");
                paginas.Add(sb.ToString());

                EscribirPdf(paginas, enc, rutaSalida);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Bajo nivel: construcción del archivo PDF a partir de los
        // content streams (uno por página) ya armados.
        // ═══════════════════════════════════════════════════════════

        private static void EscribirPdf(List<string> contenidoPorPagina, Encoding enc, string rutaSalida)
        {
            if (contenidoPorPagina.Count == 0)
            {
                contenidoPorPagina = new List<string> { "BT ET" };
            }

            const int fontRegularObjNum = 3;
            const int fontBoldObjNum = 4;

            var idsPaginas = new List<int>();
            var idsContenido = new List<int>();
            int nextObjNum = 5;
            foreach (var _ in contenidoPorPagina)
            {
                idsPaginas.Add(nextObjNum++);
                idsContenido.Add(nextObjNum++);
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

                byte[] header = enc.GetBytes("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");
                ms.Write(header, 0, header.Length);

                EscribirObjeto(fontRegularObjNum,
                    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
                EscribirObjeto(fontBoldObjNum,
                    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

                for (int i = 0; i < contenidoPorPagina.Count; i++)
                {
                    int pageObjNum = idsPaginas[i];
                    int contentObjNum = idsContenido[i];
                    string contenido = contenidoPorPagina[i];
                    byte[] contenidoBytes = enc.GetBytes(contenido);

                    EscribirObjeto(pageObjNum,
                        "<< /Type /Page /Parent 2 0 R /Resources << /Font << /F1 " + fontRegularObjNum +
                        " 0 R /F2 " + fontBoldObjNum + " 0 R >> >> /MediaBox [0 0 " + (int)PageWidth + " " +
                        (int)PageHeight + "] /Contents " + contentObjNum + " 0 R >>");

                    EscribirObjeto(contentObjNum,
                        "<< /Length " + contenidoBytes.Length + " >>\nstream\n" + contenido + "\nendstream");
                }

                string kids = string.Join(" ", idsPaginas.Select(n => n + " 0 R"));
                EscribirObjeto(2, "<< /Type /Pages /Kids [" + kids + "] /Count " + idsPaginas.Count + " >>");
                EscribirObjeto(1, "<< /Type /Catalog /Pages 2 0 R >>");

                var todosLosNumeros = new List<int> { 1, 2, fontRegularObjNum, fontBoldObjNum };
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
            return (texto ?? "").Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        private static string F(float valor)
        {
            return valor.ToString("0.##", CultureInfo.InvariantCulture);
        }

        // ═══════════════════════════════════════════════════════════
        // Estimación de ancho de texto (métricas estándar Helvetica /
        // Helvetica-Bold, en unidades de 1/1000 em) para poder alinear
        // a la derecha o centrar texto sin depender de una fuente
        // monoespaciada.
        // ═══════════════════════════════════════════════════════════

        private static float AnchoTexto(string texto, int tamanoFuente, bool negrita)
        {
            if (string.IsNullOrEmpty(texto)) return 0;
            var tabla = negrita ? AnchoHelveticaBold : AnchoHelvetica;
            int total = 0;
            foreach (char c in texto)
            {
                total += tabla.TryGetValue(c, out int w) ? w : 556;
            }
            return total * tamanoFuente / 1000f;
        }

        private static readonly Dictionary<char, int> AnchoHelvetica = new Dictionary<char, int>
        {
            [' '] = 278,
            ['!'] = 278,
            ['"'] = 355,
            ['#'] = 556,
            ['$'] = 556,
            ['%'] = 889,
            ['&'] = 667,
            ['\''] = 191,
            ['('] = 333,
            [')'] = 333,
            ['*'] = 389,
            ['+'] = 584,
            [','] = 278,
            ['-'] = 333,
            ['.'] = 278,
            ['/'] = 278,
            ['0'] = 556,
            ['1'] = 556,
            ['2'] = 556,
            ['3'] = 556,
            ['4'] = 556,
            ['5'] = 556,
            ['6'] = 556,
            ['7'] = 556,
            ['8'] = 556,
            ['9'] = 556,
            [':'] = 278,
            [';'] = 278,
            ['<'] = 584,
            ['='] = 584,
            ['>'] = 584,
            ['?'] = 556,
            ['@'] = 1015,
            ['A'] = 667,
            ['B'] = 667,
            ['C'] = 722,
            ['D'] = 722,
            ['E'] = 667,
            ['F'] = 611,
            ['G'] = 778,
            ['H'] = 722,
            ['I'] = 278,
            ['J'] = 500,
            ['K'] = 667,
            ['L'] = 556,
            ['M'] = 833,
            ['N'] = 722,
            ['O'] = 778,
            ['P'] = 667,
            ['Q'] = 778,
            ['R'] = 722,
            ['S'] = 667,
            ['T'] = 611,
            ['U'] = 722,
            ['V'] = 667,
            ['W'] = 944,
            ['X'] = 667,
            ['Y'] = 667,
            ['Z'] = 611,
            ['['] = 278,
            ['\\'] = 278,
            [']'] = 278,
            ['^'] = 469,
            ['_'] = 556,
            ['`'] = 333,
            ['a'] = 556,
            ['b'] = 556,
            ['c'] = 500,
            ['d'] = 556,
            ['e'] = 556,
            ['f'] = 278,
            ['g'] = 556,
            ['h'] = 556,
            ['i'] = 222,
            ['j'] = 222,
            ['k'] = 500,
            ['l'] = 222,
            ['m'] = 833,
            ['n'] = 556,
            ['o'] = 556,
            ['p'] = 556,
            ['q'] = 556,
            ['r'] = 333,
            ['s'] = 500,
            ['t'] = 278,
            ['u'] = 556,
            ['v'] = 500,
            ['w'] = 722,
            ['x'] = 500,
            ['y'] = 500,
            ['z'] = 500,
            ['{'] = 334,
            ['|'] = 260,
            ['}'] = 334,
            ['~'] = 584,
            // Latin-1 / español (WinAnsi) — se aproxima al ancho de la letra base
            ['á'] = 556,
            ['é'] = 556,
            ['í'] = 222,
            ['ó'] = 556,
            ['ú'] = 556,
            ['ñ'] = 556,
            ['ü'] = 556,
            ['Á'] = 667,
            ['É'] = 667,
            ['Í'] = 278,
            ['Ó'] = 778,
            ['Ú'] = 722,
            ['Ñ'] = 722,
            ['Ü'] = 722,
            ['¿'] = 556,
            ['¡'] = 333,
            ['°'] = 400
        };

        // Helvetica-Bold: métricas ligeramente más anchas que la regular.
        private static readonly Dictionary<char, int> AnchoHelveticaBold = new Dictionary<char, int>
        {
            [' '] = 278,
            ['!'] = 333,
            ['"'] = 474,
            ['#'] = 556,
            ['$'] = 556,
            ['%'] = 889,
            ['&'] = 722,
            ['\''] = 238,
            ['('] = 333,
            [')'] = 333,
            ['*'] = 389,
            ['+'] = 584,
            [','] = 278,
            ['-'] = 333,
            ['.'] = 278,
            ['/'] = 278,
            ['0'] = 556,
            ['1'] = 556,
            ['2'] = 556,
            ['3'] = 556,
            ['4'] = 556,
            ['5'] = 556,
            ['6'] = 556,
            ['7'] = 556,
            ['8'] = 556,
            ['9'] = 556,
            [':'] = 333,
            [';'] = 333,
            ['<'] = 584,
            ['='] = 584,
            ['>'] = 584,
            ['?'] = 611,
            ['@'] = 975,
            ['A'] = 722,
            ['B'] = 722,
            ['C'] = 722,
            ['D'] = 722,
            ['E'] = 667,
            ['F'] = 611,
            ['G'] = 778,
            ['H'] = 722,
            ['I'] = 278,
            ['J'] = 556,
            ['K'] = 722,
            ['L'] = 611,
            ['M'] = 833,
            ['N'] = 722,
            ['O'] = 778,
            ['P'] = 667,
            ['Q'] = 778,
            ['R'] = 722,
            ['S'] = 667,
            ['T'] = 611,
            ['U'] = 722,
            ['V'] = 667,
            ['W'] = 944,
            ['X'] = 667,
            ['Y'] = 667,
            ['Z'] = 611,
            ['['] = 333,
            ['\\'] = 278,
            [']'] = 333,
            ['^'] = 584,
            ['_'] = 556,
            ['`'] = 333,
            ['a'] = 556,
            ['b'] = 611,
            ['c'] = 556,
            ['d'] = 611,
            ['e'] = 556,
            ['f'] = 333,
            ['g'] = 611,
            ['h'] = 611,
            ['i'] = 278,
            ['j'] = 278,
            ['k'] = 556,
            ['l'] = 278,
            ['m'] = 889,
            ['n'] = 611,
            ['o'] = 611,
            ['p'] = 611,
            ['q'] = 611,
            ['r'] = 389,
            ['s'] = 556,
            ['t'] = 333,
            ['u'] = 611,
            ['v'] = 556,
            ['w'] = 778,
            ['x'] = 556,
            ['y'] = 556,
            ['z'] = 500,
            ['{'] = 389,
            ['|'] = 280,
            ['}'] = 389,
            ['~'] = 584,
            ['á'] = 556,
            ['é'] = 556,
            ['í'] = 278,
            ['ó'] = 611,
            ['ú'] = 611,
            ['ñ'] = 611,
            ['ü'] = 611,
            ['Á'] = 722,
            ['É'] = 667,
            ['Í'] = 278,
            ['Ó'] = 778,
            ['Ú'] = 722,
            ['Ñ'] = 722,
            ['Ü'] = 722,
            ['¿'] = 611,
            ['¡'] = 333,
            ['°'] = 400
        };
    }
}