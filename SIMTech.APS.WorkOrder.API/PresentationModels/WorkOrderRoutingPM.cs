using System.ComponentModel.DataAnnotations;
using SIMTech.APS.Resources;

namespace SIMTech.APS.WorkOrder.API.PresentationModels
{
    public class WorkOrderRoutingPM
    {
        [Key]
        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(ShortName = "WorkOrderRoutingId", ResourceType = typeof(SharedResources), Name = "WorkOrderRoutingId", Order = 0)]
        public int Id { get; set; }

        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(ShortName = "WorkOrderId", ResourceType = typeof(SharedResources), Name = "WorkOrderId", Order = 1)]
        public int WorkOrderId { get; set; }

        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        [StringLength(32, ErrorMessageResourceName = "ValidationErrorBadWorkOrderRoutingOperationName", ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(ShortName = "OperationName", ResourceType = typeof(SharedResources), Name = "OperationName", Order = 2)]
        public string OperationName { get; set; }

        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(ShortName = "Sequence", ResourceType = typeof(SharedResources), Name = "Sequence", Order = 3)]
        public int Sequence { get; set; }

        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(ShortName = "ResourceId", ResourceType = typeof(SharedResources), Name = "ResourceId", Order = 4)]
        public int ResourceId { get; set; }

        [Display(ShortName = "ResourcePriority", ResourceType = typeof(SharedResources), Name = "ResourcePriority", Order = 5)]
        public int? ResourcePriority { get; set; }

        [StringLength(256, ErrorMessageResourceName = "ValidationErrorBadWorkOrderRoutingRemarks", ErrorMessageResourceType = typeof(ErrorResources))]
        [Display(ShortName = "WorkOrderRoutingRemarks", ResourceType = typeof(SharedResources), Name = "WorkOrderRoutingRemarks", Order = 6)]
        public string Remarks { get; set; }

        [Display(ShortName = "WorkOrderRoutingPreTime", ResourceType = typeof(SharedResources), Name = "WorkOrderRoutingPreTime", Order = 7)]
        public double? PreTime { get; set; }

        [Display(ShortName = "WorkOrderRoutingPostTime", ResourceType = typeof(SharedResources), Name = "WorkOrderRoutingPostTime", Order = 8)]
        public double? PostTime { get; set; }

        [Display(ShortName = "WorkOrderRoutingDuration", ResourceType = typeof(SharedResources), Name = "WorkOrderRoutingDuration", Order = 9)]
        public double? Duration { get; set; }

        [Display(ShortName = "WorkOrderRoutingDurationPerUnit", ResourceType = typeof(SharedResources), Name = "WorkOrderRoutingDurationPerUnit", Order = 10)]
        public double? DurationPerUnit { get; set; }

        public WorkOrderPM WorkOrder { get; set; }
    }
}
