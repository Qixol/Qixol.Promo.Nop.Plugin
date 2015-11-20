using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Qixol.Nop.Promo.Core.Domain.Banner;
using Qixol.Nop.Promo.Services.Banner;
using Qixol.Plugin.Widgets.Promo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qixol.Plugin.Widgets.Promo.Controllers
{
    [AdminAuthorize]
    public class PromoBannerController : BasePluginController
    {
        #region fields 

        private readonly IPromoBannerService _promoBannerService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;

        #endregion

        #region constructor

        public PromoBannerController(IPromoBannerService promoBannerService,
                                     ILocalizationService localizationService,
                                     IPictureService pictureService)
        {
            this._promoBannerService = promoBannerService;
            this._localizationService = localizationService;
            this._pictureService = pictureService;
        }

        #endregion

        #region methods

        public ActionResult Index(int? id)
        {
            if (!id.HasValue || id.Value == 0)
                throw new Exception("");

            var banner = _promoBannerService.RetrieveBannerById(id.Value);

            var model = new PromoBannerEditModel() { BannerId = banner.Id, BannerName = banner.Name, BannerTransitionType = banner.TransitionType };
            model.AddPromoBannerPictureModel.BannerId = id.Value;
            model.AddPromoBannerWidgetZoneModel.BannerId = id.Value;

            var widgetZones = WidgetZonesHelper.GetWidgetZonesForBanners();
            model.AddPromoBannerWidgetZoneModel.AvailableWidgetZones = widgetZones.Select(s => new SelectListItem() { Text = s.DisplayName, Value = s.Name }).OrderBy(ob => ob.Text).ToList();
            model.AddPromoBannerWidgetZoneModel.AvailableWidgetZones.Add(new SelectListItem() { Text = "custom", Value = "custom" });
            model.AddPromoBannerWidgetZoneModel.FirstWidgetZone = widgetZones.FirstOrDefault().Name;

            return View("ConfigureBanner", model);
        }

        #endregion

        #region Banner Pictures

        public ActionResult BannerPicturesList(DataSourceRequest command, int? id)
        {
            if(!id.HasValue || id.Value == 0)
                return new EmptyResult();

            var bannerPictures = _promoBannerService.RetrievePicturesForBanner(id.Value);
            var modelItems = bannerPictures.Select(sp => new PromoBannerPictureModel()
                    {
                        Id = sp.Id,
                        Comment = sp.Comment,
                        DisplaySequence = sp.DisplaySequence,
                        PictureId = sp.PictureId,
                        PromoReference = sp.PromoReference,
                        Url = sp.Url,
                        BannerId = id.Value
                    }).ToList();

            if (modelItems != null && modelItems.Count() > 0)
            {
                modelItems.ToList().ForEach(i =>
                    {
                        i.PictureUrl = _pictureService.GetPictureUrl(i.PictureId);
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
        public ActionResult BannerPictureAdd(int bannerId, int pictureId, string promoRef, int displaySequence, string url, string comment)
        {
            if (pictureId == 0)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.BannerPicture.ValidationMsg.Add"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var newBannerPicture = new PromoBannerPicture()
            {
                PictureId = pictureId,
                PromoReference = promoRef,
                PromoBannerId = bannerId,
                Comment = comment,
                DisplaySequence = displaySequence,
                Url = url
            };

            _promoBannerService.InsertBannerPicture(newBannerPicture);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BannerPictureEdit(PromoBannerPictureModel bannerPictureModel)
        {
            if ((bannerPictureModel == null)
                || (bannerPictureModel.Id == 0))               
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.BannerPicture.ValidationMsg.Edit"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var bannerPicture = _promoBannerService.RetrieveBannerPictureById(bannerPictureModel.Id);
            if (bannerPicture == null)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.BannerPicture.ValidationMsg.Edit"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            bannerPicture.PromoReference = bannerPictureModel.PromoReference;
            bannerPicture.Url = bannerPictureModel.Url;
            bannerPicture.DisplaySequence = bannerPictureModel.DisplaySequence;
            bannerPicture.Comment = bannerPictureModel.Comment;

            _promoBannerService.UpdateBannerPicture(bannerPicture);
            return new NullJsonResult();
        }

        public ActionResult BannerPictureDelete(PromoBannerPictureModel bannerPictureModel)
        {
            if ((bannerPictureModel == null) || (bannerPictureModel.Id == 0))
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.BannerPicture.ValidationMsg.Delete"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var bannerPicture = _promoBannerService.RetrieveBannerPictureById(bannerPictureModel.Id);
            if (bannerPicture == null)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.BannerPicture.ValidationMsg.Delete"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            _promoBannerService.DeleteBannerPicture(bannerPicture);

            return new NullJsonResult();
        }

        #endregion

        #region Banner Widget Zones

        public ActionResult BannerWidgetZonesList(DataSourceRequest command, int? id)
        {
            if (!id.HasValue || id.Value == 0)
                return new EmptyResult();

            var bannerWidgetZones = _promoBannerService.RetrieveWidgetZonesForBanner(id.Value);
            var modelItems = bannerWidgetZones.Select(sp => new PromoBannerWidgetZoneModel()
            {
                Id = sp.Id,
                WidgetZone = sp.WidgetZoneSystemName
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = modelItems,
                Total = modelItems != null ? modelItems.Count() : 0
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult BannerWidgetZoneAdd(int bannerId, string widgetZone)
        {
            if (bannerId == 0 || string.IsNullOrEmpty(widgetZone.Trim()))
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.BannerWidgets.ValidationMsg.Add"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var existingWidgetZones = _promoBannerService.RetrieveWidgetZonesForBanner(bannerId);
            if (existingWidgetZones != null && existingWidgetZones.Count() > 0)
            {
                // It already exists - so just return...  To be reviewed.
                if(existingWidgetZones.Any(wz => string.Compare(wz.WidgetZoneSystemName, widgetZone, true) == 0))
                    return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
            }

            var newBannerWidgetZone = new PromoBannerWidgetZone()
            {
                PromoBannerId = bannerId,
                WidgetZoneSystemName = widgetZone
            };

            _promoBannerService.InsertBannerWidgetZone(newBannerWidgetZone);

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BannerWidgetZoneDelete(PromoBannerWidgetZoneModel bannerWidgetZoneModel)
        {
            if ((bannerWidgetZoneModel == null)
                || (bannerWidgetZoneModel.Id == 0))
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.BannerWidgets.ValidationMsg.Delete"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var bannerWidgetZone = _promoBannerService.RetrieveBannerWidgetZoneById(bannerWidgetZoneModel.Id);
            if (bannerWidgetZone == null)
            {
                ModelState.AddModelError(string.Empty, _localizationService.GetResource("Plugins.Widgets.QixolPromo.BannerWidgets.ValidationMsg.Delete"));
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            _promoBannerService.DeleteBannerWidgetZone(bannerWidgetZone);

            return new NullJsonResult();
        }

        #endregion

    }
}
