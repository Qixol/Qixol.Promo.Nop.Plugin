using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Qixol.Nop.Promo.Core.Domain.Import
{
    public enum ImportResultItemType
    {
        FailedValidation = 0,
        Inserted = 1,
        Updated = 2,
        Deleted = 3,
        Error = 4,
        NoAction = 5
    }

    [Serializable, XmlRoot("item")]
    public class ImportResultItem
    {

        /// <summary>
        /// The result action for this item
        /// </summary>
        [XmlAttribute("result")]
        public ImportResultItemType Result { get; set; }

        /// <summary>
        /// A message to be displayed indicating the result of this import item
        /// </summary>
        [XmlElement("message")]
        public ImportResultsMessage Message { get; set; }

        [XmlAttribute("productcode")]
        public string ProductCode { get; set; }

        [XmlAttribute("variantcode")]
        public string VariantCode { get; set; }

        [XmlAttribute("barcode")]
        public string Barcode { get; set; }

        /// <summary>
        /// The entity which owns the attribute
        /// </summary>
        [XmlAttribute("entityname")]
        public string EntityName { get; set; }

        /// <summary>
        /// The attribute name to import the value for
        /// </summary>
        [XmlAttribute("name")]
        public string AttributeName { get; set; }

        /// <summary>
        /// The value of the attribute
        /// </summary>
        [XmlElement("key")]
        public string Value { get; set; }

        public ImportResultItem()
        {
            Message = new ImportResultsMessage();
        }
    }
}
