using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;


namespace SIMTech.APS.Resource.API.Controllers
{
    using SIMTech.APS.Resource.API.Repository;
    using SIMTech.APS.Resource.API.Mappers;
    using SIMTech.APS.Resource.API.Models;
    using SIMTech.APS.Resource.API.PresentationModels;
    using SIMTech.APS.PresentationModels;
    using SIMTech.APS.Utilities;

    [Route("api/[controller]")]
    [ApiController]
    public class ResourceController : ControllerBase
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly IResourceBlockoutRepository _resourceBlockoutRepository;
        private readonly IResourceParameterRepository _resourceParameterRepository;
        private readonly ExceptionManager _exceptionManager;

        public ResourceController(IResourceRepository resourceRepository, IResourceBlockoutRepository resourceBlockoutRepository, IResourceParameterRepository resourceParameterRepository)
        {
            _resourceRepository = resourceRepository;
            _resourceBlockoutRepository = resourceBlockoutRepository;
            _resourceParameterRepository = resourceParameterRepository;
            _exceptionManager = new ExceptionManager();
        }



        #region APIs

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ResourcePM>>> GetAllResources()
        {

            var resources = await _resourceRepository.GetQueryAsync(x => x.Id > 0);
            
            var resourcePMs = ResourceMapper.ToPresentationModels(resources.OrderBy (x=>x.EquipmentName).ToList());
            //GetResourceOpeations(resourcePMs);

            return Ok(resourcePMs);
        }

        [HttpGet("DB")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetAllResources1()
        {

            var machines = await _resourceRepository.GetQueryAsync(x => x.Id > 0);
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true,
            };

            //return Ok(machines);

            return Ok(JsonSerializer.Serialize(machines, options));
            
        }

