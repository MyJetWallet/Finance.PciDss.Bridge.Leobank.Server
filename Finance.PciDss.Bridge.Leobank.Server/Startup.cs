using System;
using System.Collections.Concurrent;
using Finance.PciDss.Bridge.Leobank.Server.Services;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Requests;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses;
using Finance.PciDss.PciDssBridgeGrpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using ProtoBuf.Grpc.Server;
using SimpleTrading.BaseMetrics;
using SimpleTrading.ServiceStatusReporterConnector;
using SimpleTrading.SettingsReader;

namespace Finance.PciDss.Bridge.Leobank.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public static readonly ConcurrentDictionary<string, LeobankConfirmBackgroundInfo> ConfirmDepositOperationsQueue = new ConcurrentDictionary<string, LeobankConfirmBackgroundInfo>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            SettingsModel settingsModel = SettingsReader.ReadSettings<SettingsModel>();
            services.BindSettings(settingsModel);
            services.BindLogger(settingsModel);
            services.BindHttpCLient();
            services.BindGrpcServices(settingsModel);
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddControllers();
            services.AddGrpc();
            services.AddCodeFirstGrpc(option =>
            {
                option.Interceptors.Add<ErrorLoggerInterceptor>();
                option.BindMetricsInterceptors();
            });
            services.AddTransient<IFinancePciDssBridgeGrpcService, LeobankGrpcService>();
            services.AddHostedService<BackgroundJobs>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.BindServicesTree(typeof(Startup).Assembly);
            app.BindIsAlive();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<LeobankGrpcService>();
                endpoints.MapMetrics();
            });
        }
    }
}
