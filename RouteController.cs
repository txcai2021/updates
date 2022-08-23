
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;


namespace SIMTech.APS.Routing.API.Controllers
{
    using SIMTech.APS.Routing.API.Repository;
    using SIMTech.APS.Routing.API.Mappers;
    using SIMTech.APS.Routing.API.Models;
    using SIMTech.APS.Routing.API.PresentationModels;
    using SIMTech.APS.PresentationModels;
    using  SIMTech.APS.Utilities;


    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IRouteOperationRepository _routeOperationRepository;
        private readonly IProductRouteRepository _productRouteRepository;

        public RouteController(IRouteRepository routeRepository, IRouteOperationRepository routeOperationRepository, IProductRouteRepository productRouteRepository)
        {
            _routeRepository = routeRepository;
            _routeOperationRepository = routeOperationRepository;
            _productRouteRepository = productRouteRepository;
        }

        #region API
        // GET: api/Route
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]         
        public async Task<ActionResult<IEnumerable<RoutePM>>> GetRoutes()
        {
            var routes= await _routeRepository.GetRoutes();

            //var routes = await _routeRepository.GetQueryAsync(x => x.Version == 1);

            var routesPM = RouteMapper.ToPresentationModels(routes);
            
            return Ok(routesPM);
            
        }

        [HttpGet("DB/{routeIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Route>>> GetRouteList(string routeIds)
        {
            var routes = (await _routeRepository.GetRoutes(routeIds)).ToList ();

           foreach (var route in routes )
           {
               foreach (var pd in route.ProductRoutes)
                    pd.Route = null;

               foreach (var ro in route.RouteOperationRoutes)
                    ro.Route = null;
                foreach (var ro in route.RouteOperationSubroutes)
                    ro.Route = null;

            }

            return Ok(routes);

        }


        // GET: api/Route/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoutePM>> GetRoute(int id)
        {
            var route = await _routeRepository.GetRoutes(id);

            //var route = await _routeRepository.GetByIdAsync(id);

            if (route == null)
            {
                return NotFound();
            }

            var routePM = RouteMapper.ToPresentationModel(route.First ());

             
            var operationIds = routePM.RouteOperationPMs.Select(x => x.OperationId).ToList();

            if (operationIds != null && operationIds.Count>0)
            {
                var operations = ApiGetOperationIdName(string.Join(",", operationIds)).ToList();
                foreach (var a in routePM.RouteOperationPMs)
                {
                    var b = operations.Where(x => x.Id == a.OperationId).FirstOrDefault();
                    if (b != null) a.OperationName = b.Name;
                }            
            }


            return routePM;
        }

        [HttpGet]
        [Route("IdName/{routeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<IdNamePM>>> GetRouteIdName(int routeId)
        {
            
            var route = (await _routeRepository.GetRoutes(routeId)).FirstOrDefault();
            IEnumerable<IdNamePM> routeIdNames = new List<IdNamePM>();

            if (route!=null)
            {
                routeIdNames = route.RouteOperationRoutes.Select(x => new IdNamePM() { Id = x.RouteId, Name = x.Route.RouteName, Int1 = x.OperationId, Int2 = x.Id });
            }


            return Ok(routeIdNames);

        }


        [HttpGet("Operation/{operationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<BasePM>>> GetOperationbyResource(int operationId)
        {

            var routes = await _routeRepository.GetRoutesbyOperation(operationId);
           
            var routeOperations = routes.SelectMany(x => x.RouteOperationRoutes).OrderBy(x => x.OperationId).Select(x => new BasePM() { Id = x.OperationId ?? 0, Name = x.Route.RouteName });

            if (operationId > 0) routeOperations = routeOperations.Where(x => x.Id == operationId);

            return routeOperations.ToList();

        }

        [HttpGet("ProductFamily/{productFamily}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BasePM>>> GetProductFamilyRouting(string productFamily)
        {

            var routes = await _routeRepository.GetQueryAsync(x => x.Version == 1 && !string.IsNullOrWhiteSpace(x.Comment));

            if (!string.IsNullOrWhiteSpace(productFamily))
            {
                routes = routes.Where(x =>  x.Comment == productFamily);
            }

            var routesPM = routes.Select(x => new BasePM { Id = x.Id, Name = x.RouteName, Value = x.Comment }).ToList();

            return Ok(routesPM);

        }

        [HttpGet("Product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<BasePM>>> GetProductRouting(int productId)
        {

            var routes = await _productRouteRepository.GetProductRoutes(productId);

            
            var routesPM = routes.Select(x => new BasePM { Id = x.RouteId, Name = x.Route.RouteName, Value = x.ProductId.ToString() }).ToList();

            return Ok(routesPM);

        }


        // POST: api/Opearation
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<RoutePM>> AddRoute([FromBody] RoutePM routePM)
        {

            var existingRoute = _routeRepository.GetQuery(r => r.RouteName.Equals(routePM.Name) && r.Version==1).SingleOrDefault();
            if (existingRoute != null)
            {
                return Conflict();
            }


            var route = RouteMapper.FromPresentationModel(routePM);
            route.CreatedOn = DateTime.Now;
            route.ModifiedOn = null;
            route.ModifiedBy = null;

            await _routeRepository.InsertAsync(route);

            routePM.Id = route.Id;

            //foreach (var ro in routePM.RouteOperationPMs)
            //{
            //    ro.RouteId = route.Id;
            //    //InsertRouteOperations(ro);
            //}

            if (!string.IsNullOrEmpty(routePM.PartName))
            {
                var partId = 0;
                var success =int.TryParse(routePM.PartName, out partId);
                if (success)
                    await InsertProductRoute(route.Id, partId,"Y");
            }

            routePM = RouteMapper.ToPresentationModel(route);

            return CreatedAtAction("GetRoute", new { id = route.Id }, routePM);
        }

        // PUT: api/Route/5
        [HttpDelete("Product/Reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetProductRoute([FromBody] int[] productIds)
        {         
            try
            {
                var prs = await _productRouteRepository.GetQueryAsync(x=> productIds.Contains(x.ProductId));

                
                foreach (var pr in prs)
                {
                    await _productRouteRepository.DeleteAsync(pr);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            return Ok();
            
        }

            // PUT: api/Route/5
        [HttpPut("Product/{routeId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<RoutePM>> UpdateProductRoute(int routeId, [FromBody] int[] partIds)
        {
          
            var existingRoute = (await _routeRepository.GetRoutes(routeId)).FirstOrDefault();

            if (existingRoute == null)
            {
                return NotFound();
            }

            try
            {
                if (partIds.Count()>0)
                {                  
                    foreach (var partId in partIds)
                    {
                       
                            var pr = existingRoute.ProductRoutes.FirstOrDefault(x => x.ProductId == partId);
                            if (pr == null)                           
                            {                              
                                    await InsertProductRoute(routeId, partId);
                            }
                        
                    }
                }
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            var routePM = RouteMapper.ToPresentationModel(existingRoute);

            return Ok(routePM);
        }

         // PUT: api/Route/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<RoutePM>> UpdateRoute(int id, [FromBody] RoutePM routePM)
        {
            if (id != routePM.Id)
            {
                return BadRequest();
            }

            var route = RouteMapper.FromPresentationModel(routePM);

            //var existingRoute = _routeRepository.GetById(routePM.Id);
            var existingRoute = (await _routeRepository.GetRoutes(routePM.Id)).FirstOrDefault();

            if (existingRoute==null)
            {
                return NotFound();
            }

            existingRoute.RouteName = route.RouteName;
            existingRoute.LocationId = route.LocationId;
            existingRoute.Description = route.Description;
            existingRoute.Comment = route.Comment;
                
            try
            {
                await _routeRepository.UpdateAsync(existingRoute);
            

                
                var changed = false;
                var roIds = existingRoute.RouteOperationRoutes.Select(x => x.Id).ToList();              
                foreach (var roPM in routePM.RouteOperationPMs.OrderBy (x=>x.Sequence ))
                {
                    var ro = existingRoute.RouteOperationRoutes.FirstOrDefault(x => x.Sequence == roPM.Sequence);
                    if (ro!=null)
                    {
                        roIds.Remove(ro.Id);
                        if (ro.OperationId != roPM.OperationId)
                        {
                            _routeOperationRepository.Delete(ro.Id);
                            InsertRouteOperations(roPM);
                            changed = true;
                        } 
                    }
                    else
                    {
                        InsertRouteOperations(roPM);
                        changed = true;
                    }    
                }

                foreach (var roId in roIds)
                {
                    _routeOperationRepository.Delete(roId);
                    changed = true;
                }

                if (!string.IsNullOrEmpty(routePM.PartName))
                {
                    var partId = 0;
                    var success = int.TryParse(routePM.PartName, out partId);
                    if (success)
                    {
                        var pr = existingRoute.ProductRoutes.FirstOrDefault(x=>x.ProductId==partId);                       
                        if (pr!=null)
                        {
                            if (string.IsNullOrEmpty(pr.Remarks) || pr.Remarks!="Y")
                            {
                                pr.Remarks = "Y";
                                _productRouteRepository.Update(pr);
                            }
                            var pr1 = existingRoute.ProductRoutes.FirstOrDefault(x => x.Remarks == "Y" && x.ProductId != partId);
                            if (pr1 != null) _productRouteRepository.Delete(pr1.Id);
                        }                          
                        else  
                        {
                            var pr1 = existingRoute.ProductRoutes.FirstOrDefault(x => x.Remarks == "Y" );
                            if (pr1!=null)
                            {
                                pr1.ProductId = partId;
                                _productRouteRepository.Update(pr1);
                            }
                            else
                                await InsertProductRoute(route.Id, partId, "Y");
                        }

                    }
                        
                }
                else
                {
                    var pr1 = existingRoute.ProductRoutes.FirstOrDefault(x => x.Remarks == "Y");
                    if (pr1 != null)
                    {
                        _productRouteRepository.Delete(pr1);
                    }
                }


                if (changed)
                {
                    //existingRoute = _routeRepository.GetById(routePM.Id);
                    routePM.RouteOperationPMs = RouteOperationMapper.ToPresentationModels(existingRoute.RouteOperationRoutes).ToList();
                }
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            return  Ok(routePM); 
        }

        // PUT: api/Route/5
        [HttpPut("Instruction/{routeId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<int>> UpdateRouteInstruction(int routeId, [FromBody] RoutePM routePM)
        {
            if (routeId != routePM.Id)
            {
                return BadRequest();
            }

            //var route = RouteMapper.FromPresentationModel(routePM);

            //var existingRoute = _routeRepository.GetById(routePM.Id);
            var existingRoute = (await _routeRepository.GetRoutes(routePM.Id)).FirstOrDefault();

            if (existingRoute == null)
            {
                return NotFound();
            }

            foreach (var ro in existingRoute.RouteOperationRoutes )
            {
                var a=routePM.RouteOperationPMs.Where(x => x.Id == ro.Id).FirstOrDefault();
                if (a!=null)
                {
                    ro.Instruction = a.Instruction;
                    ro.CreatedBy = a.Remarks;
                    ro.ModifiedBy = a.PictureId != null ? a.PictureId.ToString() : null;
                }
                
            }

            try
            {
                await _routeRepository.UpdateAsync(existingRoute);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            return Ok(routePM.Id);
        }


        // DELETE: api/Route/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]       
        public async Task<IActionResult> DeleteRoute(int id)
        {
            //var route = await _routeRepository.GetByIdAsync(id);
            var route = (await _routeRepository.GetRoutes(id)).FirstOrDefault();

            if (route == null)
            {
                return NotFound();
            }

            try
            {

                foreach (var ro in route.RouteOperationRoutes)
                {
                    await _routeOperationRepository.DeleteAsync(ro.Id);
                }

                foreach (var pr in route.ProductRoutes)
                {
                    await _productRouteRepository.DeleteAsync(pr.Id);
                }

                //await _routeRepository.DeleteAsync(route.Id);
                await _routeRepository.DeleteAsync(route);
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

        [HttpGet("Copy/{routeId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CopyRoute(int routeId)
        {
            if (routeId <= 0) return BadRequest(0);

            var route = await _routeRepository.GetByIdAsync(routeId);

            if (route == null) return NotFound(0);

            //skip copying if the routing template is false
            if (route.Version > 1) return Ok(route.Id);

            var newRoute = await CopyRoute(route);
            return Ok(newRoute.Id);
        }

        [HttpPost("Copy")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> CopyRoute([FromBody] CopyRoute newRoute)
        {
            if (newRoute.RouteId <= 0) return BadRequest(0);

            var route = await _routeRepository.GetByIdAsync(newRoute.RouteId);

            if (route == null) return NotFound(0);

            //skip copying if the routing template is false
            if (route.Version > 1) return Ok(route.Id);

            var route1 = await CopyRouteTemplate(route, newRoute.NewRouteName, newRoute.Description);          
            return Ok(route1.Id);
        }

        [HttpPut("Confirm/{routeId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> ConfirmRoute(int routeId, [FromBody] bool isConfirmed)
        {
            if (routeId <= 0) return BadRequest();

            var route = await _routeRepository.GetByIdAsync(routeId);

            if (route == null) return NotFound();

            route.IsActive = isConfirmed;

            await _routeRepository.UpdateAsync(route);
           
            return Ok(routeId);
        }

        #endregion

        #region private methods

        private RouteOperationPM InsertRouteOperations(RouteOperationPM routeOperationPM)
        {
            var routeOperation = RouteOperationMapper.FromPresentationModel(routeOperationPM);


            routeOperation.CreatedOn = DateTime.Now;
            routeOperation.ModifiedOn = null;
            routeOperation.ModifiedBy = null;

            try
            {
                _routeOperationRepository.Insert(routeOperation);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }
            
            routeOperationPM.Id = routeOperation.Id;            
            return routeOperationPM;
        }

        private async Task<ActionResult<ProductRoutePM>> InsertProductRoute(int routeId, int productId, string remarks="")
        {
           
            var productRoute = new ProductRoute() { RouteId = routeId, ProductId = productId, CustomerId = 0, Type = 0, Status = 0, ProductRouteName = routeId.ToString() + "-" + productId.ToString(), IsActive = true, Remarks=remarks };
            productRoute.CreatedOn = DateTime.Now;
            productRoute.ModifiedOn = null;
             

            try
            {
                var pr= (await _productRouteRepository.GetQueryAsync(x=>x.ProductId==productId)).FirstOrDefault();
                if (pr != null)
                    await _productRouteRepository.DeleteAsync(pr);

               await _productRouteRepository.InsertAsync(productRoute);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            var productRoutePM = ProductRouteMapper.ToPresentationModel(productRoute);

            return productRoutePM;
        }


        private async Task<Route> CopyRoute(Route route)
        {           

            var maxVersionNo = await _routeRepository.GetQuery(x => x.RouteName == route.RouteName).MaxAsync(x => x.Version??0);

            var copiedRoute = new Route() { RouteName = route.RouteName, Type = route.Type, Description = route.Description, Comment = route.Comment, Version=maxVersionNo + 1, IsActive = route.IsActive, IsDefault = route.IsDefault, LocationId = route.LocationId, TraceLevel = route.TraceLevel };
            await _routeRepository.InsertAsync(copiedRoute);

            var routeOperations=(await _routeOperationRepository.GetQueryAsync(x => x.RouteId == route.Id)).OrderBy(x => x.Sequence).ToList();

            foreach (var ro in routeOperations)
            {
                var copiedOperationId = 0;
                if (ro.OperationId != null)
                {
                    copiedOperationId = ApiCopyOperation(ro.OperationId??0, ro.Id, ro.Instruction,  ro.CreatedBy, ro.ModifiedBy);
                    if (copiedOperationId > 0)
                    {
                        var newRouteOperation = new RouteOperation() { RouteId = copiedRoute.Id, OperationId = copiedOperationId, Sequence = ro.Sequence, DefaultResourceId = ro.DefaultResourceId, Instruction = ro.Instruction };
                        await _routeOperationRepository.InsertAsync(newRouteOperation);
                    }
                }
                else
                {
                    var subRouteId = ro.SubrouteId??0;
                    if (subRouteId>0)
                    {
                        var route1 = await _routeRepository.GetByIdAsync(subRouteId);
                        var copiedSubroute = await CopyRoute(route1);

                        if (copiedSubroute.Id > 0)
                        {
                            var newRouteOperation = new RouteOperation() { RouteId = copiedRoute.Id, SubrouteId = copiedSubroute.Id, Sequence = ro.Sequence, DefaultResourceId = ro.DefaultResourceId, Instruction = ro.Instruction };
                            await _routeOperationRepository.InsertAsync(newRouteOperation);
                        }
                    }
                }
            }
            return copiedRoute;
        }

        private async Task<Route> CopyRouteTemplate(Route route, string routeName, string description )
        {

            
            var copiedRoute = new Route() { RouteName = routeName, Type = route.Type, Description = description, Comment = route.Comment, Version = 1, IsActive = route.IsActive, IsDefault = route.IsDefault, LocationId = route.LocationId, TraceLevel = route.TraceLevel };
            await _routeRepository.InsertAsync(copiedRoute);

            var routeOperations = (await _routeOperationRepository.GetQueryAsync(x => x.RouteId == route.Id)).OrderBy(x => x.Sequence).ToList();

            foreach (var ro in routeOperations)
            {
                var copiedOperationId = 0;
                if (ro.OperationId != null)
                {
                    //copiedOperationId = ApiCopyOperation(ro.OperationId ?? 0, ro.Id, ro.Instruction, ro.CreatedBy, ro.ModifiedBy);
                    copiedOperationId = ro.OperationId??0;
                    if (copiedOperationId > 0)
                    {
                        var newRouteOperation = new RouteOperation() { RouteId = copiedRoute.Id, OperationId = copiedOperationId, Sequence = ro.Sequence, DefaultResourceId = ro.DefaultResourceId, Instruction = ro.Instruction };
                        await _routeOperationRepository.InsertAsync(newRouteOperation);
                    }
                }
                //else
                //{
                //    var subRouteId = ro.SubrouteId ?? 0;
                //    if (subRouteId > 0)
                //    {
                //        var route1 = await _routeRepository.GetByIdAsync(subRouteId);
                //        var copiedSubroute = await CopyRoute(route1);

                //        if (copiedSubroute.Id > 0)
                //        {
                //            var newRouteOperation = new RouteOperation() { RouteId = copiedRoute.Id, SubrouteId = copiedSubroute.Id, Sequence = ro.Sequence, DefaultResourceId = ro.DefaultResourceId, Instruction = ro.Instruction };
                //            await _routeOperationRepository.InsertAsync(newRouteOperation);
                //        }
                //    }
                //}
            }
            return copiedRoute;
        }

        #endregion


        #region api call

        private int ApiCopyOperation(int operationId, int routeOperationId, string instruction=null, string remarks = null, string pictureId=null)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_OPERATION_URL");
            var url = $"Copy/{operationId}/{routeOperationId}?instruction={instruction}&remarks={remarks}&pictureId ={pictureId}";
            var copiedOperationId = HttpHelper.Get<int>(apiBaseUrl,url);
            return copiedOperationId;
        }

        private IList<BasePM> ApiGetOperationIdName(string operationIds)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_OPERATION_URL");
            var url = $"IdName/{operationIds}";
            var operationIdNames = HttpHelper.Get<List<BasePM>>(apiBaseUrl, url);
            return operationIdNames;
        }
            #endregion



        }
}
