using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.Models
{
    public class ScheduleParameters
    {
        public string locationName { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string dispatchRule { get; set; }
        public string objective { get; set; }
        public bool wip { get; set; }
        public bool command { get; set; }
    }
}
