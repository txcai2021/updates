using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SIMTech.APS.Scheduling.API.Mappers
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;

    public class EngineOperationPlanMapper
    {
        public static IEnumerable<ReportSchedulePM> ToPresentationModels(IEnumerable<OperationPlan> engineoperationItems)
        {
            if (engineoperationItems == null) return null;
            return engineoperationItems.Select(ToPresentationModel);
        }

        public static ReportSchedulePM ToPresentationModel(OperationPlan objOperationPlan)
        {
            if (objOperationPlan == null) return null;

            return new ReportSchedulePM
            {
                BeginDate = objOperationPlan.StartDate,
                EndDate = objOperationPlan.EndDate,
                WorkOrderId = objOperationPlan.Demand == null ? 0 : Convert.ToInt32(objOperationPlan.Demand.Id),
                WorkOrderNumber = objOperationPlan.Demand == null ? "" : objOperationPlan.Demand.Name,
                PartNumber = objOperationPlan.Demand == null ? "" : objOperationPlan.Demand.Item == null? "": objOperationPlan.Demand.Item.Name,
              //  MachineId = scheduleDB.EquipmentID,
              //  MachineName = scheduleDB.MachineName,
              //  Runtime = scheduleDB.RunTime,
              //  UnitRunTime = scheduleDB.UnitRunTime,
                Quantity = objOperationPlan.Quantity,
                DueDate =objOperationPlan.Demand == null ? DateTime.Now: objOperationPlan.Demand.DueDate,
                ReleaseDate = objOperationPlan.Demand == null ? DateTime.Now : objOperationPlan.Demand.ReleaseDate,
                Hotness = objOperationPlan.Operation == null ? 0 : objOperationPlan.Operation.Priority,
              //  LateJob = scheduleDB.LateJob,
              //  Duration = scheduleDB.Duration,
               // ProcessingDuration=scheduleDB.ProcessingDuration,
                ScheduleType="X",
                OperationName = objOperationPlan.Operation == null ? "" : objOperationPlan.Operation.Name,
               // ShiftName=scheduleDB.ShiftName,
                OperationSequence = objOperationPlan.Operation == null ? 0 : objOperationPlan.Operation.Sequence,
             //   BarNumber = scheduleDB.BarNumber,
               // String1 = scheduleDB.String1,
              //  String2 = scheduleDB.String2,
               // String3 = scheduleDB.String3,
             //   Operator = scheduleDB.String3,
              //  ScheduleDetailId = scheduleDB.Int1 ?? 0,
              //  Int1 = scheduleDB.Int1,              
               // Int3 = scheduleDB.Int3,
              //  Date1 = scheduleDB.Date1,
             //   Date2 = scheduleDB.Date2,
             //   Date3 = scheduleDB.Date3,
                OperationId = objOperationPlan.Operation == null ? 0:  Convert.ToInt32(objOperationPlan.Operation.Id)
               // ParentWorkOrderId=scheduleDB.BarNumber ?? 0,
               // RowNo = scheduleDB.Int3 ?? 0
                
               
                
                
                

            };
        }

      
    }
}
