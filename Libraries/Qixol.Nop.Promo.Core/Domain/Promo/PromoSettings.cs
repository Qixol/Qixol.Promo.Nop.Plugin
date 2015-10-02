using Nop.Core.Configuration;

namespace Qixol.Nop.Promo.Core.Domain.Promo
{
    public static class PromotionDetailsDisplayOptions
    {
        public const int ShowEndUserText = 0;
        public const int ShowPromotionName = 1;
        public const int ShowPromotionType = 2;
        public const int ShowNoText = 3;
    }

    public class PromoSettings : ISettings
    {
        #region Integration Settings

        public bool Enabled { get; set; }
        public bool LogMessages { get; set; }
        public bool IsTest { get; set; }
        public int QueueHoldPeriodInSeconds { get; set; }

        public string VariantAttributeFormat { get; set; }
        public string VariantAttributesSeperator { get; set; }

        public string CompanyKey { get; set; }
        public int ServiceEndpointSelection { get; set; }
        public string PromoImportEndpointAddress { get; set; }
        public string BasketRequestEndpointAddress { get; set; }
        public string PromoExportEndpointAddress { get; set; }

        #endregion

        #region Syncronization settings

        public bool SynchronizeProducts { get; set; }
        public bool SynchronizeStores { get; set; }
        public bool SynchronizeCustomerRoles { get; set; }
        public bool SynchronizeShippingMethods { get; set; }
        public bool SynchronizeCheckoutAttributes { get; set; }
        public bool SynchronizeCurrencies { get; set; }

        public int ProductPictureSize { get; set; }
        public int StoreId { get; set; }
        public int CurrencyId { get; set; }
        public int BatchSize { get; set; }

        public string StoreGroup { get; set; }
        public string Channel { get; set; }

        public int MaximumAttributesForVariants { get; set; }

        #endregion

        #region Shopping Cart settings

        public int ShowPromotionDetailsInBasket { get; set; }

        #endregion

        public bool ShowHelperMessages { get; set; }

        public bool InitialSetup { get; set; }

        public bool UseSelectedCurrencyWhenSubmittingBaskets { get; set; }

        public bool HideNopDiscountMenuItems { get; set; }

        public bool ShowAdvancedIntegrationSettings { get; set; }
    }
}
