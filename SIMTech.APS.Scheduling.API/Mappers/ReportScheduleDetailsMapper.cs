using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SIMTech.APS.Scheduling.API.Mappers
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;
    public class ReportScheduleDetailsMapper
    {
        public static IEnumerable<ReportSchedulePM> ToPresentationModels(IEnumerable<DPSReport> scheduleItems)
        {
            if (scheduleItems == null) return null;
            return scheduleItems.Select(ToPresentationModel);
        }

        public static ReportSchedulePM ToPresentationModel(DPSReport scheduleDB)
        {
            if (scheduleDB == null) return null;

            return new ReportSchedulePM
            {
                BeginDate = scheduleDB.StartDate,
                EndDate = scheduleDB.EndDate,
                WorkOrderId = scheduleDB.WorkOrderId,
                WorkOrderNumber = scheduleDB.WorkOrderNumber,
                PartNumber = scheduleDB.PartName,
                MachineId = scheduleDB.EquipmentID,
                MachineName = scheduleDB.MachineName,
                Runtime = scheduleDB.RunTime,
                UnitRunTime = scheduleDB.UnitRunTime,
                Quantity = scheduleDB.Quantity,
                DueDate = scheduleDB.Date1,
                ReleaseDate = scheduleDB.Date2,
                Hotness = scheduleDB.Priority,
                LateJob = scheduleDB.LateJob,
                Duration = scheduleDB.Duration,
                ProcessingDuration=scheduleDB.ProcessingDuration,
                ScheduleType=scheduleDB.ScheduleType,
                OperationName= scheduleDB.OperationName,
                ShiftName=scheduleDB.ShiftName,
                OperationSequence = scheduleDB.OperationSequence,
                BarNumber = scheduleDB.BarNumber,
                String1 = scheduleDB.String1,
                String2 = scheduleDB.String2,
                String3 = scheduleDB.String3,
                Operator = scheduleDB.String3,
                ScheduleDetailId = scheduleDB.Int1 ?? 0,
                Int1 = scheduleDB.Int1,              
                Int3 = scheduleDB.Int3,
                Date1 = scheduleDB.Date1,
                Date2 = scheduleDB.Date2,
                Date3 = scheduleDB.Date3,
                OperationId = scheduleDB.Int2,
                ParentWorkOrderId=scheduleDB.BarNumber ?? 0,
                RowNo = scheduleDB.Int3 ?? 0
                
               
                
                
                

            };
        }

        public static ScheduleDetail FromPresentationModel(ReportSchedulePM schedulePM)
        {
            if (schedulePM == null) return null;

            return new ScheduleDetail
            {
                ScheduleId = schedulePM.ScheduleId,
                StartDate = schedulePM.BeginDate,
                EndDate = schedulePM.EndDate,
                ScheduleType = schedulePM.ScheduleType,
                OperationId = schedulePM.OperationId ?? 0,
                WorkOrderNumber = schedulePM.WorkOrderNumber,
                WorkOrderId = schedulePM.WorkOrderId,
                OperationName = schedulePM.OperationName,
                EquipmentName = schedulePM.MachineName,
                EquipmentId = schedulePM.MachineId ?? 0,
                Sequence = schedulePM.OperationSequence,
                ShiftName = schedulePM.ShiftName,
                RunTime = schedulePM.Runtime ?? 0,
                UnitRunTime = schedulePM.UnitRunTime ?? 0,
                Quantity = schedulePM.Quantity,
                WorkOrderDueDate = schedulePM.DueDate,
                String1 = schedulePM.String1,
                String2 = schedulePM.String2,
                String3 = schedulePM.String3,
                String4 = schedulePM.String4,
                String5 = schedulePM.String5

            };
        }
    }
}
