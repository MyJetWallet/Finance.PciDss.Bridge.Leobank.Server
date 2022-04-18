using System;
using System.Security.Cryptography;
using Destructurama.Attributed;
using Finance.PciDss.Bridge.Leobank.Cripto;
using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Requests
{
    public class LeobankConfirmRequest
    {
        [JsonProperty("merchant_point_id")] public int MerchantPointId { get; set; }
        [JsonProperty("method")] public string Method { get; set; }
        [JsonProperty("info")] public string Info { get; set; }
        [JsonIgnore] public LeobankConfirmRequestInfo InfoStruct { get; set; }
        [LogMasked] [JsonProperty("key")] public string Key { get; set; }
        [JsonIgnore] public LeobankInvoiceKey KeyStruct { get; set; }
        [JsonProperty("sign")] public string Sign { get; set; }
        public void PrepareDataForSending(string privateKey, string publicKey)
        {
            var crypto = new CryptographEncryptor(publicKey, privateKey);
            // Generate new keys for AES
            var key = new byte[32];
            RandomNumberGenerator.Create().GetBytes(key);
            var iv = new byte[16];
            RandomNumberGenerator.Create().GetBytes(iv);
            KeyStruct.Key = Convert.ToBase64String(key);
            KeyStruct.Iv = Convert.ToBase64String(iv);
            Key = crypto.RsaEncrypt(JsonConvert.SerializeObject(KeyStruct));
            Info = crypto.AesEncrypt(JsonConvert.SerializeObject(InfoStruct), key, iv);
            Sign = crypto.Sign(JsonConvert.SerializeObject(InfoStruct));
        }
    }

    public class LeobankConfirmRequestInfo
    {
        [JsonProperty("payment_id")] public int PaymentId { get; set; }
        [JsonProperty("order_id")] public string OrderIid { get; set; }
    }
}