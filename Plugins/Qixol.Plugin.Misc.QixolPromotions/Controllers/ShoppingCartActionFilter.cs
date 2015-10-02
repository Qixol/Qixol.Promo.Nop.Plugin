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
                if (actionDescriptor.ActionName.Equals("cart", StringComparison.InvariantCultureIgnoreCase)
                    || actionDescriptor.ActionName.Equals("ordersummary", StringComparison.InvariantCultureIgnoreCase))     // KW_REVIEW
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

            if (filterContext.IsChildAction)
            {
                // KW_REVIEW
                if (actionName.Equals("ordersummary", StringComparison.InvariantCultureIgnoreCase))
                {
                    bool prepareAndDisplayOrderReviewData = false;
                    var routeParameter = filterContext.RouteData.Values["prepareAndDisplayOrderReviewData"];
                    if (routeParameter != null)
                        bool.TryParse(routeParameter.ToString(), out prepareAndDisplayOrderReviewData);

                    var promoShoppingCartController = EngineContext.Current.Resolve<Qixol.Plugin.Misc.Promo.Controllers.ShoppingCartController>();
                    filterContext.Result = promoShoppingCartController.PromoOrderSummary(prepareAndDisplayOrderReviewData);


                    // DM: This approach didn't work - but is quite interesting...  Attempt to override the action descriptor.  Has no effect...
                    //
                    //var controllerDescriptor = new ReflectedControllerDescriptor(promoShoppingCartController.GetType());
                    //var newActionDescription = controllerDescriptor.FindAction(filterContext.Controller.ControllerContext, "PromoOrderSummary");
                    //filterContext.Controller = promoShoppingCartController;
                    //filterContext.ActionDescriptor = newActionDescription;

                }
                return;
            }

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var promoSettings = EngineContext.Current.Resolve<PromoSettings>();
            if (!promoSettings.Enabled)
                return;

            if (filterContext.Controller is global::Nop.Web.Controllers.ShoppingCartController)
            {
                if (actionName.Equals("cart", StringComparison.InvariantCultureIgnoreCase))
                {
                    var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                    var promoService = EngineContext.Current.Resolve<IPromoService>();
                    var workContext = EngineContext.Current.Resolve<IWorkContext>();
                    var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

                    if ((filterContext.ActionParameters.Count > 0) && (filterContext.ActionParameters["form"] != null))
                    {
                        var form = (FormCollection)filterContext.ActionParameters["form"];

                        if (form.AllKeys.Contains("removediscountcouponcode"))
                        {
                            genericAttributeService.SaveAttribute<string>(workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCouponCode, null);
                            promoService.ProcessShoppingCart();
                        }

                        if (form.AllKeys.Contains("updatecart"))
                        {
                            var cart = workContext.CurrentCustomer.ShoppingCartItems
                                            .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                                            .LimitPerStore(storeContext.CurrentStore.Id)
                                            .ToList(); 
                            ParseAndSaveCheckoutAttributes(cart, form);
                        }

                        if (form.AllKeys.Contains("applydiscountcouponcode"))
                        {
                            genericAttributeService.SaveAttribute<string>(workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCouponCode, form["discountcouponcode"]);
                            promoService.ProcessShoppingCart();
                        }
                    }

                    // The form failed at one point and this code was needed
                    //if (string.Compare(((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo.Name, "RemoveDiscountCoupon", StringComparison.InvariantCultureIgnoreCase) == 0)
                    //{
                    //    genericAttributeService.SaveAttribute<string>(workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCouponCode, null);
                    //    promoService.ProcessShoppingCartWithpromoService();
                    //    return;
                    //}

                    base.OnActionExecuting(filterContext);
                }
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

            ////don't apply filter to child methods
            //if (filterContext.IsChildAction)
            //    return;

            if (!DataSettingsHelper.DatabaseIsInstalled())
                return;

            var promoSettings = EngineContext.Current.Resolve<PromoSettings>();
            if (!promoSettings.Enabled)
                return;

            if (filterContext.Controller is global::Nop.Web.Controllers.ShoppingCartController)
            {
                if (actionName.Equals("cart", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (filterContext.HttpContext.Request != null && filterContext.HttpContext.Request.Form != null)
                    {
                        if (filterContext.HttpContext.Request.Form.AllKeys.Contains("updatecart") ||
                            filterContext.HttpContext.Request.Form.AllKeys.Contains("removediscountcouponcode"))
                            //filterContext.HttpContext.Request.Form.AllKeys.Contains("applydiscountcouponcode"))
                        {
                            filterContext.Result = new RedirectToRouteResult("ShoppingCart", new RouteValueDictionary());
                            return;
                        }
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }

        // Hack because the base code does not do this when updatecart is called
        private void ParseAndSaveCheckoutAttributes(List<ShoppingCartItem> cart, FormCollection form)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (form == null)
                throw new ArgumentNullException("form");

            var checkoutAttributeService = EngineContext.Current.Resolve<ICheckoutAttributeService>();
            var storeContext = EngineContext.Current.Resolve<IStoreContext>();
            var checkoutAttributeParser = EngineContext.Current.Resolve<ICheckoutAttributeParser>();
            var downloadService = EngineContext.Current.Resolve<IDownloadService>();
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

            string attributesXml = "";
            var checkoutAttributes = checkoutAttributeService.GetAllCheckoutAttributes(storeContext.CurrentStore.Id, !cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                string controlId = string.Format("checkout_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                int selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                    attributesXml = checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        attributesXml = checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid downloadGuid;
                            Guid.TryParse(form[controlId], out downloadGuid);
                            var download = downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = checkoutAttributeParser.AddCheckoutAttribute(attributesXml,
                                           attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //save checkout attributes
            genericAttributeService.SaveAttribute(workContext.CurrentCustomer, SystemCustomerAttributeNames.CheckoutAttributes, attributesXml, storeContext.CurrentStore.Id);
        }

    }
}
