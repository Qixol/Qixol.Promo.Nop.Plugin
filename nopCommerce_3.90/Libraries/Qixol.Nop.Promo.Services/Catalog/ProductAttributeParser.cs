using global::Nop.Core.Domain.Catalog;
using global::Nop.Services.Catalog;
using global::Nop.Services.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qixol.Nop.Promo.Core.Domain.Products;
using Qixol.Nop.Promo.Core.Domain.ProductAttributeConfig;
using global::Nop.Core.Domain.Tax;
using global::Nop.Core.Domain.Vendors;
using Qixol.Promo.Integration.Lib.Import;
using Nop.Core.Infrastructure;
using Qixol.Nop.Promo.Services.ProductAttributeConfig;
using Nop.Services.Vendors;
using Nop.Services.Tax;
using Nop.Data;

namespace Qixol.Nop.Promo.Services.Catalog
{
    public partial class ProductAttributeParser : global::Nop.Services.Catalog.ProductAttributeParser, IProductAttributeParser
    {
        #region fields

        private readonly IDbContext _context;
        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region constructor

        public ProductAttributeParser(IDbContext context, IProductAttributeService productAttributeService) : base(context, productAttributeService)
        {
            this._context = context;
            this._productAttributeService = productAttributeService;
        }

        #endregion

        #region methods

        /// <summary>
        /// Generate all combinations
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Attribute combinations in XML format</returns>
        public override IList<string> GenerateAllCombinations(Product product, bool ignoreNonCombinableAttributes = false)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var allProductAttributMappings = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            if (ignoreNonCombinableAttributes)
            {
                allProductAttributMappings = allProductAttributMappings.Where(x => !x.IsNonCombinable()).ToList();
            }

            var allPossibleAttributeCombinations = new List<List<ProductAttributeMapping>>();

            var requiredProductAttributeMappings = allProductAttributMappings.Where(a => a.IsRequired).ToList();
            var optionalProductAttributeMappings = allProductAttributMappings.Where(a => !a.IsRequired).ToList();

            var requiredCombination = new List<ProductAttributeMapping>();

            requiredProductAttributeMappings.ForEach(rpam =>
            {
                requiredCombination.Add(rpam);
            });

            if (requiredCombination.Any())
                allPossibleAttributeCombinations.Add(requiredCombination);

            for (int counter = 0; counter < (1 << optionalProductAttributeMappings.Count); ++counter)
            {
                var optionalCombination = new List<ProductAttributeMapping>();
                for (int i = 0; i < optionalProductAttributeMappings.Count; ++i)
                {
                    if ((counter & (1 << i)) == 0)
                    {
                        optionalCombination.Add(optionalProductAttributeMappings[i]);
                    }
                }
                if (optionalCombination.Any())
                {
                    optionalCombination.AddRange(requiredCombination);
                    allPossibleAttributeCombinations.Add(optionalCombination);
                }
            }

            var allAttributesXml = new List<string>();
            foreach (var combination in allPossibleAttributeCombinations)
            {
                var attributesXml = new List<string>();
                foreach (var pam in combination)
                {
                    if (!pam.ShouldHaveValues())
                        continue;

                    var attributeValues = _productAttributeService.GetProductAttributeValues(pam.Id);
                    if (!attributeValues.Any())
                        continue;

                    //checkboxes could have several values ticked
                    var allPossibleCheckboxCombinations = new List<List<ProductAttributeValue>>();
                    if (pam.AttributeControlType == AttributeControlType.Checkboxes ||
                        pam.AttributeControlType == AttributeControlType.ReadonlyCheckboxes)
                    {
                        for (int counter = 0; counter < (1 << attributeValues.Count); ++counter)
                        {
                            var checkboxCombination = new List<ProductAttributeValue>();
                            for (int i = 0; i < attributeValues.Count; ++i)
                            {
                                if ((counter & (1 << i)) == 0)
                                {
                                    checkboxCombination.Add(attributeValues[i]);
                                }
                            }

                            allPossibleCheckboxCombinations.Add(checkboxCombination);
                        }
                    }

                    if (!attributesXml.Any())
                    {
                        //first set of values
                        if (pam.AttributeControlType == AttributeControlType.Checkboxes ||
                            pam.AttributeControlType == AttributeControlType.ReadonlyCheckboxes)
                        {
                            //checkboxes could have several values ticked
                            foreach (var checkboxCombination in allPossibleCheckboxCombinations)
                            {
                                var tmp1 = "";
                                foreach (var checkboxValue in checkboxCombination)
                                {
                                    tmp1 = AddProductAttribute(tmp1, pam, checkboxValue.Id.ToString());
                                }
                                if (!String.IsNullOrEmpty(tmp1))
                                {
                                    attributesXml.Add(tmp1);
                                }
                            }
                        }
                        else
                        {
                            //other attribute types (dropdownlist, radiobutton, color squares)
                            foreach (var attributeValue in attributeValues)
                            {
                                var tmp1 = AddProductAttribute("", pam, attributeValue.Id.ToString());
                                attributesXml.Add(tmp1);
                            }
                        }
                    }
                    else
                    {
                        //next values. let's "append" them to already generated attribute combinations in XML format
                        var attributesXmlTmp = new List<string>();
                        if (pam.AttributeControlType == AttributeControlType.Checkboxes ||
                            pam.AttributeControlType == AttributeControlType.ReadonlyCheckboxes)
                        {
                            //checkboxes could have several values ticked
                            foreach (var str1 in attributesXml)
                            {
                                foreach (var checkboxCombination in allPossibleCheckboxCombinations)
                                {
                                    var tmp1 = str1;
                                    foreach (var checkboxValue in checkboxCombination)
                                    {
                                        tmp1 = AddProductAttribute(tmp1, pam, checkboxValue.Id.ToString());
                                    }
                                    if (!String.IsNullOrEmpty(tmp1))
                                    {
                                        attributesXmlTmp.Add(tmp1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //other attribute types (dropdownlist, radiobutton, color squares)
                            foreach (var attributeValue in attributeValues)
                            {
                                foreach (var str1 in attributesXml)
                                {
                                    var tmp1 = AddProductAttribute(str1, pam, attributeValue.Id.ToString());
                                    attributesXmlTmp.Add(tmp1);
                                }
                            }
                        }
                        attributesXml.Clear();
                        attributesXml.AddRange(attributesXmlTmp);
                    }
                }
                allAttributesXml.AddRange(attributesXml);
            }

            //validate conditional attributes (if specified)
            //minor workaround:
            //once it's done (validation), then we could have some duplicated combinations in result
            //we don't remove them here (for performance optimization) because anyway it'll be done in the "GenerateAllAttributeCombinations" method of ProductController
            for (int i = 0; i < allAttributesXml.Count; i++)
            {
                var attributesXml = allAttributesXml[i];
                foreach (var attribute in allProductAttributMappings)
                {
                    var conditionMet = IsConditionMet(attribute, attributesXml);
                    if (conditionMet.HasValue && !conditionMet.Value)
                    {
                        allAttributesXml[i] = RemoveProductAttribute(attributesXml, attribute);
                    }
                }
            }
            return allAttributesXml;
        }

        #endregion
    }
}
