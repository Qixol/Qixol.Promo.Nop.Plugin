var PromoOrderDetailsWidget = {

    init: function (data, youWillReceiveMessage, youHaveReceivedMessage) {
        try {
            if (data) {
                var model = data;
                this.IssuedCoupons.init(model.IssuedCouponsModel, youWillReceiveMessage, youHaveReceivedMessage);
                this.BasketDiscountsExcShipping.init(model.BasketLevelDiscountsExcShippingModel);
                this.ShippingMethods.init(model.ShippingModel);
                this.OrderTotal.init(model.OrderTotal);
                this.IssuedPoints.init(model.IssuedPoints);
                this.BasketTotalDiscount.init(model.BasketTotalDiscount);
            }
        }
        catch (e) {
            console.log(e);
        }
    },

    render: function () {
        this.IssuedCoupons.render();
        this.BasketDiscountsExcShipping.render();
        this.ShippingMethods.render();
        //this.OrderTotal.render();
        //this.IssuedPoints.render();
        this.BasketTotalDiscount.render();
    },

    IssuedCoupons: {

        data: null,
        youWillReceiveMessage: '',
        youHaveReceivedMessage: '',

        init: function (data, youWillReceiveMessage, youHaveReceivedMessage) {
            this.data = data;
            this.youWillReceiveMessage = youWillReceiveMessage;
            this.youHaveReceivedMessage = youHaveReceivedMessage;
        },

        render: function () {
            try {
                if (this.data.length) {
                    var issuedCoupons = this.data;
                    for (var i in issuedCoupons) {
                        var lastRowElement = $('.section.products tbody tr:last');
                        lastRowElement.after($('script[id="issued-coupons-template"]').html());
                        var newRowElement = $('.section.products tbody tr:last');
                        var newHtml = '';
                        lastRowElement.children('td').each(function () {
                            newHtml += '<td class="' + $(this).attr('class') + '"></td>';
                        });
                        newRowElement.html(newHtml);
                        var messageHtml = '<div>';
                        if (issuedCoupons[i].IsConfirmed) {
                            messageHtml += this.youHaveReceivedMessage;
                            messageHtml += ' ';
                            messageHtml += issuedCoupons[i].Code;
                        } else {
                            messageHtml += this.youWillReceiveMessage;
                        }
                        messageHtml += '</div>';
                        var couponHtml = '<div class="promotion-item">' + issuedCoupons[i].DisplayText + '</div>';
                        $('.cart-issued-coupons:last td.product').html(messageHtml + couponHtml);
                    }
                    $('.cart-issued-coupons').show();
                }
            }
            catch (e) { }
        }
    },

    // CheckoutAttributes are handled by the overridden formatter

    SubTotal: {

        data: null,

        init: function (data) {
            this.data = data;
        },

        render: function () {
            if (this.data.length) {
                var subTotal = this.data;
                $('.order-subtotal .cart-total-right .value-summary').html(subTotal);
            }
        }
    },

    BasketDiscountsExcShipping: {

        data: null,

        init: function (data) {
            this.data = data;
        },

        render: function () {
            if (this.data) {
                var discountRow = $('label:contains("Discount:")').closest('tr');
                discountRow.after('<tr class="order-subtotal-discounts-exc-placeholder"></tr>');
                discountRow.html('');
                var basketLevelDiscountsExcShipping = this.data;
                for (var i in basketLevelDiscountsExcShipping) {
                    var promotionId = basketLevelDiscountsExcShipping[i].PromotionId;
                    var html = $('script[id="sub-total-template"]').html().replace('#=promotionId#', promotionId);
                    $('.order-subtotal-discounts-exc-placeholder').before(html);
                    var discountNameElement = $('.order-subtotal-discount[data-promotion-id="' + promotionId + '"] .sub-total-discount-name');
                    discountNameElement.html(basketLevelDiscountsExcShipping[i].PromotionName);
                    var discountAmountElement = $('.order-subtotal-discount[data-promotion-id="' + promotionId + '"] .sub-total-discount-amount');
                    discountAmountElement.html(basketLevelDiscountsExcShipping[i].DiscountAmount);
                }
            }
        }
    },

    ShippingMethods: {

        data: null,

        init: function (data) {
            this.data = data;
        },

        render: function () {
            if (this.data) {
                var shippingRow = $('label:contains("Shipping:")').closest('tr');
                var originalShippingAmount = this.data.OriginalShippingAmount;
                shippingRow.children('.cart-total-right').children('span').html(originalShippingAmount);

                shippingRow.after('<tr class="shipping-discounts-placeholder"></tr>');
                if (this.data.ShippingPromotions) {
                    var shippingPromotions = this.data.ShippingPromotions;

                    for (var i in shippingPromotions) {
                        var promotionId = shippingPromotions[i].PromotionId;
                        var html = $('script[id="shipping-template"]').html().replace('#=promotionId#', promotionId);
                        $('.shipping-discounts-placeholder').before(html);
                        var discountNameElement = $('.shipping-discount[data-promotion-id="' + promotionId + '"] .sub-total-discount-name');
                        discountNameElement.html(shippingPromotions[i].PromotionName);
                        var discountAmountElement = $('.shipping-discount[data-promotion-id="' + promotionId + '"] .sub-total-discount-amount');
                        discountAmountElement.html(shippingPromotions[i].DiscountAmount);
                    }
                }
            }
        }
    },

    BasketDiscountsIncShipping: {

        data: null,

        init: function (data) {
            this.data = data;
        },

        render: function () {
            if (this.data) {
                $('.order-subtotal-discount').html('');
                var basketLevelDiscountsIncShipping = this.data;
                $('.tax-value').before('<tr class="order-subtotal-discounts-inc-placeholder"></tr>'); // TODO: tax-value not shown => before order-total
                for (var i in basketLevelDiscountsIncShipping) {
                    var promotionId = basketLevelDiscountsIncShipping[i].PromotionId;
                    var html = $('script[id="sub-total-template"]').html().replace('#=promotionId#', promotionId);
                    $('.order-subtotal-discounts-Inc-placeholder').before(html);
                    var discountNameElement = $('.order-subtotal-discount[data-promotion-id="' + promotionId + '"] .sub-total-discount-name');
                    discountNameElement.html(basketLevelDiscountsIncShipping[i].PromotionName);
                    var discountAmountElement = $('.order-subtotal-discount[data-promotion-id="' + promotionId + '"] .sub-total-discount-amount');
                    discountAmountElement.html(basketLevelDiscountsIncShipping[i].DiscountAmount);
                }
            }
        }
    },

    OrderTotal: {

        data: null,

        init: function (data) {
            this.data = data;
        },

        render: function () {
            if (this.data.length) {
                var orderTotal = this.data;
                $('.order-total .value-summary').html('<strong>' + orderTotal + '</strong>');
            }
        }
    },

    IssuedPoints: {

        data: null,

        init: function (data) {
            this.data = data;
        },

        render: function () {
            if (this.data && this.data.length) {
                var issuedPoints = this.data;
                var rewardPointsElement = $('.earn-reward-points .value-summary');
                if (!rewardPointsElement) {
                    var rewardPointsHtml = $('script[id="issued-points-template"]').html();
                    $('.order-total').after(rewardPointsHtml);
                    rewardPointsElement = $('.earn-reward-points .value-summary');
                }
                rewardPointsElement.html(issuedPoints);
            } else {
                var rewardPointsRow = $('.earn-reward-points');
                if (rewardPointsRow) {
                    rewardPointsRow.html('');
                }
            }
        }
    },

    BasketTotalDiscount: {

        data: null,

        init: function (data) {
            this.data = data;
        },

        render: function () {
            if (this.data && this.data.length) {
                var basketTotalDiscount = this.data;
                var basketTotalDiscountHtml = $('script[id="basket-total-discount"]').html().replace('#=BasketTotalDiscount#', basketTotalDiscount);
                $('.cart-total tbody').children('tr').last().after(basketTotalDiscountHtml);
            }
        }
    }
};
