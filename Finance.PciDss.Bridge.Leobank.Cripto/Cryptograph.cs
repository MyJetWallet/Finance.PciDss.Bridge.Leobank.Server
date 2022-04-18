using System;
using System.Security.Cryptography;
using System.Text;

namespace Finance.PciDss.Bridge.Leobank.Cripto
{
    public class CryptographEncryptor
    {
        private string pemPublicKey { get; set; }
        private string pemPrivateKey { get; set; }
        private RSACryptoServiceProvider rsaProvider { get; set; }
        private RSACryptoServiceProvider signerProvider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="publicKey">Open public PEM key</param>
        /// <param name="privateKey">Private PEM key</param>
        public CryptographEncryptor(string publicKey, string privateKey)
        {
            if (string.IsNullOrEmpty(publicKey)) throw new Exception("INVALID_PUBLIC_KEY");
            pemPublicKey = publicKey;
            rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportFromPem(pemPublicKey);

            if (string.IsNullOrEmpty(privateKey)) throw new Exception("INVALID_PRIVATE_KEY");
            pemPrivateKey = privateKey;
            signerProvider = new RSACryptoServiceProvider();
            signerProvider.ImportFromPem(pemPrivateKey);
        }

        public string RsaEncrypt(string data)
        {
            byte[] encryptedKeyStruct = rsaProvider.Encrypt(UTF8Encoding.UTF8.GetBytes(data), false);
            return Convert.ToBase64String(encryptedKeyStruct);
        }

        public string AesEncrypt(string data, byte[] key, byte[] iv)
        {
            byte[] encryptedData = Aes256CtrEncodeDecode.EnDecrypt(true, Encoding.UTF8.GetBytes(data), key, iv);
            return Convert.ToBase64String(encryptedData);
        }

        public string Sign(string data)
        {
            byte[] data_b = Encoding.UTF8.GetBytes(data);
            byte[] data_sing_b = signerProvider.SignData(data_b, new SHA256CryptoServiceProvider());
            return Convert.ToBase64String(data_sing_b);
        }
    }

    public class CryptographDecryptor
    {
        private string pemPrivateKey { get; set; }
        private RSACryptoServiceProvider rsaProvider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="publicKey">Open public PEM key</param>
        public CryptographDecryptor(string privateKey)
        {
            if (string.IsNullOrEmpty(privateKey)) throw new Exception("INVALID_PRIVATE_KEY");
            pemPrivateKey = privateKey;
            rsaProvider = new RSACryptoServiceProvider();
            rsaProvider.ImportFromPem(pemPrivateKey);

        }

        public string RsaDecrypt(string data)
        {
            byte[] dencryptedKeyStruct = rsaProvider.Decrypt(Convert.FromBase64String(data), false);
            return Encoding.UTF8.GetString(dencryptedKeyStruct);
        }

        public string AesDecrypt(string data, byte[] key, byte[] iv)
        {
            byte[] dencryptedData = Aes256CtrEncodeDecode.EnDecrypt(false, Convert.FromBase64String(data), key, iv);
            return Encoding.UTF8.GetString(dencryptedData);
        }

        //public string Sign(string data)
        //{
        //    byte[] data_b = Encoding.UTF8.GetBytes(data);
        //    byte[] data_sing_b = signerProvider.SignData(data_b, new SHA256CryptoServiceProvider());
        //    return Convert.ToBase64String(data_sing_b);
        //}
    }
}
