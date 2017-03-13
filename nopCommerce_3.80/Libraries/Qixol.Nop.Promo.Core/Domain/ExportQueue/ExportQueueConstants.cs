using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.ExportQueue
{
    public class ExportQueueAction
    {
        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string All = "All";
    }

    public class ExportQueueStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Ok = "Success";
        public const string ContentFailure = "ContentFailure";
        public const string CommsFailure = "CommsFailure";
    }
}
