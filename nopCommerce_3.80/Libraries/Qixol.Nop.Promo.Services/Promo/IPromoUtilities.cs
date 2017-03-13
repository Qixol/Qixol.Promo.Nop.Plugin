using global::Nop.Core.Domain.Catalog;
using global::Nop.Core.Domain.Customers;
using global::Nop.Core.Domain.Shipping;
using Qixol.Nop.Promo.Core.Domain;
using Qixol.Nop.Promo.Core.Domain.Import;
using Qixol.Nop.Promo.Core.Domain.Promo;
using Qixol.Promo.Integration.Lib.Basket;
using Qixol.Promo.Integration.Lib.Export;
using Qixol.Promo.Integration.Lib.Import;
using System.Collections.Generic;

namespace Qixol.Nop.Promo.Services.Promo
{
    public interface IPromoUtilities
    {
        AttributeValuesImportResponse ImportAttributesToPromoService(AttributeValuesImportRequest attributesImport);

        ProductImportResponse ImportProductsToPromoService(ProductImportRequest qixolPromosImport);

        HierarchyValuesImportResponse ImportHierarchyTopromoService(HierarchyValuesImportRequest hierarchyImport);

        BasketResponse GetBasketResponse();

        PromotionDetailsByProductResponse ExportPromotionsForProducts(PromotionDetailsByProductRequest request);

        PromotionDetailsResponse ExportPromotionsForBasketAndDelivery(PromotionDetailsRequest request);
    }
}
