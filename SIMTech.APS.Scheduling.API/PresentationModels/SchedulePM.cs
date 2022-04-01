using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SIMTech.APS.Resources;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    using SIMTech.APS.Scheduling.API.Models;
   public class SchedulePM
    {
       public SchedulePM()
       {
            ScheduleDetails = new List<ScheduleDetailPM>();
       }

       [Key]
       public int Id { get; set; }

       public string UnitId { get; set; }
       public int? Version { get; set; }

       [Display(ShortName = "StartDate", ResourceType = typeof(SharedResources), Name = "StartDate")]
       public DateTime? StartDate { get; set; }
       public DateTime? EndDate { get; set; }
       public int? RuleSetId1 { get; set; }
       public int? RuleSetId2 { get; set; }
       public string Objective { get; set; }

       [Display(ShortName = "Location", ResourceType = typeof(SharedResources), Name = "LocationName")]
       public string Location { get; set; }
       public string JobRules { get; set; }
       public string MachineRules { get; set; }
       public string MakeSpan { get; set; }
       public string CreatedBy { get; set; }
       public DateTime CreatedOn { get; set; }
       public string LateJobs { get; set; }
       public string UnassignedJobs { get; set; }
       public string ChangeOverTimes { get; set; }
       public string Remarks { get; set; }
       public bool IsConfirmed { get; set; }
       //public bool IsScheduleEditAllowed { get; set; }
       //public bool IsConfirmAllowed { get; set; }
       //public bool IsReRunScheduleAllowed { get; set; }

       public List<ScheduleDetailPM> ScheduleDetails { get; set; }

    }
}
