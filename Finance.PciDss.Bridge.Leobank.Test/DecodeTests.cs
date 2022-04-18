using System;
using NUnit.Framework;
using Newtonsoft.Json;
using Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Responses;

namespace Finance.PciDss.Bridge.Leobank.Server
{


    public class DecodeReaponse
    {
        const string PrivateKeyString =
                "-----BEGIN RSA PRIVATE KEY-----\r\n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\r\n-----END RSA PRIVATE KEY-----";

        private const string PublicKeyString = "-----BEGIN PUBLIC KEY-----\r\nMIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAkNxtnQnO0G/AmHfwAO61\r\nI/iwm+FXZk6yUtFZjnK9HeH8GELraHhO6plYaJ2NyDe7rYzVWbrBKClmNBYiogJc\r\n5kmi+5rnW1xtEMnAaIvI21rbHHsbylOW/pEYt5AhW9DO0Avyt58Lw4YgjE3F0W+L\r\nlgf4IL2I7Nfi//RDhEv4qDVJpn5/9rta3zmsI2HgdrLrP3E2Qvnt8B3U8LXSzzrz\r\nEFai/LQtaXyuhUstQeGYSZ9iNi26+9d92jkthRzjAdXId+i8ZK9Ptx/mNnvm8zAp\r\nbrQJ7Sy13YEBWQrnM0DUJwywqdetoNiNL2ygDWi1xch2eJExjSk9uqSfXNjyFOZq\r\nfet53co3nCo8NU9ObPMocNNfkrPjxXRBQZgASUwQwuf6K0D5eCbkOSTZJTrd9dy8\r\n/Al9+DMUYjwvb3hT2Rx5FNFx8gZhivPhIcVRP/z8n6lIWIAR47kqC5+EUT/X4BCe\r\nMHolFpVcEtbelEgOyIW6vDBpEDSeXX0TeQkP4bqC3fg1FGhaRvsufZIwyycRp8rv\r\nxMdTwfmhQwIhI80uzOU8awWHZluEkLnCWNRGblFburlTCZKDrbJlacgZYg7GaujV\r\nrDjCANYVTE9rIgQ+aSu+TdhsljaVpGdgl28gmFz1TjQ1yVpaSJbc3CfQtb5yxBMl\r\nm5pgP/zqr/0AWleKzm9fW88CAwEAAQ==\r\n-----END PUBLIC KEY-----";

        /// <summary>
        /// 1. Decode "key" from BASE64
        /// 2. Decrypt "key" with private RSA key using
        /// 3. See result as JSON string with "Key" and "Iv"
        /// 4. Decode "Key" from BASE64
        /// 5. Decode "Iv" from BASE64
        /// </summary>
        //[TestCase("NPB3L3Dsbbd9KagYMQz7GVh7iQ12SWqVQYvjSw3W2gssQz1+4qWAE/ZQNdthvbpVL+OR+nXcdGapwCnURJkFgNX78RxN8nQrCBqUd/VNaxNZVqn6UNmQBDFzvzN1mpESzetc2/BnjDkNM6DR55MsaHVGD0j98b2DxusE1dIr/I4VPJ9kxTqXcrINRkFQBGiO2//SXtwMFtotXh44f+f3BU+wkjebI6bibUhFa3+OCNhpSLiMTEFsuVRmZXZQJmfQLtMUtgz9uODKAbjxZ6Xw0CnPbCEaTlrH5xhe7uV2p3W40QiAyPLv4Rw2WaSbFaOPDLolhNHGKmwrN7msUEKaS+CYb3900Qy8fCCfip6HZ3aIZcB1rS8BJ6NfvyctGJigmFoEQuvVbadSOk2n1G73gJmemqYEKjFZMeaU0r78LNhCcgi1gNrsrBTrF7hHFuWPgVBG5QuJBORnPyvEHy8JBBJ3dTxQTOMxDnfX28tr2mop0WZA/ZuI4pmtsNb5kJi5Q/bBpje+WLTw0tTaGH0AXUg2xEwywpoBIQp3tu71cKrvg119vtWyIMmjaol3QRJ6SjvaMW36jGclWkwpL6bfYM8xXB/N/mJsuRGDXvxINK9r4YKMrygNKqQDKeJpwScqolCNselojjN7NyBKrIQXw4TXSK09SUEW4DxGed7/pRI=",
        //    "xbtIP0fsLegyj0Q3GdTSqIeG52fT37qC/HBCUYiROQ0RiQPGIYm5Le0W3f0N6uDcG+0giAEw+DZhnk56++ucgUPJcpwKtQnLFg==")]
        //public void Test_Raw_Decrypt(string responseKeyField, string responseInfoField)
        //{
            //var decriptedString = Cryptograph.Decrypt(responseKeyField, PrivateKeyString);
            //var decriptedJson = JsonConvert.DeserializeObject<LeobankInvoiceKey>(decriptedString);
            //byte[] key = Convert.FromBase64String(decriptedJson.Key);
            //byte[] IV = Convert.FromBase64String(decriptedJson.Iv);

