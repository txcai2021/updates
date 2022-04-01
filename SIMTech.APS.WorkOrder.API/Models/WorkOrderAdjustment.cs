using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace SIMTech.APS.WorkOrder.API.Models
{
    using SIMTech.APS.Models;

    [Table("WorkOrderAdjustment")]
    public partial class WorkOrderAdjustment : BaseEntity
    {
        [Column("SalesOrderAdjustID")]
        public int SalesOrderAdjustId { get; set; }
        [Column("WorkOrderID")]
        public int WorkOrderId { get; set; }
        public double PlannedQty { get; set; }
        public double ReducedQty { get; set; }
        public double BalanceQty { get; set; }
        [StringLength(250)]
        public string Remark { get; set; }
        [StringLength(10)]
        public string Status { get; set; }
    }
}
