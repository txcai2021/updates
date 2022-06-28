using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;



namespace SIMTech.APS.Operation.API.Repository
{
    using SIMTech.APS.Operation.API.Models;
    using SIMTech.APS.Operation.API.DBContext;
    using SIMTech.APS.Repository;

    public class ParameterRepository : Repository<Parameter>,  IParameterRepository
    {
        private readonly OperationContext _dbContext;
        public ParameterRepository(OperationContext context) : base(context) { _dbContext = context; }
    }
}
