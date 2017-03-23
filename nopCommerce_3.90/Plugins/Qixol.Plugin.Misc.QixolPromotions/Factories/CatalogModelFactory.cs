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
using Nop.Services.Vendors;
using Nop.Services.Security;
using Nop.Services.Topics;
using Nop.Services.Events;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Forums;
using Nop.Core.Caching;

namespace Qixol.Plugin.Misc.Promo.Factories
{
    public partial class CatalogModelFactory : global::Nop.Web.Factories.CatalogModelFactory, ICatalogModelFactory
    {
        #region Fields

        private readonly IProductModelFactory _productModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWebHelper _webHelper;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductTagService _productTagService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ITopicService _topicService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ISearchTermService _searchTermService;
        private readonly HttpContextBase _httpContext;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ForumSettings _forumSettings;
        private readonly ICacheManager _cacheManager;
        private readonly DisplayDefaultMenuItemSettings _displayDefaultMenuItemSettings;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoService _promoService;
        private readonly IPromoUtilities _promoUtilities;

        //private readonly IProductAttributeParser _productAttributeParser;
        //private readonly IProductAttributeFormatter _productAttributeFormatter;
        //private readonly ICategoryService _categoryService;
        //private readonly IProductService _productService;

        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;

        #endregion

        #region Ctor

        public CatalogModelFactory(IProductModelFactory productModelFactory,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IVendorService vendorService,
            ICategoryTemplateService categoryTemplateService,
            IManufacturerTemplateService manufacturerTemplateService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICurrencyService currencyService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            IWebHelper webHelper,
            ISpecificationAttributeService specificationAttributeService,
            IProductTagService productTagService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            ITopicService topicService,
            IEventPublisher eventPublisher,
            ISearchTermService searchTermService,
            HttpContextBase httpContext,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings,
            VendorSettings vendorSettings,
            BlogSettings blogSettings,
            ForumSettings forumSettings,
            ICacheManager cacheManager,
            DisplayDefaultMenuItemSettings displayDefaultMenuItemSettings) :
            base(productModelFactory, categoryService, manufacturerService,
                productService, vendorService, categoryTemplateService,
                manufacturerTemplateService, workContext, storeContext,
                currencyService, pictureService, localizationService,
                priceFormatter, webHelper, specificationAttributeService,
                productTagService, aclService, storeMappingService,
                topicService, eventPublisher, searchTermService,
                httpContext, mediaSettings, catalogSettings,
                vendorSettings, blogSettings, forumSettings,
                cacheManager, displayDefaultMenuItemSettings)
        {
            this._productModelFactory = productModelFactory;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._vendorService = vendorService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._priceFormatter = priceFormatter;
            this._webHelper = webHelper;
            this._specificationAttributeService = specificationAttributeService;
            this._productTagService = productTagService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._topicService = topicService;
            this._eventPublisher = eventPublisher;
            this._searchTermService = searchTermService;
            this._httpContext = httpContext;
            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
            this._vendorSettings = vendorSettings;
            this._blogSettings = blogSettings;
            this._forumSettings = forumSettings;
            this._cacheManager = cacheManager;
            this._displayDefaultMenuItemSettings = displayDefaultMenuItemSettings;
        }

        #endregion

        #region Methods

        public IList<CategorySimpleModel> PromoPrepareCategorySimpleModels(int rootCategoryId, Category category)
        {
            return base.PrepareCategorySimpleModels(rootCategoryId, false, new List<Category> { category });
        }

        #endregion
    }
}
