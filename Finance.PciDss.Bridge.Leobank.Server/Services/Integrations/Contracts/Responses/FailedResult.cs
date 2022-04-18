using Finance.PciDss.Bridge.Leobank.Cripto;
using Newtonsoft.Json;
using System;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses
{
    public class FailedResult
    {
        [JsonProperty("merchant_point_id")] public int MerchantPointId { get; set; }
        [JsonProperty("info")] public string Info { get; set; }
        [JsonIgnore] public LeobankInvoiceFailedResponseInfo InfoStruct { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonIgnore] public LeobankInvoiceKey KeyStruct { get; set; }
        [JsonProperty("sign")] public string Sign { get; set; }

        public void Decrypt(string privateKey)
        {
            var crypto = new CryptographDecryptor(privateKey);
            KeyStruct = JsonConvert.DeserializeObject<LeobankInvoiceKey>(crypto.RsaDecrypt(Key));
            var key = Convert.FromBase64String(KeyStruct.Key);
            var iv = Convert.FromBase64String(KeyStruct.Iv);
            InfoStruct = JsonConvert.DeserializeObject<LeobankInvoiceFailedResponseInfo>(crypto.AesDecrypt(Info, key, iv));
            //Sign = crypto.Sign(JsonConvert.SerializeObject(InfoStruct));
        }
    }
        public class LeobankInvoiceFailedResponseInfo
        {
            [JsonProperty("status")] public int Status { get; set; }
            [JsonProperty("final")] public int Final { get; set; }
            [JsonProperty("desc")] public string Desc { get; set; }
        }
}