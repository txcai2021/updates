using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.WorkOrder.API.PresentationModels
{
    public class SalesOrderStatus
    {
        public string SalesOrderNumber { get; set; }
        public int LineNo { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }

    }
}
