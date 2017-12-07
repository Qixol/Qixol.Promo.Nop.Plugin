using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Core.Domain.Promo;

using Nop.Web.Controllers;
using Nop.Web.Models.ShoppingCart;
using Nop.Services.Seo;
using Nop.Services.Media;
using Qixol.Plugin.Misc.Promo.Models.Checkout;
using global::Nop.Web.Models.Catalog;
using global::Nop.Web.Factories;
using Nop.Services.Discounts;
using Nop.Core.Infrastructure;
using Nop.Core.Domain.Catalog;
using Qixol.Plugin.Misc.Promo.Controllers;
using System.Xml.Linq;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using Nop.Core.Domain.Media;
using Qixol.Plugin.Misc.Promo.Models.Shared;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public partial class CheckoutModelFactory : global::Nop.Web.Factories.CheckoutModelFactory, ICheckoutModelFactory
    {
        #region Fields

        //private readonly IAddressModelFactory _addressModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        //private readonly IStoreMappingService _storeMappingService;
        //private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        //private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        //private readonly ICountryService _countryService;
        //private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        //private readonly IPaymentService _paymentService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        //private readonly IRewardPointService _rewardPointService;
        //private readonly IWebHelper _webHelper;

        //private readonly OrderSettings _orderSettings;
        //private readonly RewardPointsSettings _rewardPointsSettings;
        //private readonly PaymentSettings _paymentSettings;
        private readonly ShippingSettings _shippingSettings;
        //private readonly AddressSettings _addressSettings;
        //private readonly MediaSettings _mediaSettings;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoService _promoService;
        private readonly IPromoUtilities _promoUtilities;

        //private readonly IProductAttributeParser _productAttributeParser;
        //private readonly IProductAttributeFormatter _productAttributeFormatter;
        //private readonly ICategoryService _categoryService;
        //private readonly IProductService _productService;

        //private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        //private readonly ICatalogModelFactory _catalogModelFactory;

        #endregion

        #region Ctor

        public CheckoutModelFactory(IAddressModelFactory addressModelFactory,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            ILocalizationService localizationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IOrderProcessingService orderProcessingService,
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IShippingService shippingService,
            IPaymentService paymentService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IRewardPointService rewardPointService,
            IWebHelper webHelper,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
            PaymentSettings paymentSettings,
            ShippingSettings shippingSettings,
            AddressSettings addressSettings,
            MediaSettings mediaSettings,
            PromoSettings promoSettings,
            IPromoService promoService,
            IPromoUtilities promoUtilities,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            ICategoryService categoryService,
            IProductService productService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            ICatalogModelFactory catalogModelFactory
            ) :
            base(addressModelFactory, workContext, storeContext,
                storeMappingService, localizationService, taxService, currencyService,
                priceFormatter, orderProcessingService, genericAttributeService,
                countryService, stateProvinceService, shippingService,
                paymentService, orderTotalCalculationService, rewardPointService,
                webHelper, orderSettings, rewardPointsSettings,
                paymentSettings, shippingSettings, addressSettings)
        {
            //this._addressModelFactory = addressModelFactory;
            this._workContext = workContext;
            this._storeContext = storeContext;
            //this._storeMappingService = storeMappingService;
            //this._localizationService = localizationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            //this._orderProcessingService = orderProcessingService;
            this._genericAttributeService = genericAttributeService;
            //this._countryService = countryService;
            //this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            //this._paymentService = paymentService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            //this._rewardPointService = rewardPointService;
            //this._webHelper = webHelper;

            //this._orderSettings = orderSettings;
            //this._rewardPointsSettings = rewardPointsSettings;
            //this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            //this._addressSettings = addressSettings;
            //this._mediaSettings = mediaSettings;

            this._promoSettings = promoSettings;
            this._promoService = promoService;
            this._promoUtilities = promoUtilities;

            //this._productAttributeParser = productAttributeParser;
            //this._productAttributeFormatter = productAttributeFormatter;
            //this._categoryService = categoryService;
            //this._productService = productService;

            //this._shoppingCartModelFactory = shoppingCartModelFactory;
            //this._catalogModelFactory = catalogModelFactory;
        }

        #endregion

        #region Methods

        public override CheckoutShippingAddressModel PrepareShippingAddressModel(int? selectedCountryId = default(int?), bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            var model = base.PrepareShippingAddressModel(selectedCountryId, prePopulateNewAddressWithCustomerFields, overrideAttributesXml);

            if (!_promoSettings.Enabled)
                return model;

            if (!model.AllowPickUpInStore)
                return model;

            if (!model.PickupPoints.Any())
                return model;

            model.PickupPoints.ToList().ForEach(pp =>
            {
                decimal pickupFee = decimal.Zero;
                decimal.TryParse(pp.PickupFee.Substring(1, pp.PickupFee.Length - 1), out pickupFee);

                var shippingOption = new ShippingOption()
                {
                    Name = pp.Name,
                    Description = pp.Description,
                    Rate = pickupFee,
                    ShippingRateComputationMethodSystemName = pp.ProviderSystemName
                };

                var basketResponse = _promoService.ProcessShoppingCart(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id, shippingOption);

                if (basketResponse != null)
                {
                    if (basketResponse.DeliveryPromos().Any())
                    {
                        pickupFee = basketResponse.DeliveryPrice;
                        var pickupFeeLocalCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(pickupFee, _workContext.WorkingCurrency);
                        pp.PickupFee = _priceFormatter.FormatShippingPrice(pickupFeeLocalCurrency, true);

                        var promotions = new List<PromotionModel>();
                        var totalDiscount = decimal.Zero;
                        basketResponse.DeliveryPromos().ToList().ForEach(dp =>
                        {
                            var discountAmountLocalCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(dp.DiscountAmount, _workContext.WorkingCurrency);
                            promotions.Add(new PromotionModel()
                            {
                                PromotionName = dp.DisplayDetails(),
                                PromotionId = dp.PromotionId.ToString(),
                                DiscountAmount = _priceFormatter.FormatShippingPrice(discountAmountLocalCurrency, true)
                            });
                            totalDiscount += discountAmountLocalCurrency;
                        });

                        pp.CustomProperties.Add("TotalDiscount", _priceFormatter.FormatShippingPrice(totalDiscount, true));
                        pp.CustomProperties.Add("Promotions", promotions);
                    }
                }
            });

            return model;
        }
        /// <summary>
        /// Prepare shipping method model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <returns>Shipping method model</returns>
        public override CheckoutShippingMethodModel PrepareShippingMethodModel(IList<ShoppingCartItem> cart, Address shippingAddress)
        {
            var baseModel = base.PrepareShippingMethodModel(cart, shippingAddress);

            if (!_promoSettings.Enabled)
                return baseModel;

            var promoModel = new PromoCheckoutShippingMethodModel()
            {
                CustomProperties = baseModel.CustomProperties,
                NotifyCustomerAboutShippingFromMultipleLocations = baseModel.NotifyCustomerAboutShippingFromMultipleLocations,
                ShippingMethods = baseModel.ShippingMethods
            };

            promoModel.ShippingMethods.ToList().ForEach(sm =>
            {
                var psoModel = new PromoCheckoutShippingMethodModel.PromoShippingMethodModel()
                {
                    CustomProperties = sm.CustomProperties,
                    Description = sm.Description,
                    // Fee
                    Name = sm.Name,
                    Selected = sm.Selected,
                    ShippingOption = sm.ShippingOption,
                    ShippingRateComputationMethodSystemName = sm.ShippingRateComputationMethodSystemName
                };

                var basketResponse = _promoService.ProcessShoppingCart(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id, sm.ShippingOption);

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
                    var fee = _currencyService.ConvertFromPrimaryStoreCurrency(basketResponse.DeliveryPrice, _workContext.WorkingCurrency);
                    psoModel.Fee = _priceFormatter.FormatShippingPrice(fee, true);
                }

                promoModel.PromoShippingMethods.Add(psoModel);
            });

            return promoModel;
        }

        #endregion
    }
}
