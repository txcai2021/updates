using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SIMTech.APS.SalesOrder.API.Repository
{
    using SIMTech.APS.SalesOrder.API.Models;
    using SIMTech.APS.SalesOrder.API.DBContext;
    using SIMTech.APS.Repository;

    public class SalesOrderRepository : Repository<SalesOrder>,  ISalesOrderRepository
    {
        private readonly SalesOrderContext _dbContext;
        public SalesOrderRepository(SalesOrderContext context) : base(context) { _dbContext = context; }

        public IEnumerable<SalesOrder> GetSalesOrders(int salesOrderId =0)
        {

            var salesOrders = _dbContext.SalesOrders.Include(s => s.SalesOrderDetails).Where (s=>s.Id>0);

            if (salesOrderId > 0) salesOrders= salesOrders.Where(x => x.Id == salesOrderId);

            return salesOrders.OrderByDescending(x => x.CreatedOn).ToList();
        }

        public IEnumerable<SalesOrder> GetSalesOrders(string salesOrderLineIds)
        {
            var solIds = new List<int>();

            try
            {
                solIds = salesOrderLineIds.Split(",").Select(x => Int32.Parse(x)).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //var soIds = _dbContext.SalesOrders.Include(s => s.SalesOrderDetails).Where(s => solIds.Contains(s.Id));         
            //var salesOrders = _dbContext.SalesOrderDetails.Include(s => s.SalesOrder).Where( y => solIds.Contains(y.Id)).Select(x=>x.SalesOrder).Distinct().ToList();

            var salesOrders = _dbContext.SalesOrders.Include(s => s.SalesOrderDetails).Where(y => y.SalesOrderDetails.Any(x=> solIds.Contains(x.Id))).ToList();
            foreach (var so in salesOrders)
            {
                foreach (var sod in so.SalesOrderDetails.Where(x => !solIds.Contains(x.Id)).ToList())
                    so.SalesOrderDetails.Remove(sod);
            }



            return salesOrders.OrderByDescending(x => x.CreatedOn).ToList();
        }

    }
}
