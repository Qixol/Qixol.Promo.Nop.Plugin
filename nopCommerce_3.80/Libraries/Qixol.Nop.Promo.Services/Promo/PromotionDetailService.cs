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
    public class PromoDetailService : IPromoDetailService
    {
        private readonly IRepository<PromoDetail> _repository;
        private readonly IEventPublisher _eventPublisher;

        public PromoDetailService(IRepository<PromoDetail> repository,
                                      IEventPublisher eventPublisher)
        {
            this._repository = repository;
            this._eventPublisher = eventPublisher;
        }

        public void Insert(PromoDetail itemToInsert)
        {
            _repository.Insert(itemToInsert);
            _eventPublisher.EntityInserted<PromoDetail>(itemToInsert);
        }

        public void Delete(PromoDetail itemToDelete)
        {
            _repository.Delete(itemToDelete);
            _eventPublisher.EntityDeleted<PromoDetail>(itemToDelete);
        }

        public PromoDetail Retrieve(int id)
        {
            return _repository.Table.Where(p => p.Id == id).FirstOrDefault();
        }

        public PromoDetail RetrieveByPromoId(int promoId)
        {
            return _repository.Table.Where(p => p.PromoId == promoId).FirstOrDefault();
        }

        public IQueryable<PromoDetail> RetrieveAll()
        {
            return _repository.Table;
        }
    }
}
