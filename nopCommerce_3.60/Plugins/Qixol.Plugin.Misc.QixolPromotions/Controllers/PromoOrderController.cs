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
using Nop.Services.Orders;
using Nop.Core.Domain.Orders;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Promo;
using Nop.Services.Localization;
using Nop.Services.Directory;
using Qixol.Nop.Promo.Services.Localization;
using Qixol.Nop.Promo.Services.Orders;
using Qixol.Nop.Promo.Core.Domain.Orders;

namespace Qixol.Plugin.Misc.Promo.Controllers
{
    public partial class PromoOrderController : Controller
    {
        #region Fields

        //private readonly IPromoUtilities _promoUtilities;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly IProductMappingService _productMappingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoOrderService _promoOrderService;
        private readonly IProductService _productService;

        #endregion

        #region constructor

        public PromoOrderController(
            //IPromoUtilities promoUtilities,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            PromoSettings promoSettings,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            IProductMappingService productMappingService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            IPromoOrderService promoOrderService,
            IProductService productService)
        {
            //this._promoUtilities = promoUtilities;
            this._priceFormatter = priceFormatter;
            this._workContext = workContext;
            this._genericAttributeService = genericAttributeService;
            this._orderService = orderService;
            this._productMappingService = productMappingService;
            this._localizationService = localizationService;
            this._promoSettings = promoSettings;
            this._currencyService = currencyService;
            this._promoOrderService = promoOrderService;
            this._productService = productService;
        }

        #endregion

        #region methods

        public ActionResult GetSubTotalDiscountDisplayText(int orderId)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            PromoOrder promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);

            if (promoOrder == null)
                return new EmptyResult();

            var basketLevelPromotion = promoOrder.BasketLevelPromotion();

            if (basketLevelPromotion == null)
                return new EmptyResult();

            string subTotalDiscountText = promoOrder.GetDeliveryPromoName(_promoSettings);

            return Content(_localizationService.GetValidatedResource(subTotalDiscountText));
        }

        public ActionResult HasShippingDiscount(int orderId)
        {
            if (!_promoSettings.Enabled)
                return Content("false");

            PromoOrder promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);

            return Content(promoOrder.HasDeliveryDiscount().ToString().ToLower());
        }

        public ActionResult GetShippingOriginalAmount(int orderId, string originalAmount)
        {
            if (!_promoSettings.Enabled)
                return Content(originalAmount);

            string displayShippingPrice = originalAmount;

            PromoOrder promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);

                decimal useDeliveryPrice = promoOrder.DeliveryOriginalPrice;
                if (useDeliveryPrice != 0)
                {
                    var order = _orderService.GetOrderById(orderId);
                    useDeliveryPrice = _currencyService.ConvertCurrency(useDeliveryPrice, order.CurrencyRate);
                }
                displayShippingPrice = _priceFormatter.FormatPrice(useDeliveryPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true);

            return Content(displayShippingPrice);
        }

        public ActionResult GetShippingDiscountAmount(int orderId)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            PromoOrder promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);
            var discountAmount = promoOrder.GetDeliveryPromoDiscount();

            if (discountAmount != decimal.Zero)
            {
                var order = _orderService.GetOrderById(orderId);
                discountAmount = _currencyService.ConvertCurrency(discountAmount, order.CurrencyRate);
                return Content(_priceFormatter.FormatPrice(-discountAmount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true));
            }

            return Content(string.Empty);
        }

        public ActionResult GetShippingDiscountName(int orderId)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            PromoOrder promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);

            return Content(_localizationService.GetValidatedResource(promoOrder.GetDeliveryPromoName(_promoSettings)));
        }

        public ActionResult GetOrderTotalDiscountName(int orderId)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            PromoOrder promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);
            return Content(_localizationService.GetValidatedResource(promoOrder.GetBasketLevelPromotionName(_promoSettings)));
        }

        public ActionResult GetLineDiscountName(int orderId, global::Nop.Web.Models.Order.OrderDetailsModel.OrderItemModel orderItemModel)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            PromoOrder promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);
            if(promoOrder!=null)
            {
                var orderItem = _orderService.GetOrderItemById(orderItemModel.Id);
                var promoNamesList = promoOrder.GetLineDiscountNames(orderItem.Product, _promoSettings, orderItem.AttributesXml);
                if(promoNamesList != null && promoNamesList.Count > 0)
                    return Content(string.Join("<br />", promoNamesList.Select(n => _localizationService.GetValidatedResource(n)).ToArray()));
            }

            return Content(string.Empty);
        }

        public ActionResult GetLineDiscountAmount(int orderId, global::Nop.Web.Models.Order.OrderDetailsModel.OrderItemModel orderItemModel)
        {
            if (!_promoSettings.Enabled)
                return new EmptyResult();

            PromoOrder promoOrder = _promoOrderService.GetPromoOrderByOrderId(orderId);
            if (promoOrder != null)
            {
                var orderItem = _orderService.GetOrderItemById(orderItemModel.Id);
                decimal linePromoDiscount = promoOrder.GetLineDiscountAmount(orderItem.Product, _promoSettings, orderItem.AttributesXml);
                if (linePromoDiscount != decimal.Zero)
                {
                    linePromoDiscount = _currencyService.ConvertCurrency(linePromoDiscount, orderItem.Order.CurrencyRate);
                    return Content(string.Format("{0}: {1}", _localizationService.GetResource("ShoppingCart.ItemYouSave"), _priceFormatter.FormatPrice(linePromoDiscount, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, true)));
                }
            }

            return Content(string.Empty);
        }

        #endregion
    }
}

