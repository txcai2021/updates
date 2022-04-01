using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using SIMTech.APS.Resources;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
    public class ScheduleDetailPM
    {
        public ScheduleDetailPM()
        {

        }

        [Key]
        public int Id { get; set; }

        public int ScheduleId { get; set; }

        public string ScheduleType {get;set;}
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ShiftName { get; set; }
        public int? RouteId { get; set; }
        public string RouteName { get; set; }
        public int? Sequence { get; set; }
        public int OperationId { get; set; }
        public string OperationName { get; set; }
        public string OperationType { get; set; }
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string EquipmentType { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; }
        public DateTime? WorkOrderDueDate { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public double? UnitRunTime { get; set; }
        public double? RunTime { get; set; }
        public double? Quantity { get; set; }
        public int OperatorId { get; set; }
        public string OperatorName { get; set; }
        public string SkillName { get; set; }
        public string Status { get; set; }
        public string WorkOrderCategory { get; set; }


    }
}
