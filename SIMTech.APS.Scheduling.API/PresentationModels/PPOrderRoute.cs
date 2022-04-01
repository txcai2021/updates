using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    public class PporderRoute
    {
        
        public int Ppid { get; set; }

        public string PartNo { get; set; }

        public int RouteId { get; set; }
        public int SeqNo { get; set; }

        public string CentreId { get; set; }

        public string MacCode { get; set; }

        public string MacType { get; set; }
        public string Remark { get; set; }
        public int? MacGroup { get; set; }

        public string AttributeGroup { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public double? Qty { get; set; }
        public double? Duration { get; set; }

        public int? ScheduleId { get; set; }

        public string MaterialIdList { get; set; }
    }

}
