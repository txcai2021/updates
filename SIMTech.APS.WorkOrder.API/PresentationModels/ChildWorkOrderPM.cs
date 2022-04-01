using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.WorkOrder.API.PresentationModels
{
    public class ChildWorkOrderPM
    {
      
        public double ActualProdQty { get; set; }
        public string ChildWOID { get; set; }
        public string ParentWOID { get; set; }
        public int ProcOPseq { get; set; }
        public int RouteID { get; set; }
        public string Type { get; set; }
    }

    public class ReworkOrderPM
    {
      
        public int ReworkRouteID { get; set; }
        public string WOID { get; set; }
    }
}
