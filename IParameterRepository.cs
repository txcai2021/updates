using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace SIMTech.APS.Operation.API.Repository
{
    using SIMTech.APS.Operation.API.Models;
    using SIMTech.APS.Repository;
    public interface IParameterRepository : IRepository<Parameter>
    {    
    }

}
