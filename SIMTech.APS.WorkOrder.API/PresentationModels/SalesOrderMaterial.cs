using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.WorkOrder.API.PresentationModels
{
    public class SalesOrderMaterial
    {
        public SalesOrderMaterial()
        {
            WorkOrders = new List<WorkOrderRequiredMaterial>();
        }
        public string Title { get; set; }
        public string FactoryId { get; set; }
        public string SalesOrderNo { get; set; }
        public int LineNo { get; set; }      
        public DateTime CreatedDate { get; set; }
        public int PartId { get; set; }
        public string PartNo { get; set; }
        public double Quantity { get; set; }
        public List<WorkOrderRequiredMaterial> WorkOrders { get; set; }
    
    }

    public class WorkOrderRequiredMaterial
    {
        public WorkOrderRequiredMaterial()
        {
            RequiredRawMaterials = new List<RequiredRawMaterial>();
        }


        public int WorkorderId { get; set; }
        public string WorkorderNumber { get; set; }      
        public int PartId { get; set; }
        public string PartNo { get; set; }
        public double Quantity { get; set; }
        public List<RequiredRawMaterial> RequiredRawMaterials { get; set; }
    }

    public class RequiredRawMaterial
    {
        public int RawMaterialId { get; set; }
        public string RawMaterialNo { get; set; }
        public double BalanceQuantity { get; set; }
        public double RequiredQuantity { get; set; }
        public int Availability { get; set; }
    }
}
