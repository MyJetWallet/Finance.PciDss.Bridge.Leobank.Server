using DotNetCoreDecorators;
using Finance.PciDss.PciDssBridgeGrpc;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Finance.PciDss.Bridge.Leobank.Server.Services;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Enums;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDss.PciDssBridgeGrpc.Contracts.Enums;
using Finance.PciDss.PciDssBridgeGrpc.Models;

namespace Finance.PciDss.Bridge.Leobank.Server
{
    public class BackgroundJobs : IHostedService
    {
        private const int WaitConfirmationInterval15Minutes = 15;

        private readonly TaskTimer _operationsTimer = new TaskTimer(TimeSpan.FromSeconds(10));
        private readonly IFinancePciDssBridgeGrpcService _financePciDssBridgeGrpcService;
        private readonly ILogger _logger;

        public BackgroundJobs(IFinancePciDssBridgeGrpcService financePciDssBridgeGrpcService,
            ILogger logger)
        {
            _financePciDssBridgeGrpcService = financePciDssBridgeGrpcService;
            _logger = logger;
            _operationsTimer.Register("OperationsTimer", async () => { await Process(); });
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _operationsTimer.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _operationsTimer.Stop();
        }

        private async Task Process()
        {
            MakeConfirmGrpcRequest confirmRequest = null;
            GetDepositStatusGrpcRequest statusRequest = null;
            try
            {
                foreach (var operation in Startup.ConfirmDepositOperationsQueue)
                {
                    using var currentActivity = string.IsNullOrEmpty(operation.Value.ActivityId)
                        ? Activity.Current
                        : new Activity("confirm").SetParentId(operation.Value.ActivityId).Start();

                    var transaction = operation.Value;
                    // Check confirm expiration
                    var spanMinutes = (int)DateTime.UtcNow.Subtract(operation.Value.PaymentUtcTimeStamp).TotalMinutes;
                    if (spanMinutes > WaitConfirmationInterval15Minutes)
                    {
                        if (!Startup.ConfirmDepositOperationsQueue.TryRemove(operation.Key, out _))
                        {
                            _logger.Warning("LeobankGrpcService can't remove status request {@Transaction}",
                                transaction);
                            continue;
                        }
                        _logger.Information(
                            "LeobankGrpcService confirm process expired {@Transaction}",
                            transaction);
                        continue;
                    }

                    // Status
                    statusRequest = new GetDepositStatusGrpcRequest
                    {
                        PciDssStatausGrpcModel = new PciDssStatusGrpcModel
                        {
                            OrderId = transaction.OrderIid,
                            PsTransactionId = transaction.PaymentId.ToString(),
                            AccountId = "",
                            BrandName = "",
                            PaymentProvider = LeobankGrpcService.PaymentSystemId,
                            TraderId = transaction.TraderId
                        }
                    };

                    var statusResponse = await _financePciDssBridgeGrpcService.GetDepositStatusAsync(statusRequest);

                    if (statusResponse.Status != DepositBridgeRequestGrpcStatus.Success)
                    {
                        _logger.Warning(
                            "LeobankGrpcService can't confirm {@Transaction} {@StatusResponse}",
                            transaction, statusRequest);
                        continue;

                    }

                    // Try to confirm
                    switch (statusResponse.StatusCode)
                    {
                        case 100:
                            {
                                _logger.Information(
                                    "LeobankGrpcService already confirmed {@Response}",
                                    statusResponse);
                                if (!Startup.ConfirmDepositOperationsQueue.TryRemove(operation.Key, out _))
                                {
                                    _logger.Warning("LeobankGrpcService can't remove status request {@Response}",
                                        statusResponse);
                                }
                            }
                            break;

                        case 49:
                            {
                                // Confirm
                                confirmRequest = new MakeConfirmGrpcRequest
                                {
                                    PciDssOrderGrpcModel = new PciDssOrderGrpcModel
                                    {
                                        OrderId = transaction.OrderIid,
                                        PsTransactionId = transaction.PaymentId.ToString()
                                    }
                                };

                                _logger.Information(
                                    "LeobankGrpcService try to confirm {@StatusResponse} {@ConfirmRequest}",
                                    statusResponse, confirmRequest);

                                var confirmResponse =
                                    await _financePciDssBridgeGrpcService.MakeDepositConfirmAsync(confirmRequest);
                                if (confirmResponse.Status == DepositBridgeRequestGrpcStatus.Success &&
                                    confirmResponse.Confirmed)
                                {
                                    if (!Startup.ConfirmDepositOperationsQueue.TryRemove(operation.Key, out _))
                                    {
                                        _logger.Warning("LeobankGrpcService can't remove status request {@Response}",
                                            statusResponse);
                                        break;
                                    }

                                    _logger.Information(
                                        "LeobankGrpcService confirm process successfully finished {@ConfirmResponse}",
                                        confirmResponse);
                                }
                            }
                            break;

                        //default:
                        //    //_logger.Information(
                        //    //    "LeobankGrpcService waiting to confirm {@StatusResponse}",
                        //    //    statusResponse);
                        //    //continue;
                        //    break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("LeobankGrpcService can't confirm {@StatusRequest} {@ConfirmRequest}", statusRequest, confirmRequest);
            }
        }
    }
}