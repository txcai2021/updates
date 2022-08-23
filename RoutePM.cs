using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using SIMTech.APS.Resources;
using System.Collections.Generic;

namespace SIMTech.APS.Routing.API.PresentationModels
{
    public class RoutePM
    {
        public RoutePM()
        {
            RouteOperationPMs = new List<RouteOperationPM>();
            //ProductRoutePMs = new List<ProductRoutePM>();
        }

        [Key]
        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        public int Id { get; set; }

        //[Display(ShortName = "Name", ResourceType = typeof(SharedResources), Name = "Name", Order = 0)]
        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]
        [StringLength(250)]
        public string Name { get; set; }

        public byte Type { get; set; }

        [Display(ShortName = "Location", ResourceType = typeof(SharedResources), Name = "Location")]
        [Required(ErrorMessageResourceName = "ValidationErrorRequiredField", ErrorMessageResourceType = typeof(ErrorResources))]        
        public int? LocationId { get; set; }

        public string LocationName { get; set; }

        public Boolean Active { get; set; }

        public byte? TraceLevel { get; set; }

        public Boolean? TrackLevel { get; set; }

        public string Description { get; set; }

        public string Comment { get; set; }

         [Display(ShortName = "Created Date", Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(ShortName = "Modified Date", Name = "Modified Date")]
        public DateTime ? ModifiedDate { get; set; }

        [Display(ShortName = "Created by", Name = "Created by")]
        public string CreatedBy { get; set; }

        [Display(ShortName = "Modified by", Name = "Modified by")]
        public string ModifiedBy { get; set; }

        public int? Version { get; set; }

        public int operations { get; set; }

        public int subroutes { get; set; }

        //[Display(ShortName = "ProductFamily", ResourceType = typeof(SharedResources), Name = "ProductFamily")]
        [Display(ShortName = "Product Family", Name = "Product Family")]
        public string ProductFamily { get; set; }

        [Display(ShortName = "Assign Routing to Part", Name = "Assign Routing to Part")]
        public string PartName { get; set; }

        [Display(ShortName = "No. of Assigned Parts", Name = "No. of Assigned Parts")]
        public int NoofParts { get; set; }

        public string OperationList { get; set; }
        public List<int> PartList { get; set; }

        public List<RouteOperationPM> RouteOperationPMs { get; set; }

        //public List<ProductRoutePM> ProductRoutePMs { get; set; }

        #region Auxiliary methods

        #endregion
    }

    public class CopyRoute
    {
        public int RouteId { get; set; }
        public string NewRouteName { get; set; }
        public string Description { get; set; }
     
    }
}
