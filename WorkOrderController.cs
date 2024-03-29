﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Mime;
using System.Text;

namespace SIMTech.APS.WorkOrder.API.Controllers
{
    using SIMTech.APS.Customer.API.PresentationModels;
    using SIMTech.APS.Models;
    using SIMTech.APS.PresentationModels;
    using SIMTech.APS.Product.API.PresentationModels;
    using SIMTech.APS.SalesOrder.API.Enums;
    using SIMTech.APS.SalesOrder.API.PresentationModels;
    using SIMTech.APS.Utilities;
    using SIMTech.APS.WorkOrder.API.Enums;
    using SIMTech.APS.WorkOrder.API.Mappers;
    using SIMTech.APS.WorkOrder.API.Models;
    using SIMTech.APS.WorkOrder.API.PresentationModels;
    using SIMTech.APS.WorkOrder.API.Repository;
    //using SIMTech.APS.WorkOrder.API.Enums;

    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrderController : ControllerBase
    {
        private readonly IWorkOrderRepository _workOrderRepository;
        private readonly IWorkOrderDetailRepository _workOrderDetailRepository;
        private readonly IWorkOrderMaterialRepository _workOrderMaterialRepository;
        private readonly ExceptionManager _exceptionManager;
        private int _defaultAssemblyleadTime = 0;
        private int _defaultInternalTurnaroundTime = 0;
        private int _countWOs = 0;
        private bool _copySORemarks1;
        private int _defaultLevel = 5;
        private int _defaultWOLen = 45;
        private int _lotSize;
        private double _remainingQty;
        private List<IdNamePM> _items = new List<IdNamePM>();
        private List<BasePM> _customers = new List<BasePM>();
        private List<BasePM> _locations = new List<BasePM>();
        private List<BasePM> _routings = new List<BasePM>();     
        private List<BasePM> _displayNames = new List<BasePM>();
        private List<SalesOrderPM> _salesOrder = new List<SalesOrderPM>();


        public WorkOrderController(IWorkOrderRepository workOrderRepository, IWorkOrderDetailRepository workOrderDetailRepository, IWorkOrderMaterialRepository workOrderMaterialRepository)
        {
            _workOrderRepository = workOrderRepository;
            _workOrderDetailRepository = workOrderDetailRepository;
            _workOrderMaterialRepository = workOrderMaterialRepository;
            _exceptionManager = new ExceptionManager();
        }

        #region APIs
        // GET: api/WorkOrder
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<WorkOrderPM>>> GetWorkOrders()
        {

            var updateStatus = ApiGetOptionSettingByName("UpdateWorkOrderStatus");

            if (updateStatus == "T")
            {
                await _workOrderRepository.UpdateWorkOrderStatus();
            }


            var workOrders = await _workOrderRepository.GetWorkOrders();

            GetAllAddtitionalDataFromOtherServices(workOrders.ToList ());

            var workOrderPMs = WorkOrderMapper.ToPresentationModels(workOrders).ToList();

          

            foreach (var workOrderPm in workOrderPMs)
            {
              
                AssignAdditionalInformaitonForWorkOrder(workOrderPm);
            }

            AssignPOnumberForChildWorkOrders(workOrderPMs);


            return workOrderPMs;
        }

        [HttpGet("Category/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<WorkOrderPM>>> GetWorkOrdersByCategory(int category)
        {

            var workOrderPMs=await GetWorkOrdersByCategoryWithPage(category);
            //var st = DateTime.Now;
            //Console.WriteLine("GetWorkOrder started at " + st.ToString ());
            //var userList = new List<string>();
            ////int role = GetUserRole(userList);
            //int role = 0;

            //var st1 = DateTime.Now;
            //var sortBy = ApiGetOptionSettingByName("WO_SortOrderByDueDate");
            //var sortOrder = ApiGetOptionSettingByName("WO_SortOrder");
            //var updateStatus = ApiGetOptionSettingByName("UpdateWorkOrderStatus");
            //Console.WriteLine("Duration for Setting " + (DateTime.Now- st1).TotalSeconds.ToString());



            //st1 = DateTime.Now;
            //if (updateStatus == "T" && category!= (int)EWorkOrderCategory.Completed)
            //{
            //    await _workOrderRepository.UpdateWorkOrderStatus();
            //    Console.WriteLine("Duration for updating work order status " + (DateTime.Now - st1).TotalSeconds.ToString());
            //}

            //st1 = DateTime.Now;
            //var workOrders = await _workOrderRepository.GetWorkOrdersByCategory((EWorkOrderCategory) category,sortBy=="T", sortOrder=="D",role,userList);
            //Console.WriteLine("Duration for retrieving work orders from DB " + (DateTime.Now - st1).TotalSeconds.ToString());

            //var workOrderPMs = WorkOrderMapper.ToPresentationModels(workOrders).ToList();

            //st1 = DateTime.Now;
            //GetAllAddtitionalDataFromOtherServices(workOrders.ToList());
            //Console.WriteLine("Duration for getting all additional data using APIs " + (DateTime.Now - st1).TotalSeconds.ToString());

            //st1 = DateTime.Now;
            //foreach (var workOrderPm in workOrderPMs)
            //{
            //    AssignAdditionalInformaitonForWorkOrder(workOrderPm);
            //}

            //AssignPOnumberForChildWorkOrders(workOrderPMs);
            //Console.WriteLine("Duration for assigning additional info " + (DateTime.Now - st1).TotalSeconds.ToString());


            //var et = DateTime.Now;
            //Console.WriteLine("GetWorkOrder ended at " + et.ToString() +" Duration(seconds):" +(et-st).TotalSeconds.ToString ());

            return workOrderPMs;
        }

        [HttpGet("Category/Count/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> CountWorkOrdersByCategory(int category)
        {
            var n = await _workOrderRepository.CountWorkOrdersByCategory((EWorkOrderCategory)category,false,true,0,new List<string>());
            return n;
        }

            [HttpGet("CategoryByPage/{category}/{pageNo}/{pageSize}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<WorkOrderPM>>> GetWorkOrdersByCategoryPage(int category, int pageNo, int pageSize)
        {
            var workOrderPMs = await GetWorkOrdersByCategoryWithPage(category,pageNo ,pageSize );           
            return workOrderPMs;
        }

        [HttpGet("WorkOrderNumber/{woNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WorkOrder>> GetWorkOrderByWorkOrderNumber(string woNumber)
        {
            //var workOrders = await _workOrderRepository.GetWorkOrders();

            var workOrder = (await _workOrderRepository.GetQueryAsync(wo => wo.WorkOrderNumber==woNumber)).FirstOrDefault();

            return Ok(workOrder);

        }

        [HttpGet("SalesOrder/{woNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesOrderLinePM>> GetSalesOrderByWorkOrderNumber(string woNumber)
        {
            //var workOrders = await _workOrderRepository.GetWorkOrders();

            var n = woNumber.IndexOf('.');

            if (n > 0) woNumber = woNumber.Substring(0, n );

            var workOrder = (await _workOrderRepository.GetQueryAsync(wo => wo.WorkOrderNumber == woNumber)).FirstOrDefault();

            if (workOrder !=null)
            {
                if (workOrder.WorkOrderDetails.Count==0)
                    workOrder.WorkOrderDetails = (await _workOrderDetailRepository.GetQueryAsync(wo => wo.WorkOrderId == workOrder.Id)).ToList();

                if (workOrder.WorkOrderDetails.Count>0)
                {
                    var soDet =ApiGetSalesOrderLine(workOrder.WorkOrderDetails.First().SalesOrderLineId??0);
                    return Ok(soDet);
                }               
            }

            return Ok(null);

        }


        [HttpPost("Schedule")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<WorkOrder>>> GetWorkOrdersForSchedule([FromBody] WorkOrderSchedule woSchedule)
        {
            //var workOrders = await _workOrderRepository.GetWorkOrders();

            var updateStatus = ApiGetOptionSettingByName("UpdateWorkOrderStatus");

            if (updateStatus == "T")
            {
                await _workOrderRepository.UpdateWorkOrderStatus();
            }

            var workOrders = (await _workOrderRepository.GetQueryAsync(wo => ((wo.Status == (byte)EWorkOrderStatus.Released && wo.Date1 >= woSchedule.StartDate && wo.Date1 <= woSchedule.EndDate)|| woSchedule.WIPs.Contains(wo.WorkOrderNumber)||( (wo.Status == (byte)EWorkOrderStatus.Pending && wo.Int6 != null && wo.Int6 == 1))) && woSchedule.UnitIds.Contains(wo.LocationId??0))).ToList();

            return Ok(workOrders.Where(x=>x.Status< (byte)EWorkOrderStatus.Completed) );

            //return Ok(workOrders);

        }

        [HttpGet("Release")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<WorkOrderPM>> GetWorkOrdersForRelease()
        {
            var startTime = DateTime.Now;

            int days = 10;

            var daysSetting = ApiGetOptionSettingByName("WO_ReleasedDays");

            if (!string.IsNullOrWhiteSpace(daysSetting))
            {
                days = int.Parse(daysSetting);
            }

            var workOrders = await _workOrderRepository.GetWorkOrdersForRelease(days);

            //var productIds = workOrders.Select(x => x.ProductId).Distinct().ToList(); 
            //var materialIds = workOrders.SelectMany(x => x.WorkOrderMaterials).Select(y => y.MaterialId).Distinct();

            //var itemIds = workOrders.SelectMany(x => x.WorkOrderDetails).Select(y => y.ItemId ?? 0).Distinct().ToList().Concat(productIds).Concat(materialIds);

            //var routeIds = workOrders.Select(x => x.RouteId ?? 0).Distinct().ToList();

            //var items = _exceptionManager.Process(() => _productRepository.GetQuery<Item>(x => itemIds.Contains(x.ItemID)), "ExceptionShielding").ToList();
            //var routes = _exceptionManager.Process(() => _processRepository.GetQuery<Route>(x => routeIds.Contains(x.RouteId)), "ExceptionShielding").ToList();

            GetAllAddtitionalDataFromOtherServices(workOrders.ToList());

            var workOrderPms = WorkOrderMapper.ToPresentationModels(workOrders).ToList();

            foreach (WorkOrderPM workOrderPm in workOrderPms)
            {
                AssignAdditionalInformaitonForWorkOrder(workOrderPm);
                //AssignAdditionalInformaitonForWorkOrder1(workOrderPm, workOrder, items, routes);
            }

            AssignPOnumberForChildWorkOrders(workOrderPms);

            var runTime = (DateTime.Now - startTime).TotalSeconds;

            return workOrderPms.ToList();
        }

        [HttpGet("SplitWorkOrders/{orderType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<String>>> GetSplitWorkOrders(int orderType)
        {
            var splitWOs = (await _workOrderRepository.GetSplitWorkOrders(orderType));

            return Ok(splitWOs.Split(","));
        }

        [HttpGet("Print/{workOrderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<int>>> GetPrintWorkOrders(int workOrderId)
        {

            var workOrderNumber = _workOrderRepository.GetById(workOrderId).WorkOrderNumber;

            var WOs = new List<int>();

            var parentWONumber = workOrderNumber;        
            var n = workOrderNumber.IndexOf(".");
            if (n > 0) parentWONumber = workOrderNumber.Substring(0, n);

            var parentWO = _workOrderDetailRepository.GetQuery(x => x.WorkOrder.WorkOrderNumber == parentWONumber).FirstOrDefault();
            var sod = 0;           
            if (parentWO != null && parentWO!= null) sod = parentWO.SalesOrderLineId ?? 0;


            var pringOption = ApiGetOptionSettingByName("WOPrintOption");

            //var pringOption = "4";

            switch (pringOption)
            {
                case "1":
                    WOs.Add(workOrderId);
                    break;
                case "2":
                    WOs=(await _workOrderRepository.GetQueryAsync(x => x.WorkOrderNumber.Contains(parentWONumber))).OrderBy (x=>x.WorkOrderNumber).Select (x=>x.Id ).ToList ();
                    break;
                case "3":                    
                    if (sod>0)
                    {
                        var parentWOs= (await _workOrderRepository.GetQueryAsync(x => x.WorkOrderDetails.Any(x=>x.SalesOrderLineId==sod))).OrderBy (x=>x.WorkOrderNumber ).Select(x => x.WorkOrderNumber).ToList();

                        foreach (var pWO in parentWOs )
                        {
                            WOs.AddRange((await _workOrderRepository.GetQueryAsync(x => x.WorkOrderNumber.Contains(pWO))).OrderBy(x => x.WorkOrderNumber).Select(x => x.Id).ToList());
                        }
                            
                    }
                                    
                    break;
                case "4":
                    var so1 = ApiGetSalesOrders(sod.ToString ());
                    
                    if (so1 != null && so1.Count > 0)
                    {
                        //var so = so1.FirstOrDefault();
                        var so =ApiGetSalesOrder(so1.First().Id);
                        
                        if (so!=null && so.SalesOrderLines.Count() >0)
                        {
                            var sodList = so.SalesOrderLines.Select(x => x.Id).ToList();

                            var parentWOs = (await _workOrderRepository.GetQueryAsync(x => x.WorkOrderDetails.Any(x => sodList.Contains (x.SalesOrderLineId??0)))).OrderBy(x => x.WorkOrderNumber).Select(x => x.WorkOrderNumber).ToList();

                            foreach (var pWO in parentWOs)
                            {
                                WOs.AddRange((await _workOrderRepository.GetQueryAsync(x => x.WorkOrderNumber.Contains(pWO))).OrderBy(x => x.WorkOrderNumber).Select(x => x.Id).ToList());
                            }
                        }
                         
                    }
                    break;
            }

            var WOs1 = WOs.Distinct().ToList ();
            if (WOs1.Count!= WOs.Count ) Console.WriteLine("Distinct WOs:" + WOs1.Count.ToString() +"/"+ WOs.Count.ToString ());
            
            return Ok(WOs1);
        }

        [HttpGet("SplitWorkOrderDetail/{orderType}/{childWO}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ChildWorkOrderPM>> GetSplitWorkOrderDetail(int orderType, string childWO)
        {
            var splitWO = (await _workOrderRepository.GetSplitWorkOrderDetail(childWO,orderType));

            if (!string.IsNullOrEmpty( splitWO.ParentWO))
            {
                //splitWO.ParentWO = "2021110001";
                var wo=_workOrderRepository.GetQuery(x => x.WorkOrderNumber == splitWO.ParentWO).FirstOrDefault();
                if (wo != null) splitWO.ParentWOId = wo.Id;
            }

            return Ok(splitWO);
        }



        [HttpGet("Status/{startDate}/{endDate}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<BasePM>>> GetWordOrderStatus(DateTime startDate, DateTime endDate)
        {
            var a =_workOrderRepository.GetQuery(x=>x.CreatedOn>=startDate && x.CreatedOn<=endDate ).OrderBy (x=>x.Status ).ThenBy (x=>x.WorkOrderNumber);
            var b = a.Select(x => new BasePM() { Id = x.Id, Name = x.WorkOrderNumber, Value = ((EWorkOrderStatus)x.Status).ToString() }).ToList();
            return Ok(b);
        }

        [HttpGet("WIP")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<WIP1>>  GetWIPs()
        {

            var wips1 =  await _workOrderRepository.GetWIPs();   
            return wips1;
        }


        [HttpGet("Schedule/{locationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IEnumerable<WorkOrderPM>> GetWorkOrdersForSchedule(int locationId)
        {        
            var loadWIP = ApiGetOptionSettingByName("LoadWIP");
            var bLoadWIP = (loadWIP != "F");

            var updateWIP = ApiGetOptionSettingByName("CanShowWIPinGrid");
            var bUpdateWIP = (updateWIP == "T");

            //Call tracking API to get latest status of WIPs
            var updateStatus = ApiGetOptionSettingByName("UpdateWorkOrderStatus");

            if (updateStatus == "T")
            {
                await _workOrderRepository.UpdateWorkOrderStatus();
            }

            var workOrders = (await _workOrderRepository.GetWorkOrdersForSchedule(locationId, bLoadWIP)).ToList ();

            if (bUpdateWIP)
            {
                IEnumerable<WIP> wips = new List<WIP>();

                if (updateStatus == "T")
                {                  
                    await _workOrderRepository.UpdateWorkOrderStatus();
                  
                    var wip1=  await _workOrderRepository.GetWIPs();
                    wips=WIPMapper.ToPresentationModels(wip1).ToList ();

                }
                else
                {
                    wips = ApiGetWIPs();
                }    



                var woNumbers = new List<string>();

                if (wips!=null && wips.Count()>0)
                 woNumbers = wips.Select(x => x.WorkOrderNumber).Distinct().ToList();

                var count = 0;
                //Add split work orders
                foreach (var w in wips)
                {
                    var existingWO = workOrders.FirstOrDefault(x => x.WorkOrderNumber == w.WorkOrderNumber);
                    if (existingWO == null)
                    {
                        count++;
                        var wo = new WorkOrder() { Id = -count, WorkOrderNumber = w.WorkOrderNumber, RouteId = w.RouteId, Quantity = w.ActualRecQty };
                        if (w.ScrapDate != null) wo.DueDate = (DateTime)w.ScrapDate;

                        var i = wo.WorkOrderNumber.IndexOf('-');
                        var j = wo.WorkOrderNumber.LastIndexOf('.');
                        var parentWO = "";
                        if (i > 0)
                        {
                            parentWO = wo.WorkOrderNumber.Substring(0, i);
                        }

                        var a = workOrders.FirstOrDefault(x => x.WorkOrderNumber == parentWO);
                        if (a != null)
                        {
                            wo.ParentWorkOrderId = a.ParentWorkOrderId;
                            wo.ProductId = a.ProductId;
                            if (wo.DueDate == DateTime.MinValue) wo.DueDate = a.DueDate;
                            wo.Date1 = a.Date1;

                            wo.Status = a.Status;
                            wo.CustomerId = a.CustomerId;
                            wo.LocationId = a.LocationId;
                            wo.OrderType = a.OrderType;

                            wo.WorkOrderMaterials = a.WorkOrderMaterials;
                            wo.String1 = parentWO;
                            workOrders.Add(wo);
                        }
                    }
                    else
                    {
                        if (w.ScrapDate != null)
                        {
                            existingWO.DueDate = (DateTime)w.ScrapDate;
                            existingWO.Quantity = w.ActualRecQty;
                        }

                    }
                }
            }

            GetAllAddtitionalDataFromOtherServices(workOrders.ToList());

            var workOrderPms = WorkOrderMapper.ToPresentationModels(workOrders).ToList();

            foreach (WorkOrderPM workOrderPm in workOrderPms)
            {
                AssignAdditionalInformaitonForWorkOrder(workOrderPm);

            }

            AssignPOnumberForChildWorkOrders(workOrderPms);

           
            return workOrderPms.ToList();
        }

        // GET: api/WorkOrder/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkOrderPM>> GetWorkOrder(int id)
        {
            
            var workOrder = await _workOrderRepository.GetWorkOrders(id);           

            if (workOrder == null)
            {
                return NotFound();
            }

            var workOrderPM = WorkOrderMapper.ToPresentationModel(workOrder.First());

            GetAllAddtitionalDataFromOtherServices(workOrder.ToList());

            AssignAdditionalInformaitonForWorkOrder(workOrderPM);

            var a = new List<WorkOrderPM>();
            a.Add(workOrderPM);
            
            AssignPOnumberForChildWorkOrders(a);
            return workOrderPM;
        }

        [HttpGet("ChildWorkOrders/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<WorkOrderPM>>> GetChildWorkOrders(int id)
        {

         
            var workOrders = await _workOrderRepository.GetWorkOrders(id, true);

            if (workOrders == null)
            {
                return NotFound();
            }

            var workOrderPms = WorkOrderMapper.ToPresentationModels(workOrders).ToList();

            GetAllAddtitionalDataFromOtherServices(workOrders.ToList());

            foreach (WorkOrderPM workOrderPm in workOrderPms)
            {
                AssignAdditionalInformaitonForWorkOrder(workOrderPm);             
            }

            AssignPOnumberForChildWorkOrders(workOrderPms);

          
            return workOrderPms;

        }

        [HttpGet("WorkOrderId/{salesOrderDetailId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<int>>> GetWorkOrderIds(int salesOrderDetailId)
        {
            var woIds= (await _workOrderDetailRepository.GetQueryAsync(x => x.SalesOrderLineId == salesOrderDetailId)).Select(x=>x.WorkOrderId).ToList();           
            return woIds;        
        }

        [HttpGet("WorkOrderNumbers/{salesOrderDetailIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<WorkOrderCreationDetails>>> GetWorkOrders(string salesOrderDetailIds)
        {

             var details = new List<WorkOrderCreationDetails>() ;


            foreach (var sol in salesOrderDetailIds.Split(","))
            {
                int id = 0;
                if (int.TryParse(sol, out id))
                {
                    var woIds = (await _workOrderDetailRepository.GetQueryAsync(x => x.SalesOrderLineId == id)).Select(x => x.WorkOrderId).ToList();
                    var wos = (await _workOrderRepository.GetQueryAsync(x => woIds.Contains(x.Id))).Select(x => x.WorkOrderNumber).ToList();
                    var wodet = new WorkOrderCreationDetails() { SalesOrderDetailId = id, WorkOrderNumbers = wos };
                    details.Add(wodet);

                }    
            }
           
            return details;
        }




        // POST: api/WorkOrder
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<WorkOrderPM>> AddWorkOrder([FromBody] WorkOrderPM workOrderPM)
        {

            var existingWorkOrder = _workOrderRepository.GetQuery(wo => wo.WorkOrderNumber.Equals(workOrderPM.WorkOrderNumber) ).SingleOrDefault();
            if (existingWorkOrder != null)
            {
                return Conflict();               
            }

            var workOrder = await InsertWorkOrder(workOrderPM);

            workOrderPM = WorkOrderMapper.ToPresentationModel(workOrder);

            return CreatedAtAction("GetWorkOrder", new { id = workOrder.Id }, workOrderPM);
        }

        

        // PUT: api/WorkOrder/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<WorkOrderPM>> UpdateWorkOrder(int id, [FromBody] WorkOrderPM WorkOrderPM)
        {
            if (id != WorkOrderPM.Id)
            {
                return BadRequest();
            }

            var workOrder = WorkOrderMapper.FromPresentationModel(WorkOrderPM);

            //var existingWorkOrder = _workOrderRepository.GetById(WorkOrderPM.Id);
            var existingWorkOrder = (await _workOrderRepository.GetWorkOrders(WorkOrderPM.Id)).FirstOrDefault();

            if (existingWorkOrder == null)
            {
                return NotFound();
            }

            existingWorkOrder.UpdateFrom(workOrder);

            try
            {
                await _workOrderRepository.UpdateAsync(existingWorkOrder);
              
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            return Ok(WorkOrderPM);

        }

        [HttpPut("Release/{userName}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<int>> ReleaseWorkOrders(string userName,[FromBody] List<WorkOrderReleasePM> workOrders)
        {
            var existingWOs = await _workOrderRepository.GetQueryAsync(x => x.Status == (byte)EWorkOrderStatus.Pending || x.Status == (byte)EWorkOrderStatus.Released);

            var modifiedWOs = existingWOs.Where(wo => workOrders.Any(rel => rel.workOrderId == wo.Id && ((byte)rel.Status != wo.Status || (rel.ReleasedDate!=null && wo.Date1!=null && rel.ReleasedDate != wo.Date1)))).ToList();

            var modifiedWODets = (await _workOrderDetailRepository.GetQueryAsync(x=>modifiedWOs.Select (y=>y.Id).Distinct ().Contains(x.WorkOrderId) )).ToList();

            var itemIds = modifiedWOs.Select(x => x.ProductId).Distinct().ToList();
            var items = _items;
            int n = 0;
            foreach (var itemId in itemIds)
            {
                if (_items.FirstOrDefault(x => x.Id == itemId) == null) n++;
            }

            if (n>0) items = ApiGetProducts(string.Join(",", itemIds));

            var workOrdersForRelease = new List<WorkOrderIntegrationPM>();


            foreach (var wo in modifiedWOs.Where(x=>x.Status ==(byte)EWorkOrderStatus.Pending ))
            {
                var wo1 = workOrders.FirstOrDefault(x => x.workOrderId == wo.Id);
                if ( wo.RouteId != null && wo1!=null && wo1.ReleasedDate!=null)
                {

                    wo.Status = (byte)EWorkOrderStatus.Released;
                    wo.Date1 = (wo1.ReleasedDate??DateTime.Now ).Date ;
                    wo.String4 = userName;

                    await _workOrderRepository.UpdateAsync(wo);

                    //AllocateInventoryToWorkOrder(workOrderId);

                    //dispatch to integration tables
                    if (wo.Quantity > 0)
                    {
                        //var product =ApiGetProduct(wo.ProductId);
                        var product = items.FirstOrDefault(x => x.Id == wo.ProductId);
                        if (product != null)
                        {
                            var woId = wo.Id;
                            //if (wo.ParentWorkOrderId != null && wo.ParentWorkOrderId >0) woId= wo.ParentWorkOrderId??0;   
                            
                            var a = modifiedWODets.FirstOrDefault(x => x.WorkOrderId == woId);
                            //if (a == null && wo.ParentWorkOrderId != null && wo.ParentWorkOrderId > 0)
                            //{
                            //    var b = modifiedWOs.FirstOrDefault(x => x.Id == woId);
                            //    if (b != null && b.ParentWorkOrderId != null && b.ParentWorkOrderId > 0) a = modifiedWODets.FirstOrDefault(x => x.WorkOrderId == b.ParentWorkOrderId);
                            //}

                            var releasedWo = new WorkOrderIntegrationPM()
                            {
                                Id = wo.Id,
                                WorkOrderNumber = wo.WorkOrderNumber,
                                IssueDate = wo.IssueDate,
                                DueDate = wo.DueDate,
                                ProductId = wo.ProductId,
                                ProductNo = product.Name,
                                //ProductFamily = product.PartFamily,
                                ProductFamily = product.String1,
                                Quantity = wo.Quantity,
                                Priority = wo.Priority ?? 50,
                                RouteId = wo.RouteId ?? 0,
                                //SalesOrderDetailId = wo.WorkOrderDetails.Count() == 0 ? null : wo.WorkOrderDetails.First().SalesOrderLineId,
                                SalesOrderDetailId = (a!=null ? a.SalesOrderLineId:null),
                            };

                            workOrdersForRelease.Add(releasedWo);
                            //await ApiIntegrateForRelasedWO(releasedWo);
                        }
                        else
                            Console.WriteLine("part not found:" + wo.ProductId.ToString());
                        
                    }
                }
            }

            Console.WriteLine("Count of workOrders for release:" + workOrdersForRelease.Count.ToString ());
            if (workOrdersForRelease.Count > 0)
                await ApiIntegrateForRelasedWOs(workOrdersForRelease);

            string woIdsforUnrelease = "";
            foreach (var wo in modifiedWOs.Where(x => x.Status == (byte)EWorkOrderStatus.Released))
            {
                var wo1 = workOrders.FirstOrDefault(x => x.workOrderId == wo.Id);
                if (wo1 == null) continue;
                if (wo1.Status ==EWorkOrderStatus.Pending)
                {

                    wo.Status = (byte)EWorkOrderStatus.Pending;
                    wo.Date1 = null;
                    wo.String4 = "";
                   
                    await _workOrderRepository.UpdateAsync(wo);

                    //DeallocateInventoryToWorkOrder(workOrderId);

                    if (woIdsforUnrelease == "")
                        woIdsforUnrelease = wo.Id.ToString();
                    else
                        woIdsforUnrelease += ","+wo.Id.ToString();
                    
                    //await ApiIntegrateForUnrelasedWO(wo.Id);                   
                }
                else
                {
                    wo.Date1 = (wo1.ReleasedDate??DateTime.Now ).Date;
                    wo.String4 = userName;

                    await _workOrderRepository.UpdateAsync(wo);

                    //// DeallocateInventoryToWorkOrder(workOrderId);
                    //await ApiIntegrateForUnrelasedWO(wo.Id);

                    ////AllocateInventoryToWorkOrder(workOrderId);
                    ////dispatch to integration tables again
                    //if (wo.Quantity > 0)
                    //{
                    //    var releasedWo = new WorkOrderReleasePM()
                    //    {
                    //        workOrderId = wo.Id,
                    //        ReleasedDate = wo.Date1
                    //    };
                    //    await ApiIntegrateForRelasedWO(releasedWo);
                    //}

                }
            }

            if (woIdsforUnrelease != "")
                await ApiIntegrateForUnrelasedWOs(woIdsforUnrelease);

            return Ok(modifiedWOs.Count);
        }

        // DELETE: api/WorkOrder/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteWorkOrder(int id)
        {
            var salesOrderDetailId = 0;

            var workOrder = (await _workOrderRepository.GetWorkOrders(id)).FirstOrDefault();
            
           
            if (workOrder == null)
            {
                return NotFound();
            }

            var a = workOrder.WorkOrderDetails.FirstOrDefault();
            if (a != null) salesOrderDetailId = a.SalesOrderLineId??0;

            try
            {

                await DeleteWorkOrder(workOrder);

                var totalWOQty = (await _workOrderDetailRepository.GetQueryAsync(x => x.SalesOrderLineId == salesOrderDetailId)).ToList().Sum(x => x.Quantity);
                
                var sol=ApiGetSalesOrderLine(salesOrderDetailId);

                if (totalWOQty == 0 && sol!=null)
                {
                    sol.Status = ESalesOrderLineStatus.Pending;
                    sol.BalanceQuantity = sol.Quantity;
                    ApiUpdateSalesOrderLineStatus(sol);
                }
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



        // Post: api/WorkOrder/Generation
        [HttpPost("Generation")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> CreateWorkOrderForSalesOrder([FromBody] WorkOrderCreation woCreation )
        {
           
            if (woCreation.SalesOrderId <= 0 || woCreation.Details == null) return BadRequest();

            if (woCreation.LocationId ==null || woCreation.LocationId ==0)
            {
                var unit =ApiGetDefaultUnit();
                if (unit != null) woCreation.LocationId = unit.Id;
            }

            int.TryParse(ApiGetOptionSettingByName("LT_Internal"), out _defaultInternalTurnaroundTime);
            int.TryParse(ApiGetOptionSettingByName("LT_Assembly"), out _defaultAssemblyleadTime);
            _copySORemarks1 = (ApiGetOptionSettingByName("Remark1List") != "T");

            var so = ApiGetSalesOrder(woCreation.SalesOrderId);

            //work order will be generated for all selected line itmes
                                         
            if (so == null) return NotFound();


            _countWOs =0;

            foreach (var a in woCreation.Details)
            {
                var sol= so.SalesOrderLines.FirstOrDefault(x => x.Id == a.SalesOrderDetailId);

                if (sol != null)
                {
                    //if (!CheckSalesOrderLineStatus(sol)) return Conflict();
                    _remainingQty = 0;
                    Console.WriteLine("Sales Order Line:"+sol.Id.ToString() +"/"+sol.BalanceQuantity +"/"+sol.Status );
                    if (sol.BalanceQuantity <= 0 || sol.Status !=ESalesOrderLineStatus.Pending ) continue;

                    var workOrderPM = await GenerateWorkOrders(sol, so.CustomerId ?? 0, a.Quantity, woCreation.RequestedBy);

                    if (workOrderPM != null)
                    {
                        workOrderPM.LocationId = woCreation.LocationId;
                        var wo = await InsertWorkOrder(workOrderPM);
                        if (_remainingQty>0 && _lotSize>0)
                        {
                            await GenerateWObyLotSize(wo.Id,_lotSize, _remainingQty);
                        }
                        var totalWOQty = (await _workOrderDetailRepository.GetQueryAsync(x => x.SalesOrderLineId == sol.Id)).ToList ().Sum(x => x.Quantity);
                        
                        if (totalWOQty>0)
                        {
                            sol.Status = ESalesOrderLineStatus.WorkOrderIssued;
                            sol.BalanceQuantity = sol.Quantity - totalWOQty;
                            ApiUpdateSalesOrderLineStatus(sol);
                        }

                        GenerateMaterialShortageMessage();

                    }
                }
                
            }
             
            return Ok(_countWOs);

        }

        [HttpPost("Material")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]       
        public async Task<ActionResult<int>> UpdateMaterialAllocation([FromBody] IdNamePM material)
        {

            var materialId = material.Id;
            var materialName = material.Name;
            var qty = material.Float1;

            var woMaterials= await _workOrderMaterialRepository.GetQuery(x=>x.MaterialId ==material.Id && (x.Availability ==null || x.Availability ==false)).ToListAsync();

            var balQty = material.Float1;
            foreach (var wom in woMaterials  )
            {
                if (balQty>=wom.Quantity )
                {
                    wom.Availability = true;
                    _workOrderMaterialRepository.Update(wom);
                    balQty -= wom.Quantity;
                }
               
            }
             
            return Ok(woMaterials.Count );
        }

        [HttpPost("MaterialAllocation")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> UpdateWorkOrderMaterialAllocations([FromBody] List<int> woMaterials)
        {
            if (woMaterials!=null && woMaterials.Count >0)
            {
                foreach (var wom in woMaterials)
                {
                    var woMaterial = _workOrderMaterialRepository.GetById(wom);
                    woMaterial.Availability = true;
                    _workOrderMaterialRepository.Update(woMaterial, false);
                    Console.WriteLine("Material for WOMaterial Id:" + wom.ToString() + " has been allocated");
                }

                await _workOrderMaterialRepository.SaveAsync();

                var woIds = _workOrderMaterialRepository.GetQuery(x => woMaterials.Contains(x.Id)).Select(x => x.WorkOrderId).Distinct().ToList();

                if (woIds.Count >0)
                {
                    var woIds1 = _workOrderMaterialRepository.GetQuery(x => woIds.Contains(x.WorkOrderId) && !(x.Availability ?? false)).Select (x=>x.WorkOrderId ).Distinct ().ToList();
                    var relasingWOIds = woIds.Except(woIds).ToList ();
                    var releasingWorkOrders = new List<WorkOrderReleasePM>();
                    
                    //release work orders if all materials are allocated.
                    foreach (var woId in relasingWOIds)                  
                    {
                        releasingWorkOrders.Add(new WorkOrderReleasePM() { workOrderId = woId, ReleasedDate = DateTime.Today, Status = EWorkOrderStatus.Released });
                    }

                    if (releasingWorkOrders.Count >0)
                        await ReleaseWorkOrders("AutobyReceivingMaterial", releasingWorkOrders);

                }
                
            }




            return Ok(woMaterials.Count);
        }

        [HttpGet("MaterialShortage/{materialId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<IdNamePM>>> GetMaterialShortage(int materialId)
        {
            var materialShortages = new List<IdNamePM>();

            var woMaterials = await _workOrderMaterialRepository.GetQuery(a => a.MaterialId == materialId && !(a.Availability ?? false) && (byte)a.WorkOrder.Status <= (byte)EWorkOrderStatus.Queuing).ToListAsync();

            

            foreach (var woMaterial in woMaterials.OrderBy (x=>x.WorkOrderId)  )
            {
                materialShortages.Add(new IdNamePM() { Id = woMaterial.Id, Int1 = woMaterial.WorkOrderId,  Float1 = woMaterial.Quantity });
            }

            return Ok(materialShortages);
        }

        //get material shortage based on po number and line no
        [HttpGet("MaterialShortage1/{materialId}/{poNumber}/{lineNo}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<IdNamePM>>> GetMaterialShortage1(int materialId, string poNumber, int lineNo)
        {
            var materialShortages = new List<IdNamePM>();

            var salesOrder =ApiGetSalesOrderByPO(poNumber);

            if (salesOrder !=null)
            {
                var solId = 0;
                var sol = salesOrder.SalesOrderLines.Where(x => x.LineNumber == lineNo).FirstOrDefault();
                if (sol != null) solId = sol.Id;

                var woIds = new List<int>();
              
                var wos =_workOrderRepository.GetQuery(x => x.WorkOrderDetails.Any(x => x.SalesOrderLineId == solId)).ToList();
                foreach (var wo in wos)
                {
                    woIds.AddRange (_workOrderRepository.GetQuery(x => x.WorkOrderNumber.Contains(wo.WorkOrderNumber)).Select(x => x.Id).ToList());
                }

                var woMaterial1s = await _workOrderMaterialRepository.GetQuery(a => woIds.Contains (a.WorkOrderId ) && a.MaterialId == materialId && !(a.Availability ?? false) && (byte)a.WorkOrder.Status <= (byte)EWorkOrderStatus.Queuing).ToListAsync();

                foreach (var woMaterial in woMaterial1s.OrderBy(x => x.WorkOrderId))
                {
                    materialShortages.Add(new IdNamePM() { Id = woMaterial.Id, Int1 = woMaterial.WorkOrderId, Float1 = woMaterial.Quantity });
                }
            }
            else
            {
                var woMaterials = await _workOrderMaterialRepository.GetQuery(a => a.MaterialId == materialId && !(a.Availability ?? false) && (byte)a.WorkOrder.Status <= (byte)EWorkOrderStatus.Queuing).ToListAsync();

                foreach (var woMaterial in woMaterials.OrderBy(x => x.WorkOrderId))
                {
                    materialShortages.Add(new IdNamePM() { Id = woMaterial.Id, Int1 = woMaterial.WorkOrderId, Float1 = woMaterial.Quantity });
                }
            }



            return Ok(materialShortages);
        }

        [HttpGet("MaterialsShortage/{materialIds}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<IdNamePM>>> GetMaterialsShortage(string materialIds)
        {
            var materialShortages = new List<IdNamePM>();

            var matIds = materialIds.Split(",").Select(x => Int32.Parse(x)).ToList();

            var woMaterials = await _workOrderMaterialRepository.GetQuery(a => matIds.Contains(a.MaterialId) && !(a.Availability ?? false) && (byte)a.WorkOrder.Status <= (byte)EWorkOrderStatus.Queuing).ToListAsync();



            foreach (var woMaterial in woMaterials.OrderBy(x=>x.MaterialId).ThenBy(x => x.WorkOrderId))
            {
                materialShortages.Add(new IdNamePM() { Id = woMaterial.MaterialId, Int1 = woMaterial.WorkOrderId, Float1 = woMaterial.Quantity });
            }

            return Ok(materialShortages);
        }

        [HttpPost("Status")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> UpdateWorkOrderStatus([FromBody] IdNamePM woStatus)
        {

            var woNumber = woStatus.Name;
            var category = woStatus.Category;
            var status = woStatus.String1;
            var currentOP = woStatus.String2;
            var completedQty = woStatus.Int1;
            var scrapQty = woStatus.Int2;
            var completedDate = DateTime.Now ;
            if (!String.IsNullOrWhiteSpace(woStatus.String3)) completedDate = DateTime.Parse(woStatus.String3);

            Console.WriteLine("UpdateWorkOrderStatus--> work order number:" + woNumber + ",category:" + category + ",status:" + status + ",currentOP:" + currentOP + ",completedQty:" + completedQty.ToString() + ",scrapQty:" + scrapQty.ToString() + ",completedDate" + completedDate);
          

            var  wo = (await _workOrderRepository.GetQueryAsync(x=>x.WorkOrderNumber ==woNumber)).FirstOrDefault ();
            var woDet = (await _workOrderDetailRepository.GetQueryAsync(x => x.WorkOrderId == wo.Id)).FirstOrDefault();

            var statusCode = ApiGetStatusCode(status);

            //if (wo!=null)
            //if (wo!=null && statusCode> (byte)EWorkOrderStatus.Released)
            if (wo!=null && statusCode> (byte)EWorkOrderStatus.Released && wo.Status >= (byte)EWorkOrderStatus.Released && wo.Status <= (byte)EWorkOrderStatus.Completed)
            {
                if (category =="Process")
                {                 
                    wo.Status = statusCode;

                    if (statusCode != (byte)EWorkOrderStatus.Completed)
                    {
                        wo.String3 = status;
                        wo.MaxString1 = currentOP;
                        await _workOrderRepository.UpdateAsync(wo);
                    }
                    else
                    {
                        //wo.String3 = "";
                        //wo.CompletedDate = completedDate;
                        //await _workOrderRepository.UpdateAsync(wo);

                        //var sol = ApiGetSalesOrderLine(woDet.SalesOrderLineId ?? 0);
                        //sol.Status = ESalesOrderLineStatus.Completed;
                        //if (sol != null) ApiUpdateSalesOrderLineStatus(sol);
                    }

                    
                    UpdateSalesOrderStatus(woDet.SalesOrderLineId ?? 0, statusCode);

                }
                else
                {
                    Console.WriteLine("Work Order Status:" + wo.ToString() +"/"+ status);
                    wo.Status = statusCode;
                    wo.String3 = status;    
                    if (statusCode == (byte)EWorkOrderStatus.Completed)
                    {
                        wo.String3 = "";
                        wo.CompletedDate = completedDate;
                    }
                    await _workOrderRepository.UpdateAsync(wo);

                    if (statusCode == (byte)EWorkOrderStatus.Completed)
                    {
                        var materialId = 0;
                        if (!string.IsNullOrWhiteSpace(woStatus.Description))
                            materialId = ApiGetLinkedMaterial(woStatus.Description);
                        else
                            materialId = ApiGetLinkedMaterial(wo.ProductId);

                        var material = new Material()
                        {
                            LocationId = wo.LocationId,
                            DateIn = DateTime.Now,
                            Remarks = "Completed from tracking system",
                            PartId = wo.ProductId,
                            CompletedQty = completedQty,
                            CustomerId = wo.CustomerId,
                            workOrderId = wo.Id,
                        };


                        if (materialId > 0)
                        {
                            material.PartId = materialId;
                            var result = ApiAddInventory(material, "RM");
                            Console.WriteLine("Raw Material has beed added:" + materialId.ToString());
                        }
                        else
                        {
                            var result = ApiAddInventory(material, "FG");
                            Console.WriteLine("FG has beed added:" + wo.ProductId.ToString());
                        }
                    }
                }
               
            }

            return Ok(1);

        }


        [HttpPost("Status/{status}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> UpdateWorkOrderListStatus(string status,[FromBody] int[] workOrderIds)
        {           

            var wos = (await _workOrderRepository.GetQueryAsync(x => workOrderIds.Contains (x.Id))).ToList ();
          

            var statusCode = ApiGetStatusCode(status);
            Console.WriteLine("To Status Code:" + statusCode.ToString());
           

            if (wos.Count>0)
            {
                Console.WriteLine("From Status Code:" + wos.First().Status.ToString ());
                foreach (var wo in wos.Where (x=>x.Status <statusCode ))
                {
                    Console.WriteLine("Status of WO:" + wo.WorkOrderNumber + " will be updated");
                    wo.Status = statusCode;
                    _workOrderRepository.Update(wo, false);

                    var woDet = (await _workOrderDetailRepository.GetQueryAsync(x => x.WorkOrderId == wo.Id)).FirstOrDefault();

                    if (woDet!=null )
                    {
                        UpdateSalesOrderStatus(woDet.SalesOrderLineId ?? 0, statusCode);                      
                    }
                }

                await _workOrderRepository.SaveAsync();
            }

            return Ok(wos.Count);

        }


        [HttpPut("SalesOrderStatus")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public  IActionResult PublishSalesOrderStatus([FromBody] SalesOrderStatus salesOrder)
        {            
            var result = PublishMessage("SO", null, salesOrder);

            return Ok(result);
        }

        [HttpPut("MaterialShortage")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult PublishMaterialShortage()
        {
            var n = GenerateMaterialShortageMessage();

            return Ok(n);
        }
      

        #endregion

        #region private methods

        private WorkOrderRequiredMaterial GetWorkOrderMaterials(WorkOrder wo, List<WorkOrderMaterial> workOrderMaterials)
        {
            var part = ApiGetProduct(wo.ProductId);
            if (part == null) return null;
            var woRequiredMaterial = new WorkOrderRequiredMaterial() { WorkorderId = wo.Id, WorkorderNumber = wo.WorkOrderNumber, PartId = wo.ProductId, PartNo = part == null ? "" : part.Name, Quantity = wo.Quantity };

            foreach (var woMaterial in workOrderMaterials.Where(x => x.WorkOrderId == wo.Id))
            {
                var material = ApiGetProduct(woMaterial.MaterialId);
                woRequiredMaterial.RequiredRawMaterials.Add(new RequiredRawMaterial() { RawMaterialId = woMaterial.MaterialId, RawMaterialNo = material == null ? "" : material.Name, BalanceQuantity = 0, RequiredQuantity = woMaterial.Quantity ?? 0, Availability = 0 });
                woMaterial.ModifiedBy = "Request for Rebalancing";

            }

            return woRequiredMaterial;
        }

        private int GenerateMaterialShortageMessage()
        {
            var workOrderMaterials = _workOrderMaterialRepository.GetQuery(x => !(x.Availability ?? false) && x.ModifiedBy == null).ToList();
            var workOrderIds = workOrderMaterials.Select(x => x.WorkOrderId).ToList();
            
            var workOrderNumbers = _workOrderRepository.GetQuery(x => workOrderIds.Contains(x.Id) && x.WorkOrderNumber.Contains(".")).Select (x=>x.WorkOrderNumber ).Distinct ().ToList();
            foreach (var wo in workOrderNumbers)
            {
                var parenetWO = wo.Substring(0, wo.IndexOf('.'));
                var pwo = _workOrderRepository.GetQuery(x => x.WorkOrderNumber == parenetWO).FirstOrDefault();
                if (pwo != null && !workOrderIds.Contains(pwo.Id))
                {
                    Console.WriteLine("Added Work Order  for shortage material list:" + pwo.Id.ToString() +"/"+ pwo.WorkOrderNumber);
                    workOrderIds.Add(pwo.Id);
                }
            }
            
            var soDetIds = _workOrderDetailRepository.GetQuery(x => workOrderIds.Contains(x.WorkOrderId)).Select(x => new { x.WorkOrderId, x.SalesOrderLineId }).Distinct().ToList();

            foreach (var soDetId in soDetIds.Select(x => x.SalesOrderLineId).Distinct().ToList())
            {
                var sod = ApiGetSalesOrderLine(soDetId ?? 0);
                if (sod == null) continue;
                var soMaterial = new SalesOrderMaterial() { Title = "Request for Rebalancing", CreatedDate = DateTime.Now, FactoryId = "SIMTECH", SalesOrderNo = sod.SalesOrderNumber, LineNo = sod.LineNumber ?? 0, PartId = sod.ProductId, PartNo = sod.ProductName, Quantity = sod.Quantity };
                foreach (var woid in soDetIds.Where(x => x.SalesOrderLineId == soDetId).Select(x => x.WorkOrderId))
                {
                    var wo = _workOrderRepository.GetById(woid);
                    //var woRequiredMaterial = GetWorkOrderMaterials(wo, workOrderMaterials);
                    //if (woRequiredMaterial == null) continue;

                    //soMaterial.WorkOrders.Add(woRequiredMaterial);

                    var childWorkOrders = _workOrderRepository.GetQuery(x => x.WorkOrderNumber.Contains(wo.WorkOrderNumber)).ToList();
                    foreach (var childWO in childWorkOrders.OrderBy(x => x.WorkOrderNumber))
                    {
                        var woRequiredMaterial = GetWorkOrderMaterials(childWO, workOrderMaterials);
                        if (woRequiredMaterial == null) continue;

                        soMaterial.WorkOrders.Add(woRequiredMaterial);
                    }
                }

                var result = PublishMessage("RM", soMaterial, null);
            }

            foreach (var wom in workOrderMaterials.Where(x => x.ModifiedBy != null))
            {
                _workOrderMaterialRepository.Update(wom);
            }

            return workOrderMaterials.Where(x => x.ModifiedBy != null).Count();
        }

        private bool PublishMessage(string messageType, SalesOrderMaterial salesOrderMaterial, SalesOrderStatus salesOrderStatus)
        {

            var rabbit_enable = Environment.GetEnvironmentVariable("RABBITMQ_ENABLE");

            if (rabbit_enable == null || rabbit_enable != "100") return false;


            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT")),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME"),
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")
            };

            var vHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? string.Empty;
            if (vHost != string.Empty) factory.VirtualHost = vHost;

            Console.WriteLine(factory.HostName + ":" + factory.Port + "/" + factory.VirtualHost);


            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "cpps-rps";

                if (exchangeName != string.Empty) channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

                var queueNameList = Environment.GetEnvironmentVariable("RABBITMQ_QUEUES_SALESORDER") ?? "rps-salesorder-status";
                if (messageType =="RM") queueNameList = Environment.GetEnvironmentVariable("RABBITMQ_QUEUES_INVENTORY") ?? "rps-material-shortage";
                var queueNames = queueNameList.Split(",");

                foreach (var queueName in queueNames)
                {
                    channel.QueueDeclare(queue: queueName,
                                       durable: true,
                                       exclusive: false,
                                       autoDelete: false,
                                       arguments: null);

                    if (messageType=="RM")
                        channel.QueueBind(queueName, exchangeName, "rps-material-shortage", null);
                    else
                        channel.QueueBind(queueName, exchangeName, "rps-salesorder-status", null);
                }

                if (messageType =="RM")
                {
                    var body = Encoding.Default.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(salesOrderMaterial));


                    channel.BasicPublish(exchange: exchangeName,
                                            routingKey: "rps-material-shortage",
                                            basicProperties: null,
                                            body: body);
                }
                else
                {
                    var body = Encoding.Default.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(salesOrderStatus));


                    channel.BasicPublish(exchange: exchangeName,
                                            routingKey: "rps-salesorder-status",
                                            basicProperties: null,
                                            body: body);
                }
                
                
            }

            return true;
        }

        private WorkOrderLinePM InsertWorkOrderDetails(WorkOrderLinePM workOrderLinePM)
        {
            var workOrderDetails = WorkOrderLineMapper.FromPresentationModel(workOrderLinePM);

            workOrderDetails.Id = 0;
            workOrderDetails.CreatedOn = DateTime.Now;
            workOrderDetails.ModifiedOn = null;
            workOrderDetails.ModifiedBy = null;

            try
            {
                _workOrderDetailRepository.Insert(workOrderDetails);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            workOrderLinePM.Id = workOrderDetails.Id;
            return workOrderLinePM;
        }

        private WorkOrderMaterialPM InsertWorkOrderMaterials(WorkOrderMaterialPM workOrderMaterialPM)
        {
            var workOrderMaterial = WorkOrderMaterialMapper.FromPresentationModel(workOrderMaterialPM);

            workOrderMaterial.Id = 0;
            workOrderMaterial.CreatedOn = DateTime.Now;
            workOrderMaterial.ModifiedOn = null;
            workOrderMaterial.ModifiedBy = null;

            try
            {
                _workOrderMaterialRepository.Insert(workOrderMaterial);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }

            workOrderMaterialPM.Id = workOrderMaterial.Id;
            return workOrderMaterialPM;
        }

        private async Task<WorkOrder> InsertWorkOrder(WorkOrderPM workOrderPM)
        {
            var workOrder = WorkOrderMapper.FromPresentationModel(workOrderPM);

           
            if (workOrder.RouteId == null || workOrder.RouteId == 0)
            {
                var product = ApiGetProduct(workOrder.ProductId);
                if (product != null)
                {
                    workOrder.RouteId = ApiGetRouteforProduct(workOrder.ProductId, product.PartFamily);
                }
            }

            workOrder.CreatedOn = DateTime.Now;
            if (string.IsNullOrEmpty(workOrder.ModifiedBy) || workOrder.ModifiedBy.Trim() == "0") workOrder.ModifiedBy = null;

            await _workOrderRepository.InsertAsync(workOrder);

            _countWOs++;

            ApiConfirmRouteForWorkOrder(workOrder, workOrder.RouteId ?? 0, true);

            await CopyMaterialAndDrawing(workOrder);

            foreach (var childWO in workOrderPM.ChildWorkOrders)
            {
                childWO.ParentWorkOrderId = workOrder.Id;
                childWO.LocationId = workOrder.LocationId;
                await InsertWorkOrder(childWO);
            }

            //UpdateSalesOrderLineStatus(ESalesOrderLineStatus.WorkOrderIssued);
            return workOrder;
        }

        private async Task DeleteWorkOrder(WorkOrder workOrder)
        {

            var n = workOrder.WorkOrderMaterials.Count(); 

            foreach (var workOrderDetail in workOrder.WorkOrderDetails)
            {                
                await _workOrderDetailRepository.DeleteAsync(workOrderDetail);
            }

            foreach (var workOrderMaterial in workOrder.WorkOrderMaterials)
            {
                await _workOrderMaterialRepository.DeleteAsync(workOrderMaterial);
            }

            await _workOrderRepository.DeleteAsync(workOrder);

            if (n > 0) ApiDeallocateMaterials(workOrder.Id);
     
            var childWOs = await _workOrderRepository.GetWorkOrders(workOrder.Id, true);

            foreach (var childWO in childWOs)
            {
                await DeleteWorkOrder(childWO);
            }

            
        }

        private bool CheckSalesOrderLineStatus(SalesOrderLinePM salesOrderLine)
        {
             
            var qty = _exceptionManager.Process(() => _workOrderDetailRepository.GetQuery(wo => wo.SalesOrderLineId == salesOrderLine.Id).Sum(s => s.Quantity), "ExceptionShielding");

            if (qty >= salesOrderLine.Quantity)
            {
                if (salesOrderLine.Status == ESalesOrderLineStatus.Pending)
                {
                    salesOrderLine.Status = ESalesOrderLineStatus.WorkOrderIssued;
                }
                ApiUpdateSalesOrderLineStatus(salesOrderLine);
                //MPEL.Logger.Write("Issue WO Duolicate event from UI: " + salesOrderLineId.ToString() + "@" + DateTime.Now.ToString(), "AppLog");
                return false;
            }
             

            return true;
        }

        private string GetNextWorkOrderNumber(int offset=0)
        {
          

            Console.WriteLine("Before Next Work Order :" + DateTime.Now.ToString());

            var yearOption = ApiGetOptionSettingByName("WO_Format");

            string yearMonth = DateTime.Today.Year.ToString("D4") + DateTime.Today.Month.ToString("D2");

            if (yearOption != null && yearOption == "YYMMDD")
            {
                yearMonth = DateTime.Today.Year.ToString("D4").Substring(2, 2) + DateTime.Today.Month.ToString("D2") + DateTime.Today.Day.ToString("D2"); ;
            }

            var workOrder = _exceptionManager.Process(() => _workOrderRepository.GetQuery(wo => wo.WorkOrderNumber.Substring(0, 6) == yearMonth && wo.ParentWorkOrderId == null).OrderByDescending(wo => wo.Id).FirstOrDefault(), "ExceptionShielding");

            int max = 0;
            if (workOrder != null)
            {
                max = int.Parse(workOrder.WorkOrderNumber.Substring(6));
            }

            var woStartNo = ApiGetOptionSettingByName("WO_StartNo");
            if (woStartNo != null && woStartNo!="" )
            {
                var startPos = int.Parse(woStartNo);
                if (startPos > max) max = startPos;
            }

            var woLength = ApiGetOptionSettingByName("WO_Length");

            string woFormat = "D3";

            if (woLength != "") woFormat = woLength;

            Console.WriteLine("Next Work Order:" + string.Concat(DateTime.Today.Year.ToString("D4"), DateTime.Today.Month.ToString("D2"), (max + 1 + offset).ToString(woFormat)) + ":" + DateTime.Now.ToString());
            return string.Concat(yearMonth, (max + 1 + offset).ToString(woFormat));
        }

        private async Task<WorkOrderPM> GenerateWorkOrders(SalesOrderLinePM soLine, int customerId, double quantity, string requestedBy = "")
        {
       
            if (quantity == 0) quantity = soLine.BalanceQuantity??0;
            if (quantity == 0) return null;

            var workOrderNumber = GetNextWorkOrderNumber(0);
            if (workOrderNumber == "") return null;

            var product = ApiGetProduct(soLine.ProductId);
            if (product == null) return null;

            var isAssembly = (product.Category == "ProductHierarchyLevelR");

            var workOrder = new WorkOrderPM { Status = EWorkOrderStatus.Pending, IssueDate = DateTime.Today, OrderType = EWorkOrderType.Independent, UrgentFlag = EWorkOrderUrgentFlag.Standard };
             

            workOrder.WorkOrderNumber = workOrderNumber;
            workOrder.CustomerId = customerId;
            workOrder.CreatedBy = requestedBy != "" ? requestedBy : soLine.CreatedBy;
            workOrder.ProductId = soLine.ProductId;
            workOrder.RouteId = ApiGetRouteforProduct(soLine.ProductId, product.PartFamily);
            workOrder.UrgentFlag = soLine.UrgentFlag;
            workOrder.Priority = (short)(soLine.UrgentFlag == EWorkOrderUrgentFlag.Standard ? 50 : (soLine.UrgentFlag == EWorkOrderUrgentFlag.Urgent ? 88 : 99));
            workOrder.DueDate = (soLine.DueDate ?? DateTime.Now).AddDays(-_defaultInternalTurnaroundTime);
            //workOrder.LocationId = currentWorkOrder.LocationId;                      

           
            if (_copySORemarks1 && soLine.Remarks1 != null && soLine.Remarks1.Length < 250)
            {
                workOrder.Remarks1 = soLine.Remarks1;
            }        

            var workOrderLine = new WorkOrderLinePM
            {
                ItemId = product.Id,
                SalesOrderLineId = soLine.Id,
                Quantity = quantity
            };
            workOrder.WorkOrderLines.Add(workOrderLine);

            var productionYield = product.ProductionYield;
            if (productionYield != null && productionYield > 0 && productionYield < 100)
            {
                workOrderLine.Quantity = Math.Round(workOrderLine.Quantity * 100 / (double)(productionYield));
            }          

            if (product.LotSize > 0 && product.LotSize < workOrderLine.Quantity)
            {
                _remainingQty = workOrderLine.Quantity - product.LotSize;
                _lotSize = product.LotSize;
                workOrderLine.Quantity = product.LotSize;
               
            }

            workOrder.Quantity = workOrderLine.Quantity;


            if (isAssembly)          
            {
                workOrder.OrderType = EWorkOrderType.Assembly;
                await ProcessChildWorkOrder(workOrder);
            }

            return workOrder;

        }

        private async Task ProcessChildWorkOrder(WorkOrderPM currentWorkOrder)
        {          
            var parts=ApiGetProductbyAssembly(currentWorkOrder.ProductId);
                                                   
            int x = 1;                     
            foreach (var part in parts)
            {
                if (part.BuyFlag || part.NoActionFlag)
                {
                    if (part.BuyFlag)
                    {
                        if (string.IsNullOrEmpty(currentWorkOrder.BuyList))
                            currentWorkOrder.BuyList = part.Name;
                        else
                            currentWorkOrder.BuyList += " / " + part.Name;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(currentWorkOrder.GatePassNo))
                            currentWorkOrder.GatePassNo = part.Name;
                        else
                            currentWorkOrder.GatePassNo += " / " + part.Name;
                    }

                    continue;
                }

                var workOrder = new WorkOrderPM
                {
                    CustomerId = currentWorkOrder.CustomerId,
                    DueDate = currentWorkOrder.DueDate,
                    IssueDate = currentWorkOrder.IssueDate,
                    LocationId = currentWorkOrder.LocationId,
                    OrderType = EWorkOrderType.Dependent,
                    ProductId = part.Id,
                    Quantity = currentWorkOrder.Quantity * ((part.PerAssemblyQuantity??0) > 0 ? (part.PerAssemblyQuantity??0) : 1),
                    Remarks1 = currentWorkOrder.Remarks1,
                    Status = EWorkOrderStatus.Pending,
                    UrgentFlag = currentWorkOrder.UrgentFlag,
                    Priority = currentWorkOrder.Priority,                        
                    WorkOrderNumber = string.Concat(currentWorkOrder.WorkOrderNumber, ".", x.ToString("D2")),
                    CreatedBy = currentWorkOrder.CreatedBy,
                    Sequence = part.Sequence,

                };

                if (part.ProductionYield != null && part.ProductionYield > 0 && part.ProductionYield < 100)
                {
                    workOrder.Quantity = Math.Round(workOrder.Quantity * 100 / (double)(part.ProductionYield));
                }

                workOrder.KitTypeId = currentWorkOrder.ProductId;
                workOrder.RouteId = ApiGetRouteforProduct(part.Id,part.PartFamily);

                if (workOrder.DueDate > workOrder.IssueDate.AddDays(-_defaultAssemblyleadTime))
                    workOrder.DueDate = workOrder.DueDate.AddDays(-_defaultAssemblyleadTime);

                currentWorkOrder.OrderType = EWorkOrderType.Assembly;
                currentWorkOrder.ChildWorkOrders.Add(workOrder);
                              
                var level = workOrder.WorkOrderNumber.Count(w => w == '.') + 1;
                if (workOrder.WorkOrderNumber.Length < _defaultWOLen && level < _defaultLevel)
                {
                    await ProcessChildWorkOrder(workOrder);
                }
                        
                x++;           
            }
        }

        private async Task CopyMaterialAndDrawing(WorkOrder workOrder)
        {
           
            int n = 3;

            //Option materialDecimal = _exceptionManager.Process(() => _settingRepository.GetQuery<Option>(op => op.OptionName.Trim().Equals("MaterialDecimalNumber")).SingleOrDefault(), "ExceptionShielding");
            //if (materialDecimal != null && !string.IsNullOrWhiteSpace(materialDecimal.DefaultSetting))
            //{
            //    n = int.Parse(materialDecimal.DefaultSetting);
            //}

            var boms = ApiGetRawMaterialbyProduct(workOrder.ProductId);
            var changed = false;
            if (boms.Count() > 0)
            {
                workOrder.WorkOrderMaterials = new List<WorkOrderMaterial>();
                foreach (var bom in boms)
                {
                    var uom = bom.BomLevel;
                
                    decimal? matlQty = Math.Round((decimal)(workOrder.Quantity * bom.PerAssemblyQuantity), n);
                    var matlQty1 = System.Math.Ceiling(matlQty ?? 0);
                    var quantity = uom == 0 ? matlQty : (uom == 1 ? (decimal)bom.PerAssemblyQuantity : matlQty1);
                    
                    var woMaterial = new WorkOrderMaterial() { WorkOrderId = workOrder.Id,  MaterialId = bom.ComponentId, Ratio = bom.PerAssemblyQuantity, UnitMeasureId = bom.BomLevel, Status = bom.UomCode, Remark = bom.Remarks, Quantity = (double?)quantity, Availability=false };                                  
                    workOrder.WorkOrderMaterials.Add(woMaterial);
                    changed = true;
                }
            }

            //copy drawing
            var workOrder1 = _exceptionManager.Process(() => _workOrderRepository.GetQuery(wo => wo.ProductId == workOrder.ProductId && wo.Id != workOrder.Id).OrderByDescending(wo => wo.CreatedOn).FirstOrDefault(), "ExceptionShielding");
            if (workOrder1 != null && (workOrder.Int4 != workOrder1.Int4 || workOrder.Int5 != workOrder1.Int5 || workOrder.Int6 != workOrder1.Int6))
            {
                workOrder.Int4 = workOrder1.Int4;
                workOrder.Int5 = workOrder1.Int5;
                workOrder.Int6 = workOrder1.Int6;
                changed = true;
            }
            if (changed)
            {
                await _workOrderRepository.UpdateAsync(workOrder);

                var woMaterials = new List<IdNamePM>();
                foreach (var woMaterial in workOrder.WorkOrderMaterials)
                {
                    woMaterials.Add(new IdNamePM() { Id = woMaterial.Id, Int1 = woMaterial.WorkOrderId, Int2 = woMaterial.MaterialId, Float1 = woMaterial.Quantity });
                }
                if (woMaterials.Count > 0)
                {
                   var woMaterialIds = ApiAllocateMaterials(woMaterials);
                   if (woMaterialIds.Count >0) 
                        await UpdateWorkOrderMaterialAllocations(woMaterialIds);

                }
            }
                
        }

        private async Task GenerateWObyLotSize(int workOrderId, int lotSize, double quantity)
        {

            var settingLotSize = ApiGetOptionSettingByName("WorkOrderByLotSize");

            if (settingLotSize != "T") return;

            var workOrder = _workOrderRepository.GetById(workOrderId);

            while (quantity > 0 && workOrder!=null)
            {
                var workOrderNumber = GetNextWorkOrderNumber();
                var qty = quantity > lotSize ? lotSize : quantity;
                await CopyWorkOrderTree(workOrder, workOrderNumber, qty);
                _countWOs++;
                quantity -= qty;
            }
        }


        private async Task CopyWorkOrderTree(WorkOrder workOrder, string workOrderNumber, double quanity, int? parentWorkOrderId=null)
        {         
            var newWorkOrderId =await CopyWorkOrder(workOrder, workOrderNumber, quanity, parentWorkOrderId);

            //var childWorkOrders = await _workOrderRepository.GetQuery(x => x.ParentWorkOrderId == workOrder.Id).ToListAsync();

            var childWorkOrders = await _workOrderRepository.GetWorkOrders(workOrder.Id, true);

            foreach (var cwo in childWorkOrders)
            {
                var newChildWorkOrderNumber = cwo.WorkOrderNumber.Replace( workOrder.WorkOrderNumber, workOrderNumber);

                var qtyPer = (cwo.Quantity * quanity) / workOrder.Quantity;

                await CopyWorkOrderTree(cwo, newChildWorkOrderNumber, qtyPer, newWorkOrderId);
             
            }

        }


        private async Task<int> CopyWorkOrder(WorkOrder workOrder, string workOrderNumber, double quanity, int? parentWorkOrderId = null)
        {
            double woQuantity = workOrder.Quantity;

            if (woQuantity <= 0 || quanity <= 0) return 0;

            var ratio = quanity / woQuantity;

            var newWorkOrder = new WorkOrder()
            {
                WorkOrderNumber = workOrderNumber,
                ProductId = workOrder.ProductId,
                Quantity = quanity,
                OrderType = workOrder.OrderType,
                IssueDate = workOrder.IssueDate,
                DueDate = workOrder.DueDate,
                CompletedDate = workOrder.CompletedDate,
                Priority = workOrder.Priority,
                ParentWorkOrderId = parentWorkOrderId,
                Status = workOrder.Status,
                LocationId = workOrder.LocationId,
                CustomerId = workOrder.CustomerId,
                RouteId = workOrder.RouteId,
                Remarks = workOrder.Remarks,
                String1 = workOrder.String1,
                String2 = workOrder.String2,
                String3 = workOrder.String3,
                String4 = workOrder.String4,
                String5 = workOrder.String5,
                String6 = workOrder.String6,
                String7 = workOrder.String7,
                String8 = workOrder.String8,
                MaxString1 = workOrder.MaxString1,
                MaxString2 = workOrder.MaxString2,
                Int1 = workOrder.Int1,
                Int2 = workOrder.Int2,
                Int3 = workOrder.Int3,
                Int4 = workOrder.Int4,
                Int5 = workOrder.Int5,
                Int6 = workOrder.Int6,
                Float1 = workOrder.Float1,
                Float2 = workOrder.Float2,
                Date1 = workOrder.Date1,
                Date2 = workOrder.Date2,
                CreatedBy = workOrder.CreatedBy
            };

            foreach (var wod in workOrder.WorkOrderDetails)
            {
                newWorkOrder.WorkOrderDetails = new List<WorkOrderDetail>();
                newWorkOrder.WorkOrderDetails.Add(new WorkOrderDetail()
                {
                    SalesOrderLineId = wod.SalesOrderLineId,
                    ItemId = wod.ItemId,
                    Quantity = quanity,
                    MaxString1 = wod.MaxString1,
                    MaxString2 = wod.MaxString2,
                    Int1 = wod.Int1,
                    Int2 = wod.Int2,
                    Int3 = wod.Int3,
                    Int4 = wod.Int4,
                    Int5 = wod.Int5,
                    Float1 = wod.Float1,
                    Float2 = wod.Float2,
                    CreatedBy = wod.CreatedBy
                });
            }

            foreach (var wom in workOrder.WorkOrderMaterials)
            {
                newWorkOrder.WorkOrderMaterials = new List<WorkOrderMaterial>();
                newWorkOrder.WorkOrderMaterials.Add(new WorkOrderMaterial()
                {
                    MaterialId = wom.MaterialId,
                    Quantity = wom.Quantity * ratio,
                    UnitMeasureId = wom.UnitMeasureId,
                    Remark = wom.Remark,
                    Status = wom.Status,
                    Availability = wom.Availability,
                    CreatedBy = wom.CreatedBy,
                    Ratio = wom.Ratio
                });
            }

            await _workOrderRepository.InsertAsync(newWorkOrder);
            return newWorkOrder.Id;
        }

        private void AssignAdditionalInformaitonForWorkOrder(WorkOrderPM workOrderPm)
        {
            if (workOrderPm.Status >= EWorkOrderStatus.Completed)
            {             
                workOrderPm.CompletedQuantity = ApiCompletedQuantity(workOrderPm.Id);
                if (workOrderPm.CompletedQuantity == 0) workOrderPm.CompletedQuantity = workOrderPm.Quantity;
            }

           
            var product = _items.FirstOrDefault(x=>x.Id == workOrderPm.ProductId);
            if (product==null)
            {
                var p1 = ApiGetProduct(workOrderPm.ProductId);
                if (p1 != null)
                {
                    product = new IdNamePM() { Id = p1.Id, Name = p1.Name, Code = p1.Revision, Description = p1.Description, Float1 = (double?)p1.ProductionYield, String1 = p1.PartFamily, Category = p1.Category, Value = p1.LotSize.ToString() };
                    _items.Add(product);
                }
            }
            if (product != null)
            {
                workOrderPm.ProductName = product.Description;
                workOrderPm.ProductNo = product.Name;
                //workOrderPm.ProductionYield = (decimal?)product.ProductionYield;
                //workOrderPm.ProductFamily = product.PartFamily;
                workOrderPm.ProductionYield = (decimal?)product.Float1;
                workOrderPm.ProductFamily = product.String1;

            }
         
            if (workOrderPm.ParentWorkOrderId!=null && workOrderPm.ParentWorkOrderId >0)
            {
                var parentWO=_workOrderRepository.GetById(workOrderPm.ParentWorkOrderId??0);
                if (parentWO!=null && parentWO.ProductId>0 )
                {
                    //var assembly = ApiGetProduct(parentWO.ProductId);
                    var assembly = _items.FirstOrDefault(x => x.Id == parentWO.ProductId);
                    if (assembly ==null)
                    {                       
                        var p1 = ApiGetProduct(parentWO.ProductId);
                        if (p1 != null)
                        {
                            assembly = new IdNamePM() { Id = p1.Id, Name = p1.Name, Code = p1.Revision, Description = p1.Description, Float1 = (double?)p1.ProductionYield, String1 = p1.PartFamily, Category = p1.Category, Value = p1.LotSize.ToString() };
                            _items.Add(assembly);
                        }
                        
                    }
                    if (assembly != null)
                    {
                        workOrderPm.AssemblyNo = assembly.Name;
                        workOrderPm.AssemblyName = assembly.Description;
                    }
                        
                }
            }

            workOrderPm.NoofChildWOs=_workOrderRepository.GetQuery(x => x.ParentWorkOrderId != null && x.ParentWorkOrderId == workOrderPm.Id).Count();

            if (workOrderPm.RouteId != null)
            {

                //var route = ApiGetRoute(workOrderPm.RouteId??0);

                BasePM route = null;
                try
                {
                     route = _routings.FirstOrDefault(x => x.Id == (workOrderPm.RouteId ?? 0));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in getting routing for work order id / routeId / count:" + workOrderPm.Id.ToString() +"/"+ workOrderPm.RouteId.ToString ()+"/"+_routings.Count ().ToString ());
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
               
                if (route == null && (workOrderPm.RouteId ?? 0) > 0)
                {
                    var r1 = ApiGetRoute(workOrderPm.RouteId ?? 0);
                    _routings.Add(r1);
                    route = r1;
                }

                if (route != null )
                {
                   workOrderPm.RouteName = route.Name;
                   //workOrderPm.IsRouteConfirmed = (route.Version > 1 && route.IsActive ? true : false);
                   workOrderPm.IsRouteConfirmed = true;
                }

            }

            if (workOrderPm.LocationId!=null)
            {
                //workOrderPm.Location = ApiGetLocation(workOrderPm.LocationId??0);           
                workOrderPm.Location = _locations.FirstOrDefault(x => x.Id == (workOrderPm.LocationId ?? 0));
            }

            if (workOrderPm.CustomerId != null)
            {
                //workOrderPm.Customer = ApiGetCustomer(workOrderPm.CustomerId??0);
                workOrderPm.Customer = _customers.FirstOrDefault(x => x.Id == (workOrderPm.CustomerId ?? 0));
            }

            if (workOrderPm.LinkWorkOrderId != null)
            {
                WorkOrder linkWorkOrder = _exceptionManager.Process(() => _workOrderRepository.GetById(workOrderPm.LinkWorkOrderId??0), "ExceptionShielding");
                if (linkWorkOrder != null) workOrderPm.LinkWorkOrderNumber = linkWorkOrder.WorkOrderNumber;
            }

            //var displayName =ApiGetDisplayName(workOrderPm.CreatedBy);
            var displayName = workOrderPm.CreatedBy;
            var a =_displayNames.FirstOrDefault(x => x.Code == workOrderPm.CreatedBy);
            if (a != null) displayName = a.Name;
            if (!string.IsNullOrWhiteSpace(displayName)) workOrderPm.CreatedBy = displayName;


            if (!string.IsNullOrWhiteSpace(workOrderPm.ReleasedBy))
            {
                //displayName = ApiGetDisplayName(workOrderPm.ReleasedBy);
                displayName = workOrderPm.ReleasedBy;
                a = _displayNames.FirstOrDefault(x => x.Code == workOrderPm.ReleasedBy);
                if (a != null) displayName = a.Name;
                if (!string.IsNullOrWhiteSpace(displayName)) workOrderPm.ReleasedBy = displayName;              
            }


            foreach (WorkOrderLinePM workOrderLinePM in workOrderPm.WorkOrderLines)
            {
                WorkOrderLinePM pm = workOrderLinePM;
                if (pm.SalesOrderLineId != null)
                {
                    //var sol = ApiGetSalesOrderLine(pm.SalesOrderLineId??0);
                    var so = _salesOrder.FirstOrDefault(x => x.SalesOrderLines.Any(y => y.Id == (pm.SalesOrderLineId ?? 0)));

                    if (so==null && (pm.SalesOrderLineId ?? 0)>0)
                    {
                        var so1 = ApiGetSalesOrders((pm.SalesOrderLineId ?? 0).ToString ());
                        if (so1!=null && so1.Count >0)
                        {
                            so = so1.FirstOrDefault();
                            _salesOrder.Add(so);
                        }
                    }

                    SalesOrderLinePM sol = null;
                    if ( so != null)
                    {
                         sol = so.SalesOrderLines.FirstOrDefault(y => y.Id == (pm.SalesOrderLineId ?? 0));
                    }
                   
                    if (so!=null && sol !=null )
                    {
                        workOrderLinePM.SalesOrderNumber = so.SalesOrderNumber;
                        workOrderLinePM.SalesOrderLineNumber = sol.LineNumber ?? 0;

                        workOrderPm.PONumbers = string.IsNullOrWhiteSpace(workOrderPm.PONumbers) ? so.SalesOrderNumber : workOrderPm.PONumbers + "; " + so.SalesOrderNumber;
                        workOrderPm.ContactPersonName = so.ContactPersonName;
                        workOrderPm.ContactNo = so.ContactNo;
                        workOrderPm.LineNo = sol.LineNumber ?? 0;
                        workOrderPm.PORemarks = sol.Remarks;
                        workOrderPm.ServerFileName = sol.SeverFileName;
                        workOrderPm.FileName = sol.ThumbnailImageFileName;
                        workOrderPm.Revision = sol.Revision;                     
                        workOrderPm.CommittedDeliveryDate = ((DateTime)sol.DueDate).ToString("dd/MM/yyyy");

                        workOrderPm.address2 = so.Comment;

                        if (sol.UrgentFlag == EWorkOrderUrgentFlag.Urgent)
                        {
                            workOrderPm.SOPriority = "Urgent";
                        }
                        else if (sol.UrgentFlag == EWorkOrderUrgentFlag.VeryUrgent)
                        {
                            workOrderPm.SOPriority = "Very Urgent";
                        }
                        else
                        {
                            workOrderPm.SOPriority = "Standard";
                        }
                    }  
                }

                if (pm.ItemId == null) continue;
                
                if (product!=null && pm.ItemId==product.Id)
                {
                    pm.ProductName = product.Name;
                }
                else
                {
                    //var item = ApiGetProduct(pm.ItemId??0);
                    var item = _items.FirstOrDefault(x => x.Id == (pm.ItemId??0));
                    if (item != null) pm.ProductName = item.Name;
                }             
            }

            workOrderPm.MaterialList = "";
            workOrderPm.IsMaterialAllocated = true;

            foreach (WorkOrderMaterialPM workOrderMaterialPM in workOrderPm.WorkOrderMaterials)
            {

                
                //var material = ApiGetProduct(workOrderMaterialPM.MaterialId);
                var material = _items.FirstOrDefault(x => x.Id == workOrderMaterialPM.MaterialId);

                if (material == null)
                {
                    var p1 = ApiGetProduct(workOrderPm.ProductId);
                    if (p1 != null)
                    {
                        material = new IdNamePM() { Id = p1.Id, Name = p1.Name, Code = p1.Revision, Description = p1.Description, Float1 = (double?)p1.ProductionYield, String1 = p1.PartFamily, Category = p1.Category, Value = p1.LotSize.ToString() };
                        _items.Add(material);
                    }
                }

                if (material != null)
                {
                    if (!(workOrderMaterialPM.Availability ?? false))
                        workOrderPm.IsMaterialAllocated = false;

                    if (workOrderPm.MaterialList == "")
                        workOrderPm.MaterialList = material.Name;
                    else
                        workOrderPm.MaterialList = workOrderPm.MaterialList + ";" + material.Name;
                }
            }

            if (workOrderPm.MaterialList == "") workOrderPm.IsMaterialAllocated = null;

            if (workOrderPm.PictureA != null && workOrderPm.PictureA > 0)
            {
                var picture = ApiGetPicture(workOrderPm.PictureA??0);
                workOrderPm.ThumbnailImageA = picture.ThumbNailImage;
            }

            if (workOrderPm.PictureB != null & workOrderPm.PictureB > 0)
            {
                var picture = ApiGetPicture(workOrderPm.PictureB ?? 0);
                workOrderPm.ThumbnailImageB = picture.ThumbNailImage;
            }

            if (workOrderPm.PictureC != null && workOrderPm.PictureC > 0)
            {
                var picture = ApiGetPicture(workOrderPm.PictureC ?? 0);
                workOrderPm.ThumbnailImageC = picture.ThumbNailImage;
            }
        }

        

        private void AssignPOnumberForChildWorkOrders(IEnumerable<WorkOrderPM> workOrderPms)
        {
            foreach (WorkOrderPM workOrderPm in workOrderPms.OrderBy(x => x.WorkOrderNumber))
            {
                if (workOrderPm.WorkOrderLines == null || workOrderPm.WorkOrderLines.Count() == 0)
                {
                    var parentWO = workOrderPms.FirstOrDefault(wo => wo.Id == workOrderPm.ParentWorkOrderId);

                    if (parentWO != null)
                    {
                        workOrderPm.PONumbers = parentWO.PONumbers;
                        workOrderPm.ContactPersonName = parentWO.ContactPersonName;
                        workOrderPm.ContactNo = parentWO.ContactNo;
                        workOrderPm.LineNo = parentWO.LineNo;
                        workOrderPm.PORemarks = parentWO.PORemarks;
                        workOrderPm.ServerFileName = parentWO.ServerFileName;
                        workOrderPm.FileName = parentWO.FileName;
                        workOrderPm.CommittedDeliveryDate = parentWO.CommittedDeliveryDate;
                    }
                    else
                    {
                        //Parent WO isn't in this page, need to retrieve it from database
                        var parentWorkOrderId = workOrderPm.ParentWorkOrderId;
                        WorkOrder wo = null;
                        while (parentWorkOrderId > 0)
                        {
                            //wo = _exceptionManager.Process(() => _workOrderRepository.GetById(parentWorkOrderId??0), "ExceptionShielding");
                            wo =   (_workOrderRepository.GetWorkOrders(parentWorkOrderId ?? 0)).Result.FirstOrDefault();
                            if (wo != null)
                                parentWorkOrderId = wo.ParentWorkOrderId ?? 0;
                            else
                                parentWorkOrderId = 0;
                        }

                        if (wo != null)
                        {
                            var woPM = WorkOrderMapper.ToPresentationModel(wo);
                            woPM.WorkOrderLines = WorkOrderLineMapper.ToPresentationModels(wo.WorkOrderDetails).ToList();

                            AssignAdditionalInformaitonForWorkOrder(woPM);
                            workOrderPm.PONumbers = woPM.PONumbers;
                            workOrderPm.ContactPersonName = woPM.ContactPersonName;
                            workOrderPm.ContactNo = woPM.ContactNo;
                            workOrderPm.LineNo = woPM.LineNo;
                            workOrderPm.PORemarks = woPM.PORemarks;
                            workOrderPm.ServerFileName = woPM.ServerFileName;
                            workOrderPm.FileName = woPM.FileName;

                            workOrderPm.CommittedDeliveryDate = woPM.CommittedDeliveryDate;
                        }
                    }
                }
            }
        }
       
        private void UpdateSalesOrderStatus( int soDetId, byte statusCode)
        {
           
            if (soDetId <= 0) return;

            var sol = ApiGetSalesOrderLine(soDetId);

            //var statusCode = ApiGetStatusCode(status);

            if (sol != null)
            {
                switch (statusCode)
                {
                    case 100:
                        sol.Status = ESalesOrderLineStatus.Scheduled;
                        break;
                    case 110:
                        sol.Status = ESalesOrderLineStatus.Started;
                        break;
                    case 120:
                        sol.Status = ESalesOrderLineStatus.Started;
                        break;
                    case 230:
                        sol.Status = ESalesOrderLineStatus.Completed;
                        break;
                    case 255:
                        sol.Status = ESalesOrderLineStatus.Closed;
                        break;
                }

                ApiUpdateSalesOrderLineStatus(sol);
            }

        }

        private void GetAllAddtitionalDataFromOtherServices(List<WorkOrder> workOrders)
        {
            var itemIds = workOrders.Select(x => x.ProductId).Distinct().ToList();
            var materialIds = workOrders.SelectMany(x => x.WorkOrderMaterials.Select(y=>y.MaterialId)).Distinct().ToList();
            var customerIds = workOrders.Select(x => x.CustomerId).Distinct().ToList();
            var routeIds = workOrders.Select(x => x.RouteId ?? 0).Distinct().ToList();
            var locationIds = workOrders.Select(x => x.LocationId ?? 0).Distinct().ToList();
            var createdBys= workOrders.Select(x => x.CreatedBy).Distinct().ToList();
            var releasedBys = workOrders.Select(x => x.String4).Distinct().ToList(); //released by

            var salesOrderLineIds = workOrders.SelectMany(x => x.WorkOrderDetails.Select(y => y.SalesOrderLineId??0).ToList ()).Distinct().ToList ();

            var st1 = DateTime.Now;          
            _items = ApiGetProducts(string.Join(",", itemIds.Concat(materialIds)));
            Console.WriteLine("Duration for getting product info: " + (DateTime.Now - st1).TotalSeconds.ToString()+",parts:"+_items.Count().ToString ());

            st1 = DateTime.Now;
            _customers = ApiGetCustomers(string.Join(",", customerIds));
            Console.WriteLine("Duration for getting customer info: " + (DateTime.Now - st1).TotalSeconds.ToString()+",customers:" + _customers.Count().ToString());

            st1 = DateTime.Now;
            _routings = ApiGetRoutes(string.Join(",", routeIds));
            Console.WriteLine("Duration for getting route info: " + (DateTime.Now - st1).TotalSeconds.ToString()+ ",routes:" + _routings.Count().ToString());

            st1 = DateTime.Now;
            _salesOrder = ApiGetSalesOrders(string.Join(",", salesOrderLineIds));
            Console.WriteLine("Duration for getting sales order info: " + (DateTime.Now - st1).TotalSeconds.ToString()+ ",sales order:" + _salesOrder.Count().ToString());

            st1 = DateTime.Now;          
            _locations=ApiGetLocations(string.Join(",", locationIds));
            Console.WriteLine("Duration for getting location info " + (DateTime.Now - st1).TotalSeconds.ToString()+ ",locations:" + _locations.Count().ToString());

            st1 = DateTime.Now;
            foreach (var createdBy in createdBys.Concat(releasedBys).Distinct())
            {
                var displayName = ApiGetDisplayName(createdBy);
                _displayNames.Add(new BasePM() { Code = createdBy, Name = displayName });              
            }
            Console.WriteLine("Duration for getting createdby/releasedby user info " + (DateTime.Now - st1).TotalSeconds.ToString()+ ",createdby/releasedby:" + _displayNames.Count().ToString());
        }

        private async Task<List<WorkOrderPM>> GetWorkOrdersByCategoryWithPage(int category, int pageNo=0, int pageSize =0)
        {
            var st = DateTime.Now;
            Console.WriteLine("GetWorkOrder started at " + st.ToString());
            var userList = new List<string>();
            //int role = GetUserRole(userList);
            int role = 0;

            var st1 = DateTime.Now;
            var sortBy = ApiGetOptionSettingByName("WO_SortOrderByDueDate");
            var sortOrder = ApiGetOptionSettingByName("WO_SortOrder");
            var updateStatus = ApiGetOptionSettingByName("UpdateWorkOrderStatus");
            Console.WriteLine("Duration for Setting " + (DateTime.Now - st1).TotalSeconds.ToString());



            st1 = DateTime.Now;
            if (updateStatus == "T" && category != (int)EWorkOrderCategory.Completed)
            {
                await _workOrderRepository.UpdateWorkOrderStatus();
                Console.WriteLine("Duration for updating work order status " + (DateTime.Now - st1).TotalSeconds.ToString());
            }

            st1 = DateTime.Now;
            var workOrders = await _workOrderRepository.GetWorkOrdersByCategory((EWorkOrderCategory)category, sortBy == "T", sortOrder == "D", role, userList,pageNo,pageSize);
            Console.WriteLine("Duration for retrieving work orders from DB " + (DateTime.Now - st1).TotalSeconds.ToString());

            var workOrderPMs = WorkOrderMapper.ToPresentationModels(workOrders).ToList();

            st1 = DateTime.Now;
            GetAllAddtitionalDataFromOtherServices(workOrders.ToList());
            Console.WriteLine("Duration for getting all additional data using APIs " + (DateTime.Now - st1).TotalSeconds.ToString());

            st1 = DateTime.Now;
            foreach (var workOrderPm in workOrderPMs)
            {
                AssignAdditionalInformaitonForWorkOrder(workOrderPm);
            }

            AssignPOnumberForChildWorkOrders(workOrderPMs);
            Console.WriteLine("Duration for assigning additional info " + (DateTime.Now - st1).TotalSeconds.ToString());


            var et = DateTime.Now;
            Console.WriteLine("GetWorkOrder ended at " + et.ToString() + " Duration(seconds):" + (et - st).TotalSeconds.ToString());

            return workOrderPMs;
        }



        #endregion

        #region  API call of other services


        private  string ApiGetOptionSettingByName(string optionName)
        {
            Option option=null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SETTING_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    option = HttpHelper.Get<Option>(apiBaseUrl, $"Name/{optionName}");
                }
                catch { }
            }
           
            return option == null ? "" : option.DefaultSetting;
        }

        private ItemPM ApiGetProduct(int productId)
        {
            ItemPM product = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    product = HttpHelper.Get<ItemPM>(apiBaseUrl, $"{productId}");
                }
                catch { }
            }
            
            return product;
        }

        private List<IdNamePM> ApiGetProducts(string itemsId)
        {
            var product = new List<IdNamePM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    product = HttpHelper.Get<List<IdNamePM>>(apiBaseUrl, $"IdName/{itemsId}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in getting assembies/parts:" + $"IdName/{itemsId}");
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return product?? new List<IdNamePM>();
        }

        private List<ItemPM> ApiGetProductbyAssembly(int assemblyId)
        {
            var items = new List<ItemPM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {

                    items = HttpHelper.Get<List<ItemPM>>(apiBaseUrl, $"Component/{assemblyId}");
                }
                catch { }
            }
           
            return items.Where (x=>x.Category!= "RawMaterial").ToList ();
        }

        private List<BillOfMaterialPM> ApiGetRawMaterialbyProduct(int productId)
        {
            var boms = new List<BillOfMaterialPM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCTBOM_URL");


            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {

                    boms = HttpHelper.Get<List<BillOfMaterialPM>>(apiBaseUrl, $"GetBillofMaterialsByProductId/{productId}/M");
                }
                catch { }
            }
          
            return boms;
        }

        private Dictionary<int, RawMaterialPM> ApiGetRawMaterialAsDictionary()
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");
            var dictionary = new Dictionary<int, RawMaterialPM>();

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var rawMaterials = HttpHelper.Get<List<RawMaterialPM>>(apiBaseUrl, "Material");

                    rawMaterials.ForEach(rm =>
                    {
                        dictionary.Add(rm.Id, rm);
                    });
                }
                catch { }
            }

            return dictionary;
        }


        private SalesOrderLinePM ApiGetSalesOrderLine(int soLineId)
        {
            SalesOrderLinePM soLine = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SALESORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl) && soLineId > 0)
            {
                try
                {                   
                    soLine = HttpHelper.Get<SalesOrderLinePM>(apiBaseUrl, $"SODetail/{soLineId}");
                }
                catch { }
            }
                       
            return soLine; 
        }

        private SalesOrderPM ApiGetSalesOrder(int soId)
        {
            SalesOrderPM so = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SALESORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    so = HttpHelper.Get<SalesOrderPM>(apiBaseUrl, $"{soId}");
                }
                catch { }
            }
           
            return so;
        }

        private SalesOrderPM ApiGetSalesOrderByPO(string poNumber)
        {
            SalesOrderPM so = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SALESORDER_URL");

            Console.WriteLine("apiGetSalesOrderByPO:" + poNumber.ToString());

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    so = HttpHelper.Get<SalesOrderPM>(apiBaseUrl, $"PONumber/{poNumber}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in getting sales order:" + $"PONumber/{poNumber}");
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            if (so!=null) Console.WriteLine("apiGetSalesOrderByPO:" + so.Id.ToString ());
            return so;
        }

        private List<SalesOrderPM> ApiGetSalesOrders(string soDetIds)
        {
            var salesOrders = new List<SalesOrderPM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SALESORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    salesOrders = HttpHelper.Get<List<SalesOrderPM>>(apiBaseUrl, $"DetailIds/{soDetIds}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in getting sales order:" + $"DetailIds/{soDetIds}");
                    Console.WriteLine(e.Message);
                    if (e.InnerException!=null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return salesOrders?? new List<SalesOrderPM>();
        }     

        private void ApiUpdateSalesOrderLineStatus(SalesOrderLinePM sol)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SALESORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    HttpHelper.PutAsync<SalesOrderLinePM>(apiBaseUrl, $"DetailId/{sol.Id}", sol);

                    var salesOrder = new SalesOrderStatus() { SalesOrderNumber = sol.SalesOrderNumber, LineNo = sol.LineNumber??0, StatusCode = (int)sol.Status, Status = sol.Status.ToString ()};

                    var result = PublishMessage("SO", null, salesOrder);

                }
                catch { }
            }
        }

        private List<int> ApiAllocateMaterials(List<IdNamePM> woMaterials)
        {
            var woMaterialIds = new List<int>();

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INVENTORY_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var task =HttpHelper.PostAsync<List<IdNamePM>, List<int>>(apiBaseUrl, "MaterialAllocation", woMaterials);
                    task.Wait();
                    if (task.Result != null) woMaterialIds = task.Result;
                   
                    Console.WriteLine("allocated wo material:" + woMaterialIds.Count.ToString() );

                }
                catch (Exception e) 
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message );
                }
            }
            return woMaterialIds;
        }

        private void ApiDeallocateMaterials(int workOrderId)
        {
           
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INVENTORY_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var task = HttpHelper.PostAsync<int, int>(apiBaseUrl, "MaterialDeallocation", workOrderId);
                    task.Wait();
                    if (task.Result >0)
                    {                       
                        Console.WriteLine("deallocated wo materials:" + task.Result);
                    }

                    }
                catch (Exception e)
                {
                    Console.WriteLine("In ApiDeallocateMaterials:" + workOrderId.ToString () );
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }        
        }


        private BasePM ApiGetRoute(int routeId)
        {
            BasePM route = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_ROUTE_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl) && routeId>0)
            {
                try
                {
                    route = HttpHelper.Get<BasePM>(apiBaseUrl, $"{routeId}");
                }
                catch { }
            }
                  
            return route;
        }

        private List<BasePM> ApiGetRoutes(string routeIds)
        {
            var routes = new List<BasePM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_ROUTE_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl) && routeIds !="")
            {
                try
                {
                    routes = HttpHelper.Get<List<BasePM>>(apiBaseUrl, $"IdNames/{routeIds}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in getting routings:" + $"IdNames/{routeIds}");
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return routes ?? new List<BasePM>(); 
        }

        private int ApiGetRouteforProduct(int productId, string productFamily)
        {
            int routeId =0;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_ROUTE_URL");

            Console.WriteLine("ApiGetRouteforProduct:" + apiBaseUrl+ $"Product/{productId}" );

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var routes = HttpHelper.Get<IEnumerable<BasePM>>(apiBaseUrl, $"Product/{productId}");
                    if (routes != null && routes.Count() > 0) routeId = routes.First().Id;

                    if (routeId==0 && !string.IsNullOrWhiteSpace(productFamily))
                    {
                        routes = HttpHelper.Get<IEnumerable<BasePM>>(apiBaseUrl, $"ProductFamily/{productFamily}");
                        Console.WriteLine("ApiGetRouteforProduct:" + apiBaseUrl + $"ProductFamily/{productFamily}");
                        if (routes != null && routes.Count() > 0) routeId = routes.First().Id;
                    }                  
                }
                catch { }              
            }

            Console.WriteLine("ApiGetRouteforProduct/RouteId:" + routeId.ToString() );
            return routeId;
        }

        private void ApiConfirmRouteForWorkOrder(WorkOrder workOrder, int routeId, bool confirmed)
        {

            if (routeId <= 0 || workOrder == null) return;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_ROUTE_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var newRouteId = HttpHelper.Get<int>(apiBaseUrl, $"copy/{routeId}");

                    if (newRouteId==0)
                    {
                        Console.WriteLine("Failed in copying route for WO, original route Id will be used:" + routeId.ToString ());
                        newRouteId = routeId;
                    }

                    if (newRouteId > 0)
                    {
                        HttpHelper.PutAsync<bool>(apiBaseUrl, $"confirm/{newRouteId}", confirmed);
                        workOrder.RouteId = confirmed ? newRouteId : null;
                        _workOrderRepository.Update(workOrder);
                    }
                }
                catch { }
            }
           
        }

        private BasePM ApiGetCustomer(int customerId)
        {
            BasePM customer = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var customer1 = HttpHelper.Get<CustomerPM>(apiBaseUrl, $"{customerId}");
                    if (customer1 != null) customer = new BasePM() { Id = customer1.Id, Name = customer1.Name, Description = customer1.Code };
                }
                catch { }
            }
           
            return customer;
        }

        private List<BasePM> ApiGetCustomers(string customerIds)
        {
            var customers = new List<BasePM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                     customers = HttpHelper.Get<List<BasePM>>(apiBaseUrl, $"IdName/{customerIds}");
 
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in getting customers:" + $"IdName/{customerIds}");
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return customers?? new List<BasePM>(); 
        }

        private BasePM ApiGetLocation(int locationId)
        {
            BasePM location = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_LOCATION_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    location = HttpHelper.Get<BasePM>(apiBaseUrl, $"{locationId}");
                }
                catch { }
            }
           
            return location;
        }

        private List<BasePM> ApiGetLocations(string locationIds)
        {
            var locations = new List<BasePM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_LOCATION_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    locations = HttpHelper.Get<List<BasePM>>(apiBaseUrl, $"IdName/{locationIds}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in getting locations:" + $"DetailIds/{locationIds}");
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return locations?? new List<BasePM>();
        }

        private BasePM ApiGetDefaultUnit()
        {
            BasePM location = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_LOCATION_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                   location = HttpHelper.Get<BasePM>(apiBaseUrl, $"Unit");
                }
                catch { }
            }
           
            return location;
        }


        private async Task ApiIntegrateForUnrelasedWO(int woId)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INTEGRATION_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    await HttpHelper.DeleteAsync(apiBaseUrl, $"UnreleaseWO/{woId}");
                }
                catch { }
                
            }
        }

        private async Task ApiIntegrateForUnrelasedWOs(string woIds)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INTEGRATION_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    await HttpHelper.DeleteAsync(apiBaseUrl, $"UnreleaseWOs/{woIds}");
                }
                catch { }

            }
        }

        private async Task ApiIntegrateForRelasedWO(WorkOrderIntegrationPM wo)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INTEGRATION_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    await HttpHelper.PostAsync<WorkOrderIntegrationPM>(apiBaseUrl, $"ReleaseWO", wo);
                }
                catch { }
               
            }
        }

        private async Task ApiIntegrateForRelasedWOs(List<WorkOrderIntegrationPM> wos)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INTEGRATION_URL");

            var genRoute = ApiGetOptionSettingByName("GeneratePPRoute");
            if (genRoute == "") genRoute = "T";

            Console.WriteLine("GeneratePPRoute:" + genRoute);


            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    await HttpHelper.PostAsync<List<WorkOrderIntegrationPM>>(apiBaseUrl, $"ReleaseWOs/{genRoute}", wos);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{ e.Message}");
                    if (e.InnerException != null) Console.WriteLine($"{ e.InnerException.Message}");
                }

            }
        }


