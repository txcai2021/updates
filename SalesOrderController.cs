using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace SIMTech.APS.SalesOrder.API.Controllers
{

    using SIMTech.APS.SalesOrder.API.Repository;
    using SIMTech.APS.SalesOrder.API.Models;
    using SIMTech.APS.SalesOrder.API.PresentationModels;
    using SIMTech.APS.SalesOrder.API.Mappers;
    using SIMTech.APS.PresentationModels;
    using SIMTech.APS.Utilities;
    using SIMTech.APS.Product.API.PresentationModels;
    using SIMTech.APS.SalesOrder.API.Enums;

    [Route("api/[controller]")]
    [ApiController]
    public class SalesOrderController : ControllerBase
    {

        //private readonly ILogger<SalesOrderController> _logger;
        private readonly ISalesOrderRepository _salesOrderRepository;
        private readonly ISalesOrderDetailRepository _salesOrderDetailRepository;
        private readonly ExceptionManager _exceptionManager;


        public SalesOrderController( ISalesOrderRepository salesOrderRepository, ISalesOrderDetailRepository salesOrderDetailRepository)
        {
            //_logger = logger;
            _salesOrderRepository = salesOrderRepository;
            _salesOrderDetailRepository = salesOrderDetailRepository;
            _exceptionManager = new ExceptionManager();
        }

        //GET: api/SalesOrder
        [HttpGet]
        //public async Task<IEnumerable<Customer>> GetAllCustomers() => await _SalesRepository.GetAll();
        public IEnumerable<SalesOrderPM> GetAllSalesOrders()
        {
            //var salesOrders = _salesOrderRepository.GetQuery(x => x.Id > 0).OrderBy(x => x.SalesOrderNumber).ToList();
            //var salesOrders = _salesOrderRepository.GetQuery(x => x.Id > 0).OrderByDescending(x => x.CreatedOn).ToList();

            var salesOrders = _salesOrderRepository.GetSalesOrders();

            var salesOrdersPM= SalesOrderMapper.ToPresentationModels(salesOrders).ToList ();

            var customerIds = salesOrdersPM.Where (x=>x.CustomerId!=null).Select (x=>x.CustomerId).ToList ();

            var customers = ApiGetCustomers(string.Join(",", customerIds));

            foreach (var so in salesOrdersPM)
            {
                //if (so.CustomerId != null) so.CustomerName = ApiGetCustomerName(so.CustomerId??0);
                if  (so.CustomerId != null)
                {
                    var customer = customers.Where(x => x.Id == so.CustomerId).FirstOrDefault();
                    if (customer != null) so.CustomerName = customer.Code;
                }

                //so.SalesOrderLines= GetSalesOrderLines(so.Id,false).ToList ();
                var so1 = salesOrders.First(x => x.Id == so.Id);
                so.SalesOrderLines =  SalesOrderLineMapper.ToPresentationModels(so1.SalesOrderDetails).ToList(); ;
            }

            

            return salesOrdersPM.AsQueryable();

           

        }

        [HttpGet]
        [Route("{id}")]
        public SalesOrderPM GetSalesOrderById(int id)
        {
            var a = _salesOrderRepository.GetById(id);
           
            var b= SalesOrderMapper.ToPresentationModel(a);

            if (b.CustomerId != null) b.CustomerName = ApiGetCustomerName(b.CustomerId ?? 0);
            b.SalesOrderLines = GetSalesOrderLines(b.Id).ToList ();

            return b;
        }

        [HttpPost]
        public IActionResult AddSalesOrder([FromBody] SalesOrderPM salesOrderPM)
        {

            var so = _exceptionManager.Process(() => _salesOrderRepository.GetQuery(so => so.SalesOrderNumber.Equals(salesOrderPM.SalesOrderNumber)).FirstOrDefault(), "ExceptionShielding");
            if (so != null)
            {
                throw new Exception("Duplicated PO NUmber!");
            }
            else
            {
                salesOrderPM.Status= ESalesOrderStatus.Pending;
                var salesOrder = SalesOrderMapper.FromPresentationModel(salesOrderPM);
               
                _salesOrderRepository.Insert(salesOrder);


                foreach (var soLine in salesOrderPM.SalesOrderLines)
                {
                    soLine.SalesOrderId = salesOrder.Id;                
                    InsertSalesOrderLine(soLine);
                }

                SalesOrderMapper.UpdatePresentationModel(salesOrderPM, salesOrder);

                return new OkObjectResult(salesOrderPM);
            }

        }

        [HttpPut]
        public IActionResult UpdateSalesOrder([FromBody] SalesOrderPM salesOrderPM)
        {
            var salesOrder = SalesOrderMapper.FromPresentationModel(salesOrderPM);
            var existingSalesOrder = _exceptionManager.Process(() => _salesOrderRepository.GetById(salesOrder.Id), "ExceptionShielding");

            existingSalesOrder.UpdateFrom(salesOrder);

            _exceptionManager.Process(() =>
            {
                _salesOrderRepository.Update(existingSalesOrder);               
            }, "ExceptionShielding");

            SalesOrderMapper.UpdatePresentationModel(salesOrderPM, existingSalesOrder);

            var existingSalesOrderLines = _exceptionManager.Process(() => _salesOrderDetailRepository.GetQuery (x=>x.SalesOrderId== salesOrder.Id).ToList(), "ExceptionShielding");

            var soLineIds = new List<int>();
            foreach (var salesOrderLinePM in salesOrderPM.SalesOrderLines)
            {
                if (salesOrderLinePM.Id > 0)
                {
                    soLineIds.Add(salesOrderLinePM.Id);
                    //if (salesOrderLinePM.Modified)
                    //{
                        UpdateSalesOrderLine(salesOrderLinePM);                        
                    //}
                }
                else
                {
                    InsertSalesOrderLine(salesOrderLinePM);                   
                }               
            }

            var deletedIds=existingSalesOrderLines.Select(x => x.Id).ToList().Except(soLineIds);
            foreach (var soLineId in deletedIds)
            {
                _salesOrderDetailRepository.Delete(soLineId);
            }

           
            return new OkObjectResult(salesOrderPM);
        }

        [HttpDelete("{id}")]
        public int DeleteSalesOrder(int id)
        {
            try
            {
                //SalesOrder salesOrder = _salesOrderRepository.GetById(id);
              
                foreach (var salesOrderDetail in _salesOrderDetailRepository.GetQuery (x=>x.SalesOrderId==id).ToList())
                {
                    _salesOrderDetailRepository.Delete(salesOrderDetail);
                }

                _salesOrderRepository.Delete(id);

                return id;
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

        [HttpGet]
        [Route("SODetails/{soId}")]
        public IQueryable<SalesOrderLinePM> GetSalesOrderLines(int soId, bool salesOrderDetail=true)
        {
            //IEnumerable<SalesOrderDetail> salesOrderDetails = _exceptionManager.Process(() => _salesOrderDetailRepository.GetQuery(so => so.SalesOrderId == soId), "ExceptionShielding");

            var so = _salesOrderRepository.GetSalesOrders(soId).FirstOrDefault();

            var sols = new List<SalesOrderLinePM>();

            if (so != null)
            {
                sols = SalesOrderLineMapper.ToPresentationModels(so.SalesOrderDetails).ToList();
            }
            

            if (salesOrderDetail)
            {
                var pids = sols.Select(x => x.ProductId).ToList();
                var products = ApiGetProducts(string.Join(",", pids));

                foreach (var sol in sols)
                {
                    //var product= GetProductName(sol.ProductId);
                    //var product = ApiGetProduct(sol.ProductId);
                    var product = products.FirstOrDefault(x => x.Id == sol.ProductId);
                    if (product != null)
                    {
                        sol.ProductNo = product.Name;
                        sol.ProductName = product.Description;
                        //sol.LotSize = product.LotSize;
                        sol.LotSize = Int32.Parse(product.Value);
                    }
                    //var so = _salesOrderRepository.GetById(sol.SalesOrderId);
                    //if (so != null) sol.SalesOrderNumber = so.SalesOrderNumber;
                    sol.SalesOrderNumber = so.SalesOrderNumber;
                }

            }

           
            return sols.AsQueryable();
        }

        [HttpGet]
        [Route("SODetail/{soLineId}")]
        public SalesOrderLinePM GetSalesOrderLine(int soLineId)
        {
            //var salesOrderDetail = _exceptionManager.Process(() => _salesOrderDetailRepository.GetQuery(so => so.Id == soLineId).FirstOrDefault(), "ExceptionShielding");
            var salesOrderDetail =  _salesOrderDetailRepository.GetSalesOrderLine(soLineId);
            var sol = SalesOrderLineMapper.ToPresentationModel(salesOrderDetail);

            
            //var product = GetProductName(sol.ProductId);
            var product = ApiGetProduct(sol.ProductId);
            if (product != null)
            {
                sol.ProductNo = product.Name;
                sol.ProductName = product.Description;
                sol.LotSize = product.LotSize;
            }
            //var so = _salesOrderRepository.GetById(sol.SalesOrderId);
            //if (so != null) sol.SalesOrderNumber = so.SalesOrderNumber;
            sol.SalesOrderNumber = salesOrderDetail.s.SalesOrderNumber;


            return sol;
        }

        [HttpPost]
        [Route("Details")]
        public IActionResult AddSalesOrderLine([FromBody] SalesOrderLinePM salesOrderLinePM)
        {

            var soLine =InsertSalesOrderLine(salesOrderLinePM);

            return new OkObjectResult(soLine);
        }

        [HttpPut]
        [Route("DetailId/{id}")]
        public void UpdateSalesOrderLine(SalesOrderLinePM salesOrderLinePM)
        {
            SalesOrderDetail salesOrderDetail = SalesOrderLineMapper.FromPresentationModel(salesOrderLinePM);
            SalesOrderDetail existingSalesOrderDetail = _exceptionManager.Process(() => _salesOrderDetailRepository.GetById(salesOrderDetail.Id), "ExceptionShielding");

            existingSalesOrderDetail.UpdateFrom(salesOrderDetail);

            ValidateAssemblyID(existingSalesOrderDetail);

            

            _exceptionManager.Process(() =>
            {
                _salesOrderDetailRepository.Update(existingSalesOrderDetail);
            }, "ExceptionShielding");
        }


        [HttpDelete]
        [Route("DetailId/{id}")]
        public void DeleteSalesOrderLine(int id)
        {
            try
            {
                _salesOrderDetailRepository.Delete(id);
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







        #region Auxiliary
        private void ValidateAssemblyID(SalesOrderDetail salesOrderDetail)
        {
            //if (salesOrderDetail.Int2 != null && salesOrderDetail.Int2 > 0)
            //{
            //    var assemblist = _exceptionManager.Process(() => _productRepository.GetQuery<BillOfMaterial>(bom => bom.ComponentId == salesOrderDetail.ProductID), "ExceptionShielding").ToList();
            //    if (assemblist != null && assemblist.Count > 0)
            //    {
            //        var findmatch = assemblist.Where(a => a.ProductAssemblyId == salesOrderDetail.Int2).ToList();
            //        if (findmatch != null && findmatch.Count > 0)
            //        {
            //            // no issues

            //        }
            //        else
            //        {
            //            int originalid = salesOrderDetail.Int2 ?? 0;
            //            salesOrderDetail.Int2 = assemblist.FirstOrDefault().ProductAssemblyId;
            //            MPEL.Logger.Write("Add salesorder replace kit error int2 original- " + originalid.ToString() + "-replaced with:" + salesOrderDetail.Int2.ToString() + "@" + DateTime.Now.ToString() + " By " + _userName, "AppLog");
            //        }
            //    }
            //    else
            //    {

            //        MPEL.Logger.Write("Add salesorder no parts under kit error int2- " + salesOrderDetail.Int2.ToString() + "-prodid:" + salesOrderDetail.ProductID.ToString() + "@" + DateTime.Now.ToString() + " By " + _userName, "AppLog");
            //        salesOrderDetail.Int2 = null;


            //    }

            //}
        }

        private SalesOrderLinePM InsertSalesOrderLine(SalesOrderLinePM salesOrderLinePM)
        {
            salesOrderLinePM.BalanceQuantity = salesOrderLinePM.Quantity;

            salesOrderLinePM.Status= ESalesOrderLineStatus.Pending;

            SalesOrderDetail salesOrderDetail = SalesOrderLineMapper.FromPresentationModel(salesOrderLinePM);

            ValidateAssemblyID(salesOrderDetail);

            //string fname = null;
            //if (!string.IsNullOrEmpty(salesOrderLinePM.ThumbnailImageFileName) && salesOrderLinePM.ThumbnailImage != null)
            //{
            //    fname = SaveDataToFile(salesOrderLinePM.ThumbnailImageFileName, salesOrderLinePM.ThumbnailImage, (salesOrderLinePM.LineNumber ?? 0));
            //    salesOrderDetail.MaxString2 = fname;
            //}


            salesOrderDetail.CreatedOn = DateTime.Today;
            //salesOrderDetail.CreatedBy = _userName;



            _exceptionManager.Process(() =>
            {
                _salesOrderDetailRepository.Insert(salesOrderDetail);
            }, "ExceptionShielding");

            SalesOrderLineMapper.UpdatePresentationModel(salesOrderLinePM, salesOrderDetail);

            return salesOrderLinePM;
        }

        #endregion

        #region api call of other services

        private List<BasePM> ApiGetCustomers(string customerIds)
        {
            var customers = new List<BasePM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.Get<List<BasePM>>(apiBaseUrl, $"IdName/{customerIds}");
                    if (result != null) customers = result;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{ e.Message}");
                }
            }

            Console.WriteLine(customers.ToString());
            return customers;

        }

        private string ApiGetCustomerName(int customerId)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");
            var customer = HttpHelper.Get<BasePM>(apiBaseUrl, $"{customerId}");
            if (customer!=null) return customer.Name;
            return "";
        }

        private ItemPM ApiGetProduct(int productId)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");
            var product = HttpHelper.Get<ItemPM>(apiBaseUrl, $"{productId}");
            return product;
        }

        private List<BasePM> ApiGetProducts(string productIds)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");
            var products = HttpHelper.Get<List<BasePM>>(apiBaseUrl, $"IdName/{productIds}");
            return products;
        }


        #endregion




    }
}
