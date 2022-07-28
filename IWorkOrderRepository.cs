using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIMTech.APS.WorkOrder.API.Repository
{
    using SIMTech.APS.WorkOrder.API.Models;
    using SIMTech.APS.Repository;
    using SIMTech.APS.WorkOrder.API.Enums;
    public interface IWorkOrderRepository : IRepository<WorkOrder>
    {
        Task<IEnumerable<WorkOrder>> GetWorkOrders(int workOrderId=0, bool isParentWO = false);
        Task<IEnumerable<WorkOrder>> GetWorkOrdersByCategory(EWorkOrderCategory category, bool sortByDueDate=false, bool sortOrderDec=true, int role=0 , List<string> userList=null );
        Task<IEnumerable<WorkOrder>> GetWorkOrdersForRelease(int releasedDays);
        Task<IEnumerable<WorkOrder>> GetWorkOrdersForSchedule(int locationId, bool loadWip=true);
        Task<int> UpdateWorkOrderStatus(int workOrderId = 0);

    }
}
