using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Finance.PciDss.Abstractions;
using Finance.PciDss.Bridge.Leobank.Server.Services.Extensions;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Enums;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Requests;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses;
using Finance.PciDss.PciDssBridgeGrpc;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDss.PciDssBridgeGrpc.Contracts.Enums;
using Flurl;
using MyCrm.AuditLog.Grpc;
using MyCrm.AuditLog.Grpc.Models;
using Newtonsoft.Json;
using Serilog;
using SimpleTrading.Common.Helpers;
using SimpleTrading.ConvertService.Grpc;
using SimpleTrading.ConvertService.Grpc.Contracts;
using SimpleTrading.GrpcTemplate;

namespace Finance.PciDss.Bridge.Leobank.Server.Services
{
    public class LeobankGrpcService : IFinancePciDssBridgeGrpcService
    {
        public const string PaymentSystemId = "pciDssLeobankBankCards";
        private const string UsdCurrency = "USD";
        private const string EurCurrency = "EUR";
        private readonly ILogger _logger;
        private readonly GrpcServiceClient<IMyCrmAuditLogGrpcService> _myCrmAuditLogGrpcService;
        private readonly ISettingsModelProvider _optionsMonitorSettingsModelProvider;
        private readonly ILeobankHttpClient _leobankHttpClient;
        private readonly GrpcServiceClient<IConvertService> _convertServiceClient;

        public LeobankGrpcService(ILeobankHttpClient leobankHttpClient,
            GrpcServiceClient<IMyCrmAuditLogGrpcService> myCrmAuditLogGrpcService,
            GrpcServiceClient<IConvertService> convertServiceClient,
            ISettingsModelProvider optionsMonitorSettingsModelProvider,
            ILogger logger)
        {
            _leobankHttpClient = leobankHttpClient;
            _myCrmAuditLogGrpcService = myCrmAuditLogGrpcService;
            _convertServiceClient = convertServiceClient;
            _optionsMonitorSettingsModelProvider = optionsMonitorSettingsModelProvider;
            _logger = logger;
        }
        private SettingsModel _settingsModel => _optionsMonitorSettingsModelProvider.Get();

        public async ValueTask<MakeBridgeDepositGrpcResponse> MakeDepositAsync(MakeBridgeDepositGrpcRequest request)
        {
            var bridgeBrand = _settingsModel.RedirectUrlMapping.GetBridgeBrand(); 
            _logger.Information("LeobankGrpcService start process MakeBridgeDepositGrpcRequest {bridgeBrand} {@request}", bridgeBrand, request);
            try
            {
                request.PciDssInvoiceGrpcModel.KycVerification = string.IsNullOrEmpty(request.PciDssInvoiceGrpcModel.KycVerification) ?
                    "Empty" : request.PciDssInvoiceGrpcModel.KycVerification;

                request.PciDssInvoiceGrpcModel.Country = CountryManager.Iso2ToNumericCode(
                    CountryManager.Iso3ToIso2(request.PciDssInvoiceGrpcModel.Country)).ToString();


                var validateResult = request.Validate();
                if (validateResult.IsFailed)
                {
                    _logger.Warning("Leobank request is not valid. Errors {@validateResult}", validateResult);
                    await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                        $"Fail Leobank create invoice. Error {validateResult}");
                    return MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.PaymentDeclined,
                        validateResult.ToString());
                }

                //Preapare invoice
                var createInvoiceRequest = request.PciDssInvoiceGrpcModel.ToCreatePaymentInvoiceRequest(_settingsModel);
                createInvoiceRequest.PrepareDataForSending(_settingsModel.PrivateKeyString, _settingsModel.PublicKeyString);

                _logger.Information("Leobank send request {@Request}", createInvoiceRequest.InfoStruct);
                await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                    @"Leobank send sale request Amount: {createInvoiceRequest.Amount} currency: {createInvoiceRequest.Currency}");

