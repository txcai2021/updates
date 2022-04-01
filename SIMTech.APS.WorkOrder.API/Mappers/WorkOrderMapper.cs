using System.Collections.Generic;
using System.Linq;


namespace SIMTech.APS.WorkOrder.API.Mappers
{
    using SIMTech.APS.WorkOrder.API.Models;
    using SIMTech.APS.WorkOrder.API.PresentationModels;
    using SIMTech.APS.WorkOrder.API.Enums;

    public static class WorkOrderMapper
    {
        public static IEnumerable<WorkOrderPM> ToPresentationModels(IEnumerable<ChildWorkOrderPM> workOrders)
        {
            if (workOrders == null) return null;
            return workOrders.Select(ToPresentationModel);
        }

        public static WorkOrderPM ToPresentationModel(ChildWorkOrderPM workOrder)
        {
            if (workOrder == null) return null;

            return new WorkOrderPM
            {
                LinkedWorkOrderNumber = workOrder.ParentWOID,
                WorkOrderNumber = workOrder.ChildWOID,
                RouteId = workOrder.RouteID,
                LineNo = workOrder.ProcOPseq,
                Quantity = workOrder.ActualProdQty,
                Remarks = workOrder.Type
            };
        }


        public static IEnumerable<WorkOrderPM> ToPresentationModels(IEnumerable<ReworkOrderPM> workOrders)
        {
            if (workOrders == null) return null;
            return workOrders.Select(ToPresentationModel);
        }

        public static WorkOrderPM ToPresentationModel(ReworkOrderPM workOrder)
        {
            if (workOrder == null) return null;

            return new WorkOrderPM
            {
                WorkOrderNumber = workOrder.WOID,
                RouteId = workOrder.ReworkRouteID
            };
        }




        public static IEnumerable<WorkOrderPM> ToPresentationModels(IEnumerable<WorkOrder> workOrders)
        {
            if (workOrders == null) return null;
            return workOrders.Select(ToPresentationModel);
        }

        public static WorkOrderPM ToPresentationModel(WorkOrder workOrder)
        {
            if (workOrder == null) return null;

            return new WorkOrderPM
            {
                Id = workOrder.Id,
                WorkOrderNumber = workOrder.WorkOrderNumber,
                CustomerId = workOrder.CustomerId,
                DueDate = workOrder.DueDate,
                DueDate1 = workOrder.DueDate.ToString("dd/MM/yyyy"),
                IssueDate = workOrder.IssueDate,
                LocationId = workOrder.LocationId,
                OrderType = (EWorkOrderType)workOrder.OrderType,
                ParentWorkOrderId = workOrder.ParentWorkOrderId,
                ProductId = workOrder.ProductId,
                RouteId = workOrder.RouteId,
                Quantity = workOrder.Quantity,
                Remarks = workOrder.Remarks,
                Remarks1 = workOrder.String5,   
                PRDate = workOrder.String8 ,
                Status = (EWorkOrderStatus)workOrder.Status,


                //customization starts here
                GatePassNo = workOrder.String1,
                ChamberId = workOrder.String2,
                Disposition = workOrder.String3,
                ReleasedBy = workOrder.String4,
                
                Reason = workOrder.MaxString1,
                BuyList = workOrder.MaxString2,

                UrgentFlag = (EWorkOrderUrgentFlag)(workOrder.Int1 ?? 0),
                PMReason = workOrder.Int2,
                LinkWorkOrderId = workOrder.Int3,
                //ConsumptionMode = workOrder.Int4,
                ReleasedDate = workOrder.Date1,
                CompletionDate = workOrder.CompletedDate ,

                PictureA = workOrder.Int4,
                PictureB = workOrder.Int5,
                PictureC = workOrder.Int6,
                CreatedBy = workOrder.CreatedBy,

                KitTypeId = (int?)workOrder.Float2,
                AllocatedQuantity =workOrder.Float1==0?null:workOrder.Float1 ,
                Priority = (short)(workOrder.Priority ?? (workOrder.Int1 == 0 ? 50 : (workOrder.Int1 == 1 ? 88 : 99))),

                WorkOrderMaterials = WorkOrderMaterialMapper.ToPresentationModels(workOrder.WorkOrderMaterials).ToList (),
                WorkOrderLines = WorkOrderLineMapper.ToPresentationModels(workOrder.WorkOrderDetails).ToList()
            };
        }


        public static IEnumerable<WorkOrder> FromPresentationModels(IEnumerable<WorkOrderPM> workOrders)
        {
            if (workOrders == null) return null;
            return workOrders.Select(FromPresentationModel);
        }

