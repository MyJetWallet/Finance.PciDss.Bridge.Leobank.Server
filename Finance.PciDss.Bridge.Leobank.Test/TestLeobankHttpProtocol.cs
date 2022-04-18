using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Finance.PciDss.Abstractions;
using Finance.PciDss.Bridge.Leobank.Server;
using Finance.PciDss.Bridge.Leobank.Server.Services;
using Finance.PciDss.Bridge.Leobank.Server.Services.Extensions;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDss.PciDssBridgeGrpc.Contracts.Enums;
using Finance.PciDss.PciDssBridgeGrpc.Models;
using Flurl;
using Newtonsoft.Json;
using NUnit.Framework;
using SimpleTrading.Common.Helpers;

namespace Finance.PciDss.Bridge.Leobank.Server
{
    public class Tests
    {
        private Activity _unitTestActivity;
        private SettingsModel _settingsModel;
        private LeobankHttpClient _leobankHttpClient;
        private MakeBridgeDepositGrpcRequest _request;
        public void Dispose()
        {
            _unitTestActivity.Stop();
        }


        [SetUp]
        public void Setup()
        {
            _leobankHttpClient = new LeobankHttpClient();
            _unitTestActivity = new Activity("UnitTest").Start();
            
            _settingsModel = new SettingsModel()
            {
                SeqServiceUrl = "http://192.168.1.80:5341",
                PciDssBaseUrl = "https://test-leobank.leogaming.net",
                //PciDssBaseUrl = "https://webhook.site/5e18504d-c5a6-4fef-9897-981b5f88c79d/?yuriy=BaseUrl",
                MerchantPointId = 1,
                DefaultRedirectUrl = "https://webhook.site/5e18504d-c5a6-4fef-9897-981b5f88c79d/?yuriy=DefaultRedirect",
                CallbackUrl = "https://webhook.site/5e18504d-c5a6-4fef-9897-981b5f88c79d/?yuriy=CallbackUrl",
                RedirectUrlMapping = "Monfex@st@https://webhook.site/5e18504d-c5a6-4fef-9897-981b5f88c79d/?yuriy=RedirectUrl",
                AuditLogGrpcServiceUrl = @"http://10.240.0.122:80",
                ConvertServiceGrpcUrl = @"http://10.240.1.9:8080",
                PrivateKeyString = "-----BEGIN RSA PRIVATE KEY-----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-----END RSA PRIVATE KEY-----",
                PublicKeyString = "-----BEGIN PUBLIC KEY-----MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAkNxtnQnO0G/AmHfwAO61I/iwm+FXZk6yUtFZjnK9HeH8GELraHhO6plYaJ2NyDe7rYzVWbrBKClmNBYiogJc5kmi+5rnW1xtEMnAaIvI21rbHHsbylOW/pEYt5AhW9DO0Avyt58Lw4YgjE3F0W+Llgf4IL2I7Nfi//RDhEv4qDVJpn5/9rta3zmsI2HgdrLrP3E2Qvnt8B3U8LXSzzrzEFai/LQtaXyuhUstQeGYSZ9iNi26+9d92jkthRzjAdXId+i8ZK9Ptx/mNnvm8zApbrQJ7Sy13YEBWQrnM0DUJwywqdetoNiNL2ygDWi1xch2eJExjSk9uqSfXNjyFOZqfet53co3nCo8NU9ObPMocNNfkrPjxXRBQZgASUwQwuf6K0D5eCbkOSTZJTrd9dy8/Al9+DMUYjwvb3hT2Rx5FNFx8gZhivPhIcVRP/z8n6lIWIAR47kqC5+EUT/X4BCeMHolFpVcEtbelEgOyIW6vDBpEDSeXX0TeQkP4bqC3fg1FGhaRvsufZIwyycRp8rvxMdTwfmhQwIhI80uzOU8awWHZluEkLnCWNRGblFburlTCZKDrbJlacgZYg7GaujVrDjCANYVTE9rIgQ+aSu+TdhsljaVpGdgl28gmFz1TjQ1yVpaSJbc3CfQtb5yxBMlm5pgP/zqr/0AWleKzm9fW88CAwEAAQ==-----END PUBLIC KEY-----",
                TurnOffConvertion = false
            };

            _request = MakeBridgeDepositGrpcRequest.Create(new PciDssInvoiceGrpcModel
            {
                CardNumber = "4390544362820125",
                //CardNumber = "4242424242424242",
                FullName = "TEST TEST",
                Amount = 12,
                Zip = "03201",
                City = "Kiev",
                Country = "UKR",                
                Address = "test",
                OrderId = "QraDMvwquEaTYraMemOQ" + RequestValidator.RandomString(3),
                Email = "leo.bridge@mailinator.com",
                TraderId = "c300b7426e80431aa4300a793f020d19",
                AccountId = "stl00002349usd",
                PaymentProvider = "pciDssLeobankCards",
                Currency = "USD",
                Ip = "213.141.131.96",
                PsAmount = 20.99,
                PsCurrency = "UAH",
                Brand = BrandName.Monfex,
                BrandName = "Monfex",
                PhoneNumber = "+380633985848",
                KycVerification = "Verified",
                Cvv = "123",
                ExpirationDate = DateTime.Parse("2024-12")
            });
        }

        [Test]
        public async Task Send_Leobank_Invoice_Request_And_Check_Status()
        {
            MakeBridgeDepositGrpcResponse returnResult;

            //Modify request data
            _request.PciDssInvoiceGrpcModel.KycVerification = string.IsNullOrEmpty(_request.PciDssInvoiceGrpcModel.KycVerification) ?
                "Empty" : _request.PciDssInvoiceGrpcModel.KycVerification;

            _request.PciDssInvoiceGrpcModel.Country = CountryManager.Iso2ToNumericCode(
                CountryManager.Iso3ToIso2(_request.PciDssInvoiceGrpcModel.Country)).ToString();
            
            var validateResult = _request.Validate();
            if (validateResult.IsFailed)
            {
                returnResult = MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.PaymentDeclined,
                    validateResult.ToString());
                return;
            }

