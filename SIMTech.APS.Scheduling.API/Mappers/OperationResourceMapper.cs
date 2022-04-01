using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SIMTech.APS.Scheduling.API.Mappers
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;
    public class OperationResourceMapper
    {
        public static IEnumerable<DPSOperationResourcePM> ToPresentationModels(IEnumerable<OperationResource> operationResources)
        {
            if (operationResources == null) return null;
            return operationResources.Select(ToPresentationModel);
        }

        public static DPSOperationResourcePM ToPresentationModel(OperationResource operationResource)
        {
            if (operationResource == null) return null;
            return new DPSOperationResourcePM
            {
                OperationResourceId = operationResource.OperationResourceId,
                ResourceID = operationResource.ResourceID,
                OperationID = operationResource.OperationID,
                Isdefault = operationResource.IsDefault,
                Duration = operationResource.Duration ?? 0,
                DurationPer = operationResource.DurationPer,
                OperationName =operationResource.Operation == null ? "": operationResource.Operation.OperationName

            };
        }

        public static IEnumerable<DPSOpResDetailsPM> ToPresentationModels_DPS(IEnumerable<OperationResource> operationResources)
        {
            if (operationResources == null) return null;
            return operationResources.Select(ToPresentationModel_DPS);
        }


        public static DPSOpResDetailsPM ToPresentationModel_DPS(OperationResource operationResource)
        {
            if (operationResource == null) return null;
            return new DPSOpResDetailsPM
            {
                OperationID = operationResource.OperationID,
                Duration = operationResource.Duration ?? 0,

            };
        }

    }
}
