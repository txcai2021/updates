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


namespace SIMTech.APS.Supplier.API.Controllers
{
    using SIMTech.APS.Customer.API.Repository;
    using SIMTech.APS.Customer.API.Mappers;
    using SIMTech.APS.Customer.API.Models;
    using SIMTech.APS.Customer.API.PresentationModels;
    using SIMTech.APS.PresentationModels;
    //using SIMTech.APS.Customer.API.PresentationModels;

    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ICustomerRepository _supplierRepository;

        public SupplierController(ICustomerRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }


        //GET: api/Role
        [HttpGet]
        //public async Task<IEnumerable<Customer>> GetAllCustomers() => await _CustomerRepository.GetAll();
        public  IEnumerable<SupplierPM> GetAllSuppliers()
        {
           var customers = _supplierRepository.GetQuery(x => x.Category == "Supplier").OrderBy(x=>x.CustomerName).ToList();

            return SupplierMapper.ToPresentationModels(customers).OrderBy(cus => cus.Code).AsQueryable();
        }

      

        [HttpGet]
        [Route("{id}")]
        //public Customer GetCustomerById(int id) => _customerRepository.GetById(id);
        public SupplierPM GetSupplierById(int id)
        {
            var a = _supplierRepository.GetById(id);
            return SupplierMapper.ToPresentationModel(a);
        }
        


        [HttpPost]
        public IActionResult AddSupplier([FromBody]  SupplierPM supplierPM)
        {
            var supplier = SupplierMapper.FromPresentationModel(supplierPM);
            supplier.CreatedOn = DateTime.Today;
            supplier.Category = "Supplier";      
            _supplierRepository.Insert(supplier);

            SupplierMapper.UpdatePresentationModel(supplierPM, supplier);
            return new OkObjectResult(supplierPM);
        }

        [HttpPost("Suppliers")]
        public async Task<ActionResult<List<BasePM>>> AddSuppliers([FromBody] List<BasePM> suppliers)
        {

            foreach (var supplier in suppliers)
            {
                var a = _supplierRepository.GetQuery(x => x.CustomerName == supplier.Code && x.Category =="Supplier").FirstOrDefault();
                if (a!=null)
                {
                    supplier.Id = a.Id;
                }
                else
                {
                    var b = new Customer() { CustomerName = supplier.Code, CompanyName = supplier.Name , Category ="Supplier" };
                    await _supplierRepository.InsertAsync(b);
                    supplier.Id = b.Id;
                }

            }

            return new OkObjectResult(suppliers);
           
        }

        [HttpPost("Import")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Importsuppliers([FromBody] List<SupplierPM> suppliersPM)
        {
            var suppliers = SupplierMapper.FromPresentationModels(suppliersPM);
            var count = 0;

            try
            {
                foreach (var supplier in suppliers)
                {
                    var a = _supplierRepository.GetQuery(x => x.CustomerName == supplier.CustomerName && x.Category == "Supplier").FirstOrDefault();
                    if (a == null)
                    {
                        supplier.CreatedOn = DateTime.Now;
                        supplier.Category = "Supplier";
                        _supplierRepository.Insert(supplier);
                        count++;
                    }
                    else
                    {
                        a.CompanyName = supplier.CompanyName;
                        a.Address = supplier.Address;
                        a.CompanyName = supplier.CompanyName;
                        a.ContactPerson = supplier.ContactPerson;
                        a.Email = supplier.Email;
                        a.Phone = supplier.Phone;
                        a.BillingAddress = supplier.BillingAddress;
                        a.String3 = supplier.String3; //fax
                        a.String5 = supplier.String5; //creditterm
                        a.MaxString1 = supplier.MaxString1; //description
                        _supplierRepository.Update(a);
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
        public void UpdateSupplier([FromBody] SupplierPM supplierPM)
        {
            var supplier = SupplierMapper.FromPresentationModel(supplierPM);
            var existingsupplier = _supplierRepository.GetById(supplier.Id);

            existingsupplier.CustomerName = supplier.CustomerName;
            existingsupplier.Address = supplier.Address;
            existingsupplier.CompanyName = supplier.CompanyName;
            existingsupplier.ContactPerson = supplier.ContactPerson;

            existingsupplier.Email = supplier.Email;
            existingsupplier.MaxString1 = supplier.MaxString1;
            existingsupplier.Phone = supplier.Phone;
            existingsupplier.String1 = supplier.String1;
            existingsupplier.String2 = supplier.String2;
            existingsupplier.String3 = supplier.String3;
            existingsupplier.String4 = supplier.String4;
            existingsupplier.String5 = supplier.String5;
            existingsupplier.BillingAddress = supplier.BillingAddress;
            existingsupplier.Category = "Supplier";

            _supplierRepository.Update(existingsupplier);
        }




        // DELETE api/<RoleController>/5
        [HttpDelete("{id}")]
        public void DeleteSupplier(int id)
        {
            try
            {
                _supplierRepository.Delete(id);
            }
            catch (Exception exception)
            {
                if (exception.InnerException == null) throw;
                var sqlException = exception.InnerException as Microsoft.Data.SqlClient.SqlException;
                if (sqlException == null) throw;

                if (sqlException.Number != 547) throw;
                throw new Exception("Record cannot be deleted due related records.");
            }


        }

    }
}
