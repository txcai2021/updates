using System.ComponentModel.DataAnnotations;
using SIMTech.APS.Resources;

namespace SIMTech.APS.WorkOrder.API.PresentationModels
{
    public class WOSalesDetailsPM
    {
        [Key]
        public int Id { get; set; }

        public string SONumber { get; set; }

       
        public int SONo { get; set; }

     
        public string string1 { get; set; }

        public string string2 { get; set; }

    }
}
