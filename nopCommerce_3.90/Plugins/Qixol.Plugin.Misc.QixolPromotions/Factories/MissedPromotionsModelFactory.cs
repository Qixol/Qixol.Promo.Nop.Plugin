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

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public partial class MissedPromotionsModelFactory : IMissedPromotionsModelFactory
    {
        #region Fields

        //private readonly IAddressModelFactory _addressModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        //private readonly IStoreMappingService _storeMappingService;
        //private readonly ILocalizationService _localizationService;
        //private readonly ITaxService _taxService;
        //private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        //private readonly IOrderProcessingService _orderProcessingService;
        //private readonly IGenericAttributeService _genericAttributeService;
        //private readonly ICountryService _countryService;
        //private readonly IStateProvinceService _stateProvinceService;
        //private readonly IShippingService _shippingService;
        //private readonly IPaymentService _paymentService;
        //private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        //private readonly IRewardPointService _rewardPointService;
        //private readonly IWebHelper _webHelper;

        //private readonly OrderSettings _orderSettings;
        //private readonly RewardPointsSettings _rewardPointsSettings;
        //private readonly PaymentSettings _paymentSettings;
        //private readonly ShippingSettings _shippingSettings;
        //private readonly AddressSettings _addressSettings;
        private readonly MediaSettings _mediaSettings;

        private readonly PromoSettings _promoSettings;
        //private readonly IPromoService _promoService;
        private readonly IPromoUtilities _promoUtilities;

        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly ICatalogModelFactory _catalogModelFactory;

        private readonly IPromoProductDetailsModelFactory _promoProductDetailsModelFactory;
        #endregion

        #region Ctor

        public MissedPromotionsModelFactory(
            //IAddressModelFactory addressModelFactory,
            IWorkContext workContext,
            IStoreContext storeContext,
            //IStoreMappingService storeMappingService,
            //ILocalizationService localizationService,
            //ITaxService taxService,
            //ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            //IOrderProcessingService orderProcessingService,
            //IGenericAttributeService genericAttributeService,
            //ICountryService countryService,
            //IStateProvinceService stateProvinceService,
            //IShippingService shippingService,
            //IPaymentService paymentService,
            //IOrderTotalCalculationService orderTotalCalculationService,
            //IRewardPointService rewardPointService,
            //IWebHelper webHelper,
            //OrderSettings orderSettings,
            //RewardPointsSettings rewardPointsSettings,
            //PaymentSettings paymentSettings,
            //ShippingSettings shippingSettings,
            //AddressSettings addressSettings,
            MediaSettings mediaSettings,
            PromoSettings promoSettings,
            IPromoService promoService,
            IPromoUtilities promoUtilities,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            ICategoryService categoryService,
            IProductService productService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            ICatalogModelFactory catalogModelFactory,
            IPromoProductDetailsModelFactory promoProductDetailsModelFactory
            )
        {
            //this._addressModelFactory = addressModelFactory;
            this._workContext = workContext;
            this._storeContext = storeContext;
            //this._storeMappingService = storeMappingService;
            //this._localizationService = localizationService;
            //this._taxService = taxService;
            //this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            //this._orderProcessingService = orderProcessingService;
            //this._genericAttributeService = genericAttributeService;
            //this._countryService = countryService;
            //this._stateProvinceService = stateProvinceService;
            //this._shippingService = shippingService;
            //this._paymentService = paymentService;
            //this._orderTotalCalculationService = orderTotalCalculationService;
            //this._rewardPointService = rewardPointService;
            //this._webHelper = webHelper;

            //this._orderSettings = orderSettings;
            //this._rewardPointsSettings = rewardPointsSettings;
            //this._paymentSettings = paymentSettings;
            //this._shippingSettings = shippingSettings;
            //this._addressSettings = addressSettings;
            this._mediaSettings = mediaSettings;

            this._promoSettings = promoSettings;
            //this._promoService = promoService;
            this._promoUtilities = promoUtilities;

            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._categoryService = categoryService;
            this._productService = productService;

            this._shoppingCartModelFactory = shoppingCartModelFactory;
            this._catalogModelFactory = catalogModelFactory;

            this._promoProductDetailsModelFactory = promoProductDetailsModelFactory;
        }

        #endregion

        #region Utilities

        private MissedPromotionBaseModel PrepareMissedPromotionModel(BasketResponseMissedPromotion missedPromotion, List<ShoppingCartItem> cart, BasketResponse basketResponse, MissedPromotionBaseModel missedPromotionModel)
        {
            string promoDisplayDetails = string.Empty;
            switch (_promoSettings.ShowPromotionDetailsInBasket)
            {
                case PromotionDetailsDisplayOptions.ShowEndUserText:
                    // The display text is not mandatory, so default it to the promotion type if there is no text.
                    if (!string.IsNullOrEmpty(missedPromotion.DisplayText))
                        promoDisplayDetails = missedPromotion.DisplayText;
                    else
                        promoDisplayDetails = missedPromotion.PromotionTypeDisplay;
                    break;

                case PromotionDetailsDisplayOptions.ShowPromotionName:
                    promoDisplayDetails = missedPromotion.PromotionName;
                    break;

                case PromotionDetailsDisplayOptions.ShowNoText:
                    promoDisplayDetails = string.Empty;
                    break;

                default:
                    promoDisplayDetails = missedPromotion.PromotionTypeDisplay;
                    break;
            }
            missedPromotionModel.PromotionName = promoDisplayDetails;
            missedPromotionModel.PromotionImageUrl = "/Plugins/Misc.QixolPromo/Content/Images/default-missedpromotion.png";
            missedPromotionModel.SaveFrom = missedPromotion.Action.SaveFrom > decimal.Zero ? _priceFormatter.FormatPrice(missedPromotion.Action.SaveFrom) : string.Empty;

            var fullyMatchedCriteria = (from mc in missedPromotion.Criteria.CriteriaItems where mc.FullyMatched select mc).ToList();
            var partiallyMatchedCriteria = (from mc in missedPromotion.Criteria.CriteriaItems where !mc.FullyMatched select mc).ToList();

            foreach (var fullyMatchedCriterium in fullyMatchedCriteria)
            {
                var matchedItems = (from fmc in fullyMatchedCriterium.Items where fmc.IsMatched select fmc).ToList();
                if (matchedItems != null)
                {
                    foreach (BasketResponseMissedPromotionCriteriaListItem matchedItem in matchedItems)
                    {
                        List<ShoppingCartModel.ShoppingCartItemModel> cartItemModels = getMatchedCartItemModels(basketResponse, cart, matchedItem);
                        if (cartItemModels != null)
                        {
                            var promoMatchedCartItemModels = missedPromotionModel.MatchedCartItemModels.ToList();
                            promoMatchedCartItemModels.AddRange(cartItemModels);
                            missedPromotionModel.MatchedCartItemModels = promoMatchedCartItemModels;
                        }
                    }
                }
            }

            foreach (var partiallyMatchedCriterium in partiallyMatchedCriteria)
            {
                var matchedItems = (from mi in partiallyMatchedCriterium.Items where mi.IsMatched && !mi.Exclude select mi).ToList();
                var unmatchedProducts = (from umi in partiallyMatchedCriterium.Items where !umi.IsMatched && !umi.Exclude select umi).ToList();

                foreach (var matchedItem in matchedItems)
                {
                    List<ShoppingCartModel.ShoppingCartItemModel> cartItemModels = getMatchedCartItemModels(basketResponse, cart, matchedItem);
                    if (cartItemModels != null)
                    {
                        var promoMatchedCartItemModels = missedPromotionModel.MatchedCartItemModels.ToList();
                        promoMatchedCartItemModels.AddRange(cartItemModels);
                        missedPromotionModel.MatchedCartItemModels = promoMatchedCartItemModels;
                    }

                    if (partiallyMatchedCriterium.AdditionalQuantity > 0)
                    {
                        if (partiallyMatchedCriterium.OnlyMatchedItems)
                        {
                            List<ShoppingCartItem> matchedCartItems = getMatchedCartItems(basketResponse, cart, matchedItem);

                            foreach (var matchedCartItem in matchedCartItems)
                            {
                                int productId = matchedCartItem.Product.Id;
                                var product = _productService.GetProductById(productId);

                                if (product != null)
                                {
                                    var productOverviewModel = _promoProductDetailsModelFactory.PromoPrepareProductOverviewModel(product);
                                    string additionalQuantity = partiallyMatchedCriterium.AdditionalQuantity.ToString();
                                    productOverviewModel.Attributes = ParseAttributeXml(matchedCartItem.AttributesXml);

                                    missedPromotionModel.UnmatchedProductOverviewModels.Add(productOverviewModel);
                                }
                            }
                        }
                        else
                        {
                            missedPromotionModel.CategorySimpleModels = getCategoryAttributes(matchedItem);
                        }
                    }
                }

                foreach (var unmatchedProduct in unmatchedProducts)
                {
                    #region missed product details

                    int productId = 0;
                    if (int.TryParse(unmatchedProduct.ProductCode, out productId))
                    {
                        var variantCode = unmatchedProduct.VariantCode;

                        var product = _productService.GetProductById(productId);

                        if (product != null)
                        {
                            var productDetailsModel = _promoProductDetailsModelFactory.PromoPrepareProductOverviewModel(product);
                            productDetailsModel.AdditionalQuantityNeededToTriggerPromotion = partiallyMatchedCriterium.AdditionalQuantity;
                            missedPromotionModel.UnmatchedProductOverviewModels.Add(productDetailsModel);
                        }
                    }

                    #endregion

                    var promoCategorySimpleModels = missedPromotionModel.CategorySimpleModels.ToList();
                    promoCategorySimpleModels.AddRange(getCategoryAttributes(unmatchedProduct));
                    missedPromotionModel.CategorySimpleModels = promoCategorySimpleModels;
                }
            }

            return missedPromotionModel;
        }

        // TODO: handle multiple line matches (does this ever occur...?)
        private List<ShoppingCartModel.ShoppingCartItemModel> getMatchedCartItemModels(BasketResponse basketResponse, List<ShoppingCartItem> cart, BasketResponseMissedPromotionCriteriaListItem matchedItem)
        {
            List<ShoppingCartModel.ShoppingCartItemModel> cartItemModels = new List<ShoppingCartModel.ShoppingCartItemModel>();
            List<ShoppingCartItem> matchedCartItems = getMatchedCartItems(basketResponse, cart, matchedItem);

            foreach (var matchedCartItem in matchedCartItems)
            {
                ShoppingCartModel.ShoppingCartItemModel cartItemModel = new ShoppingCartModel.ShoppingCartItemModel()
                {
                    Id = matchedCartItem.Id,
                    Sku = matchedCartItem.Product.FormatSku(matchedCartItem.AttributesXml, _productAttributeParser),
                    ProductId = matchedCartItem.Product.Id,
                    ProductName = matchedCartItem.Product.GetLocalized(x => x.Name),
                    ProductSeName = matchedCartItem.Product.GetSeName(),
                    Quantity = matchedCartItem.Quantity,
                    AttributeInfo = _productAttributeFormatter.FormatAttributes(matchedCartItem.Product, matchedCartItem.AttributesXml),
                };

                cartItemModel.Picture = _shoppingCartModelFactory.PrepareCartItemPictureModel(matchedCartItem, _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);

                cartItemModels.Add(cartItemModel);
            }

            return cartItemModels;
        }

        // TODO: handle multipe categories with the same name
        private Category GetCategoryFromCategoryName(string attributeValue)
        {
            var categories = _categoryService.GetAllCategories(showHidden: true);
            Category matchedCategory = null;

            foreach (var category in categories)
            {
                if (string.Compare(category.Name, attributeValue, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    matchedCategory = category;
                    continue;
                }
            }

            return matchedCategory;
        }

        private Category GetCategoryFromBreadcrumb(string attributeValue)
        {
            var categories = _categoryService.GetAllCategories(showHidden: true);
            Category matchedCategory = null;
            // TODO: store this in cache to speed up the process
            //var categoryBreadcrumbs = new Dictionary<string, Category>();
            //foreach (var c in categories)
            //{
            //    categoryBreadcrumbs.Add(c.GetFormattedBreadCrumb(categories), c);
            //}

            foreach (var category in categories)
            {
                string breadcrumb = category.GetFormattedBreadCrumb(categories);
                if (string.Compare(breadcrumb, attributeValue, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    matchedCategory = category;
                    continue;
                }
            }

            return matchedCategory;
        }

        private IDictionary<string, string> ParseAttributeXml(string attributeXml)
        {
            IDictionary<string, string> attributesList = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(attributeXml))
                return attributesList;

            var xmlDoc = XDocument.Parse(attributeXml);

            var attributes = xmlDoc.Descendants().Where(d => d.Name == "Attributes").ToList();
            foreach (var a in attributes)
            {
                var productAttributes = (from pa in a.Descendants() where pa.Name == "ProductAttribute" select pa).ToList();

                foreach (var productAttribute in productAttributes)
                {
                    var id = productAttribute.Attributes().Where(attrib => attrib.Name == "ID").FirstOrDefault().Value;

                    var productAttributeValue = (from pav in productAttribute.Descendants() where pav.Name == "ProductAttributeValue" select pav).FirstOrDefault();
                    var value = productAttributeValue.Descendants().Where(v => v.Name == "Value").FirstOrDefault().Value;

                    attributesList.Add(id, value);
                }
            }

            return attributesList;

        }

        private List<ShoppingCartItem> getMatchedCartItems(BasketResponse basketResponse, List<ShoppingCartItem> cart, BasketResponseMissedPromotionCriteriaListItem matchedItem)
        {
            List<ShoppingCartItem> matchedCartItems = new List<ShoppingCartItem>();
            matchedItem.MatchedLineIds.ForEach(matchedLineId =>
            {
                var basketResponseItems = (from bri in basketResponse.Items where bri.Id == matchedLineId select bri).ToList();

                foreach (var basketResponseItem in basketResponseItems)
                {
                    int originalCartItemId = basketResponseItem.Generated ? basketResponseItem.SplitFromLineId : basketResponseItem.Id;
                    var items = (from c in cart where c.Id == originalCartItemId select c).ToList();
                    if (items != null)
                        matchedCartItems.AddRange(items);
                }
            });

            return matchedCartItems;
        }

        private List<CategorySimpleModel> getCategoryAttributes(BasketResponseMissedPromotionCriteriaListItem item)
        {
            List<CategorySimpleModel> categoryList = new List<CategorySimpleModel>();

            if (!string.IsNullOrEmpty(item.AttributeToken) && !string.IsNullOrEmpty(item.AttributeValue))
            {
                switch (item.AttributeToken)
                {
                    case ProductAttributeConfigSystemNames.CATEGORY_BREADCRUMBS:
                        var breadCrumbCategory = GetCategoryFromBreadcrumb(item.AttributeValue);
                        var breadCrumbcategories = _catalogModelFactory.PrepareCategorySimpleModels(breadCrumbCategory.ParentCategoryId, false, new List<Category> { breadCrumbCategory }).ToList();
                        breadCrumbcategories.ForEach(c => categoryList.Add(c));
                        break;

                    case ProductAttributeConfigSystemNames.CATEGORY:
                        var category = GetCategoryFromCategoryName(item.AttributeValue);
                        var categories = _catalogModelFactory.PrepareCategorySimpleModels(category.ParentCategoryId, false, new List<Category> { category }).ToList();
                        categories.ForEach(c => categoryList.Add(c));
                        break;

                    default:
                        break;
                }
            }
            return categoryList;
        }

        #endregion

        #region Methods

        public virtual MissedPromotionsModel PrepareMissedPromotionsModel()
        {
            var model = new MissedPromotionsModel();

            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_storeContext.CurrentStore.Id)
                .ToList();

            if (cart.Count == 0)
                return model;

            var basketResponse = _promoUtilities.GetBasketResponse();
            if (basketResponse == null || basketResponse.MissedPromotions.Count == 0)
            {
                return model;
            }

            foreach (var missedPromo in basketResponse.MissedPromotions)
            {
                // TODO: enum for PromotionType (from integration lib...?)
                switch (missedPromo.PromotionType)
                {
                    case MissedPromotionsModel.PromotionTypeSystemName.BuyOneGetOneFree:
                        MissedPromotionBogofModel missedPromotionBogofModel = new MissedPromotionBogofModel();
                        model.MissedPromotions.Add(PrepareMissedPromotionModel(missedPromo, cart, basketResponse, missedPromotionBogofModel));
                        break;
                    case MissedPromotionsModel.PromotionTypeSystemName.BuyOneGetOneReduced:
                        MissedPromotionBogorModel missedPromotionBogorModel = new MissedPromotionBogorModel();
                        model.MissedPromotions.Add(PrepareMissedPromotionModel(missedPromo, cart, basketResponse, missedPromotionBogorModel));
                        break;
                    case MissedPromotionsModel.PromotionTypeSystemName.DealPrice:
                        MissedPromotionDealModel missedPromotionDealModel = new MissedPromotionDealModel();
                        model.MissedPromotions.Add(PrepareMissedPromotionModel(missedPromo, cart, basketResponse, missedPromotionDealModel));
                        break;
                    case MissedPromotionsModel.PromotionTypeSystemName.BundlePrice:
                        MissedPromotionBundleModel missedPromotionBundleModel = new MissedPromotionBundleModel();
                        model.MissedPromotions.Add(PrepareMissedPromotionModel(missedPromo, cart, basketResponse, missedPromotionBundleModel));
                        break;
                    default:
                        MissedPromotionUnknownModel missedPromotionUnknownModel = new MissedPromotionUnknownModel();
                        model.MissedPromotions.Add(PrepareMissedPromotionModel(missedPromo, cart, basketResponse, missedPromotionUnknownModel));
                        break;
                }
            }

            return model;
        }

        #endregion
    }
}
