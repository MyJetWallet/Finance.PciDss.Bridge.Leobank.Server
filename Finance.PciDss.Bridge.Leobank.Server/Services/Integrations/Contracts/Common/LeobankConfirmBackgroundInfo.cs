using System;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts
{
    public class LeobankConfirmBackgroundInfo
    {
        public DateTime PaymentUtcTimeStamp { get; set; }
        public int PaymentId { get; set; }
        public string OrderIid { get; set; }
        public string ActivityId { get; set; }
        public string TraderId { get; set; }
    }
}
