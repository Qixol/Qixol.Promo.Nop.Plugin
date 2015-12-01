using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Stores;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Tax;
using Nop.Services.Logging;
using Qixol.Nop.Promo.Services;
using Nop.Core.Domain.Tasks;
using Nop.Services.Tasks;
using Qixol.Nop.Promo.Core.Domain.Tasks;
using Qixol.Nop.Promo.Data;
using Qixol.Nop.Promo.Services.ProductAttributeConfig;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using Nop.Services.Cms;
using Nop.Core.Domain.Cms;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Data.Mapping;
using Qixol.Plugin.Widgets.Promo.Domain;
using Qixol.Nop.Promo.Services.Banner;

namespace Qixol.Plugin.Widgets.Promo
{
    public class PromoWidgets : BasePlugin, IWidgetPlugin
    {
        #region Fields

        private readonly WidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;
        private readonly IPromoBannerService _promoBannerService;
        private List<KeyValuePair<string, string>> _stringsList = new List<KeyValuePair<string, string>>();

        #endregion

        #region constructor

        public PromoWidgets(WidgetSettings widgetSettings,
                            ISettingService settingService,
                            IPromoBannerService promoBannerService)
        {
            this._widgetSettings = widgetSettings;
            this._settingService = settingService;
            this._promoBannerService = promoBannerService;
        }

        #endregion

        #region methods

