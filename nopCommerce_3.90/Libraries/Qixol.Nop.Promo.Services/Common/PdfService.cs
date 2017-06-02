using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using global::Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Qixol.Nop.Promo.Core.Domain.Orders;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.Orders;
using Qixol.Nop.Promo.Services.Promo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qixol.Nop.Promo.Core.Html;

namespace Qixol.Nop.Promo.Services.Common
{
    public partial class PdfService : global::Nop.Services.Common.PdfService, global::Nop.Services.Common.IPdfService
    {

        #region fields

        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IMeasureService _measureService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingContext;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly AddressSettings _addressSettings;

        private readonly PromoSettings _promoSettings;
        private readonly IPromoOrderService _promoOrderService;

        #endregion

        #region ctor

        public PdfService(ILocalizationService localizationService,
            ILanguageService languageService,
            IWorkContext workContext,
            IOrderService orderService,
            IPaymentService paymentService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IMeasureService measureService,
            IPictureService pictureService,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            IStoreService storeService,
            IStoreContext storeContext,
            ISettingService settingContext,
            IAddressAttributeFormatter addressAttributeFormatter,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            MeasureSettings measureSettings,
            PdfSettings pdfSettings,
            TaxSettings taxSettings,
            AddressSettings addressSettings,
            PromoSettings promoSettings,
            IPromoOrderService promoOrderService)
            : base(localizationService, languageService, workContext, orderService, paymentService, dateTimeHelper, priceFormatter,
                                                    currencyService, measureService, pictureService, productService, productAttributeParser, storeService, storeContext,
                                                    settingContext, addressAttributeFormatter, catalogSettings, currencySettings, measureSettings,
                                                    pdfSettings, taxSettings, addressSettings)
        {
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._workContext = workContext;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._measureService = measureService;
            this._pictureService = pictureService;
            this._productService = productService;
            this._productAttributeParser = productAttributeParser;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._settingContext = settingContext;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._currencySettings = currencySettings;
            this._catalogSettings = catalogSettings;
            this._measureSettings = measureSettings;
            this._pdfSettings = pdfSettings;
            this._taxSettings = taxSettings;
            this._addressSettings = addressSettings;
            this._promoSettings = promoSettings;
            this._promoOrderService = promoOrderService;
        }

        #endregion

        public override void PrintOrdersToPdf(global::System.IO.Stream stream, IList<global::Nop.Core.Domain.Orders.Order> orders, int languageId = 0, int vendorId = 0)
        {
            if (!_promoSettings.Enabled)
            {
                base.PrintOrdersToPdf(stream, orders, languageId, vendorId);
                return;
            }

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (orders == null)
                throw new ArgumentNullException("orders");

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.LETTER;
            }


            var doc = new Document(pageSize);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.BLACK;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var promosFont = GetFont();
            promosFont.Color = BaseColor.BLUE;
            promosFont.SetStyle(Font.ITALIC);

            int ordCount = orders.Count;
            int ordNum = 0;

