using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Widgets.Promo.Services;
using Qixol.Plugin.Widgets.Promo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Factories
{
    public partial class ProductBoxPromoModelFactory : IProductBoxPromoModelFactory
    {

        #region private variables

        private readonly PromoSettings _promoSettings;
        private readonly PromoWidgetSettings _widgetSettings;
        private readonly IProductPromotionService _productPromotionService;

        #endregion

        #region constructor

        public ProductBoxPromoModelFactory(PromoSettings promoSettings,
            PromoWidgetSettings widgetSettings,
            IProductPromotionService productPromotionService)
        {
            this._promoSettings = promoSettings;
            this._widgetSettings = widgetSettings;
            this._productPromotionService = productPromotionService;
        }

        #endregion

        #region Utilities
        #endregion

        #region methods

        public ProductBoxPromotionModel PrepareProductBoxPromoModel(object additionalData)
        {
            ProductBoxPromotionModel model = new ProductBoxPromotionModel();
            model.HasPromo = false;

            if (_promoSettings.Enabled && _widgetSettings.ShowStickersInCatalogue)
            {
                int productId = 0;
                if (int.TryParse(additionalData.ToString(), out productId))
                {
                    var productPromotions = _productPromotionService.PromotionsForProduct(productId);
                    if (productPromotions.HasPromo)
                    {
                        model.Id = productId;
                        model.HasPromo = true;
                        model.ImageUrl = _productPromotionService.GetSingleImageUrl(productPromotions.PromosToDisplay, productPromotions.BaseImageUrl);
                    }
                }
            }

            return model;
        }

        #endregion
    }
}
