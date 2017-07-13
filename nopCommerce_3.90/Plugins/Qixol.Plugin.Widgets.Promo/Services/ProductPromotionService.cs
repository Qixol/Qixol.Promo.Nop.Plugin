using Nop.Core;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Themes;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Nop.Promo.Core.Domain.AttributeValues;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Services.AttributeValues;
using Qixol.Nop.Promo.Services.ProductMapping;
using Qixol.Nop.Promo.Services.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Services
{
    public class ProductPromotionService : IProductPromotionService
    {
        #region private variables

        private readonly IProductMappingService _productMappingService;
        private readonly IProductPromoMappingService _productPromoMappingService;
        private readonly IPromoDetailService _promoDetailService;
        private readonly IStoreService _storeService;
        private readonly IAttributeValueService _attributeValueService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IPromoPictureService _promoPictureService;
        private readonly IThemeContext _themeContext;
        private readonly IPictureService _pictureService;

        #endregion

        #region constructor

        public ProductPromotionService(IProductMappingService productMappingService,
            IProductPromoMappingService productPromoMappingService,
            IPromoDetailService promoDetailService,
            IStoreService storeService,
            IAttributeValueService attributeValueService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IPromoPictureService promoPictureService,
            IThemeContext themeContext,
            IPictureService pictureService)
        {
            this._productMappingService = productMappingService;
            this._productPromoMappingService = productPromoMappingService;
            this._promoDetailService = promoDetailService;
            this._storeService = storeService;
            this._attributeValueService = attributeValueService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._promoPictureService = promoPictureService;
            this._themeContext = themeContext;
            this._pictureService = pictureService;
        }

        #endregion

        #region utilities

        // TODO: BasketCriteriaItems only made public because the banner code needs rewriting
        public List<KeyValuePair<string, string>> BasketCriteriaItems()
        {
            var basketCriteriaChecks = new List<KeyValuePair<string, string>>();

            // ** Current Store
            var allStores = _storeService.GetAllStores();
            if (allStores.Count > 1)
            {
                // If we have a store code, add it as a criteria
                var storeCode = _attributeValueService.Retrieve(_storeContext.CurrentStore.Id, EntityAttributeName.Store);
                if (storeCode != null && !string.IsNullOrEmpty(storeCode.Code))
                    basketCriteriaChecks.Add(new KeyValuePair<string, string>(EntityAttributeName.ToPromoAttributeName(EntityAttributeName.Store), storeCode.Code));
            }

            // ** Role
            if (_workContext.CurrentCustomer != null && _workContext.CurrentCustomer.CustomerRoles != null)
            {
                // The user can be in multiple roles, so use the Priority assigned to the Integration code to establish
                // which one to use.
                //
                var rolesList = _workContext.CurrentCustomer.CustomerRoles.ToList();
                var attribsList = _attributeValueService.RetrieveAllForAttribute(EntityAttributeName.CustomerRole).ToList();

                if (attribsList != null && attribsList.Count > 0)
                {
                    var selectedRole = (from r in rolesList
                                        join a in attribsList
                                            on r.Id equals a.AttributeValueId
                                        orderby a.Priority == null ? 0 : a.Priority descending
                                        select a).FirstOrDefault();

                    if (selectedRole != null)
                        basketCriteriaChecks.Add(new KeyValuePair<string, string>(EntityAttributeName.ToPromoAttributeName(EntityAttributeName.CustomerRole), selectedRole.Code));
                }
            }

            // ** Currency
            if (_workContext.WorkingCurrency != null)
                basketCriteriaChecks.Add(new KeyValuePair<string, string>(EntityAttributeName.ToPromoAttributeName(EntityAttributeName.Currency), _workContext.WorkingCurrency.CurrencyCode));

            return basketCriteriaChecks;
        }

        #endregion

        #region methods

        public string GetImageUrl(string baseImageName, string themeName)
        {
            var imageUrl = string.Format("/Plugins/Widgets.QixolPromo/Themes/{0}/Content/images/{1}.png", themeName, baseImageName);
            if (string.IsNullOrEmpty(themeName))
            {
                imageUrl = string.Format("/Plugins/Widgets.QixolPromo/Content/images/{0}.png", baseImageName);
            }
            return imageUrl;
        }

        public string GetPromoImageUrl(string promoReference, string promoType, string promoBaseImageName)
        {
            var promoImage = _promoPictureService.RetrieveForPromo(promoReference, promoType);
            if (promoImage != null)
                return _pictureService.GetPictureUrl(promoImage.PictureId);
            else
                return this.GetImageUrl(promoBaseImageName, _themeContext.WorkingThemeName);
        }

        public string GetSingleImageUrl(List<ValidatedPromo> promosToDisplay, string baseUrl)
        {
            string returnUrl = baseUrl;
            if (promosToDisplay.Count == 1)
            {
                var promo = promosToDisplay.FirstOrDefault();
                returnUrl = GetPromoImageUrl(promo.YourReference, promo.PromoType, promo.ImageName);
            }
            else
            {
                var multiplePromosImage = _promoPictureService.RetrieveForPromo(string.Empty, PromotionTypeName.Multiple_Promos);
                if (multiplePromosImage != null)
                    returnUrl = _pictureService.GetPictureUrl(multiplePromosImage.PictureId);
            }
            return returnUrl;
        }

        public ProductPromotions PromotionsForProduct(int productId)
        {
            var returnDetails = new ProductPromotions() { ProductId = productId };
            var mappings = _productMappingService.RetrieveAllVariantsByProductId(productId, EntityAttributeName.Product, true);

            if (mappings != null && mappings.Count() > 0)
            {
                var mappingIds = mappings.Select(m => m.Id).ToList();
                var productPromoMappings = _productPromoMappingService.RetrieveForProductMappingsList(mappingIds);
                if (productPromoMappings != null && productPromoMappings.Count() > 0)
                {
                    var promoIds = productPromoMappings.GroupBy(gb => gb.PromotionId).Select(s => s.FirstOrDefault().PromotionId).ToList();
                    if (promoIds != null && promoIds.Count > 0)
                    {
                        var basketCriteriaChecks = BasketCriteriaItems();
                        promoIds.ForEach(promoId =>
                        {
                            var promo = _promoDetailService.RetrieveByPromoId(promoId);
                            if (promo != null)
                            {
                                // Note have to put this in a variable otherwise it won't add to the list!!
                                var validatedPromo = ValidatedPromo.Create(promo, mappings, basketCriteriaChecks, productPromoMappings);
                                returnDetails.PromoDetails.Add(validatedPromo);
                            }
                        });
                    }
                    returnDetails.BaseImageUrl = this.GetImageUrl("promo_offers", _themeContext.WorkingThemeName);
                }
            }

            return returnDetails;
        }

        #endregion
    }
}
