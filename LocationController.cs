using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using SIMTech.APS.Location.API.PresentationModels;
using Newtonsoft.Json;

using Microsoft.Extensions.Configuration;
namespace SIMTech.APS.Location.API.Controllers
{
    using SIMTech.APS.Location.API.Repository;
    using SIMTech.APS.Location.API.Mappers;
    using SIMTech.APS.Location.API.Models;
    using SIMTech.APS.PresentationModels;
    using SIMTech.APS.Utilities;
    using SIMTech.APS.Location.API.PresentationModels;
    using SIMTech.APS.Customer.API.PresentationModels;

    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationRepository _LocationRepository;

        public LocationController(ILocationRepository LocationRepository)
        {
            _LocationRepository = LocationRepository;
        }


        //GET: api/Location
        [HttpGet]
        //[Route("GetLocations")]
        public IEnumerable<LocationPM> GetLocations()
        {
            var locations = _LocationRepository.GetQuery().OrderBy(s => s.Id).ToList();
            IEnumerable<LocationPM> locationPMs = LocationMapper.ToPresentationModels(locations).ToList();
            int customerId = 0;


            try
            {
                foreach (var a in locationPMs)
                {
                    Int32.TryParse(a.SubCategory, out customerId);
                    a.Customer = ApiGetCustomerInfo(customerId);
                    a.OperationIds = ApiGetOperations(a.Id);
                }
            }
            catch { }

            
            return locationPMs.ToList().AsQueryable();
        }

        [HttpGet("DB")]
        public IEnumerable<Location> GetLocations1()
        {
            var locations = _LocationRepository.GetQuery().OrderBy(s => s.Id).ToList();
            
            return locations;
        }


        [HttpGet("{id}")]
        public LocationPM GetLocationById(int id)
        {
            var location = _LocationRepository.GetById(id);
            var locationPM = LocationMapper.ToPresentationModel(location);
            int customerId = 0;

            Int32.TryParse(locationPM.SubCategory, out customerId);
            try
            {
                locationPM.Customer = ApiGetCustomerInfo(customerId);
                locationPM.OperationIds = ApiGetOperations(locationPM.Id);
            }
            catch { }
           

            return locationPM;
        }

