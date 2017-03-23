using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Security.Captcha;
using System.Web;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Services.Catalog;
using Nop.Services.Customers;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public partial class ShoppingCartModelFactory : global::Nop.Web.Factories.ShoppingCartModelFactory, IShoppingCartModelFactory
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILocalizationService _localizationService;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoService _promoService;
        private readonly IPromoUtilities _promoUtilities;
        private readonly IPromosPriceCalculationService _promosPriceCalculationService;

        #endregion

        #region Ctor

        public ShoppingCartModelFactory(IAddressModelFactory addressModelFactory,
            IStoreContext storeContext, 
            IWorkContext workContext, 
            IShoppingCartService shoppingCartService, 
            IPictureService pictureService, 
            ILocalizationService localizationService, 
            IProductAttributeFormatter productAttributeFormatter, 
            IProductAttributeParser productAttributeParser, 
            ITaxService taxService, 
            ICurrencyService currencyService, 
            IPriceCalculationService priceCalculationService, 
            IPriceFormatter priceFormatter, 
            ICheckoutAttributeParser checkoutAttributeParser, 
            ICheckoutAttributeFormatter checkoutAttributeFormatter, 
            IOrderProcessingService orderProcessingService, 
            IDiscountService discountService, 
            ICountryService countryService, 
            IStateProvinceService stateProvinceService, 
            IShippingService shippingService, 
            IOrderTotalCalculationService orderTotalCalculationService, 
            ICheckoutAttributeService checkoutAttributeService, 
            IPaymentService paymentService, 
            IPermissionService permissionService, 
            IDownloadService downloadService, 
            ICacheManager cacheManager, 
            IWebHelper webHelper, 
            IGenericAttributeService genericAttributeService, 
            HttpContextBase httpContext, 
            MediaSettings mediaSettings, 
            ShoppingCartSettings shoppingCartSettings, 
            CatalogSettings catalogSettings, 
            OrderSettings orderSettings, 
            ShippingSettings shippingSettings, 
            TaxSettings taxSettings, 
            CaptchaSettings captchaSettings, 
            AddressSettings addressSettings, 
            RewardPointsSettings rewardPointsSettings, 
            CustomerSettings customerSettings,
            PromoSettings promoSettings,
            IPromoService promoService,
            IPromoUtilities promoUtilities,
            IPromosPriceCalculationService promosPriceCalculationService)
            : base(addressModelFactory, 
                storeContext, 
                workContext, 
                shoppingCartService, 
                pictureService, 
                localizationService, 
                productAttributeFormatter, 
                productAttributeParser, 
                taxService, 
                currencyService, 
                priceCalculationService, 
                priceFormatter, 
                checkoutAttributeParser, 
                checkoutAttributeFormatter, 
                orderProcessingService, 
                discountService, 
                countryService, 
                stateProvinceService, 
                shippingService, 
                orderTotalCalculationService, 
                checkoutAttributeService, 
                paymentService, 
                permissionService, 
                downloadService, 
                cacheManager, 
                webHelper, 
                genericAttributeService, 
                httpContext, 
                mediaSettings, 
                shoppingCartSettings, 
                catalogSettings, 
                orderSettings, 
                shippingSettings, 
                taxSettings, 
                captchaSettings, 
                addressSettings, 
                rewardPointsSettings, 
                customerSettings)
        {
            this._workContext = workContext;
            this._mediaSettings = mediaSettings;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._localizationService = localizationService;

            this._promoSettings = promoSettings;
            this._promoService = promoService;
            this._promoUtilities = promoUtilities;
            this._promosPriceCalculationService = promosPriceCalculationService;
        }

        #endregion

        #region Utilities
        #endregion

        #region Methods

        public PictureModel PrepareCartItemPictureModel(ShoppingCartItem shoppingCartItem, string productName)
        {
            return base.PrepareCartItemPictureModel(shoppingCartItem, _mediaSettings.CartThumbPictureSize, true, productName);
        }

        /// <summary>
        /// Prepare shopping cart model
        /// </summary>
        /// <param name="model">Model instance</param>
        /// <param name="cart">Shopping cart</param>
        /// <param name="isEditable">A value indicating whether cart is editable</param>
        /// <param name="validateCheckoutAttributes">A value indicating whether we should validate checkout attributes when preparing the model</param>
        /// <param name="prepareEstimateShippingIfEnabled">A value indicating whether we should prepare "Estimate shipping" model</param>
        /// <param name="setEstimateShippingDefaultAddress">A value indicating whether we should prefill "Estimate shipping" model with the default customer address</param>
        /// <param name="prepareAndDisplayOrderReviewData">A value indicating whether we should prepare review data (such as billing/shipping address, payment or shipping data entered during checkout)</param>
        /// <returns>Model</returns>
        public override ShoppingCartModel PrepareShoppingCartModel(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool prepareEstimateShippingIfEnabled = true, bool setEstimateShippingDefaultAddress = true,
            bool prepareAndDisplayOrderReviewData = false)
        {

            if (_promoSettings.Enabled)
            {
                List<string> cartWarnings = _promoService.ProcessShoppingCart();
                // refresh the cart, in case any changes were made
                cart = (from cartItem in _workContext.CurrentCustomer.ShoppingCartItems where cartItem.ShoppingCartType.Equals(ShoppingCartType.ShoppingCart) select cartItem).ToList();
                foreach (string cartWarning in cartWarnings)
                {
                    model.Warnings.Add(cartWarning);
                }
            }

            // Get the base to do most of the work...
            base.PrepareShoppingCartModel(model, cart, isEditable, validateCheckoutAttributes, prepareEstimateShippingIfEnabled, setEstimateShippingDefaultAddress, prepareAndDisplayOrderReviewData);

            if (_promoSettings.Enabled)
            {
                var basketResponse = _promoUtilities.GetBasketResponse();
                if (basketResponse.IsValid())
                {
                    if (basketResponse.TotalDiscount != decimal.Zero)
                    {
                        cart.Where(sci => !sci.Product.CallForPrice)
                            .ToList()
                            .ForEach(sci =>
                            {
                                var cartItemModel = model.Items.Where(mi => mi.Id == sci.Id).FirstOrDefault();

                                //sub total
                                List<DiscountForCaching> scDiscounts;
                                decimal shoppingCartItemDiscountBase;
                                decimal taxRate;
                                int? maximumDiscountQty;
                                decimal tempSubTotal = _promosPriceCalculationService.GetSubTotal(sci, true, out shoppingCartItemDiscountBase, out scDiscounts, out maximumDiscountQty);
                                decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(sci.Product, tempSubTotal, out taxRate);
                                decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
                                cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

                                //display an applied discount amount
                                if (shoppingCartItemSubTotalWithDiscountBase > decimal.Zero)
                                {
                                    shoppingCartItemDiscountBase = _taxService.GetProductPrice(sci.Product, shoppingCartItemDiscountBase, out taxRate);
                                    if (shoppingCartItemDiscountBase > decimal.Zero)
                                    {
                                        decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
                                        cartItemModel.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
                                    }
                                }
                            });
                    }

                    model.DiscountBox.Messages = new List<string>();

                    var discountCouponCodes = _workContext.CurrentCustomer.ParseAppliedDiscountCouponCodes();
                    foreach (var couponCode in discountCouponCodes)
                    {
                        if (basketResponse.CouponIsValid(couponCode))
                        {
                            model.DiscountBox.IsApplied = true;
                            model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied"));
                        }
                        else
                        {
                            model.DiscountBox.IsApplied = false;
                            model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                        }
                    }
                }
            }
            return model;
        }

        #endregion
    }
}
