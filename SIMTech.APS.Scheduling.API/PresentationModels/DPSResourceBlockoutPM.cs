using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{

    public class DPSResourceBlockoutPM
    {
        public DPSResourceBlockoutPM()
        {

        }

         [Key]
        public int EquipmentBlockOutID { get; set; }

        public int EquipmentID { get; set; }

        public string BlockOutType { get; set; }
        public string MachineName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Remarks { get; set; }
        public string ScheduleStatus { get; set; }
        public bool IsScheduled { get; set; }
        public bool IsItemChecked { get; set; }
        public int Value { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }


    }
}
