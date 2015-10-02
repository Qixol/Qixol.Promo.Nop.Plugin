using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Nop.Promo.Core.Domain.Banner;
using Qixol.Nop.Promo.Services.Promo;
using Qixol.Nop.Promo.Services.Banner;
using Qixol.Plugin.Widgets.Promo.Domain;
using Qixol.Plugin.Widgets.Promo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Web.Framework.Themes;

namespace Qixol.Plugin.Widgets.Promo.Controllers
{
    [AdminAuthorize]
    public partial class PromoWidgetAdminController : BasePluginController
    {
        #region fields 

        private readonly IPromoPictureService _promoPictureService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly PromoWidgetSettings _widgetSettings;
        private readonly ISettingService _settingService;
        private readonly IPromoBannerService _promoBannerService;
        private readonly IThemeContext _themeContext;
        #endregion

        #region constructor

        public PromoWidgetAdminController(
                            ILocalizationService localizationService,
                            IPromoPictureService promoPictureService,
                            IPictureService pictureService,
                            PromoWidgetSettings widgetSettings,
                            ISettingService settingService,
                            IPromoBannerService promoBannerService,
                            IThemeContext themeContext)
        {
            this._localizationService = localizationService;
            this._promoPictureService = promoPictureService;
            this._pictureService = pictureService;
            this._widgetSettings = widgetSettings;
            this._settingService = settingService;
            this._promoBannerService = promoBannerService;
            this._themeContext = themeContext;
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public ActionResult Configure(int? selectTab)
        {
            var model = new WidgetConfigModel();

            model.SelectedTab = selectTab.HasValue ? selectTab.Value : 0;
            model.ShowPromotionDetailsOnProductPage = _widgetSettings.ShowPromoDetailsOnProductPage;
            model.ShowStickersInCatalogue = _widgetSettings.ShowStickersInCatalogue;
            model.ShowStickersInProductPage = _widgetSettings.ShowStickersInProductPage;
            model.ProductPagePromoDetailsWidgetZone = _widgetSettings.ProductPagePromoDetailsWidgetZone;

            model.AddPictureModel.PromoTypes = new List<SelectListItem>();
            model.AddPictureModel.PromoTypes.Add(new SelectListItem() { Value = PromotionTypeName.BuyOneGetOneFree, Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.BOGOF") });
            model.AddPictureModel.PromoTypes.Add(new SelectListItem() { Value = PromotionTypeName.BuyOneGetOneReduced, Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.BOGOR") });
            model.AddPictureModel.PromoTypes.Add(new SelectListItem() { Value = PromotionTypeName.Bundle, Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.BUNDLE") });
            model.AddPictureModel.PromoTypes.Add(new SelectListItem() { Value = PromotionTypeName.Deal, Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.DEAL") });
            model.AddPictureModel.PromoTypes.Add(new SelectListItem() { Value = PromotionTypeName.ProductsReduction, Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.PRODUCTSREDUCTION") });
            model.AddPictureModel.PromoTypes.Add(new SelectListItem() { Value = PromotionTypeName.Multiple_Promos, Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.MULTIPLE") });
            model.AddPictureModel.PromoTypes.Add(new SelectListItem() { Value = PromotionTypeName.FreeProduct, Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.FREEPRODUCT") });
            model.AddPictureModel.PromoTypes.Add(new SelectListItem() { Value = PromotionTypeName.IssueCoupon, Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.ISSUECOUPON") });
            model.AddPictureModel.PromoTypes.Add(new SelectListItem() { Value = PromotionTypeName.IssuePoints, Text = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.ISSUEPOINTS") });

            model.AddPromoBannerModel.AvailableTransitionTypes = new List<SelectListItem>();
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.SLICEDOWN, Text = _localizationService.GetResource(TransitionTypeName.SLICEDOWN) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.SLICEDOWNLEFT, Text = _localizationService.GetResource(TransitionTypeName.SLICEDOWNLEFT) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.SLICEUP, Text = _localizationService.GetResource(TransitionTypeName.SLICEUP) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.SLICEUPLEFT, Text = _localizationService.GetResource(TransitionTypeName.SLICEUPLEFT) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.SLICEUPDOWN, Text = _localizationService.GetResource(TransitionTypeName.SLICEUPDOWN) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.SLICEUPDOWNLEFT, Text = _localizationService.GetResource(TransitionTypeName.SLICEUPDOWNLEFT) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.FOLD, Text = _localizationService.GetResource(TransitionTypeName.FOLD) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.FADE, Text = _localizationService.GetResource(TransitionTypeName.FADE) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.RANDOM, Text = _localizationService.GetResource(TransitionTypeName.RANDOM) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.SLIDEINRIGHT, Text = _localizationService.GetResource(TransitionTypeName.SLIDEINRIGHT) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.SLIDEINLEFT, Text = _localizationService.GetResource(TransitionTypeName.SLIDEINLEFT) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.BOXRANDOM, Text = _localizationService.GetResource(TransitionTypeName.BOXRANDOM) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.BOXRAIN, Text = _localizationService.GetResource(TransitionTypeName.BOXRAIN) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.BOXRAINREVERSE, Text = _localizationService.GetResource(TransitionTypeName.BOXRAINREVERSE) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.BOXRAINGROW, Text = _localizationService.GetResource(TransitionTypeName.BOXRAINGROW) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.BOXRAINGROWREVERSE, Text = _localizationService.GetResource(TransitionTypeName.BOXRAINGROWREVERSE) });

            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.STACKHORIZONTAL, Text = _localizationService.GetResource(TransitionTypeName.STACKHORIZONTAL) });
            model.AddPromoBannerModel.AvailableTransitionTypes.Add(new SelectListItem() { Value = TransitionTypeName.STACKVERTICAL, Text = _localizationService.GetResource(TransitionTypeName.STACKVERTICAL) });

            var widgetZones = WidgetZonesHelper.GetWidgetZonesForProductPromos();
            model.ProductPagePromoDetailsWidgetZonesList = widgetZones.Select(wz => new SelectListItem() { Text = wz.DisplayName, Value = wz.Name }).OrderBy(ob => ob.Text).ToList();

            if (string.IsNullOrEmpty(model.ProductPagePromoDetailsWidgetZone))
            {
                var defaultWidgetZone = widgetZones.Where(z => z.DefaultForProductPromos).FirstOrDefault();
                if (defaultWidgetZone != null)
                    model.ProductPagePromoDetailsWidgetZone = defaultWidgetZone.Name;
            }


            return View("~/Plugins/Widgets.QixolPromo/Views/Admin/Configure.cshtml", model);
        }

        [HttpPost]
        [ChildActionOnly]
        [FormValueRequired("save")]
        public ActionResult Configure(WidgetConfigModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Configure(0);
            }

            _widgetSettings.ShowPromoDetailsOnProductPage = model.ShowPromotionDetailsOnProductPage;
            _widgetSettings.ShowStickersInCatalogue = model.ShowStickersInCatalogue;
            _widgetSettings.ShowStickersInProductPage = model.ShowStickersInProductPage;
            _widgetSettings.ProductPagePromoDetailsWidgetZone = model.ProductPagePromoDetailsWidgetZone;

            _settingService.SaveSetting(_widgetSettings);

            ModelState.Clear();

            //redisplay the form
            return Configure(0);
        }

        [HttpPost]
        public ActionResult TransitionTypes(DataSourceRequest command)
        {
            List<TransitionType> tts = new List<TransitionType>();

            tts.Add(ToTT(TransitionTypeName.SLICEDOWN));
            tts.Add(ToTT(TransitionTypeName.SLICEDOWNLEFT));
            tts.Add(ToTT(TransitionTypeName.SLICEUP));
            tts.Add(ToTT(TransitionTypeName.SLICEUPLEFT));
            tts.Add(ToTT(TransitionTypeName.SLICEUPDOWN));
            tts.Add(ToTT(TransitionTypeName.SLICEUPDOWNLEFT));
            tts.Add(ToTT(TransitionTypeName.FOLD));
            tts.Add(ToTT(TransitionTypeName.FADE));
            tts.Add(ToTT(TransitionTypeName.RANDOM));
            tts.Add(ToTT(TransitionTypeName.SLIDEINRIGHT));
            tts.Add(ToTT(TransitionTypeName.SLIDEINLEFT));
            tts.Add(ToTT(TransitionTypeName.BOXRANDOM));
            tts.Add(ToTT(TransitionTypeName.BOXRAIN));
            tts.Add(ToTT(TransitionTypeName.BOXRAINREVERSE));
            tts.Add(ToTT(TransitionTypeName.BOXRAINGROW));
            tts.Add(ToTT(TransitionTypeName.BOXRAINGROWREVERSE));

            tts.Add(ToTT(TransitionTypeName.STACKHORIZONTAL));
            tts.Add(ToTT(TransitionTypeName.STACKVERTICAL));

            return Json(tts);
        }

        #endregion

        #region Banners

        public ActionResult BannersList(DataSourceRequest command)
        {
            var allBanners = _promoBannerService.RetrieveAllBanners();
            var modelItems
                = allBanners.Select(s => new PromoBannerModel()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Enabled = s.Enabled,
                    TransitionType = s.TransitionType
                }).ToList();

            if (modelItems != null && modelItems.Count() > 0)
            {
                modelItems.ForEach(i =>
                    {
                        i.TransitionTypeName = _localizationService.GetResource(i.TransitionType);
                    });
            }

            var gridModel = new DataSourceResult
            {
                Data = modelItems,
                Total = modelItems != null ? modelItems.Count() : 0
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult BannerAdd(string name, bool enabled, string transitionType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var newBanner = new PromoBanner()
            {
                Name = name,
                Enabled = enabled,
                TransitionType = transitionType
            };

            _promoBannerService.InsertBanner(newBanner);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BannerEdit(PromoBannerModel bannerModel)
        {
            if ((bannerModel == null)
                || (bannerModel.Id == 0)
                || (string.IsNullOrEmpty(bannerModel.Name)))
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.Banner.ValidationMsg.Edit"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var bannerItem = _promoBannerService.RetrieveBannerById(bannerModel.Id);
            if (bannerItem == null)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.Banner.ValidationMsg.Edit"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            bannerItem.Name = bannerModel.Name;
            bannerItem.Enabled = bannerModel.Enabled;
            bannerItem.TransitionType = bannerModel.TransitionType;

            _promoBannerService.UpdateBanner(bannerItem);
            return new NullJsonResult();
        }

        public ActionResult BannerDelete(PromoBannerModel bannerModel)
        {
            if (bannerModel == null)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.Banner.ValidationMsg.Delete"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            if (bannerModel.Id == 0)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.Banner.ValidationMsg.Delete"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var bannerItem = _promoBannerService.RetrieveBannerById(bannerModel.Id);
            if (bannerItem == null)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.Banner.ValidationMsg.Delete"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            _promoBannerService.DeleteBanner(bannerItem);

            return new NullJsonResult();
        }

        #endregion

        #region Pictures

        [HttpPost]
        public ActionResult PicturesList(DataSourceRequest command)
        {
            var modelItems = new List<PromoPictureModel>();

            var allPictures = _promoPictureService.RetrieveAll();
            var defaultItems = RetrieveSystemDefaults();
            var defaultItemsToAdd = defaultItems.Where(i => !allPictures.Any(mi => mi.IsDefaultForType && mi.PromoTypeName == i.DefaultForTypeName)).ToList();

            modelItems.AddRange(defaultItemsToAdd);
            modelItems.AddRange(allPictures.Select(p => new PromoPictureModel()
            {
                Id = p.Id,
                PictureId = p.PictureId,
                IsDefaultForType = p.IsDefaultForType,
                PromoReference = p.PromoReference,
                DefaultForTypeName = p.PromoTypeName,
            }));

            modelItems.Where(mi => !mi.SystemDefault).ToList().ForEach(m =>
            {
                m.PictureUrl = _pictureService.GetPictureUrl(m.PictureId);
            });

            modelItems.Where(mi => !string.IsNullOrEmpty(mi.DefaultForTypeName)).ToList().ForEach(mi =>
            {
                string displayName = mi.DefaultForTypeName;     // Default
                switch (mi.DefaultForTypeName)
                {
                    case PromotionTypeName.Bundle:
                        displayName = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.BUNDLE");
                        break;
                    case PromotionTypeName.BuyOneGetOneFree:
                        displayName = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.BOGOF");
                        break;
                    case PromotionTypeName.BuyOneGetOneReduced:
                        displayName = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.BOGOR");
                        break;
                    case PromotionTypeName.Deal:
                        displayName = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.DEAL");
                        break;
                    case PromotionTypeName.ProductsReduction:
                        displayName = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.PRODUCTSREDUCTION");
                        break;
                    case PromotionTypeName.Multiple_Promos:
                        displayName = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.MULTIPLE");
                        break;
                    case PromotionTypeName.FreeProduct:
                        displayName = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.FREEPRODUCT");
                        break;
                    case PromotionTypeName.IssueCoupon:
                        displayName = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.ISSUECOUPON");
                        break;
                    case PromotionTypeName.IssuePoints:
                        displayName = _localizationService.GetResource("Plugins.Misc.QixolPromo.PromoType.ISSUEPOINTS");
                        break;
                    default:
                        break;
                }

                mi.DefaultForTypeDisplay = displayName;

                if (mi.SystemDefault)
                    mi.PictureUrl = GetImageUrl(string.Format("promo_{0}", mi.DefaultForTypeName));
            });

            var gridModel = new DataSourceResult
            {
                Data = modelItems.OrderByDescending(ob => ob.IsDefaultForType).ThenBy(tb => tb.DefaultForTypeName).ThenBy(tb => tb.PromoReference),
                Total = modelItems != null ? modelItems.Count() : 0
            };

            return Json(gridModel);
        }

        private List<PromoPictureModel> RetrieveSystemDefaults()
        {
            var returnList = new List<PromoPictureModel>();

            returnList.Add(new PromoPictureModel() { IsDefaultForType = true, SystemDefault = true, DefaultForTypeName = PromotionTypeName.BuyOneGetOneFree });
            returnList.Add(new PromoPictureModel() { IsDefaultForType = true, SystemDefault = true, DefaultForTypeName = PromotionTypeName.BuyOneGetOneReduced });
            returnList.Add(new PromoPictureModel() { IsDefaultForType = true, SystemDefault = true, DefaultForTypeName = PromotionTypeName.Bundle });
            returnList.Add(new PromoPictureModel() { IsDefaultForType = true, SystemDefault = true, DefaultForTypeName = PromotionTypeName.Deal });
            returnList.Add(new PromoPictureModel() { IsDefaultForType = true, SystemDefault = true, DefaultForTypeName = PromotionTypeName.ProductsReduction });
            returnList.Add(new PromoPictureModel() { IsDefaultForType = true, SystemDefault = true, DefaultForTypeName = PromotionTypeName.FreeProduct });
            returnList.Add(new PromoPictureModel() { IsDefaultForType = true, SystemDefault = true, DefaultForTypeName = PromotionTypeName.IssueCoupon });
            returnList.Add(new PromoPictureModel() { IsDefaultForType = true, SystemDefault = true, DefaultForTypeName = PromotionTypeName.IssuePoints });
            returnList.Add(new PromoPictureModel() { IsDefaultForType = true, SystemDefault = true, DefaultForTypeName = PromotionTypeName.Multiple_Promos });

            return returnList;
        }

        [HttpPost]
        public ActionResult PictureAdd(int pictureId, bool defaultForType, string promoRef, string promoType)
        {
            if ((pictureId == 0)
                || (!defaultForType && string.IsNullOrEmpty(promoRef))
                || (defaultForType && string.IsNullOrEmpty(promoType)))
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.PromoPicture.ValidationMsg.Add"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }                

            var newPromoPicture = new PromoPicture()
            {
                PictureId = pictureId,
                IsDefaultForType = defaultForType,
                PromoReference = defaultForType ? string.Empty : promoRef,
                PromoTypeName = defaultForType ? promoType : string.Empty
            };

            _promoPictureService.Insert(newPromoPicture);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PictureDelete(int id)
        {
            if (id == 0)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.PromoPicture.ValidationMsg.Delete"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var promoPicture = _promoPictureService.RetrieveById(id);
            if (promoPicture == null)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.PromoPicture.ValidationMsg.Delete"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            _promoPictureService.Delete(promoPicture);
            return new NullJsonResult();
        }

        private string GetImageUrl(string baseImageName)
        {
            return string.Format("/Plugins/Widgets.QixolPromo/Themes/{1}/Content/images/{1}.png", _themeContext.WorkingThemeName, baseImageName);
        }

        private TransitionType ToTT(string ttName)
        {
            TransitionType tt = new TransitionType()
            {
                TransitionTypeID = ttName,
                TransitionTypeName = _localizationService.GetResource(ttName) // TODO: language
            };

            return tt;
        }

        #endregion
    }

    public class TransitionType
    {
        public string TransitionTypeName { get; set; }
        public string TransitionTypeID { get; set; }
    }
}
