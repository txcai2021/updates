using System;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SIMTech.APS.Customer.API.Repository
{
    using SIMTech.APS.Customer.API.Models;
    using SIMTech.APS.Customer.API.DBContext;
    using SIMTech.APS.Repository;

    public class CustomerRepository : Repository<Customer>,  ICustomerRepository
    {
        private readonly CustomerContext _dbContext;
        public CustomerRepository(CustomerContext context) : base(context) { _dbContext = context; }

        public async Task<string>  CheckCustomer(int customerId)
        {
           
            var paraCustomerId = new SqlParameter
            {
                ParameterName = "@CustomerId",
                SqlDbType = System.Data.SqlDbType.Int,
                Value = customerId,
            };

            var paraResult = new SqlParameter
            {
                ParameterName = "@Result",
                SqlDbType = System.Data.SqlDbType.NVarChar,
                Size =200,
                Direction = System.Data.ParameterDirection.Output,
            };

            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync("usp_CheckCustomer @CustomerId,@Result OUTPUT", paraCustomerId, paraResult);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in  usp_CheckCustomer" + $"{ e.Message}");
                if (e.InnerException != null) Console.WriteLine($"{ e.InnerException.Message}");
                return "Error in checking customer";
            }


            return paraResult.Value.ToString ();
        }

    }
}
