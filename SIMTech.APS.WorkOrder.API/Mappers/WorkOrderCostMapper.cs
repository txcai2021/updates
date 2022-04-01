using System.Collections.Generic;
using System.Linq;


namespace SIMTech.APS.WorkOrder.API.Mappers
{
    using SIMTech.APS.WorkOrder.API.Models;
    using SIMTech.APS.WorkOrder.API.PresentationModels;

    public class WorkOrderCostMapper
    {
        public static IEnumerable<WorkOrderCostPM> ToPresentationModels(IEnumerable<WorkOrderCost> workOrderCosts)
        {
            if(workOrderCosts == null) return null;
            return workOrderCosts.Select(ToPresentationModel);
        }

        public static WorkOrderCostPM ToPresentationModel(WorkOrderCost workOrderCost)
        {
            if (workOrderCost == null) return null;
            
            return new WorkOrderCostPM
            {                
                Id = workOrderCost.Id,
                WorkOrderId = workOrderCost.WorkOrderId, 
                WorkOrderNumber  = workOrderCost.WorkOrderNumber ,              
                Category  = workOrderCost.Category ,
                Description = workOrderCost.Name,       
                Quantity = workOrderCost.Quantity ,
                PRNo = workOrderCost.Description,
                Remarks = workOrderCost.Remarks,
                UnitPrice = (double)workOrderCost.UnitPrice,
                CompanyName = workOrderCost.CustomerName,
                productId =(int)workOrderCost.Int1 ,
                Cost = workOrderCost.Quantity * (double) workOrderCost.UnitPrice,
            };
        }
        
        public static IEnumerable<WorkOrderCost> FromPresentationModels(IEnumerable<WorkOrderCostPM> workOrderCostPms)
        {
            if (workOrderCostPms == null) return null;

            return workOrderCostPms.Select(FromPresentationModel);
        }

        public static WorkOrderCost FromPresentationModel(WorkOrderCostPM workOrderCostPM)
        {
            if (workOrderCostPM == null) return null;

            return new WorkOrderCost
            {
                Id=workOrderCostPM.Id,
                WorkOrderId = workOrderCostPM.WorkOrderId, 
                WorkOrderNumber  = workOrderCostPM.WorkOrderNumber ,              
                Category  = workOrderCostPM.Category ,
                Name = workOrderCostPM.Description,       
                Quantity = workOrderCostPM.Quantity ,
                Remarks = workOrderCostPM.Remarks,
                Description = workOrderCostPM.PRNo,
                UnitPrice = (decimal)workOrderCostPM.UnitPrice,
                CustomerName = workOrderCostPM.CompanyName,
                Int1 = workOrderCostPM.productId ,
            };
        }

        public static void UpdatePresentationModel(WorkOrderCostPM workOrderCostPM, WorkOrderCost workOrderCost)
        {
            if (workOrderCostPM == null || workOrderCost == null) return;

            workOrderCostPM.Id = workOrderCost.Id;
            workOrderCostPM.WorkOrderId = workOrderCost.WorkOrderId;
            workOrderCostPM.WorkOrderNumber  = workOrderCost.WorkOrderNumber;              
            workOrderCostPM.Category  = workOrderCost.Category;
            workOrderCostPM.PRNo = workOrderCost.Description;      
            workOrderCostPM.Quantity = workOrderCost.Quantity;
            workOrderCostPM.Remarks = workOrderCost.Remarks;
            workOrderCostPM.Description = workOrderCost.Name;
            workOrderCostPM.UnitPrice = (double)workOrderCost.UnitPrice;
            workOrderCostPM.CompanyName = workOrderCost.CustomerName;
            workOrderCostPM.productId = (int)workOrderCost.Int1;
            workOrderCostPM.Cost = workOrderCost.Quantity * (double)workOrderCost.UnitPrice;
        }

        public static void UpdateFromPresentationModel(WorkOrderCostPM workOrderCostPM, WorkOrderCost workOrderCost)
        {
            if (workOrderCostPM == null || workOrderCost == null) return;

          
            workOrderCost.WorkOrderId=workOrderCostPM.WorkOrderId ;
            workOrderCost.WorkOrderNumber=workOrderCostPM.WorkOrderNumber;
            workOrderCost.Category=workOrderCostPM.Category;
            workOrderCost.Description=workOrderCostPM.PRNo;
            workOrderCost.Quantity=workOrderCostPM.Quantity;
            workOrderCost.Remarks=workOrderCostPM.Remarks;
            workOrderCost.Name=workOrderCostPM.Description;
            workOrderCost.UnitPrice=(decimal)workOrderCostPM.UnitPrice;
            workOrderCost.CustomerName= workOrderCostPM.CompanyName;
            workOrderCost.Int1=workOrderCostPM.productId;
            
        }
    }
}
