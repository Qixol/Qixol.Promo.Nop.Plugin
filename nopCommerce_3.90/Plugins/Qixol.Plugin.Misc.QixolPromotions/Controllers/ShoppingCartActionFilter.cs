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
using Nop.Services.Customers;
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
using Nop.Services.Discounts;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public class ShoppingCartActionFilter : ActionFilterAttribute, IFilterProvider
    {
        #region fields

        private readonly List<string> _actionNames = new List<string>
        {
            "Cart"
        };

        #endregion

        #region Utilities

        #endregion

        #region methods

        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            if (controllerContext.Controller is global::Nop.Web.Controllers.ShoppingCartController)
            {
                if (_actionNames.Contains(actionDescriptor.ActionName))
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
                if (actionName.Equals("Cart", StringComparison.InvariantCultureIgnoreCase))
                {
                    var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                    var promoService = EngineContext.Current.Resolve<IPromoService>();
                    var workContext = EngineContext.Current.Resolve<IWorkContext>();
                    var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
                    var discountService = EngineContext.Current.Resolve<IDiscountService>();

                    if (filterContext.ActionParameters != null && filterContext.ActionParameters.Any())
                    {
                        if (filterContext.ActionParameters["form"] != null)
                        {
                            var form = (FormCollection) filterContext.ActionParameters["form"];

                            if (form.AllKeys.Contains("applydiscountcouponcode"))
                            {
                                workContext.CurrentCustomer.ApplyDiscountCouponCode(form["discountcouponcode"]);
                                promoService.ProcessShoppingCart(workContext.CurrentCustomer, storeContext.CurrentStore.Id);
                            }

                            var removeDiscountKeys = (from k in form.AllKeys where k.StartsWith("removediscount-") select k).ToList();
                            if (removeDiscountKeys != null && removeDiscountKeys.Any())
                            {
                                removeDiscountKeys.ForEach(rdk =>
                                {
                                    var rdkArray = rdk.Split('-');
                                    if (rdkArray != null && rdkArray.Length == 2)
                                    {
                                        int promotionId = 0;
                                        if (int.TryParse(rdkArray[1], out promotionId))
                                        {
                                            var coupon = discountService.GetDiscountById(promotionId);
                                            if (coupon != null)
                                            {
                                                workContext.CurrentCustomer.RemoveDiscountCouponCode(coupon.CouponCode);
                                            }
                                        }
                                    }
                                });
                                promoService.ProcessShoppingCart(workContext.CurrentCustomer, storeContext.CurrentStore.Id);
                            }

                            if (form.AllKeys.Contains("updatecart"))
                            {
                                // force an update of the cart, then call promo, then allow the nop code to continue (ie call UpdateCart and return the view)
                                var promoShoppingCartController = EngineContext.Current.Resolve<Qixol.Plugin.Misc.Promo.Controllers.ShoppingCartController>();
                                promoShoppingCartController.UpdateCart(form);
                                promoService.ProcessShoppingCart(workContext.CurrentCustomer, storeContext.CurrentStore.Id);
                            }
                        }
                    }
                    else
                    {
                        promoService.ProcessShoppingCart(workContext.CurrentCustomer, storeContext.CurrentStore.Id);
                    }
                    base.OnActionExecuting(filterContext);
                }
            }
        }

        #endregion
    }
}
