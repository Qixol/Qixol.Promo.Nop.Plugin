using Qixol.Nop.Promo.Core.Domain.Banner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Banner
{
    public interface IPromoBannerService
    {
        IQueryable<PromoBanner> RetrieveAllBanners();

        IQueryable<PromoBannerPicture> RetrievePicturesForBanner(int bannerId);

        IQueryable<PromoBannerWidgetZone> RetrieveWidgetZonesForBanner(int bannerId);

        IQueryable<PromoBannerWidgetZone> RetrieveAllEnabledWidgetZones();

        PromoBanner RetrieveBannerById(int bannerId);

        PromoBannerPicture RetrieveBannerPictureById(int bannerPictureId);

        PromoBannerWidgetZone RetrieveBannerWidgetZoneById(int bannerWidgetZoneId);

        void InsertBanner(PromoBanner bannerToInsert);

        void InsertBannerPicture(PromoBannerPicture bannerPictureToInsert);

        void InsertBannerWidgetZone(PromoBannerWidgetZone bannerWidgetZoneToInsert);

        void UpdateBanner(PromoBanner bannerToUpdate);

        void UpdateBannerPicture(PromoBannerPicture bannerPictureToUpdate);

        void DeleteBanner(PromoBanner bannerToDelete);

        void DeleteBannerPicture(PromoBannerPicture bannerPictureToDelete);

        void DeleteBannerWidgetZone(PromoBannerWidgetZone bannerWidgetZoneToDelete);

    }
}
