using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SIMTech.APS.Scheduling.API.Mappers
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;
    public class ScheduleHistoryMapper
    {
        public static IEnumerable<SchedulePM> ToPresentationModels(IEnumerable<Schedule> schedules)
        {
            if (schedules == null) return null;
            return schedules.Select(ToPresentationModel);
        }

        public static SchedulePM ToPresentationModel(Schedule schedule)
        {
            if (schedule == null) return null;

            return new SchedulePM
            {
                Id = schedule.Id,
                UnitId = schedule.UnitId,
                Version = schedule.Version,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                RuleSetId1 = schedule.Rule1,
                RuleSetId2 = schedule.Rule2,
                CreatedOn = schedule.CreatedOn,
                CreatedBy = schedule.CreatedBy,
                Objective = schedule.String1,
                JobRules = schedule.String2,
                MachineRules = schedule.String3,
                MakeSpan = schedule.String4,
                LateJobs = schedule.MaxString1,
                UnassignedJobs = schedule.MaxString2,
                ChangeOverTimes = schedule.MaxString3,
                Remarks = schedule.MaxString4,
                //IsScheduleEditAllowed = true,
                //IsConfirmAllowed = true,
                //IsReRunScheduleAllowed = false,
                ScheduleDetails = ScheduleHistoryMapper.ToPresentationModels(schedule.ScheduleDetails.Where (x=>x.ScheduleType.Trim() !="A")).ToList (),
                IsConfirmed = ((schedule.Confirmed ?? 0) == 0) ? false : schedule.Confirmed ==1 ? true:false
            };
        }

        public static IEnumerable<ScheduleDetailPM> ToPresentationModels(IEnumerable<ScheduleDetail> scheduleDetails)
        {
            if (scheduleDetails == null) return null;
            return scheduleDetails.Select(ToPresentationModel);
        }

        public static ScheduleDetailPM ToPresentationModel(ScheduleDetail scheduleDetail)
        {
            if (scheduleDetail == null) return null;

            return new ScheduleDetailPM
            {
                Id = scheduleDetail.Id,    
                ScheduleId = scheduleDetail.ScheduleId,
                ScheduleType = scheduleDetail.ScheduleType.Trim(),      
                StartDate = scheduleDetail.StartDate,     
                EndDate = scheduleDetail.EndDate,     
                ShiftName = scheduleDetail.ShiftName,     
                RouteId =scheduleDetail.RouteId,    
                RouteName = scheduleDetail.RouteName,    
                Sequence = scheduleDetail.Sequence, 
                OperationId = scheduleDetail.OperationId,     
                OperationName = scheduleDetail.OperationName,     
                OperationType = scheduleDetail.OperationType,     
                EquipmentId = scheduleDetail.EquipmentId,     
                EquipmentName = scheduleDetail.EquipmentName,   
                EquipmentType = scheduleDetail.EquipmentType,    
                WorkOrderId = scheduleDetail.WorkOrderId,     
                WorkOrderNumber = scheduleDetail.WorkOrderNumber  ,   
                WorkOrderDueDate = scheduleDetail.WorkOrderDueDate ,    
                ItemId = scheduleDetail.ItemId,      
                ItemName = scheduleDetail.ItemName,    
                UnitRunTime = scheduleDetail.UnitRunTime,    
                RunTime = scheduleDetail.RunTime,   
                Quantity = scheduleDetail.Quantity,     
                OperatorId = scheduleDetail.OperationId,     
                OperatorName = scheduleDetail.OperatorName,     
                SkillName = scheduleDetail.SkillName,    
                Status = scheduleDetail.Status,     
                WorkOrderCategory = scheduleDetail.String1

            };
        }

        public static Schedule FromPresentationModel(SchedulePM schedulePM)
        {
            if (schedulePM == null) return null;

            return new Schedule
            {
                Id= schedulePM.Id,
                StartDate = schedulePM.StartDate,
                EndDate = schedulePM.EndDate,
              //  RuleSetID = schedulePM.RuleSetID,
                Version = schedulePM.Version,
                UnitId = schedulePM.UnitId,
                CreatedBy = schedulePM.CreatedBy

            };
        }
    }
}
