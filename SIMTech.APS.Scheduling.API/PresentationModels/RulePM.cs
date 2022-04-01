using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SIMTech.APS.Scheduling.API.PresentationModels
{
   public class RulePM
    {
       public RulePM()
       {

       }

       [Key]
       public int Id { get; set; }
     
       public string Name { get; set; }
       public string Description { get; set; }              
       public string Category { get; set; }

       public int LinkedRuleId { get; set; }     
    }
}
