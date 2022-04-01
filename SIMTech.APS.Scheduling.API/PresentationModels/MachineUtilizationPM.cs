using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
   public class MachineUtilizationPM
    {
       public MachineUtilizationPM()
       {
           
       }

       public int Id { get; set; }

       public string OperationName { get; set; }
       public string Machine { get; set; }
       public string ScheduleType { get; set; }
       public string LoadType { get; set; }

       public DateTime StartDate { get; set; }
       public DateTime EndDate { get; set; }
       public double RunTime { get; set; }
       public bool IsWIP { get; set; }
       public int EquipmentId { get; set; }

       public double WIP { get; set; }
       public double WIPRatio { get; set; }

       public double WO { get; set; }
       public double WORatio { get; set; }

       public double Setup { get; set; }
       public double SetupRatio { get; set; }


       public double Available { get; set; }
       public double AvailableRatio { get; set; }

       public DateTime ScheduleDate { get; set; }


       public DateTime Date1 { get; set; }
       public DateTime Date2 { get; set; }

       public string String1 { get; set; }
       public string String2 { get; set; }

       public double Float1 { get; set; }
       public double Float2 { get; set; }
       public string EntityName { get; set; }

       public string SubEntity { get; set; }



    }
}
