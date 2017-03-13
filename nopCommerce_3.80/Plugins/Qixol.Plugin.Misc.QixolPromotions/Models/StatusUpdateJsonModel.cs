using System;

namespace Qixol.Plugin.Misc.Promo.Models
{
    public partial class StatusUpdateJsonModel
    {
        public bool is_running { get; set; }

        public int total_records { get; set; }
        public int written_records { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string error_message { get; set; }
    }
}
