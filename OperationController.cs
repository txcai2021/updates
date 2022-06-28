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




namespace SIMTech.APS.Operation.API.Controllers
{
    using SIMTech.APS.Operation.API.Repository;
    using SIMTech.APS.Operation.API.Mappers;
    using SIMTech.APS.Operation.API.Models;
    using SIMTech.APS.Operation.API.PresentationModels;
    using SIMTech.APS.PresentationModels;
    using SIMTech.APS.Utilities;
    using SIMTech.APS.Models;
    using SIMTech.APS.Setting.API.Models;
    using SIMTech.APS.Resource.API.Enums;
    using SIMTech.APS.Customer.API.PresentationModels;
    using SIMTech.APS.Product.API.PresentationModels;

    [Route("api/[controller]")]
    [ApiController]
    public class OperationController : ControllerBase
    {
        private readonly IOperationRepository _operationRepository;
        private readonly IOperationResourceRepository _operationResourceRepository;
        private readonly IOperationRateRepository _operationRateRepository;
        private readonly IOperationParameterRepository _operationParameterRepository;
        private readonly IParameterRepository _parameterRepository;

        public OperationController(IOperationRepository operationRepository, IOperationResourceRepository operationResourceRepository, IOperationParameterRepository operationParameterRepository, IOperationRateRepository operationRateRepository, IParameterRepository paramterRepository)
        {
            _operationRepository = operationRepository;
            _operationResourceRepository = operationResourceRepository;
            _operationParameterRepository = operationParameterRepository;
            _operationRateRepository = operationRateRepository;
            _parameterRepository = paramterRepository;
        }

        #region APIs
        // GET: api/Operation
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OperationPM>>> GetOperations()
        {
            var operations = (await _operationRepository.GetOperations()).ToList ();

            var operationPMs = OperationMapper.ToPresentationModels(operations);


            //get list of machine name          
            GetOperationsResourceNames(operationPMs);

            //get list of route name
            GetOpeationRoutes(operationPMs);

            return operationPMs.ToList();
        }


