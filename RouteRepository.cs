using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Routing.API.Repository
{
    using SIMTech.APS.Routing.API.Models;
    using SIMTech.APS.Routing.API.DBContext;
    using SIMTech.APS.Repository;

    public class RouteRepository : Repository<Route>,  IRouteRepository
    {
        private readonly RouteContext _dbContext;
        public RouteRepository(RouteContext context) : base(context) { _dbContext = context; }

        public async Task<IEnumerable<Route>> GetRoutes(int routeId = 0, bool template = true)
        {
          
            var routes = _dbContext.Routes.Include(o => o.RouteOperationRoutes.OrderBy (ro=>ro.Sequence )).Include(r=>r.ProductRoutes).Where(x=>x.Id>0);
        
            if (routeId > 0)
            {
                routes = routes.Where(x => x.Id == routeId);
            }
            else if(template)
            {              
                    routes = routes.Where(x => x.Version == 1);
            }
                

            return await routes.ToListAsync();
        }

        public async Task<IEnumerable<Route>> GetRoutes(string routeIds)
        {
            var routeList = routeIds.Split(",");
            var routes = _dbContext.Routes.Include(o => o.RouteOperationRoutes).Where(x => routeList.Contains(x.Id.ToString ()));

            return await routes.ToListAsync();
        }

        public async Task<IEnumerable<Route>> GetRoutesbyOperation(int opeartionId)
        {
            var routes = _dbContext.Routes.Include(o => o.RouteOperationRoutes).Where(x => x.Version == 1);

            if (opeartionId > 0)
                routes = routes.Where(x => x.RouteOperationRoutes.Any(a => a.OperationId == opeartionId));

            return await routes.ToListAsync();
        }

    }
}
