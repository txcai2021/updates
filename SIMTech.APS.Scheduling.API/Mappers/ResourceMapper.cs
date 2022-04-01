using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SIMTech.APS.Scheduling.API.Mappers
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;
    public static class ResourceMapper
    {
        public static IList<ResourcePM> ToPresentationModels(IEnumerable<Equipment> resources)
        {
            if (resources == null) return null;
            return resources.Select(u => ToPresentationModel(u)).ToList();
        }

        public static ResourcePM ToPresentationModel(Equipment resource)
        {
            if (resource == null) return null;

            return new ResourcePM
            {
                ResourceId = resource.EquipmentId,
                EquipmentName = resource.EquipmentName,

               // Type = resource.Type,
                //TypeName = (resource.Type.HasValue ? (resource.Type == 1 ? "InHouse" : (resource.Type == 2 ? "QC" : "SubCon")) : ""),

                Category = resource.Category,
                Subcategory = resource.Subcategory,
                LocationId = resource.LocationID.Value,
                Description = resource.Description,

                CreatedDate = resource.CreatedOn

            };
        }
    }
}
