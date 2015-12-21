using global::Nop.Core.Data;
using global::Nop.Services.Events;
using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Promo
{
    public class PromoPictureService : IPromoPictureService
    {
        private readonly IRepository<PromoPicture> _repository;
        private readonly IEventPublisher _eventPublisher;

        public PromoPictureService(IRepository<PromoPicture> repository,
                                      IEventPublisher eventPublisher)
        {
            this._repository = repository;
            this._eventPublisher = eventPublisher;
        }

        public IQueryable<PromoPicture> RetrieveAll()
        {
            return this._repository.Table;
        }

        public PromoPicture RetrieveById(int id)
        {
            return this._repository.Table.Where(t => t.Id == id).FirstOrDefault();
        }

        public PromoPicture RetrieveForPromo(string promoReference, string promoTypeName)
        {
            return this._repository.Table.Where(t => (!string.IsNullOrEmpty(promoReference) && t.PromoReference == promoReference) || (t.IsDefaultForType == true && t.PromoTypeName == promoTypeName))
                                         .OrderByDescending(ob => ob.PromoReference)
                                         .FirstOrDefault();                                         
        }

        public void Insert(PromoPicture pictureDetails)
        {
            this._repository.Insert(pictureDetails);
            _eventPublisher.EntityInserted<PromoPicture>(pictureDetails);
        }

        public void Delete(PromoPicture pictureDetails)
        {
            this._repository.Delete(pictureDetails);
            _eventPublisher.EntityDeleted<PromoPicture>(pictureDetails);
        }
    }
}
