using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    public class WIPRequest
    {
        public List<string> WOStatus { get; set; }
        public string WOID { get; set; }

    }
}
