﻿using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;
using Qixol.Plugin.Misc.Promo.ViewEngines;

namespace Qixol.Plugin.Misc.Promo
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            #region Custom View Engine

            global::System.Web.Mvc.ViewEngines.Engines.Insert(0, new PromoViewEngine());

            #endregion

            #region admin routes

            var promosProductFeedRoute = routes.MapRoute("Qixol.Promos.GenerateFeed",
                    "Admin/PromosFeed/Admin/Feed/PromosFeed",
                    new { controller = "PromoAdmin", action = "GenerateFeed" },
                    new[] { "Qixol.Plugin.Misc.Promo.Controllers" }
            );
            promosProductFeedRoute.DataTokens.Add("area", "admin");

            var adminDiscountRoute = routes.MapRoute("QixolPromosAdminDiscount",
                    "Admin/Discount/{action}/{id}",
                    new { controller = "AdminDiscount", action = "Index", id = "", area = "PromoAdmin" },
                    new[] { "Qixol.Plugin.Misc.Promo.Controllers" }
            );
            adminDiscountRoute.DataTokens.Add("area", "admin");
            routes.Remove(adminDiscountRoute);
            routes.Insert(0, adminDiscountRoute);

            #endregion

            #region website routes

            var promosShoppingCartRoute = routes.MapRoute("PromosShoppingCart",
                    "cart/",
                    new { controller = "ShoppingCart", action = "Cart" },
                    new[] { "Qixol.Plugin.Misc.Promo.Controllers" }
                    );
            routes.Remove(promosShoppingCartRoute);
            routes.Insert(0, promosShoppingCartRoute);

            var promosCheckoutShippingMethodRoute = routes.MapRoute("PromosCheckoutShippingMethod",
                    "checkout/shippingmethod",
                    new { controller = "Checkout", action = "ShippingMethod" },
                    new[] { "Qixol.Plugin.Misc.Promo.Controllers" }
                    );
            routes.Remove(promosCheckoutShippingMethodRoute);
            routes.Insert(0, promosCheckoutShippingMethodRoute);

            var promosCheckoutOpcSaveShippingRoute = routes.MapRoute("PromosCheckoutOpcSaveShipping",
                    "checkout/OpcSaveShipping",
                    new { controller = "Checkout", action = "OpcSaveShipping" },
                    new[] { "Qixol.Plugin.Misc.Promo.Controllers" }
                    );
            routes.Remove(promosCheckoutOpcSaveShippingRoute);
            routes.Insert(0, promosCheckoutOpcSaveShippingRoute);

            var promosCheckoutOnePageRoute = routes.MapRoute("PromosCheckoutOnePage",
                    "onepagecheckout/",
                    new { controller = "Checkout", action = "OnePageCheckout" },
                    new[] { "Qixol.Plugin.Misc.Promo.Controllers" }
                    );
            routes.Remove(promosCheckoutOnePageRoute);
            routes.Insert(0, promosCheckoutOnePageRoute);

            var missedPromotionsRoute = routes.MapRoute("MissedPromotions",
                    "missedpromotions/",
                    new { controller = "MissedPromotions", action = "MissedPromotions" },
                    new[] { "Qixol.Plugin.Misc.Promo.Controllers" }
                    );
            // not strictly necessary as this is not superseding a base nop route
            routes.Remove(missedPromotionsRoute);
            routes.Insert(0, missedPromotionsRoute);

            //login page for checkout as guest
            var loginMissedPromotionsAsGuestRoute = routes.MapRoute("LoginMissedPromotionsAsGuest",
                            "login/missedpromotionsasguest",
                            new { controller = "Customer", action = "Login", checkoutAsGuest = true },
                            new[] { "Qixol.Plugin.Misc.Promo.Controllers" });
            // not strictly necessary as this is not superseding a base nop route
            routes.Remove(loginMissedPromotionsAsGuestRoute);
            routes.Insert(0, loginMissedPromotionsAsGuestRoute);

            #endregion
        }

        private void InsertNewMapRoute(RouteCollection routes, string name, string url, string controllerName, string actionName, object routeParams = null, bool isAdmin = true, bool expectsId = false, bool insertAtZeroRoute = true)
        {
            object controllerParams = null;
            if (!expectsId)
                controllerParams = new { controller = controllerName, action = actionName };
            else
            {
                controllerParams = new { controller = controllerName, action = actionName, Id = UrlParameter.Optional };
                if (routeParams == null)
                    routeParams = new { id = @"\d+" };
            }

            var newMapRoute = routes.MapRoute(name, url, controllerParams, routeParams, new[] { "Qixol.Nop.Controllers" } );

            if(isAdmin)
                newMapRoute.DataTokens.Add("area", "admin");

            if (insertAtZeroRoute)
            {
                routes.Remove(newMapRoute);
                routes.Insert(0, newMapRoute);
            }
        }

        public int Priority
        {
            get { return 999; }
        }
    }
}
