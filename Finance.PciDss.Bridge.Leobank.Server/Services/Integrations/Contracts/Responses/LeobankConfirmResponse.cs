using Finance.PciDss.Bridge.Leobank.Cripto;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Enums;
using Newtonsoft.Json;
using System;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses
{
    public class LeobankConfirmResponse
    {
        [JsonProperty("merchant_point_id")] public int MerchantPointId { get; set; }
        [JsonProperty("info")] public string Info { get; set; }
        [JsonIgnore] public LeobankConfirmResponseInfo InfoStruct { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonIgnore] public LeobankInvoiceKey KeyStruct { get; set; }
        [JsonProperty("sign")] public string Sign { get; set; }
        public void Decrypt(string privateKey)
        {
            var crypto = new CryptographDecryptor(privateKey);
            KeyStruct = JsonConvert.DeserializeObject<LeobankInvoiceKey>(crypto.RsaDecrypt(Key));
            var key = Convert.FromBase64String(KeyStruct.Key);
            var iv = Convert.FromBase64String(KeyStruct.Iv);
            InfoStruct = JsonConvert.DeserializeObject<LeobankConfirmResponseInfo>(crypto.AesDecrypt(Info, key, iv));
            //Sign = crypto.Sign(JsonConvert.SerializeObject(InfoStruct));
        }


        public bool IsFailed()
        {
            var fullStatus = LeobankTransactionStatusDictionary.Instance.GetDescription(InfoStruct.Status);
            return fullStatus.Status == LeobankTransactionStatus.Declined;
        }

        public bool IsSuccessWithoutRedirectTo3ds()
        {
            var fullStatus = LeobankTransactionStatusDictionary.Instance.GetDescription(InfoStruct.Status);
            return fullStatus.Status == LeobankTransactionStatus.Completed;
        }

        //public bool IsEnrolled3Ds()
        //{
        //    //if (string.IsNullOrEmpty(Result.ResultCode) || string.IsNullOrEmpty(Redirect3DUrl))
        //    //{
        //    //    return false;
        //    //}
        //    //var status = (CertusFinanceTransactionResultCode)Enum.Parse(typeof(CertusFinanceTransactionResultCode), Result.ResultCode);
        //    //return !IsFailed() && 
        //    //       status == CertusFinanceTransactionResultCode.Enrolled3Ds &&
        //    //       !string.IsNullOrEmpty(Redirect3DUrl);
        //    return true;
        //}
    }

    public class LeobankConfirmResponseInfo
    {
        [JsonProperty("bank_point_id")] public int bank_point_id { get; set; }
        [JsonProperty("bank_terminal_name")] public string bank_terminal_name { get; set; }
        [JsonProperty("bank_payment_id")] public string BankPaymentId { get; set; }
        [JsonProperty("method")] public string method { get; set; }
        [JsonProperty("amount")] public double Amount { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; }
        [JsonProperty("comment")] public string Comment { get; set; }
        [JsonProperty("order_id")] public string OrderIid { get; set; }
        [JsonProperty("payment_id")] public int PaymentId { get; set; }
        [JsonProperty("status")] public int Status { get; set; }
        [JsonProperty("final")] public int Final { get; set; }
        [JsonProperty("3ds_url")] public string Redirect3DUrl { get; set; }
        [JsonProperty("secure")] public LeobankInvoiceResponseInfoSecure Secure { get; set; }
        [JsonProperty("card")] public string Card { get; set; }
        [JsonProperty("card_token")] public string CardToken { get; set; }
        [JsonProperty("payment_type")] public int PaymentType { get; set; }
        [JsonProperty("date_create")] public string DateCreate { get; set; }
        [JsonProperty("date_confirm")] public string DateConfirm { get; set; }
        [JsonProperty("date_success")] public string DateSuccess { get; set; }
        [JsonProperty("desc")] public string Desc { get; set; }
    }
}