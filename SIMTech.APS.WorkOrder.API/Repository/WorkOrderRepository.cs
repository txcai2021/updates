using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SIMTech.APS.WorkOrder.API.Repository
{
    using SIMTech.APS.WorkOrder.API.Models;
    using SIMTech.APS.WorkOrder.API.DBContext;
    using SIMTech.APS.Repository;
    using SIMTech.APS.WorkOrder.API.Enums;

    public class WorkOrderRepository : Repository<WorkOrder>,  IWorkOrderRepository
    {
        private readonly WorkOrderContext _dbContext;
        public WorkOrderRepository(WorkOrderContext context) : base(context) { _dbContext = context; }

        
        public async Task<IEnumerable<WorkOrder>> GetWorkOrders( int workOrderId=0, bool isParentWO=false)
        {          
            var workOrders = _dbContext.WorkOrders.Include(wo => wo.WorkOrderDetails).Include(wo => wo.WorkOrderMaterials).Where (wo=>wo.Id >0);

            if (workOrderId > 0)
            {
                if (isParentWO)
                    workOrders = workOrders.Where(wo => wo.ParentWorkOrderId == workOrderId);              
                else
                    workOrders = workOrders.Where(wo => wo.Id == workOrderId); ;
            }
               

            return await workOrders.ToListAsync();
        }

        public async Task<IEnumerable<WorkOrder>>  GetWorkOrdersForRelease(int releasedDays)
        {
            var startDate = DateTime.Today.AddDays(-releasedDays);

            var workOrders = _dbContext.WorkOrders.Include(wo => wo.WorkOrderDetails).Include(wo => wo.WorkOrderMaterials).Where(wo => wo.Status == (byte)EWorkOrderStatus.Pending || (wo.Status == (byte)EWorkOrderStatus.Released && wo.Date1 != null && wo.Date1 > startDate));

            return await workOrders.ToListAsync();

        }

        public async Task<IEnumerable<WorkOrder>> GetWorkOrdersForSchedule(int locationId =0, bool loadWIP=true)
        {
            IQueryable<WorkOrder> workOrders;
            
            if (loadWIP)
                workOrders = _dbContext.WorkOrders.Include(wo => wo.WorkOrderDetails).Include(wo => wo.WorkOrderMaterials).Where(wo => wo.Status >= (byte)EWorkOrderStatus.Released && wo.Status < 200);
            else
                workOrders = _dbContext.WorkOrders.Include(wo => wo.WorkOrderDetails).Include(wo => wo.WorkOrderMaterials).Where(wo => wo.Status == (byte)EWorkOrderStatus.Released );
                      
            if (locationId > 0)
                workOrders = workOrders.Where(wo => wo.LocationId == locationId);

           

            return await workOrders.ToListAsync();
        }

    }
}
