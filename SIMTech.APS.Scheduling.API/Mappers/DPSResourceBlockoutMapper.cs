using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SIMTech.APS.Scheduling.API.Mappers
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;
    public class DPSResourceBlockoutMapper
    {

        public static IList<DPSResourceBlockoutPM> ToPresentationModels(IEnumerable<EquipmentBlockOut> resources)
        {
            if (resources == null) return null;
            return resources.Select(u => ToPresentationModel(u)).ToList();
        }

        public static DPSResourceBlockoutPM ToPresentationModel(EquipmentBlockOut resource)
        {
            if (resource == null) return null;

            return new DPSResourceBlockoutPM
            {
                EquipmentBlockOutID = resource.EquipmentBlockOutID,
                EquipmentID = resource.EquipmentID,
                StartDate = resource.StartDate,
                EndDate = resource.EndDate,
                Remarks = resource.Remarks,
                BlockOutType = resource.BlockOutType,
                Value = resource.Value ?? 0,
                CreatedBy = resource.CreatedBy,
                CreatedOn = resource.CreatedOn

            };
        }

    }
}
