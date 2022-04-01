using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    public class ScheduleCPPS24
    {
        public string WorkOrderNumber { get; set; }
        public string PartNumber { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public double Quantity { get; set; }
    }

   
}
