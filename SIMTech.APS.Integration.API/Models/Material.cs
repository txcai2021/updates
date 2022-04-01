using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Integration.RabbitMQ.Models
{
    public class Material
    {
        public int Id { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }

        public int? CustomerId { get; set; }
        public DateTime DateIn { get; set; }
        public int PartId { get; set; }
        public string PartNo { get; set; }
        public string Remarks { get; set; }
        public decimal? CompletedQty { get; set; }
    }
}
