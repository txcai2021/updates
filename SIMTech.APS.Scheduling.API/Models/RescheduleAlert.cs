using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace SIMTech.APS.Scheduling.API.Models
{
    using SIMTech.APS.Models;

    [Table("RescheduleAlert")]
    public partial class RescheduleAlert : BaseEntity
    {
        [Required]
        [StringLength(10)]
        public string AlertType { get; set; }
        [StringLength(50)]
        public string WorkOrderNumber { get; set; }
        public int? Sequence { get; set; }
        [StringLength(50)]
        public string OperationName { get; set; }
        [StringLength(50)]
        public string MachineName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? StartDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? EndDate { get; set; }
        [StringLength(50)]
        public string PlannedMachine { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? PlannedStartDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? PlannedEndDate { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? DurationbyHours { get; set; }
        [StringLength(10)]
        public string Status { get; set; }
        [StringLength(500)]
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        [StringLength(50)]
        public string String1 { get; set; }
        [StringLength(50)]
        public string String2 { get; set; }
        public string MaxString1 { get; set; }
        public string MaxString2 { get; set; }

    }
}
