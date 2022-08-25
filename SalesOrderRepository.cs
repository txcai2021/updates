using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

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

    }
}
