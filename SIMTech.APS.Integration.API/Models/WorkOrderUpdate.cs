using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Integration.RabbitMQ.Models
{
    public class WorkOrderUpdate
    {
        public string WOID { get; set; }
        public int OutStandingQty { get; set; }
        public int CompletedQty { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string WOStatus { get; set; }
    }

    public class ProcessUpdate
    {
        public string WOID { get; set; }
        public string ProcessName { get; set; }
        public int OpSeq { get; set; }
        public int OutstandingQty { get; set; }
        public int ScrapQty { get; set; }
        public int CompletedQty { get; set; }
        public DateTime? ProdStartDate { get; set; }
        public DateTime? ProdEndDate { get; set; }
        public string WOProcessStatus { get; set; }
        public DateTime? CompletedDate { get; set; }

    }
}
