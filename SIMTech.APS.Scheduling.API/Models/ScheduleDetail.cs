using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace SIMTech.APS.Scheduling.API.Models
{
    using SIMTech.APS.Models;

    [Table("ScheduleDetail")]
    public  class ScheduleDetail : BaseEntity
    {

        [Column("ScheduleID")]
        public int ScheduleId { get; set; }
        [Required]
        [StringLength(3)]
        public string ScheduleType { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime StartDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime EndDate { get; set; }
        [StringLength(50)]
        public string ShiftName { get; set; }
        [Column("RouteID")]
        public int? RouteId { get; set; }
        [StringLength(500)]
        public string RouteName { get; set; }
        public int? Sequence { get; set; }
        [Column("OperationID")]
        public int OperationId { get; set; }
        [Required]
        [StringLength(50)]
        public string OperationName { get; set; }
        [StringLength(50)]
        public string OperationType { get; set; }
        [Column("EquipmentID")]
        public int EquipmentId { get; set; }
        [Required]
        [StringLength(50)]
        public string EquipmentName { get; set; }
        [StringLength(50)]
        public string EquipmentType { get; set; }
        [Column("WorkOrderID")]
        public int WorkOrderId { get; set; }
        [Required]
        [StringLength(50)]
        public string WorkOrderNumber { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? WorkOrderDueDate { get; set; }
        [Column("ItemID")]
        public int ItemId { get; set; }
        [Required]
        [StringLength(100)]
        public string ItemName { get; set; }
        public double? UnitRunTime { get; set; }
        public double RunTime { get; set; }
        public double Quantity { get; set; }
        [Column("OperatorID")]
        public int? OperatorId { get; set; }
        [StringLength(50)]
        public string OperatorName { get; set; }
        [StringLength(50)]
        public string SkillName { get; set; }
        [StringLength(50)]
        public string Status { get; set; }
        [StringLength(50)]
        public string String1 { get; set; }
        [StringLength(50)]
        public string String2 { get; set; }
        [StringLength(50)]
        public string String3 { get; set; }
        [StringLength(50)]
        public string String4 { get; set; }
        [StringLength(50)]
        public string String5 { get; set; }
        public string MaxString1 { get; set; }
        public string MaxString2 { get; set; }
        public string MaxString3 { get; set; }
        public int? Int1 { get; set; }
        public int? Int2 { get; set; }
        public int? Int3 { get; set; }
        public int? Int4 { get; set; }
        public int? Int5 { get; set; }
        public int? Int6 { get; set; }
        public double? Float1 { get; set; }
        public double? Float2 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? Date1 { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? Date2 { get; set; }

        [ForeignKey(nameof(ScheduleId))]
        [InverseProperty("ScheduleDetails")]
        public virtual Schedule Schedule { get; set; }
    }
}
