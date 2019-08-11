using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace AzTech.Plugin.Payments.Zarinpal.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.Payments.Zarinpal.ResultHandler", "Plugins/PaymentZarinpal/ResultHandler",
                 new { controller = "PaymentZarinpal", action = "ResultHandler" });

            routeBuilder.MapRoute("Plugin.Payments.Zarinpal.ErrorHandler", "Plugins/PaymentZarinpal/ErrorHandler",
                 new { controller = "PaymentZarinpal", action = "ErrorHandler" });

            // //IPN
            // routeBuilder.MapRoute("Plugin.Payments.Zarinpal.IPNHandler", "Plugins/PaymentZarinpal/IPNHandler",
            //      new { controller = "PaymentZarinpal", action = "IPNHandler" });

            // //Cancel
            // routeBuilder.MapRoute("Plugin.Payments.Zarinpal.CancelOrder", "Plugins/PaymentZarinpal/CancelOrder",
            //      new { controller = "PaymentZarinpal", action = "CancelOrder" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}