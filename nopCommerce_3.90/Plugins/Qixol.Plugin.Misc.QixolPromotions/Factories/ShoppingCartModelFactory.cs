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
using Qixol.Plugin.Misc.Promo.Models.ShoppingCart;
using Qixol.Promo.Integration.Lib.Basket;

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
        private readonly IShippingService _shippingService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly ShippingSettings _shippingSettings;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoService _promoService;
        private readonly IPromoUtilities _promoUtilities;
        private readonly IPriceCalculationService _priceCalculationService;

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
            IPriceCalculationService promosPriceCalculationService)
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
            this._shippingService = shippingService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._storeContext = storeContext;
            this._shippingSettings = shippingSettings;

            this._promoSettings = promoSettings;
            this._promoService = promoService;
            this._promoUtilities = promoUtilities;
            this._priceCalculationService = promosPriceCalculationService;
        }

        #endregion

        #region Utilities

        private PromoEstimateShippingResultModel.PromoShippingOptionModel PrepareShippingOptionModelPromotions(ShippingOption so)
        {
            var psoModel = new PromoEstimateShippingResultModel.PromoShippingOptionModel()
            {
                Name = so.Name,
                Description = so.Description,
                Price = so.Description,
                DiscountAmount = _priceFormatter.FormatShippingPrice(decimal.Zero, true)
            };

            _promoService.ProcessShoppingCart(_workContext.CurrentCustomer, so);

            BasketResponse basketResponse = _promoUtilities.GetBasketResponse(_workContext.CurrentCustomer);

            if (basketResponse != null && basketResponse.IsValid())
            {
                var totalDiscount = 0M;
                basketResponse.DeliveryPromos().ToList().ForEach(dp =>
                {
                    var discount = _currencyService.ConvertFromPrimaryStoreCurrency(dp.DiscountAmount, _workContext.WorkingCurrency);
                    totalDiscount += discount;
                    psoModel.Promotions.Add(new Models.Shared.PromotionModel()
                    {
                        PromotionName = dp.DisplayDetails(),
                        PromotionId = dp.PromotionId.ToString(),
                        DiscountAmount = _priceFormatter.FormatShippingPrice(discount, true)
                    });
                });
                psoModel.DiscountAmount = _priceFormatter.FormatShippingPrice(totalDiscount, true);
                var price = _currencyService.ConvertFromPrimaryStoreCurrency(basketResponse.DeliveryPrice, _workContext.WorkingCurrency);
                psoModel.Price = _priceFormatter.FormatShippingPrice(price, true);
            }

            return psoModel;
        }

        #endregion

        #region Methods

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
                List<string> cartWarnings = _promoService.ProcessShoppingCart(_workContext.CurrentCustomer);
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
                var basketResponse = _promoUtilities.GetBasketResponse(_workContext.CurrentCustomer);
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
                                decimal tempSubTotal = _priceCalculationService.GetSubTotal(sci, true, out shoppingCartItemDiscountBase, out scDiscounts, out maximumDiscountQty);
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

                    var discountCoupons = basketResponse.Coupons.Where(c => !c.Issued);
                    foreach (var coupon in discountCoupons)
                    {
                        if (basketResponse.CouponIsValid(coupon.CouponCode))
                        {
                            int fakeId = 1;
                            model.DiscountBox.IsApplied = true;
                            model.DiscountBox.Messages.Add(_localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied"));
                            model.DiscountBox.AppliedDiscountsWithCodes.Add(new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel()
                            {
                                CouponCode = coupon.CouponCode,
                                Id = fakeId
                            });
                            fakeId++;
                        }
                        else
                        {
                            var message = string.Format("{0}: {1}", coupon.CouponCode, _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                            model.DiscountBox.IsApplied = false;
                            model.DiscountBox.Messages.Add(message);
                        }
                    }
                }
            }
            return model;
        }

        public override EstimateShippingResultModel PrepareEstimateShippingResultModel(IList<ShoppingCartItem> cart, int? countryId, int? stateProvinceId, string zipPostalCode)
        {
            if (!_promoSettings.Enabled)
                return base.PrepareEstimateShippingResultModel(cart, countryId, stateProvinceId, zipPostalCode); ;

            if (!cart.RequiresShipping())
                return new EstimateShippingResultModel();

            var promoEstimateShippingResultModel = new PromoEstimateShippingResultModel();

            var address = new Address
            {
                CountryId = countryId,
                Country = countryId.HasValue ? _countryService.GetCountryById(countryId.Value) : null,
                StateProvinceId = stateProvinceId,
                StateProvince = stateProvinceId.HasValue ? _stateProvinceService.GetStateProvinceById(stateProvinceId.Value) : null,
                ZipPostalCode = zipPostalCode,
            };

            var getShippingOptionResponse = _shippingService
                    .GetShippingOptions(cart, address, _workContext.CurrentCustomer, storeId: _storeContext.CurrentStore.Id);

            if (getShippingOptionResponse.Success)
            {
                if (getShippingOptionResponse.ShippingOptions.Any())
                {
                    getShippingOptionResponse.ShippingOptions.ToList().ForEach(so =>
                    {
                        var psoModel = PrepareShippingOptionModelPromotions(so);

                        promoEstimateShippingResultModel.PromoShippingOptions.Add(psoModel);
                        promoEstimateShippingResultModel.ShippingOptions.Add(psoModel); // for completeness
                    });
                }
                else
                    getShippingOptionResponse.Errors.ToList().ForEach(error =>
                    {
                        promoEstimateShippingResultModel.Warnings.Add(error);
                    });

                if (_shippingSettings.AllowPickUpInStore)
                {
                    var pickupPointsResponse = _shippingService.GetPickupPoints(address, _workContext.CurrentCustomer, storeId: _storeContext.CurrentStore.Id);
                    if (pickupPointsResponse.Success)
                    {
                        if (pickupPointsResponse.PickupPoints.Any())
                        {
                            var cheapestFee = pickupPointsResponse.PickupPoints.Min(p => p.PickupFee);

                            var so = new ShippingOption()
                            {
                                Name = _localizationService.GetResource("Checkout.PickupPoints"),
                                Description = _localizationService.GetResource("Checkout.PickupPoints.Description"),
                                Rate = cheapestFee
                            };

                            var psoModel = PrepareShippingOptionModelPromotions(so);

                            promoEstimateShippingResultModel.PromoShippingOptions.Add(psoModel);
                            promoEstimateShippingResultModel.ShippingOptions.Add(psoModel); // for completeness
                        };
                    }
                    else
                        pickupPointsResponse.Errors.ToList().ForEach(error =>
                        {
                            promoEstimateShippingResultModel.Warnings.Add(error);
                        });
                }
            }

            return promoEstimateShippingResultModel;
        }

        #endregion
    }
}