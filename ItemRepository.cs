
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIMTech.APS.Product.API.Repository
{
    using SIMTech.APS.Repository;
    using SIMTech.APS.Product.API.Models;
    using SIMTech.APS.Product.API.DBContext;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

    public class ItemRepository : Repository<Item>, IItemRepository
    {
        private readonly ProductContext _dbContext;
        private readonly ExceptionManager _exceptionManager;

        public ItemRepository(ProductContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _exceptionManager = new ExceptionManager();
        }

        public IList<Item> GetRawMaterails(int itemId)
        {
            return _dbContext.BillOfMaterials.Include(ci => ci.Component).Where(k => k.ProductAssemblyId == itemId && k.Component.Category.Equals("RawMaterial")).Select(x => x.Component).ToList();
        }

        public IList<int> GetCustomers(int itemId)
        {
            return _dbContext.CustomerItems.Where(k => k.ItemId == itemId).Select(x => x.CustomerId).ToList();
        }


        public IList<Item> GetCustomerAssemblies(int customerId)
        {
            return _dbContext.CustomerItems.Include(ci => ci.Item).Where(ci => ci.CustomerId == customerId && ci.Item.Category.Equals("ProductHierarchyLevelR") && ci.Float2 == null).Select(ci => ci.Item).ToList();
        }

        public IList<Item> GetPartAssemblies(int partId)
        {
            return _dbContext.BillOfMaterials.Include(ci => ci.ProductAssembly).Where(ci => ci.ComponentId == partId).Select(ci => ci.ProductAssembly).ToList();
        }

        public IList<Item> SearchPartTypesByName(int customerId, string partName, int stockId)
        {
            IEnumerable<Item> partTypes;
            IList<Item> partTypes1;

            if (String.IsNullOrEmpty(partName))
            {
                partTypes = _exceptionManager.Process(() => _dbContext.Items.Include(i=>i.CustomerItems).Where(item => item.Category.Equals("ProductHierarchyLevelB") || item.Category.Equals("ProductHierarchyLevelR")), "ExceptionShielding");
            }
            else
            {
                partTypes = _exceptionManager.Process(() => _dbContext.Items.Include(i => i.CustomerItems).Where(item => (item.Category.Equals("ProductHierarchyLevelB") || item.Category.Equals("ProductHierarchyLevelR")) && (item.ItemName.ToLower().Contains(partName.ToLower()) || item.Description.ToLower().Contains(partName.ToLower()))), "ExceptionShielding");
            }


            if (customerId != 0)
            {
                var partTypes2 = partTypes.Where(pt => !(pt.Group3 != null && pt.Group3 == "T") && (pt.Int10 == customerId || pt.Int10 == stockId || pt.Int10 == null)).ToList();
                partTypes1 = partTypes2.Where(pt => pt.Int10 != null || (pt.Int10 == null && pt.CustomerItems.Any(ci => ci.CustomerId == customerId || ci.CustomerId == stockId))).ToList();
            }
            else
            {
                partTypes1 = partTypes.Where(pt => !(pt.Group3 != null && pt.Group3 == "T")).ToList();
            }

            return partTypes1;

        }

        public Item GetItem(int itemId)
        {
            return _dbContext.Items.Include(ci => ci.BillOfMaterialProductAssemblies).Include(x => x.CustomerItems).Where(x => x.Id == itemId).FirstOrDefault();
        }


        public IList<Item> GetAssemblyParts(int assemblyId, bool sequence = false)
        {
            if (sequence)
                return _dbContext.BillOfMaterials.Include(ci => ci.Component).Where(ci => ci.ProductAssemblyId == assemblyId).OrderBy(ci=>ci.Uomcode).ThenBy(b => b.Id).Select(ci => ci.Component).ToList();
            else
                return _dbContext.BillOfMaterials.Include(ci => ci.Component).Where(ci => ci.ProductAssemblyId == assemblyId).Select(ci => ci.Component).ToList();
        }

        public IList<Item> GetIndependentParts(int customerId, int customerId1)
        {
           
            if (customerId == 0)
            {
                //return _dbContext.Items.Include(ci => ci.BillOfMaterialProductAssemblies).Where( x => x.Category.Equals("ProductHierarchyLevelB") && (x.BillOfMaterialProductAssemblies.Count ==0 || x.BillOfMaterialProductAssemblies.Where (x=>x.Component.Category=="RawMaterial").Count ()==0)).ToList();
                return _dbContext.Items.Include(ci => ci.BillOfMaterialProductAssemblies).Where(x => x.Category.Equals("ProductHierarchyLevelB") &&  x.BillOfMaterialComponents.Count == 0).ToList();
            }
            else
            {
                if (customerId1 == 0)
                    return _dbContext.Items.Include(ci => ci.BillOfMaterialProductAssemblies).Where(x => x.Category.Equals("ProductHierarchyLevelB") && x.BillOfMaterialComponents.Count == 0 && x.Int10==customerId).ToList();
                else
                    return _dbContext.Items.Include(ci => ci.BillOfMaterialProductAssemblies).Where(x => x.Category.Equals("ProductHierarchyLevelB") && x.BillOfMaterialComponents.Count == 0 && (x.Int10 == customerId || x.Int10==customerId1)).ToList();
            }
        }


        public void AddCustomerItem(CustomerItem customerItem)
        {
            _dbContext.CustomerItems.Add(customerItem);
            _dbContext.SaveChanges();
        }

        public void UpdateCustomerItem(int customerId, int itemId)
        {
            var customerItem = _exceptionManager.Process(() => _dbContext.CustomerItems.Where(ci => ci.CustomerId == customerId && ci.ItemId == itemId).FirstOrDefault(), "ExceptionShielding"); 

            if (customerItem == null)
            {
                customerItem = new CustomerItem() { CustomerId = (int)customerId, ItemId = itemId };

                _exceptionManager.Process(() =>
                {
                    _dbContext.CustomerItems.RemoveRange(_dbContext.CustomerItems.Where(ci => ci.ItemId == itemId).ToList());
                    _dbContext.CustomerItems.Add(customerItem);
                    _dbContext.SaveChanges();
                }, "ExceptionShielding");
            }
        }

        public void DeleteItem(int id)
        {
            var customerItems = _dbContext.CustomerItems.Where(ci => ci.ItemId == id);
            _dbContext.CustomerItems.RemoveRange(customerItems);
            var BOMs = _dbContext.BillOfMaterials.Where(bom => bom.ProductAssemblyId == id || bom.ComponentId ==id);
            _dbContext.BillOfMaterials.RemoveRange(BOMs);
            var item = _dbContext.Items.SingleOrDefault(s => s.Id == id);
            _dbContext.Items.Remove(item);
            _dbContext.SaveChanges();         
        }


    }
}

