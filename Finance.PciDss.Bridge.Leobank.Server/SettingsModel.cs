using SimpleTrading.SettingsReader;

namespace Finance.PciDss.Bridge.Leobank.Server
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("PciDssBridgeLeobank.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("PciDssBridgeLeobank.PciDssBaseUrl")]
        public string PciDssBaseUrl { get; set; }

        [YamlProperty("PciDssBridgeLeobank.MerchantPointId")]
        public int MerchantPointId { get; set; }

        [YamlProperty("PciDssBridgeLeobank.DefaultRedirectUrl")]
        public string DefaultRedirectUrl { get; set; }

        [YamlProperty("PciDssBridgeLeobank.CallbackUrl")]
        public string CallbackUrl { get; set; }

        //{brand}@{prefix}@{redirectUrl}|{brand}@{prefix}@{redirectUrl} 
        [YamlProperty("PciDssBridgeLeobank.RedirectUrlMapping")]
        public string RedirectUrlMapping { get; set; }

        [YamlProperty("PciDssBridgeLeobank.AuditLogGrpcServiceUrl")]
        public string AuditLogGrpcServiceUrl { get; set; }

        [YamlProperty("PciDssBridgeLeobank.ConvertServiceGrpcUrl")]
        public string ConvertServiceGrpcUrl { get; set; }

        [YamlProperty("PciDssBridgeLeobank.TurnOffConvertion")]
        public bool TurnOffConvertion { get; set; }
        
        [YamlProperty("PciDssBridgeLeobank.PrivateKeyString")]
        public string PrivateKeyString { get; set; }
        
        [YamlProperty("PciDssBridgeLeobank.PublicKeyString")]
        public string PublicKeyString { get; set; }
    }
}