        [HttpGet("DB/{operationIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Operation>>> GetOperationList(string operationIds)
        {


            var operations = await _operationRepository.GetOperations(operationIds);

            return operations.ToList();

        }

        [HttpGet("Operator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ParameterPM>>> GetOperatorParameters()
        {
            UpdateUserParameters();          
            var parameters = (await _parameterRepository.GetQueryAsync(x=>x.Type == (byte)EParameterType.CONTROLPARAMETER)).OrderBy(p => p.ParameterName).ToList ();
            return ParameterMapper.ToPresentationModels(parameters).ToList();
        }

        [HttpGet("OperationRate/{routeIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OperationRate>>> GetOperationRates(string routeIds)
        {


            var operationRates = await _operationRateRepository.GetQueryAsync (x=> routeIds.Contains (x.RouteId.ToString ())) ;

            return operationRates.OrderBy(x=>x.RouteId ).ToList ();

        }

        [HttpGet("Unit/{unitId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BasePM>>> GetOperationsbyLocationId(int unitId)
        {

            var operations = await _operationRepository.GetQueryAsync(x => x.Version == 1);

            if (unitId > 0) operations = operations.Where(x => x.LocationId != null && x.LocationId == unitId);

            return operations.Select(x => new BasePM() { Id = x.Id, Name = x.OperationName, Description = x.Description, Value=(x.LocationId??0).ToString() }).ToList();

        }



        // GET: api/operation/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OperationPM>> GetOperation(int id)
        {
            //var operation = await _operationRepository.GetByIdAsync(id);
            var operation = await _operationRepository.GetOperations(id);

            if (operation == null)
            {
                return NotFound();
            }


            var operationPM = OperationMapper.ToPresentationModel(operation.First());

            //get list of machine name     
            GetOperationResourceNames(operationPM);

            //get list of route name
            GetOpeationRoute(operationPM);


            return operationPM;
        }

        [HttpGet]
        [Route("IdName/{operationIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BasePM>>> GetOperationIdName(string operationIds)
        {
            IEnumerable<Operation> operations = new List<Operation>();
            

            if (!string.IsNullOrWhiteSpace(operationIds) && operationIds != "0")
            {
                List<string> result = operationIds.Split(',').ToList();
                operations = await _operationRepository.GetQueryAsync(x => result.Contains(x.Id.ToString()));
            }
            else
            {
                operations = await _operationRepository.GetQueryAsync(x => x.Version == 1);
            }

            var operationIdNames = operations.OrderBy(x => x.OperationName).ToList().Select(x => new BasePM() { Id = x.Id, Name = x.OperationName, Value = x.Type.ToString(), Description =x.Categroy  });

            return Ok(operationIdNames);

        }


        // GET: api/operation/resource/5
        [HttpGet("Resource/{resourceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<BasePM>>> GetOperationbyResource(int resourceId)
        {

            var operations = await _operationRepository.GetOperationsbyResource(resourceId);

            if (operations.Count() == 0)
            {
                return NotFound();
            }

            //var rrsOperations = operations.Select (x=>x.OperationName).ToList();

            var resOperations = operations.SelectMany(x => x.OperationResources).OrderBy(x => x.ResourceId).Select(x => new BasePM() { Id = x.ResourceId ?? 0, Name = x.Operation.OperationName });

            if (resourceId > 0) resOperations = resOperations.Where(x => x.Id == resourceId);

            return resOperations.ToList();

        }

        [HttpGet]
        [Route("MachineSetting/{operationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OperationRatePM>>> GetMachineSetting(int operationId)
        {
            var options = ApiGetOptionSettingIncludeName("MachineSetting");

            var operationRates = await _operationRateRepository.GetQueryAsync(x => x.RouteId == 0 && x.OperationId ==operationId);

            var byProduct = options.FirstOrDefault(x => x.OptionName == "MachineSettingByProduct");
            if (byProduct != null && byProduct.DefaultSetting == "F") operationRates = operationRates.Where(x => x.Uom != 1 && x.Uom != null);

            var byCustomer = options.FirstOrDefault(x => x.OptionName == "MachineSettingByCustomer");
            if (byCustomer == null || byCustomer.DefaultSetting == "F") operationRates = operationRates.Where(x => x.Uom != 2);

            var byMaterialType = options.FirstOrDefault(x => x.OptionName == "MachineSettingByMaterialType");
            if (byMaterialType == null || byMaterialType.DefaultSetting == "F") operationRates = operationRates.Where(x => x.Uom != 3);

            var byWOQty = options.FirstOrDefault(x => x.OptionName == "MachineSettingByWOQty");
            if (byWOQty == null || byWOQty.DefaultSetting == "F") operationRates = operationRates.Where(x => x.Uom != 4);


            var machineSettings = OperationRateMapper.ToPresentationModels(operationRates.ToList()).AsQueryable();

            var resourceIds = machineSettings.Select(x => x.ResourceId).Distinct().ToList();
            var machines = ApiGetMachines(string.Join(",", resourceIds));

            var itemIds = machineSettings.Select(x => x.ItemId).Distinct().ToList();

            var customers = new List<CustomerPM>();
            if (machineSettings.Where(x => x.ProdRateUoM == 2).Count() > 0)
                customers = ApiGetCustomers();

            foreach (var a in machineSettings)
            {
                var op = _operationRepository.GetQuery(x => x.Id == a.OperationId).FirstOrDefault();
                if (op != null) a.OperationName = op.OperationName;


                var res = machines.FirstOrDefault(x => x.Id == a.ResourceId);
                if (res != null) a.ResourceName = res.Name;

                var b = operationRates.FirstOrDefault(x => x.Id == a.Id);

                if (a.ProdRateUoM == 1 || a.ProdRateUoM == null) //product
                {
                    if (a.ItemId == 0)
                    {
                        a.ItemName = b.ModifiedBy;
                        if (a.isDefault)
                            a.Description = "Product Family(Assembly)";
                        else
                            a.Description = "Product Family(Part)";
                    }
                    else
                    {
                        if (a.isDefault)
                            a.Description = "Product(Assembly)";
                        else
                            a.Description = "Product(Part)";

                        var item = ApiGetItem(a.ItemId ?? 0);
                        if (item != null)
                            a.ItemName = item.Name;
                    }

                }
                else if (a.ProdRateUoM == 2) //customer
                {
                    var customer = customers.FirstOrDefault(x => x.Id == a.ItemId);
                    a.Description = "Customer";
                    if (customer != null) a.ItemName = customer.Name;
                }
                else if (a.ProdRateUoM == 3) //material type
                {
                    a.Description = "Material Type";
                    if (a.ItemId == 0) a.ItemName = b.ModifiedBy;
                }
                else if (a.ProdRateUoM == 4) //wo qty range
                {
                    a.Description = "WO Qty Range";
                    var paramter = _parameterRepository.GetQuery(x => x.Id == a.ItemId).FirstOrDefault();
                    if (paramter != null) a.ItemName = paramter.ParameterName;
                }
                //else if (a.ProdRateUoM == 5) //product category
                //{
                //    if (a.ItemId == 0) a.ItemName = b.ModifiedBy;
                //}

            }

            return Ok(machineSettings);
        }

        [HttpPut("MachineSetting/{operationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> UpdateMachineSetting(int operationId, [FromBody] IEnumerable<OperationRatePM> operationRatesPM)
        {
            var n = operationRatesPM.Where(x=>x.OperationId==operationId).Count();

            if (n != operationRatesPM.Count())
            {
                return BadRequest(operationRatesPM.Count()-n);
            }

            var operationRates = OperationRateMapper.FromPresentationModels(operationRatesPM);
            var operationRateIds = operationRates.Select(x => x.Id).ToList();
            
            var existingOperationRates = await _operationRateRepository.GetQueryAsync(x => operationRateIds.Contains(x.Id));

            var deletedOperationRates = await _operationRateRepository.GetQueryAsync(x => x.OperationId==operationId && x.RouteId==0 && !operationRateIds.Contains(x.Id));

            foreach (var x in deletedOperationRates)
            {
                await _operationRateRepository.DeleteAsync(x);
            }

            foreach (var x in existingOperationRates)
            {
                var updatingOperationRate = operationRates.Where(y => y.Id == x.Id).FirstOrDefault();
                x.IsDefault = updatingOperationRate.IsDefault;
                x.RunTime = updatingOperationRate.RunTime;
                x.Uom = updatingOperationRate.Uom;
                x.Cost = updatingOperationRate.Cost;
                x.Pretime = updatingOperationRate.Pretime;
                x.Posttime = updatingOperationRate.Posttime;
                x.Instruction = updatingOperationRate.Instruction;
                x.ModifiedBy = updatingOperationRate.ModifiedBy;
                await _operationRateRepository.UpdateAsync(x);
            }

            foreach (var x in operationRates.Where (x=>x.Id==0))
            {              
                await _operationRateRepository.InsertAsync(x);
            }

            await _operationRateRepository.SaveAsync();

            return  Ok(n);
  
        }

        [HttpPut("Rate/{routeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> UpdateOperationRates(int routeId,[FromBody] IEnumerable<OperationRatePM> operationRatesPM )
        {

            var n=operationRatesPM.Where(x => x.RouteId != null && x.RouteId == routeId).Count();

            if (n!= operationRatesPM.Count())
            {
                return BadRequest(0);
            }

            var operationRates = OperationRateMapper.FromPresentationModels(operationRatesPM);

            var operationIds = operationRates.Select(x => x.Id).ToList();
         
            var existingOperationRates = await _operationRateRepository.GetQueryAsync(x=>operationIds.Contains(x.Id));

            foreach (var x in existingOperationRates)
            {

                var updatingOperationRate = operationRates.Where(y => y.Id == x.Id).FirstOrDefault();
                x.IsDefault = updatingOperationRate.IsDefault;
                x.RunTime = updatingOperationRate.RunTime;
                x.Uom = updatingOperationRate.Uom;
                x.Cost = updatingOperationRate.Cost;
                x.Pretime = updatingOperationRate.Pretime;
                x.Posttime = updatingOperationRate.Posttime;
                await _operationRateRepository.UpdateAsync(x);
            }
          
            return Ok(routeId);
        }

        [HttpGet("Rate/{routeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OperationRatePM>>> GetOperationRatesByRoute(int routeId)
        {

            var route1 = ApiGetRouteById(routeId);

            if (route1==null || route1.Count ==0)
            {
                return NotFound();
            }

            UpdateOperationRate(routeId, route1);

            var codeType = ApiGetCodeSettingByName("Production Rate Measurement");

            var UOMList1 = UoMMapper.ToPresentationModels(codeType.CodeDetails.Where(x => x.Description.Contains("Per") && !x.Description.Contains("Day"))).ToList();
            var UOMList2 = UoMMapper.ToPresentationModels(codeType.CodeDetails.Where(x => !x.Description.Contains("Per") && !x.Description.Contains("Seconds"))).ToList();

            

            IEnumerable<OperationRate> operationRates = (from rate in await _operationRateRepository.GetQueryAsync()
                                                         join route  in route1 on rate.RouteId equals route.Id
                                                         join oResource in await _operationResourceRepository.GetQueryAsync() on route.Int1 equals oResource.OperationId
                                                         where (rate.RouteId == routeId && rate.RouteOperationId == route.Int2 && rate.OperationId == route.Int1 && rate.ResourceId == oResource.ResourceId)
                                                         select rate).ToList();


            IEnumerable<OperationRatePM> operationRatePMs = OperationRateMapper.ToPresentationModels(operationRates);
            foreach (var operationRatePM in operationRatePMs)
            {
                if (operationRatePM.ResourceId != null)
                {
                    //Equipment equipment = _resourceRepository.Single<Equipment>(e => e.EquipmentId == operationRatePM.ResourceId);
                    var equipment = ApiGetResourcesIdName(operationRatePM.ResourceId.ToString ()).FirstOrDefault();
                    if (equipment != null)
                    {
                        int mcType = 0;
                        double mcCost=0;
                        Int32.TryParse(equipment.Value, out mcType);
                        double.TryParse(equipment.Description, out mcCost);
                        operationRatePM.ResourceName = equipment.Name;
                        operationRatePM.ResourceType = mcType;
                        operationRatePM.ResourceCost = mcCost;
                        operationRatePM.IsInhouse = operationRatePM.ResourceType == 3 ? false : true;
                    }
                }
              
                if (operationRatePM.OperationId != null)
                {
                    var operation = _operationRepository.GetById(operationRatePM.OperationId??0);

                    if (operation.Type == 1) //cycle based
                    {
                        var a = CopyUomList(UOMList2, operationRatePM.Id);
                        operationRatePM.UOMList = new List<UoMPM>(a).ToList();
                    }
                    else
                    {
                        if (!operationRatePM.IsInhouse) //subcon
                        {
                            var a = CopyUomList(UOMList2, operationRatePM.Id);
                            operationRatePM.UOMList = new List<UoMPM>(a).ToList();
                        }
                        else
                        {
                            var a = CopyUomList(UOMList1, operationRatePM.Id);
                            operationRatePM.UOMList = new List<UoMPM>(a).ToList();
                        }

                    }

                    if (operation != null && (operationRatePM.ProdRateUoM == null || operationRatePM.ProductionRate == null))
                    {
                        operationRatePM.ProdRateUoM = operation.Type == 1 ? 256 : (operationRatePM.ResourceType == 3 ? 254 : 249);

                    }
                }

            }

            return Ok(operationRatePMs);
        }



        // POST: api/Opearation
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Operation>> AddOperation([FromBody] OperationPM operationPM)
        {

            var existingOperation = _operationRepository.GetQuery(o => o.OperationName.Equals(operationPM.Name) && o.Version == 1).SingleOrDefault();
            if (existingOperation != null)
            {
                return Conflict();
                //throw new Exception("Duplicated operation name.");
            }


            var operation = OperationMapper.FromPresentationModel(operationPM);
            operation.CreatedOn = DateTime.Now;
            operation.ModifiedOn = null;
            operation.ModifiedBy = null;

            await _operationRepository.InsertAsync(operation);

            operationPM = OperationMapper.ToPresentationModel(operation);

            return CreatedAtAction("GetOperation", new { id = operation.Id }, operationPM);

        }


        // PUT: api/operation/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<OperationPM>> UpdateOperation(int id, [FromBody] OperationPM operationPM)
        {
            if (id != operationPM.Id)
            {
                return BadRequest();
            }

            var operation = OperationMapper.FromPresentationModel(operationPM);

            //var existingOperation = _operationRepository.GetById(operationPM.Id);
            var existingOperation = (await _operationRepository.GetOperations(operationPM.Id)).FirstOrDefault();

            if (existingOperation == null)
            {
                return NotFound();
            }

            existingOperation.OperationName = operation.OperationName;
            existingOperation.LocationId = operation.LocationId;
            existingOperation.Description = operation.Description;
            existingOperation.Categroy = operation.Categroy;
            existingOperation.Instruction = operation.Instruction;
            existingOperation.Type = operation.Type;
            existingOperation.SizeMin = operation.SizeMin;
            existingOperation.SizeMultiple = operation.SizeMultiple;
            existingOperation.DurationPer = operation.DurationPer; //maxsize

            existingOperation.ModifiedBy = operation.ModifiedBy;
            existingOperation.ModifiedOn = DateTime.Now;

            try
            {
                await _operationRepository.UpdateAsync(existingOperation);

                var changed = false;
                var roIds = existingOperation.OperationResources.Select(x => x.Id).ToList();
                foreach (var roPM in operationPM.operationResourcePMs.OrderBy(x => x.ResourceId))
                {
                    var ro = existingOperation.OperationResources.FirstOrDefault(x => x.ResourceId == roPM.ResourceId);
                    if (ro != null)
                    {
                        roIds.Remove(ro.Id);
                    }
                    else
                    {
                        InsertOperationResources(roPM);
                        changed = true;
                    }
                }

                foreach (var roId in roIds)
                {
                    _operationResourceRepository.Delete(roId);
                    changed = true;
                }

                if (changed)
                {
                    operationPM.operationResourcePMs = OperationResourceMapper.ToPresentationModels(existingOperation.OperationResources).ToList();
                }
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            return Ok(operationPM);

        }

        [HttpPut("Unit/{unitId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<int>> UpdateOperationLocation(int unitId, [FromBody] int[] opeations)
        {
            if (unitId <= 0)
            {
                return BadRequest();
            }

            var allOps = (await _operationRepository.GetQueryAsync(x => x.Version == 1)).ToList();
            var existingOps = allOps.Where (x=> x.LocationId != null && x.LocationId == unitId).ToList();
            var existingIds = existingOps.Select(x => x.Id).ToList();
            var deleteIds = existingIds.Except(opeations);
            foreach (var a in existingOps.Where(x => deleteIds.Contains(x.Id)))
            {
                a.LocationId = null;
                _operationRepository.Update(a, false);
            }

            var newIds = opeations.Except(existingIds);
            foreach (var a in allOps.Where(x => newIds.Contains(x.Id)))
            {
                a.LocationId = unitId;
                _operationRepository.Update(a, false);
            }

            if (deleteIds.Count() > 0 || newIds.Count()>0)
                await _operationRepository.SaveAsync();

            return deleteIds.Count()+ newIds.Count();
        }



            // DELETE: api/Operation/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteOperation(int id)
        {

            var operation = (await _operationRepository.GetOperations(id)).FirstOrDefault ();

            if (operation == null)
            {
                return NotFound();
            }

            try
            {
                foreach (var operationParameter in operation.OperationParameters)
                {
                    await _operationParameterRepository.DeleteAsync(operationParameter);
                }

                foreach (var operationResource in operation.OperationResources)
                {
                    await _operationResourceRepository.DeleteAsync(operationResource);
                }

                //await _operationRepository.DeleteAsync(operation.Id);
                await _operationRepository.DeleteAsync(operation);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }


            return Ok(id);
        }

        [HttpGet("Copy/{operationId}/{routeOperationId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> CopyOperation(int operationId, int routeOperationId, string instruction = null, string remarks = null, string pictureId = null)
        {
            var newOperationId=await _operationRepository.CopyOperation(operationId, routeOperationId, instruction, remarks, pictureId);
            return Ok(newOperationId);
        }
        #endregion

        #region private methods

        //public IQueryable<ParameterPM> GetParameters()
        //{
        //    UpdateUserParameters();
        //    IEnumerable<Parameter> parameters = _exceptionManager.Process(() => _processRepository.GetQuery<Parameter>().Where(p => p.Type == (byte)EParameterType.CONTROLPARAMETER).OrderBy(p => p.ParameterName), "ExceptionShielding");
        //    return ParameterMapper.ToPresentationModels(parameters).AsQueryable();
        //}

        private void UpdateUserParameters()
        {
            var parameters = _parameterRepository.GetQuery().Where(p => p.Type == (byte)EParameterType.CONTROLPARAMETER).ToList ();

            //Call API to get user list
            var users = ApiGetOperators();

            if (users.Count() == 0) return;

            //var users = _exceptionManager.Process(() => _securityRepository.GetQuery<User>(u => u.UserName != "Dev"), "ExceptionShielding");

          
            foreach (var user in users)
            {
                var p = parameters.FirstOrDefault(x => x.ParameterName == user.Name);
                if (p != null)
                {
                    p.DefaultValue = user.Description;
                    _parameterRepository.Update(p);                    
                    //var equipmentParameters = _resourceRepository.GetQuery<EquipmentParameter>(x => x.ParameterID == p.ParameterID && x.Value != p.DefaultValue);
                    //foreach (var ep in equipmentParameters)
                    //{
                    //    ep.Value = p.DefaultValue;
                    //    _resourceRepository.Update(ep);
                    //}
                }
                else
                {
                    p = new Parameter() { ParameterName = user.Name, Type = 2, DataType = "2", DefaultValue = user.Description };
                    _parameterRepository.Insert(p);
                }
            }

           
            var userNames = users.Select(u => u.Name).ToList();

            foreach (var parameter in parameters.Where(p => !userNames.Contains(p.ParameterName)))
            {
                _parameterRepository.Delete(parameter);              
            }

            _parameterRepository.Save();
        }
    

        private OperationResourcePM InsertOperationResources(OperationResourcePM operationResourcePM)
        {
            var operationResource = OperationResourceMapper.FromPresentationModel(operationResourcePM);

            operationResource.Id = 0;
            operationResource.CreatedOn = DateTime.Now;
            operationResource.ModifiedOn = null;
            operationResource.ModifiedBy = null;

            try
            {
                _operationResourceRepository.Insert(operationResource);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            operationResourcePM.Id = operationResource.Id;
            return operationResourcePM;
        }
        private void GetOperationsResourceNames(IEnumerable <OperationPM> operationPMs)
        {
            var resources = ApiGetResourcesIdName("0").ToList ();

            foreach (var operationPM in operationPMs)
            {                   
                GetOperationResourceNames(operationPM, resources);                  
            }          
        }

        private void GetOperationResourceNames(OperationPM operationPM, List<BasePM> resources=null)
        {
            if (operationPM.operationResourcePMs.Count ==0) return;

            var resourceIds = operationPM.operationResourcePMs.Select(x => x.Id);

            if (resources == null)
            {
                resources = ApiGetResourcesIdName(string.Join (",", resourceIds)).ToList();
            }

            var selectedResources = resources.Where(y => resourceIds.Contains(y.Id)).Select(x => x.Name).ToList();

            operationPM.ResourceNames = string.Join(",", selectedResources);
            
        }

        private void GetOpeationRoutes(IEnumerable<OperationPM> opeartionPMs)
        {
            var routes = ApiGetRoutesbyOperation(0);
            if (routes!=null)
            {
                foreach (var operationPM in opeartionPMs)
                {
                    var selectedRoutes = routes.Where(x => x.Id == operationPM.Id);
                    GetOpeationRoute(operationPM, selectedRoutes);
                }
            }
           

        }
        private void GetOpeationRoute(OperationPM operationPM, IEnumerable<BasePM> routes=null)
        {
            if (routes==null)
            {
                routes = ApiGetRoutesbyOperation(operationPM.Id);
            }
            
            if (routes!=null )
            {
                operationPM.Routes = routes.Count();
                operationPM.RouteNames = string.Join(",", routes.Select(x => x.Name).ToList());
            }
           
        }

        private void UpdateOperationRate(int routeId, List<IdNamePM> route1)
        {

            //var route1 = ApiGetRouteById(routeId);
            if (route1 == null || route1.Count == 0) return;
            var operationRates = (from routeOperation in route1
                                  join oResource in _operationResourceRepository.GetQuery() on routeOperation.Int1 equals oResource.OperationId
                                  select new OperationRate
                                  {
                                      RouteId = routeId,
                                      RouteOperationId = routeOperation.Int2,
                                      OperationId = oResource.OperationId,
                                      ResourceId=oResource.ResourceId,
                                      Instruction= oResource.Instruction, 
                                      Cost= oResource.Cost, 
                                      Pretime=oResource.Pretime,
                                      Posttime = oResource.Posttime, 
                                      RunTime =oResource.Duration, 
                                      Uom=(int?)oResource.DurationPer, 
                                      IsDefault=oResource.IsDefault , 
                                      CreatedOn = DateTime.Now ,
                                      ModifiedBy ="New"
                                  }).ToList();


            //var newRates = new List<OperationRate>();
            foreach (var or in operationRates )
            {
               var a = _operationRateRepository.GetQuery(x=>x.RouteOperationId ==or.RouteOperationId && x.OperationId ==or.OperationId && x.ResourceId ==or.ResourceId ).FirstOrDefault ();
               if (a!=null)
               {
                    or.ModifiedBy="Found";
               }
            }

            var routeOprations= _operationRateRepository.GetQuery(x => (x.RouteId??0) == routeId &&  (x.IsDefault??false) ).Select(x => x.RouteOperationId).ToList();
            foreach (var or in operationRates.Where(x => routeOprations.Contains(x.RouteOperationId)))
            {
                or.IsDefault = false;
            }

            //  delete[Process].[OperationRate] where routeid not in (select routeid from[process].Route Union select 0 )

            bool changed = false;
            foreach (var or in operationRates.Where(x=>x.ModifiedBy=="New"))
            {
                changed = true;
                _operationRateRepository.Insert(or,false);
            }
            if (changed) _operationRateRepository.Save();

        }

        private List<UoMPM> CopyUomList(List<UoMPM> UOMList, int rateId)
        {
            var UOMList1 = new List<UoMPM>();
            int i = 300 + 8 * rateId;
            foreach (var uom in UOMList)
            {

                var uom1 = new UoMPM() { Id = ++i, UOMId = uom.Id, CodeNo = uom.CodeNo, CodeTypeId = uom.CodeTypeId, Description = uom.Description, RateId = rateId };
                UOMList1.Add(uom1);
            }

            return UOMList1;
        }

        private  IQueryable<OperationRatePM> GetMachineSetting1(int operationId)
        {

            var options = ApiGetOptionSettingIncludeName("MachineSetting");

            var operationRates =  _operationRateRepository.GetQuery(x => x.RouteId == 0);

            var byProduct = options.FirstOrDefault(x=>x.OptionName == "MachineSettingByProduct");
            if (byProduct != null && byProduct.DefaultSetting == "F") operationRates = operationRates.Where(x => x.Uom != 1 && x.Uom != null);

            var byCustomer = options.FirstOrDefault(x => x.OptionName == "MachineSettingByCustomer");  
            if (byCustomer == null || byCustomer.DefaultSetting == "F") operationRates = operationRates.Where(x => x.Uom != 2);

            var byMaterialType = options.FirstOrDefault(x => x.OptionName == "MachineSettingByMaterialType");
            if (byMaterialType == null || byMaterialType.DefaultSetting == "F") operationRates = operationRates.Where(x => x.Uom != 3);

            var byWOQty = options.FirstOrDefault(x => x.OptionName == "MachineSettingByWOQty");
            if (byWOQty == null || byWOQty.DefaultSetting == "F") operationRates = operationRates.Where(x => x.Uom != 4);


            var machineSettings = OperationRateMapper.ToPresentationModels(operationRates.ToList()).AsQueryable();
            
            var resourceIds = machineSettings.Select(x => x.ResourceId).Distinct().ToList();
            var machines = ApiGetMachines(string.Join(",", resourceIds));

            var itemIds = machineSettings.Select(x => x.ItemId).Distinct().ToList();

            var customers = new List<CustomerPM>();
            if (machineSettings.Where (x=>x.ProdRateUoM == 2).Count()>0)
                customers = ApiGetCustomers();

            foreach (var a in machineSettings)
            {
                var op = _operationRepository.GetQuery(x => x.Id == a.OperationId).FirstOrDefault();
                if (op != null) a.OperationName = op.OperationName;


                var res = machines.FirstOrDefault(x=>x.Id == a.ResourceId);
                if (res != null) a.ResourceName = res.Name;

                var b = operationRates.FirstOrDefault(x => x.Id == a.Id);

                if (a.ProdRateUoM == 1 || a.ProdRateUoM == null) //product
                {
                    if (a.ItemId == 0)
                    {
                        a.ItemName = b.ModifiedBy;
                        if (a.isDefault)
                            a.Description = "Product Family(Assembly)";
                        else
                            a.Description = "Product Family(Part)";
                    }
                    else
                    {
                        if (a.isDefault)
                            a.Description = "Product(Assembly)";
                        else
                            a.Description = "Product(Part)";

                        var item = ApiGetItem(a.ItemId??0);
                        if (item != null)
                            a.ItemName = item.Name;
                    }

                }
                else if (a.ProdRateUoM == 2) //customer
                {
                    var customer = customers.FirstOrDefault(x=>x.Id ==a.ItemId );
                    a.Description = "Customer";
                    if (customer != null) a.ItemName = customer.Name;
                }
                else if (a.ProdRateUoM == 3) //material type
                {
                    a.Description = "Material Type";
                    if (a.ItemId == 0) a.ItemName = b.ModifiedBy;
                }
                else if (a.ProdRateUoM == 4) //wo qty range
                {
                    a.Description = "WO Qty Range";
                    var paramter = _parameterRepository.GetQuery(x => x.Id == a.ItemId).FirstOrDefault();
                    if (paramter != null) a.ItemName = paramter.ParameterName;
                }
                //else if (a.ProdRateUoM == 5) //product category
                //{
                //    if (a.ItemId == 0) a.ItemName = b.ModifiedBy;
                //}

            }

            return machineSettings;
        }


        #endregion

        #region  API call
        private IList<BasePM> ApiGetResourcesIdName(string resourceIds)
        {
            string resourceURL = Environment.GetEnvironmentVariable("RPS_RESOURCE_URL");

            //sCustomerUrl = sCustomerUrl + "/" + customerId.ToString();
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(resourceURL + "IdName/");

                //HTTP GET
                try
                {
                    var responseTask = client.GetAsync(resourceIds);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        var resources = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<BasePM>>(alldata);
                        return resources;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                    throw;
                }

            }
            return null;
        }

        private List<BasePM> ApiGetRoutesbyOperation(int operationId)
        {
            string routeURL = Environment.GetEnvironmentVariable("RPS_ROUTE_URL");
            if (routeURL.Substring(routeURL.Length - 1) != "/") routeURL += "/";

            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(routeURL + "operation/");

                //HTTP GET
                try
                {
                    var responseTask = client.GetAsync(operationId.ToString());
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        var routes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BasePM>>(alldata);
                        return routes;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }


            }
            return null;

        }

        private string ApiGetOptionSettingByName(string optionName)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SETTING_URL");
            var option = HttpHelper.Get<Option>(apiBaseUrl, $"Name/{optionName}");
            return option == null ? "" : option.DefaultSetting;
        }

        private IEnumerable<BasePM> ApiGetOperators()
        {
            List<BasePM> operators = null;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_USER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    operators = HttpHelper.Get<List<BasePM>>(apiBaseUrl, $"Operator");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.InnerException != null ? e.InnerException.Message : "");
                }
            }

            return operators == null ? new List<BasePM>() : operators.ToList();
        }

        private CodeType ApiGetCodeSettingByName(string  codeName)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SETTING_CODE_URL");
            var code = HttpHelper.Get<CodeType>(apiBaseUrl, $"Name/{codeName}");
            return code;
        }

        private List<IdNamePM> ApiGetRouteById(int routeId)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_ROUTE_URL");
            if (apiBaseUrl.Substring(apiBaseUrl.Length - 1) != "/") apiBaseUrl += "/";          
            var routeOpeations = HttpHelper.Get<List<IdNamePM>>(apiBaseUrl, $"IdName/{routeId}");
            return routeOpeations;
        }

        private List<Option> ApiGetOptionSettingIncludeName(string optionName)
        {
            List<Option> options = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SETTING_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    options = HttpHelper.Get<List<Option>>(apiBaseUrl, $"Include/{optionName}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.InnerException != null ? e.InnerException.Message : "");
                }
            }

            return options == null ? new List<Option>() : options.ToList();
        }

        private IEnumerable<IdNamePM> ApiGetMachines(string resourceIds)
        {
            List<IdNamePM> machines = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_RESOURCE_URL");


            if (!string.IsNullOrWhiteSpace(apiBaseUrl) && !string.IsNullOrWhiteSpace(resourceIds))
            {
                try
                {
                    machines = HttpHelper.Get<List<IdNamePM>>(apiBaseUrl, $"IdName/{resourceIds}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.InnerException != null ? e.InnerException.Message : "");
                }
            }

            return machines == null ? new List<IdNamePM>() : machines.ToList();
        }

        private List<CustomerPM> ApiGetCustomers()
        {
            List<CustomerPM> customers = null;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    customers = HttpHelper.Get<List<CustomerPM>>(apiBaseUrl, "");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.InnerException != null ? e.InnerException.Message : "");
                }
            }

            return customers == null ? new List<CustomerPM>() : customers.ToList();
        }

        private ItemPM ApiGetItem(int itemId)
        {
            ItemPM item = null;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    item = HttpHelper.Get<ItemPM>(apiBaseUrl, $"{itemId}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.InnerException != null ? e.InnerException.Message : "");
                }
            }

            return item ;
        }

        #endregion

    }
}
