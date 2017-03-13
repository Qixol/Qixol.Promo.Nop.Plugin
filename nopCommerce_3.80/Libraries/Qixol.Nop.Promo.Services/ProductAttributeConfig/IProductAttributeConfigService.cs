using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Services.ProductAttributeConfig
{
    public interface IProductAttributeConfigService
    {

        void Insert(ProductAttributeConfigItem item);

        void Update(ProductAttributeConfigItem item);

        IQueryable<ProductAttributeConfigItem> RetrieveAll();

    }
}
