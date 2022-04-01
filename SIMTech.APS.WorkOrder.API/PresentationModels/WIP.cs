using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.WorkOrder.API.PresentationModels
{
   

    public class WIP
    {
        public DateTime ActualRecDate { get; set; }
        public int ActualRecQty { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int CompletedQty { get; set; }
        public string MachineName { get; set; }
        public string OperationName { get; set; }
        public DateTime? OutstandingDate { get; set; }
        public int OutstandingQty { get; set; }
        public DateTime? ProdStartDate { get; set; }
        public int RouteId { get; set; }
        public DateTime? ScrapDate { get; set; }
        public int ScrapQty { get; set; }
        public int Sequence { get; set; }
        public string Status { get; set; }
        public string WorkOrderNumber { get; set; }
    }
}
