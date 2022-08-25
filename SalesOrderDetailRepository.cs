using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace SIMTech.APS.SalesOrder.API.Repository
{
    using SIMTech.APS.SalesOrder.API.Models;
    using SIMTech.APS.SalesOrder.API.DBContext;
    using SIMTech.APS.Repository;

    public class SalesOrderDetailRepository : Repository<SalesOrderDetail>,  ISalesOrderDetailRepository
    {
        private readonly SalesOrderContext _dbContext;
        public SalesOrderDetailRepository(SalesOrderContext context) : base(context) { _dbContext = context; }

        public SalesOrderDetail GetSalesOrderLine(int salesOrderDetailId )
        {
            var salesOrderDetail = _dbContext.SalesOrderDetails.Include(s => s.SalesOrder).Where(s => s.Id== salesOrderDetailId).FirstOrDefault();
            return salesOrderDetail;
        }

    }
}
