var MissedPromotions = {
    continueShoppingUrl: '',
    showMissedPromotionsStep: true,

    init: function (continueShoppingUrl, showMissedPromotionsStep) {
        this.continueShoppingUrl = continueShoppingUrl,
        this.showMissedPromotionsStep = showMissedPromotionsStep
    },

    continueShopping: function() {
        $.ajax({
            cache: false,
            url: this.continueShoppingUrl,
            type: 'post',
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: Checkout.ajaxFailure
        });
    },

    success_process: function (response) {
        window.location = response.continue_shopping_url;
    },

    next: function () {
        if (Checkout.loadWaiting != false) return;

        Checkout.setLoadWaiting('missed-promotions');
        this.nextStep();
        Checkout.setLoadWaiting(false);
    },

    resetLoadWaiting: function () {
        Checkout.setLoadWaiting(false);
    },

    nextStep: function () {
        var response = { goto_section: 'billing' };
        Checkout.setStepResponse(response);
    }
};
