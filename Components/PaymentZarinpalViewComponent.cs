using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace AzTech.Plugin.Payments.Zarinpal.Components
{
    [ViewComponent(Name = "PaymentZarinpal")]
    public class PaymentZarinpalViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.Zarinpal/Views/PaymentInfo.cshtml");
        }
    }
}
