using Nop.Core.Configuration;

namespace AzTech.Plugin.Payments.Zarinpal
{
    /// <summary>
    /// Represents settings of the PayPal Standard payment plugin
    /// </summary>
    public class ZarinpalPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        public bool UseSandbox { get; set; }
        public string MerchantID { get; set; }
        /// <summary>
        /// Hide Zarinpal for overseases
        /// </summary>
        public bool BlockOverseas { get; set; }
        /// <summary>
        /// changes Rial to toman (if you use toman do not check)
        /// </summary>
        public bool RialToToman { get; set; }
    }
}