        [HttpGet("IdName/{locationIds}")]
        public async Task<ActionResult<IEnumerable<IdNamePM>>> GetLocationIdNames(string locationIds)
        {
            var locIds = new List<int>();
            var locations = new List<BasePM>();

            if (locationIds == "0")
            {
                locations = (await _LocationRepository.GetQueryAsync(x =>x.Category=="Unit")).Select(x => new BasePM() { Id = x.Id, Code = x.LocationName, Name=x.LocationName , Description = x.Description, Value=(x.CalendarId??0).ToString () }).ToList();
            }
            else
            {
                try
                {
                    locIds = locationIds.Split(",").Select(x => Int32.Parse(x)).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                locations = (await _LocationRepository.GetQueryAsync(x => x.Category == "Unit" && locIds.Contains (x.Id))).Select(x => new BasePM() { Id = x.Id, Code = x.LocationName, Name = x.LocationName, Description = x.Description, Value = (x.CalendarId ?? 0).ToString() }).ToList();
              
            }
            return Ok(locations);

        }

        [HttpGet("Unit")]
        public BasePM GetDefaultUnit()
        {
            var location = _LocationRepository.GetQuery(x=>x.Category =="Unit").OrderBy(x=>x.LocationName).FirstOrDefault();          
            var locationPM = LocationMapper.ToPresentationModel(location);

            //if (locationPM!=null)
            //{
            //    int customerId = 0;

            //    Int32.TryParse(locationPM.SubCategory, out customerId);
                
            //    try
            //    {
            //        locationPM.Customer = ApiGetCustomerInfo(customerId);
            //        locationPM.OperationIds = ApiGetOperations(locationPM.Id);
            //    }
            //    catch { }

               
            //}

            var unit = new BasePM() { Id = locationPM.Id, Name = locationPM.Name };
            if (locationPM.Customer!=null)
            {
                unit.Description = locationPM.Customer.Name;
                if (locationPM.Customer.PictureId!=null)
                {
                    unit.Value = locationPM.Customer.PictureId.ToString();
                }
                    
            }


            return unit;
        }



        [HttpPost]
        //[Route("AddLocation")]
        public async Task<IActionResult> AddLocation([FromBody] LocationPM locationPM)
        {
            locationPM.Id = 0;
            Location dbLocation = LocationMapper.FromPresentationModel(locationPM);

            if (locationPM != null && locationPM.Customer != null
                && dbLocation != null)
            {
                CustomerPM objCustomer = locationPM.Customer;

                if (objCustomer != null)
                {
                    objCustomer.Category = "Unit";
                    int customerid = ApiAddCustomer(objCustomer);
                    locationPM.CustomerId = customerid;
                    locationPM.SubCategory = customerid.ToString();
                    dbLocation.Subcategory = customerid.ToString();
                }
            }
            dbLocation.Category = "Unit";
            dbLocation.CreatedOn = DateTime.Now;


            _LocationRepository.Insert(dbLocation);
            locationPM.Id = dbLocation.Id;

            if (locationPM.OperationIds != null)
            {
                var opIds = locationPM.OperationIds;
                await ApiUpdateOperations(locationPM.Id, opIds);
                //refresh from api
                locationPM.OperationIds = ApiGetOperations(locationPM.Id);
            }

            return new OkObjectResult(locationPM);
        }


        [HttpPut]
        //[Route("UpdateLocation")]
        public async Task UpdateLocation([FromBody] LocationPM locationPM)
        {
            Location dbLocation = LocationMapper.FromPresentationModel(locationPM);
            Location existingLocation = _LocationRepository.GetById(locationPM.Id);
            if (existingLocation != null)
            {
                existingLocation.Description = dbLocation.Description;
                existingLocation.CalendarId = dbLocation.CalendarId;
                existingLocation.ModifiedOn = DateTime.Now;
                _LocationRepository.Update(existingLocation);
            }


            if (locationPM != null && locationPM.Customer != null && locationPM.Customer.Id > 0)
            {
                ApiUpdateCustomer(locationPM.Customer);

            }

            if (locationPM.OperationIds != null)
            {
                var opIds = locationPM.OperationIds;
                await ApiUpdateOperations(locationPM.Id, opIds);
                //refresh from api
                locationPM.OperationIds = ApiGetOperations(locationPM.Id);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _LocationRepository.GetByIdAsync(id);

            if (location == null) return NotFound();

            await _LocationRepository.DeleteAsync(location);

            if (location.Subcategory != null)
            {
                int customerId = 0;

                if (Int32.TryParse(location.Subcategory, out customerId))
                {
                    await ApiDeleteCustomer(customerId);
                }
                    
            }

            var ops=ApiGetOperations(id);

            if (ops != null && ops.Length>0)
            {
                var a = new List<int>();
                await ApiUpdateOperations(id,a.ToArray ());
            }

            return Ok(id);

        }


        #region api call of other services

        private int[] ApiGetOperations(int locationId)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_OPERATION_URL");
            var operations = HttpHelper.Get<List<BasePM>>(apiBaseUrl, $"unit/{locationId}");
            return operations.Select(x=>x.Id).ToArray();
        }

        private async Task ApiUpdateOperations(int locationId, int[] operations)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_OPERATION_URL");
            await HttpHelper.PutAsync<int[]>(apiBaseUrl, $"unit/{locationId}", operations);            
        }

        private async Task ApiDeleteCustomer(int customerId)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");
            await HttpHelper.DeleteAsync(apiBaseUrl, $"{customerId}");
        }

        private CustomerPM ApiGetCustomerInfo(int customerId)
        {
          
            //var AppName = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["CustomerAPIUrl"];

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");         
            string sCustomerUrl = apiBaseUrl ?? string.Empty;
          
            using (var client = new System.Net.Http.HttpClient())
            {               
                client.BaseAddress = new Uri(sCustomerUrl);

                //HTTP GET
                try
                {
                    var responseTask = client.GetAsync(customerId.ToString());
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        CustomerPM roles = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomerPM>(alldata);
                        return roles;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }

            }
            return null;          
        }

        private int ApiAddCustomer(CustomerPM customer)
        {
            //var AppName = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["CustomerAPIUrl"];

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");

            string sCustomerUrl = apiBaseUrl ?? string.Empty;

          
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(sCustomerUrl);

                //HTTP Post
                try
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string json = JsonConvert.SerializeObject(customer);
                    StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                    client.BaseAddress = new Uri(sCustomerUrl);
                    var response1 = client.PostAsync(sCustomerUrl, data);
                    response1.Wait();

                    var result = response1.Result;
                   
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        var customer1 = Newtonsoft.Json.JsonConvert.DeserializeObject<CustomerPM>(alldata);
                        return customer1.Id;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }

            }
            return 0;        
        }


        private void ApiUpdateCustomer(CustomerPM customer)
        {
            //var AppName = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["CustomerAPIUrl"];

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");
            string sCustomerUrl = apiBaseUrl ?? string.Empty;
           
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(sCustomerUrl);


                //HTTP Put
                try
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string json = JsonConvert.SerializeObject(customer);
                    StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                    client.BaseAddress = new Uri(sCustomerUrl);
                    var response1 = client.PutAsync(sCustomerUrl, data);
                    response1.Wait();

                    var result = response1.Result;

                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                       
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }

            }
         
        }

        #endregion



    }
}