        [HttpGet("DB/{resourceIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetResourcesList(string resourceIds)
        {

            var resourceList = resourceIds.Split(",").Select(x => int.Parse(x)).ToList();

            var machines = await _resourceRepository.GetQueryAsync(x => x.Id > 0 && resourceList.Contains(x.Id));
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true,
            };

            //return Ok(machines);

            return Ok(JsonSerializer.Serialize(machines, options));

        }

        [HttpGet("Blockout/{startDate}/{endDate}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EquipmentBlockOut>>> GetResourceBlockouts(DateTime startDate, DateTime endDate)
        {

            var blockouts = (await _resourceBlockoutRepository.GetQueryAsync(x => x.EndDate >= startDate && x.StartDate <= endDate)).OrderBy(x => x.EquipmentId).ToList();

            //foreach (var b in blockouts )
            //{               
            //    b.Equipment = null;
            //}

           
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true,
            };

            return Ok(JsonSerializer.Serialize(blockouts, options));
        }

       
        [HttpGet]
        [Route("IdName/{resourceIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BasePM>>> GetResourcesIdName(string resourceIds)
        {     
            var resources = await _resourceRepository.GetQueryAsync(x => x.Id > 0);

            if (!string.IsNullOrWhiteSpace(resourceIds) && resourceIds!="0")
            {
                List<string> result = resourceIds.Split(',').ToList();
                resources = resources.Where(x => result.Contains(x.Id.ToString()));
            }
                
            var resourceIdNames = resources.OrderBy(x => x.EquipmentName).ToList().Select(x => new BasePM() { Id = x.Id, Name = x.EquipmentName, Value =x.Type.ToString (),Description=x.Float1.ToString () });
    
           return Ok(resourceIdNames);

        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResourcePM>>  GetResourceById(int id)
        {
            var resouce = await _resourceRepository.GetByIdAsync(id);
            if (resouce==null)
            {
                return NotFound();
            }
            var resourcePM= ResourceMapper.ToPresentationModel(resouce);
            GetResourceOpeations(resourcePM);          
            return resourcePM;
        }

        [HttpGet]
        [Route("Blockout/{machineId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ResourceBlockoutPM>>> GetBlockOutByMachine(int machineId)
        {
            var blockouts = new List<EquipmentBlockOut>();

            if (machineId == 0)
            {
                 blockouts = (await _resourceBlockoutRepository.GetQueryAsync(x => x.Id >0)).OrderBy(x => x.EquipmentId).ToList();
            }
            else
            {
                var resource = await _resourceRepository.GetByIdAsync(machineId);
                if (resource == null)
                {
                    return NotFound();
                }
                blockouts = resource.EquipmentBlockOuts.ToList();

            }

            
            var blockoutPMs = ResourceBlockoutMapper.ToPresentationModels(blockouts);
            
            return blockoutPMs.ToList ();
        }

        [HttpPost]
        [Route("Blockout/{machineId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> UpdateBlockOutByMachine(int machineId, [FromBody] List<ResourceBlockoutPM> blockOuts )
        {

            if (machineId==0 || (blockOuts!=null && blockOuts.Where (x=>x.EquipmentID != machineId).Count()>0) )
            {
                return BadRequest();
            }

            var resource = await _resourceRepository.GetByIdAsync(machineId);
            if (resource == null)
            {
                return NotFound();
            }

          
            var changed = false;
            var roIds = resource.EquipmentBlockOuts.Select(x => x.Id).ToList();
            foreach (var roPM in blockOuts.OrderBy(x => x.EquipmentBlockOutID))
            {
                if (roPM.EquipmentBlockOutID==0)
                {
                    //Add Blockouts;
                    var blockout = ResourceBlockoutMapper.FromPresentationModel(roPM);

                    if (!string.IsNullOrWhiteSpace(roPM.CreatedBy)) 
                        blockout.CreatedBy = roPM.CreatedBy;
                    if (!string.IsNullOrWhiteSpace(blockout.BlockOutType)) 
                        blockout.BlockOutType = "PM";
                    blockout.CreatedOn = DateTime.Now;
                    resource.EquipmentBlockOuts.Add(blockout);

                    changed = true;
                }
                else
                {
                    var ro = resource.EquipmentBlockOuts.FirstOrDefault(x => x.Id == roPM.EquipmentBlockOutID);
                    if (ro != null)
                    {
                        roIds.Remove(ro.Id);
                        if (ro.StartDate != roPM.StartDate || ro.EndDate != roPM.EndDate || ro.Remarks != roPM.Remarks || ro.Value !=roPM.Value )
                        {
                            //update existing blockouts                   
                            ro.StartDate = roPM.StartDate;
                            ro.EndDate = roPM.EndDate;
                            ro.Remarks = roPM.Remarks;
                            ro.Value = roPM.Value;
                            ro.ModifiedBy = roPM.CreatedBy;
                            ro.ModifiedOn = DateTime.Now;
                            changed = true;
                        }
                    }

                }               
            }

         
          

            foreach (var roId in roIds)
            {
                var a = resource.EquipmentBlockOuts.Where(x => x.Id == roId).FirstOrDefault();
                resource.EquipmentBlockOuts.Remove(a);  
                _resourceBlockoutRepository.Delete(a);                            
                changed = true;
            }

            if (changed)
            {
                await _resourceRepository.UpdateAsync(resource);
                await _resourceRepository.SaveAsync();
            }


            //if (changed) await _resourceBlockoutRepository.SaveAsync();



            return Ok(resource.EquipmentBlockOuts.Count);
        }

        [HttpGet]
        [Route("Operator/{machineId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ResourceParameterPM>>> GetOperatorByMachine(int machineId)
        {
            if (machineId <=0)
            {
                return BadRequest ("Invalid machine Id") ;
            }
            
            var resource = await _resourceRepository.GetByIdAsync(machineId);
            if (resource == null)
            {
                return NotFound();
            }
            var parameters = resource.EquipmentParameters.ToList();

            var parameterPMs = ResourceParameterMapper.ToPresentationModels(parameters);

            return parameterPMs.ToList();
        }

        [HttpPost]
        [Route("Operator/{machineId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> UpdateOperatorByMachine(int machineId, [FromBody] List<ResourceParameterPM> parameters)
        {

            if (machineId == 0 || (parameters != null && parameters.Where(x => x.ResourceId != machineId).Count() > 0))
            {
                return BadRequest("Invalid machine Id");
            }

            var resource = await _resourceRepository.GetByIdAsync(machineId);
            if (resource == null)
            {
                return NotFound();
            }


            var changed = false;
            var roIds = resource.EquipmentParameters.Select(x => x.Id).ToList();
            foreach (var roPM in parameters.OrderBy(x => x.Id))
            {
                if (roPM.Id == 0)
                {
                    //Add Blockouts;
                    var parameter = ResourceParameterMapper.FromPresentationModel(roPM);
                    parameter.CreatedOn = DateTime.Now;
                    resource.EquipmentParameters.Add(parameter);

                    changed = true;
                }
                else
                {
                    var ro = resource.EquipmentParameters.FirstOrDefault(x => x.Id == roPM.Id);
                    if (ro != null)
                    {
                        roIds.Remove(ro.Id);
                        if (ro.ParameterId != roPM.ParameterId || ro.Value != roPM.ParameterDefaultValue)
                        {
                            //update existing blockouts                   
                            ro.ParameterId = roPM.ParameterId;
                            ro.Value = roPM.ParameterDefaultValue;                          
                            changed = true;
                        }
                    }

                }
            }

            foreach (var roId in roIds)
            {
                var a = resource.EquipmentParameters.Where(x => x.Id == roId).FirstOrDefault();
                resource.EquipmentParameters.Remove(a);
                _resourceParameterRepository.Delete(a);
                changed = true;
            }

            if (changed)
            {
                await _resourceRepository.UpdateAsync(resource);
                await _resourceRepository.SaveAsync();
            }



            return Ok(resource.EquipmentParameters.Count);
        }





        [HttpGet]
        [Route("Category/{categoryName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ResourcePM>>> GetResourcesByCategoryName(string categoryName)
        {
            IEnumerable<Equipment> resources=null;

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                 resources = await _exceptionManager.Process(() => _resourceRepository.GetQueryAsync(e => e.Category == null || e.Category ==""), "ExceptionShielding");
            }
            else
            {
                resources = await _exceptionManager.Process(() => _resourceRepository.GetQueryAsync(e => e.Category.Trim().ToLower() == categoryName.Trim().ToLower()), "ExceptionShielding");
            }

            var resourcePMs = ResourceMapper.ToPresentationModels(resources.OrderBy(x => x.EquipmentName).ToList());
            GetResourceOpeations(resourcePMs);
            return Ok(resourcePMs);
        }




        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ResourcePM>> AddResource([FromBody] ResourcePM resourcePM)
        {
            var existingResource =  (await _resourceRepository.GetQueryAsync(r => r.EquipmentName.Equals(resourcePM.Name) && r.Type==resourcePM.Type )).FirstOrDefault();
            if (existingResource != null)
            {
                return Conflict();
            }


            var resource = ResourceMapper.FromPresentationModel(resourcePM);

            try
            {
                await _resourceRepository.InsertAsync(resource);
            }
            catch (Exception e)
            { 
                if (e.InnerException == null) throw;

                var sqlException  = e.InnerException as Microsoft.Data.SqlClient.SqlException;
               
                if (e == null) throw;

                if (sqlException.Number != 2601) throw sqlException;
                throw new Exception("Cannot have duplicate equipment (Code + Type)."
                    + Environment.NewLine + Environment.NewLine
                    + "Please consider altering either equipment code or type");
            }

            resourcePM.Id = resource.Id;
            //return new OkObjectResult(resourcePM);
            return CreatedAtAction("GetResourceById", new { id = resource.Id }, resourcePM);

        }

        [HttpPost("Blockout")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResourceBlockoutPM>> AddMachineBlockout([FromBody] ResourceBlockoutPM blockoutPM)
        {
            var existingResource = (await _resourceRepository.GetQueryAsync(r => r.EquipmentName.Equals(blockoutPM.EquipmentName) && r.Type == 1)).FirstOrDefault();
            if (existingResource == null)
            {
                return NotFound();
            }

            blockoutPM.EquipmentID = existingResource.Id;
            var blockout = ResourceBlockoutMapper.FromPresentationModel(blockoutPM);

            if (!string.IsNullOrWhiteSpace(blockoutPM.CreatedBy)) blockout.CreatedBy = blockoutPM.CreatedBy;

            var a =existingResource.EquipmentBlockOuts.Where(x => x.CreatedBy == "PDM").FirstOrDefault();

            if (a!=null)
            {
                a.StartDate = blockout.StartDate;
                a.EndDate = blockout.EndDate;
                a.Remarks = blockout.Remarks;
                a.ModifiedBy = blockout.CreatedBy;
                a.ModifiedOn = DateTime.Now;
            }
            else
            {
                blockout.CreatedOn = DateTime.Now;
                existingResource.EquipmentBlockOuts.Add(blockout);
            }
               


            try
            {
                await _resourceRepository.UpdateAsync(existingResource);
            }
            catch (Exception e)
            {
                if (e.InnerException == null) throw;

                var sqlException = e.InnerException as Microsoft.Data.SqlClient.SqlException;

                if (e == null) throw;

                if (sqlException.Number != 2601) throw sqlException;
               
            }

            blockoutPM.EquipmentBlockOutID = blockout.Id;
            
            return new OkObjectResult(blockoutPM);
            

        }



        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateResource(int id, [FromBody] ResourcePM equipmentPM)
        {
            if (equipmentPM ==null || id != equipmentPM.Id)
            {
                return BadRequest();
            }
                        
            equipmentPM.TypeName = (equipmentPM.Type.HasValue ? (equipmentPM.Type == 1 ? "INHOUSE" : (equipmentPM.Type == 2 ? "QC" : (equipmentPM.Type == 3 ? "SUBCON" : "AutoMated"))) : "");         

            Equipment updatingEquipment = ResourceMapper.FromPresentationModel(equipmentPM);
            Equipment existingEquipment = await _exceptionManager.Process(() =>  _resourceRepository.GetByIdAsync(updatingEquipment.Id), "ExceptionShielding");

            if (existingEquipment == null)
            {
                return NotFound();
            }

            existingEquipment.EquipmentName = updatingEquipment.EquipmentName;
            existingEquipment.Subcategory = updatingEquipment.Subcategory;
            existingEquipment.Type = updatingEquipment.Type;
            existingEquipment.Category = updatingEquipment.Category;
            existingEquipment.LocationId = updatingEquipment.LocationId;
            existingEquipment.Description = updatingEquipment.Description;
            existingEquipment.String1 = updatingEquipment.String1;
            existingEquipment.Float1 = updatingEquipment.Float1;
            existingEquipment.CalendarId = updatingEquipment.CalendarId;
            existingEquipment.ModifiedOn = DateTime.Now;

            try
            {
                await _resourceRepository.UpdateAsync(existingEquipment);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }



            return Ok(equipmentPM);
           
        }




        // DELETE api/Resource/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteResource(int id)
        {

            try
            {
                await _resourceRepository.DeleteAsync(id);
            }
            catch (Exception e)
            {
                if (e.InnerException == null) throw;
                var sqlException = e.InnerException as Microsoft.Data.SqlClient.SqlException;
                if (sqlException == null) throw;

                if (sqlException.Number != 547) throw sqlException;
                throw new Exception("Record cannot be deleted due related records.");
            }

            return Ok(id);
        }
        #endregion


        #region  private methods

        private void GetResourceOpeations(IList<ResourcePM> resourcePMs)
        {
            var operations = ApiGetOperationsbyResource(0);
            if (operations!=null)
            {
                foreach (var a in resourcePMs)
                {
                    var selectedOperations = operations.Where(x => x.Id == a.Id).ToList();
                    GetResourceOpeations(a, selectedOperations);
                    //a.Operations = selectedOperations.Count();
                    //a.OperationNames = string.Join(",", selectedOperations.Select(x => x.Name).ToList());
                }
            }
            
                
        }
        private void GetResourceOpeations(ResourcePM resourcePM, List<BasePM> operations=null)
        {
            if (operations==null)
            {
                 operations = ApiGetOperationsbyResource(resourcePM.Id);
            }
            
            if (operations!=null)
            {
                resourcePM.Operations = operations.Count();
                resourcePM.OperationNames = string.Join(",", operations.Select(x => x.Name).ToList());
            }
           
        }
        #endregion


        #region calling API of other services
        private List<BasePM> ApiGetOperationsbyResource(int resourceId)
        {
            using (var client = new System.Net.Http.HttpClient())
            {              
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("RPS_OPERATION_URL") + "/resource/");

                //HTTP GET
                try
                {
                    var responseTask = client.GetAsync(resourceId.ToString());
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        var operations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BasePM>>(alldata);
                        return operations;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }


            }
            return null;

        }

        
        #endregion





    }
}
