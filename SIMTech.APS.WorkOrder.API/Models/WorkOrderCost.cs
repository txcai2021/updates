using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace SIMTech.APS.WorkOrder.API.Models
{
    using SIMTech.APS.Models;

    [Table("WorkOrderCost")]
    public partial class WorkOrderCost:BaseEntity
    {
        public int WorkOrderId { get; set; }
        [Required]
        [StringLength(50)]
        public string WorkOrderNumber { get; set; }
        [Required]
        [StringLength(50)]
        public string Category { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        [StringLength(250)]
        public string Description { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }
        public double Quantity { get; set; }
        [Required]
        [StringLength(50)]
        public string CustomerName { get; set; }
        public string Remarks { get; set; }
        [StringLength(50)]
        public string String1 { get; set; }
        [StringLength(50)]
        public string String2 { get; set; }
        [StringLength(250)]
        public string String3 { get; set; }
        [StringLength(250)]
        public string String4 { get; set; }
        public string MaxString1 { get; set; }
        public string MaxString2 { get; set; }
        public bool? Flag1 { get; set; }
        public bool? Flag2 { get; set; }
        public int? Int1 { get; set; }
        public int? Int2 { get; set; }
        public double? Float1 { get; set; }
        public double? Float2 { get; set; }
       

    }
}
