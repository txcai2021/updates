using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace SIMTech.APS.Integration.API.Models
{
    using SIMTech.APS.Models;
    [Table("PPOrderRoute")]
    public partial class PporderRoute 
    {
        [Key]
        public int Id { get; set; }
        [Column("PPID")]
        public int Ppid { get; set; }
        [StringLength(100)]
        public string PartNo { get; set; }
        [Column("RouteID")]
        public int RouteId { get; set; }
        public int SeqNo { get; set; }
        [Required]
        [Column("CentreID")]
        [StringLength(50)]
        public string CentreId { get; set; }
        [StringLength(50)]
        public string MacCode { get; set; }
        [StringLength(20)]
        public string MacType { get; set; }
        public string Remark { get; set; }
        public int? MacGroup { get; set; }
        [StringLength(32)]
        public string AttributeGroup { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? StartDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? EndDate { get; set; }
        public double? Qty { get; set; }
        public double? Duration { get; set; }
        [Column("ScheduleID")]
        public int? ScheduleId { get; set; }
        [Column("materialIdList")]
        [StringLength(50)]
        public string MaterialIdList { get; set; }

        [ForeignKey(nameof(Ppid))]
        [InverseProperty(nameof(Pporder.PporderRoutes))]
        public virtual Pporder Pp { get; set; }
    }
}