                var createInvoiceResult =
                    await _leobankHttpClient.RegisterInvoiceAsync(createInvoiceRequest, _settingsModel.PciDssBaseUrl);

                if (createInvoiceResult.IsFailed)
                {
                    await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                        $"{PaymentSystemId}. Fail Leobank create invoice with kyc: {request.PciDssInvoiceGrpcModel.KycVerification}. Message: {createInvoiceResult.FailedResult.InfoStruct.Desc}. " +
                        $"Error: {JsonConvert.SerializeObject(createInvoiceResult.FailedResult.InfoStruct.Desc)}");
                    return MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError,
                        createInvoiceResult.FailedResult.InfoStruct.Desc);
                }

                createInvoiceResult.SuccessResult.Decrypt(_settingsModel.PrivateKeyString);

                if (createInvoiceResult.SuccessResult.IsFailed())
                {
                    await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                        $"{PaymentSystemId}. Fail Leobank create invoice with kyc: {request.PciDssInvoiceGrpcModel.KycVerification}. Message: {createInvoiceResult.SuccessResult.InfoStruct.Desc}. " +
                        $"Error: {JsonConvert.SerializeObject(createInvoiceResult.SuccessResult.InfoStruct)}");
                    return MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.PaymentDeclined,
                        createInvoiceResult.SuccessResult.InfoStruct.Desc);
                }

                _logger.Information("Created deposit invoice {@Id} {@Kyc} {@Result}",
                    request.PciDssInvoiceGrpcModel.OrderId, 
                    request.PciDssInvoiceGrpcModel.KycVerification, 
                    createInvoiceResult.SuccessResult.InfoStruct);
                
                await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                    $"Created deposit invoice with id: {request.PciDssInvoiceGrpcModel.OrderId} " +
                    $"kyc: {request.PciDssInvoiceGrpcModel.KycVerification} " +
                    $"redirectUrl: {createInvoiceResult.SuccessResult.InfoStruct.Redirect3DUrl}");

                if (!Startup.ConfirmDepositOperationsQueue.TryAdd(createInvoiceResult.SuccessResult.InfoStruct.PaymentId.ToString(),
                    new LeobankConfirmBackgroundInfo
                    {
                        PaymentUtcTimeStamp = DateTime.UtcNow,
                        OrderIid = createInvoiceResult.SuccessResult.InfoStruct.OrderIid,
                        PaymentId = createInvoiceResult.SuccessResult.InfoStruct.PaymentId,
                        ActivityId = Activity.Current?.Id,
                        TraderId = request.PciDssInvoiceGrpcModel.TraderId
                    }))
                {
                    _logger.Information("Can't add deposit invoice to status checker {@Id} {@PaymentId}",
                        request.PciDssInvoiceGrpcModel.OrderId,
                        createInvoiceResult.SuccessResult.InfoStruct.PaymentId);
                }

                return MakeBridgeDepositGrpcResponse.Create(createInvoiceResult.SuccessResult.InfoStruct.Redirect3DUrl,
                    createInvoiceResult.SuccessResult.InfoStruct.PaymentId.ToString(), DepositBridgeRequestGrpcStatus.Success);
            }
            catch (Exception e)
            {
                _logger.Error(e, "MakeDepositAsync failed for traderId {traderId}",
                    request.PciDssInvoiceGrpcModel.TraderId);
                await SendMessageToAuditLogAsync(request.PciDssInvoiceGrpcModel,
                    $"{PaymentSystemId}. MakeDeposit failed");
                return MakeBridgeDepositGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError, e.Message);
            }
        }

        public ValueTask<GetPaymentSystemGrpcResponse> GetPaymentSystemNameAsync()
        {
            return new ValueTask<GetPaymentSystemGrpcResponse>(GetPaymentSystemGrpcResponse.Create(PaymentSystemId));
        }

        public ValueTask<GetPaymentSystemCurrencyGrpcResponse> GetPsCurrencyAsync()
        {
            return new ValueTask<GetPaymentSystemCurrencyGrpcResponse>(
                GetPaymentSystemCurrencyGrpcResponse.Create(EurCurrency));
        }

        public async ValueTask<GetPaymentSystemAmountGrpcResponse> GetPsAmountAsync(GetPaymentSystemAmountGrpcRequest request)
        {
            if (_settingsModel.TurnOffConvertion)
            {
                return GetPaymentSystemAmountGrpcResponse.Create(request.Amount, request.Currency);
            }

            if (request.Currency.Equals(UsdCurrency, StringComparison.OrdinalIgnoreCase))
            {

                var convertResponse = await _convertServiceClient.Value.Convert(new CovertRequest
                {
                    InstrumentId = EurCurrency + UsdCurrency,
                    ConvertType = ConvertTypes.QuoteToBase,
                    Amount = request.Amount
                });

                return GetPaymentSystemAmountGrpcResponse.Create(convertResponse.ConvertedAmount, EurCurrency);
            }
            return default;
        }


        private ValueTask SendMessageToAuditLogAsync(IPciDssInvoiceModel invoice, string message)
        {
            return _myCrmAuditLogGrpcService.Value.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = invoice.TraderId,
                Action = "deposit",
                ActionId = invoice.OrderId,
                DateTime = DateTime.UtcNow,
                Message = message
            });
        }

        public async ValueTask<GetDepositStatusGrpcResponse> GetDepositStatusAsync(GetDepositStatusGrpcRequest request)
        {
            var bridgeBrand = _settingsModel.RedirectUrlMapping.GetBridgeBrand();
            _logger.Information("LeobankGrpcService start process GetDepositStatusAsync {bridgeBrand} {@request}", bridgeBrand, request);
            try
            {
                var confirmInvoiceRequest = request.ToStatusRequest(_settingsModel);
                confirmInvoiceRequest.PrepareDataForSending(_settingsModel.PrivateKeyString,
                    _settingsModel.PublicKeyString);
                var confirmResponse =
                    await _leobankHttpClient.GetStatusInvoiceAsync(confirmInvoiceRequest,
                        _settingsModel.PciDssBaseUrl);
                confirmResponse.SuccessResult.Decrypt(_settingsModel.PrivateKeyString);

                var confirmResponseMessage = JsonConvert.SerializeObject(confirmResponse.SuccessResult.InfoStruct);

                var messsage = LeobankTransactionStatusDictionary.Instance.GetDescription(
                    confirmResponse.SuccessResult.InfoStruct.Status).Text;

                return GetDepositStatusGrpcResponse.Create(confirmResponse.SuccessResult.InfoStruct.OrderId, confirmResponse.SuccessResult.InfoStruct.PaymentId.ToString(),
                    confirmResponse.SuccessResult.InfoStruct.Redirect3DUrl, confirmResponse.SuccessResult.InfoStruct.Status, confirmResponse.SuccessResult.InfoStruct.Desc,
                    DepositBridgeRequestGrpcStatus.Success);
            }
            catch (Exception e)
            {
                _logger.Error(e, "GetDepositStatusAsync failed for orderId: {orderId} txId: {psTransactionId} : traderId: {traderId}",
                    request.PciDssStatausGrpcModel.OrderId, 
                    request.PciDssStatausGrpcModel.PsTransactionId, 
                    request.PciDssStatausGrpcModel.TraderId);

                return GetDepositStatusGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError, e.Message);
            }
        }

        //public async ValueTask<LeobankConfirmResponseInfo> MakeDepositConfirmAsync(LeobankInvoiceResponse createInvoiceResult)
        //{
        //    var confirmInvoiceRequest = createInvoiceResult.ToConfirmRequest(_settingsModel);
        //    confirmInvoiceRequest.PrepareDataForSending(_settingsModel.PrivateKeyString, _settingsModel.PublicKeyString);
        //    var confirmResponse = await _leobankHttpClient.GetConfirmInvoiceAsync(confirmInvoiceRequest, _settingsModel.PciDssBaseUrl);
        //    confirmResponse.SuccessResult.Decrypt(_settingsModel.PrivateKeyString);
        //    var confirmResponseMessage = JsonConvert.SerializeObject(confirmResponse.SuccessResult.InfoStruct);
        //    return confirmResponse.SuccessResult.InfoStruct;
        //}

        public ValueTask<DecodeBridgeInfoGrpcResponse> DecodeInfoAsync(DecodeBridgeInfoGrpcRequest request)
        {
            var bridgeBrand = _settingsModel.RedirectUrlMapping.GetBridgeBrand();
            _logger.Information("LeobankGrpcService start process DecodeInfoAsync {bridgeBrand} {@request}", bridgeBrand, request);
            try
            {
                var response = JsonConvert.DeserializeObject<LeobankStatusResponse>(request.PciDssDecodeGrpcModel.InfoToDecode);
                response.Decrypt(_settingsModel.PrivateKeyString);
                _logger.Information("Decoded callback info for traderId {traderId} {@Result}", 
                    request.PciDssDecodeGrpcModel.TraderId, 
                    response.InfoStruct);
                return new ValueTask<DecodeBridgeInfoGrpcResponse>(DecodeBridgeInfoGrpcResponse.Create(JsonConvert.SerializeObject(response.InfoStruct), DepositBridgeRequestGrpcStatus.Success));
            }
            catch (Exception e)
            {
                _logger.Error(e, "DecodeInfoAsync failed for traderId {traderId}",
                    request.PciDssDecodeGrpcModel.TraderId);

                return new ValueTask<DecodeBridgeInfoGrpcResponse>(DecodeBridgeInfoGrpcResponse.Failed(e.Message, DepositBridgeRequestGrpcStatus.ServerError));
            }
        }

        public async ValueTask<MakeConfirmGrpcResponse> MakeDepositConfirmAsync(MakeConfirmGrpcRequest request)
        {
            var bridgeBrand = _settingsModel.RedirectUrlMapping.GetBridgeBrand();
            _logger.Information("LeobankGrpcService start process MakeDepositConfirmAsync {bridgeBrand} {@request}", bridgeBrand, request);
            try
            {
                var confirmInvoiceRequest = request.ToConfirmRequest(_settingsModel);
                confirmInvoiceRequest.PrepareDataForSending(_settingsModel.PrivateKeyString,
                    _settingsModel.PublicKeyString);
                var confirmResponse =
                    await _leobankHttpClient.GetConfirmInvoiceAsync(confirmInvoiceRequest,
                        _settingsModel.PciDssBaseUrl);
                
                confirmResponse.SuccessResult.Decrypt(_settingsModel.PrivateKeyString);
                var confirmResponseMessage = JsonConvert.SerializeObject(confirmResponse.SuccessResult.InfoStruct);

                var confirmed = (confirmResponse.SuccessResult.InfoStruct.Final == 1 &&
                                 confirmResponse.SuccessResult.InfoStruct.Status == 100);

                var messsage = LeobankTransactionStatusDictionary.Instance.GetDescription(
                    confirmResponse.SuccessResult.InfoStruct.Status).Text;

                _logger.Information("LeobankGrpcService get confirm response {@Result}",
                    confirmResponse.SuccessResult.InfoStruct);

                return MakeConfirmGrpcResponse.Create(confirmResponse.SuccessResult.InfoStruct.OrderIid,
                    confirmResponse.SuccessResult.InfoStruct.PaymentId.ToString(), 
                    DepositBridgeRequestGrpcStatus.Success, 
                    messsage, 
                    confirmed);
            }
            catch (Exception e)
            {
                _logger.Error(e, "MakeDepositConfirmAsync failed for orderId {orderId}",
                    request.PciDssOrderGrpcModel.OrderId);
                return MakeConfirmGrpcResponse.Failed(DepositBridgeRequestGrpcStatus.ServerError, e.Message);
            }
        }
    }
}