using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace SIMTech.APS.WorkOrder.API.Models
{
    using SIMTech.APS.Models;

    [Table("WorkOrderDetail")]
    [Index(nameof(WorkOrderId), nameof(SalesOrderLineId), nameof(ItemId), Name = "IX_WorkOrderHeader_SalesOrderDetailId", IsUnique = true)]
    public partial class WorkOrderDetail:BaseEntity
    {
       
        [Column("WorkOrderID")]
        public int WorkOrderId { get; set; }
        [Column("SalesOrderLineID")]
        public int? SalesOrderLineId { get; set; }
        [Column("ItemID")]
        public int? ItemId { get; set; }
        public double Quantity { get; set; }
        public string MaxString1 { get; set; }
        public string MaxString2 { get; set; }
        public int? Int1 { get; set; }
        public int? Int2 { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? Int3 { get; set; }
        public int? Int4 { get; set; }
        public int? Int5 { get; set; }
        public double? Float1 { get; set; }
        public double? Float2 { get; set; }
      
        [ForeignKey(nameof(WorkOrderId))]
        [InverseProperty("WorkOrderDetails")]
        public virtual WorkOrder WorkOrder { get; set; }
    }
}
