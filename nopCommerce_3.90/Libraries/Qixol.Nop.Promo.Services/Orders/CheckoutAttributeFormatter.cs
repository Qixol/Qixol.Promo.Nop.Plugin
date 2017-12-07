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
using Nop.Services.Common;

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
        private readonly IStoreContext _storeContext;

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
            IAttributeValueService attributeValueService,
            IStoreContext storeContext)
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
            this._storeContext = storeContext;
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

            BasketResponse basketResponse = customer.GetAttribute<BasketResponse>(PromoCustomerAttributeNames.PromoBasketResponse, _storeContext.CurrentStore.Id);
            if (basketResponse == null || !basketResponse.IsValid())
                return string.Empty;

            List<string> attributeStrings = new List<string>();

            var attributes = _checkoutAttributeParser.ParseCheckoutAttributes(attributesXml);
            foreach (var attribute in attributes)
            {
                var valuesStr = _checkoutAttributeParser.ParseValues(attributesXml, attribute.Id);
                foreach (var valueStr in valuesStr)
                {
                    string formattedAttribute = string.Empty;
                    var formattedPromos = new List<string>();
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
                                    var appliedPromos = (from ap in checkoutAttributeItem.AppliedPromotions where !ap.BasketLevelPromotion && !ap.DeliveryLevelPromotion select ap).ToList();

                                    if (appliedPromos != null && appliedPromos.Any())
                                    {
                                        appliedPromos.ForEach(appliedPromo =>
                                        {
                                            var formattedPromo = new StringBuilder();
                                            //formattedPromo.Append("<div class=\"truncate-275\" style=\"display: table-cell\">");

                                            if (htmlEncode)
                                                formattedPromo.Append(HttpUtility.HtmlEncode(appliedPromo.DisplayDetails(customer)));
                                            else
                                                formattedPromo.Append(appliedPromo.DisplayDetails(customer));

                                            if (renderPrices)
                                            {
                                                //formattedPromo.Append("</div><div style=\"display: table-cell\">");
                                                if (htmlEncode)
                                                {
                                                    formattedPromo.Append(HttpUtility.HtmlEncode(" [-"));
                                                    formattedPromo.Append(HttpUtility.HtmlEncode(_priceFormatter.FormatPrice(appliedPromo.DiscountAmount)));
                                                    formattedPromo.Append(HttpUtility.HtmlEncode("]"));
                                                }
                                                else
                                                {
                                                    formattedPromo.Append(" [-");
                                                    formattedPromo.Append(_priceFormatter.FormatPrice(appliedPromo.DiscountAmount));
                                                    formattedPromo.Append("]");
                                                }
                                            }

                                            //formattedPromo.Append("</div>");

                                            formattedPromos.Add(formattedPromo.ToString());

                                        });
                                    }
                                }
                            }
                            
                            //encode (if required)

                            #endregion

                        }
                    }

                    if (!String.IsNullOrEmpty(formattedAttribute))
                    {
                        attributeStrings.Add(formattedAttribute);
                    }
                    if (formattedPromos.Any())
                    {
                        formattedPromos.ForEach(formattedPromo =>
                        {
                            attributeStrings.Add(formattedPromo);
                        });
                    }
                }
            }


            var returnString = new StringBuilder();
            //returnString.Append("<div style=\"display: table\">");
            //returnString.Append("<div style=\"display: table-row\">");
            //returnString.Append(string.Join("</div><div style=\"display: table-row\">", attributeStrings.ToArray()));
            //returnString.Append("</div>");

            //return returnString.ToString();

            return string.Join("<br />", attributeStrings.ToArray());
        }
    }
}
