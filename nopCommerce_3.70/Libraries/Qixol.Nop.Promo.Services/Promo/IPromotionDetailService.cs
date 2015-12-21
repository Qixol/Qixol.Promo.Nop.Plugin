using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Promo
{
    public interface IPromoDetailService
    {
        void Insert(PromoDetail itemToInsert);

        void Delete(PromoDetail itemToDelete);

        PromoDetail Retrieve(int id);

        PromoDetail RetrieveByPromoId(int promoId);

        IQueryable<PromoDetail> RetrieveAll();

    }
}
