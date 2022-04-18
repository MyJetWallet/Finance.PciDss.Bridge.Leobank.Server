using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Finance.PciDss.Abstractions;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Requests;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses;
using Finance.PciDss.PciDssBridgeGrpc;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Flurl;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Extensions
{
    public static class MapperExtensions
    {
        public static LeobankInvoiceRequest ToCreatePaymentInvoiceRequest(this IPciDssInvoiceModel model, SettingsModel settingsModel)
        {
            return new LeobankInvoiceRequest
            {
                InfoStruct = new LeobankInvoiceRequestInfo
                {
                    Amount = System.Convert.ToDecimal(model.PsAmount),
                    CallbackUrl = settingsModel.CallbackUrl.CombineUrl(model.OrderId.Trim()),
                    Card = new LeobankInvoiceRequestCard
                    { 
                        Cvv = model.Cvv.Trim(),
                        ExpireMonth = model.ExpirationDate.ToString("MM").Trim(),
                        ExpireYear = model.ExpirationDate.ToString("yy").Trim(),
                        Number = model.CardNumber.Trim()
                    },
                    CardHolder = new LeobankInvoiceRequestCardHolder
                    {
                        FirstName = model.GetName().Trim(),
                        LastName = model.GetLastName().Trim(),
                        City = model.City.Trim(),
                        PostCode = model.Zip.Trim(),
                        Address1 = model.Address.Trim(),
                        //Address2 = "",
                        //Address3 = "",
                        Country = model.Country.Trim(),
                        Email = model.Email.Trim(),
                        Phone = model.PhoneNumber.Trim(),
                        //State = "RO"
                    },
                    //CardToken = "",
                    Comment = "Platform deposit",
                    Currency = model.PsCurrency.Trim(),
                    OrderIid = model.OrderId.Trim(),
                    RedirectUrl = model.GetRedirectUrlForInvoice(settingsModel.RedirectUrlMapping.Trim(), settingsModel.DefaultRedirectUrl.Trim())
                        .CombineUrl(model.OrderId.Trim())
                },
                KeyStruct = new Integrations.Contracts.LeobankInvoiceKey
                {
                    Key = "",
                    Iv = ""
                },
                Method = "charge",
                MerchantPointId = settingsModel.MerchantPointId,
            };
        }

        public static string CombineSuccessUrl(this string baseUrl, string orderId)
        {
            var activityId = Activity.Current?.Id;
            var url = baseUrl.SetQueryParam("status", "success")
                .SetQueryParam("orderId", orderId)
                .SetQueryParam("activityId", activityId);
            return url.ToString();
        }

        public static string CombineFailUrl(this string baseUrl, string orderId)
        {
            var activityId = Activity.Current?.Id;
            var url = baseUrl.SetQueryParam("status", "fail")
                .SetQueryParam("orderId", orderId)
                .SetQueryParam("activityId", activityId);
            return url.ToString();
        }

        public static string CombineUrl(this string baseUrl, string orderId)
        {
            var activityId = Activity.Current?.Id;
            var url = baseUrl
                .SetQueryParam("orderId", orderId)
                .SetQueryParam("activityId", activityId);
            return url.ToString();
        }


        public static LeobankStatusRequest ToStatusRequest(this LeobankInvoiceResponse src, SettingsModel settingsModel)
        {
            return new LeobankStatusRequest
            {
                InfoStruct = new LeobankStatusRequestInfo
                {
                 OrderIid = src.InfoStruct.OrderIid,
                 PaymentId = src.InfoStruct.PaymentId
                },
                KeyStruct = new Integrations.Contracts.LeobankInvoiceKey
                {
                    Key = "",
                    Iv = ""
                },
                Method = "status",
                MerchantPointId = settingsModel.MerchantPointId,
            };
        }

        public static LeobankConfirmRequest ToConfirmRequest(this LeobankInvoiceResponse src, SettingsModel settingsModel)
        {
            return new LeobankConfirmRequest
            {
                InfoStruct = new LeobankConfirmRequestInfo
                {
                    OrderIid = src.InfoStruct.OrderIid,
                    PaymentId = src.InfoStruct.PaymentId
                },
                KeyStruct = new Integrations.Contracts.LeobankInvoiceKey
                {
                    Key = "",
                    Iv = ""
                },
                Method = "confirm",
                MerchantPointId = settingsModel.MerchantPointId,
            };
        }

        public static LeobankConfirmRequest ToConfirmRequest(this MakeConfirmGrpcRequest src, SettingsModel settingsModel)
        {
            return new LeobankConfirmRequest
            {
                InfoStruct = new LeobankConfirmRequestInfo
                {
                    OrderIid = src.PciDssOrderGrpcModel.OrderId,
                    PaymentId = Convert.ToInt32(src.PciDssOrderGrpcModel.PsTransactionId)
                },
                KeyStruct = new Integrations.Contracts.LeobankInvoiceKey
                {
                    Key = "",
                    Iv = ""
                },
                Method = "confirm",
                MerchantPointId = settingsModel.MerchantPointId,
            };
        }

        public static LeobankStatusRequest ToStatusRequest(this GetDepositStatusGrpcRequest src, SettingsModel settingsModel)
        {
            return new LeobankStatusRequest
            {
                InfoStruct = new LeobankStatusRequestInfo
                {
                    OrderIid = src.PciDssStatausGrpcModel.OrderId,
                    PaymentId = Convert.ToInt32(src.PciDssStatausGrpcModel.PsTransactionId)
                },
                KeyStruct = new Integrations.Contracts.LeobankInvoiceKey
                {
                    Key = "",
                    Iv = ""
                },
                Method = "status",
                MerchantPointId = settingsModel.MerchantPointId,
            };
        }
    }
}
