using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Integration.API.Repository
{
    using SIMTech.APS.Integration.API.Models;
    using SIMTech.APS.Integration.API.DBContext;
    using SIMTech.APS.Repository;

    public class PPOrderRepository :  IPPOrderRepository
    {
        private readonly IntegrationContext _dbContext;
        public PPOrderRepository(IntegrationContext context)  { _dbContext = context; }

        public async Task<IEnumerable<Pporder>> GetAll()
        {
            var ppOrders = await _dbContext.Pporders.Include(x => x.PporderDetails).Include(x => x.PporderRoutes).ToListAsync();
            

            return RemoveCycleRefernce(ppOrders);
        }

        public int GetPPId(string workOrderNumber)
        {
            int ppId = 0;

            var a=_dbContext.PporderDetails.Where(x => x.SublotId == workOrderNumber).FirstOrDefault ();

            if (a != null) ppId= a.Ppid??0;
            return ppId;
        }

        public async Task<Pporder> GetbyId(int id)
        {
            var ppOrder = await _dbContext.Pporders.Include(x => x.PporderDetails).Include(x => x.PporderRoutes).FirstOrDefaultAsync(x => x.Id == id);

            return RemoveCycleRefernce(ppOrder);
        }

        public async Task<IEnumerable<Pporder>> GetbyScheduleId(int id)
        {
            var ppOrdersQuery =  _dbContext.Pporders.Include (x=>x.PporderDetails ).Include (x=>x.PporderRoutes );

            var ppOrders = new List<Pporder>();


            if (id == 0)
            {
                ppOrders = await ppOrdersQuery.Where(x => x.PporderRoutes.Any(y => y.ScheduleId == null || y.ScheduleId == 0)).ToListAsync();
            }
            else
            {
                ppOrders = await ppOrdersQuery.Where(x => x.PporderRoutes.Any(y => y.ScheduleId != null && (y.ScheduleId ?? 0) == id)).ToListAsync();
                foreach (var ppOrder in ppOrders)
                {
                    var ppRoutes = ppOrder.PporderRoutes.Where(x => x.ScheduleId == null || x.ScheduleId != id).ToList();
                    foreach (var ppRoute in ppRoutes)
                        ppOrder.PporderRoutes.Remove(ppRoute);
                }
            }

          
               

            return RemoveCycleRefernce(ppOrders);
        }

       

        private List<Pporder> RemoveCycleRefernce(List<Pporder> pporders)
        {
            foreach (var pporder in pporders)
            {
                RemoveCycleRefernce( pporder);
            }
            return pporders;
        }

        private Pporder RemoveCycleRefernce( Pporder pporder)
        {
            
            foreach (var det in pporder.PporderDetails)
                det.Pp = null;
            foreach (var route in pporder.PporderRoutes)
                route.Pp = null;

            return pporder;

        }

        public async Task Insert<T>(T entity)  where T : class
        {
             var entities = _dbContext.Set<T>();

            if (entity != null)
            {
                entities.Add(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteByScheduleId(int scheduleId)
        {
            if (scheduleId > 0)
            {
                var ppOrderRoutes = await _dbContext.PporderRoutes.Where (x=>x.ScheduleId !=null && x.ScheduleId ==scheduleId ).ToListAsync();

                if (ppOrderRoutes != null && ppOrderRoutes.Count>0)
                {
                    _dbContext.PporderRoutes.RemoveRange(ppOrderRoutes);
                    await _dbContext.SaveChangesAsync();
                }
            }

        }

        public async Task Delete(int id)
        {           
            if (id > 0)
            {
                var ppOrder = _dbContext.Pporders.Include(x => x.PporderDetails).Include(x => x.PporderRoutes).SingleOrDefault(s => s.Id == id);

                if (ppOrder != null)
                {
                   if (ppOrder.PporderDetails.Count>0)
                    {
                        _dbContext.PporderDetails.RemoveRange(ppOrder.PporderDetails);
                        await _dbContext.SaveChangesAsync();
                    }

                    if (ppOrder.PporderRoutes.Count > 0)
                    {
                        _dbContext.PporderRoutes.RemoveRange(ppOrder.PporderRoutes);
                        await _dbContext.SaveChangesAsync();
                    }

                    _dbContext.Pporders.Remove(ppOrder);
                     await _dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteByWorkOrder(int workOrderId)
        {
            if (workOrderId > 0)
            {
                var ppOrderDetail = _dbContext.PporderDetails.SingleOrDefault(s => s.SalesOrderId == workOrderId);

                if (ppOrderDetail != null) await Delete(ppOrderDetail.Ppid??0);

                var woMacs = _dbContext.WorkorderMacs.Where(x => x.Woid == workOrderId.ToString()).ToList();

                if (woMacs!=null && woMacs.Count>0)
                {
                    _dbContext.WorkorderMacs.RemoveRange(woMacs);
                    await _dbContext.SaveChangesAsync();
                } 
            }
        }

    }
}
