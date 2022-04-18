using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts
{
    public class LeobankInvoiceKey
    {
        [JsonProperty("iv")] public string Iv { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
    }
}
