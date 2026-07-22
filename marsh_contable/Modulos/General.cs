using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace marsh_contable.Modulos
{
    public class General
    {

        // El logo de la empresa (Empresa.ruta_logo) se incluye automáticamente como imagen
        // incrustada (inline/cid) en el cuerpo HTML de todos los correos enviados con Send_Mail,
        // ver implementación más abajo.

         String input = ConfigurationManager.AppSettings["Input"];

        public string GenerarClave()
        {
            string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefg0123456789?+*$@-";
            int longitud = 10;
            Random rnd = new Random();
            string cadena = "";
            while (cadena.Length < longitud)
            {
                cadena += caracteres[rnd.Next(0, caracteres.Length)];
            }
            return cadena;
        }


        public string EncriptarArchivo(string pArchivoBase64)

        {

            try

            {
                byte[] bytesToBeEncrypted = Convert.FromBase64String(pArchivoBase64);
                byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted);
                return Convert.ToBase64String(bytesEncrypted);
            }

            catch (Exception ex)

            {

                return ex.Message;

            }

        }



        public string DesencriptarArchivo(string pArchivoBase64)
        {

            try

            {
                byte[] bytesToBeDecrypted = Convert.FromBase64String(pArchivoBase64);

                byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted);



                return Convert.ToBase64String(bytesDecrypted);
            }

            catch (Exception ex)

            {

                return ex.Message;

            }

        }



        public string Encriptar(string texto)
        {
            try

            {
                byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(texto);
                byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted);
                return Convert.ToBase64String(bytesEncrypted);
            }
            catch (Exception ex)

            {
                return ex.Message;
            }
        }



        public string Desencriptar(string texto)

        {

            try

            {
                byte[] bytesToBeEncrypted = Convert.FromBase64String(texto);

                byte[] bytesEncrypted = Decrypt(bytesToBeEncrypted);

                return Encoding.UTF8.GetString(bytesEncrypted);

            }

            catch (Exception ex)

            {

                return "ERROR: " + ex.Message;

            }

        }



        private byte[] Encrypt(byte[] bytesToBeEncrypted)

        {

            byte[] encryptedBytes;

            byte[] saltBytes = { 1, 2, 3, 4, 5, 6, 7, 8 };



            using (MemoryStream ms = new MemoryStream())

            {

                using (AesManaged AES = new AesManaged())

                {

                    var pdb = new PasswordDeriveBytes(input, saltBytes);



                    AES.KeySize = 256;

                    AES.BlockSize = 128;

                    AES.Key = pdb.GetBytes(AES.KeySize / 8);

                    AES.IV = pdb.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;



                    using (CryptoStream cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))

                    {

                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);

                        cs.Close();

                    }

                    encryptedBytes = ms.ToArray();

                }

            }



            return encryptedBytes;

        }



        private byte[] Decrypt(byte[] bytesToBeEncrypted)

        {



            try

            {



                byte[] decryptedBytes;

                byte[] saltBytes = { 1, 2, 3, 4, 5, 6, 7, 8 };



                using (MemoryStream ms = new MemoryStream())

                {

                    using (AesManaged AES = new AesManaged())

                    {

                        var pdb = new PasswordDeriveBytes(input, saltBytes);



                        AES.KeySize = 256;

                        AES.BlockSize = 128;

                        AES.Key = pdb.GetBytes(AES.KeySize / 8);

                        AES.IV = pdb.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;



                        using (CryptoStream cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))

                        {

                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);

                            cs.Close();

                        }

                        decryptedBytes = ms.ToArray();

                    }

                }



                return decryptedBytes;

            }

            catch (Exception ex)

            {

                throw ex;

            }



        }


        public bool ValidaTexto(string text)
        {
            const String regexfotNames = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s.,;:¡!¿?\-_()%#]+$";
            if (!string.IsNullOrWhiteSpace(text) && Regex.IsMatch(text, regexfotNames))
            {
                return true;
            }
            return false;

        }
    //    ^CR[0 - 9]{2}
    //[A-Za-z0-9]{18}$

        public bool ValidaRuta(string text)
        {
            const String regexfotNames = @"^(https?:\/\/|[a-zA-Z]:\\)([\w\-]+\.)*[\w\-]+([\\/][\w\-._~:/?#\[\]@!$&'()*+,;=%\\ ]*)?$";
            if (!string.IsNullOrWhiteSpace(text) && Regex.IsMatch(text, regexfotNames))
            {
                return true;
            }
            return false;

        }

        public bool ValidaIBAN(string text)
        {


            return text.StartsWith("CR") && text.Length == 20;


        }


        public bool ValidaCorreo(string text)
        {
            const String regexfotNames = @"^[#a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            return Regex.IsMatch(text, regexfotNames);
        }

        public bool validaNumeros(string valor, bool esDecimal = false)
        {
     
            if(esDecimal)
            {

                if (decimal.TryParse(valor, out decimal numero))
                {
                    return true;
                }

            }
            else
            {
                if (int.TryParse(valor, out int numero))
                {
                    return true;
                }
            }
            return false;
           
        }

        public bool validaPassword(string text)
        {
            const string pwdFormat = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$";
            return Regex.IsMatch(text, pwdFormat);

        }


        public string GenerateTokenJWT(string username, List<int> permisos = null)
        {
            var SecretKey = ConfigurationManager.AppSettings["JWT_SECRET_KEY"];
            var audienceToken = ConfigurationManager.AppSettings["JWT_AUDIENCE_TOKEN"];
            var issuerToken = ConfigurationManager.AppSettings["JWT_ISSUER_TOKEN"];
            var expireTime = ConfigurationManager.AppSettings["JWT_EXPIRE_MINUTES"];
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(SecretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims base
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, username)
    };

            // Agregar cada permiso como claim individual
            if (permisos != null && permisos.Any())
            {
                foreach (var permiso in permisos)
                {
                    claims.Add(new Claim("permiso", permiso.ToString()));
                }
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims);

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var JwtSecurityToken = tokenHandler.CreateJwtSecurityToken(
                audience: audienceToken,
                issuer: issuerToken,
                subject: claimsIdentity,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireTime)),
                signingCredentials: signingCredentials
            );

            return tokenHandler.WriteToken(JwtSecurityToken);
        }


        public string FormatearCodigoActividad(string codigo)
        {
            string codigoStr = codigo.ToString();

            if (codigoStr.Length > 6)
            {
                throw new Exception("codigo_actividad_longer_than_6_characters");
            }

            return codigoStr.PadLeft(6, '0');
        }


        public void Send_Mail(string destinatario, string Subject, string Html)
        {
            Send_Mail(destinatario, Subject, Html, null);
        }

        /// <summary>
        /// Envía un correo HTML incluyendo, cuando esté disponible, el logo de la empresa
        /// (Empresa.ruta_logo) incrustado en el cuerpo del mensaje (referenciado como cid:logoEmpresa),
        /// y opcionalmente una lista de rutas de archivos a adjuntar (ej. XML de Hacienda, PDF de factura).
        /// </summary>
        public void Send_Mail(string destinatario, string Subject, string Html, List<string> rutasAdjuntos)
        {
            // Leer configuración desde Web.config
            string smtpHost = System.Configuration.ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"]);
            bool smtpSsl = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpSsl"]);
            string smtpUser = System.Configuration.ConfigurationManager.AppSettings["SmtpUser"];
            string smtpPassword = System.Configuration.ConfigurationManager.AppSettings["SmtpPassword"];
            string smtpFrom = System.Configuration.ConfigurationManager.AppSettings["SmtpFrom"];
            string smtpFromName = System.Configuration.ConfigurationManager.AppSettings["SmtpFromName"];

            // Intentar obtener la ruta del logo de la empresa configurada
            string rutaLogo = null;
            try
            {
                using (var ctx = new Models.EntitiesModel())
                {
                    rutaLogo = ctx.Empresa.Where(u => u.Emp_id == 1).Select(u => u.ruta_logo).FirstOrDefault();
                }
            }
            catch
            {
                rutaLogo = null; // Si falla la consulta, se envía el correo sin logo
            }

            bool incluyeLogo = !string.IsNullOrEmpty(rutaLogo) && System.IO.File.Exists(rutaLogo);

            using (var mensaje = new System.Net.Mail.MailMessage())
            {
                mensaje.From = new System.Net.Mail.MailAddress(smtpFrom, smtpFromName);
                mensaje.To.Add(new System.Net.Mail.MailAddress(destinatario));
                mensaje.Subject = Subject;
                mensaje.IsBodyHtml = true;

                if (incluyeLogo)
                {
                    // Encabezado con el logo incrustado (cid) + el cuerpo original del correo
                    string htmlConLogo = $@"<div style='text-align:center; margin-bottom:15px;'>
                        <img src='cid:logoEmpresa' alt='Logo' style='max-height:80px;' />
                    </div>" + Html;

                    var vistaHtml = System.Net.Mail.AlternateView.CreateAlternateViewFromString(htmlConLogo, null, "text/html");
                    var logoResource = new System.Net.Mail.LinkedResource(rutaLogo)
                    {
                        ContentId = "logoEmpresa",
                        TransferEncoding = System.Net.Mime.TransferEncoding.Base64
                    };
                    vistaHtml.LinkedResources.Add(logoResource);
                    mensaje.AlternateViews.Add(vistaHtml);
                }
                else
                {
                    mensaje.Body = Html;
                }

                // Adjuntar archivos (XML de Hacienda, PDF de factura, etc.) si existen en disco
                if (rutasAdjuntos != null)
                {
                    foreach (var ruta in rutasAdjuntos)
                    {
                        if (!string.IsNullOrEmpty(ruta) && System.IO.File.Exists(ruta))
                        {
                            mensaje.Attachments.Add(new System.Net.Mail.Attachment(ruta));
                        }
                    }
                }

                using (var smtp = new System.Net.Mail.SmtpClient(smtpHost, smtpPort))
                {
                    smtp.EnableSsl = smtpSsl;
                    smtp.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
                    smtp.Timeout = 10000; // 10 segundos
                    smtp.Send(mensaje);
                }
            }
        }

        public string HideMail(string correo)
        {
            try
            {
                string[] partes = correo.Split('@');
                string usuario = partes[0];
                string dominio = partes[1];
                int visible = Math.Max(2, usuario.Length / 3);
                string mascara = usuario.Substring(0, visible)
                                   + new string('*', usuario.Length - visible)
                                   + "@" + dominio;
                return mascara;
            }
            catch
            {
                return correo;
            }
        }



        public String NumeroConsecutivo(String sede,  String terminalPuntodeVenta, string tipoDocumento, string numero)
        {
            return sede + terminalPuntodeVenta + tipoDocumento + numero.PadLeft(10, '0');
        }

        public String ClaveNumerica(string pais, string cedEmison, string numeroConsecutivo, string situacionDocumento, string codigoSeguridad)
        {
            var fecha = String.Format("{0:ddMMyy}", DateTime.Now);
            return pais + fecha + cedEmison.PadLeft(12, '0') + numeroConsecutivo + situacionDocumento + codigoSeguridad.PadLeft(8, '0');
        }



        public string FormatearSede(int sede)
        {
            if (sede < 0 || sede > 999)
                throw new Exception("invalid_value_sede_must_be_between_0_and_999");

            return sede.ToString().PadLeft(3, '0');
        }

        public string FormatearTerminal(int terminal)
        {
            if (terminal < 0 || terminal > 99999)
                throw new Exception("invalid_value_terminal_must_be_between_0_and_99999");

            return terminal.ToString().PadLeft(5, '0');
        }
        public string FormatearTipoDocumento(TipoDocumentoId tipoDocumento)
        {
            int valor = (int)tipoDocumento;

            if (!Enum.IsDefined(typeof(TipoDocumentoId), valor))
                throw new Exception("invalid_value_tipo_documento");

            return valor.ToString().PadLeft(2, '0');
        }

        public Models.TipoCambioViewModel ActualizarTipoCambio()
        {
            try
            {
                // ── Fechas del día actual
                string fechaHoy = DateTime.Now.ToString("yyyy/MM/dd");
                string apiUrl = $"https://apim.bccr.fi.cr/SDDE/api/Bccr.Ge.SDDE.Publico.Indicadores.API/cuadro/1/series/?idioma=ES&fechaInicio={fechaHoy}&fechaFin={fechaHoy}";

                // ── Leer Bearer token desde Web.config
                string bccrToken = ConfigurationManager.AppSettings["BCCR_API_TOKEN"];

                if (string.IsNullOrEmpty(bccrToken))
                    throw new Exception("bccr_api_token_not_configured");

                // ── Consultar API del BCCR con Bearer token
                string jsonRespuesta;
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(30);

                    // Agregar el Bearer token en el header
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bccrToken);

                    var respuesta = httpClient.GetAsync(apiUrl).Result;

                    if (!respuesta.IsSuccessStatusCode)
                        throw new Exception($"bccr_api_error_{(int)respuesta.StatusCode}");

                    jsonRespuesta = respuesta.Content.ReadAsStringAsync().Result;
                }

                // ── Deserializar respuesta
                var resultado = Newtonsoft.Json.JsonConvert.DeserializeObject<BccrResponse>(jsonRespuesta);

                if (resultado == null || !resultado.estado)
                    throw new Exception("bccr_api_respuesta_invalida");

                // ── Extraer compra y venta
                double compra = 0;
                double venta = 0;

                var indicadores = resultado.datos?.FirstOrDefault()?.indicadores;

                if (indicadores == null || !indicadores.Any())
                    throw new Exception("bccr_api_sin_indicadores");

                foreach (var indicador in indicadores)
                {
                    double valor = indicador.series?.FirstOrDefault()?.valorDatoPorPeriodo ?? 0;

                    if (indicador.codigoIndicador == "317") compra = valor;
                    if (indicador.codigoIndicador == "318") venta = valor;
                }

                if (compra == 0 || venta == 0)
                    throw new Exception("bccr_api_valores_no_encontrados");

                // ── Guardar en base de datos
                using (var ctx = new Models.EntitiesModel())
                {
                    DateTime hoy = DateTime.Today;
                    DateTime manana = hoy.AddDays(1);

                    bool yaExiste = ctx.Tipo_cambio
                        .Any(t => t.fecha >= hoy &&
                                  t.fecha < manana &&
                                  t.Tipo_moneda_id == 1);

                    if (yaExiste)
                        throw new Exception("tipo_cambio_ya_registrado_hoy");

                    Models.Tipo_cambio tipoCambio = new Models.Tipo_cambio()
                    {
                        fecha = DateTime.Now,
                        compra = compra,
                        venta = venta,
                        Tipo_moneda_id = 1,
                        Usuarios_Usuario_id = 1
                    };

                    ctx.Tipo_cambio.Add(tipoCambio);
                    ctx.SaveChanges();


                    return new Models.TipoCambioViewModel
                    {
                        id = tipoCambio.id,
                        fecha = tipoCambio.fecha,
                        compra = tipoCambio.compra,
                        venta = tipoCambio.venta,
                        Tipo_moneda_id = tipoCambio.Tipo_moneda_id
                    };


                    //oR.CodeStatus = HttpStatusCode.OK;
                    //oR.Data = new
                    //{
                    //    id = tipoCambio.id,
                    //    fecha = tipoCambio.fecha,
                    //    compra = tipoCambio.compra,
                    //    venta = tipoCambio.venta
                    //};
                    //return oR;
                }
            }
            catch (Exception ex)
            {
                throw ex;
    
            }
        }






    }
}