using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
   public class ResourcePM
    {
       public ResourcePM()
       {

       }

       [Key]
       public int ResourceId { get; set; }

       public int LocationId { get; set; }
       public int Level { get; set; }

       public string EquipmentName { get; set; }
       public string Description { get; set; }
       public DateTime CreatedDate { get; set; }
       public string Subcategory { get; set; }
       public string Category { get; set; }
       public string FirstOperationName { get; set; }
       public IEnumerable<DPSOperationResourcePM> OperationResourcePMs { get; set; }
     
    }
}