            //var info = Encoding.UTF8.GetString(Aes256CtrEncodeDecode.EnDecrypt(false, Convert.FromBase64String(responseInfoField), key, IV)); ;

            //Assert.Pass();
        //}

        //[TestCase("NPB3L3Dsbbd9KagYMQz7GVh7iQ12SWqVQYvjSw3W2gssQz1+4qWAE/ZQNdthvbpVL+OR+nXcdGapwCnURJkFgNX78RxN8nQrCBqUd/VNaxNZVqn6UNmQBDFzvzN1mpESzetc2/BnjDkNM6DR55MsaHVGD0j98b2DxusE1dIr/I4VPJ9kxTqXcrINRkFQBGiO2//SXtwMFtotXh44f+f3BU+wkjebI6bibUhFa3+OCNhpSLiMTEFsuVRmZXZQJmfQLtMUtgz9uODKAbjxZ6Xw0CnPbCEaTlrH5xhe7uV2p3W40QiAyPLv4Rw2WaSbFaOPDLolhNHGKmwrN7msUEKaS+CYb3900Qy8fCCfip6HZ3aIZcB1rS8BJ6NfvyctGJigmFoEQuvVbadSOk2n1G73gJmemqYEKjFZMeaU0r78LNhCcgi1gNrsrBTrF7hHFuWPgVBG5QuJBORnPyvEHy8JBBJ3dTxQTOMxDnfX28tr2mop0WZA/ZuI4pmtsNb5kJi5Q/bBpje+WLTw0tTaGH0AXUg2xEwywpoBIQp3tu71cKrvg119vtWyIMmjaol3QRJ6SjvaMW36jGclWkwpL6bfYM8xXB/N/mJsuRGDXvxINK9r4YKMrygNKqQDKeJpwScqolCNselojjN7NyBKrIQXw4TXSK09SUEW4DxGed7/pRI=",
        //    "xbtIP0fsLegyj0Q3GdTSqIeG52fT37qC/HBCUYiROQ0RiQPGIYm5Le0W3f0N6uDcG+0giAEw+DZhnk56++ucgUPJcpwKtQnLFg==")]
        //public void Test_CryptoLib_Decrypt(string responseKeyField, string responseInfoField)
        //{
            //Cryptograph cryptograph = new Cryptograph(PrivateKeyString, PublicKeyString);
            //var info = cryptograph.getDecryptedInfo(responseInfoField, responseKeyField);

            //Assert.Pass();
        //}


