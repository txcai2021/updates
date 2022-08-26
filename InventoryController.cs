using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Net.Mime;


namespace SIMTech.APS.Inventory.API.Controllers
{
    using SIMTech.APS.Inventory.API.Repository;
    using SIMTech.APS.Inventory.API.Mappers;
    using SIMTech.APS.Inventory.API.Models;
    using SIMTech.APS.Inventory.API.PresentationModels;
    using SIMTech.APS.WorkOrder.API.PresentationModels;
    using SIMTech.APS.PresentationModels;
    using SIMTech.APS.Utilities;

    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryController(IInventoryRepository InventoryRepository)
        {
            _inventoryRepository = InventoryRepository;
        }




        [HttpGet]
        [Route("{id}")]
        public InventoryPM GetInventoryById(int id)
        {
            var inventory = _inventoryRepository.GetQuery(x => x.Id ==id).FirstOrDefault ();

            var inventoryPM = InventoryMapper.ToPresentationModel(inventory);
            inventoryPM.InventoryUsages = InventoryUsageMapper.ToPresentationModels(inventory.InventoryUsages);

            return inventoryPM;
        }

        [HttpGet("FG")]
        public IEnumerable<InventoryPM> GetAllFG()
        {
            var inventories = _inventoryRepository.GetQuery(x => x.Type == "FG").OrderBy(x => x.DateIn).ToList();

            return InventoryMapper.ToPresentationModels(inventories);
        }

        [HttpGet("RM")]
        public IEnumerable<InventoryPM> GetAllMaterials()
        {
            var inventories = _inventoryRepository.GetQuery(x => x.Type == "RM").OrderBy(x => x.DateIn).ToList();

            return InventoryMapper.ToPresentationModels(inventories);
        }

        [HttpGet("Receipt/{materialId}")]
        public IEnumerable<InventoryPM> GetAllMaterials1(int materialId)
        {
            var inventories = _inventoryRepository.GetQuery(x => x.Type == "RM1" && x.ProductId ==materialId ).OrderBy(x => x.DateIn).ToList();

            return InventoryMapper.ToPresentationModels(inventories);
        }

        [HttpGet("Allocation/{materialId}")]
        public IEnumerable<InventoryUsagePM> GetMaterialAllocations(int materialId)
        {
            List<InventoryUsagePM> allocations = new List<InventoryUsagePM>();

            var inventory = _inventoryRepository.GetQuery(x=>x.ProductId==materialId && x.Type =="RM").FirstOrDefault();

            if (inventory !=null)
            {
                allocations = InventoryUsageMapper.ToPresentationModels(inventory.InventoryUsages).ToList ();

                foreach (var allocation in allocations )
                {
                    var wo =ApiGetWorkOrderById(allocation.UsedByOrderId);
                    if (wo!=null)
                    {
                        allocation.Remarks = wo.WorkOrderNumber;
                    }
                }
                
            }

            return allocations;
        }


