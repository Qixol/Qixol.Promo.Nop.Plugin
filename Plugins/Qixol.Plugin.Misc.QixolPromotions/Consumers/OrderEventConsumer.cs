//using Nop.Core.Events;
//using Nop.Core.Domain.Orders;
//using Nop.Services.Events;
//using Qixol.Nop.Promo.Services;
//using Qixol.Nop.Promo.Core.Domain;
//using Nop.Services.Catalog;
//using System.Collections.Generic;
//using Nop.Services.Media;
//using System.Linq;
//using System.IO;
//using System.ServiceModel;
//using System.Text;
//using Nop.Services.Common;
//using Nop.Services.Customers;
//using Nop.Core.Domain.Customers;
//using Qixol.Nop.Promo.Core.Domain.Promo;
//using Nop.Core;
//using Qixol.Nop.Promo.Services.Promo;

//namespace Qixol.Plugin.Misc.Promo.Consumers
//{
//    public partial class OrderEventConsumer : IConsumer<OrderPlacedEvent>
//    {
//        #region Fields

//        private readonly PromoSettings _promoSettings;
//        private readonly IGenericAttributeService _genericAttributeService;
//        private readonly IPromoUtilities _promoUtilities;
//        private readonly IPromoService _promoService;
//        private readonly ICustomerService _customerService;
//        private readonly IStoreContext _storeContext;

//        #endregion

//        #region constructor

//        public OrderEventConsumer(PromoSettings promoSettings,
//            IGenericAttributeService genericAttributeService,
//            IPromoUtilities promoUtilities,
//            IPromoService promoService,
//            ICustomerService customerService,
//            IStoreContext storeContext)
//        {
//            this._promoSettings = promoSettings;
//            this._genericAttributeService = genericAttributeService;
//            this._promoUtilities = promoUtilities;
//            this._promoService = promoService;
//            this._customerService = customerService;
//            this._storeContext = storeContext;
//        }

//        #endregion

//        #region handlers

//        public void HandleEvent(OrderPlacedEvent orderPlacedEventMessage)
//        {
//            if (!_promoSettings.Enabled)
//                return;

//            Order placedOrder = orderPlacedEventMessage.Order;

//            Customer customer = _customerService.GetCustomerById(placedOrder.CustomerId);

//            _promoService.SendConfirmedBasket(placedOrder);

//            #region clean up

//            // basket guid
//            _genericAttributeService.SaveAttribute<string>(customer, PromoCustomerAttributeNames.PromoBasketUniqueReference, null, _storeContext.CurrentStore.Id);

//            // basket response
//            string basketResponseString = customer.GetAttribute<string>(PromoCustomerAttributeNames.PromoBasketResponse, _storeContext.CurrentStore.Id);
//            _genericAttributeService.SaveAttribute<string>(placedOrder, PromoCustomerAttributeNames.PromoBasketResponse, basketResponseString);
//            _genericAttributeService.SaveAttribute<string>(customer, PromoCustomerAttributeNames.PromoBasketResponse, null, _storeContext.CurrentStore.Id);

//            // basket request
//            _genericAttributeService.SaveAttribute<string>(customer, PromoCustomerAttributeNames.PromoBasketRequest, null, _storeContext.CurrentStore.Id);

//            // coupon code
//            _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.DiscountCouponCode, null);

//            #endregion
//        }

//#endregion
//    }
//}
