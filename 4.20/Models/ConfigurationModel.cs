using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace AzTech.Plugin.Payments.Zarinpal.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Zarinpal.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("Plugins.Payments.Zarinpal.Fields.MerchantID")]
        public string MerchantID { get; set; }
        public bool MerchantID_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Zarinpal.Fields.BlockOverseas")]
        public bool BlockOverseas  { get; set; }
        public bool BlockOverseas_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.Payments.Zarinpal.Fields.RialToToman")]
        public bool RialToToman  { get; set; }
        public bool RialToToman_OverrideForStore { get; set; }
    }
}