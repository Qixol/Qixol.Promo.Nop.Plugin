using Nop.Core.Data;
using Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.Banner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Banner
{
    public class PromoBannerService : IPromoBannerService
    {
        private readonly IRepository<PromoBanner> _bannerRepository;
        private readonly IRepository<PromoBannerPicture> _bannerPictureRepository;
        private readonly IRepository<PromoBannerWidgetZone> _bannerWidgetZoneRepository;
        private readonly IEventPublisher _eventPublisher;

        public PromoBannerService(IRepository<PromoBanner> bannerRepository,
                                  IRepository<PromoBannerPicture> bannerPictureRepository,
                                  IRepository<PromoBannerWidgetZone> bannerWidgetZoneRepository,
                                  IEventPublisher eventPublisher)
        {
            this._bannerRepository = bannerRepository;
            this._bannerPictureRepository = bannerPictureRepository;
            this._bannerWidgetZoneRepository = bannerWidgetZoneRepository;
            this._eventPublisher = eventPublisher;
        }

        public IQueryable<PromoBanner> RetrieveAllBanners()
        {
            return _bannerRepository.Table;
        }

        public IQueryable<PromoBannerPicture> RetrievePicturesForBanner(int bannerId)
        {
            return _bannerPictureRepository.Table.Where(sp => sp.PromoBannerId == bannerId).OrderBy(pic => pic.DisplaySequence);
        }

        public void InsertBanner(PromoBanner bannerToInsert)
        {
            _bannerRepository.Insert(bannerToInsert);
            _eventPublisher.EntityInserted<PromoBanner>(bannerToInsert);
        }

        public void InsertBannerPicture(PromoBannerPicture bannerPictureToInsert)
        {
            _bannerPictureRepository.Insert(bannerPictureToInsert);
            _eventPublisher.EntityInserted<PromoBannerPicture>(bannerPictureToInsert);
        }

        public void UpdateBanner(PromoBanner bannerToUpdate)
        {
            _bannerRepository.Update(bannerToUpdate);
            _eventPublisher.EntityUpdated<PromoBanner>(bannerToUpdate);
        }

        public void UpdateBannerPicture(PromoBannerPicture bannerPictureToUpdate)
        {
            _bannerPictureRepository.Update(bannerPictureToUpdate);
            _eventPublisher.EntityUpdated<PromoBannerPicture>(bannerPictureToUpdate);
        }

        public void DeleteBanner(PromoBanner bannerToDelete)
        {
            // Delete all pictures assoicated with banner
            var allPictures = RetrievePicturesForBanner(bannerToDelete.Id);
            allPictures.ToList().ForEach(sp =>
                {
                    DeleteBannerPicture(sp);
                });

            // Delete all widget zones associated with banner.
            var allWidgetZones = RetrieveWidgetZonesForBanner(bannerToDelete.Id);
            allWidgetZones.ToList().ForEach(zr =>
                {
                    DeleteBannerWidgetZone(zr);
                });

            // Now delete the actual banner
            _bannerRepository.Delete(bannerToDelete);
            _eventPublisher.EntityDeleted<PromoBanner>(bannerToDelete);
        }

        public void DeleteBannerPicture(PromoBannerPicture bannerPictureToDelete)
        {
            _bannerPictureRepository.Delete(bannerPictureToDelete);
            _eventPublisher.EntityDeleted<PromoBannerPicture>(bannerPictureToDelete);
        }


        public IQueryable<PromoBannerWidgetZone> RetrieveWidgetZonesForBanner(int bannerId)
        {
            return _bannerWidgetZoneRepository.Table.Where(zr => zr.PromoBannerId == bannerId);
        }

        public IQueryable<PromoBannerWidgetZone> RetrieveAllEnabledWidgetZones()
        {
            return  (from zr in _bannerWidgetZoneRepository.Table
                     join s in _bannerRepository.Table
                        on zr.PromoBannerId equals s.Id
                     where s.Enabled
                     select zr);
        }

        public void InsertBannerWidgetZone(PromoBannerWidgetZone bannerWidgetZoneToInsert)
        {
            _bannerWidgetZoneRepository.Insert(bannerWidgetZoneToInsert);
            _eventPublisher.EntityInserted<PromoBannerWidgetZone>(bannerWidgetZoneToInsert);
        }

        public void DeleteBannerWidgetZone(PromoBannerWidgetZone bannerWidgetZoneToDelete)
        {
            _bannerWidgetZoneRepository.Delete(bannerWidgetZoneToDelete);
            _eventPublisher.EntityDeleted<PromoBannerWidgetZone>(bannerWidgetZoneToDelete);
        }


        public PromoBanner RetrieveBannerById(int bannerId)
        {
            return _bannerRepository.Table.Where(s => s.Id == bannerId).FirstOrDefault();
        }

        public PromoBannerPicture RetrieveBannerPictureById(int bannerPictureId)
        {
            return _bannerPictureRepository.Table.Where(sp => sp.Id == bannerPictureId).FirstOrDefault();
        }

        public PromoBannerWidgetZone RetrieveBannerWidgetZoneById(int bannerWidgetZoneId)
        {
            return _bannerWidgetZoneRepository.Table.Where(zr => zr.Id == bannerWidgetZoneId).FirstOrDefault();
        }
    }
}
