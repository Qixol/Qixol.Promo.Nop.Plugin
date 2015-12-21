using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Plugin.Misc.Promo.Models;
using Qixol.Nop.Promo.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Core.Domain.Catalog;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Core.Domain.Products;
using Nop.Core.Domain.Orders;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Nop.Services.Stores;
using Qixol.Nop.Promo.Services.AttributeValues;
using Nop.Services.Directory;
using Nop.Services.Media;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Plugin.Misc.Promo.Extensions.MappingExtensions;
using Nop.Services.Localization;
using Qixol.Nop.Promo.Services.Localization;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public partial class PromoController : Controller
    {
        #region Fields

        private readonly IPromoUtilities _promoUtilities;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly IProductMappingService _productMappingService;
        private readonly IProductPromoMappingService _productPromoMappingService;
        private readonly IPromoDetailService _promoDetailService;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly IAttributeValueService _attributeValueService;
        private readonly ICurrencyService _currencyService;
        private readonly IPromoPictureService _promoPictureService;
        private readonly PromoSettings _promoSettings;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region constructor

        public PromoController(
            IPromoUtilities promoUtilities,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            PromoSettings promoSettings,
            IProductMappingService productMappingService,
            IProductPromoMappingService productPromoMappingService,
            IPromoDetailService promoDetailService,
            IStoreService storeService,
            IStoreContext storeContext,
            IAttributeValueService attributeValueService,
            ICurrencyService currencyService,
            IPromoPictureService promoPictureService,
            IPictureService pictureService,
            IProductService productService,
            ILocalizationService localizationService)
        {
            this._promoUtilities = promoUtilities;
            this._priceFormatter = priceFormatter;
            this._workContext = workContext;
            this._productMappingService = productMappingService;
            this._productPromoMappingService = productPromoMappingService;
            this._promoDetailService = promoDetailService;
            this._promoSettings = promoSettings;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._attributeValueService = attributeValueService;
            this._currencyService = currencyService;
            this._promoPictureService = promoPictureService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._localizationService = localizationService;
        }

        #endregion

        #region methods

        public ActionResult GetSubTotalDiscountName()
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null || !basketResponse.IsValid())
                return new EmptyResult();

            return Content(_localizationService.GetValidatedResource(basketResponse.GetBasketLevelPromotionName(_promoSettings)));
        }

        public ActionResult HasShippingDiscount()
        {
            if (!_promoSettings.Enabled)
                return Content("false");

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null || !basketResponse.IsValid())
                return new EmptyResult();

            return Content(basketResponse.HasDeliveryDiscount().ToString().ToLower());
        }

        public ActionResult GetShippingOriginalAmount(string originalAmount)
        {
            if (!_promoSettings.Enabled)
                return Content(originalAmount);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();
            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
                return Content(originalAmount);

            if (!basketResponse.Summary.ProcessingResult)
                return Content(originalAmount);

            var useDeliveryOriginalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(basketResponse.DeliveryOriginalPrice, _workContext.WorkingCurrency);
            return Content(_priceFormatter.FormatPrice(useDeliveryOriginalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true));
        }

        public ActionResult GetShippingDiscountAmount()
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();
            if (basketResponse == null || basketResponse.Items == null || basketResponse.Summary == null)
                return Content(string.Empty);

            var useDeliveryPromoDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(basketResponse.DeliveryPromo().DiscountAmount, _workContext.WorkingCurrency);
            return Content(_priceFormatter.FormatPrice(-useDeliveryPromoDiscount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true));
        }

        public ActionResult GetShippingDiscountName()
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();
            if (basketResponse == null || !basketResponse.IsValid())
                return new EmptyResult();

            return Content(_localizationService.GetValidatedResource(basketResponse.GetDeliveryPromoName(_promoSettings)));
        }

        public ActionResult GetOrderTotalDiscountName()
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();
            if (basketResponse == null || !basketResponse.IsValid())
                return new EmptyResult();

            return Content(_localizationService.GetValidatedResource(basketResponse.GetBasketLevelPromotionName(_promoSettings)));
        }

        public ActionResult GetLineDiscountName(ShoppingCartModel.ShoppingCartItemModel shoppingCartItemModel)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

            if (basketResponse == null || !basketResponse.IsValid())
                return new EmptyResult();           

            ShoppingCartItem shoppingCartItem = (from c in _workContext.CurrentCustomer.ShoppingCartItems where c.Id == shoppingCartItemModel.Id select c).FirstOrDefault();
            if (shoppingCartItem == null)
                return new EmptyResult();

            var promoNames = basketResponse.GetLineDiscountNames(shoppingCartItem.Product, _promoSettings, shoppingCartItem.AttributesXml);
            if (promoNames != null && promoNames.Count > 0)
                return Content(string.Join("<br />", promoNames.Select(n => _localizationService.GetValidatedResource(n)).ToArray()));
            else
                return new EmptyResult();
        }

        #endregion
    }
}

