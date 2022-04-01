using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIMTech.APS.Scheduling.API.Repository
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Repository;
    public interface IScheduleRepository : IRepository<Schedule>
    {
        Task<IEnumerable<Schedule>> GetSchedules(int scheduleId = 0);
    }

    
}
