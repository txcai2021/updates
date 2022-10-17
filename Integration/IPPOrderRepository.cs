using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SIMTech.APS.Integration.API.Repository
{
    using SIMTech.APS.Integration.API.Models;
    using SIMTech.APS.Repository;
    public interface IPPOrderRepository 
    {
        Task<IEnumerable<Pporder>> GetAll();
        Task<Pporder> GetbyId(int id);
        Task<IEnumerable<Pporder>> GetbyScheduleId(int schedueId);
        Task Delete(int id);
        Task DeleteByWorkOrder(int workOrderId);
        Task DeleteByWorkOrderNumber(string workOrderNumber);
        Task Insert<T>(T entity ) where T : class;
        int GetPPId(string workOrderNumber);

        Task DeleteByScheduleId(int scheduleId);

        Task<int> GeneratePPRoute(int routeId, int ppOrderId, string partNo);

        PporderRoute GetPPRoutebyWO(string workOrderNumber, int routeId, int woSequence);

        Task Update<T>(T entity) where T : class;

    }
}
