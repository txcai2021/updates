using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    public class UnscheduledWorkOrders
    {
        
        public int? BarNumber { get; set; }
        public DateTime? Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public DateTime? Date3 { get; set; }
        public DateTime? DueDate { get; set; }
        public string Duration { get; set; }
        public DateTime EndDate { get; set; }
        public int EquipmentID { get; set; }
        public int Flag1 { get; set; }
        public int? Int1 { get; set; }
        public int? Int2 { get; set; }
        public int? Int3 { get; set; }
        public string LateJob { get; set; }
        public string MachineName { get; set; }
        public int OperationId { get; set; }
        public string OperationName { get; set; }
        public int OperationSequence { get; set; }
        public string Operator { get; set; }
        public int ParentWorkOrderId { get; set; }
        public string PartName { get; set; }
        public int Priority { get; set; }
        public string ProcessingDuration { get; set; }
        public double Quantity { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int RowId { get; set; }
        public int RowNo { get; set; }
        public double? RunTime { get; set; }
        public int ScheduleDetailId { get; set; }
        public int ScheduleId { get; set; }
        public string ScheduleType { get; set; }
        public string ShiftName { get; set; }
        public DateTime StartDate { get; set; }
        public string String1 { get; set; }
        public string String2 { get; set; }
        public string String3 { get; set; }
        public double? UnitRunTime { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
    }
}
