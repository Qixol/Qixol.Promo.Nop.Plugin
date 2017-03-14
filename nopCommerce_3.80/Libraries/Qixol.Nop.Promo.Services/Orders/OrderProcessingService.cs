using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::Nop.Core;
using global::Nop.Core.Domain.Catalog;
using global::Nop.Core.Domain.Common;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Domain.Directory;
using global::Nop.Core.Domain.Localization;
using global::Nop.Core.Domain.Orders;
using global::Nop.Core.Domain.Payments;
using global::Nop.Core.Domain.Shipping;
using global::Nop.Core.Domain.Tax;
using global::Nop.Core.Domain.Vendors;
using global::Nop.Services.Affiliates;
using global::Nop.Services.Catalog;
using global::Nop.Services.Common;
using global::Nop.Services.Customers;
using global::Nop.Services.Directory;
using global::Nop.Services.Discounts;
using global::Nop.Services.Events;
using global::Nop.Services.Localization;
using global::Nop.Services.Logging;
using global::Nop.Services.Messages;
using global::Nop.Services.Orders;
using global::Nop.Services.Payments;
using global::Nop.Services.Security;
using global::Nop.Services.Shipping;
using global::Nop.Services.Tax;
using global::Nop.Services.Vendors;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.Orders;

namespace Qixol.Nop.Promo.Services.Orders
{
    public partial class OrderProcessingService : global::Nop.Services.Orders.OrderProcessingService, IOrderProcessingService
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IGiftCardService _giftCardService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService;
        private readonly ITaxService _taxService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorService _vendorService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICurrencyService _currencyService;
        private readonly IAffiliateService _affiliateService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPdfService _pdfService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICountryService _countryService;

        private readonly ShippingSettings _shippingSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CurrencySettings _currencySettings;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoUtilities _promoUtilities;

        private readonly IPromoOrderService _promoOrderService;
        private readonly IPromoService _promoService;

