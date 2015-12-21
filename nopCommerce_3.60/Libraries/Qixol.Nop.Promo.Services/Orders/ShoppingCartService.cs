using System;
using System.Collections.Generic;
using System.Linq;
using global::Nop.Core;
using global::Nop.Core.Data;
using global::Nop.Core.Domain.Catalog;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Domain.Orders;
using global::Nop.Services.Catalog;
using global::Nop.Services.Common;
using global::Nop.Services.Customers;
using global::Nop.Services.Directory;
using global::Nop.Services.Events;
using global::Nop.Services.Localization;
using global::Nop.Services.Security;
using global::Nop.Services.Stores;
using Qixol.Nop.Promo.Core.Domain.Promo;

namespace Qixol.Nop.Promo.Services.Orders
{
    public partial class ShoppingCartService : global::Nop.Services.Orders.ShoppingCartService, global::Nop.Services.Orders.IShoppingCartService
    {
        #region Fields

        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly global::Nop.Services.Orders.ICheckoutAttributeService _checkoutAttributeService;
        private readonly global::Nop.Services.Orders.ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomerService _customerService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly PromoSettings _promoSettings;

        #endregion

        #region Ctor

        public ShoppingCartService(IRepository<ShoppingCartItem> sciRepository,
            IWorkContext workContext, IStoreContext storeContext,
            ICurrencyService currencyService,
            IProductService productService, ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            global::Nop.Services.Orders.ICheckoutAttributeService checkoutAttributeService,
            global::Nop.Services.Orders.ICheckoutAttributeParser checkoutAttributeParser,
            IPriceFormatter priceFormatter,
            ICustomerService customerService,
            ShoppingCartSettings shoppingCartSettings,
            IEventPublisher eventPublisher,
            IPermissionService permissionService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IGenericAttributeService genericAttributeService,
            IProductAttributeService productAttributeService,
            PromoSettings promoSettings) :
            base( sciRepository,
                workContext,
                storeContext,
                currencyService,
                productService,
                localizationService,
                productAttributeParser,
                checkoutAttributeService,
                checkoutAttributeParser,
                priceFormatter,
                customerService,
                shoppingCartSettings,
                eventPublisher,
                permissionService,
                aclService,
                storeMappingService,
                genericAttributeService,
                productAttributeService)
        {
            this._sciRepository = sciRepository;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._currencyService = currencyService;
            this._productService = productService;
            this._localizationService = localizationService;
            this._productAttributeParser = productAttributeParser;
            this._checkoutAttributeService = checkoutAttributeService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._priceFormatter = priceFormatter;
            this._customerService = customerService;
            this._shoppingCartSettings = shoppingCartSettings;
            this._eventPublisher = eventPublisher;
            this._permissionService = permissionService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._genericAttributeService = genericAttributeService;
            this._productAttributeService = productAttributeService;
            this._promoSettings = promoSettings;
        }

        #endregion

        #region Methods

        public override void MigrateShoppingCart(Customer fromCustomer, Customer toCustomer, bool includeCouponCodes)
        {
            if (!_promoSettings.Enabled)
            {
                base.MigrateShoppingCart(fromCustomer, toCustomer, includeCouponCodes);
                return;
            }

            var basketUniqueReference = fromCustomer.GetAttribute<Guid>(PromoCustomerAttributeNames.PromoBasketUniqueReference, _storeContext.CurrentStore.Id);
            var basketResponseString = fromCustomer.GetAttribute<string>(PromoCustomerAttributeNames.PromoBasketResponse, _storeContext.CurrentStore.Id);

            _genericAttributeService.SaveAttribute<string>(fromCustomer, PromoCustomerAttributeNames.PromoBasketUniqueReference, null, _storeContext.CurrentStore.Id);
            _genericAttributeService.SaveAttribute<string>(fromCustomer, PromoCustomerAttributeNames.PromoBasketResponse, null, _storeContext.CurrentStore.Id);

            _genericAttributeService.SaveAttribute<Guid>(toCustomer, PromoCustomerAttributeNames.PromoBasketUniqueReference, basketUniqueReference, _storeContext.CurrentStore.Id);
            _genericAttributeService.SaveAttribute<string>(toCustomer, PromoCustomerAttributeNames.PromoBasketResponse, basketResponseString, _storeContext.CurrentStore.Id);

            base.MigrateShoppingCart(fromCustomer, toCustomer, includeCouponCodes);
        }

        #endregion
    }
}
