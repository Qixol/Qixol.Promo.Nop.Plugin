using Qixol.Nop.Promo.Core.Domain.Promo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Products
{
    public class ProductPromotions
    {
        /// <summary>
        /// The product Id for the product being validated
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// The base image url to be used for the product.
        /// </summary>
        public string BaseImageUrl { get; set; }

        /// <summary>
        /// A list of all validated promos for this item (though further checking will be performed to see which ones
        /// are available at this moment in time, for this store/customer group).
        /// </summary>
        public List<ValidatedPromo> PromoDetails { get; set; }

        /// <summary>
        /// Flag indicating whether the item should be regarded as having promos available.
        /// </summary>
        public bool HasPromo
        {
            get
            {
                return (this.PromoDetails != null 
                        && this.PromoDetails.Count > 0
                            && this.PromoDetails.Any(pd => pd.ValidToDisplay));
            }
        }

        /// <summary>
        /// A list of promos which have been validated as available now for the product.
        /// </summary>
        public List<ValidatedPromo> PromosToDisplay
        {
            get
            {
                if (PromoDetails == null || PromoDetails.Count == 0)
                    return new List<ValidatedPromo>();
                else
                    return PromoDetails.Where(pd => pd.ValidToDisplay).ToList();
            }
        }

        public ProductPromotions()
        {
            PromoDetails = new List<ValidatedPromo>();
        }

    }
}
