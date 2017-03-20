using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Qixol.Nop.Promo.Core.Domain
{
    public partial class PromosFeedStatus
    {
        public int TotalNumberOfRecords { get; set; }
        public int NumberOfRecordsWritten { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime CompletedDateTime { get; set; }
        public string ErrorMessage { get; set; }
    }
}
