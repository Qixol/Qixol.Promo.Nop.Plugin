using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Services
{
    public interface IProductPromotionService
    {
        // TODO: BasketCriteriaItems only made public because the banner code needs rewriting
        List<KeyValuePair<string, string>> BasketCriteriaItems();

        ProductPromotions PromotionsForProduct(int productId);

        string GetImageUrl(string baseImageName, string themeName);

        string GetPromoImageUrl(string promoReference, string promoType, string promoBaseImageName);

        string GetSingleImageUrl(List<ValidatedPromo> promosToDisplay, string baseUrl);

    }
}
