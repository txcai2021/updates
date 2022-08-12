using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMTech.APS.Product.API.Repository
{
    using SIMTech.APS.Product.API.Models;
    using SIMTech.APS.Repository;
    public interface IItemRepository : IRepository<Item>
    {
         IList<Item> GetRawMaterails(int itemId);
         IList<int> GetCustomers(int itemId);
         IList<Item> GetCustomerAssemblies(int customerId);
         IList<Item> GetPartAssemblies(int partId);
         IList<Item> GetAssemblyParts(int assemblyId,bool sequence=false);
         IList<Item> GetIndependentParts(int customerId, int customerId1);

        Item GetItem(int itemId);

        void DeleteItem(int id);

        void AddCustomerItem(CustomerItem customerItem);

        void UpdateCustomerItem(int customerId, int itemId);

        IList<Item> SearchPartTypesByName(int customerId, string PartName, int stockId);


    }
}
