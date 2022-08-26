
using System.Threading.Tasks;

namespace SIMTech.APS.Customer.API.Repository
{
    using SIMTech.APS.Customer.API.Models;
    using SIMTech.APS.Repository;
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<string> CheckCustomer(int customerId);
    }
}
