﻿using System.Collections.Generic;

namespace SIMTech.APS.SalesOrder.API.Repository
{
    using SIMTech.APS.SalesOrder.API.Models;
    using SIMTech.APS.Repository;
    public interface ISalesOrderDetailRepository : IRepository<SalesOrderDetail>
    {
        public SalesOrderDetail GetSalesOrderLine(int salesOrderDetailId );
    }
}
