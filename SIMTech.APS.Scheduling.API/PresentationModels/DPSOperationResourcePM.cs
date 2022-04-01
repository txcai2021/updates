using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    public class DPSOperationResourcePM
    {
        public DPSOperationResourcePM()
        {

        }

       
        public int OperationResourceId { get; set; }

        public int OperationID { get; set; }
        public int Version { get; set; }
        public string OperationName { get; set; }
        public string CodeDetailType { get; set; }
        public int? ResourceID { get; set; }
        public double Duration { get; set; }
        public double? DurationPer { get; set; }
        public string DurationType { get; set; }
        public double UnitRunTime { get; set; }
        public bool? Isdefault { get; set; }
    }
}