            foreach (var order in orders)
            {
                //by default _pdfSettings contains settings for the current active store
                //and we need PdfSettings for the store which was used to place an order
                //so let's load it based on a store of the current order
                var pdfSettingsByStore = _settingContext.LoadSetting<PdfSettings>(order.StoreId);

                var lang = _languageService.GetLanguageById(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = _workContext.WorkingLanguage;

                #region Header

                //logo
                var logoPicture = _pictureService.GetPictureById(pdfSettingsByStore.LogoPictureId);
                var logoExists = logoPicture != null;

                //header
                var headerTable = new PdfPTable(logoExists ? 2 : 1);
                headerTable.RunDirection = GetDirection(lang);
                headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

                //store info
                var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;
                var anchor = new Anchor(store.Url.Trim(new[] { '/' }), font);
                anchor.Reference = store.Url;

                var cellHeader = new PdfPCell(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.Order#", lang.Id), order.Id), titleFont));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase(anchor));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase(String.Format(_localizationService.GetResource("PDFInvoice.OrderDate", lang.Id), _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc).ToString("D", new CultureInfo(lang.LanguageCulture))), font));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
                cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
                cellHeader.Border = Rectangle.NO_BORDER;

                headerTable.AddCell(cellHeader);

                if (logoExists)
                    if (lang.Rtl)
                        headerTable.SetWidths(new[] { 0.2f, 0.8f });
                    else
                        headerTable.SetWidths(new[] { 0.8f, 0.2f });
                headerTable.WidthPercentage = 100f;

                //logo               
                if (logoExists)
                {
                    var logoFilePath = _pictureService.GetThumbLocalPath(logoPicture, 0, false);
                    var logo = Image.GetInstance(logoFilePath);
                    logo.Alignment = GetAlignment(lang, true);
                    logo.ScaleToFit(65f, 65f);

                    var cellLogo = new PdfPCell();
                    cellLogo.Border = Rectangle.NO_BORDER;
                    cellLogo.AddElement(logo);
                    headerTable.AddCell(cellLogo);
                }
                doc.Add(headerTable);

                #endregion

                #region Addresses

                var addressTable = new PdfPTable(2);
                addressTable.RunDirection = GetDirection(lang);
                addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
                addressTable.WidthPercentage = 100f;
                addressTable.SetWidths(new[] { 50, 50 });

                //billing info
                var billingAddress = new PdfPTable(1);
                billingAddress.DefaultCell.Border = Rectangle.NO_BORDER;
                billingAddress.RunDirection = GetDirection(lang);

                billingAddress.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.BillingInformation", lang.Id), titleFont));

                if (_addressSettings.CompanyEnabled && !String.IsNullOrEmpty(order.BillingAddress.Company))
                    billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Company", lang.Id), order.BillingAddress.Company), font));

                billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Name", lang.Id), order.BillingAddress.FirstName + " " + order.BillingAddress.LastName), font));
                if (_addressSettings.PhoneEnabled)
                    billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Phone", lang.Id), order.BillingAddress.PhoneNumber), font));
                if (_addressSettings.FaxEnabled && !String.IsNullOrEmpty(order.BillingAddress.FaxNumber))
                    billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Fax", lang.Id), order.BillingAddress.FaxNumber), font));
                if (_addressSettings.StreetAddressEnabled)
                    billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Address", lang.Id), order.BillingAddress.Address1), font));
                if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(order.BillingAddress.Address2))
                    billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Address2", lang.Id), order.BillingAddress.Address2), font));
                if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled || _addressSettings.ZipPostalCodeEnabled)
                    billingAddress.AddCell(new Paragraph("   " + String.Format("{0}, {1} {2}", order.BillingAddress.City, order.BillingAddress.StateProvince != null ? order.BillingAddress.StateProvince.GetLocalized(x => x.Name, lang.Id) : "", order.BillingAddress.ZipPostalCode), font));
                if (_addressSettings.CountryEnabled && order.BillingAddress.Country != null)
                    billingAddress.AddCell(new Paragraph("   " + String.Format("{0}", order.BillingAddress.Country != null ? order.BillingAddress.Country.GetLocalized(x => x.Name, lang.Id) : ""), font));

                //VAT number
                if (!String.IsNullOrEmpty(order.VatNumber))
                    billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.VATNumber", lang.Id), order.VatNumber), font));

                //custom attributes
                var customBillingAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes);
                if (!String.IsNullOrEmpty(customBillingAddressAttributes))
                {
                    //TODO: we should add padding to each line (in case if we have sevaral custom address attributes)
                    billingAddress.AddCell(new Paragraph("   " + HtmlHelper.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true), font));
                }



                //vendors payment details
                if (vendorId == 0)
                {
                    //payment method
                    var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
                    string paymentMethodStr = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, lang.Id) : order.PaymentMethodSystemName;
                    if (!String.IsNullOrEmpty(paymentMethodStr))
                    {
                        billingAddress.AddCell(new Paragraph(" "));
                        billingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.PaymentMethod", lang.Id), paymentMethodStr), font));
                        billingAddress.AddCell(new Paragraph());
                    }

