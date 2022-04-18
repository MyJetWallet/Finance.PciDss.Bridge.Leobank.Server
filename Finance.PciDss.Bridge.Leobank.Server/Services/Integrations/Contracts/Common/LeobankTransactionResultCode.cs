using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finance.PciDss.Bridge.Leobank.Server.Services.Integrations.Contracts.Enums
{
    public enum LeobankTransactionStatus
    {
        Declined,
        Pending,
        Approved,
        Completed
    }
    public class LeobankTransactionStatusDescription
    {
        public string Text { get; set; }
        public bool IsFinal { get; set; }
        public LeobankTransactionStatus Status { get; set; }
    }

    public sealed class LeobankTransactionStatusDictionary
    {
        private static Dictionary<int, LeobankTransactionStatusDescription> dictionary;
        private static readonly Lazy<LeobankTransactionStatusDictionary> lazy = new Lazy<LeobankTransactionStatusDictionary> (() => new LeobankTransactionStatusDictionary());
        public static LeobankTransactionStatusDictionary Instance { get { return lazy.Value; } }

        private LeobankTransactionStatusDictionary()
        {
            FillLeobankTransactionStatusDictionary();
        }
        private void FillLeobankTransactionStatusDictionary()
        {
            dictionary = new Dictionary<int, LeobankTransactionStatusDescription>();
            dictionary.Add(0, new LeobankTransactionStatusDescription { Text = "New payment", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(1, new LeobankTransactionStatusDescription { Text = "Error when executing the request", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(2, new LeobankTransactionStatusDescription { Text = "Request data error", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(3, new LeobankTransactionStatusDescription { Text = "Another unknown error", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(4, new LeobankTransactionStatusDescription { Text = "Unsuccessful payment", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(5, new LeobankTransactionStatusDescription { Text = "The payment was not found for the specified details", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(6, new LeobankTransactionStatusDescription { Text = "The payment has been refunded", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(7, new LeobankTransactionStatusDescription { Text = "Test payment", IsFinal = true, Status = LeobankTransactionStatus.Approved });
            dictionary.Add(8, new LeobankTransactionStatusDescription { Text = "Client's OTP confirmation is required", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(9, new LeobankTransactionStatusDescription { Text = "3DS verification is required", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(10, new LeobankTransactionStatusDescription { Text = "You need to enter the CVV of the sender's card", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(11, new LeobankTransactionStatusDescription { Text = "The sender's data is required to be entered", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(12, new LeobankTransactionStatusDescription { Text = "The recipient's data is required to be entered", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(13, new LeobankTransactionStatusDescription { Text = "The client is expected to enter the phone number", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(14, new LeobankTransactionStatusDescription { Text = "Confirmation by ivr call is expected", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(15, new LeobankTransactionStatusDescription { Text = "Pin-code confirmation is expected", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(16, new LeobankTransactionStatusDescription { Text = "Captcha confirmation is expected", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(17, new LeobankTransactionStatusDescription { Text = "Confirmation of the application password is expected ", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(18, new LeobankTransactionStatusDescription { Text = "Confirmation is expected in the application", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(19, new LeobankTransactionStatusDescription { Text = "The payment is being processed", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(20, new LeobankTransactionStatusDescription { Text = "The payment has been created, it is expected to be completed by the sender", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(21, new LeobankTransactionStatusDescription { Text = "Bitcoin transfer from the client is expected", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(22, new LeobankTransactionStatusDescription { Text = "Payment is being verified", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(24, new LeobankTransactionStatusDescription { Text = "The money has been debited from the client, confirmation of the delivery of the goods is expected", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(25, new LeobankTransactionStatusDescription { Text = "The amount was successfully blocked on the sender's account", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(26, new LeobankTransactionStatusDescription { Text = "Cash payment is expected in the TCO", IsFinal = true, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(27, new LeobankTransactionStatusDescription { Text = "The client is expected to scan the QR code", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(28, new LeobankTransactionStatusDescription { Text = "The client is expected to confirm the payment in the application", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(29, new LeobankTransactionStatusDescription { Text = "The recipient's refund method is not set", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(31, new LeobankTransactionStatusDescription { Text = "The invoice was created successfully, payment is expected", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(32, new LeobankTransactionStatusDescription { Text = "The funds for the payment are reserved for making a refund on a previously submitted application", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(35, new LeobankTransactionStatusDescription { Text = "The sender's card data must be entered", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(36, new LeobankTransactionStatusDescription { Text = "The payment confirmation time has expired", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(37, new LeobankTransactionStatusDescription { Text = "Payment is prohibited", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(38, new LeobankTransactionStatusDescription { Text = "The payment status must be specified manually", IsFinal = true, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(39, new LeobankTransactionStatusDescription { Text = "Insufficient funds on the card", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(40, new LeobankTransactionStatusDescription { Text = "The map does not exist", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(41, new LeobankTransactionStatusDescription { Text = "The card is lost or stolen", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(42, new LeobankTransactionStatusDescription { Text = "Incorrect expiration date", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(43, new LeobankTransactionStatusDescription { Text = "Operation limit exceeded", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(44, new LeobankTransactionStatusDescription { Text = "The operation is not allowed by the issuing bank", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(45, new LeobankTransactionStatusDescription { Text = "Unsuccessful payment", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(46, new LeobankTransactionStatusDescription { Text = "Unsuccessful payment", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(47, new LeobankTransactionStatusDescription { Text = "Unsuccessful payment", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(48, new LeobankTransactionStatusDescription { Text = "Payment without 3DS is prohibited, the payment will be canceled", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(49, new LeobankTransactionStatusDescription { Text = "Payment confirmation is required", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(50, new LeobankTransactionStatusDescription { Text = "The method is not allowed", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(51, new LeobankTransactionStatusDescription { Text = "Unsuccessful payment", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(52, new LeobankTransactionStatusDescription { Text = "Unsuccessful payment", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(53, new LeobankTransactionStatusDescription { Text = "Unsuccessful payment", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(54, new LeobankTransactionStatusDescription { Text = "Unsuccessful payment", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(55, new LeobankTransactionStatusDescription { Text = "Unsuccessful payment", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(56, new LeobankTransactionStatusDescription { Text = "Cancellation is not possible", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(57, new LeobankTransactionStatusDescription { Text = "The invoice was created successfully, payment is expected in the bank", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(58, new LeobankTransactionStatusDescription { Text = "LookUp code sent", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(59, new LeobankTransactionStatusDescription { Text = "LookUp code is confirmed", IsFinal = true, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(60, new LeobankTransactionStatusDescription { Text = "Exceeded the number of attempts", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(61, new LeobankTransactionStatusDescription { Text = "Payment using the specified details is no longer possible", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(62, new LeobankTransactionStatusDescription { Text = "The payment was refunded, carried out under a different ID", IsFinal = true, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(63, new LeobankTransactionStatusDescription { Text = "The payment has been refunded", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(64, new LeobankTransactionStatusDescription { Text = "LookUp is not confirmed", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(65, new LeobankTransactionStatusDescription { Text = "LookUp the number of attempts exceeded", IsFinal = true, Status = LeobankTransactionStatus.Declined });
            dictionary.Add(66, new LeobankTransactionStatusDescription { Text = "Payment cancellation is being processed", IsFinal = false, Status = LeobankTransactionStatus.Pending });
            dictionary.Add(100, new LeobankTransactionStatusDescription { Text = "The operation was completed successfully", IsFinal = true, Status = LeobankTransactionStatus.Completed });
        }
        public LeobankTransactionStatusDescription GetDescription(int statusCode)
        {
            if (dictionary == null)
            {
                FillLeobankTransactionStatusDictionary();
            }

            if (dictionary.ContainsKey(statusCode))
            {
                return dictionary[statusCode];
            }
            return null;
        }
    }
}
