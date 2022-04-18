using System;
using System.Diagnostics;
using Finance.PciDss.Bridge.Leobank.Server.Services;
using Finance.PciDss.Bridge.Leobank.Server.Services.Extensions;
using NUnit.Framework;
using Newtonsoft.Json;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses;

namespace Finance.PciDss.Bridge.Leobank.Server
{
    public class TestActivity
    {

        [Test]
        public void Check_Random_Activity_Setter()
        {
            for (int i = 0; i < 100; i++)
            {
                var activityNew = RequestValidator.RandomString(12);
                using var currentActivity = string.IsNullOrEmpty(activityNew)
                    ? Activity.Current
                    : new Activity("confirm").SetParentId(activityNew).Start();

                Assert.AreEqual(Activity.Current.ParentId, activityNew);
            }
        }

        [TestCase("Monfex@st@https://trading-api-test.mnftx.biz/deposit/redirect/xpate", "Monfex")]
        [TestCase("Monfex@st@https://trading-api-test.mnftx.biz/deposit/redirect/xpate|HandelPro@st@https://trading-api-test.mnftx.biz/deposit/redirect/xpate", "Monfex")]
        [TestCase("HandelPro@st@https://trading-api-test.mnftx.biz/deposit/redirect/xpate", "HandelPro")]
        [TestCase("Allianzemarket@st@https://trading-api-test.mnftx.biz/deposit/redirect/xpate", "Allianzemarket")]
        public void Test_Get_Brand_From_Settings(string settings, string brand)
        {
            var decodedBrand = settings.GetBridgeBrand();
            Assert.AreEqual(decodedBrand, brand);
        }
    }
}