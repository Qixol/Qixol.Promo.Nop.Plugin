$(document).ready(function () {
    /* patch fixes for underlying nop shopping cart */
    /* ensure that pressing enter in the coupon text entry box presses the "Apply Coupon" submit button */
    $("input[name=discountcouponcode]").keyup(function (event) {
        if (event.keyCode == 13) {
            $("input[name=applydiscountcouponcode]").click();
        }
    });
    /* ensure that pressing enter in the gift card entry box presses the "Add Gift Card" submit button */
    $("input[name=giftcardcouponcode]").keyup(function (event) {
        if (event.keyCode == 13) {
            $("input[name=applygiftcardcouponcode]").click();
        }
    });
    /* ensure that pressing enter in the Zip/postal code entry box presses the "Estimate Shipping" submit button */
    $("#ZipPostalCode").keyup(function (event) {
        if (event.keyCode == 13) {
            $("input[name=estimateshipping]").click();
        }
    });
    /* #endregion patch fixes for underlying nop shopping cart */
});