        [TestCase("{" +
            "\"merchant_point_id\": 1," +
            "\"info\": \"8KbTYKbIQ46D0MLT5JTTYsJyuEaN+Hq+NId9olXkT/82UKdndnewEVCQUpQ5tIDbJ0+OBGMrQ1XMI12pcCcixuDlRYh7rMUgPjVlVRV2AUfR7Y8O63/rnVCQPsHZ6MHXVF6NeQBScb/iywRaWxchL15QesOKC4C6F7dhRDZQmMbPxz4b3NtjlGWjAJ9CqLN32gDU+LTfSmYfVS75uVhwsr5vLhx3JlcgLet8yD7Iia4u+IG+KSUrgT77AdtI9/CL7EpSjnRZFLcEvHdwuMLKKhRCjeRUVXwL4AAJ2xwaKhXO3Rsgi5wRTT1bPCgs3VlBNj+4b+9DoR9uniQJ4T4jEGCKcGzyHEAWkWJSygXYK5LIZQr6yXyrAiQVhLK1CN3DrltsoRE+hmkRg+SvrqJxBKb+7tBBJNQFj87NBdh+tSrH9Fdre6mSvPRR9vg/jc5PC2031gn/9ov8pSe8XZbPI/T62oaX/Ffy862SFv4x3CeY4rPnZZ2eDZaL7CX4FfXJhZuL5hXGNXJAcC6eI7dnk8gG18Q64oHXcktrvc3tYw9oBluxW7mvMaDWLF2mOQeQYNMFPuPB04x84NnkOrYXH1lekjQD2lbzYFM9K3PgeUaCoC7gQIGtYjp02bvr3lpwIt4eU8phGQ==\"," +
            "\"key\": \"RC29UwcAGxZZ8T2x7tT2L9cK0Xfw4HYaLOJH744hGLkona5O86eXNXScb9ZCyYxJlTKxW15m9QEAIhssG+IxVKicCiwiAWD3VHwY49Tx+Qa3maiGEsQQklqUFZUsgfzkSzM/iTpysEXobujn2hVmEh/cvaVJ6/k9hAC9CrttIji6oKiMSw8wAVhoF/T+fckEG3u6dvMYrGemefWH5NxdgLAfB1/iU/1sOyOekhbrEAZJkgkPsHMNzS1e0DLPhQ7B/u7HSvBx/EO3y3HYeG+iNjhlCE/XaPaQRrxuOLyOLLQgN0obIfdCMymEzWb//ke8IC4TstprYN+AJp/kTqTf3UPFKifFBocwLF4xBMyp9IgFqm8ruZ2RYCA8ll743fyO6GrYDB2ZTqZk9A3WyhqRYJujaQazhU3TqiBWBjyZDPIHx3acs9J4oLc2SNC1pQnX2yyF07Ty9WzCDeUDv2Rs0jNC2Dbx1mKM1Q9Ena5Hp0reSLNeqECBdKyMG8uhZJelWSYq34Xvdizcby06OKdYXscOyvpwpSvVMuNyZFSoCdUOY2KdoVvd+i1dHPWDhVJ0OhTzOBL959O/viI/s060lSLrHNl+fnMpwRDd4Mr7Ed4LeJBEnZNZmuEsdAokeHuDZAI6wfBGoq7CTbSpTlbRkPfXzdRnBGLOQYnICTR2lIU=\"," +
            "\"sign\": \"COV/2v4YDJfR0bLnsrrNRfCcLNije0mvhGpKXMIKlvHnX1VzfIemy2iPHETLMoJTLcgtJ89EyE8njDrw0cHNPEI2AhNXqV6Q3LyLzBXLhos4lOF4C6JAzHxEnPq6UmW2HxMTYtFEd7UVwGIBEWUXMvRSLyZCiHr8i5fMLvCt7uM3qkc2R2QSQD2FfAJgAOGGdec4P9QxidUjMRs+/yb9YDIFbUgz8Icnv0hbipy8hAxASpX2eykh5+vp19Plm/TkzWrZS1Jt+wCm6QqxWh2QdkV86tu3MTydDI59i8J35HBzDVGAnubmrCg7SBHtcnu1KfVVn4Ogptwxy3OJZy4B0UpptTmMLqJKiPW/4br0nwPihDldl0kcIibjWHHxV8Dx0mMsMgmGvRQYbjTRRu4SZy4EIwKzUHxjwPdCDQBcXVMa85Bbh6AJvxBQXpF1fhUwXyeA6hwC9MuHQOHrDE9Wuvc8WIlXPkNdjcjZvrC0teuWHHTaePgj77ZUrqitUqQsNEi8g7cWxARWF+2TVbvECBm3AA5XGmWI88+HGFkH8gdZMose+yKdsyfLu8bQkRCJvguqwHaiAF6+VDHDqyPGnox0Yp4cKVSkHBlObapTH7GMe0vkrGqCmL2OVaW5Dx3PWsP7EQ2waqDHoQOiHwD8I5hgutyOYNdDERfp898D8kg=\"}")]
        public void Test_String_To_Json_Decrypt(string decodeRequest)
        {
            var response = JsonConvert.DeserializeObject<LeobankStatusResponse>(decodeRequest);
            response.Decrypt(PrivateKeyString);
            var txId = response.InfoStruct.PaymentId;
            var orderId = response.InfoStruct.OrderId;
            var invoiceResponse = JsonConvert.SerializeObject(response.InfoStruct);
            Assert.AreEqual((UInt64)txId, (UInt64)427428);
            Assert.AreEqual(orderId, "QraDMvwquEaTYraMemOQ933");
        }
    }
}