                    //custom values
                    var customValues = order.DeserializeCustomValues();
                    if (customValues != null)
                    {
                        foreach (var item in customValues)
                        {
                            billingAddress.AddCell(new Paragraph(" "));
                            billingAddress.AddCell(new Paragraph("   " + item.Key + ": " + item.Value, font));
                            billingAddress.AddCell(new Paragraph());
                        }
                    }
                }
                addressTable.AddCell(billingAddress);

                //shipping info
                var shippingAddress = new PdfPTable(1);
                shippingAddress.DefaultCell.Border = Rectangle.NO_BORDER;
                shippingAddress.RunDirection = GetDirection(lang);

                if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
                {
                    //cell = new PdfPCell();
                    //cell.Border = Rectangle.NO_BORDER;

                    if (!order.PickUpInStore)
                    {
                        if (order.ShippingAddress == null)
                            throw new NopException(string.Format("Shipping is required, but address is not available. Order ID = {0}", order.Id));

                        shippingAddress.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.ShippingInformation", lang.Id), titleFont));
                        if (!String.IsNullOrEmpty(order.ShippingAddress.Company))
                            shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Company", lang.Id), order.ShippingAddress.Company), font));
                        shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Name", lang.Id), order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName), font));
                        if (_addressSettings.PhoneEnabled)
                            shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Phone", lang.Id), order.ShippingAddress.PhoneNumber), font));
                        if (_addressSettings.FaxEnabled && !String.IsNullOrEmpty(order.ShippingAddress.FaxNumber))
                            shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Fax", lang.Id), order.ShippingAddress.FaxNumber), font));
                        if (_addressSettings.StreetAddressEnabled)
                            shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Address", lang.Id), order.ShippingAddress.Address1), font));
                        if (_addressSettings.StreetAddress2Enabled && !String.IsNullOrEmpty(order.ShippingAddress.Address2))
                            shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.Address2", lang.Id), order.ShippingAddress.Address2), font));
                        if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled || _addressSettings.ZipPostalCodeEnabled)
                            shippingAddress.AddCell(new Paragraph("   " + String.Format("{0}, {1} {2}", order.ShippingAddress.City, order.ShippingAddress.StateProvince != null ? order.ShippingAddress.StateProvince.GetLocalized(x => x.Name, lang.Id) : "", order.ShippingAddress.ZipPostalCode), font));
                        if (_addressSettings.CountryEnabled && order.ShippingAddress.Country != null)
                            shippingAddress.AddCell(new Paragraph("   " + String.Format("{0}", order.ShippingAddress.Country != null ? order.ShippingAddress.Country.GetLocalized(x => x.Name, lang.Id) : ""), font));
                        //custom attributes
                        var customShippingAddressAttributes = _addressAttributeFormatter.FormatAttributes(order.ShippingAddress.CustomAttributes);
                        if (!String.IsNullOrEmpty(customShippingAddressAttributes))
                        {
                            //TODO: we should add padding to each line (in case if we have sevaral custom address attributes)
                            shippingAddress.AddCell(new Paragraph("   " + HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                        }
                        shippingAddress.AddCell(new Paragraph(" "));
                    }
                    else
                        if (order.PickupAddress != null)
                    {
                        shippingAddress.AddCell(new Paragraph(_localizationService.GetResource("PDFInvoice.Pickup", lang.Id), titleFont));
                        if (!string.IsNullOrEmpty(order.PickupAddress.Address1))
                            shippingAddress.AddCell(new Paragraph(string.Format("   {0}", string.Format(_localizationService.GetResource("PDFInvoice.Address", lang.Id), order.PickupAddress.Address1)), font));
                        if (!string.IsNullOrEmpty(order.PickupAddress.City))
                            shippingAddress.AddCell(new Paragraph(string.Format("   {0}", order.PickupAddress.City), font));
                        if (order.PickupAddress.Country != null)
                            shippingAddress.AddCell(new Paragraph(string.Format("   {0}", order.PickupAddress.Country.GetLocalized(x => x.Name, lang.Id)), font));
                        if (!string.IsNullOrEmpty(order.PickupAddress.ZipPostalCode))
                            shippingAddress.AddCell(new Paragraph(string.Format("   {0}", order.PickupAddress.ZipPostalCode), font));
                        shippingAddress.AddCell(new Paragraph(" "));
                    }
                    shippingAddress.AddCell(new Paragraph("   " + String.Format(_localizationService.GetResource("PDFInvoice.ShippingMethod", lang.Id), order.ShippingMethod), font));
                    shippingAddress.AddCell(new Paragraph());

                    addressTable.AddCell(shippingAddress);
                }
                else
                {
                    shippingAddress.AddCell(new Paragraph());
                    addressTable.AddCell(shippingAddress);
                }

                doc.Add(addressTable);
                doc.Add(new Paragraph(" "));

                #endregion

                #region Products

                //products
                var productsHeader = new PdfPTable(1);
                productsHeader.RunDirection = GetDirection(lang);
                productsHeader.WidthPercentage = 100f;
                var cellProducts = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.Product(s)", lang.Id), titleFont));
                cellProducts.Border = Rectangle.NO_BORDER;
                productsHeader.AddCell(cellProducts);
                doc.Add(productsHeader);
                doc.Add(new Paragraph(" "));


                var orderItems = order.OrderItems;

                var productsTable = new PdfPTable(_catalogSettings.ShowSkuOnProductDetailsPage ? 5 : 4);
                productsTable.RunDirection = GetDirection(lang);
                productsTable.WidthPercentage = 100f;
                if (lang.Rtl)
                {
                    productsTable.SetWidths(_catalogSettings.ShowSkuOnProductDetailsPage
                        ? new[] { 15, 10, 15, 15, 45 }
                        : new[] { 20, 10, 20, 50 });
                }
                else
                {
                    productsTable.SetWidths(_catalogSettings.ShowSkuOnProductDetailsPage
                        ? new[] { 45, 15, 15, 10, 15 }
                        : new[] { 50, 20, 10, 20 });
                }

                //product name
                var cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductName", lang.Id), font));
                cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.SKU", lang.Id), font));
                    cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //price
                cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductPrice", lang.Id), font));
                cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //qty
                cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductQuantity", lang.Id), font));
                cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                //total
                cellProductItem = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.ProductTotal", lang.Id), font));
                cellProductItem.BackgroundColor = BaseColor.LIGHT_GRAY;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);

                foreach (var orderItem in orderItems)
                {
                    var p = orderItem.Product;

                    // promo - get details
                    var linePromotions = orderItem.Promotions().ToList();
                    var linePromotionNames = new List<string>();
                    var linePromotionAmounts = new List<string>();
                    if (linePromotions != null && linePromotions.Any())
                    {
                        linePromotions.ForEach(lp =>
                        {
                            linePromotionNames.Add(lp.DisplayDetails());
                            var lineDiscountAmountInclTaxiInCustomerCurrency = _currencyService.ConvertCurrency(lp.DiscountAmount, order.CurrencyRate);
                            var youSaveText = string.Format(_localizationService.GetResource("ShoppingCart.ItemYouSave"), _priceFormatter.FormatPrice(lineDiscountAmountInclTaxiInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true));
                            linePromotionAmounts.Add(youSaveText);
                        });
                    }

                    //a vendor should have access only to his products
                    if (vendorId > 0 && p.VendorId != vendorId)
                        continue;

                    var pAttribTable = new PdfPTable(1);
                    pAttribTable.RunDirection = GetDirection(lang);
                    pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    //product name
                    string name = p.GetLocalized(x => x.Name, lang.Id);
                    pAttribTable.AddCell(new Paragraph(name, font));
                    cellProductItem.AddElement(new Paragraph(name, font));
                    //attributes
                    if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        pAttribTable.AddCell(attributesParagraph);
                    }
                    //rental info
                    if (orderItem.Product.IsRental)
                    {
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalStartDateUtc.Value) : "";
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue ? orderItem.Product.FormatRentalDate(orderItem.RentalEndDateUtc.Value) : "";
                        var rentalInfo = string.Format(_localizationService.GetResource("Order.Rental.FormattedDate"),
                            rentalStartDate, rentalEndDate);

                        var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                        pAttribTable.AddCell(rentalInfoParagraph);
                    }

                    // promo - line level promotions
                    if (linePromotionNames.Any())
                    {
                        var allPromoNames = string.Join(Environment.NewLine, linePromotionNames.ToArray());
                        var promoInfo = new Paragraph(allPromoNames, promosFont);
                        pAttribTable.AddCell(promoInfo);
                    }
                    productsTable.AddCell(pAttribTable);

                    //SKU
                    if (_catalogSettings.ShowSkuOnProductDetailsPage)
                    {
                        var sku = p.FormatSku(orderItem.AttributesXml, _productAttributeParser);
                        cellProductItem = new PdfPCell(new Phrase(sku ?? String.Empty, font));
                        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                        productsTable.AddCell(cellProductItem);
                    }

                    //price
                    string unitPrice;
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        unitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        unitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);
                    }
                    cellProductItem = new PdfPCell(new Phrase(unitPrice, font));
                    cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    productsTable.AddCell(cellProductItem);

                    //qty
                    cellProductItem = new PdfPCell(new Phrase(orderItem.Quantity.ToString(), font));
                    cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    productsTable.AddCell(cellProductItem);

                    //total
                    string subTotal;
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                        subTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);
                    }
                    else
                    {
                        //excluding tax
                        var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                        subTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);
                    }

                    #region Promo - discount amounts

                    var ptotalTable = new PdfPTable(1);
                    ptotalTable.RunDirection = GetDirection(lang);
                    ptotalTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    ptotalTable.AddCell(new Paragraph(subTotal, font));

                    if (linePromotionAmounts.Any())
                    {
                        linePromotionAmounts.ForEach(lpa =>
                        {
                            ptotalTable.AddCell(new Paragraph(lpa, promosFont));
                        });
                    }

                    productsTable.AddCell(ptotalTable);

                    #endregion
                }

                #endregion

                #region issued coupons

                var promoOrderIssuedCoupons = order.PromoIssuedCoupons();
                promoOrderIssuedCoupons.ToList().ForEach(poic =>
                {
                    var issuedCouponTable = new PdfPTable(1);
                    issuedCouponTable.RunDirection = GetDirection(lang);
                    issuedCouponTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    // coupon details
                    issuedCouponTable.AddCell(new Paragraph(_localizationService.GetResource("Plugin.Misc.QixolPromo.Coupon.YouReceived"), font));
                    issuedCouponTable.AddCell(new Paragraph(poic.DisplayText, promosFont));
                    string code = string.Format("{0}: {1}", _localizationService.GetResource("Plugin.Misc.QixolPromo.Coupon.Code"), poic.CouponCode);
                    issuedCouponTable.AddCell(new Paragraph(code, font));

                    productsTable.AddCell(issuedCouponTable);

                    //SKU - empty cell
                    if (_catalogSettings.ShowSkuOnProductDetailsPage)
                    {
                        var sku = string.Empty;
                        cellProductItem = new PdfPCell(new Phrase(sku ?? String.Empty, font));
                        cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                        productsTable.AddCell(cellProductItem);
                    }

                    //price - empty cell
                    string unitPrice = string.Empty;
                    cellProductItem = new PdfPCell(new Phrase(unitPrice, font));
                    cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    productsTable.AddCell(cellProductItem);

                    //qty
                    cellProductItem = new PdfPCell(new Phrase(string.Empty, font));
                    cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    productsTable.AddCell(cellProductItem);

                    //total
                    string subTotal = string.Empty;
                    cellProductItem = new PdfPCell(new Phrase(subTotal, font));
                    cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                    productsTable.AddCell(cellProductItem);
                });
                doc.Add(productsTable);

                #endregion

                #region Checkout attributes

                //vendors cannot see checkout attributes
                if (vendorId == 0 && !String.IsNullOrEmpty(order.CheckoutAttributeDescription))
                {
                    doc.Add(new Paragraph(" "));
                    var attribTable = new PdfPTable(1);
                    attribTable.RunDirection = GetDirection(lang);
                    attribTable.WidthPercentage = 100f;

                    string attributes = HtmlHelper.ConvertHtmlToPlainText(order.CheckoutAttributeDescription, true, true);
                    var cCheckoutAttributes = new PdfPCell(new Phrase(attributes, font));
                    cCheckoutAttributes.Border = Rectangle.NO_BORDER;
                    cCheckoutAttributes.HorizontalAlignment = Element.ALIGN_RIGHT;
                    attribTable.AddCell(cCheckoutAttributes);
                    doc.Add(attribTable);
                }

                #endregion

                #region Totals

                //vendors cannot see totals
                if (vendorId == 0)
                {
                    //subtotal
                    var totalsTable = new PdfPTable(1);
                    totalsTable.RunDirection = GetDirection(lang);
                    totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;
                    totalsTable.WidthPercentage = 100f;

                    //order subtotal
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                    {
                        //including tax

                        var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                        string orderSubtotalInclTaxStr = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                        var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id), orderSubtotalInclTaxStr), font));
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }
                    else
                    {
                        //excluding tax

                        var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                        string orderSubtotalExclTaxStr = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                        var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Sub-Total", lang.Id), orderSubtotalExclTaxStr), font));
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }

                    //discount (applied to order subtotal)
                    if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                    {
                        //order subtotal
                        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                        {
                            //including tax

                            var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                            string orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                            var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", lang.Id), orderSubTotalDiscountInCustomerCurrencyStr), font));
                            p.HorizontalAlignment = Element.ALIGN_RIGHT;
                            p.Border = Rectangle.NO_BORDER;
                            totalsTable.AddCell(p);
                        }
                        else
                        {
                            //excluding tax

                            var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                            string orderSubTotalDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                            var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Discount", lang.Id), orderSubTotalDiscountInCustomerCurrencyStr), font));
                            p.HorizontalAlignment = Element.ALIGN_RIGHT;
                            p.Border = Rectangle.NO_BORDER;
                            totalsTable.AddCell(p);
                        }
                    }

                    //shipping
                    if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
                    {
                        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                        {
                            //including tax
                            var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                            string orderShippingInclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                            var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", lang.Id), orderShippingInclTaxStr), font));
                            p.HorizontalAlignment = Element.ALIGN_RIGHT;
                            p.Border = Rectangle.NO_BORDER;
                            totalsTable.AddCell(p);
                        }
                        else
                        {
                            //excluding tax
                            var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                            string orderShippingExclTaxStr = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                            var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Shipping", lang.Id), orderShippingExclTaxStr), font));
                            p.HorizontalAlignment = Element.ALIGN_RIGHT;
                            p.Border = Rectangle.NO_BORDER;
                            totalsTable.AddCell(p);
                        }
                    }

                    var shippingDiscounts = order.DeliveryPromotions().ToList();
                    if (shippingDiscounts.Any())
                    {
                        shippingDiscounts.ForEach(shippingDiscount =>
                        {
                            var shippingDiscountAmount = _currencyService.ConvertCurrency(shippingDiscount.DiscountAmount, order.CurrencyRate);
                            var displayShippingDiscount = _priceFormatter.FormatPrice(-shippingDiscountAmount, true, order.CustomerCurrencyCode, false, lang);
                            var p = new PdfPCell(new Paragraph(string.Format("{0} {1}", shippingDiscount.DisplayDetails(), displayShippingDiscount), promosFont));
                            p.HorizontalAlignment = Element.ALIGN_RIGHT;
                            p.Border = Rectangle.NO_BORDER;
                            totalsTable.AddCell(p);
                        });
                    }

                    //payment fee
                    if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
                    {
                        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                        {
                            //including tax
                            var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                            string paymentMethodAdditionalFeeInclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, true);

                            var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id), paymentMethodAdditionalFeeInclTaxStr), font));
                            p.HorizontalAlignment = Element.ALIGN_RIGHT;
                            p.Border = Rectangle.NO_BORDER;
                            totalsTable.AddCell(p);
                        }
                        else
                        {
                            //excluding tax
                            var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                            string paymentMethodAdditionalFeeExclTaxStr = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, lang, false);

                            var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.PaymentMethodAdditionalFee", lang.Id), paymentMethodAdditionalFeeExclTaxStr), font));
                            p.HorizontalAlignment = Element.ALIGN_RIGHT;
                            p.Border = Rectangle.NO_BORDER;
                            totalsTable.AddCell(p);
                        }
                    }

                    //tax
                    string taxStr = string.Empty;
                    var taxRates = new SortedDictionary<decimal, decimal>();
                    bool displayTax = true;
                    bool displayTaxRates = true;
                    if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        displayTax = false;
                    }
                    else
                    {
                        if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                        {
                            displayTax = false;
                            displayTaxRates = false;
                        }
                        else
                        {
                            taxRates = order.TaxRatesDictionary;

                            displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                            displayTax = !displayTaxRates;

                            var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                            taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);
                        }
                    }
                    if (displayTax)
                    {
                        var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.Tax", lang.Id), taxStr), font));
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }
                    if (displayTaxRates)
                    {
                        foreach (var item in taxRates)
                        {
                            string taxRate = String.Format(_localizationService.GetResource("PDFInvoice.TaxRate", lang.Id), _priceFormatter.FormatTaxRate(item.Key));
                            string taxValue = _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, false, lang);

                            var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", taxRate, taxValue), font));
                            p.HorizontalAlignment = Element.ALIGN_RIGHT;
                            p.Border = Rectangle.NO_BORDER;
                            totalsTable.AddCell(p);
                        }
                    }

                    //discount (applied to order total) ("Your total savings" ???)
                    if (order.OrderDiscount > decimal.Zero)
                    {
                        string discountName = _localizationService.GetResource("PDFInvoice.Discount", lang.Id);
                        Font discountFont = font;

                        //if (promoOrder != null)
                        //{
                        //    discountName = promoOrder.GetBasketLevelPromotionName(_promoSettings);
                        //    discountFont = promosFont;
                        //}

                        var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                        string orderDiscountInCustomerCurrencyStr = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);

                        var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", discountName, orderDiscountInCustomerCurrencyStr), discountFont));
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }

                    //gift cards
                    foreach (var gcuh in order.GiftCardUsageHistory)
                    {
                        string gcTitle = string.Format(_localizationService.GetResource("PDFInvoice.GiftCardInfo", lang.Id), gcuh.GiftCard.GiftCardCouponCode);
                        string gcAmountStr = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, lang);

                        var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", gcTitle, gcAmountStr), font));
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }

                    //reward points
                    if (order.RedeemedRewardPointsEntry != null)
                    {
                        string rpTitle = string.Format(_localizationService.GetResource("PDFInvoice.RewardPoints", lang.Id), -order.RedeemedRewardPointsEntry.Points);
                        string rpAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, lang);

                        var p = new PdfPCell(new Paragraph(String.Format("{0} {1}", rpTitle, rpAmount), font));
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }

                    //order total
                    var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                    string orderTotalStr = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, lang);


                    var pTotal = new PdfPCell(new Paragraph(String.Format("{0} {1}", _localizationService.GetResource("PDFInvoice.OrderTotal", lang.Id), orderTotalStr), titleFont));
                    pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                    pTotal.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(pTotal);

                    doc.Add(totalsTable);
                }

                #endregion

                #region Order notes

                if (pdfSettingsByStore.RenderOrderNotes)
                {
                    var orderNotes = order.OrderNotes
                        .Where(on => on.DisplayToCustomer)
                        .OrderByDescending(on => on.CreatedOnUtc)
                        .ToList();
                    if (orderNotes.Any())
                    {
                        var notesHeader = new PdfPTable(1);
                        notesHeader.RunDirection = GetDirection(lang);
                        notesHeader.WidthPercentage = 100f;
                        var cellOrderNote = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes", lang.Id), titleFont));
                        cellOrderNote.Border = Rectangle.NO_BORDER;
                        notesHeader.AddCell(cellOrderNote);
                        doc.Add(notesHeader);
                        doc.Add(new Paragraph(" "));

                        var notesTable = new PdfPTable(2);
                        notesTable.RunDirection = GetDirection(lang);
                        if (lang.Rtl)
                        {
                            notesTable.SetWidths(new[] { 70, 30 });
                        }
                        else
                        {
                            notesTable.SetWidths(new[] { 30, 70 });
                        }
                        notesTable.WidthPercentage = 100f;

                        //created on
                        cellOrderNote = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes.CreatedOn", lang.Id), font));
                        cellOrderNote.BackgroundColor = BaseColor.LIGHT_GRAY;
                        cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
                        notesTable.AddCell(cellOrderNote);

                        //note
                        cellOrderNote = new PdfPCell(new Phrase(_localizationService.GetResource("PDFInvoice.OrderNotes.Note", lang.Id), font));
                        cellOrderNote.BackgroundColor = BaseColor.LIGHT_GRAY;
                        cellOrderNote.HorizontalAlignment = Element.ALIGN_CENTER;
                        notesTable.AddCell(cellOrderNote);

                        foreach (var orderNote in orderNotes)
                        {
                            cellOrderNote = new PdfPCell(new Phrase(_dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc).ToString(), font));
                            cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                            notesTable.AddCell(cellOrderNote);

                            cellOrderNote = new PdfPCell(new Phrase(HtmlHelper.ConvertHtmlToPlainText(orderNote.FormatOrderNoteText(), true, true), font));
                            cellOrderNote.HorizontalAlignment = Element.ALIGN_LEFT;
                            notesTable.AddCell(cellOrderNote);

                            //should we display a link to downloadable files here?
                            //I think, no. Onyway, PDFs are printable documents and links (files) are useful here
                        }
                        doc.Add(notesTable);
                    }
                }

                #endregion

                #region Footer

                if (!String.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1) || !String.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2))
                {
                    var column1Lines = String.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn1) ?
                        new List<string>() :
                        pdfSettingsByStore.InvoiceFooterTextColumn1
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    var column2Lines = String.IsNullOrEmpty(pdfSettingsByStore.InvoiceFooterTextColumn2) ?
                        new List<string>() :
                        pdfSettingsByStore.InvoiceFooterTextColumn2
                        .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    if (column1Lines.Any() || column2Lines.Any())
                    {
                        var totalLines = Math.Max(column1Lines.Count, column2Lines.Count);
                        const float margin = 43;

                        //if you have really a lot of lines in the footer, then replace 9 with 10 or 11
                        int footerHeight = totalLines * 9;
                        var directContent = pdfWriter.DirectContent;
                        directContent.MoveTo(pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight);
                        directContent.LineTo(pageSize.GetRight(margin), pageSize.GetBottom(margin) + footerHeight);
                        directContent.Stroke();


                        var footerTable = new PdfPTable(2);
                        footerTable.WidthPercentage = 100f;
                        footerTable.SetTotalWidth(new float[] { 250, 250 });
                        footerTable.RunDirection = GetDirection(lang);

                        //column 1
                        if (column1Lines.Any())
                        {
                            var column1 = new PdfPCell(new Phrase());
                            column1.Border = Rectangle.NO_BORDER;
                            column1.HorizontalAlignment = Element.ALIGN_LEFT;
                            foreach (var footerLine in column1Lines)
                            {
                                column1.Phrase.Add(new Phrase(footerLine, font));
                                column1.Phrase.Add(new Phrase(Environment.NewLine));
                            }
                            footerTable.AddCell(column1);
                        }
                        else
                        {
                            var column = new PdfPCell(new Phrase(" "));
                            column.Border = Rectangle.NO_BORDER;
                            footerTable.AddCell(column);
                        }

                        //column 2
                        if (column2Lines.Any())
                        {
                            var column2 = new PdfPCell(new Phrase());
                            column2.Border = Rectangle.NO_BORDER;
                            column2.HorizontalAlignment = Element.ALIGN_LEFT;
                            foreach (var footerLine in column2Lines)
                            {
                                column2.Phrase.Add(new Phrase(footerLine, font));
                                column2.Phrase.Add(new Phrase(Environment.NewLine));
                            }
                            footerTable.AddCell(column2);
                        }
                        else
                        {
                            var column = new PdfPCell(new Phrase(" "));
                            column.Border = Rectangle.NO_BORDER;
                            footerTable.AddCell(column);
                        }

                        footerTable.WriteSelectedRows(0, totalLines, pageSize.GetLeft(margin), pageSize.GetBottom(margin) + footerHeight, directContent);
                    }
                }

                #endregion

                ordNum++;
                if (ordNum < ordCount)
                {
                    doc.NewPage();
                }
            }
            doc.Close();
        }

    }
}
