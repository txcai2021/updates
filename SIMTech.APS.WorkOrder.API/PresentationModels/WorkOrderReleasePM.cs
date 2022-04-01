using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SIMTech.APS.WorkOrder.API.PresentationModels
{

    using SIMTech.APS.WorkOrder.API.Enums;
  

    public class WorkOrderReleasePM
    {
        [Key]
        public int workOrderId { get; set; }
        public EWorkOrderStatus Status { get; set; }
        public DateTime? ReleasedDate { get; set; }

    }
}