        public void GetConfigurationRoute(out string actionName, out string controllerName, out global::System.Web.Routing.RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PromoWidgetAdmin";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Qixol.Plugin.Widgets.Promo.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install widget
        /// </summary>
        public override void Install()
        {
            // settings
            var settings = new PromoWidgetSettings()
            {
                ShowPromoDetailsOnProductPage = true,
                ShowStickersInCatalogue = true,
                ShowStickersInProductPage = true,
                ProductPagePromoDetailsWidgetZone = "productdetails_add_info"       // Default!
            };
            _settingService.SaveSetting(settings);

            // Deal with strings
            SetupStringResources();
            InstallAllStringResources();

            // set the widget to active (for the Customer Navigation menu only at present...)
            _widgetSettings.ActiveWidgetSystemNames.Add(this.PluginDescriptor.SystemName);
            
            base.Install();
        }

        /// <summary>
        /// Uninstall widget
        /// </summary>
        public override void Uninstall()
        {
            //locales
            SetupStringResources();
            UninstallAllStringResources();

            //settings
            _settingService.DeleteSetting<PromoWidgetSettings>();

            base.Uninstall();
        }

        #endregion

        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "PromoWidget";
            routeValues = new RouteValueDictionary()
            {
                {"Namespaces", "Qixol.Plugin.Widgets.Promo.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }

        public IList<string> GetWidgetZones()
        {
            List<string> activeWidgetZoneNames = _promoBannerService.RetrieveAllEnabledWidgetZones().Select(cw => cw.WidgetZoneSystemName).ToList();

            return activeWidgetZoneNames;
        }

        #region Locale Resources

        private void SetupStringResources()
        {
            this.InsertStringResource("Plugins.Widgets.QixolPromo.General", "General");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Pictures", "Promotion Stickers");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Banners", "Banners");

            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.AddButon", "Add sticker");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.Picture", "Sticker");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.Picture.Hint", "Choose a picture to upload as a promo sticker");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.DefaultForType", "Default for Promotion Type?");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.DefaultForType.Hint", "Indicates whether the picture to be uploaded will act as the default sticker for the selected Promotion Type");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.DefaultForTypeName", "Promotion Type");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.DefaultForTypeName.Hint", "Indicates which promotion type the picture to be uploaded will be the default for.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.PromoReference", "Promotion Reference");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.PromoReference.Hint", "The 'Your Reference' field defined against the promotion.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.DefaultForType.ColumnHeader", "Default for Promotion Type");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.ValidationMsg.Add", "A problem was encountered whilst attempting to add the picture.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.ValidationMsg.Delete", "A problem was encountered whilst attempting to delete the picture.");


            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.Validation.Msg.1", "Please upload a picture first");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.Validation.Msg.2", "A promotion reference must be specified.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.Validation.Msg.3", "A promotion type must be selected.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoPicture.Failure.Msg", "Failed to add sticker.");

            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoDetails", "Display Promotion Information");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.ShowStickersInCatalogue", "Show promotion stickers in catalogue?");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.ShowStickersInCatalogue.Hint", "Show promotion stickers over the product image in product listing pages in the catalogue.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.ShowStickersOnProductPage", "Show promotion stickers on product page?");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.ShowStickersOnProductPage.Hint", "Show promotion stickers over the product image on the product page.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.ShowAvailablePromotionsOnProductPage", "Show available promotions on product page?");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.ShowAvailablePromotionsOnProductPage.Hint", "Show details of available promotions for a product on the product page.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.ProductPagePromotionsInWidgetZone", "Show Promotion Details in Widget Zone");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.ProductPagePromotionsInWidgetZone.Hint", "Select where the promotion details will be presented on the product page.");

            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.AddNewTitle", "Add New Banner");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.AddButton", "Add Banner");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.Name", "Name");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.Name.Hint", "The name for the banner");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.Enabled", "Enabled");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.Enabled.Hint", "If this is checked then the banner can be displayed. If images within the banner are subject to promotion restrictions then they will only display if the promotion is valid at the point of display.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.TransitionType", "Transition Type");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.TransitionType.Hint", "The type of transition between images");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.WidgetZone", "Widget Zone");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.Validation.Msg.1", "A name must be specified for the new banner.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.PromoBanner.Failure.Msg", "Failed to add the banner.");

            this.InsertStringResource("Plugins.Widgets.QixolPromo.Banner.Pictures", "Banner Pictures");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Banner.WidgetZones", "Widget Zones");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Banner.Title", "Edit Banner");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Banner.Header", "Configure - Promo Banner -");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Banner.BackToConfig", "back to Qixol Promo Widgets configuration");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Banner.ValidationMsg.Edit", "A problem was encountered whilst attempting to edit the banner.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Banner.ValidationMsg.Delete", "A problem was encountered whilst attempting to delete the banner.");

            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.Picture", "Picture");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.Picture.Hint", "Choose a picture to upload");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.PromoRef", "Promotion Reference");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.PromoRef.Hint", "The 'Your Reference' field defined against the promotion.  If this field is left blank, the picture will always be displayed.  If a Promotion Reference is supplied, the picture will only be displayed if the promotion is available.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.Comment", "Comment");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.Comment.Hint", "The comment to be displayed when hovering over the picture.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.Url", "Url");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.Url.Hint", "The Url to navigate to when the picture is clicked.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.DisplaySequence", "DisplaySequence");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.DisplaySequence.Hint", "A number which indicates the sequence in which the pictures will be displayed.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.AddNewTitle", "Add New Banner Picture");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.AddButton", "Add Banner Picture");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.Validation.Msg.1", "Please upload a picture first");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.Failure.Msg", "Failed to add banner picture.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.ValidationMsg.Add", "A problem was encountered whilst attempting to add the picture.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.ValidationMsg.Edit", "A problem was encountered whilst attempting to edit the picture.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerPicture.ValidationMsg.Delete", "A problem was encountered whilst attempting to delete the picture.");

            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerWidgets.Zone", "Widget Zone");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerWidgets.Zone.Hint", "Select a widget zone where this banner will be displayed.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerWidgets.AddNewZone", "Add New Widget Zone");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerWidgets.AddNewZoneButton", "Add Widget Zone");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerWidgets.Failure.Msg", "Failed to add the widget zone.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerWidgets.ValidationMsg.Add", "A problem was encountered whilst attempting to add the widget zone.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerWidgets.ValidationMsg.Delete", "A problem was encountered whilst attempting to delete the widget zone.");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerWidgets.CustomWidgetZone", "The name of a custom widget zone");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.BannerWidgets.CustomWidgetZone.Hint", "This enables a banner to be displayed in a custom widget zone.");

            this.InsertStringResource("Plugins.Widgets.QixolPromo.Coupons.Code", "Code");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Coupons.Status", "Status");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Coupons.ValidTo", "Valid To");
            this.InsertStringResource("Plugins.Widgets.QixolPromo.Coupons.Description", "Description");

            this.InsertStringResource(NivoTransition.SLICEDOWN.ResourceName, "Slice Down");
            this.InsertStringResource(NivoTransition.SLICEDOWNLEFT.ResourceName, "Slice Down Left");
            this.InsertStringResource(NivoTransition.SLICEUP.ResourceName, "Slice Up");
            this.InsertStringResource(NivoTransition.SLICEUPLEFT.ResourceName, "Slice Up Left");
            this.InsertStringResource(NivoTransition.SLICEUPDOWN.ResourceName, "Slice Up Down");
            this.InsertStringResource(NivoTransition.SLICEUPDOWNLEFT.ResourceName, "Slice Up Down Left");
            this.InsertStringResource(NivoTransition.FOLD.ResourceName, "Fold");
            this.InsertStringResource(NivoTransition.FADE.ResourceName, "Fade");
            this.InsertStringResource(NivoTransition.RANDOM.ResourceName, "Random");
            this.InsertStringResource(NivoTransition.SLIDEINRIGHT.ResourceName, "Slide In Right");
            this.InsertStringResource(NivoTransition.SLIDEINLEFT.ResourceName, "Slide In Left");
            this.InsertStringResource(NivoTransition.BOXRANDOM.ResourceName, "Box Random");
            this.InsertStringResource(NivoTransition.BOXRAIN.ResourceName, "Box Rain");
            this.InsertStringResource(NivoTransition.BOXRAINREVERSE.ResourceName, "Box Rain Reverse");
            this.InsertStringResource(NivoTransition.BOXRAINGROW.ResourceName, "Box Rain Grow");
            this.InsertStringResource(NivoTransition.BOXRAINGROWREVERSE.ResourceName, "Box Rain Grow Reverse");

            this.InsertStringResource(NivoTransition.STACKHORIZONTAL.ResourceName, "Stack Horizontal");
            this.InsertStringResource(NivoTransition.STACKVERTICAL.ResourceName, "Stack Vertical");
        }

        private void InsertStringResource(string resourceKey, string text)
        {
            this._stringsList.Add(new KeyValuePair<string, string>(resourceKey, text));
        }

        private void InstallAllStringResources()
        {
            this._stringsList.ForEach(r =>
                {
                    this.AddOrUpdatePluginLocaleResource(r.Key, r.Value);
                });
        }

        private void UninstallAllStringResources()
        {
            this._stringsList.ForEach(r =>
                {
                    this.DeletePluginLocaleResource(r.Key);
                });
        }

        #endregion

    }
}
