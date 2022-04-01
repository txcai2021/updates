using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace SIMTech.APS.Scheduling.API.Repository
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.DBContext;
    using SIMTech.APS.Repository;

    public class ScheduleDetailRepository : Repository<ScheduleDetail>,  IScheduleDetailRepository
    {
        private readonly ScheduleContext _dbContext;
        public ScheduleDetailRepository(ScheduleContext context) : base(context) { _dbContext = context; }
       
    }
}