        [HttpGet("DB/{itemIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventoryForProducts(string itemIds)
        {
            //var routes = await _routeRepository.GetRoutes();

            var inventories = await _inventoryRepository.GetQueryAsync(x =>x.Type == "RM" && itemIds.Contains(x.ProductId.ToString ()));

            //remove cycle ref
            foreach (var inv in inventories  )
            {
                foreach (var invUsage in inv.InventoryUsages )
                {
                    invUsage.Inventory = null;
                }
            }
                
            
            return Ok(inventories);

        }

        [HttpPost("FG")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddInventory([FromBody] InventoryPM inventoryPM)
        {
            var inventory = InventoryMapper.FromPresentationModel(inventoryPM);
            inventory.CreatedOn = DateTime.Now;
            
            inventory.Type = "FG";

            _inventoryRepository.Insert(inventory);

            inventoryPM.Id = inventory.Id; 
            
            return new OkObjectResult(inventoryPM);
        }

        [HttpPost("BalanceQuantity")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<IdNamePM> GetMaterialBalanceQuantity([FromBody] IdNamePM rawMaterial)
        {
            GetRawMaterialBalanceQuantity(rawMaterial);

            return Ok(rawMaterial);
        }

        [HttpPost("BalanceQuantities")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<List<IdNamePM>> GetMaterialBalanceQuantities([FromBody] List<IdNamePM> rawMaterials)
        {

            foreach (var rawMaterial in rawMaterials )
            {
                GetRawMaterialBalanceQuantity(rawMaterial,false);
            }
           

            return Ok(rawMaterials);
        }

        [HttpPost("RM")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddRawMaterial([FromBody] InventoryPM inventoryPM)
        {
            if (inventoryPM.LocationId == 0) inventoryPM.LocationId = 2;

            var inventory = InventoryMapper.FromPresentationModel(inventoryPM);
            inventory.CreatedOn = DateTime.Now;

           

            inventory.Type = "RM1";

            _inventoryRepository.Insert(inventory);

            inventoryPM.Id = inventory.Id;

            var inventory1= _inventoryRepository.GetQuery(x => x.ProductId == inventory.ProductId && x.Type == "RM").FirstOrDefault();
            if (inventory1==null)
            {
                var inv = InventoryMapper.FromPresentationModel(inventoryPM);
                inv.Type = "RM";
                _inventoryRepository.Insert(inv);
            }
            else
            {
                inventory1.Quantity += inventory.Quantity;
                _inventoryRepository.Update(inventory1);
            }

            AllocateRawMaterial(inventory.ProductId);

            return new OkObjectResult(inventoryPM);
        }

        [HttpPost("MaterialAllocation")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<int>>> AllocateRawMaterials([FromBody] List<IdNamePM> woMaterials)
        {
            int workOrderId , materialId ,  workOrderMaterialId ;
            double requiredQty, materialQty, allocatedQty;

            var workOrderMaterialIds = new List<int>();

            foreach (var woMaterial in woMaterials )
            {
                workOrderId = woMaterial.Int1??0;
                materialId = woMaterial.Int2 ?? 0;
                workOrderMaterialId = woMaterial.Id;
                requiredQty = woMaterial.Float1??0;

                var inventory = (await _inventoryRepository.GetQueryAsync(x => x.ProductId == materialId && x.Type == "RM")).FirstOrDefault();


                if (inventory !=null)
                {
                    materialQty = (double)(inventory.Quantity??0);
                    allocatedQty = (double)(inventory.InventoryUsages.Sum(x => x.Quantity ?? 0));

                    if (materialQty -allocatedQty >=requiredQty )
                    {

                    }

                    if (@materialQty - @allocatedQty >= @requiredQty )
                    {
                        InventoryUsage inventoryUsage = new InventoryUsage() { InventoryId = inventory.Id, UsageType = 1, UsedByOrderId = workOrderId, Quantity = (decimal)requiredQty };
                        inventoryUsage.DateOut = DateTime.Now;

                        inventory.InventoryUsages.Add(inventoryUsage);

                        workOrderMaterialIds.Add(workOrderMaterialId);
                    }
                    _inventoryRepository.Update(inventory);
                }              
            }

            //if (  workOrderMaterialIds.Count > 0)
            //{
            //    var result = ApiUpdateWorkOrderMaterialShortages(workOrderMaterialIds);
            //    if (result > 0) Console.WriteLine("No. of work order materials: " + workOrderMaterialIds.Count ());
            //}

            //return Ok(workOrderMaterialIds.Count());
            return Ok(workOrderMaterialIds);
        }

        [HttpPut]
        public void UpdateInventory([FromBody] Inventory Inventory) => _inventoryRepository.Update(Inventory);

        // DELETE api/<InventoryController>/5
        [HttpDelete("{id}")]
        public void DeleteInventory(int id) => _inventoryRepository.Delete(id);

        private void AllocateRawMaterial(int materialId)
        {

            if (materialId == 0) return;

            var workOrderMaterialIds = new List<int>();

            var inventory = _inventoryRepository.GetQuery(a => a.ProductId == materialId && a.Type == "RM").FirstOrDefault();

            if (inventory != null)
            {
                var inventoryId = inventory.Id;
                var balanceQuantity = inventory.Quantity - inventory.InventoryUsages.Sum(a => (decimal?)a.Quantity);
                if (balanceQuantity > 0)
                {
                     
                    var workOrderRawmaterials = ApiGetWorkOrderMaterialShortages(materialId);
                    foreach (var workOrderMaterial in workOrderRawmaterials)
                    {
                        if (balanceQuantity >= (decimal?)workOrderMaterial.Float1)
                        {
                            InventoryUsage inventoryUsage = new InventoryUsage() { InventoryId = inventoryId, UsageType = 1, UsedByOrderId = workOrderMaterial.Int1??0, Quantity = (decimal)workOrderMaterial.Float1 };
                            inventoryUsage.DateOut = DateTime.Now;

                            inventory.InventoryUsages.Add(inventoryUsage);
                            

                            // update allocation flag via trigger
                            //workOrderMaterial.Availability = true;
                            //_orderRepository.Update(workOrderMaterial);

                            workOrderMaterialIds.Add(workOrderMaterial.Id);


                            balanceQuantity -= (decimal?)workOrderMaterial.Float1;
                        }
                    }

                    _inventoryRepository.Update(inventory);

                    if (workOrderMaterialIds!=null && workOrderMaterialIds.Count >0)
                    {
                        var result = ApiUpdateWorkOrderMaterialShortages(workOrderMaterialIds);
                        if (result>0) Console.WriteLine("The material Id:" + inventory.ProductId + " has been allocated to work orders succesfully");
                    }
                       

                }

            }

        }


        private void GetRawMaterialBalanceQuantity(IdNamePM material, bool requiredQty=true )
        {

            var supplier =ApiGetSupplierName(material.Int1??0); //int1--> supplier id
            if (supplier != null)
            {
                material.String1 = supplier.Name;
                material.String2 = supplier.Description;
            }

          
            var rawMatls = GetInventoryByCustomer(0, material.Id, true).Where(a => a.Type.Trim() == "RM" || a.PartId == (material.Int2 ?? 0)); //int2 -->linkPartId
            decimal? balanceQty = 0;
            decimal? cost = 0;
            foreach (var rawMatl in rawMatls)
            {
                balanceQty += rawMatl.CompletedQty - rawMatl.InventoryUsages.Sum(x => x.Quantity);
                cost += rawMatl.Cost - rawMatl.InventoryUsages.Sum(x => x.Cost ?? 0);
            }
            material.Int1 = (int?)balanceQty;
            material.Float1 = (double?)cost;
            if (material.Int1 == 0) material.Int1 = null;
            if (material.Float1 == 0) material.Float1 = null;
             
            if (requiredQty )
            {
                var workOrderRawmaterials = ApiGetWorkOrderMaterialShortages(material.Id);
                material.Int2 = (int?)workOrderRawmaterials.Sum(x => x.Float1 ?? 0);
            }

            if (material.Int2 == 0) material.Int2 = null;
        }

        private IEnumerable<InventoryPM> GetInventoryByCustomer(int customerId, int productId, bool hasBalanceQty)
        {

            //if (productId > 0)
            //{
            //    var workOrderIds = _orderRepository.GetQuery<WorkOrderMaterial>(x => x.MaterialId == productId && (bool)x.Availability).Select(x => x.WorkOrderId).ToList();
            //    if (workOrderIds.Count() > 0)
            //    {
            //        _inventoryRepository.Delete<InventoryUsage>(x => x.UsageType == 1 && x.Inventory.ProductID == productId && !workOrderIds.Contains(x.UsedByOrderID));
            //        _inventoryRepository.UnitOfWork.SaveChanges();
            //    }

            //}

            var inventories1 = _inventoryRepository.GetQuery();

            if (customerId > 0) inventories1 = inventories1.Where(a => a.CustomerId == customerId);
            if (productId > 0)
            {
                inventories1 = inventories1.Where(a => a.ProductId == productId);
            }

            if (hasBalanceQty) inventories1 = inventories1.Where(a => a.InventoryUsages.Count == 0 || (a.Quantity > a.InventoryUsages.Sum(b => b.Quantity)));

            var inventories = inventories1.ToList();

            var inventoryPMs = inventories.Select(GetInventory).OrderBy(i => i.PartNo).ToList();
            foreach (var inventoryPM in inventoryPMs)
            {
                var inventory = inventories.Single(so => so.Id == inventoryPM.Id);
                inventoryPM.InventoryUsages = InventoryUsageMapper.ToPresentationModels(inventory.InventoryUsages).ToList();

                if (inventoryPM.Cost != null)
                {
                    inventoryPM.BalanceCost = inventoryPM.Cost - inventoryPM.InventoryUsages.Sum(x => x.Cost ?? 0);
                    if (inventoryPM.BalancedQty == 0) inventoryPM.BalanceCost = 0;

                }


                //    foreach (var inventoryUsage in inventoryPM.InventoryUsages)
                //    {                 
                //        if (inventoryUsage.Type == 0)
                //        {
                //            DeliveryOrderDetail deliveryOrderDetail = _exceptionManager.Process(() => _orderRepository.GetByKey<DeliveryOrderDetail>(inventoryUsage.UsedByOrderId), "ExceptionShielding");

                //            if (deliveryOrderDetail != null)
                //            {
                //                inventoryUsage.Remarks = deliveryOrderDetail.Comment;
                //                inventoryUsage.DeliveryOrderNumber = deliveryOrderDetail.DeliveryOrder.DeliveryOrderNumber;
                //                inventoryUsage.DOQty = (Decimal?)deliveryOrderDetail.Quantity;

                //                var salesOrderDetail = _exceptionManager.Process(() => _orderRepository.GetByKey<SalesOrderDetail>(deliveryOrderDetail.SalesOrderLineID), "ExceptionShielding");
                //                inventoryUsage.SalesOrderNumber = salesOrderDetail.SalesOrder.SalesOrderNumber;
                //                inventoryUsage.LineNo = salesOrderDetail.LineNumber;
                //                inventoryUsage.SalesOrderQty = salesOrderDetail.OrderQty;
                //                inventoryUsage.CommittedDeliveryDate = salesOrderDetail.DueDate;
                //            }
                //        }
                //        else
                //        {
                //            WorkOrder workOrder = null;

                //            if (inventoryUsage.Type == 3) //FIFO material allocation 
                //            {
                //                InventoryUsage invUsage = _exceptionManager.Process(() => _inventoryRepository.GetQuery<InventoryUsage>(x => x.InventoryUsageID == inventoryUsage.UsedByOrderId).FirstOrDefault(), "ExceptionShielding");
                //                if (invUsage != null)
                //                    workOrder = _exceptionManager.Process(() => _orderRepository.GetQuery<WorkOrder>(x => x.WorkOrderID == invUsage.UsedByOrderID).FirstOrDefault(), "ExceptionShielding");
                //            }
                //            else
                //            {
                //                workOrder = _exceptionManager.Process(() => _orderRepository.GetQuery<WorkOrder>(x => x.WorkOrderID == inventoryUsage.UsedByOrderId).FirstOrDefault(), "ExceptionShielding");
                //            }


                //            if (workOrder != null)
                //            {
                //                inventoryUsage.Remarks = workOrder.Remarks;
                //                inventoryUsage.DeliveryOrderNumber = workOrder.WorkOrderNumber;
                //            }


                //        }

                //    }
            }

            return inventoryPMs.AsQueryable();
        }

        private InventoryPM GetInventory(Inventory item)
        {

            InventoryPM inventoryPM = InventoryMapper.ToPresentationModel(item);
           
            inventoryPM.DOQty = item.InventoryUsages.Sum(a => (decimal?)a.Quantity);
            inventoryPM.BalancedQty = inventoryPM.CompletedQty - inventoryPM.DOQty;
            inventoryPM.InStock = inventoryPM.DOQty == 0;

            if (inventoryPM.DOQty == 0) inventoryPM.DOQty = null;

            //var product = _exceptionManager.Process(() => _productRepository.GetByKey<Item>(item.ProductID), "ExceptionShielding");
            //inventoryPM.PartNo = product.ItemName;
            //inventoryPM.PartName = product.Description;
            //inventoryPM.PartFamily = product.Group1;

            //if (item.CustomerID != null && item.CustomerID > 0)
            //{
            //    var customer = _exceptionManager.Process(() => _orderRepository.GetByKey<Customer>(item.CustomerID), "ExceptionShielding");
            //    if (customer != null) inventoryPM.CustomerCode = customer.CustomerName;
            //}


            //if (item.WorkOrderID != null && item.WorkOrderID > 0)
            //{
            //    var workOrder = _exceptionManager.Process(() => _orderRepository.GetByKey<WorkOrder>(item.WorkOrderID), "ExceptionShielding");
            //    if (workOrder != null)
            //    {
            //        inventoryPM.WorkOrderNumber = workOrder.WorkOrderNumber;
            //        inventoryPM.WOQty = (decimal?)workOrder.Quantity;
            //    }

            //}

            return inventoryPM;
        }


        private List<IdNamePM> ApiGetWorkOrderMaterialShortages(int materialId)
        {
            var items = new List<IdNamePM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {

                    var result = HttpHelper.Get<List<IdNamePM>>(apiBaseUrl, $"MaterialShortage/{materialId}");

                    if (result != null) items = result;
                }
                catch (Exception e )
                {
                    Console.WriteLine(e.Message );
                    if (e.InnerException !=null) Console.WriteLine(e.Message);
                }
            }

            return items.ToList();
        }

        private WorkOrderPM ApiGetWorkOrderById(int workOrderId)
        {
            WorkOrderPM wo = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    wo = HttpHelper.Get<WorkOrderPM>(apiBaseUrl, $"{workOrderId}");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.Message);
                }
            }

            return wo;
        }

        private int ApiUpdateWorkOrderMaterialShortages(List<int> materialShortages)
        {
            int result = 0;
           
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var  task = HttpHelper.PostAsync<List<int>>(apiBaseUrl, "MaterialAllocation", materialShortages);
                    task.Wait();
                    result = task.Result; 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.Message);
                }
            }

            return result;
        }

        private BasePM ApiGetSupplierName(int supplierId)
        {
            BasePM supplier =  null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl) && supplierId >0)
            {
                try
                {
                    supplier = HttpHelper.Get<BasePM>(apiBaseUrl, $"{supplierId}");                   
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.Message);
                }
            }

            return supplier;
        }

    }
}
