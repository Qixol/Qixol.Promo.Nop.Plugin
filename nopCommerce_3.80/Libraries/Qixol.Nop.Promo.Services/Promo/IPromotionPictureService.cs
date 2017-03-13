using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.Promo
{
    public interface IPromoPictureService
    {
        IQueryable<PromoPicture> RetrieveAll();

        PromoPicture RetrieveById(int id);

        PromoPicture RetrieveForPromo(string promoReference, string promoTypeName);

        void Insert(PromoPicture pictureDetails);

        void Delete(PromoPicture pictureDetails);

    }
}
