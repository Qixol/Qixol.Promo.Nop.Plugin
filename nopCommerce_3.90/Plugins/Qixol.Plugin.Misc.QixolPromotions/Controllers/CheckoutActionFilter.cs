using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using Nop.Services.Orders;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Payments;
using Nop.Services.Tax;
using Nop.Services.Directory;
using Nop.Core.Domain.Tax;
using Nop.Services.Localization;
using System.Web.Routing;
using Nop.Core.Domain.Shipping;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Plugin.Misc.Promo.Models.Checkout;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public class CheckoutActionFilter : ActionFilterAttribute, IFilterProvider
    {
        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            if (controllerContext.Controller is global::Nop.Web.Controllers.CheckoutController)
            {
                if (actionDescriptor.ActionName.Equals("CheckoutProgress", StringComparison.InvariantCultureIgnoreCase)
                    || actionDescriptor.ActionName.Equals("OpcSaveShippingMethod", StringComparison.InvariantCultureIgnoreCase)
                    || actionDescriptor.ActionName.Equals("ShippingMethod", StringComparison.InvariantCultureIgnoreCase)
                    || actionDescriptor.ActionName.Equals("Completed", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new List<Filter>() { new Filter(this, FilterScope.Action, 0) };
                }
            }
            return new List<Filter>();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            string actionName = filterContext.ActionDescriptor.ActionName;
            if (String.IsNullOrEmpty(actionName))
                return;

            string controllerName = filterContext.Controller.ToString();
            if (String.IsNullOrEmpty(controllerName))
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var promoSettings = EngineContext.Current.Resolve<PromoSettings>();

            if (actionName.Equals("CheckoutProgress", StringComparison.InvariantCultureIgnoreCase))
            {
                PromoCheckoutProgressStep checkoutProgressStep = PromoCheckoutProgressStep.Cart;
                if (filterContext.ActionParameters.ContainsKey("step") && filterContext.ActionParameters["step"] != null)
                {
                    string stepEnum = filterContext.ActionParameters["step"].ToString();
                    checkoutProgressStep = (PromoCheckoutProgressStep) Enum.Parse(typeof(PromoCheckoutProgressStep), stepEnum, true);
                }

                bool showMissedPromotions = promoSettings.Enabled && promoSettings.ShowMissedPromotions;

                var promoCheckoutController = EngineContext.Current.Resolve<Qixol.Plugin.Misc.Promo.Controllers.PromoCheckoutController>();
                filterContext.Result = promoCheckoutController.PromoCheckoutProgress(checkoutProgressStep, showMissedPromotions);
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            string actionName = filterContext.ActionDescriptor.ActionName;
            if (String.IsNullOrEmpty(actionName))
                return;

            string controllerName = filterContext.Controller.ToString();
            if (String.IsNullOrEmpty(controllerName))
                return;

            if (filterContext.IsChildAction)
                return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var promoSettings = EngineContext.Current.Resolve<PromoSettings>();
            if (!promoSettings.Enabled)
                return;

            if (filterContext.Controller is global::Nop.Web.Controllers.CheckoutController)
            {
                var customer = EngineContext.Current.Resolve<IWorkContext>().CurrentCustomer;
                var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                var orderTotalCalculationService = EngineContext.Current.Resolve<IOrderTotalCalculationService>();
                var promoService = EngineContext.Current.Resolve<IPromoService>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

                bool nextstep = false;

                if (filterContext.RequestContext.HttpContext.Request.Form != null)
                {
                    if (filterContext.RequestContext.HttpContext.Request.Form.AllKeys.Contains("nextstep"))
                        nextstep = true;
                }

                if (actionName.Equals("OpcSaveShippingMethod", StringComparison.InvariantCultureIgnoreCase)
                    || (actionName.Equals("ShippingMethod", StringComparison.InvariantCultureIgnoreCase) && nextstep))
                {
                    promoService.ProcessShoppingCart(customer, storeContext.CurrentStore.Id, customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, storeContext.CurrentStore.Id));
                }

                base.OnActionExecuted(filterContext);
            }
        }
    }
}
