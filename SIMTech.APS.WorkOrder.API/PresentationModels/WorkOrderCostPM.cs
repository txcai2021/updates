using System.ComponentModel.DataAnnotations;


namespace SIMTech.APS.WorkOrder.API.PresentationModels
{
    public class WorkOrderCostPM
    {

        public WorkOrderCostPM()
        {
           
        }

        [Key]       
        public int Id { get; set; }
      
        public int WorkOrderId { get; set; }

        public string WorkOrderNumber { get; set; }

        public string Category { get; set; }

        
        
        public string Description { get; set; }

        public string Remarks { get; set; }

        [Display(ShortName = "Unit Price", Name = "Unit Price")]
        public double UnitPrice { get; set; }

        public double Quantity { get; set; }

        [Display(ShortName = "Company Name", Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(ShortName = "PR No", Name = "PR No")]
        public string PRNo { get; set; }

          
        public double Cost { get; set; }


        public int productId { get; set; }
         
    }
}


 