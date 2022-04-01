using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace SIMTech.APS.Scheduling.API.Models
{
    using SIMTech.APS.Models;

    [Table("Schedule")]
    public  class Schedule :BaseEntity
    {
        public Schedule()
        {
            ScheduleDetails = new HashSet<ScheduleDetail>();
        }

        [Required]
        [StringLength(50)]
        public string UnitId { get; set; }
        public int? Version { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? StartDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? EndDate { get; set; }
        public int? Rule1 { get; set; }
        public int? Rule2 { get; set; }
        public int? Rule3 { get; set; }
        public int? Rule4 { get; set; }
        public short? Confirmed { get; set; }
        [StringLength(50)]
        public string String1 { get; set; }
        [StringLength(50)]
        public string String2 { get; set; }
        [StringLength(50)]
        public string String3 { get; set; }
        [StringLength(50)]
        public string String4 { get; set; }
        public string MaxString1 { get; set; }
        public string MaxString2 { get; set; }
        public string MaxString3 { get; set; }
        public string MaxString4 { get; set; }      

        [InverseProperty(nameof(ScheduleDetail.Schedule))]
        public virtual ICollection<ScheduleDetail> ScheduleDetails { get; set; }
    }
}
