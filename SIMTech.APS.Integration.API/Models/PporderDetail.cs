using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

#nullable disable

namespace SIMTech.APS.Integration.API.Models
{
    using SIMTech.APS.Models;

    [Table("PPOrderDetail")]
    public partial class PporderDetail 
    {
        [Key]
        public int Id { get; set; }
        [Column("MeltingOrderID")]
        public int? MeltingOrderId { get; set; }
        [Column("PPID")]
        public int? Ppid { get; set; }
        [StringLength(100)]
        public string PartNo { get; set; }
        [Column("SublotID")]
        [StringLength(50)]
        public string SublotId { get; set; }
        [Column("SalesOrderID")]
        public int SalesOrderId { get; set; }
        [Column("SalesOrderDetID")]
        public int? SalesOrderDetId { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Weight { get; set; }
        [StringLength(50)]
        public string Remark { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? EstCompletionDate { get; set; }

        [ForeignKey(nameof(Ppid))]
        [InverseProperty(nameof(Pporder.PporderDetails))]
        public virtual Pporder Pp { get; set; }
    }
}
