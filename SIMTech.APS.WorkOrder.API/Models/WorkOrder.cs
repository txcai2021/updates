using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace SIMTech.APS.WorkOrder.API.Models
{
    using SIMTech.APS.Models;

    [Table("WorkOrder")]
    [Index(nameof(WorkOrderNumber), Name = "IX_WorkOrder", IsUnique = true)]
    public partial class WorkOrder : BaseEntity
    {
        public WorkOrder()
        {
            WorkOrderDetails = new HashSet<WorkOrderDetail>();
            WorkOrderMaterials = new HashSet<WorkOrderMaterial>();
        }

        [Required]
        [StringLength(50)]
        public string WorkOrderNumber { get; set; }
        [Column("ProductID")]
        public int ProductId { get; set; }
        public double Quantity { get; set; }
        public byte OrderType { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime IssueDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DueDate { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? CompletedDate { get; set; }
        public short? Priority { get; set; }
        [Column("ParentWorkOrderID")]
        public int? ParentWorkOrderId { get; set; }
        public byte Status { get; set; }
        [Column("LocationID")]
        public int? LocationId { get; set; }
        [Column("CustomerID")]
        public int? CustomerId { get; set; }
        [Column("RouteID")]
        public int? RouteId { get; set; }
        [StringLength(250)]
        public string Remarks { get; set; }
        public string String1 { get; set; }
        [StringLength(50)]
        public string String2 { get; set; }
        [StringLength(50)]
        public string String3 { get; set; }
        [StringLength(50)]
        public string String4 { get; set; }
        [StringLength(250)]
        public string String5 { get; set; }
        [StringLength(250)]
        public string String6 { get; set; }
        [StringLength(250)]
        public string String7 { get; set; }
        [StringLength(250)]
        public string String8 { get; set; }
        public string MaxString1 { get; set; }
        public string MaxString2 { get; set; }
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
        

        [InverseProperty(nameof(WorkOrderDetail.WorkOrder))]
        public virtual ICollection<WorkOrderDetail> WorkOrderDetails { get; set; }
        [InverseProperty(nameof(WorkOrderMaterial.WorkOrder))]
        public virtual ICollection<WorkOrderMaterial> WorkOrderMaterials { get; set; }
    }
}
