using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    public class DPSReport1
    {
        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public DateTime? EndDate { get; set; }
        public string LoginId { get; set; }
        public string MachineName { get; set; }
        public int ReportTypeId { get; set; }
        public int ScheduleId { get; set; }
        public string Search1 { get; set; }
        public string Search2 { get; set; }
        public string SearchCustomer { get; set; }
        public string SearchPart { get; set; }
        public string SearchPO { get; set; }
        public int SearchTypeId { get; set; }
        public string SearchWO { get; set; }
        public DateTime? StartDate { get; set; }
        public string WorkOrderNumber { get; set; }
    }
}
