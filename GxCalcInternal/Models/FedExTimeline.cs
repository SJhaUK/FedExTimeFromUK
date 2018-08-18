using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace GXCalcTool.Models
{
    public class FedExTimeline
    {
        public string Docs { get; set; }
        public string Goods { get; set; }
        public string Time { get; set; }
        public string TimeForAmerica { get; set; }
        public string TimeForROW { get; set; }
        public string AdditionalDays { get; set; }
        public string AdditionalDays_Uk { get; set; }
        public string DepotName { get; set; }
    }
}
