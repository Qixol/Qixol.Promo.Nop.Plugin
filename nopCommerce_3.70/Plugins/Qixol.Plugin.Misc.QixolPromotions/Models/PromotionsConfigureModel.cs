using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Qixol.Plugin.Misc.Promo.Models
{
    public class PromoConfigureModel
    {
        public PromoConfigureModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableCurrencies = new List<SelectListItem>();
            ProductConfigItems = new List<ProductAttributeConfigItemModel>();
            ServicesEndpointsList = new List<SelectListItem>();
            ShowPromotionNameOptionsList = new List<SelectListItem>();
        }

        #region first tab

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.LogMessages")]
        public bool LogMessages { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.CompanyKey")]
        public string CompanyKey { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.ImportServiceEndpointAddress")]
        public string PromoImportServiceEndpointAddress { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.BasketRequestServiceEndpointAddress")]
        public string BasketRequestServiceEndpointAddress { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.ExportServiceEndpointAddress")]
        public string PromoExportServiceEndpointAddress { get; set; }

        #endregion

        #region second tab

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.SyncProducts")]
        public bool SynchronizeProducts { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.SyncShippingMethods")]
        public bool SynchronizeShippingMethods { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.SyncCustomerRoles")]
        public bool SynchronizeCustomerRoles { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.SyncStores")]
        public bool SynchronizeStores { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.SyncCurrencies")]
        public bool SynchronizeCurrencies { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.SyncCheckoutAttributes")]
        public bool SynchronizeCheckoutAttributes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.ProductPictureSize")]
        public int ProductPictureSize { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.Store")]
        public int StoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.Currency")]
        public int CurrencyId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.BatchSize")]
        public int BatchSize { get; set; }

        public IList<SelectListItem> AvailableCurrencies { get; set; }

        #endregion

        #region basket settings

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.StoreGroup")]
        public string StoreGroup { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.Channel")]
        public string Channel { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.ShowPromoDetails")]
        public int ShowPromotionNameOption { get; set; }

        #endregion

        public List<ProductAttributeConfigItemModel> ProductConfigItems { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.ServiceEndpoint")]
        public int SelectedServicesEndPoint { get; set; }

        public IList<SelectListItem> ServicesEndpointsList { get; set; }

        public IList<SelectListItem> ShowPromotionNameOptionsList { get; set; }

        public bool AllowCustomEndpoint { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.ShowHelpMessages")]
        public bool ShowHelperMessages { get; set; }

        public string DisplayVersion { get; set; }

        public bool CanUpdateSynchronizeAll { get; set; }

        public bool ShowAdvancedIntegrationSettings { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.SynchronizeNow")]
        public bool SynchronizeAllWhenSaving { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.UseSelectedCurrency")]
        public bool UseSelectedCurrencyWhenSubmittingBaskets { get; set; }

        [NopResourceDisplayName("Plugins.Misc.QixolPromo.ShowMissedPromotions")]
        public bool ShowMissedPromotions { get; set; }

    }
}
