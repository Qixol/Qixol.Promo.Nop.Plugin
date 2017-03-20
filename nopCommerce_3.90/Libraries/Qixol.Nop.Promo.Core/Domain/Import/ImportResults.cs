using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Qixol.Nop.Promo.Core.Domain.Import
{
    [Serializable]
    [XmlRoot("response")]
    public class ImportResults
    {
        [XmlElement("summary")]
        public ImportResultsSummary Summary { get; set; }

        [XmlAttribute("ref")]
        public string Reference { get; set; }

        /// <summary>
        /// Where an exception is encountered during processing, this will be captured here.
        /// </summary>
        [XmlIgnore]
        public Exception ProcessingException { get; set; }

        /// <summary>
        /// A list of the items imported.
        /// </summary>
        [XmlArray("items")]
        [XmlArrayItem("item")]
        public List<ImportResultItem> ResultItems { get; set; }

        public ImportResults()
        {
            ResultItems = new List<ImportResultItem>();
            Summary = new ImportResultsSummary();
        }

        /// <summary>
        /// Deserialize teh passed XML and return the instance.
        /// </summary>
        /// <param name="fromXml"></param>
        /// <returns></returns>
        public static ImportResults Retrieve(string fromXml)
        {
            ImportResults returnItem = null;
            using (XmlReader reader = XmlReader.Create(new StringReader(fromXml)))
            {
                reader.MoveToContent();
                returnItem = new XmlSerializer(typeof(ImportResults)).Deserialize(reader) as ImportResults;
            }
            return returnItem;
        }

        /// <summary>
        /// Get a formatted string of all messages against the response item.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string GetMessages()
        {
            if (this.Summary == null || this.Summary.Messages == null || this.Summary.Messages.Count == 0)
                return string.Empty;

            var strOut = new StringBuilder();
            this.Summary.Messages.ForEach(m =>
            {
                strOut.AppendLine(string.Format("{0} - {1}", m.Code, m.Message));
            });

            return strOut.ToString();
        }
    }
}
