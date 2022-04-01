using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace SIMTech.APS.Integration.API.Models
{
    using SIMTech.APS.Models;

    [Table("WorkorderMac")]
    public partial class WorkorderMac
    {
        [Key]
        public int Id { get; set; }
        [Column("WOID")]
        [StringLength(50)]
        public string Woid { get; set; }
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
    }
}
