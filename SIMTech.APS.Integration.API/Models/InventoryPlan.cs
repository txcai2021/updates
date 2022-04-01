using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Integration.RabbitMQ.Models
{
    public class InventoryPlan
    {
        public string Title { get; set; }
        public DateTime RebalancingPlanGenerationDate { get; set; }
        public string RequestedByFactoryID { get; set; }
        public string SalesOrderNo { get; set; }
        public int LineNo { get; set; }
        public int PartID { get; set; }
        public string PartNo { get; set; }
        public string FromWarehouse { get; set; }
        public string ToWarehouse { get; set; }
        public List<InventoryPlanDetails> RawMaterialRebalacingPlan { get; set; }
    }

    public class InventoryPlanDetails
    {
        public int RawMaterialId { get; set; }
        public string RawMaterialNo { get; set; }

        public double QuantityToTransfer { get; set; }

        public DateTime ETA { get; set; }

        public double Balance { get; set; }
    }

      
}
