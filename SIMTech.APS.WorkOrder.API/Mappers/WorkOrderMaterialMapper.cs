using System.Collections.Generic;
using System.Linq;


namespace SIMTech.APS.WorkOrder.API.Mappers
{
    using SIMTech.APS.WorkOrder.API.Models;
    using SIMTech.APS.WorkOrder.API.PresentationModels;
    public class WorkOrderMaterialMapper
    {
        public static IEnumerable<WorkOrderMaterialPM> ToPresentationModels(IEnumerable<WorkOrderMaterial> workOrderMaterials)
        {
            if(workOrderMaterials == null) return null;
            return workOrderMaterials.Select(ToPresentationModel);
        }

        public static WorkOrderMaterialPM ToPresentationModel(WorkOrderMaterial workOrderMaterial)
        {
            if (workOrderMaterial == null) return null;
            var uom = workOrderMaterial.UnitMeasureId;
            double? matlQty = workOrderMaterial.WorkOrder.Quantity * workOrderMaterial.Ratio;
            var matlQty1 = System.Math.Ceiling(matlQty ?? 0);

            return new WorkOrderMaterialPM
            {
                Availability = workOrderMaterial.Availability,
                Id = workOrderMaterial.Id,
                MaterialId = workOrderMaterial.MaterialId,              
                Ratio  = (decimal?)workOrderMaterial.Ratio ,
                //Quantity = uom == 0 ? matlQty : (uom == 1 ? workOrderMaterial.Ratio : matlQty1),
                Quantity = workOrderMaterial.Quantity ,
                Remarks = workOrderMaterial.Remark,
                Status = workOrderMaterial.Status,
                UnitOfMeasureId = workOrderMaterial.UnitMeasureId,
                WorkOrderId = workOrderMaterial.WorkOrderId
            };
        }
        
        public static IEnumerable<WorkOrderMaterial> FromPresentationModels(IEnumerable<WorkOrderMaterialPM> workOrderMaterialPms)
        {
            if (workOrderMaterialPms == null) return null;

            return workOrderMaterialPms.Select(FromPresentationModel);
        }

        public static WorkOrderMaterial FromPresentationModel(WorkOrderMaterialPM workOrderMaterialPM)
        {
            if (workOrderMaterialPM == null) return null;

            return new WorkOrderMaterial
            {
                Availability = workOrderMaterialPM.Availability,
                MaterialId = workOrderMaterialPM.MaterialId,
                Quantity = workOrderMaterialPM.Quantity,
                Ratio =(double?) workOrderMaterialPM.Ratio,
                Remark = workOrderMaterialPM.Remarks,
                Status = workOrderMaterialPM.Status,
                UnitMeasureId = workOrderMaterialPM.UnitOfMeasureId,
                WorkOrderId = workOrderMaterialPM.WorkOrderId,
                Id = workOrderMaterialPM.Id
            };
        }

        public static void UpdatePresentationModel(WorkOrderMaterialPM workOrderMaterialPM, WorkOrderMaterial workOrderMaterial)
        {
            if (workOrderMaterialPM == null || workOrderMaterial == null) return;

            workOrderMaterialPM.Availability = workOrderMaterial.Availability;
            workOrderMaterialPM.Id = workOrderMaterial.Id;
            workOrderMaterialPM.MaterialId = workOrderMaterial.MaterialId;
            workOrderMaterialPM.Quantity = workOrderMaterial.Quantity;
            workOrderMaterialPM.Ratio = (decimal?)workOrderMaterial.Ratio;
            workOrderMaterialPM.Remarks = workOrderMaterial.Remark;
            workOrderMaterialPM.Status = workOrderMaterial.Status;
            workOrderMaterialPM.UnitOfMeasureId = workOrderMaterial.UnitMeasureId;
            workOrderMaterialPM.WorkOrderId = workOrderMaterial.WorkOrderId;
        }
    }
}
