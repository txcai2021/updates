using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SIMTech.APS.Product.API.Controllers
{
    using SIMTech.APS.Product.API.Repository;
    using SIMTech.APS.Product.API.Models;
    using SIMTech.APS.Product.API.PresentationModels;
    using SIMTech.APS.Product.Web.Mappers; 
    using SIMTech.APS.PresentationModels;
    using SIMTech.APS.Models;
    using SIMTech.APS.Utilities;

    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {

        private readonly IItemRepository _itemRepository;
        private readonly IBOMRepository _bomRepository;
        private readonly ExceptionManager _exceptionManager;
        private List<BasePM> _customers = new List<BasePM>();
        private List<PicturePM> _pictures = new List<PicturePM>();

        public ItemController(IItemRepository itemRepository, IBOMRepository bomRepository)
        {
            _itemRepository = itemRepository;
            _bomRepository = bomRepository;

            _exceptionManager = new ExceptionManager();
        }

        #region Item
        [HttpGet]
        public async Task<IEnumerable<ItemPM>> GetAllItems()
        {
            var items = await _itemRepository.GetAll();
            return ItemMapper.ToPartTypes(items);
        }

        
        [HttpGet("IdName/{itemIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public List<IdNamePM> GetItemsIdName(string itemIds)
        {

            var productIds = new List<int>();
            var items = new List<IdNamePM>();

            if (itemIds == "0")
            {
                items = ( _itemRepository.GetQuery(x => x.Id > 0)).Select(x => new IdNamePM() { Id = x.Id, Code = x.String9, Name = x.ItemName, Description = x.Description,Value=(x.Int7??0).ToString (), Category = x.Category, Float1 = x.Float1, String1 = x.Group1 }).OrderBy (x=>x.Name).ToList();
            }
            else
            {
                try
                {
                    productIds = itemIds.Split(",").Select(x => Int32.Parse(x)).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                //return revision using code
                items = _itemRepository.GetQuery(x=>productIds.Contains(x.Id)).Select(x => new IdNamePM() { Id = x.Id, Code = x.String9, Name = x.ItemName, Description = x.Description, Value = (x.Int7 ?? 0).ToString(), Category = x.Category, Float1 = x.Float1, String1 = x.Group1 }).OrderBy(x=>x.Name).ToList(); 
            }

            return items;
            
        }

        [HttpGet("DB/{itemIds}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemList(string itemIds)
        {
            //var routes = await _routeRepository.GetRoutes();

            var items = await _itemRepository.GetQueryAsync(x=>itemIds.Contains(x.Id.ToString()));
            return Ok(items);

        }


        [HttpGet]
        [Route("{id}")]
        public ItemPM GetItembyId(int id)
        {
            var item = _itemRepository.GetById(id);
            return ItemMapper.ToPartType(item);           
        }

        [HttpDelete("{id}")]  
        public void DeleteItem(int id)
        {
            try
            {
                _itemRepository.DeleteItem(id);
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
        [Route("Component/{assemblyId}")]
        public IEnumerable<ItemPM> GetPartTypesByKitTypeForWO(int assemblyId)
        {
            var kitType = _exceptionManager.Process(() => _itemRepository.GetById(assemblyId), "ExceptionShielding");
            var boms = _exceptionManager.Process(() => _bomRepository.GetQuery(x=>x.ProductAssemblyId==assemblyId), "ExceptionShielding");

            if (kitType == null || kitType.Category != "ProductHierarchyLevelR") return new List<ItemPM>();
          

            List<ItemPM> partTypes;

            var sequenceOption = ApiGetOptionSettingByName("AssemblySequence");


            var components = _itemRepository.GetAssemblyParts(assemblyId, sequenceOption == "T");

            if (sequenceOption == "T")
            {               
                partTypes = ItemMapper.ToPartTypes(components).ToList();
            }
            else
            {
                partTypes = ItemMapper.ToPartTypes(components).OrderBy(p => p.Name).ToList();
            }

            foreach (var item in partTypes)
            {
                var it = boms.Single(i => i.ComponentId == item.Id);
                if (it != null)
                {
                    item.PerAssemblyQuantity = it.PerAssemblyQty;
                }
                else
                {
                    item.PerAssemblyQuantity = 1;
                }

                item.Sequence = it.CreatedBy;
               
            }
            return partTypes;
        }

        [HttpGet]
        [Route("PartFamilies")]
        public IEnumerable<string> GetPartFamilies()
        {
            var result = _exceptionManager.Process(() => _itemRepository.GetQuery(item => item.Category.Equals("ProductHierarchyLevelB")).Select(i => i.Group1).Distinct(), "ExceptionShielding");
            return result;
        }

        [HttpGet]
        [Route("AssemblyFamilies")]
        public IEnumerable<string> GetAssemblyFamilies()
        {
            var result = _exceptionManager.Process(() => _itemRepository.GetQuery(item => item.Category.Equals("ProductHierarchyLevelR")).Select(i => i.Group1).Distinct(), "ExceptionShielding");

            return result;
        }

        [HttpGet]
        [Route("ProductFamilies")]
        public IEnumerable<string> GetProductFamilies()
        {
            var result = _exceptionManager.Process(() => _itemRepository.GetQuery(item => item.Category.Equals("ProductHierarchyLevelR") || item.Category.Equals("ProductHierarchyLevelB")).Select(i => i.Group1).Distinct(), "ExceptionShielding");
            return result;
        }


      

        [HttpGet]
        [Route("ProductFamilies/{productFamily}")]
        public IEnumerable<ItemPM> GetPartsByProductFamily(string productFamily)
        {
            IEnumerable<Item> items;

            if (string.IsNullOrEmpty(productFamily))
            {
                items = _exceptionManager.Process(() => _itemRepository.GetQuery(i => (i.Category.Equals("ProductHierarchyLevelR") || i.Category.Equals("ProductHierarchyLevelB"))  && i.Group1 == null), "ExceptionShielding");
            }
            else
            {
                items = _exceptionManager.Process(() => _itemRepository.GetQuery(i => (i.Category.Equals("ProductHierarchyLevelR") || i.Category.Equals("ProductHierarchyLevelB")) && i.Group1 == productFamily), "ExceptionShielding");
            }

            return ItemMapper.ToPresentationModels(items);

        }

        [HttpGet]
        [Route("ProductRoute")]
        public IEnumerable<ItemPM> GetProductsRoute()
        {
            IEnumerable<Item> items;


            items = _exceptionManager.Process(() => _itemRepository.GetQuery(i => (i.Category.Equals("ProductHierarchyLevelR") || i.Category.Equals("ProductHierarchyLevelB"))), "ExceptionShielding");

            var itemPMs = ItemMapper.ToPresentationModels(items).ToList();

            GetProductRoutes(itemPMs);


            return itemPMs;

        }


        [HttpGet]
        [Route("FGDimensions")]
        public IEnumerable<string> GetFGDimensions()
        {
            var result = _exceptionManager.Process(() => _itemRepository.GetQuery(item => item.Category.Equals("ProductHierarchyLevelR")|| item.Category.Equals("ProductHierarchyLevelB") && item.String4 !=null).Select(i => i.String4).Distinct(), "ExceptionShielding");
            return result;
        }
        #endregion

        #region Assembly
        [HttpGet]
        [Route("Assembly")]
        public IQueryable<KitTypePM> GetKitTypes()
        {
            var st = DateTime.Now;
            Console.WriteLine("GetAssemblies started at " + st.ToString());

            IEnumerable<Item> kitTypes = _itemRepository.GetQuery(i => i.Category.Equals("ProductHierarchyLevelR")).ToList();

            Console.WriteLine("Duration for retrieving data from DB: " + (DateTime.Now - st).TotalSeconds.ToString() + ",Assemblies:" + kitTypes.Count().ToString());


            var pictureIds = kitTypes.Where (x=>x.PictureId!=null && x.PictureId>0).Select(x => x.PictureId??0).Distinct().ToList();

            var st1 = DateTime.Now;

            if (pictureIds.Count>0)
            {
               
                _pictures = ApiGetPictures(string.Join(",", pictureIds));
                Console.WriteLine("Duration for getting product pictures: " + (DateTime.Now - st1).TotalSeconds.ToString() + ",_pictures:" + _pictures.Count().ToString());
                st1 = DateTime.Now;
            }

            var result = kitTypes.Select(GetCustomersForKitType).OrderBy(kt => kt.Name).AsQueryable();

            Console.WriteLine("Duration for getting customer info: " + (DateTime.Now - st1).TotalSeconds.ToString() );

            var et = DateTime.Now;
            Console.WriteLine("GetAssemblies ended at " + et.ToString() + " Duration(seconds):" + (et - st).TotalSeconds.ToString());


            return result;
        }

        [HttpGet]
        [Route("Assembly/Count")]
        public int GetKitTypesCount()
        {
            var n = _itemRepository.GetQuery(i => i.Category.Equals("ProductHierarchyLevelR")).Count();
            return n;
        }

        [HttpGet]
        [Route("Assembly/IdName")]
        public List<BasePM> GetKitTypesIdName()
        {
            IQueryable<Item> kitTypes = _itemRepository.GetQuery(i => i.Category.Equals("ProductHierarchyLevelR"));
            return kitTypes.Select(kt => new BasePM() { Id = kt.Id, Name = kt.ItemName, Description = kt.Description, Code=kt.String9 }).OrderBy(bp => bp.Name).ToList();          
        }

        [HttpGet]
        [Route("Assembly/Page/{pageNumber}/{pageSize}")]
        public IEnumerable<KitTypePM> GetKitTypesByPage(int pageNumber, int pageSize)
        {
            IEnumerable<Item> kitTypes = _itemRepository.GetByPage(i => i.Category.Equals("ProductHierarchyLevelR"), i => i.ItemName, pageNumber, pageSize).ToList();

            return kitTypes.Select(GetCustomersForKitType).OrderBy(kt => kt.Name).AsQueryable();
        }

        


        [HttpGet]
        [Route("Assembly/{customerId}")]
        public IQueryable<KitTypePM> GetKitTypesByCustomer(int customerId)
        {
            var kitTypes = _itemRepository.GetCustomerAssemblies(customerId);
            return kitTypes.Select(GetCustomersForKitType).OrderBy(kt => kt.Name).AsQueryable();
        }


        [HttpGet]
        [Route("AssemblyId/{id}")]
        public KitTypePM GetKitType(int id)
        {
            Item kitType = _itemRepository.GetById(id);
            return GetCustomersForKitType(kitType);

        }

        //public IEnumerable<KitTypePM> SearchKitTypesByName(int pageNumber, int pageSize, string kitTypeName)
        //{

        //    List<int> customerIds = _orderRepository.GetQuery<Customer>(c => c.Category == "Customer" && c.CustomerName.Contains(kitTypeName)).Select(x => x.CustomerID).ToList();
        //    IEnumerable<Item> kitTypes = _exceptionManager.Process(() => _productRepository.GetByPage<Item>(item => item.Category.Equals("ProductHierarchyLevelR") && (item.ItemName.ToLower().Contains(kitTypeName.ToLower()) || item.Description.ToLower().Contains(kitTypeName.ToLower()) || item.Group1.ToLower().Contains(kitTypeName.ToLower()) || customerIds.Contains(item.CustomerItems.FirstOrDefault().CustomerID)), item => item.ItemName, pageNumber, pageSize), "ExceptionShielding");
        //    return kitTypes.Select(GetCustomersForKitType).OrderBy(kt => kt.Name).AsQueryable();
        //}

        //public int GetKitTypeCount()
        //{
        //    return _exceptionManager.Process(() => _productRepository.GetQuery<Item>(i => i.Category.Equals("ProductHierarchyLevelR")).Count(), "ExceptionShielding");
        //}

        //public int SearchKitTypeCountByName(string kitTypeName)
        //{
        //    List<int> customerIds = _orderRepository.GetQuery<Customer>(c => c.Category == "Customer" && c.CustomerName.Contains(kitTypeName)).Select(x => x.CustomerID).ToList();

        //    return _exceptionManager.Process(() => _productRepository.GetQuery<Item>(i => i.Category.Equals("ProductHierarchyLevelR") && (i.ItemName.ToLower().Contains(kitTypeName.ToLower()) || i.Group1.ToLower().Contains(kitTypeName.ToLower()) || i.Description.ToLower().Contains(kitTypeName.ToLower()) || customerIds.Contains(i.CustomerItems.FirstOrDefault().CustomerID))).Count(), "ExceptionShielding");

        //}

        [HttpPost]
        [Route("[action]")]
        public IActionResult AddAssembly([FromBody] KitTypePM kitTypePM)
        {
            var existingKitType = _itemRepository.GetQuery(it => it.ItemName.Equals(kitTypePM.Name) && it.Category.Equals("ProductHierarchyLevelR")).SingleOrDefault();
            if (existingKitType != null)
            {
                throw new Exception("Duplicated user name.");
            }               
            else
            {
                Item kitType = KitTypeMapper.FromPresentationModel(kitTypePM);
                kitType.Category = "ProductHierarchyLevelR";
                kitType.CreatedOn = DateTime.Today;

                _itemRepository.Insert(kitType);

                foreach (var componet in kitTypePM.Components)
                {
                    componet.ProductAssemblyId = kitType.Id;
                    var bomController = new BOMController(_bomRepository);
                    bomController.AddBOM(componet);                   
                }

               
                //call picture service
                try
                {
                    if (kitTypePM.ThumbnailImage != null && kitTypePM.ThumbnailImage.Length > 0)
                    {
                        var picture = new PicturePM
                        {
                            ThumbNailImage = kitTypePM.ThumbnailImage,
                            ThumbnailImageFileName = kitTypePM.ThumbnailImageFileName,
                        };


                        var pictureId = ApiAddPicture(picture);

                        if (pictureId > 0)
                        {
                            kitType.PictureId = pictureId;
                            _itemRepository.Update(kitType);
                        }
                    }
                }
                catch { }

                if (kitTypePM.CustomerId != null && kitTypePM.CustomerId != 0)
                {
                    CustomerItem customerItem = new CustomerItem() { CustomerId = (int)kitTypePM.CustomerId, ItemId = kitType.Id };

                    _exceptionManager.Process(() => { _itemRepository.AddCustomerItem(customerItem); }, "ExceptionShielding");
                }

                KitTypeMapper.UpdatePresentationModel(kitTypePM, kitType);

                return new OkObjectResult(kitTypePM);
            }
        }

        [HttpPut]
        [Route("[action]")]
        public void UpdateAssembly([FromBody] KitTypePM kitTypePM)
        {

            Item kitType = KitTypeMapper.FromPresentationModel(kitTypePM);
            Item existingKitType = _itemRepository.GetById(kitType.Id);

            //existingKitType.Category = kitType.Category;
            existingKitType.Description = kitType.Description;
            existingKitType.ItemName = kitType.ItemName;
            existingKitType.UnitPrice = kitType.UnitPrice;

            existingKitType.Group1 = kitType.Group1;
            existingKitType.Group2 = kitType.Group2;
            existingKitType.Group3 = kitType.Group3;

            existingKitType.Int6 = kitType.Int6;
            existingKitType.Int7 = kitType.Int7;
            
            existingKitType.String4 = kitType.String4;
            existingKitType.String9 = kitType.String9;
            existingKitType.MaxString1 = kitType.MaxString1;

            existingKitType.Flag2 = kitType.Flag2;
            existingKitType.Float1 = kitType.Float1;
            existingKitType.PictureId = kitType.PictureId;

            var bomController = new BOMController(_bomRepository);

            var componentIds = _bomRepository.GetQuery(x => x.ProductAssemblyId == kitType.Id).Select(x => x.ComponentId).ToList();

            foreach (var component in kitTypePM.Components)
            {
                if (component.Id==0)
                {
                    component.ProductAssemblyId = kitType.Id;
                    bomController.AddBOM(component);
                }
                else
                {
                    if (component.Category!= "RawMaterial")
                    {
                        bomController.UpdateBOM(component);
                        componentIds.Remove(component.ComponentId);
                    }                 
                } 
            }

            foreach (var component in componentIds)
            {
                var a= _itemRepository.GetById(component);
                if (a!=null && a.Category != "RawMaterial")
                {
                    var b= _bomRepository.GetQuery(x=>x.ProductAssemblyId==kitType.Id && x.ComponentId==component).FirstOrDefault();
                    if (b!=null) bomController.DeleteBOM(b.Id);
                }
            }

                //call picture service
            //try
            //{
            //    if (existingKitType.PictureId != null && kitTypePM.ThumbnailImage != null && kitTypePM.ThumbnailImage.Length > 0)
            //    {

            //        var existingPicture = ApiGetPicture(existingKitType.PictureId ?? 0);

            //        existingPicture.ThumbNailImage = kitTypePM.ThumbnailImage;
            //        existingPicture.ThumbnailImageFileName = kitTypePM.ThumbnailImageFileName;

            //        ApiUpdatePicture(existingPicture);

            //    }
            //    else if (existingKitType.PictureId == null && kitTypePM.ThumbnailImage != null && kitTypePM.ThumbnailImage.Length > 0)
            //    {
            //        var picture = new PicturePM
            //        {
            //            ThumbNailImage = kitTypePM.ThumbnailImage,
            //            ThumbnailImageFileName = kitTypePM.ThumbnailImageFileName,
            //        };
            //        var pictureId = ApiAddPicture(picture);

            //        if (pictureId > 0) existingKitType.PictureId = pictureId;
            //    }
            //}
            //catch { }


            _itemRepository.Update(existingKitType);
           
        }

    
        #endregion

        #region Part
        [HttpGet]
        [Route("PartId/{id}")]
        public PartTypePM GetPartType(int id)
        {
            Item partType = _exceptionManager.Process(() => _itemRepository.GetById(id), "ExceptionShielding");
            //return PartTypeMapper.ToPresentationModel(partType);
            return GetCustomersForPartType(partType);
        }

        [HttpGet]
        [Route("Part/Count")]
        public int GetPartCount()
        {
            var n = _itemRepository.GetQuery(i => i.Category.Equals("ProductHierarchyLevelB")).Count();
            return n;
        }

        [HttpGet]
        [Route("Part")]
        public IQueryable<PartTypePM> GetPartTypes()
        {

            var st = DateTime.Now;
            Console.WriteLine("GetAssemblies started at " + st.ToString());

            IEnumerable<Item> partTypes = _exceptionManager.Process(() => _itemRepository.GetQuery(i => i.Category.Equals("ProductHierarchyLevelB")), "ExceptionShielding").ToList();

            Console.WriteLine("Duration for retrieving data from DB: " + (DateTime.Now - st).TotalSeconds.ToString() + ",Parts:" + partTypes.Count().ToString());


            var pictureIds = partTypes.Where(x => x.PictureId != null && x.PictureId>0).Select(x => x.PictureId ?? 0).Distinct().ToList();

            var st1 = DateTime.Now;

            if (pictureIds.Count > 0)
            {

                _pictures = ApiGetPictures(string.Join(",", pictureIds));
                Console.WriteLine("Duration for getting product pictures: " + (DateTime.Now - st1).TotalSeconds.ToString() + ",_pictures:" + _pictures.Count().ToString());
                st1 = DateTime.Now;
            }

            var result = partTypes.Select(GetCustomersForPartType).OrderBy(kt => kt.Name).AsQueryable(); ;
            

            Console.WriteLine("Duration for getting customer info: " + (DateTime.Now - st1).TotalSeconds.ToString());

            var et = DateTime.Now;
            Console.WriteLine("GetAssemblies ended at " + et.ToString() + " Duration(seconds):" + (et - st).TotalSeconds.ToString());


            return result;
        }

        [HttpGet]
        [Route("Part/IdName")]
        public List<BasePM> GetPartTypesIdName()
        {
            IQueryable<Item> partTypes = _exceptionManager.Process(() => _itemRepository.GetQuery(i => i.Category.Equals("ProductHierarchyLevelB")), "ExceptionShielding");
            return partTypes.Select (kt=>new BasePM() {Id=kt.Id,Name =kt.ItemName , Description =kt.Description, Code=kt.String9 }).OrderBy(kt => kt.Name).ToList ();
        }

        [HttpGet]
        [Route("Product")]
        public IEnumerable<BasePM> GetAllProducts()
        {
            var products = _exceptionManager.Process(() => _itemRepository.GetQuery(i => i.Category.Equals("ProductHierarchyLevelB") || i.Category.Equals("ProductHierarchyLevelR")), "ExceptionShielding").Select (x=> new BasePM() { Id = x.Id, Name = x.ItemName, Description = x.Category } ).ToList();
            return products.OrderBy(x=>x.Description).ThenBy (x=>x.Name );
            
        }

        [HttpGet]
        [Route("AllParts/{assemblyId}")]
        public IQueryable<PartTypePM> GetAllPartTypes(int assemblyId)
        {
            IEnumerable<Item> partTypes = null;

            if (assemblyId == 0)
            {
                partTypes = _exceptionManager.Process(() => _itemRepository.GetQuery(i => i.Category.Equals("ProductHierarchyLevelB")), "ExceptionShielding");
            }
            else
            {
                List<int> itemIDs = new List<int>();
                List<int> pendingItems = new List<int>();
                itemIDs.Add(assemblyId);
                pendingItems.Add(assemblyId);

                while (pendingItems.Count > 0)
                {
                    var itemId = pendingItems.First();
                    var componentIds = _itemRepository.GetAssemblyParts(assemblyId).Select (x=>x.Id).ToList();

                    foreach (var componentId in componentIds)
                    {
                        itemIDs.Add(componentId);
                        pendingItems.Add(componentId);
                    }
                    pendingItems.Remove(itemId);
                }

                partTypes = _exceptionManager.Process(() => _itemRepository.GetQuery(x => itemIDs.Contains(x.Id)), "ExceptionShielding");
            }

            return partTypes.Select(GetCustomersForPartType).OrderBy(kt => kt.Name).AsQueryable();
        }

        [HttpGet]
        [Route("IndependendParts/{customerId}")]
        public IEnumerable<IdNamePM> GetIndependentPartTypes(int customerId)
        {

            int customerId1 = 0;

            var customerName = ApiGetOptionSettingByName("Customer_Stock");

            if (!string.IsNullOrWhiteSpace(customerName))
            {

                customerId1 = ApiGetCustomerId(customerName);

            }


            var partTypes = _itemRepository.GetIndependentParts(customerId, customerId1);

            return PartTypeMapper.ToPresentationModels2(partTypes).OrderBy (x=>x.Name ).ToList();
        }

        [HttpGet]
        [Route("Parts/{assemblyId}")]
        public IEnumerable<PartTypePM> GetPartTypesByKitType(int assemblyId)
        {
            Item kitType = _exceptionManager.Process(() => _itemRepository.GetById(assemblyId), "ExceptionShielding");

            if (kitType == null || kitType.Category != "ProductHierarchyLevelR") return null;

            var components = _itemRepository.GetAssemblyParts(assemblyId).ToList();

            return components.Select(GetCustomersForPartType).OrderBy(kt => kt.Name).AsQueryable();

        }

       
        //[Query(HasSideEffects = true)]
        //public IEnumerable<PartTypePM> GetPartTypesByKitTypes(int[] kitTypeIds)
        //{
        //    List<Item> kitTypes = _exceptionManager.Process(() => _productRepository.GetQuery<Item>(item => kitTypeIds.Contains(item.ItemID)).ToList(), "ExceptionShielding");
        //    //List<PartTypePM> partTypes = PartTypeMapper.ToPresentationModels(kitTypes.SelectMany(kt => kt.BillOfMaterials1).Select(bom => bom.Item).Distinct()).ToList();
        //    //return partTypes.OrderBy(pt => pt.Name);
        //    List<Item> partTypes = kitTypes.SelectMany(kt => kt.BillOfMaterials1).Select(bom => bom.Item).Distinct().ToList();
        //    return partTypes.Select(GetCustomersForPartType).OrderBy(kt => kt.Name).AsQueryable();
        //}


        [HttpGet]
        [Route("Part/Page/{pageNumber}/{pageSize}")]
        public IEnumerable<PartTypePM> GetPartTypesByPage(int pageNumber, int pageSize)
        {
            IEnumerable<Item> partTypes = _exceptionManager.Process(() => _itemRepository.GetByPage(i => i.Category.Equals("ProductHierarchyLevelB"), i => i.ItemName, pageNumber, pageSize), "ExceptionShielding");
            return partTypes.Select(GetCustomersForPartType).OrderBy(kt => kt.Name).AsQueryable();  
        }

        //public IEnumerable<PartTypePM> SearchPartTypesByName(int pageNumber, int pageSize, string partTypeName)
        //{
        //    List<int> customerIds = _orderRepository.GetQuery<Customer>(c => c.Category == "Customer" && c.CustomerName.Contains(partTypeName)).Select(x => x.CustomerID).ToList();


        //    IEnumerable<Item> partTypes = _exceptionManager.Process(() => _productRepository.GetByPage<Item>(item => item.Category.Equals("ProductHierarchyLevelB") && (item.ItemName.ToLower().Contains(partTypeName.ToLower()) || item.Description.ToLower().Contains(partTypeName.ToLower()) || item.Group1.ToLower().Contains(partTypeName.ToLower()) || customerIds.Contains(item.Int10 ?? 0)), item => item.ItemName, pageNumber, pageSize).ToList(), "ExceptionShielding");

        //    //IEnumerable<Item> partTypes = _exceptionManager.Process(() => _productRepository.GetByPage<Item>(item => item.Category.Equals("ProductHierarchyLevelB") && (item.ItemName.ToLower().Contains(partTypeName.ToLower()) || item.Description.ToLower().Contains(partTypeName.ToLower())), item => item.ItemName, pageNumber, pageSize).ToList(), "ExceptionShielding");
        //    return partTypes.Select(GetCustomersForPartType).AsQueryable();

        //    //foreach (var parttype in partTypes)
        //    //{
        //    //    var bom = _productRepository.GetQuery<BillOfMaterial>(k => k.ComponentId == parttype.ItemID);

        //    //    List<string> cusstr = new List<string>();
        //    //    List<string> assstr = new List<string>();

        //    //    foreach (var item in bom)
        //    //    {
        //    //        var cusitems = _productRepository.GetQuery<CustomerItem>(i => i.ItemID == item.ProductAssemblyId);

        //    //        foreach (var cusitem in cusitems)
        //    //        {
        //    //            var customer = _orderRepository.GetByKey<Customer>(cusitem.CustomerID);
        //    //            cusstr.Add(customer.CustomerName);
        //    //        }

        //    //        var assembly = _productRepository.GetByKey<Item>(item.ProductAssemblyId);
        //    //        assstr.Add(assembly.ItemName);

        //    //    }
        //    //    foreach (var item in cusstr.Distinct().OrderBy(x => x))
        //    //    {
        //    //        parttype.String9 += item + "; ";
        //    //    }
        //    //    parttype.String9 = string.IsNullOrEmpty(parttype.String9) ? string.Empty : parttype.String9.Remove(parttype.String9.Length - 2);

        //    //    foreach (var item in assstr.Distinct().OrderBy(x => x))
        //    //    {
        //    //        parttype.String8 += item + "; ";
        //    //    }
        //    //    parttype.String8 = string.IsNullOrEmpty(parttype.String8) ? string.Empty : parttype.String8.Remove(parttype.String8.Length - 2);
        //    //}


        //    //return PartTypeMapper.ToPresentationModels(partTypes).AsQueryable();
        //}

        [HttpGet]
        [Route("Part/Search/{customerID}/{partName}")]
        public IEnumerable<PartTypePM> SearchPartTypesByName( int customerID,  string partName)
        {
           
            int customerID1 = 0;

            var customerName =ApiGetOptionSettingByName("Customer_Stock");
           
            if (!string.IsNullOrWhiteSpace(customerName))
            {

                customerID1 = ApiGetCustomerId(customerName);
              
            }
            var parts = _itemRepository.SearchPartTypesByName(customerID, partName, customerID1);

            return PartTypeMapper.ToPresentationModels1(parts).AsQueryable();

        }
    
        //[Invoke]
        //public int GetPartTypeCount()
        //{
        //    return _exceptionManager.Process(() => _productRepository.GetQuery<Item>(i => i.Category.Equals("ProductHierarchyLevelB")).Count(), "ExceptionShielding");

        //}

        //public int SearchPartTypeCountByName(string partTypeName)
        //{

        //    List<int> customerIds = _orderRepository.GetQuery<Customer>(c => c.Category == "Customer" && c.CustomerName.Contains(partTypeName)).Select(x => x.CustomerID).ToList();

        //    return _exceptionManager.Process(() => _productRepository.GetQuery<Item>(i => i.Category.Equals("ProductHierarchyLevelB") && (i.ItemName.ToLower().Contains(partTypeName.ToLower()) || i.Group1.ToLower().Contains(partTypeName.ToLower()) || i.Description.ToLower().Contains(partTypeName.ToLower()) || customerIds.Contains(i.Int10 ?? 0))).Count(), "ExceptionShielding");

        //    //return _exceptionManager.Process(() => _productRepository.GetQuery<Item>(i => i.Category.Equals("ProductHierarchyLevelB") && (i.ItemName.ToLower().Contains(partTypeName.ToLower()) || i.Description.ToLower().Contains(partTypeName.ToLower()))).Count(), "ExceptionShielding");
        //}




        [HttpPost]
        [Route("[action]")]
        public IActionResult AddPart([FromBody] PartTypePM partTypePM)
        {
            var existingPart = _itemRepository.GetQuery(it => it.ItemName.Equals(partTypePM.Name) && it.Category.Equals("ProductHierarchyLevelB")).FirstOrDefault();
            if (existingPart != null)
                throw new Exception("Duplicated part name");
            else
            {
                Item partType = PartTypeMapper.FromPresentationModel(partTypePM);
                partType.Category = "ProductHierarchyLevelB";
                partType.CreatedOn = DateTime.Today;
               
                //call picture service
                try
                {
                    if (partTypePM.ThumbnailImage != null && partTypePM.ThumbnailImage.Length > 0)
                    {
                        var picture = new PicturePM
                        {
                            ThumbNailImage = partTypePM.ThumbnailImage,
                            ThumbnailImageFileName = partTypePM.ThumbnailImageFileName,
                        };

                        
                        var pictureId = ApiAddPicture(picture);

                        if (pictureId>0) partType.PictureId = pictureId;
                    }                  
                }
                catch { }



                _exceptionManager.Process(() => { _itemRepository.Insert(partType); }, "ExceptionShielding");

                if (partTypePM.CustomerId != null && partTypePM.CustomerId != 0)
                {
                    CustomerItem customerItem = new CustomerItem() { CustomerId = (int)partTypePM.CustomerId, ItemId = partType.Id };

                    _exceptionManager.Process(() => { _itemRepository.AddCustomerItem(customerItem); }, "ExceptionShielding");
                }

                PartTypeMapper.UpdatePresentationModel(partTypePM, partType);

                return new OkObjectResult(partTypePM);
            }
        }


        [HttpPut]
        [Route("[action]")]
        public IActionResult UpdatePart([FromBody] PartTypePM partTypePM)
        {
            Item partType = PartTypeMapper.FromPresentationModel(partTypePM);
            Item existingPartType = _exceptionManager.Process(() => _itemRepository.GetById(partType.Id), "ExceptionShielding");

            //existingPartType.Category = partType.Category;
            existingPartType.Description = partType.Description;
            existingPartType.ItemName = partType.ItemName;
            existingPartType.UnitPrice = partType.UnitPrice;

            existingPartType.String4 = partType.String4;
            existingPartType.String9 = partType.String9;
            existingPartType.MaxString1 = partType.MaxString1;
            existingPartType.String8 = partType.String8;


            existingPartType.Int6 = partType.Int6;
            existingPartType.Int7 = partType.Int7;
            existingPartType.Int8 = partType.Int8;
            existingPartType.Int9 = partType.Int9;
            existingPartType.Int10 = partType.Int10;

            existingPartType.Group1 = partType.Group1;
            existingPartType.Group2 = partType.Group2;
            existingPartType.Group3 = partType.Group3;

            existingPartType.Float1 = partType.Float1;
            existingPartType.Group5 = partType.Group5;
            existingPartType.PictureId = partType.PictureId;


            ////call picture service
            //try
            //{
            //    if (existingPartType.PictureId != null && partTypePM.ThumbnailImage != null && partTypePM.ThumbnailImage.Length > 0)
            //    {

            //        var existingPicture = ApiGetPicture(existingPartType.PictureId ?? 0);

            //        existingPicture.ThumbNailImage = partTypePM.ThumbnailImage;
            //        existingPicture.ThumbnailImageFileName = partTypePM.ThumbnailImageFileName;

            //        ApiUpdatePicture(existingPicture);

            //    }
            //    else if (existingPartType.PictureId == null && partTypePM.ThumbnailImage != null && partTypePM.ThumbnailImage.Length > 0)
            //    {
            //        var picture = new PicturePM
            //        {
            //            ThumbNailImage = partTypePM.ThumbnailImage,
            //            ThumbnailImageFileName = partTypePM.ThumbnailImageFileName,
            //        };
            //        var pictureId = ApiAddPicture(picture);

            //        if (pictureId > 0) existingPartType.PictureId = pictureId;
            //    }
            //}
            //catch{ }
            

            _exceptionManager.Process(() => { _itemRepository.Update(existingPartType); }, "ExceptionShielding");

            if (partTypePM.CustomerId != null && partTypePM.CustomerId != 0)
            {
                _itemRepository.UpdateCustomerItem(partTypePM.CustomerId ?? 0, partTypePM.Id);
                //call customer service 
                //partTypePM.Customers = ApiGetCustomerName (partTypePM.CustomerId??0);
                var customers =ApiGetCustomers((partTypePM.CustomerId ?? -1).ToString());
                if (customers.Count >0) partTypePM.Customers = customers.First().Code;
            }

            PartTypeMapper.UpdatePresentationModel(partTypePM, existingPartType);

            return new OkObjectResult(partTypePM);
        }

        [HttpDelete]
        [Route("[action]/{productId}")]
        public int DeleteProductRawMaterial(int productId)
        {
            
            foreach (var bom in  _bomRepository.GetQuery(x => x.ProductAssemblyId == productId).ToList())
            {
                _exceptionManager.Process(() => { _bomRepository.Delete(bom); }, "ExceptionShielding");
               
            }

            return productId;
        }

        [HttpPut]
        [Route("[action]/{productId}")]
        public int UpdateProductRawMaterial(int productId, [FromBody] IEnumerable<BillOfMaterialPM> materials)
        {
            if (materials == null || materials.Count() == 0) return 0;

            if (materials.Where(m => m.ProductAssemblyId == productId).Count() != materials.Count()) return -1;

            var rawMatIds = materials.Select(x => x.ComponentId).ToArray ();              
            var ratios = materials.Select(x =>(decimal) x.PerAssemblyQuantity).ToArray();           
            var rawMatUoMTypes = materials.Select(x => x.BomLevel).ToArray();
            var remarks = materials.Select(x => x.Remarks).ToArray();

            Dictionary<int, decimal> ratioDictionary = new Dictionary<int, decimal>();
            Dictionary<int, int> uoMTypeDictionary = new Dictionary<int, int>();


            for (int i = 0; i < rawMatIds.Count(); i++)
                ratioDictionary.Add(rawMatIds[i], ratios[i]);

            for (int i = 0; i < rawMatIds.Count(); i++)
                uoMTypeDictionary.Add(rawMatIds[i], rawMatUoMTypes[i]);

            var existingRawMats = _bomRepository.GetBillOfMaterialsbyAssemblyId(productId, "M").Select (x=>x.ComponentId ).ToList ();

            var removedRawMats = existingRawMats.Except(rawMatIds);

            var newRawMats = rawMatIds.Except(existingRawMats);

            foreach (var item in removedRawMats)
            {
                var bom = _bomRepository.GetQuery(x => x.ProductAssemblyId == productId && x.ComponentId == item).FirstOrDefault();
                if (bom!=null)
                    _bomRepository.Delete(bom);
            }


            foreach (var item in newRawMats)
            {
                var bom = new BillOfMaterial
                {
                    ProductAssemblyId = productId,
                    ComponentId = item,
                    PerAssemblyQty = (double)ratioDictionary[item],
                    Uomcode = Array.IndexOf(rawMatIds, item).ToString("000"),
                    Bomlevel = (short)uoMTypeDictionary[item],
                    ModifiedBy = remarks[Array.IndexOf(rawMatIds, item)],
                };

                _bomRepository.Insert(bom);
                
            }

            foreach (var item in existingRawMats.Except(removedRawMats))
            {
                var a = _bomRepository.GetQuery(bom => bom.ProductAssemblyId == productId && bom.ComponentId == item).FirstOrDefault();
                if (a != null)
                {
                    a.PerAssemblyQty = (double)ratioDictionary[item];
                    a.Bomlevel = (short)uoMTypeDictionary[item];
                    a.Uomcode = Array.IndexOf(rawMatIds, item).ToString("000");
                    a.ModifiedBy = remarks[Array.IndexOf(rawMatIds, item)];
                    _bomRepository.Update(a);
                }
            }

            return productId;
        }

        #endregion

        #region RawMaterial

        [HttpGet]
        [Route("Material")]
        public IList<RawMaterialPM> GetRawMaterial()
        {
            IEnumerable<Item> rawMaterials = _exceptionManager.Process(() => _itemRepository.GetQuery(i => i.Category.Equals("RawMaterial")), "ExceptionShielding").ToList();
            //return RawMaterialMapper.ToPresentationModels(rawMaterials).ToList();
            var rawMaterialPMs = RawMaterialMapper.ToPresentationModels(rawMaterials).ToList();

            var rawMaterialIdNames = new List<IdNamePM>();
            foreach (var rawMaterialPM in rawMaterialPMs)
            {
                rawMaterialIdNames.Add (new IdNamePM() { Id = rawMaterialPM.Id });              
            }

            var rawMaterialQuantities = ApiGetMaterialBalanceQuantities(rawMaterialIdNames);
            var requiredQuantities = ApiGetWorkOrderMaterialsShortage(string.Join(",",rawMaterialIdNames.Select (x=>x.Id)));
            foreach (var rawMaterialPM in rawMaterialPMs)
            {
                var a= rawMaterialQuantities.Where(x => x.Id == rawMaterialPM.Id).FirstOrDefault();
                var b = requiredQuantities.Where(x => x.Id == rawMaterialPM.Id);
                if (a != null)
                {
                    rawMaterialPM.BalanceQuantity = a.Int1;
                    //rawMaterialPM.RequiredQuantity = a.Int2;                   
                }
                if (b != null && b.Count() >0)
                {
                    rawMaterialPM.RequiredQuantity = b.Sum(x => x.Float1 ?? 0);
                }
            }

            //foreach (var rawMaterialPM in rawMaterialPMs )
            //{
            //    var a= ApiGetMaterialBalanceQuantity(new IdNamePM() { Id = rawMaterialPM.Id });
            //    if (a!=null)
            //    {
            //        rawMaterialPM.BalanceQuantity = a.Int1;
            //        rawMaterialPM.RequiredQuantity = a.Int2;
            //    }                  
            //}

            return rawMaterialPMs;
        }

        [HttpGet]
        [Route("Material/Shortage")]
        public IList<RawMaterialPM> GetRawMaterialShortage()
        {

            var rawMaterialIdNames = _exceptionManager.Process(() => _itemRepository.GetQuery(i => i.Category.Equals("RawMaterial")), "ExceptionShielding").Select (x=>new IdNamePM() { Id =x.Id }).ToList();


            
            var requiredQuantities = ApiGetWorkOrderMaterialsShortage(string.Join(",", rawMaterialIdNames.Select(x => x.Id)));
            var shortageIds= requiredQuantities.Where (x=>x.Float1 !=null && x.Float1>0).Select (x=>x.Id).Distinct();

            IEnumerable<Item> rawMaterials = _exceptionManager.Process(() => _itemRepository.GetQuery(i => i.Category.Equals("RawMaterial") && shortageIds.Contains (i.Id)), "ExceptionShielding").ToList();           
            var rawMaterialPMs = RawMaterialMapper.ToPresentationModels(rawMaterials).ToList();

            var rawMaterialQuantities = ApiGetMaterialBalanceQuantities(rawMaterialIdNames.Where (x=> shortageIds.Contains (x.Id)).ToList ());

            foreach (var rawMaterialPM in rawMaterialPMs)
            {
                var a = rawMaterialQuantities.Where(x => x.Id == rawMaterialPM.Id).FirstOrDefault();
                var b = requiredQuantities.Where(x => x.Id == rawMaterialPM.Id);
                if (a != null)
                {
                    rawMaterialPM.BalanceQuantity = a.Int1;
                    //rawMaterialPM.RequiredQuantity = a.Int2;                   
                }
                if (b != null && b.Count() > 0)
                {
                    rawMaterialPM.RequiredQuantity = b.Sum(x => x.Float1 ?? 0);
                }
            }

            return rawMaterialPMs;
        }

        [HttpGet]
        [Route("Material/{rawMaterialId}")]
        public RawMaterialPM GetRawMaterial(int rawMaterialId)
        {
            var rawMaterail = _exceptionManager.Process(() => _itemRepository.GetQuery(x =>x.Id== rawMaterialId), "ExceptionShielding").FirstOrDefault (); ;
            return RawMaterialMapper.ToPresentationModel(rawMaterail);
        }

        [HttpGet]
        [Route("Material/Grades")]
        public IEnumerable<string> GetGrades()
        {          
            return _exceptionManager.Process(() => _itemRepository.GetQuery(item => item.Category.Equals("RawMaterial")).Where(a => !a.String8.Equals(string.Empty)).Select(i => i.String8).Distinct(), "ExceptionShielding");
        }

        [HttpGet]
        [Route("Material/MaterialTypes")]
        public IEnumerable<string> GetRawMaterialTypes()
        {
            return _exceptionManager.Process(() => _itemRepository.GetQuery(item => item.Category.Equals("RawMaterial")).Where(a => !a.String9.Equals(string.Empty)).Select(i => i.String9).Distinct(), "ExceptionShielding");
        }

        [HttpGet]
        [Route("Material/Uoms")]
        public IEnumerable<string> GetUoMs()
        {
            return _exceptionManager.Process(() => _itemRepository.GetQuery(item => item.Category.Equals("RawMaterial")).Where(a => !a.Group1.Equals(string.Empty)).Select(i => i.Group1).Distinct(), "ExceptionShielding");
        }



        [HttpGet]
        [Route("Material/Page/{pageNumber}/{pageSize}")]
        public IEnumerable<RawMaterialPM> GetRawMaterialByPage(int pageNumber, int pageSize)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            IEnumerable<Item> rm = _exceptionManager.Process(() => _itemRepository.GetByPage(i => i.Category.Equals("RawMaterial"), i => i.ItemName, pageNumber, pageSize), "ExceptionShielding");
            
            var rawMaterialPMs = RawMaterialMapper.ToPresentationModels(rm).ToList();

            foreach (var item in rawMaterialPMs)
            {
                var rawMaterial = rm.FirstOrDefault(x => x.Id == item.Id);
                item.NoofParts = rawMaterial.BillOfMaterialProductAssemblies.Count();            
            }

            var rawMaterialIdNames = new List<IdNamePM>();
            foreach (var rawMaterialPM in rawMaterialPMs)
            {
                rawMaterialIdNames.Add(new IdNamePM() { Id = rawMaterialPM.Id });
            }

            var rawMaterialQuantities = ApiGetMaterialBalanceQuantities(rawMaterialIdNames);
            var requiredQuantities = ApiGetWorkOrderMaterialsShortage(string.Join(",", rawMaterialIdNames.Select(x => x.Id)));
            foreach (var rawMaterialPM in rawMaterialPMs)
            {
                var a = rawMaterialQuantities.Where(x => x.Id == rawMaterialPM.Id).FirstOrDefault();
                var b = requiredQuantities.Where(x => x.Id == rawMaterialPM.Id);
                if (a != null)
                {
                    rawMaterialPM.BalanceQuantity = a.Int1;
                    //rawMaterialPM.RequiredQuantity = a.Int2;                   
                }
                if (b != null && b.Count() > 0)
                {
                    rawMaterialPM.RequiredQuantity = b.Sum(x => x.Float1 ?? 0);
                }
            }


            return rawMaterialPMs;
        }

        [HttpGet]
        [Route("Material/Count")]
        public int GetRawMaterialCount()
        {
            var returnable = _exceptionManager.Process(() => _itemRepository.GetQuery(i => i.Category.Equals("RawMaterial")).Count(), "ExceptionShielding");           
            return returnable;
        }

        //public int SearchRawMaterialCountByName(string itemName)
        //{

        //    List<int> supplierIds = _orderRepository.GetQuery<Customer>(c => c.Category == "Supplier" && c.CustomerName.Contains(itemName)).Select(x => x.CustomerID).ToList();
        //    List<int> itemIds = _inventoryRepository.GetQuery<Inventory>(c => c.Type == "RM1" && c.Remarks.Contains(itemName)).Select(x => x.ProductID).ToList();
        //    return _exceptionManager.Process(() => _productRepository.Count<Item>(item => item.Category.Equals("RawMaterial") && (item.ItemName.ToLower().Contains(itemName.ToLower()) || item.String7.ToLower().Contains(itemName.ToLower()) || item.String8.ToLower().Contains(itemName.ToLower()) || item.String9.ToLower().Contains(itemName.ToLower()) || itemIds.Contains(item.ItemID) || supplierIds.Contains(item.Int1 ?? 0) || item.Description.ToLower().Contains(itemName.ToLower()))), "ExceptionShielding");
        //}

        //public IEnumerable<RawMaterialPM> SearchRawMaterialsByName(int pageNumber, int pageSize, string rawMaterialName)
        //{
        //    IEnumerable<Item> rawMaterials;


        //    List<int> supplierIds = _orderRepository.GetQuery<Customer>(c => c.Category == "Supplier" && c.CustomerName.Contains(rawMaterialName)).Select(x => x.CustomerID).ToList();

        //    List<int> itemIds = _inventoryRepository.GetQuery<Inventory>(c => c.Type == "RM1" && c.Remarks.Contains(rawMaterialName)).Select(x => x.ProductID).ToList();


        //    if (rawMaterialName == "")
        //        rawMaterials = _exceptionManager.Process(() => _productRepository.GetByPage<Item>(item => item.Category.Equals("RawMaterial"), item => item.ItemName, pageNumber, pageSize), "ExceptionShielding");
        //    else
        //        //rawMaterials = _exceptionManager.Process(() => _productRepository.GetByPage<Item>(item => item.Category.Equals("RawMaterial") && item.ItemName.ToLower().Contains(rawMaterialName.ToLower()), item => item.ItemName, pageNumber, pageSize), "ExceptionShielding");
        //        //rawMaterials = _exceptionManager.Process(() => _productRepository.GetByPage<Item>(item => item.Category.Equals("RawMaterial") && (item.ItemName.ToLower().Contains(rawMaterialName.ToLower()) || item.String8.ToLower().Contains(rawMaterialName.ToLower())), item => item.ItemName, pageNumber, pageSize), "ExceptionShielding");
        //        //rawMaterials = _exceptionManager.Process(() => _productRepository.GetByPage<Item>(item => item.Category.Equals("RawMaterial") && (item.ItemName.ToLower().Contains(rawMaterialName.ToLower()) || item.String7.ToLower().Contains(rawMaterialName.ToLower()) || item.String8.ToLower().Contains(rawMaterialName.ToLower()) || item.String9.ToLower().Contains(rawMaterialName.ToLower()) || item.Description.ToLower().Contains(rawMaterialName.ToLower())), item => item.ItemName, pageNumber, pageSize), "ExceptionShielding");
        //        rawMaterials = _exceptionManager.Process(() => _productRepository.GetByPage<Item>(item => item.Category.Equals("RawMaterial") && (item.ItemName.ToLower().Contains(rawMaterialName.ToLower()) || item.String7.ToLower().Contains(rawMaterialName.ToLower()) || item.String8.ToLower().Contains(rawMaterialName.ToLower()) || item.String9.ToLower().Contains(rawMaterialName.ToLower()) || itemIds.Contains(item.ItemID) || supplierIds.Contains(item.Int1 ?? 0) || item.Description.ToLower().Contains(rawMaterialName.ToLower())), item => item.ItemName, pageNumber, pageSize), "ExceptionShielding");

        //    var returnable = RawMaterialMapper.ToPresentationModels(rawMaterials).ToList();
        //    foreach (var item in returnable)
        //    {
        //        GetRawMaterialBalanceQuantity(item);
        //        var rawMaterial = rawMaterials.FirstOrDefault(x => x.ItemID == item.Id);
        //        item.NoofParts = rawMaterial.BillOfMaterials.Count();
        //    }

        //    return returnable;
        //}

        //public IEnumerable<RawMaterialPM> SearchRawMaterialsByName1(string rawMaterialName, int partId)
        //{
        //    IEnumerable<Item> rawMaterials;

        //    var part = _exceptionManager.Process(() => _productRepository.GetByKey<Item>(partId), "ExceptionShielding");

        //    var partMatIds = part.BillOfMaterials1.Where(x => x.Item.Category == "RawMaterial").Select(x => x.Item.ItemID).ToList();


        //    if (rawMaterialName == "")
        //        //rawMaterials = _exceptionManager.Process(() => _productRepository.GetQuery<Item>(item => item.Category.Equals("RawMaterial")), "ExceptionShielding");
        //        rawMaterials = _exceptionManager.Process(() => _productRepository.GetQuery<Item>(item => item.Category.Equals("RawMaterial") && !partMatIds.Contains(item.ItemID)), "ExceptionShielding");
        //    else
        //        //rawMaterials = _exceptionManager.Process(() => _productRepository.GetQuery<Item>(item => item.Category.Equals("RawMaterial") && (item.ItemName.ToLower().Contains(rawMaterialName.ToLower())||item.Description.ToLower().Contains(rawMaterialName.ToLower()))), "ExceptionShielding");
        //        rawMaterials = _exceptionManager.Process(() => _productRepository.GetQuery<Item>(item => item.Category.Equals("RawMaterial") && !partMatIds.Contains(item.ItemID) && (item.ItemName.ToLower().Contains(rawMaterialName.ToLower()) || item.Description.ToLower().Contains(rawMaterialName.ToLower()) || item.String7.ToLower().Contains(rawMaterialName.ToLower()))), "ExceptionShielding");


        //    Option option = _exceptionManager.Process(() => _settingRepository.GetQuery<Option>(op => op.OptionName.Trim().Equals("MaterialAssignment")).SingleOrDefault(), "ExceptionShielding");

        //    if (option != null && option.DefaultSetting == "1")
        //    {
        //        rawMaterials = rawMaterials.Where(rm => rm.BillOfMaterials.Count() == 0 || rm.BillOfMaterials.Count(bom => bom.ProductAssemblyId == partId) > 0).ToList();
        //    }

        //    //filter out inactive material
        //    rawMaterials = rawMaterials.Where(pt => !(pt.Group3 != null && pt.Group3 == "T"));

        //    var returnable = RawMaterialMapper.ToPresentationModels(rawMaterials).ToList();

        //    var customers = _exceptionManager.Process(() => _orderRepository.GetAll<Customer>(), "ExceptionShielding").ToList();

        //    foreach (var item in returnable)
        //    {
        //        //Customer c = _exceptionManager.Process(() => _orderRepository.GetByKey<Customer>(item.SupplierId), "ExceptionShielding");
        //        Customer c = customers.FirstOrDefault(x => x.CustomerID == item.SupplierId);

        //        //Customer c = _orderRepository.GetByKey<Customer>(item.SupplierId);

        //        if (c != null)
        //        {
        //            item.SupplierName = c.CustomerName;
        //        }
        //    }


        //    return returnable;
        //}

        [HttpPost]
        [Route("[action]")]
        public IActionResult AddRawMaterial([FromBody] RawMaterialPM rawMaterialPM)
        {
            Item item = RawMaterialMapper.FromPresentationModel(rawMaterialPM);          
            item.Category = "RawMaterial";
            item.Type = 0;
            item.CreatedOn = DateTime.Today;

            _exceptionManager.Process(() =>
            {
                _itemRepository.Insert(item);
            }, "ExceptionShielding");

            RawMaterialMapper.UpdatePresentationModel(rawMaterialPM, item);
            return new OkObjectResult(rawMaterialPM);
        }

        [HttpPut]
        [Route("[action]")]
        public IActionResult UpdateRawMaterial([FromBody] RawMaterialPM rawMaterialPM)
        {
            Item item = RawMaterialMapper.FromPresentationModel(rawMaterialPM);
            Item existingItem = _exceptionManager.Process(() => _itemRepository.GetById(item.Id), "ExceptionShielding");

            existingItem.ItemName = item.ItemName;
            //existingItem.Category = item.Category;           
            existingItem.Description = item.Description;
            existingItem.UnitPrice = item.UnitPrice;

            existingItem.Int1 = item.Int1;
            existingItem.Int2 = item.Int2;
            //existingItem.Type = item.Type;

            existingItem.String1 = item.String1;
            existingItem.String2 = item.String2;
            existingItem.String3 = item.String3;
            existingItem.String4 = item.String4;
            existingItem.String5 = item.String5;
            existingItem.String6 = item.String6;
            existingItem.String7 = item.String7;
            existingItem.String8 = item.String8;
            existingItem.String9 = item.String9;

            existingItem.Group1 = item.Group1;
            existingItem.Group2 = item.Group2;
            existingItem.Group3 = item.Group3;
            existingItem.Flag1 = item.Flag1;


            existingItem.Float1 = item.Float1;

            _exceptionManager.Process(() =>
            {
                _itemRepository.Update(existingItem);
            }, "ExceptionShielding");

            RawMaterialMapper.UpdatePresentationModel(rawMaterialPM, existingItem);

            return new OkObjectResult(rawMaterialPM);

        }

        [HttpPut("LotSize")]
        public IActionResult UpdateLotSize([FromBody] List<BasePM> productLotSizes)
        {

            foreach (var productLotSize in productLotSizes)
            {
                var productName = productLotSize.Name;
                int lotSize = 0;
                if (!string.IsNullOrWhiteSpace(productLotSize.Value)) lotSize = Int32.Parse(productLotSize.Value);
                var item =_itemRepository.GetQuery(x=>x.ItemName== productName && x.Category!= "RawMaterial").FirstOrDefault();

                if (item != null)
                {
                    item.Int7 = lotSize;
                    _itemRepository.Update(item);
                }
            }

            return new OkObjectResult(productLotSizes.Count);

        }


        [HttpGet]
        [Route("Material/UsedByParts")]
        public IEnumerable<ItemPM> GetUsedByParts([FromBody] int rawMaterialId)
        {
            Item rawMaterial = _exceptionManager.Process(() => _itemRepository.GetById(rawMaterialId), "ExceptionShielding");

            var items = new List<Item>();

            foreach (var bom in rawMaterial.BillOfMaterialProductAssemblies)
            {
                items.Add(bom.Component);
            }

            return ItemMapper.ToPresentationModels(items).ToList();
        }

        [HttpPost("Material/Import")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Importrawmaterials([FromBody] List<RawMaterialPM> materialsPM)
        {
            var materials = RawMaterialMapper.FromPresentationModels(materialsPM);
            var count = 0;

            var suppliers = materialsPM.Select(x=> new BasePM() {Code = x.SupplierName,Name=x.SupplierName }).Distinct ().ToList();

            suppliers = ApiGetSupplierIds(suppliers);

            try
            {
                foreach (var material in materials)
                {
                    var a = _itemRepository.GetQuery(x => x.ItemName == material.ItemName && x.Category == "RawMaterial").FirstOrDefault();
                    var b = suppliers.FirstOrDefault(x=>x.Code==material.MaxString1);
                    if (a == null)
                    {

                        material.CreatedOn = DateTime.Now;
                        material.Category = "RawMaterial";
                        if (b!=null) material.Int1 = b.Id;
                        _itemRepository.Insert(material);
                        count++;
                    }
                    else
                    {
                        a.ItemName = material.ItemName;
                        a.UnitPrice = material.UnitPrice;
                        a.Description = material.Description;

                        if (b != null) material.Int1 = b.Id;
                        a.Float1 = material.Float1; //productionLoss
                        a.String3 = material.String3; //Length
                        a.String4 = material.String4; //OuterDiameter
                        a.String5 = material.String5; //thickness
                        a.String6 = material.String6; //width
                        a.String7 = material.String7; //remarks
                        a.String8 = material.String8; //grade
                        a.String9 = material.String9; //type
                        a.Group1 = material.Group1; //uom
                        a.Group2 = material.Group2; //currency

                        a.MaxString1 = material.MaxString1; //description
                        _itemRepository.Update(a);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return new OkObjectResult(count++);
        }

        [HttpPost("Part/Import")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ImportParts([FromBody] List<KitTypePM> partsPM)
        {
          
           
            var count = 0;
            var customers = partsPM.Select(x => new BasePM() { Code = x.Customers, Name = x.Customers }).Distinct().ToList();
            customers = ApiGetCustomerIds(customers);

            try
            {
                foreach (var partPM in partsPM)
                {
                    var part = KitTypeMapper.FromPresentationModel(partPM);
       
                   
                    var a = _itemRepository.GetQuery(x => x.ItemName == part.ItemName && x.Category == "ProductHierarchyLevelB").FirstOrDefault();
                    var b = customers.FirstOrDefault(x => x.Code == partPM.Customers && x.Id > 0);
                    if (a == null)
                    {
                        part.Category = "ProductHierarchyLevelB";
                        part.CreatedOn = DateTime.Now;
                        if (b != null) part.Int10 = b.Id;
                        _itemRepository.Insert(part);

                        foreach (var matl in partPM.Components)
                        {
                            var c = _itemRepository.GetQuery(x => x.ItemName == matl.ProductName && x.Category == "RawMaterial").FirstOrDefault();
                            var n = 0;
                            if (c != null)
                            {
                                matl.ProductAssemblyId = part.Id;
                                matl.ComponentId = c.Id;
                                matl.UomCode = n.ToString("000");
                                matl.BomLevel = 0;
                                var bomController = new BOMController(_bomRepository);
                                bomController.AddBOM(matl);
                            }
                        }

                        if (b != null)
                        {
                            var customerItem = new CustomerItem() { CustomerId = (int)b.Id, ItemId = part.Id };

                            _exceptionManager.Process(() => { _itemRepository.AddCustomerItem(customerItem); }, "ExceptionShielding");
                        }

                        count++;
                    }
                    else
                    {
                        a.ItemName = part.ItemName;
                        a.Description = part.Description;

                        if (b != null) part.Int1 = b.Id;
                        a.Float1 = part.Float1; //production yield

                        a.String4 = part.String4; //FGDimension                      
                        a.String9 = part.String9; //Revision                     
                        a.Group1 = part.Group1; //part family
                        a.Group2 = part.Group2; //make or buy
                        a.Int7 = part.Int7; //qty per lot

                        a.MaxString1 = part.MaxString1; //remarks
                        _itemRepository.Update(a);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return new OkObjectResult(count);
        }

        

        [HttpPost("Assembly/Import")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ImportAssemblies([FromBody] List<KitTypePM> partsPM)
        {


            var count = 0;
            var customers = partsPM.Select(x => new BasePM() { Code = x.Customers, Name = x.Customers }).Distinct().ToList();
            customers = ApiGetCustomerIds(customers);

            try
            {
                foreach (var partPM in partsPM)
                {
                    var part = KitTypeMapper.FromPresentationModel(partPM);


                    var a = _itemRepository.GetQuery(x => x.ItemName == part.ItemName && x.Category == "ProductHierarchyLevelR").FirstOrDefault();
                    var b = customers.FirstOrDefault(x => x.Code == partPM.Customers && x.Id > 0);
                    if (a == null)
                    {
                        part.Category = "ProductHierarchyLevelR";
                        part.CreatedOn = DateTime.Now;
                        if (b != null) part.Int10 = b.Id;
                        _itemRepository.Insert(part);

                        foreach (var matl in partPM.Components)
                        {
                            var c = _itemRepository.GetQuery(x => x.ItemName == matl.ProductName && x.Category != "RawMaterial").FirstOrDefault();
                            if (c==null) c = _itemRepository.GetQuery(x => x.ItemName == matl.ProductName && x.Category == "RawMaterial").FirstOrDefault();
                            
                            var n = 0;
                            if (c != null)
                            {
                                matl.ProductAssemblyId = part.Id;
                                matl.ComponentId = c.Id;
                                matl.UomCode = n.ToString("000");
                                matl.BomLevel = 0;
                                var bomController = new BOMController(_bomRepository);
                                bomController.AddBOM(matl);
                            }
                        }

                        if (b != null)
                        {
                            var customerItem = new CustomerItem() { CustomerId = (int)b.Id, ItemId = part.Id };

                            _exceptionManager.Process(() => { _itemRepository.AddCustomerItem(customerItem); }, "ExceptionShielding");
                        }

                        count++;
                    }
                    else
                    {
                        a.ItemName = part.ItemName;
                        a.Description = part.Description;

                        if (b != null) part.Int1 = b.Id;
                        a.Float1 = part.Float1; //production yield

                        a.String4 = part.String4; //FGDimension                      
                        a.String9 = part.String9; //Revision                     
                        a.Group1 = part.Group1; //part family
                        a.Group2 = part.Group2; //make or buy
                        a.Int7 = part.Int7; //qty per lot

                        a.MaxString1 = part.MaxString1; //remarks
                        _itemRepository.Update(a);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return new OkObjectResult(count);
        }

        [HttpGet]
        [Route("Material/LinkedPart/{partId}")]
        public ItemPM GetLinkedPart( int partId)
        {
            var linkedPart = _exceptionManager.Process(() => _itemRepository.GetQuery(x=>x.Int2!=null && x.Int2 == partId &&  x.Category == "RawMaterial").FirstOrDefault (), "ExceptionShielding");

            return ItemMapper.ToPresentationModel(linkedPart);
        }

        [HttpGet]
        [Route("Material/LinkedPartName/{partName}")]
        public ItemPM GetLinkedPartName(string partName)
        {
            var part = _itemRepository.GetQuery(x => x.ItemName == partName && x.Category != "RawMaterial").FirstOrDefault();
            if (part != null)
            {
                var linkedPart = _exceptionManager.Process(() => _itemRepository.GetQuery(x => x.Int2 != null && x.Int2 == part.Id && x.Category  == "RawMaterial").FirstOrDefault(), "ExceptionShielding");

                return ItemMapper.ToPresentationModel(linkedPart);
            }
            else
                return null;          
        }

        [HttpGet]
        [Route("[action]")]
        public int CheckPartName([FromBody] PartTypePM partPM )
        {
            int count = 0;

            int partId = partPM.Id;
            string partName = partPM.Name;
            string revision = partPM.Revision;

            if (partId == 0) //check part name
            {              
                count = _exceptionManager.Process(() => _itemRepository.GetQuery(r => r.ItemName == partName.Trim() && r.Category == revision).Count(), "ExceptionShielding");
            }
            else //check revision, revision-->string9
            {
                var part = _exceptionManager.Process(() => _itemRepository.GetById(partId), "ExceptionShielding");
                count = _exceptionManager.Process(() => _itemRepository.GetQuery(r => r.ItemName == part.ItemName && r.String9 == revision).Count(), "ExceptionShielding");
            }


            return count;

        }



        [HttpPost]
        [Route("Copy")]
        public PartTypePM CopyPart([FromBody] PartTypePM partPM )
        {
            var p=CopyPart(partPM.Id, partPM.Name, partPM.Revision, true);
            return PartTypeMapper.ToPresentationModel (p);
        }


       


        #endregion

        #region Auxiliary methods
        private KitTypePM GetCustomersForKitType(Item item)
        {

            KitTypePM kitTypePM = KitTypeMapper.ToPresentationModel(item);

            var customerIds = _itemRepository.GetCustomers(item.Id);

            var custIds =customerIds.Except(_customers.Select(x => x.Id)).ToList ();

            if (custIds.Count>0)
            {
                
                var customers = ApiGetCustomers(string.Join(",", custIds));
                _customers.AddRange(customers);
            }
            
           
            //call api from customer service using eventbus
            foreach (var customerId in customerIds)
            {
                //kitTypePM.Customers += ";" + ApiGetCustomerName(customerId);
                var customer = _customers.Where(x => x.Id == customerId).FirstOrDefault();
                if (customer!=null) kitTypePM.Customers += ";" + customer.Code;
                kitTypePM.CustomerId = customerId;
            }

            if (!string.IsNullOrEmpty(kitTypePM.Customers))
                kitTypePM.Customers = kitTypePM.Customers.Remove(0, 1);

            var materials = _itemRepository.GetRawMaterails(item.Id);
            kitTypePM.Materials = "";
            foreach (var m in materials)
            {
                kitTypePM.Materials += ";" + m.ItemName;
            }
            if (!string.IsNullOrEmpty(kitTypePM.Materials))
                kitTypePM.Materials = kitTypePM.Materials.Remove(0, 1);


            try
            {
                if (item.PictureId != null && item.PictureId > 0)
                {                   
                    var picture = _pictures.FirstOrDefault(x => x.PictureID == (item.PictureId ?? 0));
                    //if (picture == null) picture = ApiGetPicture(item.PictureId ?? 0);
                    if (picture != null)
                    {
                        kitTypePM.ThumbnailImage = picture.ThumbNailImage;
                        kitTypePM.ThumbnailImageFileName = picture.ThumbnailImageFileName;
                    }

                }
            }
            catch { }
           


            if (kitTypePM.RouteId == null || kitTypePM.RouteId == 0)
            {
                //Todo: call api from process service using eventbus

                //var familyRoute = _processRepository.GetQuery<SIMTech.APS.Process.Business.Route>(x => x.Comment == kitTypePM.PartFamily && x.Version == 1).FirstOrDefault();
                //if (familyRoute != null)
                //    kitTypePM.RouteId = familyRoute.RouteId;
            }

            if (kitTypePM.RouteId != null || kitTypePM.RouteId > 0)
            {
                //Todo: call api from process service using eventbus

                //var route = _processRepository.GetQuery<SIMTech.APS.Process.Business.Route>(x => x.RouteId == kitTypePM.RouteId).FirstOrDefault();

                //if (route != null)
                //    kitTypePM.RouteName = route.RouteName;
            }

            var bomController = new BOMController(_bomRepository);
            kitTypePM.Components = bomController.GetBillOfMaterialsbyAssemblyId(item.Id);
            kitTypePM.NoOfParts = kitTypePM.Components.Where(x=>x.Category!= "RawMaterial").Count();

            return kitTypePM;
        }

       
        private PartTypePM GetCustomersForPartType(Item item)
        {

            PartTypePM partTypePM = PartTypeMapper.ToPresentationModel(item);

            partTypePM.Customers = "";

            var customerIds = _itemRepository.GetCustomers(item.Id);

            var custIds = customerIds.Except(_customers.Select(x => x.Id)).ToList();

            if (custIds.Count > 0)
            {

                var customers = ApiGetCustomers(string.Join(",", custIds));
                _customers.AddRange(customers);
            }

           
            //call api from customer service using eventbus
            foreach (var customerId in customerIds)
            {
                //partTypePM.Customers += ";" + ApiGetCustomerName(customerId);
                var customer = _customers.Where(x => x.Id == customerId).FirstOrDefault();
                if (customer != null) partTypePM.Customers += ";" + customer.Code;
                partTypePM.CustomerId = customerId;
            }

            if (!string.IsNullOrEmpty(partTypePM.Customers))  partTypePM.Customers = partTypePM.Customers.Remove(0, 1);

            var assemblyies =_itemRepository.GetPartAssemblies(item.Id);

            foreach (var a in assemblyies)
            {
                partTypePM.Assemblies += ";" + a.ItemName;
            }

            if (!string.IsNullOrEmpty(partTypePM.Assemblies))
                partTypePM.Assemblies = partTypePM.Assemblies.Remove(0, 1);


            var materials = _itemRepository.GetRawMaterails(item.Id);
            partTypePM.Materials = "";
            foreach (var m in materials)
            {
                partTypePM.Materials += ";" + m.ItemName;
            }

            try
            {
                if (item.PictureId != null && item.PictureId > 0)
                {                  
                    var picture = _pictures.FirstOrDefault(x=>x.PictureID== (item.PictureId ?? 0));
                    //if (picture == null) picture = ApiGetPicture(item.PictureId ?? 0);                   
                    if (picture != null)
                    {
                        partTypePM.ThumbnailImage = picture.ThumbNailImage;
                        partTypePM.ThumbnailImageFileName = picture.ThumbnailImageFileName;
                    }

                }
            }
            catch { }
            
           


            if (!string.IsNullOrEmpty(partTypePM.Materials))
                partTypePM.Materials = partTypePM.Materials.Remove(0, 1);          

            return partTypePM;
        }

        private Item CopyPart(int partId, string partName, string revision, bool disableOldRevison)
        {
            //var item = _exceptionManager.Process(() => _itemRepository.GetById(partId), "ExceptionShielding");
            var item = _exceptionManager.Process(() => _itemRepository.GetItem(partId), "ExceptionShielding");
            
            var newPart = new Item();

            newPart.Category = item.Category;
            newPart.Description = item.Description;
            newPart.UnitPrice = item.UnitPrice;

            newPart.Int1 = item.Int1;
            newPart.Int2 = item.Int2;
            newPart.Int3 = item.Int3;
            newPart.Int4 = item.Int4;
            newPart.Int5 = item.Int5;
            newPart.Int6 = item.Int6;
            newPart.Int7 = item.Int7;
            newPart.Int8 = item.Int8;
            newPart.Int9 = item.Int9;
            newPart.Int10 = item.Int10;

            newPart.String1 = item.String1;
            newPart.String2 = item.String2;
            newPart.String3 = item.String3;
            newPart.String4 = item.String4;
            newPart.String5 = item.String5;
            newPart.String6 = item.String6;
            newPart.String7 = item.String7;
            newPart.String8 = item.String8;


            newPart.MaxString1 = item.MaxString1;
            newPart.MaxString2 = item.MaxString2;
            newPart.MaxString3 = item.MaxString3;
            newPart.MaxString4 = item.MaxString4;
            newPart.MaxString5 = item.MaxString5;


            newPart.Group1 = item.Group1;
            newPart.Group2 = item.Group2;
            newPart.Group3 = item.Group3;
            newPart.Group4 = item.Group4;
            newPart.Group5 = item.Group5;
            newPart.Group6 = item.Group6;

            newPart.Flag1 = item.Flag1;
            newPart.Flag2 = item.Flag2;
            newPart.Flag3 = item.Flag3;
            newPart.Flag4 = item.Flag4;
            newPart.Flag5 = item.Flag5;


            newPart.Float1 = item.Float1;
            newPart.Float2 = item.Float2;
            newPart.Float3 = item.Float3;
            newPart.Float4 = item.Float4;
            newPart.Float5 = item.Float5;

            newPart.ParentItemId = item.ParentItemId;

            newPart.ItemName = partName;
            newPart.String9 = revision;


            //deactive the old revision
            if (newPart.ItemName == item.ItemName && disableOldRevison)
            {
                item.Group3 = "T";
            }


            _exceptionManager.Process(() =>
            {
                _itemRepository.Insert(newPart);              
            }, "ExceptionShielding");



            newPart.BillOfMaterialProductAssemblies = new List<BillOfMaterial>();
            foreach (var bom in item.BillOfMaterialProductAssemblies)
            {
                var bom1 = new BillOfMaterial();
                bom1.Bomlevel = bom.Bomlevel;
                bom1.ComponentId = bom.ComponentId;
                bom1.ProductAssemblyId = newPart.Id;
                bom1.ModifiedBy = bom.ModifiedBy;
                bom1.Uomcode = bom.Uomcode;
                bom1.StartDate = bom.StartDate;
                bom1.EndDate = bom.EndDate;
                bom1.PerAssemblyQty = bom.PerAssemblyQty;

                newPart.BillOfMaterialProductAssemblies.Add(bom1);
            }

            newPart.CustomerItems = new List<CustomerItem>();
            foreach (var customerItem in item.CustomerItems)
            {
                var customerItem1 = new CustomerItem();
                customerItem1.CustomerId = customerItem.CustomerId;
                customerItem1.ItemId = newPart.Id;

                newPart.CustomerItems.Add(customerItem1);
            }



            _exceptionManager.Process(() =>
            {
                _itemRepository.Update(newPart);              
            }, "ExceptionShielding");

            return newPart;
        }

        private void GetProductRoutes(IEnumerable<ItemPM> itemPMs)
        {
            var productRoutes = ApiGetProductRoutings();
            var productFamilyRoutes = ApiGetProductFamilyRoutings();

            foreach (var itemPM in itemPMs)
            {
                var a = productFamilyRoutes!=null?productFamilyRoutes.Where(x => x.Value == itemPM.PartFamily).FirstOrDefault():null;
                if (a != null)
                {
                    itemPM.FamilyRouteName = a.Name;
                }
                    
                var b = productRoutes!=null?productRoutes.Where(x => x.Value == itemPM.Id.ToString ()).FirstOrDefault():null;
                if (b != null)
                {
                    itemPM.RouteId = b.Id;
                    itemPM.RouteName = b.Name;
                }
                   
            }
           
        }

        #endregion

        #region Integration API call

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


        //To Get Custeomer Name by calling Customer API
        private string ApiGetCustomerName(int customerId)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL"));
                
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
                        var customer = Newtonsoft.Json.JsonConvert.DeserializeObject<BasePM>(alldata);
                        if (customer!=null) return customer.Name;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e); 
                }
                
            }
            return "";
        }

        private int ApiGetCustomerId(string customerName)
        {
     
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                   
                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL") + "Name/");


                    //HTTP GET
                    var responseTask = client.GetAsync(customerName.ToString());
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        var customer = Newtonsoft.Json.JsonConvert.DeserializeObject<BasePM>(alldata);
                        if (customer!=null) return customer.Id;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }


            return 0;


        }



        private string ApiGetOptionSettingByName(string optionName)
        {

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {

                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("RPS_SETTING_URL")+"Name/");


                    //HTTP GET
                    var responseTask = client.GetAsync(optionName.ToString());
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        var option = Newtonsoft.Json.JsonConvert.DeserializeObject<Option>(alldata);
                        if (option!=null) return option.DefaultSetting;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }


            return "";


        }

        private List<BasePM> ApiGetProductFamilyRoutings()
        {

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {

                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("RPS_ROUTE_URL") + "ProductFamily/");


                    //HTTP GET
                    var responseTask = client.GetAsync("%20");
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

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }


            return null;


        }

        private List<BasePM> ApiGetProductRoutings()
        {

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {

                    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("RPS_ROUTE_URL") + "Product/0");


                    //HTTP GET
                    var responseTask = client.GetAsync("");
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

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }


            return null;


        }

        private PicturePM ApiGetPicture(int pictureId)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PICTURE_URL");
            var url = $"{pictureId}";
            var picture = HttpHelper.Get<PicturePM>(apiBaseUrl, url);
            return picture;
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

            return locations ?? new List<BasePM>();
        }

        private List<PicturePM> ApiGetPictures(string pictureIds)
        {

            var pictures = new List<PicturePM>();

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PICTURE_URL");
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    pictures = HttpHelper.Get<List<PicturePM>>(apiBaseUrl, $"Ids/{pictureIds}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in getting pictures:" + $"Ids/{pictureIds}");
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return pictures ?? new List<PicturePM>();
        }


        private void ApiUpdatePicture(PicturePM picture)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PICTURE_URL");
            var url = $"{picture.PictureID}";
            HttpHelper.PutAsync<PicturePM>(apiBaseUrl, url, picture);           
        }

        private  int ApiAddPicture(PicturePM picture)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PICTURE_URL");
            var url = "";
            var task = HttpHelper.PostAsync<PicturePM>(apiBaseUrl, url, picture);
            return task.Result;
        }

        private void ApiDeletePicture(int pictureId)
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PICTURE_URL");
            var url = $"{pictureId}";
            HttpHelper.DeleteAsync(apiBaseUrl, url);
        }

        private IdNamePM ApiGetMaterialBalanceQuantity(IdNamePM material)
        {
            IdNamePM invMaterial=null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INVENTORY_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl) && material.Id > 0)
            {
                if (material.Id == 1526)
                    Console.WriteLine("1526");
                 
                try
                {
                    var task = HttpHelper.PostAsync<IdNamePM, IdNamePM>(apiBaseUrl, "BalanceQuantity", material);
                    task.Wait();
                    invMaterial = task.Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.Message);
                }
            }

            return invMaterial;
        }

        private List<IdNamePM> ApiGetMaterialBalanceQuantities(List<IdNamePM> materials)
        {
            List<IdNamePM> invMaterials = new List<IdNamePM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INVENTORY_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl) && materials.Count > 0)
            {
              
                try
                {
                    var task = HttpHelper.PostAsync<List<IdNamePM>, List<IdNamePM>>(apiBaseUrl, "BalanceQuantities", materials);
                    task.Wait();
                    invMaterials = task.Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.Message);
                }
            }

            return invMaterials;
        }


        private List<IdNamePM> ApiGetWorkOrderMaterialsShortage(string materialIds)
        {
            var items = new List<IdNamePM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {

                    var result = HttpHelper.Get<List<IdNamePM>>(apiBaseUrl, $"MaterialsShortage/{materialIds}");

                    if (result != null) items = result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.Message);
                }
            }

            return items.ToList();
        }


        private List<BasePM> ApiGetSupplierIds(List<BasePM> suppliers)
        {
            
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");

            apiBaseUrl = apiBaseUrl.Replace("customer", "supplier");
            apiBaseUrl = apiBaseUrl.Replace("Customer", "Supplier");


            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {

                    var task =  HttpHelper.PostAsync<List<BasePM>, List<BasePM>>(apiBaseUrl, $"Suppliers", suppliers);
                    task.Wait();
                    var result = task.Result;
          
                    if (result != null) suppliers = result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.Message);
                }
            }

            return suppliers;
        }

       
        private List<BasePM> ApiGetCustomerIds(List<BasePM> customers)
        {

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");

           
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {

                    var task = HttpHelper.PostAsync<List<BasePM>, List<BasePM>>(apiBaseUrl, $"Customers", customers);
                    task.Wait();
                    var result = task.Result;

                    if (result != null) customers = result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.Message);
                }
            }

            return customers;
        }



        #endregion

    }
}
