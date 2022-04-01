using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace SIMTech.APS.WorkOrder.API.Repository
{
    using SIMTech.APS.WorkOrder.API.Models;
    using SIMTech.APS.WorkOrder.API.DBContext;
    using SIMTech.APS.Repository;

    public class WorkOrderDetailRepository : Repository<WorkOrderDetail>,  IWorkOrderDetailRepository
    {
        private readonly WorkOrderContext _dbContext;
        public WorkOrderDetailRepository(WorkOrderContext context) : base(context) { _dbContext = context; }
       
    }
}
