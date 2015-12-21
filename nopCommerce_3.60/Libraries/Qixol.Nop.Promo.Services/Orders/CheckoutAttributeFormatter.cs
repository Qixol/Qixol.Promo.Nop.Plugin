using global::Nop.Core;
using global::Nop.Core.Domain.Catalog;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Html;
using global::Nop.Services.Catalog;
using global::Nop.Services.Directory;
using global::Nop.Services.Localization;
using global::Nop.Services.Media;
using global::Nop.Services.Orders;
using global::Nop.Services.Tax;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Services.AttributeValues;

namespace Qixol.Nop.Promo.Services.Orders
{
    public partial class CheckoutAttributeFormatter : global::Nop.Services.Orders.CheckoutAttributeFormatter, ICheckoutAttributeFormatter
    {
        private readonly IWorkContext _workContext;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly ITaxService _taxService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDownloadService _downloadService;
        private readonly IWebHelper _webHelper;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoUtilities _qixolPromoUtilities;
        private readonly IAttributeValueService _attributeValueService;

        public CheckoutAttributeFormatter(IWorkContext workContext,
            ICheckoutAttributeService checkoutAttributeService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            ITaxService taxService,
            IPriceFormatter priceFormatter,
            IDownloadService downloadService,
            IWebHelper webHelper,
            PromoSettings promoSettings,
            IPromoUtilities qixolPromoUtilities,
            IAttributeValueService attributeValueService)
            : base(workContext, checkoutAttributeService, checkoutAttributeParser,
                    currencyService, taxService, priceFormatter,
                    downloadService, webHelper)
        {
            this._workContext = workContext;
            this._checkoutAttributeService = checkoutAttributeService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._currencyService = currencyService;
            this._taxService = taxService;
            this._priceFormatter = priceFormatter;
            this._downloadService = downloadService;
            this._webHelper = webHelper;

            this._promoSettings = promoSettings;
            this._qixolPromoUtilities = qixolPromoUtilities;
            this._attributeValueService = attributeValueService;
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customer">Customer</param>
        /// <param name="separator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperink tags could be rendered (if required)</param>
        /// <returns>Attributes</returns>
        public override string FormatAttributes(string attributesXml,
            Customer customer,
            string separator = "<br />",
            bool htmlEncode = true,
            bool renderPrices = true,
            bool allowHyperlinks = true)
        {

            if (!_promoSettings.Enabled)
                return base.FormatAttributes(attributesXml, customer, separator, htmlEncode, renderPrices, allowHyperlinks);

            BasketResponse basketResponse = _qixolPromoUtilities.GetBasketResponse();

            List<string> attributeStrings = new List<string>();

            var attributes = _checkoutAttributeParser.ParseCheckoutAttributes(attributesXml);
            foreach (var attribute in attributes)
            {
                var valuesStr = _checkoutAttributeParser.ParseValues(attributesXml, attribute.Id);
                foreach (var valueStr in valuesStr)
                {
                    string formattedAttribute = string.Empty;
                    string formattedPromo = string.Empty;
                    if (!attribute.ShouldHaveValues())
                    {
                        //no values
                        if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            //multiline textbox
                            var attributeName = attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                            //encode (if required)
                            if (htmlEncode)
                                attributeName = HttpUtility.HtmlEncode(attributeName);
                            formattedAttribute = string.Format("{0}: {1}", attributeName, HtmlHelper.FormatText(valueStr, false, true, false, false, false, false));
                            //we never encode multiline textbox input
                        }
                        else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                        {
                            //file upload
                            Guid downloadGuid;
                            Guid.TryParse(valueStr, out downloadGuid);
                            var download = _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                                string attributeText = "";
                                var fileName = string.Format("{0}{1}",
                                    download.Filename ?? download.DownloadGuid.ToString(),
                                    download.Extension);
                                //encode (if required)
                                if (htmlEncode)
                                    fileName = HttpUtility.HtmlEncode(fileName);
                                if (allowHyperlinks)
                                {
                                    //hyperlinks are allowed
                                    var downloadLink = string.Format("{0}download/getfileupload/?downloadId={1}", _webHelper.GetStoreLocation(false), download.DownloadGuid);
                                    attributeText = string.Format("<a href=\"{0}\" class=\"fileuploadattribute\">{1}</a>", downloadLink, fileName);
                                }
                                else
                                {
                                    //hyperlinks aren't allowed
                                    attributeText = fileName;
                                }
                                var attributeName = attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = HttpUtility.HtmlEncode(attributeName);
                                formattedAttribute = string.Format("{0}: {1}", attributeName, attributeText);
                            }
                        }
                        else
                        {
                            //other attributes (textbox, datepicker)
                            formattedAttribute = string.Format("{0}: {1}", attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), valueStr);
                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = HttpUtility.HtmlEncode(formattedAttribute);
                        }
                    }
                    else
                    {
                        int attributeValueId;
                        if (int.TryParse(valueStr, out attributeValueId))
                        {
                            var attributeValue = _checkoutAttributeService.GetCheckoutAttributeValueById(attributeValueId);
                            if (attributeValue != null)
                            {
                                formattedAttribute = string.Format("{0}: {1}", attribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), attributeValue.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id));
                                if (renderPrices)
                                {
                                    decimal priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(attributeValue, customer);
                                    decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                                    if (priceAdjustmentBase > 0)
                                    {
                                        string priceAdjustmentStr = _priceFormatter.FormatPrice(priceAdjustment);
                                        formattedAttribute += string.Format(" [+{0}]", priceAdjustmentStr);
                                    }
                                }
                            }

                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = HttpUtility.HtmlEncode(formattedAttribute);

                            #region promos

                            if (basketResponse != null)
                            {
                                var checkoutAttributeItem = basketResponse.CheckoutAttributeItem(attribute);

                                if (checkoutAttributeItem != null)
                                {
                                    var appliedPromo = (from ap in checkoutAttributeItem.AppliedPromotions where !ap.BasketLevelPromotion && !ap.DeliveryLevelPromotion select ap).FirstOrDefault();

                                    if (appliedPromo != null)
                                    {
                                        var summaryPromo = (from sp in basketResponse.Summary.AppliedPromotions where sp.PromotionId == appliedPromo.PromotionId select sp).FirstOrDefault();
                                        if (summaryPromo != null)
                                            formattedPromo += BasketResponseExtensions.GetDisplayPromoDetails(_promoSettings.ShowPromotionDetailsInBasket, summaryPromo);

                                        if (renderPrices)
                                        {
                                            formattedPromo += " [-";
                                            formattedPromo += _priceFormatter.FormatPrice(appliedPromo.DiscountAmount);
                                            formattedPromo += "]";
                                        }
                                    }
                                }
                            }
                            
                            //encode (if required)
                            if (htmlEncode)
                                formattedPromo = HttpUtility.HtmlEncode(formattedPromo);

                            #endregion

                        }
                    }

                    if (!String.IsNullOrEmpty(formattedAttribute))
                    {
                        attributeStrings.Add(formattedAttribute);
                    }
                    if (!String.IsNullOrEmpty(formattedPromo))
                    {
                        attributeStrings.Add(formattedPromo);
                    }
                }
            }

            //return string.Format("<span style=\"float: right; text-align: right; white-space: nowrap;\">{0}</span>", string.Join("<br />", attributeStrings.ToArray()));
            return string.Join("<br />", attributeStrings.ToArray());
        }
    }
}
