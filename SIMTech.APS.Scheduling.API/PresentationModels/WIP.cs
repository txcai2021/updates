using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{

    public class WIPResponse
    {
        public string Message { get; set; }
        public string StatusCode { get; set; }
        public List<WIP> Data { get; set; }
    }


    public class WIP
    {
        public string WorkOrderNumber { get; set; }
        public string Status { get; set; }
        public DateTime? ActualRecDate { get; set; }
        public int ActualRecQty { get; set; }
        public DateTime? ProdStartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int CompletedQty { get; set; }
        public DateTime? OutstandingDate { get; set; }
        public int OutstandingQty { get; set; }
        public DateTime? ScrapDate { get; set; }
        public int ScrapQty { get; set; }
        public string Routeid { get; set; }
        public int Sequence { get; set; }
        public string OperationName { get; set; }
        public string MachineName { get; set; }
        public int ProcOpSeq { get; set; }
    }
}
