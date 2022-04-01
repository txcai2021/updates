using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIMTech.APS.WorkOrder.API.Repository
{
    using SIMTech.APS.WorkOrder.API.Models;
    using SIMTech.APS.Repository;
    public interface IWorkOrderRepository : IRepository<WorkOrder>
    {
        Task<IEnumerable<WorkOrder>> GetWorkOrders(int workOrderId=0, bool isParentWO = false);
        Task<IEnumerable<WorkOrder>> GetWorkOrdersForRelease(int releasedDays);
        Task<IEnumerable<WorkOrder>> GetWorkOrdersForSchedule(int locationId, bool loadWip=true);
    }
}
