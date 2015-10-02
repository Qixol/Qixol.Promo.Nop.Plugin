using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Qixol.Nop.Promo.Core.Domain.Import
{
    [Serializable]
    public class ImportResultsSummary
    {
        [XmlAttribute("result")]
        public bool ProcessedSuccessfully { get; set; }

        [XmlAttribute("errors")]
        public int ErrorCount { get; set; }


        [XmlAttribute("inserted")]
        public int InsertedCount { get; set; }

        [XmlAttribute("updated")]
        public int UpdatedCount { get; set; }

        [XmlAttribute("deleted")]
        public int DeletedCount { get; set; }

        /// <summary>
        /// A list of messages to be returned to the basket as generated during processing of promos.
        /// </summary>
        [XmlArray("messages")]
        [XmlArrayItem("message")]
        public List<ImportResultsMessage> Messages { get; set; }

        public ImportResultsSummary()
        {
            Messages = new List<ImportResultsMessage>();
        }
    }
}
