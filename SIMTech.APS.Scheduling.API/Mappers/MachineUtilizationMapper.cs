using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SIMTech.APS.Scheduling.API.Mappers
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;

    public class MachineUtilizationMapper
    {
        public static IEnumerable<MachineUtilizationPM> ToPresentationModels(IEnumerable<MachineUtilizationPM> machinedetails)
        {
            if (machinedetails == null) return null;
            return machinedetails.Select(ToPresentationModel);
        }

        public static MachineUtilizationPM ToPresentationModel(MachineUtilizationPM machineDB)
        {
            if (machineDB == null) return null;

            return new MachineUtilizationPM
            {
                Id = machineDB.Id,
                OperationName = machineDB.EntityName,
                Machine = machineDB.SubEntity.Trim(),
                StartDate = machineDB.ScheduleDate,
                EndDate = machineDB.Date1,
                RunTime = machineDB.WIP,
                ScheduleType = machineDB.String1.Trim(),
                LoadType = machineDB.String2.Trim(),
                IsWIP = (machineDB.String2.Trim().ToUpper() == "WIP") ,
                //EquipmentId = (int)machineDB.Float1,
                Setup=0,
                SetupRatio=0,
                WIP=0,
                WIPRatio=0,
                WO=0,
                WORatio=0,
                Available = machineDB.Available,
                AvailableRatio = machineDB.AvailableRatio,
                ScheduleDate = machineDB.ScheduleDate,
                Date1 = machineDB.Date1,
                Date2 = machineDB.Date2,
                String1 = machineDB.String1,
                String2 = machineDB.String2,
                Float1 = machineDB.Float1,
                Float2 = machineDB.Float2
            };
        }

     
    }
}
