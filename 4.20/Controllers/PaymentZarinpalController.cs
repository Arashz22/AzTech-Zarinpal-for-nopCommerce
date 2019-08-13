using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using System.Text;
using System.Globalization;
using Nop.Web;
using Nop.Services.Messages;
using AzTech.Plugin.Payments.Zarinpal;
using AzTech.Plugin.Payments.Zarinpal.Models;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework;
using Nop.Services.Security;

namespace Nop.Plugin.Payments.Controllers
{
    public class PaymentZarinPalController : BasePaymentController
    {
        #region Fields
        private readonly IPaymentService _paymentService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ZarinpalPaymentSettings _ZarinPalPaymentSettings;

        #endregion

        #region Ctor
        public PaymentZarinPalController(IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            ShoppingCartSettings shoppingCartSettings,
            ZarinpalPaymentSettings ZarinPalPaymentSettings)
        {
            _genericAttributeService = genericAttributeService;
            _paymentService = paymentService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
            _ZarinPalPaymentSettings = ZarinPalPaymentSettings;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var ZarinPalPaymentSettings = _settingService.LoadSetting<ZarinpalPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = ZarinPalPaymentSettings.UseSandbox,
                MerchantID = ZarinPalPaymentSettings.MerchantID,
                BlockOverseas = ZarinPalPaymentSettings.BlockOverseas,
                RialToToman = ZarinPalPaymentSettings.RialToToman
            };

            if (storeScope <= 0)
                return View("~/Plugins/Payments.ZarinPal/Views/Configure.cshtml", model);

            model.UseSandbox_OverrideForStore = _settingService.SettingExists(ZarinPalPaymentSettings, x => x.UseSandbox, storeScope);
            model.MerchantID_OverrideForStore = _settingService.SettingExists(ZarinPalPaymentSettings, x => x.MerchantID, storeScope);
            model.BlockOverseas_OverrideForStore = _settingService.SettingExists(ZarinPalPaymentSettings, x => x.BlockOverseas, storeScope);
            model.RialToToman_OverrideForStore = _settingService.SettingExists(ZarinPalPaymentSettings, x => x.RialToToman, storeScope);

            return View("~/Plugins/Payments.ZarinPal/Views/Configure.cshtml", model);
        }


        [HttpPost]
        [AuthorizeAdmin]
        [AdminAntiForgery]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var ZarinPalPaymentSettings = _settingService.LoadSetting<ZarinpalPaymentSettings>(storeScope);

            //save settings
            ZarinPalPaymentSettings.UseSandbox = model.UseSandbox;
            ZarinPalPaymentSettings.MerchantID = model.MerchantID;
            ZarinPalPaymentSettings.BlockOverseas = model.BlockOverseas;
            ZarinPalPaymentSettings.RialToToman = model.RialToToman;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSettingOverridablePerStore(ZarinPalPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ZarinPalPaymentSettings, x => x.MerchantID, model.MerchantID_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ZarinPalPaymentSettings, x => x.BlockOverseas, model.MerchantID_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(ZarinPalPaymentSettings, x => x.RialToToman, model.RialToToman_OverrideForStore, storeScope, false);


            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion

        public ActionResult ResultHandler(string Status, string Authority, string OGUID)
        {
            if (!(_paymentPluginManager.LoadPluginBySystemName("Payments.Zarinpal") is ZarinPalPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("ZarinPal module cannot be loaded");

            Guid orderNumberGuid = Guid.Empty;
            try
            {
                orderNumberGuid = new Guid(OGUID);
            }
            catch { }

            Order order = _orderService.GetOrderByGuid(orderNumberGuid);
            var total = Convert.ToInt32(Math.Round(order.OrderTotal, 2));
            if (_ZarinPalPaymentSettings.RialToToman) total = total / 10;

            if (string.IsNullOrEmpty(Status) == false && string.IsNullOrEmpty(Authority) == false)
            {
                long _refId = 0;
                System.Net.ServicePointManager.Expect100Continue = false;
                int _status = -1;
                var storeScope = _storeContext.ActiveStoreScopeConfiguration;
                var ZarinPalSettings = _settingService.LoadSetting<ZarinpalPaymentSettings>(storeScope);

                if (_ZarinPalPaymentSettings.UseSandbox)
                    using (ServiceReferenceZarinpalSandBox.PaymentGatewayImplementationServicePortTypeClient ZpalSr = new ServiceReferenceZarinpalSandBox.PaymentGatewayImplementationServicePortTypeClient())
                    {
                        var res = ZpalSr.PaymentVerificationAsync(
                            ZarinPalSettings.MerchantID,
                            Authority,
                            total).Result; //test
                        _status = res.Body.Status;
                        _refId = res.Body.RefID;
                    }
                else
                    using (ServiceReferenceZarinpal.PaymentGatewayImplementationServicePortTypeClient ZpalSr = new ServiceReferenceZarinpal.PaymentGatewayImplementationServicePortTypeClient())
                    {
                        var res = ZpalSr.PaymentVerificationAsync(
                            ZarinPalSettings.MerchantID,
                            Authority,
                            total).Result; //test
                        _status = res.Body.Status;
                        _refId = res.Body.RefID;
                    }

                var result = ZarinpalHelper.StatusToMessage(_status);

                order.OrderNotes.Add(new OrderNote()
                {
                    Note = string.Concat(
                     "پرداخت ",
                    (result.IsOk ? "" : "نا"), "موفق", " - ",
                        "پیغام درگاه : ", result.Message,
                      result.IsOk ? string.Concat(" - ", "کد پی گیری : ", _refId.ToString()) : ""
                      ),
                    DisplayToCustomer = true,
                    CreatedOnUtc = DateTime.UtcNow
                });

                _orderService.UpdateOrder(order);

                if (result.IsOk && _orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    order.AuthorizationTransactionId = _refId.ToString();
                    _orderService.UpdateOrder(order);
                    _orderProcessingService.MarkOrderAsPaid(order);
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
            }
            return RedirectToRoute("orderdetails", new { orderId = order.Id });
        }
        public ActionResult ErrorHandler(string Error)
        {
            int code = 0;
            Int32.TryParse(Error, out code);
            if (code != 0)
                Error = ZarinpalHelper.StatusToMessage(code).Message;
            ViewBag.Err = string.Concat("خطا : ", Error);
            return View("~/Plugins/Payments.Zarinpal/Views/ErrorHandler.cshtml");
        }
    }
}