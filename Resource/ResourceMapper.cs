using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace SIMTech.APS.Resource.API.Mappers
{
    using SIMTech.APS.Resource.API.Models;
    using SIMTech.APS.Resource.API.PresentationModels;

    /// <summary>
    /// Maps  PMs (Presentation Models) to  BOs (Business Objects) and vice versa.
    /// </summary>
    public static class ResourceMapper
    {
        
        /// <summary>
        /// Transforms list of resource BOs list of resource PMs.
        /// </summary>
        /// <param name="resources">List of resource BOs.</param>
        /// <returns>List of resource PMs.</returns>
        public static IList<ResourcePM> ToPresentationModels(IEnumerable<Equipment> resources)
        {
            if (resources == null) return null;
            return resources.Select(u => ToPresentationModel(u)).ToList();
        }

        /// <summary>
        /// Transforms resource BO to resource PM.
        /// </summary>
        /// <param name="resource">resource BO.</param>
        /// <returns>resource PM.</returns>
        public static ResourcePM ToPresentationModel(Equipment resource)
        {
            if (resource == null) return null;

            return new ResourcePM
            {
                Id = resource.Id,
                Name = resource.EquipmentName,

                Type = resource.Type,
                TypeName = (resource.Type.HasValue ? (resource.Type == 1 ? "INHOUSE" : (resource.Type == 2 ? "QC" : (resource.Type == 3 ? "SUBCON" : "Automated"))) : ""),

                Category = resource.Category,
                Subcategory = resource.Subcategory,
                LocationId = resource.LocationId.Value,
                Description = resource.Description,
                String1 = resource.String1,
                Cost = resource.Float1,
                Quantity = resource.Float2,

                ParentResourceId = resource.ParentEquipmentId,
                CalendarId = resource.CalendarId,

                NoofBlockOuts = resource.EquipmentBlockOuts ==null? 0:resource.EquipmentBlockOuts.Count(),

                CreatedDate = resource.CreatedOn,
                ModifiedDate = resource.ModifiedOn

                //resourceParameterPMs = ResourceParameterMapper.ToPresentationModels(resource.EquipmentParameters)
                //resourceOperationPMs = ResourceParameterMapper.ToPresentationModels(resource.EquipmentOperations)
                
            };
        }

        
        /// <summary>
        /// Transforms list of resource PMs list of resource BOs.
        /// </summary>
        /// <param name="resourcePMs">List of resource PMs.</param>
        /// <returns>List of resource BOs.</returns>
        public static IList<Equipment> FromPresentationModels(IEnumerable<ResourcePM> resourcePMs)
        {
            if (resourcePMs == null) return null;
            return resourcePMs.Select(u => FromPresentationModel(u)).ToList();
        }

        /// <summary>
        /// Transforms resource PM to resource BO.
        /// </summary>
        /// <param name="resourcePM">resource PM.</param>
        /// <returns>resource BO.</returns>
        public static Equipment FromPresentationModel(ResourcePM resourcePM)
        {
            if (resourcePM == null) return null;

            return new Equipment
            {
                Id = resourcePM.Id,
                EquipmentName = resourcePM.Name,
                
                Type = resourcePM.Type,
                Category = resourcePM.Category,
                Subcategory = resourcePM.Subcategory,
                LocationId = resourcePM.LocationId,
                Description = resourcePM.Description,
                String1 = resourcePM.String1,
                Float1 = resourcePM.Cost ,
                Float2 = resourcePM.Quantity,

                ParentEquipmentId = resourcePM.ParentResourceId,
                CalendarId = resourcePM.CalendarId,

                CreatedOn = resourcePM.CreatedDate,
                ModifiedOn = DateTime.Today

                //EquipmentParameters = ResourceParameterMapper.FromPresentationModels(resourcePM.resourceParameterPMs)
                //EquipmentOperations = ResourceOperationMapper.FromPresentationModels(resourcePM.resourceOperationPMs)
            };
        }

              
    }
}
