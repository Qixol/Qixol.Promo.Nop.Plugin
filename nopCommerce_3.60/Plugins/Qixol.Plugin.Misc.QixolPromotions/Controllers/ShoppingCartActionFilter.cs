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
using Qixol.Nop.Promo.Services.Promo;
using Nop.Core.Domain.Catalog;
using Nop.Services.Media;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public class ShoppingCartActionFilter : ActionFilterAttribute, IFilterProvider
    {
        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            if (controllerContext.Controller is global::Nop.Web.Controllers.ShoppingCartController)
            {
                if (actionDescriptor.ActionName.Equals("cart", StringComparison.InvariantCultureIgnoreCase) ||
                    actionDescriptor.ActionName.Equals("ordersummary", StringComparison.InvariantCultureIgnoreCase))
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
            if (!promoSettings.Enabled)
                return;

            if (filterContext.Controller is global::Nop.Web.Controllers.ShoppingCartController)
            {
                if (actionName.Equals("ordersummary", StringComparison.InvariantCultureIgnoreCase))
                {
                    bool prepareAndDisplayOrderReviewData = false;
                    var routeParameter = filterContext.RouteData.Values["prepareAndDisplayOrderReviewData"];
                    if (routeParameter != null)
                        bool.TryParse(routeParameter.ToString(), out prepareAndDisplayOrderReviewData);

                    var promoShoppingCartController = EngineContext.Current.Resolve<Qixol.Plugin.Misc.Promo.Controllers.ShoppingCartController>();
                    filterContext.Result = promoShoppingCartController.PromoOrderSummary(prepareAndDisplayOrderReviewData);
                }

                if (actionName.Equals("cart", StringComparison.InvariantCultureIgnoreCase))
                {
                    var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                    var promoService = EngineContext.Current.Resolve<IPromoService>();
                    var workContext = EngineContext.Current.Resolve<IWorkContext>();
                    var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

                    if ((filterContext.ActionParameters.Count > 0) && (filterContext.ActionParameters["form"] != null))
                    {
                        var form = (FormCollection)filterContext.ActionParameters["form"];

                        if (form.AllKeys.Contains("applydiscountcouponcode"))
                        {
                            genericAttributeService.SaveAttribute<string>(workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCouponCode, form["discountcouponcode"]);
                        }

                        if (form.AllKeys.Contains("removediscountcouponcode"))
                        {
                            genericAttributeService.SaveAttribute<string>(workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCouponCode, null);
                        }

                        if (form.AllKeys.Contains("updatecart"))
                        {
                            var cart = workContext.CurrentCustomer.ShoppingCartItems
                                            .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                                            .LimitPerStore(storeContext.CurrentStore.Id)
                                            .ToList();
                            var promoShoppingCartController = EngineContext.Current.Resolve<Qixol.Plugin.Misc.Promo.Controllers.ShoppingCartController>();
                            promoShoppingCartController.PromoParseAndSaveCheckoutAttributes(cart, form);
                        }

                    }

                    base.OnActionExecuting(filterContext);
                }
            }
        }
    }
}
