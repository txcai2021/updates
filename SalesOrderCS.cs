using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

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
        public string PersonalizedMsg { get; set; }
        

    }

    public class WorkOrderReleasePM
    {
        [Key]
        public int workOrderId { get; set; }
        public int Status { get; set; }
        public DateTime? ReleasedDate { get; set; }

    }

    public class SalesOrderStatus
    {
        public string SalesOrderNumber { get; set; }
        public int LineNo { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }

    }
}
