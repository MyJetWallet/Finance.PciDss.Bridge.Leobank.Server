using System.Threading.Tasks;
using Finance.PciDss.Bridge.Leobank.Server.Services.Extensions;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Requests;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Integrations
{
    public class LeobankHttpClient : ILeobankHttpClient
    {
        public async Task<Response<LeobankInvoiceResponse, LeobankFailResponseDataPayment>> 
            RegisterInvoiceAsync(LeobankInvoiceRequest request, string baseUrl)
        {
            var result = await baseUrl
                .AppendPathSegments("api")
                .WithHeader("Content-Type", "application/json")
                .AllowHttpStatus("400")
                //.PostJsonAsync(request)
                .PostStringAsync(JsonConvert.SerializeObject(request));
            return await result.DeserializeTo<LeobankInvoiceResponse, LeobankFailResponseDataPayment>();
        }

        public async Task<Response<LeobankStatusResponse, LeobankFailResponseDataPayment>>
            GetStatusInvoiceAsync(LeobankStatusRequest request, string baseUrl)
        {
            var result = await baseUrl
                .AppendPathSegments("api")
                .WithHeader("Content-Type", "application/json")
                .AllowHttpStatus("400")
                //.PostJsonAsync(request)
                .PostStringAsync(JsonConvert.SerializeObject(request));
            return await result.DeserializeTo<LeobankStatusResponse, LeobankFailResponseDataPayment>();
        }

        public async Task<Response<LeobankConfirmResponse, LeobankFailResponseDataPayment>>
            GetConfirmInvoiceAsync(LeobankConfirmRequest request, string baseUrl)
        {
            var result = await baseUrl
                .AppendPathSegments("api")
                .WithHeader("Content-Type", "application/json")
                .AllowHttpStatus("400")
                //.PostJsonAsync(request)
                .PostStringAsync(JsonConvert.SerializeObject(request));
            return await result.DeserializeTo<LeobankConfirmResponse, LeobankFailResponseDataPayment>();
        }
    }
}