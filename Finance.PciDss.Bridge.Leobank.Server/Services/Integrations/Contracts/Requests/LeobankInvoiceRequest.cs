using System;
using System.Security.Cryptography;
using Destructurama.Attributed;
using Finance.PciDss.Bridge.Leobank.Cripto;
using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Requests
{
    public class LeobankInvoiceRequest
    {
        [JsonProperty("merchant_point_id")] public int MerchantPointId { get; set; }
        [JsonProperty("method")] public string Method { get; set; }
        [JsonProperty("info")] public string Info { get; set; }
        [JsonIgnore] public LeobankInvoiceRequestInfo InfoStruct { get; set; }
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

    public class LeobankInvoiceRequestInfo
    {
        [JsonProperty("card")] public LeobankInvoiceRequestCard Card { get; set; }
        //[JsonProperty("card_token")] public string CardToken { get; set; }
        [JsonIgnore] public decimal Amount { get; set; }
        [JsonProperty("amount")] public decimal RoundedAmount { get { return Math.Round(Amount, 2); } }
        [JsonProperty("currency")] public string Currency { get; set; }
        [JsonProperty("comment")] public string Comment { get; set; }
        [JsonProperty("result_url")] public string CallbackUrl { get; set; }
        [JsonProperty("return_url")] public string RedirectUrl { get; set; }
        [JsonProperty("order_id")] public string OrderIid { get; set; }
        [JsonProperty("card_holder")] public LeobankInvoiceRequestCardHolder CardHolder { get; set; }
    }


    public class LeobankInvoiceRequestCard
    {
        [LogMasked(ShowFirst = 6, ShowLast = 4, PreserveLength = true)]
        [JsonProperty("number")] public string Number { get; set; }
        [LogMasked(PreserveLength = true)]
        [JsonProperty("cvv")] public string Cvv { get; set; }
        [LogMasked(PreserveLength = true)]
        [JsonProperty("expire_month")] public string ExpireMonth { get; set; }
        [LogMasked(PreserveLength = true)]
        [JsonProperty("expire_year")] public string ExpireYear { get; set; }
    }

    public class LeobankInvoiceRequestCardHolder
    {
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("first_name")] public string FirstName { get; set; }
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("last_name")] public string LastName { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("post_code")] public string PostCode { get; set; }
        //[JsonProperty("state")] public string State { get; set; }
        [JsonProperty("city")] public string City { get; set; }
        [JsonProperty("address_line_1")] public string Address1 { get; set; }
        //[JsonProperty("address_line_2")] public string Address2 { get; set; }
        //[JsonProperty("address_line_3")] public string Address3 { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("phone")] public string Phone { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("email")] public string Email { get; set; }
    }
}