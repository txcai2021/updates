using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace SIMTech.APS.Integration.API.Models
{
    using SIMTech.APS.Models;

    [Table("InvMas")]
    public  class InvMas 
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Location { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DateIn { get; set; }
        [Required]
        [StringLength(5)]
        public string InvType { get; set; }
        [Required]
        [StringLength(150)]
        public string PartNo { get; set; }
        [Required]
        [StringLength(50)]
        public string WorkOrder { get; set; }
        [StringLength(50)]
        public string SubWorkOrder { get; set; }
        [StringLength(50)]
        public string ReservedWorkOrder { get; set; }
        public int? SeqNo { get; set; }
        [Column("CenterID")]
        [StringLength(50)]
        public string CenterId { get; set; }
        [StringLength(50)]
        public string Customer { get; set; }
        public int Qty { get; set; }
        public int BalQty { get; set; }
        [StringLength(300)]
        public string Remarks { get; set; }
        [StringLength(50)]
        public string Status { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ActualBalQty { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? CreatedOn { get; set; }
        [StringLength(16)]
        public string CreatedBy { get; set; }
    }
}
