using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;


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

        public async Task<IEnumerable<WorkOrder>> GetWorkOrdersByCategory(EWorkOrderCategory category=EWorkOrderCategory.All, bool sortByDueDate=false, bool sortOrderDec=true, int role=0, List<string> userList = null)
        {

            var workOrders = _dbContext.WorkOrders.Include(wo => wo.WorkOrderDetails).Include(wo => wo.WorkOrderMaterials).Where(wo => wo.Id > 0);

            if (sortByDueDate )
            {
                switch (category)
                {
                    case EWorkOrderCategory.Completed:
                        //workOrders = workOrders.Where(wo => wo.Int3 != null && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);
                        workOrders = workOrders.Where(wo => wo.Status ==(byte)EWorkOrderStatus.Completed && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);
                        break;
                    case EWorkOrderCategory.WIP:                       
                        //workOrders = workOrders.Where(wo => wo.Int3 == null && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);
                        //workOrders = workOrders.Where(wo => (wo.Status == (byte)EWorkOrderStatus.Processing || wo.Status == (byte)EWorkOrderStatus.Queuing )&& (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);
                        workOrders = workOrders.Where(wo => wo.Status != (byte)EWorkOrderStatus.Completed && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);
                        break;
                    default:                  
                        workOrders = workOrders.Where(wo => role == 0 || userList.Contains(wo.String7)).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);                      
                        break;
                }
            }
            else
            {              
                if (sortOrderDec)
                {
                    switch (category)
                    {
                        case EWorkOrderCategory.Completed:
                            //workOrders = workOrders.Where(wo => wo.Int3 != null && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);
                            workOrders = workOrders.Where(wo => wo.Status == (byte)EWorkOrderStatus.Completed && (role == 0 || userList.Contains(wo.String7))).OrderByDescending(x => x.WorkOrderNumber);
                            break;
                        case EWorkOrderCategory.WIP:
                            //workOrders = workOrders.Where(wo => wo.Int3 == null && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);
                            workOrders = workOrders.Where(wo => wo.Status != (byte)EWorkOrderStatus.Completed && (role == 0 || userList.Contains(wo.String7))).OrderByDescending(x => x.WorkOrderNumber);
                            break;
                        default:
                            workOrders = workOrders.Where(wo => role == 0 || userList.Contains(wo.String7)).OrderByDescending(x => x.WorkOrderNumber);
                            break;
                    }
                }
                else
                {
                    switch (category)
                    {
                        case EWorkOrderCategory.Completed:
                            //workOrders = workOrders.Where(wo => wo.Int3 != null && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);
                            workOrders = workOrders.Where(wo => wo.Status == (byte)EWorkOrderStatus.Completed && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.WorkOrderNumber);
                            break;
                        case EWorkOrderCategory.WIP:
                            //workOrders = workOrders.Where(wo => wo.Int3 == null && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.DueDate).ThenBy(x => x.WorkOrderNumber);
                            workOrders = workOrders.Where(wo => wo.Status != (byte)EWorkOrderStatus.Completed && (role == 0 || userList.Contains(wo.String7))).OrderBy(x => x.WorkOrderNumber);
                            break;
                        default:
                            workOrders = workOrders.Where(wo => role == 0 || userList.Contains(wo.String7)).OrderBy(x => x.WorkOrderNumber);
                            break;
                    }
                }

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


        public async Task<int> UpdateWorkOrderStatus(int workOrderId = 0)
        {

            var result = 0;
           
            try
            {
                 result = await _dbContext.Database.ExecuteSqlRawAsync("sp_UpdateWorkOrderStatus @WorkOrderID", new SqlParameter("@WorkOrderID", workOrderId));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in  sp_UpdateWorkOrderStatus" + $"{ e.Message}");
                if (e.InnerException!=null) Console.WriteLine($"{ e.InnerException.Message}");
            }


            return result;
        }
    }
}
