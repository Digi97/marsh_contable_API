using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace marsh_contable.Modulos
{
    public class General
    {


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


        public string EncriptarArchivo(string pArchivoBase64, int pInd_Activa_Encriptacion, string pEmpresaInput)

        {

            try

            {

                if (pInd_Activa_Encriptacion == 1)

                {

                    byte[] bytesToBeEncrypted = Convert.FromBase64String(pArchivoBase64);

                    byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted, pEmpresaInput);



                    return Convert.ToBase64String(bytesEncrypted);

                }

                else

                {

                    return pArchivoBase64;

                }

            }

            catch (Exception ex)

            {

                return ex.Message;

            }

        }



        public string DesencriptarArchivo(string pArchivoBase64, int pInd_Activa_Encriptacion, string pEmpresaInput)

        {

            try

            {

                if (pInd_Activa_Encriptacion == 1)

                {

                    byte[] bytesToBeDecrypted = Convert.FromBase64String(pArchivoBase64);

                    byte[] bytesDecrypted = Decrypt(bytesToBeDecrypted, pEmpresaInput);



                    return Convert.ToBase64String(bytesDecrypted);

                }

                else

                {

                    return pArchivoBase64;

                }

            }

            catch (Exception ex)

            {

                return ex.Message;

            }

        }



        public string Encriptar(string texto, int pInd_Activa_Encriptacion, string pEmpresaInput)

        {

            try

            {

                if (pInd_Activa_Encriptacion == 1)

                {

                    byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(texto);

                    byte[] bytesEncrypted = Encrypt(bytesToBeEncrypted, pEmpresaInput);



                    return Convert.ToBase64String(bytesEncrypted);

                }

                else

                {

                    return texto;

                }

            }

            catch (Exception ex)

            {

                return ex.Message;

            }

        }



        public string Desencriptar(string texto, int pInd_Activa_Encriptacion, string pEmpresaInput)

        {

            try

            {

                if (pInd_Activa_Encriptacion == 1)

                {

                    byte[] bytesToBeEncrypted = Convert.FromBase64String(texto);

                    byte[] bytesEncrypted = Decrypt(bytesToBeEncrypted, pEmpresaInput);



                    return Encoding.UTF8.GetString(bytesEncrypted);

                }

                else

                {

                    return texto;

                }

            }

            catch (Exception ex)

            {

                return "ERROR: " + ex.Message;

            }

        }



        private byte[] Encrypt(byte[] bytesToBeEncrypted, string pEmpresaInput)

        {

            byte[] encryptedBytes;

            byte[] saltBytes = { 1, 2, 3, 4, 5, 6, 7, 8 };



            using (MemoryStream ms = new MemoryStream())

            {

                using (AesManaged AES = new AesManaged())

                {

                    var pdb = new PasswordDeriveBytes(pEmpresaInput, saltBytes);



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



        private byte[] Decrypt(byte[] bytesToBeEncrypted, string pEmpresaInput)

        {



            try

            {



                byte[] decryptedBytes;

                byte[] saltBytes = { 1, 2, 3, 4, 5, 6, 7, 8 };



                using (MemoryStream ms = new MemoryStream())

                {

                    using (AesManaged AES = new AesManaged())

                    {

                        var pdb = new PasswordDeriveBytes(pEmpresaInput, saltBytes);



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


}
}