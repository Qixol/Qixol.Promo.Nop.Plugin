using Nop.Web.Framework.Themes;

namespace Qixol.Plugin.Misc.Promo.ViewEngines
{
    public class PromoViewEngine : ThemeableRazorViewEngine
    {
        public PromoViewEngine()
        {
            /*
            AreaViewLocationFormats = new[]
                {
                };

            AreaMasterLocationFormats = new[]
                {
                };

            AreaPartialViewLocationFormats = new[]
                {
                };
            */

            ViewLocationFormats = new[]
                {
                    // themes
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Shared/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Order/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Checkout/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/MissedPromotions/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/ShoppingCart/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Customer/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Admin/{0}.cshtml",

                    // default
                    "~/Plugins/Misc.QixolPromo/Views/Shared/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Order/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Checkout/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/MissedPromotions/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/ShoppingCart/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Customer/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Admin/{0}.cshtml"
                };

            /*
            MasterLocationFormats = new[]
                {
                }
            */

            PartialViewLocationFormats =
                new[]
                {
                    // themes
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Shared/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Order/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Checkout/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/MissedPromotions/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/ShoppingCart/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Customer/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Themes/{2}/Views/Admin/{0}.cshtml",

                    // default
                    "~/Plugins/Misc.QixolPromo/Views/Shared/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Order/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Checkout/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/MissedPromotions/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/ShoppingCart/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Customer/{0}.cshtml",
                    "~/Plugins/Misc.QixolPromo/Views/Admin/{0}.cshtml"
                };

            FileExtensions = new[] { "cshtml" };
        }
    }
}
