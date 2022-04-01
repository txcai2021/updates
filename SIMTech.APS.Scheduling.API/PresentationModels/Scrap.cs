using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    public class Scrap
    {
        public int ScrapQty { get; set; }
        public int ProcOpSeq { get; set; }
        public string WOStatus { get; set; }
        public string WOID { get; set; }

    }

    public class ScrapRequest
    {
        public string WOID { get; set; }
        public int ProcOpSeq { get; set; }

    }
}
