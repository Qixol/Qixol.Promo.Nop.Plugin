using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Qixol.Nop.Promo.Core.Domain.Import
{
    [Serializable]
    public class ImportResultsMessage
    {
        [XmlAttribute("code")]
        public string Code { get; set; }

        [XmlText]
        public string Message { get; set; }
    }
}
