﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace SIMTech.APS.Scheduling.API.Repository
{
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;
    using SIMTech.APS.Scheduling.API.DBContext;
    using SIMTech.APS.Repository;
    using Microsoft.Data.SqlClient;

    public class ScheduleRepository : Repository<Schedule>,  IScheduleRepository
    {
        private readonly ScheduleContext _dbContext;
        public ScheduleRepository(ScheduleContext context) : base(context) { _dbContext = context; }

        public async Task<IEnumerable<Schedule>> GetSchedules(int id = 0)
        {

            var schedules = _dbContext.Schedules.Include(o => o.ScheduleDetails).Where(x => x.Id > 0);

            if (id > 0)
            {
                schedules = schedules.Where(x => x.Id == id);
            }

            //return await schedules.ToListAsync();
            return RemoveCycleReference(await schedules.ToListAsync());
        }

        private List<Schedule> RemoveCycleReference(List<Schedule> schedules)
        {
            foreach (var schedule in schedules)
            {
                foreach (var det in schedule.ScheduleDetails)
                {
                    det.Schedule = null;
                }
            }

            return schedules;
        }

        public async Task<int> DeleteDemoData(int deleteSO = 0)
        {
           var result = await _dbContext.Database.ExecuteSqlRawAsync("sp_DeleteDemoData @deleteSO", new SqlParameter("@deleteSO", deleteSO));

            return result;
        }

        public async Task<int> TrackingDispatchWOs(int ScheduleId)
        {

            var result = 0;

            try
            {
                result = await _dbContext.Database.ExecuteSqlRawAsync("sp_TrackingDispatchWOs @ScheduleId", new SqlParameter("@ScheduleId", ScheduleId));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in  sp_TrackingDispatchWOs" + $"{ e.Message}");
                // if (e.InnerException != null) Console.WriteLine($"{ e.InnerException.Message}");
                throw;
            }


            return result;
        }

    }
}