        private readonly IStoreContext _storeContext;


        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderService">Order service</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="productService">Product service</param>
        /// <param name="paymentService">Payment service</param>
        /// <param name="logger">Logger</param>
        /// <param name="orderTotalCalculationService">Order total calculationservice</param>
        /// <param name="priceCalculationService">Price calculation service</param>
        /// <param name="priceFormatter">Price formatter</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <param name="productAttributeFormatter">Product attribute formatter</param>
        /// <param name="giftCardService">Gift card service</param>
        /// <param name="shoppingCartService">Shopping cart service</param>
        /// <param name="checkoutAttributeFormatter">Checkout attribute service</param>
        /// <param name="shippingService">Shipping service</param>
        /// <param name="shipmentService">Shipment service</param>
        /// <param name="taxService">Tax service</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="discountService">Discount service</param>
        /// <param name="encryptionService">Encryption service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="vendorService">Vendor service</param>
        /// <param name="customerActivityService">Customer activity service</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="affiliateService">Affiliate service</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="pdfService">PDF service</param>
        /// <param name="rewardPointService">Reward point service</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="countryService">Country service</param>
        /// <param name="paymentSettings">Payment settings</param>
        /// <param name="shippingSettings">Shipping settings</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="orderSettings">Order settings</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="currencySettings">Currency settings</param>
        public OrderProcessingService(IOrderService orderService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IProductService productService,
            IPaymentService paymentService,
            ILogger logger,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            IGiftCardService giftCardService,
            IShoppingCartService shoppingCartService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ITaxService taxService,
            ICustomerService customerService,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            ICustomerActivityService customerActivityService,
            ICurrencyService currencyService,
            IAffiliateService affiliateService,
            IEventPublisher eventPublisher,
            IPdfService pdfService,
            IRewardPointService rewardPointService,
            IGenericAttributeService genericAttributeService,
            ICountryService countryService,
            ShippingSettings shippingSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            LocalizationSettings localizationSettings,
            CurrencySettings currencySettings,
            PromoSettings promoSettings,
            IPromoUtilities promoUtilities,
            IPromoOrderService promoOrderService,
            IPromoService promoService,
            IStoreContext storeContext)
            : base(orderService, webHelper, localizationService, languageService, productService, paymentService,
                                                        logger, orderTotalCalculationService, priceCalculationService, priceFormatter, productAttributeParser,
                                                        productAttributeFormatter, giftCardService, shoppingCartService, checkoutAttributeFormatter, shippingService,
                                                        shipmentService, taxService, customerService, discountService, encryptionService, workContext, workflowMessageService,
                                                        vendorService, customerActivityService, currencyService, affiliateService, eventPublisher, pdfService, rewardPointService,
                                                        genericAttributeService, countryService, shippingSettings, paymentSettings, rewardPointsSettings, orderSettings, taxSettings,
                                                        localizationSettings, currencySettings)
        {
            this._orderService = orderService;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._productService = productService;
            this._paymentService = paymentService;
            this._logger = logger;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._giftCardService = giftCardService;
            this._shoppingCartService = shoppingCartService;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._vendorService = vendorService;
            this._shippingService = shippingService;
            this._shipmentService = shipmentService;
            this._taxService = taxService;
            this._customerService = customerService;
            this._discountService = discountService;
            this._encryptionService = encryptionService;
            this._customerActivityService = customerActivityService;
            this._currencyService = currencyService;
            this._affiliateService = affiliateService;
            this._eventPublisher = eventPublisher;
            this._pdfService = pdfService;
            this._rewardPointService = rewardPointService;
            this._genericAttributeService = genericAttributeService;
            this._countryService = countryService;

            this._paymentSettings = paymentSettings;
            this._shippingSettings = shippingSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._orderSettings = orderSettings;
            this._taxSettings = taxSettings;
            this._localizationSettings = localizationSettings;
            this._currencySettings = currencySettings;
            this._promoSettings = promoSettings;
            this._promoUtilities = promoUtilities;
            this._promoOrderService = promoOrderService;
            this._promoService = promoService;
            this._storeContext = storeContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare details to place an order. It also sets some properties to "processPaymentRequest"
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Details</returns>
        protected override PlaceOrderContainter PreparePlaceOrderDetails(ProcessPaymentRequest processPaymentRequest)
        {
            return base.PreparePlaceOrderDetails(processPaymentRequest);
        }
        /// Award reward points
        /// </summary>
        /// <param name="order">Order</param>
        protected override void AwardRewardPoints(Order order)
        {
            if (!_promoSettings.Enabled)
            {
                base.AwardRewardPoints(order);
                return;
            }

            int points = CalculateRewardPoints(order);

            if (points == 0)
                return;

            //Ensure that reward points were not added before. We should not add reward points if they were already earned for this order
            if (order.RewardPointsWereAdded)
                return;

            //add reward points
            _rewardPointService.AddRewardPointsHistoryEntry(order.Customer, points, order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.EarnedForOrder"), order.Id));
            order.RewardPointsWereAdded = true;
            _orderService.UpdateOrder(order);
        }

        /// <summary>
        /// Award reward points
        /// </summary>
        /// <param name="order">Order</param>
        protected override void ReduceRewardPoints(Order order)
        {
            if (!_promoSettings.Enabled)
            {
                base.ReduceRewardPoints(order);
                return;
            }

            int points = CalculateRewardPoints(order);
            if (points == 0)
                return;

            //ensure that reward points were already earned for this order before
            if (!order.RewardPointsWereAdded)
                return;

            //reduce reward points
            _rewardPointService.AddRewardPointsHistoryEntry(order.Customer, -points, order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.ReducedForOrder"), order.Id));
            _orderService.UpdateOrder(order);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        public override PlaceOrderResult PlaceOrder(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentNullException("processPaymentRequest");

            var result = new PlaceOrderResult();
            try
            {
                if (processPaymentRequest.OrderGuid == Guid.Empty)
                    processPaymentRequest.OrderGuid = Guid.NewGuid();

                //prepare order details
                var details = PreparePlaceOrderDetails(processPaymentRequest);

                #region Payment workflow


                //process payment
                ProcessPaymentResult processPaymentResult = null;
                //skip payment workflow if order total equals zero
                var skipPaymentWorkflow = details.OrderTotal == decimal.Zero;
                if (!skipPaymentWorkflow)
                {
                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                    if (paymentMethod == null)
                        throw new NopException("Payment method couldn't be loaded");

                    //ensure that payment method is active
                    if (!paymentMethod.IsPaymentMethodActive(_paymentSettings))
                        throw new NopException("Payment method is not active");

                    if (details.IsRecurringShoppingCart)
                    {
                        //recurring cart
                        switch (_paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName))
                        {
                            case RecurringPaymentType.NotSupported:
                                throw new NopException("Recurring payments are not supported by selected payment method");
                            case RecurringPaymentType.Manual:
                            case RecurringPaymentType.Automatic:
                                processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                                break;
                            default:
                                throw new NopException("Not supported recurring payment type");
                        }
                    }
                    else
                        //standard cart
                        processPaymentResult = _paymentService.ProcessPayment(processPaymentRequest);
                }
                else
                    //payment is not required
                    processPaymentResult = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Paid };

                if (processPaymentResult == null)
                    throw new NopException("processPaymentResult is not available");

                #endregion

                if (processPaymentResult.Success)
                {
                    #region Save order details

                    var order = SaveOrderDetails(processPaymentRequest, processPaymentResult, details);

                    PromoSaveOrderDetails(order);

                    result.PlacedOrder = order;

                    BasketResponse basketResponse = _promoUtilities.GetBasketResponse();

                    //move shopping cart items to order items
                    foreach (var sc in details.Cart)
                    {
                        var basketResponseItems = basketResponse.FindBasketResponseItems(sc);

                        if (basketResponseItems == null)
                        {
                            // TODO: handle this error
                        }
                        else
                        {
                            //prices
                            decimal taxRate;
                            //List <global::Nop.Core.Domain.Discounts.Discount> scDiscounts;
                            decimal discountAmount = basketResponseItems.Sum(i => i.LinePromotionDiscount);
                            var scUnitPrice = _priceCalculationService.GetUnitPrice(sc);
                            var scSubTotal = basketResponseItems.Sum(i => i.LineAmount);
                            var scUnitPriceInclTax = _taxService.GetProductPrice(sc.Product, scUnitPrice, true, details.Customer, out taxRate);
                            var scUnitPriceExclTax = _taxService.GetProductPrice(sc.Product, scUnitPrice, false, details.Customer, out taxRate);
                            var scSubTotalInclTax = _taxService.GetProductPrice(sc.Product, scSubTotal, true, details.Customer, out taxRate);
                            var scSubTotalExclTax = _taxService.GetProductPrice(sc.Product, scSubTotal, false, details.Customer, out taxRate);
                            var discountAmountInclTax = _taxService.GetProductPrice(sc.Product, discountAmount, true, details.Customer, out taxRate);
                            var discountAmountExclTax = _taxService.GetProductPrice(sc.Product, discountAmount, false, details.Customer, out taxRate);
                            throw new NotImplementedException("CHECK - do promotions need to be added to details.AppliedDiscounts at this point?");
                            //foreach (var disc in scDiscounts)
                            //    if (!details.AppliedDiscounts.ContainsDiscount(disc))
                            //        details.AppliedDiscounts.Add(disc);

                            //attributes
                            var attributeDescription = _productAttributeFormatter.FormatAttributes(sc.Product, sc.AttributesXml, details.Customer);

                            var itemWeight = _shippingService.GetShoppingCartItemWeight(sc);

                            //save order item
                            var orderItem = new OrderItem
                            {
                                OrderItemGuid = Guid.NewGuid(),
                                Order = order,
                                ProductId = sc.ProductId,
                                UnitPriceInclTax = scUnitPriceInclTax,
                                UnitPriceExclTax = scUnitPriceExclTax,
                                PriceInclTax = scSubTotalInclTax,
                                PriceExclTax = scSubTotalExclTax,
                                OriginalProductCost = _priceCalculationService.GetProductCost(sc.Product, sc.AttributesXml),
                                AttributeDescription = attributeDescription,
                                AttributesXml = sc.AttributesXml,
                                Quantity = sc.Quantity,
                                DiscountAmountInclTax = discountAmountInclTax,
                                DiscountAmountExclTax = discountAmountExclTax,
                                DownloadCount = 0,
                                IsDownloadActivated = false,
                                LicenseDownloadId = 0,
                                ItemWeight = itemWeight,
                                RentalStartDateUtc = sc.RentalStartDateUtc,
                                RentalEndDateUtc = sc.RentalEndDateUtc
                            };
                            order.OrderItems.Add(orderItem);
                            _orderService.UpdateOrder(order);

                            //gift cards
                            if (sc.Product.IsGiftCard)
                            {
                                string giftCardRecipientName;
                                string giftCardRecipientEmail;
                                string giftCardSenderName;
                                string giftCardSenderEmail;
                                string giftCardMessage;
                                _productAttributeParser.GetGiftCardAttribute(sc.AttributesXml, out giftCardRecipientName,
                                    out giftCardRecipientEmail, out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                                for (var i = 0; i < sc.Quantity; i++)
                                {
                                    _giftCardService.InsertGiftCard(new GiftCard
                                    {
                                        GiftCardType = sc.Product.GiftCardType,
                                        PurchasedWithOrderItem = orderItem,
                                        Amount = sc.Product.OverriddenGiftCardAmount.HasValue ? sc.Product.OverriddenGiftCardAmount.Value : scUnitPriceExclTax,
                                        IsGiftCardActivated = false,
                                        GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                                        RecipientName = giftCardRecipientName,
                                        RecipientEmail = giftCardRecipientEmail,
                                        SenderName = giftCardSenderName,
                                        SenderEmail = giftCardSenderEmail,
                                        Message = giftCardMessage,
                                        IsRecipientNotified = false,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });
                                }
                            }

                            //inventory
                            _productService.AdjustInventory(sc.Product, -sc.Quantity, sc.AttributesXml);
                        }
                    }

                    //clear shopping cart
                    details.Cart.ToList().ForEach(sci => _shoppingCartService.DeleteShoppingCartItem(sci, false));

                    //discount usage history
                    // TODO: replace with Promo discount usage history?

                    //gift card usage history
                    if (details.AppliedGiftCards != null)
                        foreach (var agc in details.AppliedGiftCards)
                        {
                            agc.GiftCard.GiftCardUsageHistory.Add(new GiftCardUsageHistory
                            {
                                GiftCard = agc.GiftCard,
                                UsedWithOrder = order,
                                UsedValue = agc.AmountCanBeUsed,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _giftCardService.UpdateGiftCard(agc.GiftCard);
                        }

                    //recurring orders
                    if (details.IsRecurringShoppingCart)
                    {
                        //create recurring payment (the first payment)
                        var rp = new RecurringPayment
                        {
                            CycleLength = processPaymentRequest.RecurringCycleLength,
                            CyclePeriod = processPaymentRequest.RecurringCyclePeriod,
                            TotalCycles = processPaymentRequest.RecurringTotalCycles,
                            StartDateUtc = DateTime.UtcNow,
                            IsActive = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            InitialOrder = order,
                        };
                        _orderService.InsertRecurringPayment(rp);

                        switch (_paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName))
                        {
                            case RecurringPaymentType.NotSupported:
                                //not supported
                                break;
                            case RecurringPaymentType.Manual:
                                rp.RecurringPaymentHistory.Add(new RecurringPaymentHistory
                                {
                                    RecurringPayment = rp,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    OrderId = order.Id,
                                });
                                _orderService.UpdateRecurringPayment(rp);
                                break;
                            case RecurringPaymentType.Automatic:
                                //will be created later (process is automated)
                                break;
                            default:
                                break;
                        }
                    }

                    #endregion

                    //notifications
                    SendNotificationsAndSaveNotes(order);

                    //reset checkout data
                    _customerService.ResetCheckoutData(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);
                    _customerActivityService.InsertActivity("PublicStore.PlaceOrder", _localizationService.GetResource("ActivityLog.PublicStore.PlaceOrder"), order.Id);

                    //check order status
                    CheckOrderStatus(order);

                    //raise event       
                    _eventPublisher.Publish(new OrderPlacedEvent(order));

                    if (order.PaymentStatus == PaymentStatus.Paid)
                        ProcessOrderPaid(order);
                }
                else
                    foreach (var paymentError in processPaymentResult.Errors)
                        result.AddError(string.Format(_localizationService.GetResource("Checkout.PaymentError"), paymentError));
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
                result.AddError(exc.Message);
            }

            #region Process errors

            if (!result.Success)
            {
                //log errors
                var logError = result.Errors.Aggregate("Error while placing order. ",
                    (current, next) => string.Format("{0}Error {1}: {2}. ", current, result.Errors.IndexOf(next) + 1, next));
                var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
                _logger.Error(logError, customer: customer);
            }

            #endregion

            return result;
        }

        #endregion
    }
}
