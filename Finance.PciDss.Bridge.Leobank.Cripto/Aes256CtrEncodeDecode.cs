using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Finance.PciDss.Bridge.Leobank.Cripto
{
    public static class Aes256CtrEncodeDecode
    {
        /// <summary>
        /// Encrypt and decrypt data in AES counter mode
        /// </summary>
        /// <param name="forEncrypt">true if need encrypt data</param>
        /// <param name="info">byte array to encryp/decrypt</param>
        /// <param name="key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        public static byte[] EnDecrypt(bool forEncrypt, byte[] info, byte[] key, byte[] IV)
        {
            string algorithm = "AES/CTR/NoPadding";
            IBufferedCipher cipher = CipherUtilities.GetCipher(algorithm);
            cipher.Init(forEncrypt, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), IV));
            int outBytesSize = cipher.GetOutputSize(info.Length);
            byte[] outBytes = new byte[outBytesSize];
            int outLentgh;

            outLentgh = cipher.ProcessBytes(info, 0, info.Length, outBytes, 0);
            outLentgh += cipher.DoFinal(outBytes, outLentgh);

            if (outLentgh != outBytesSize)
            {
                return null;
            }

            return outBytes;
        }

    }
}
