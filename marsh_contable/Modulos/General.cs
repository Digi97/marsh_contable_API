using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;


namespace marsh_contable.Modulos
{
    public class General
    {
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
            const String regexfotNames = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s.,;:¡!¿?\-_()%]+$";
            if (!string.IsNullOrWhiteSpace(text) && Regex.IsMatch(text, regexfotNames))
            {
                return true;
            }
            return false;

        }

        public bool ValidaCorreo(string text)
        {
            const String regexfotNames = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
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
        

        public string GenerateTokenJWT(string username)
        {
            var SecretKey = ConfigurationManager.AppSettings["JWT_SECRET_KEY"];
            var audienceToken = ConfigurationManager.AppSettings["JWT_AUDIENCE_TOKEN"];
            var issuerToken = ConfigurationManager.AppSettings["JWT_ISSUER_TOKEN"];
            var expireTime = ConfigurationManager.AppSettings["JWT_EXPIRE_MINUTES"];


            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(SecretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username)});

            // var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var JwtSecurityToken = tokenHandler.CreateJwtSecurityToken(
                audience:audienceToken,
                issuer:issuerToken,
                subject: claimsIdentity,
                notBefore: DateTime.UtcNow,
                expires:DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireTime)),                
                signingCredentials:signingCredentials
                );
            var jwtTokenString = tokenHandler.WriteToken(JwtSecurityToken);
            return jwtTokenString;

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
            // Leer configuración desde Web.config
            string smtpHost = System.Configuration.ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpPort"]);
            bool smtpSsl = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["SmtpSsl"]);
            string smtpUser = System.Configuration.ConfigurationManager.AppSettings["SmtpUser"];
            string smtpPassword = System.Configuration.ConfigurationManager.AppSettings["SmtpPassword"];
            string smtpFrom = System.Configuration.ConfigurationManager.AppSettings["SmtpFrom"];
            string smtpFromName = System.Configuration.ConfigurationManager.AppSettings["SmtpFromName"];

            // Construir el cuerpo HTML del correo
          

            using (var mensaje = new System.Net.Mail.MailMessage())
            {
                mensaje.From = new System.Net.Mail.MailAddress(smtpFrom, smtpFromName);
                mensaje.To.Add(new System.Net.Mail.MailAddress(destinatario));
                mensaje.Subject = Subject;
                mensaje.Body = Html;
                mensaje.IsBodyHtml = true;

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



    }
}