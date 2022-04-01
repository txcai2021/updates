using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SIMTech.APS.Scheduling.API.Mappers
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;
    public class UnScheduleDetailsMapper
    {
        public static IEnumerable<ReportSchedulePM> ToPresentationModels(IEnumerable<UnscheduledWorkOrders> scheduleItems)
        {
            if (scheduleItems == null) return null;
            return scheduleItems.Select(ToPresentationModel);
        }

        public static ReportSchedulePM ToPresentationModel(UnscheduledWorkOrders scheduleDB)
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
                DueDate = scheduleDB.DueDate,
                ReleaseDate = scheduleDB.ReleaseDate,
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
                ScheduleDetailId = scheduleDB.ScheduleDetailId,
                Int1 = scheduleDB.Int1,              
                Int3 = scheduleDB.Int3,
                Date1 = scheduleDB.Date1,
                Date2 = scheduleDB.Date2,
                Date3 = scheduleDB.Date3,
                OperationId = scheduleDB.OperationId,
                ParentWorkOrderId=scheduleDB.ParentWorkOrderId,
                RowNo = scheduleDB.RowNo,
                IsListBoxDisable=scheduleDB.Flag1 == 1 ? true : false

            };
        }

      
    }
}
