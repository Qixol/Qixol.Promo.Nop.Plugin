using Nop.Services.Catalog;
using Qixol.Plugin.Widgets.Promo.Models;
using Qixol.Promo.Integration.Lib.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Plugin.Widgets.Promo.Factories
{
    public partial class DiscountRangeModelFactory : IDiscountRangeModelFactory
    {
        #region private variables

        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region constructor

        public DiscountRangeModelFactory(IPriceFormatter priceFormatter)
        {
            this._priceFormatter = priceFormatter;
        }

        #endregion

        #region utilities

        #endregion

        #region methods

        public DiscountRangeModel PrepareDiscountRangeModel(ExportPromotionDetailsDiscountRange entity)
        {
            if (entity == null)
                return null;

            var model = new DiscountRangeModel()
            {
                Discount = entity.DiscountPercentage > decimal.Zero ? string.Format("{0:#}%", entity.DiscountPercentage) : _priceFormatter.FormatPrice(entity.DiscountValuePerItem),
                Range = entity.MinimumQuantity > decimal.Zero ? string.Format("{0:#}+", entity.MinimumQuantity) : string.Format("{0}+", _priceFormatter.FormatPrice(entity.MinimumSpend))
            };

            return model;
        }

        #endregion
    }
}
