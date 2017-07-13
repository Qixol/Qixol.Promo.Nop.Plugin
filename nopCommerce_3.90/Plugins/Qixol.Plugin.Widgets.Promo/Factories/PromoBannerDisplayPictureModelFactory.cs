using Nop.Services.Media;
using Qixol.Nop.Promo.Core.Domain.Banner;
using Qixol.Plugin.Widgets.Promo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Factories
{
    public partial class PromoBannerDisplayPictureModelFactory : IPromoBannerDisplayPictureModelFactory
    {
        #region private variables

        private readonly IPictureService _pictureService;

        #endregion

        #region constructor

        public PromoBannerDisplayPictureModelFactory(IPictureService pictureService)
        {
            this._pictureService = pictureService;
        }

        #endregion

        #region utilities
        #endregion

        #region methods

        public PromoBannerDisplayPictureModel PreparePromoBannerDisplayPictureModel(PromoBannerPicture promoBannerPicture, string transitionType)
        {
            var returnItem = new PromoBannerDisplayPictureModel()
            {
                Comment = promoBannerPicture.Comment,
                DisplaySequence = promoBannerPicture.DisplaySequence,
                PictureId = promoBannerPicture.PictureId,
                Url = promoBannerPicture.Url,
                TransitionType = transitionType
            };

            returnItem.PictureUrl = _pictureService.GetPictureUrl(returnItem.PictureId);
            return returnItem;
        }

        #endregion
    }
}
