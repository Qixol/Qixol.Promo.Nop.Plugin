using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Web.Factories;
using Qixol.Plugin.Misc.Promo.Models;
using System.Collections.Generic;
using System.Linq;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public partial class PromoProductDetailsModelFactory : IPromoProductDetailsModelFactory
    {
        #region fields

        private readonly IProductModelFactory _productModelFactory;
        private readonly MediaSettings _mediaSettings;

        #endregion

        #region constructors

        public PromoProductDetailsModelFactory(IProductModelFactory productModelFactory, MediaSettings mediaSettings)
        {
            this._productModelFactory = productModelFactory;
            this._mediaSettings = mediaSettings;
        }

        #endregion

        #region Utilities
        #endregion

        #region methods

        /// <summary>
        /// Prepare the promo product details model
        /// </summary>
        /// <returns>Issued coupons model</returns>
        public PromoProductOverviewModel PromoPrepareProductOverviewModel(Product product, ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            PromoProductOverviewModel promoProductDetailsModel = new PromoProductOverviewModel();
            var productOverviewModels = _productModelFactory.PrepareProductOverviewModels(new List<Product>() { product },
                preparePictureModel: true,
                preparePriceModel: true,
                productThumbPictureSize: _mediaSettings.CartThumbPictureSize);

            promoProductDetailsModel.ProductOverviewModel = productOverviewModels.FirstOrDefault();

            return promoProductDetailsModel;
        }

        #endregion
    }
}
