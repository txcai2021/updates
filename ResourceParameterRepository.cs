using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace SIMTech.APS.Resource.API.Repository
{
    using SIMTech.APS.Resource.API.Models;
    using SIMTech.APS.Resource.API.DBContext;
    using SIMTech.APS.Repository;

    public class ResourceParameterRepository : Repository<EquipmentParameter>, IResourceParameterRepository
    {
        private readonly ResourceContext _dbContext;
        public ResourceParameterRepository(ResourceContext context) : base(context) { _dbContext = context; }
       
    }
}
