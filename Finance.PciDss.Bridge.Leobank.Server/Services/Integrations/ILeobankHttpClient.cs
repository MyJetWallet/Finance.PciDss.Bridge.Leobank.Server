using System.Threading.Tasks;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Requests;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Integrations
{
    public interface ILeobankHttpClient
    {
        /// <summary>
        /// A purchase deduct amount immediately. This transaction type is intended when the goods or services
        /// can be immediately provided to the customer. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        Task<Response<LeobankInvoiceResponse, LeobankFailResponseDataPayment>> RegisterInvoiceAsync(
            LeobankInvoiceRequest request, string baseUrl);

        ///// <summary>
        ///// It allows to get previous transaction basic information
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="baseUrl"></param>
        ///// <returns></returns>
        Task<Response<LeobankStatusResponse, LeobankFailResponseDataPayment>> GetStatusInvoiceAsync(
            LeobankStatusRequest request, string baseUrl);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        Task<Response<LeobankConfirmResponse, LeobankFailResponseDataPayment>> GetConfirmInvoiceAsync(
            LeobankConfirmRequest request, string baseUrl);
    }
}