        public static WorkOrder FromPresentationModel(WorkOrderPM workOrderPM)
        {
            if (workOrderPM == null) return null;

            return new WorkOrder
            {
                Id = workOrderPM.Id,
                WorkOrderNumber = workOrderPM.WorkOrderNumber,
                CustomerId = workOrderPM.CustomerId,
                DueDate = workOrderPM.DueDate,
                IssueDate = workOrderPM.IssueDate,
                LocationId = workOrderPM.LocationId,
                OrderType = (byte)workOrderPM.OrderType,
                ParentWorkOrderId = workOrderPM.ParentWorkOrderId,
                ProductId = workOrderPM.ProductId,
                RouteId = workOrderPM.RouteId,
                Quantity = workOrderPM.Quantity,
                Remarks = workOrderPM.Remarks,
                Status = (byte)workOrderPM.Status,

                //customization starts here
                String1 = workOrderPM.GatePassNo,
                String2 = workOrderPM.ChamberId,
                String3 = workOrderPM.Disposition,
                String4 = workOrderPM.ReleasedBy,
                String5 = workOrderPM.Remarks1,

                MaxString1 = workOrderPM.Reason,
                MaxString2 = workOrderPM.BuyList,
                Int1 = (int?)workOrderPM.UrgentFlag,
                Int2 = (int?)workOrderPM.PMReason,
                Int3 = workOrderPM.LinkWorkOrderId,
                //Int4 = workOrderPM.ConsumptionMode,
                Date1 = workOrderPM.ReleasedDate,

                Int4 = workOrderPM.PictureA,
                Int5 = workOrderPM.PictureB,
                Int6 = workOrderPM.PictureC,

                Float2 = workOrderPM.KitTypeId,
                Priority =workOrderPM.Priority ,
                CreatedBy =workOrderPM.CreatedBy ,
                ModifiedBy =workOrderPM.Sequence ,
                WorkOrderMaterials = WorkOrderMaterialMapper.FromPresentationModels(workOrderPM.WorkOrderMaterials).ToList(),
                WorkOrderDetails = WorkOrderLineMapper.FromPresentationModels(workOrderPM.WorkOrderLines).ToList()
            };
        }


        public static void UpdatePresentationModel(WorkOrderPM workOrderPM, WorkOrder workOrder)
        {
            if (workOrderPM == null || workOrder == null) return;


            workOrderPM.Id = workOrder.Id;
            workOrderPM.WorkOrderNumber = workOrder.WorkOrderNumber;
            workOrderPM.CustomerId = workOrder.CustomerId;
            workOrderPM.DueDate = workOrder.DueDate;
            workOrderPM.IssueDate = workOrder.IssueDate;
            workOrderPM.LocationId = workOrder.LocationId;
            workOrderPM.OrderType = (EWorkOrderType)workOrder.OrderType;
            workOrderPM.ParentWorkOrderId = workOrder.ParentWorkOrderId;
            workOrderPM.ProductId = workOrder.ProductId;
            workOrderPM.RouteId = workOrder.RouteId;
            workOrderPM.Quantity = workOrder.Quantity;
            workOrderPM.Remarks = workOrder.Remarks;
            workOrderPM.Remarks1 = workOrder.String5;
            workOrderPM.Status = (EWorkOrderStatus)workOrder.Status;

            //customization starts here
            workOrderPM.GatePassNo = workOrder.String1;
            workOrderPM.ChamberId = workOrder.String2;
            workOrderPM.Disposition = workOrder.String3;
            workOrderPM.Reason = workOrder.MaxString1;

            workOrderPM.UrgentFlag = (EWorkOrderUrgentFlag)(workOrder.Int1 ?? 0);
            workOrderPM.PMReason = workOrder.Int2;
            workOrderPM.LinkWorkOrderId = workOrder.Int3;
            workOrderPM.ConsumptionMode = workOrder.Int4;
            workOrderPM.ReleasedDate = workOrder.Date1;

            workOrderPM.PictureA = workOrder.Int4;
            workOrderPM.PictureB = workOrder.Int5;
            workOrderPM.PictureC = workOrder.Int6;

            workOrderPM.Priority = (short)(workOrder.Priority ?? (workOrderPM.UrgentFlag == EWorkOrderUrgentFlag.Standard ? 50 : (workOrderPM.UrgentFlag == EWorkOrderUrgentFlag.Urgent ? 88 : 99)));

            


            foreach (WorkOrderLinePM workOrderLinePM in workOrderPM.WorkOrderLines)
            {
                workOrderLinePM.WorkOrderId = workOrder.Id;
            }

            foreach (WorkOrderPM childWorkOrder in workOrderPM.ChildWorkOrders)
            {
                childWorkOrder.ParentWorkOrderId = workOrder.Id;
            }
        }

        public static void UpdateFrom(this WorkOrder existingWorkOrder, WorkOrder workOrder)
        {
            if (existingWorkOrder == null || workOrder == null) return ;


            existingWorkOrder.CustomerId = workOrder.CustomerId;
            existingWorkOrder.DueDate = workOrder.DueDate;
            existingWorkOrder.IssueDate = workOrder.IssueDate;
            existingWorkOrder.LocationId = workOrder.LocationId;
            existingWorkOrder.OrderType = workOrder.OrderType;
            existingWorkOrder.ParentWorkOrderId = workOrder.ParentWorkOrderId;
            existingWorkOrder.ProductId = workOrder.ProductId;
            existingWorkOrder.RouteId = workOrder.RouteId;
            existingWorkOrder.Quantity = workOrder.Quantity;
            existingWorkOrder.Remarks = workOrder.Remarks;
            existingWorkOrder.Status = workOrder.Status;
            existingWorkOrder.WorkOrderNumber = workOrder.WorkOrderNumber;

            existingWorkOrder.String1 = workOrder.String1;
            existingWorkOrder.String2 = workOrder.String2;
            existingWorkOrder.String3 = workOrder.String3;
            existingWorkOrder.String5 = workOrder.String5;
            existingWorkOrder.MaxString1 = workOrder.MaxString1;
            existingWorkOrder.Int1 = workOrder.Int1;
            existingWorkOrder.Int2 = workOrder.Int2;
            existingWorkOrder.Int3 = workOrder.Int3;
            existingWorkOrder.Int4 = workOrder.Int4;
            existingWorkOrder.Int5 = workOrder.Int5;
            existingWorkOrder.Int6 = workOrder.Int6;
            existingWorkOrder.Float1 = workOrder.Float1;
            existingWorkOrder.Float2 = workOrder.Float2;
            existingWorkOrder.Priority = workOrder.Priority;

            //return existingWorkOrder;
        }
    }
}
