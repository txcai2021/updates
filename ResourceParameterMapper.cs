using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIMTech.APS.Resource.API.Mappers
{
    using SIMTech.APS.Resource.API.Models;
    using SIMTech.APS.Resource.API.PresentationModels;

    /// <summary>
    /// Maps  PMs (Presentation Models) to  BOs (Business Objects) and vice versa.
    /// </summary>
    public static class ResourceParameterMapper
    {

        /// <summary>
        /// Transforms list of resourceparameter BOs list of resourceparameter PMs.
        /// </summary>
        /// <param name="resourceparameters">List of resourceparameter BOs.</param>
        /// <returns>List of resourceparameter PMs.</returns>
        public static IList<ResourceParameterPM> ToPresentationModels(IEnumerable<EquipmentParameter> resourceparameters)
        {
            if (resourceparameters == null) return null;
            return resourceparameters.Select(u => ToPresentationModel(u)).ToList();
        }

        /// <summary>
        /// Transforms resourceparameter BO to resourceparameter PM.
        /// </summary>
        /// <param name="resourceparameter">resourceparameter BO.</param>
        /// <returns>resourceparameter PM.</returns>
        public static ResourceParameterPM ToPresentationModel(EquipmentParameter resourceparameter)
        {
            if (resourceparameter == null) return null;

            return new ResourceParameterPM
            {
                Id = resourceparameter.Id,

                ResourceId = resourceparameter.EquipmentId,
                ParameterId = resourceparameter.ParameterId,
                
                ParameterDefaultValue = resourceparameter.Value,
                ParameterMinValue = resourceparameter.MinValue,
                ParameterMaxValue = resourceparameter.MaxValue,

                CreatedDate = resourceparameter.CreatedOn,
               
                //OperationResources = ToPresentationModels(resourceparameter.OperationResources),
            };
        }


        /// <summary>
        /// Transforms list of resourceparameter PMs list of resourceparameter BOs.
        /// </summary>
        /// <param name="resourceoperationPMs">List of resourceparameter PMs.</param>
        /// <returns>List of resourceparameter BOs.</returns>
        public static IList<EquipmentParameter> FromPresentationModels(IEnumerable<ResourceParameterPM> resourceOperationPMs)
        {
            if (resourceOperationPMs == null) return null;
            return resourceOperationPMs.Select(u => FromPresentationModel(u)).ToList();
        }

        /// <summary>
        /// Transforms resourceparameter PM to resourceparameter BO.
        /// </summary>
        /// <param name="resourceoperationPM">resourceparameter PM.</param>
        /// <returns>resourceparameter BO.</returns>
        public static EquipmentParameter FromPresentationModel(ResourceParameterPM resourceParameterPM)
        {
            if (resourceParameterPM == null) return null;

            return new EquipmentParameter
            {
                Id = resourceParameterPM.Id,
                
                ParameterId = resourceParameterPM.ParameterId,
                EquipmentId = resourceParameterPM.ResourceId,

                Value = resourceParameterPM.ParameterDefaultValue,
                MinValue = resourceParameterPM.ParameterMinValue,
                MaxValue = resourceParameterPM.ParameterMaxValue,

                CreatedOn = resourceParameterPM.CreatedDate,
                ModifiedOn = resourceParameterPM.ModifiedDate

                //EquipmentParameter = FromPresentationModels(resourceoperationPM.OperationResources)
                
            };
        }

      
    }
}
