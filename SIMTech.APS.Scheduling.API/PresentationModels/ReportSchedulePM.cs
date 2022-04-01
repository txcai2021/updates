using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
   public class ReportSchedulePM
    {
       public ReportSchedulePM()
       {
           
       }

       public int Id { get; set; }
       
       public int? BarNumber { get; set; }
       public DateTime BeginDate { get; set; }
       public DateTime? DueDate { get; set; }
       public string Duration { get; set; }
       public string DurationType { get; set; }
       public DateTime EndDate { get; set; }
       public int? Hotness { get; set; }
       public string LateJob { get; set; }
       public int? MachineId { get; set; }
       public string MachineName { get; set; }
       public int ScheduleId { get; set; }
       public int ScheduleDetailId { get; set; }
       public string ShiftName { get; set; }
       public int? OperationId { get; set; }
       public int OperationSequence { get; set; }
       public string PartNumber { get; set; }
       public double Quantity { get; set; }
       public DateTime? ReleaseDate { get; set; }
       public double? Runtime { get; set; }
       public string String1 { get; set; }
       public string String2 { get; set; }
       public string String3 { get; set; }
       public string String4 { get; set; }
       public string String5 { get; set; }
       public int? Int1 { get; set; }
       public int? Int2 { get; set; }
       public int? Int3 { get; set; }
       public DateTime? Date1 { get; set; }
       public DateTime? Date2 { get; set; }
       public DateTime? Date3 { get; set; }
       public double? UnitRunTime { get; set; }
       public int WorkOrderId { get; set; }
       public string WorkOrderNumber { get; set; }
       public string ProcessingDuration { get; set; }
       public string OperationName { get; set; }
       public string ScheduleType { get; set; }
       public bool IsListBoxDisable { get; set; }
       public int ParentWorkOrderId { get; set; }
       public string Operator { get; set; }
       public int RowNo { get; set; }
       
       
    }
}
