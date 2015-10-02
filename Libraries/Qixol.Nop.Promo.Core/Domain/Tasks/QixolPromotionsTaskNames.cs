using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qixol.Nop.Promo.Core.Domain.Tasks
{
    public static partial class PromoTaskNames
    {
        public static string DataFeedTask { get { return "Qixol.Plugin.Misc.Promo.Tasks.DataFeedTask, Qixol.Plugin.Misc.Promo"; } }
        public static string PromoSyncTask { get { return "Qixol.Plugin.Misc.Promo.Tasks.PromoSyncTask, Qixol.Plugin.Misc.Promo"; } }
    }
}
