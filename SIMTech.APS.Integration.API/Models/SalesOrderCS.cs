using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Integration.RabbitMQ.Models
{
    
    public class SalesOrderCS
    {
        public SalesOrderCS()
        {
            OrderItems = new List<SalesOrderDetailsCS>();
        }
        public int OrderId { get; set; }
        public string OrderNo { get; set; }
        public string OrderStatus { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public List<SalesOrderDetailsCS> OrderItems;


    }

    public class SalesOrderDetailsCS
    {       
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Scent { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }
        public int? LineNumber { get; set; }
        public string Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }

    }
}
