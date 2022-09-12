using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;


namespace SIMTech.APS.Customer.API.Controllers
{
    using SIMTech.APS.Customer.API.Repository;
    using SIMTech.APS.Customer.API.Mappers;
    using SIMTech.APS.Customer.API.Models;
    using SIMTech.APS.Customer.API.PresentationModels;
    using SIMTech.APS.PresentationModels;
    //using SIMTech.APS.Customer.API.PresentationModels;

    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerController(ICustomerRepository CustomerRepository)
        {
            _customerRepository = CustomerRepository;
        }


        //GET: api/Role
        [HttpGet]
        //public async Task<IEnumerable<Customer>> GetAllCustomers() => await _CustomerRepository.GetAll();
        public  IEnumerable<CustomerPM> GetAllCustomers()
        {
           var customers = _customerRepository.GetQuery(x => x.Category == "Customer").OrderBy(x=>x.CustomerName).ToList();

            return CustomerMapper.ToPresentationModels(customers).OrderBy(cus => cus.Code).AsQueryable();
        }

        [HttpGet("DB/{customerIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomerList(string customerIds)
        {
            //var routes = await _routeRepository.GetRoutes();

            var customers = await _customerRepository.GetQueryAsync(x => customerIds.Contains(x.Id.ToString()));
            return Ok(customers);

        }

        [HttpGet("IdName/{customerIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IdNamePM>>> GetCustomerIdNames(string customerIds)
        {
            var custIds = new List<int>();
            var customers = new List<BasePM>();

            if (customerIds=="0")
            {
                customers = (await _customerRepository.GetQueryAsync(x => x.Category=="Customer" )).Select(x => new BasePM() { Id = x.Id, Code = x.CustomerName, Name = x.CompanyName, Description = x.MaxString1 }).ToList();
            }             
            else
            {
                try
                {
                    custIds = customerIds.Split(",").Select(x => Int32.Parse(x)).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                customers = (await _customerRepository.GetQueryAsync(x => x.Category == "Customer" && custIds.Contains(x.Id))).Select(x => new BasePM() { Id = x.Id, Code = x.CustomerName, Name = x.CompanyName, Description = x.MaxString1 }).ToList();
            }
               
            
            return Ok(customers);

        }

        [HttpGet]
        [Route("SalesOrder")]
        public IEnumerable<CustomerPM> GetCustomersWithSalesOrders()
        {
            //ToDo: Call sales order service to get customer id 
            //IEnumerable<Customer> customers = _exceptionManager.Process(() => _orderRepository.GetQuery<Customer>(cus => cus.Category == "Customer" && cus.SalesOrders.Count > 0).OrderBy(c => c.CustomerName), "ExceptionShielding");
            //return CustomerMapper.ToPresentationModels(customers);
            return null;
        }

        [HttpGet]
        [Route("{id}")]
        //public Customer GetCustomerById(int id) => _customerRepository.GetById(id);
        public CustomerPM GetCustomerById(int id)
        {
            var a = _customerRepository.GetById(id);
            return CustomerMapper.ToPresentationModel(a);
        }

        [HttpGet]
        [Route("Name/{customerName}")]
        //public Customer GetCustomerById(int id) => _customerRepository.GetById(id);
        public CustomerPM GetCustomerByName(string customerName)
        {
            var customer = _customerRepository.GetQuery(x => x.Category == "Customer" && x.CustomerName==customerName).FirstOrDefault();
            
             return CustomerMapper.ToPresentationModel(customer);
            
        }

        [HttpPost("Customers")]
        public async Task<ActionResult<List<BasePM>>> AddCustomers1([FromBody] List<BasePM> customers)
        {

            foreach (var customer in customers)
            {
                var a = _customerRepository.GetQuery(x => x.CustomerName == customer.Code && x.Category == "Customer").FirstOrDefault();
                if (a != null)
                {
                    customer.Id = a.Id;
                }
                else
                {
                    var b = new Customer() { CustomerName = customer.Code, CompanyName = customer.Name, Category = "Customer" };
                    await _customerRepository.InsertAsync(b);
                    customer.Id = b.Id;
                }

            }
            return new OkObjectResult(customers);
        }



            [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddCustomer([FromBody]  CustomerPM customerPM)
        {
            Customer customer = CustomerMapper.FromPresentationModel(customerPM);
            customer.CreatedOn = DateTime.Now;
            if(!string.IsNullOrEmpty(customerPM.Category) && customerPM.Category.ToUpper() != "UNIT")
            {
                customer.Category = "Customer";
            }
          
            _customerRepository.Insert(customer);

            CustomerMapper.UpdatePresentationModel(customerPM, customer);
            return new OkObjectResult(customerPM);
        }

        [HttpPost("Import")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ImportCustomers([FromBody] List<CustomerPM> customersPM)
        {
            var customers = CustomerMapper.FromPresentationModels(customersPM);
            var count = 0;

            try
            {
                foreach (var customer in customers)
                {
                    var a =_customerRepository.GetQuery(x=>x.CustomerName== customer.CustomerName && x.Category== "Customer").FirstOrDefault();
                    if (a==null)
                    {
                        customer.CreatedOn = DateTime.Now;
                        customer.Category = "Customer";                   
                        _customerRepository.Insert(customer);
                        count++;
                    }
                    else
                    {
                        a.CompanyName = customer.CompanyName;
                        a.Address = customer.Address;
                        a.CompanyName = customer.CompanyName;
                        a.ContactPerson = customer.ContactPerson;
                        a.Email = customer.Email;
                        a.Phone = customer.Phone;
                        a.BillingAddress = customer.BillingAddress;
                        a.String3 = customer.String3; //fax
                        a.String5 = customer.String5; //creditterm
                        a.MaxString1 = customer.MaxString1; //description
                        _customerRepository.Update(a);
                    }
                   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            
            return new OkObjectResult(count++);
        }

        [HttpPut]
        //public void UpdateCustomer([FromBody] Customer Customer) => _customerRepository.Update(Customer);
        public void UpdateCustomer([FromBody] CustomerPM customerPM)
        {
            Customer customer = CustomerMapper.FromPresentationModel(customerPM);
            Customer existingCustomer = _customerRepository.GetById(customerPM.Id);

            existingCustomer.Address = customer.Address;
            existingCustomer.CompanyName = customer.CompanyName;
            existingCustomer.ContactPerson = customer.ContactPerson;
            existingCustomer.CustomerName = customer.CustomerName;
            existingCustomer.Email = customer.Email;
            existingCustomer.MaxString1 = customer.MaxString1;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.String1 = customer.String1;
            existingCustomer.String2 = customer.String2;
            existingCustomer.String3 = customer.String3;
            existingCustomer.String4 = customer.String4;
            existingCustomer.String5 = customer.String5;
            existingCustomer.BillingAddress = customer.BillingAddress;

            existingCustomer.PictureId = customer.PictureId;
            existingCustomer.MaxString2 = customer.MaxString2;
            existingCustomer.MaxString3 = customer.MaxString3;

            existingCustomer.Priority = customer.Priority;
            if (!string.IsNullOrEmpty(customerPM.Category) && customerPM.Category.ToUpper() != "UNIT")
            {
                existingCustomer.Category = "Customer";
            }
            
               
            _customerRepository.Update(existingCustomer);
        }




        // DELETE api/<RoleController>/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
          
            var msg = await _customerRepository.CheckCustomer(id);
            if (!string.IsNullOrEmpty (msg)) return Conflict(msg);

            var customer= await _customerRepository.GetByIdAsync(id);

            if (customer == null) return NotFound();
            try
            {
                await _customerRepository.DeleteAsync(customer);
            }
            catch (Exception exception)
            {
                if (exception.InnerException == null) throw;
                var sqlException = exception.InnerException as Microsoft.Data.SqlClient.SqlException;
                if (sqlException == null) throw;

                if (sqlException.Number != 547) throw;
                throw new Exception("Record cannot be deleted due related records.");
            }

            return Ok(id);


        }


        


    }
}
