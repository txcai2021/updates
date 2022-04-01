using System.Collections.Generic;
using System.Linq;


namespace SIMTech.APS.WorkOrder.API.Mappers
{
    using SIMTech.APS.WorkOrder.API.Models;
    using SIMTech.APS.WorkOrder.API.PresentationModels;
    public class WorkOrderLineMapper
    {
        public static IEnumerable<WorkOrderLinePM> ToPresentationModels(IEnumerable<WorkOrderDetail> workOrderLines)
        {
            if (workOrderLines == null) return null;
            return workOrderLines.Select(ToPresentaionModel);
        }

        public static WorkOrderLinePM ToPresentaionModel(WorkOrderDetail workOrderLine)
        {
            if (workOrderLine == null) return null;

            return new WorkOrderLinePM
            {

                Id = workOrderLine.Id,
                ItemId = workOrderLine.ItemId,
                Quantity = workOrderLine.Quantity,
                SalesOrderLineId = workOrderLine.SalesOrderLineId,
                WorkOrderId = workOrderLine.WorkOrderId,

                CurrentRecycleCount = workOrderLine.Int1,
                CurrentRefurbishmentCount = workOrderLine.Int2,
                CurrentRfHours = workOrderLine.Int3,
                CurrentWaferCount = workOrderLine.Int4,
                UnitPrice = workOrderLine.Float1,
                Cost = workOrderLine.Float2,
            };
        }

        public static IEnumerable<WorkOrderDetail> FromPresentationModels(IEnumerable<WorkOrderLinePM> workOrderLinePms)
        {
            if (workOrderLinePms == null) return null;
            //if (workOrderLinePms.Count() == 0) return new List<WorkOrderDetail>();
            return workOrderLinePms.Select(FromPresentationModel);
        }

        public static WorkOrderDetail FromPresentationModel(WorkOrderLinePM workOrderLinePM)
        {
            if (workOrderLinePM == null) return null;

            return new WorkOrderDetail
            {
                ItemId = workOrderLinePM.ItemId,
                Quantity = workOrderLinePM.Quantity,
                SalesOrderLineId = workOrderLinePM.SalesOrderLineId,
                WorkOrderId = workOrderLinePM.WorkOrderId,
                Id = workOrderLinePM.Id,

                Int1 = workOrderLinePM.CurrentRecycleCount,
                Int2 = workOrderLinePM.CurrentRefurbishmentCount,
                Int3 = workOrderLinePM.CurrentRfHours,
                Int4 = workOrderLinePM.CurrentWaferCount,
                Float1 = workOrderLinePM.UnitPrice,
                Float2 = workOrderLinePM.Cost,
            };
        }

        public static void UpdatePresentationModel(WorkOrderLinePM workOrderLinePM, WorkOrderDetail workOrderLine)
        {
            if (workOrderLinePM == null || workOrderLine == null) return;

            workOrderLinePM.Id = workOrderLine.Id;
            workOrderLinePM.ItemId = workOrderLine.ItemId;
            workOrderLinePM.Quantity = workOrderLine.Quantity;
            workOrderLinePM.SalesOrderLineId = workOrderLine.SalesOrderLineId;
            workOrderLinePM.WorkOrderId = workOrderLine.WorkOrderId;

            workOrderLinePM.CurrentRecycleCount = workOrderLine.Int1;
            workOrderLinePM.CurrentRefurbishmentCount = workOrderLine.Int2;
            workOrderLinePM.CurrentRfHours = workOrderLine.Int3;
            workOrderLinePM.CurrentWaferCount = workOrderLine.Int4;

            workOrderLinePM.UnitPrice = workOrderLine.Float1;
            workOrderLinePM.Cost = workOrderLine.Float2;
        }
    }
}
