using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace SIMTech.APS.WorkOrder.API.Models
{
    using SIMTech.APS.Models;

    [Table("WorkOrderMaterial")]
    [Index(nameof(WorkOrderId), nameof(MaterialId), Name = "IX_WorkOrderMaterial_MaterialID", IsUnique = true)]
    public partial class WorkOrderMaterial:BaseEntity
    {
        public int WorkOrderId { get; set; }
        public int MaterialId { get; set; }
        [Column("UnitMeasureID")]
        public int UnitMeasureId { get; set; }
        [StringLength(250)]
        public string Remark { get; set; }
        [StringLength(10)]
        public string Status { get; set; }
        public bool? Availability { get; set; }       
        public double? Quantity { get; set; }
        public double? Ratio { get; set; }

        [ForeignKey(nameof(WorkOrderId))]
        [InverseProperty("WorkOrderMaterials")]
        public virtual WorkOrder WorkOrder { get; set; }
    }
}