            //Preapare invoice
            var createInvoiceRequest = _request.PciDssInvoiceGrpcModel.ToCreatePaymentInvoiceRequest(_settingsModel);
            createInvoiceRequest.PrepareDataForSending(_settingsModel.PrivateKeyString, _settingsModel.PublicKeyString);
            var invoiceRequest = JsonConvert.SerializeObject(createInvoiceRequest.InfoStruct);

            var createInvoiceResult =
                await _leobankHttpClient.RegisterInvoiceAsync(createInvoiceRequest, _settingsModel.PciDssBaseUrl);

            if (createInvoiceResult.IsFailed)
            {
                createInvoiceResult.FailedResult.Decrypt(_settingsModel.PrivateKeyString);
                returnResult = MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError,
                    createInvoiceResult.FailedResult.InfoStruct.Desc);
                Assert.IsNotNull(returnResult);
                return;
            }

            createInvoiceResult.SuccessResult.Decrypt(_settingsModel.PrivateKeyString);
            
            var invoiceResponse = JsonConvert.SerializeObject(createInvoiceResult.SuccessResult.InfoStruct);

            if (createInvoiceResult.SuccessResult.IsFailed())
            {
                returnResult = MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError,
                    createInvoiceResult.SuccessResult.InfoStruct.Desc);
                Assert.IsNotNull(returnResult);
                return;
            }

            ////Preapare confirm
            //var confirmInvoiceRequest = createInvoiceResult.SuccessResult.ToConfirmRequest(_settingsModel);
            //confirmInvoiceRequest.PrepareDataForSending(_settingsModel.PrivateKeyString, _settingsModel.PublicKeyString);
            //var confirmResponse = await _leobankHttpClient.GetConfirmInvoiceAsync(confirmInvoiceRequest, _settingsModel.PciDssBaseUrl);
            //confirmResponse.SuccessResult.Decrypt(_settingsModel.PrivateKeyString);
            //var confirmResponseMessage = JsonConvert.SerializeObject(confirmResponse.SuccessResult.InfoStruct);

            //Preapare status
            var statusInvoiceRequest = createInvoiceResult.SuccessResult.ToStatusRequest(_settingsModel);
            statusInvoiceRequest.PrepareDataForSending(_settingsModel.PrivateKeyString, _settingsModel.PublicKeyString);
            var statusRequest = JsonConvert.SerializeObject(statusInvoiceRequest.InfoStruct);
            Response<LeobankStatusResponse, LeobankFailResponseDataPayment> statusInvoceResult = null;
            for (int i = 0; i < 10; i++)
            {
                statusInvoceResult =
                    await _leobankHttpClient.GetStatusInvoiceAsync(statusInvoiceRequest, _settingsModel.PciDssBaseUrl);

                if (statusInvoceResult.IsFailed/* || statusInvoceResult.SuccessResult is null || statusInvoceResult.SuccessResult.IsFailed()*/)
                {
                    createInvoiceResult.FailedResult.Decrypt(_settingsModel.PrivateKeyString);
                    returnResult = MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError,
                        statusInvoceResult.FailedResult.InfoStruct.Desc);
                    Assert.IsNotNull(returnResult);
                    return;
                }

                statusInvoceResult.SuccessResult.Decrypt(_settingsModel.PrivateKeyString);
                var statusResponse = JsonConvert.SerializeObject(statusInvoceResult.SuccessResult.InfoStruct);

                if (statusInvoceResult.SuccessResult.InfoStruct.Status == 49)
                {
                    //Preapare confirm
                    var confirmInvoiceRequest = createInvoiceResult.SuccessResult.ToConfirmRequest(_settingsModel);
                    confirmInvoiceRequest.PrepareDataForSending(_settingsModel.PrivateKeyString, _settingsModel.PublicKeyString);
                    var confirmResponse = await _leobankHttpClient.GetConfirmInvoiceAsync(confirmInvoiceRequest, _settingsModel.PciDssBaseUrl);
                    confirmResponse.SuccessResult.Decrypt(_settingsModel.PrivateKeyString);
                    var confirmResponseMessage = JsonConvert.SerializeObject(confirmResponse.SuccessResult.InfoStruct);
                    // break;
                }
                await Task.Delay(100);
            }

            if (createInvoiceResult.SuccessResult.IsSuccessWithoutRedirectTo3ds())
            {
                createInvoiceResult.SuccessResult.InfoStruct.Redirect3DUrl = _settingsModel.DefaultRedirectUrl
                    .SetQueryParam("OrderId", _request.PciDssInvoiceGrpcModel.OrderId);
            }
            else
            {
                if (string.IsNullOrEmpty(createInvoiceResult.SuccessResult.InfoStruct.Redirect3DUrl))
                    createInvoiceResult.SuccessResult.InfoStruct.Redirect3DUrl = _settingsModel.DefaultRedirectUrl;
                else
                    createInvoiceResult.SuccessResult.InfoStruct.Redirect3DUrl =
                        Uri.UnescapeDataString(createInvoiceResult.SuccessResult.InfoStruct.Redirect3DUrl);

            }

            //returnResult = MakeBridgeDepositGrpcResponse.Create(createInvoiceResult.SuccessResult.InfoStruct.Redirect3DUrl,
            //    statusInvoceResult.SuccessResult.TxId, DepositBridgeRequestGrpcStatus.Success);
            //Assert.IsNotNull(returnResult);
            Assert.Pass();
        }
    }
}
