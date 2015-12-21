using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qixol.Plugin.Widgets.Promo.Models
{
    public class WidgetConfigModel
    {

        public int SelectedTab { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.ShowStickersInCatalogue")]
        public bool ShowStickersInCatalogue { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.ShowStickersOnProductPage")]
        public bool ShowStickersInProductPage { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.ShowAvailablePromotionsOnProductPage")]
        public bool ShowPromotionDetailsOnProductPage { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.QixolPromo.ProductPagePromotionsInWidgetZone")]
        public string ProductPagePromoDetailsWidgetZone { get; set; }

        public IList<SelectListItem> ProductPagePromoDetailsWidgetZonesList { get; set; }

        public PromoAddPictureModel AddPictureModel { get; set; }

        public PromoBannerModel AddPromoBannerModel { get; set; }

        public WidgetConfigModel()
        {
            AddPictureModel = new PromoAddPictureModel();
            AddPromoBannerModel = new PromoBannerModel();
            ProductPagePromoDetailsWidgetZonesList = new List<SelectListItem>();
        }
    }
}