        private double ApiCompletedQuantity(int workOrderId)
        {
            double qty = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INVENTORY_URL");

            //skip first
            //if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            //{
            //    try
            //    {
            //        qty = HttpHelper.Get<double>(apiBaseUrl, $"{workOrderId}");
            //    }
            //    catch { }
            //}

            return qty;
        }

        private string ApiGetDisplayName(string userName)
        {
            string displayName = "";
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_USER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    displayName = HttpHelper.Get<string>(apiBaseUrl, $"{userName}");
                }
                catch { }
            }

            return displayName;
        }

        private PicturePM ApiGetPicture(int pictureId)
        {
            PicturePM picture = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PICTURE_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    picture = HttpHelper.Get<PicturePM>(apiBaseUrl, $"{pictureId}");
                }
                catch { }
            }
           
            return picture;
        }

        private IEnumerable<WIP> ApiGetWIPs()
        {
            IEnumerable<WIP> wips = new List<WIP>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_TRACKING_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    wips = HttpHelper.Get<IEnumerable<WIP>>(apiBaseUrl, $"WIP");
                }
                catch { }
            }

            return wips;
        }

        private int ApiAddInventory(Material material, string category)
        {
            int inventoryId = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INVENTORY_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.PostAsync<Material, Material>(apiBaseUrl, category, material).Result;
                    if (result != null) inventoryId = result.Id;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{ e.Message}");
                    if (e.InnerException != null) Console.WriteLine($"{ e.InnerException.Message}");
                }
            }

            return inventoryId;
        }

        private int ApiGetLinkedMaterial(int partId)
        {
            var materialId =0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.Get<ItemPM>(apiBaseUrl, $"Material/LinkedPart/{partId}");
                    if (result != null) materialId = result.Id;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{ e.Message}");
                    if (e.InnerException != null) Console.WriteLine($"{ e.InnerException.Message}");
                }
            }

            return materialId;
        }

        private int ApiGetLinkedMaterial(string partName)
        {
            var materialId = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.Get<ItemPM>(apiBaseUrl, $"Material/LinkedPartName/{partName}");
                    if (result != null) materialId = result.Id;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{ e.Message}");
                    if (e.InnerException != null) Console.WriteLine($"{ e.InnerException.Message}");
                }
            }

            return materialId;
        }


        private byte ApiGetStatusCode(string status)
        {
            byte statusCode = 0;

            switch (status)
            {
                case "Released":
                    statusCode = (byte)EWorkOrderStatus.Released;
                    break;
                case "Dispatched":
                    statusCode = (byte)EWorkOrderStatus.Dispatched;
                    break;
                case "Queuing":
                    statusCode = (byte)EWorkOrderStatus.Queuing;
                    break;
                case "Ready":
                    statusCode = (byte)EWorkOrderStatus.Queuing;
                    break;
                case "Processing":
                    statusCode = (byte)EWorkOrderStatus.Processing;
                    break;
                case "Scrapped":
                    statusCode = (byte)EWorkOrderStatus.Scrapped;
                    break;
                case "Discard":
                    statusCode = (byte)EWorkOrderStatus.Discard;
                    break;
                case "ReturnUnclean":
                    statusCode = (byte)EWorkOrderStatus.ReturnUnclean;
                    break;
                case "Completed":
                    statusCode = (byte)EWorkOrderStatus.Completed;
                    break;
                case "Charged":
                    statusCode = (byte)EWorkOrderStatus.Charged;
                    break;
                case "SCRGenerated":
                    statusCode = (byte)EWorkOrderStatus.SCRGenerated;
                    break;
                default :
                    statusCode = (byte)EWorkOrderStatus.Pending;
                    break;

            }

            return statusCode;
    }

        //private IEnumerable<CodeDetail> ApiGetCodeListByType(string typeName)
        //{
        //    CodeType codeType = null;
        //    var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CODE_URL");

        //    if (!string.IsNullOrWhiteSpace(apiBaseUrl))
        //    {
        //        try
        //        {
        //            codeType = HttpHelper.Get<CodeType>(apiBaseUrl, $"Name/{typeName}");
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.Message + e.InnerException != null ? e.InnerException.Message : "");
        //        }
        //    }

        //    return codeType == null ? new List<CodeDetail>() : codeType.CodeDetails;
        //}



        #endregion

    }
}