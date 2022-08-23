using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;


namespace SIMTech.APS.Product.API.Controllers
{
    using SIMTech.APS.Product.API.Repository;
    using SIMTech.APS.Product.API.Mappers;
    using SIMTech.APS.Product.API.Models;
    using SIMTech.APS.Product.API.PresentationModels;
   


    [Route("api/[controller]")]
    [ApiController]
    public class BOMController : ControllerBase
    {
        private readonly IBOMRepository _bomRepository;

        private readonly ExceptionManager _exceptionManager;


        public BOMController(IBOMRepository bomRepository)
        {
            _bomRepository = bomRepository;
            _exceptionManager = new ExceptionManager();

        }


        //GET: api/BOM
        [HttpGet]
        public  IEnumerable<BillOfMaterialPM> GetAllBOMs()
        {
            var billOfMaterials = _exceptionManager.Process(() => _bomRepository.GetQuery (x=>x.Id>0).Include (x=>x.Component ).ToList(), "ExceptionShielding");

            return BillOfMaterialMapper.ToPresentationModels(billOfMaterials).AsQueryable();
      
        }

        [HttpGet]
        [Route("{id}")]
        public IEnumerable<BillOfMaterialPM> GetBOMbyProductId(int id)
        {
            List<int> itemIDs = new List<int>();
            List<int> pendingItems = new List<int>();
            itemIDs.Add(id);
            pendingItems.Add(id);

            while (pendingItems.Count > 0)
            {
                var itemId = pendingItems.First();
                var componentIds = _bomRepository.GetQuery(x => x.ProductAssemblyId == itemId).Select(x => x.ComponentId).ToList();

                foreach (var componentId in componentIds)
                {
                    itemIDs.Add(componentId);
                    pendingItems.Add(componentId);
                }
                pendingItems.Remove(itemId);
            }
            //var billOfMaterials = _exceptionManager.Process(() => _bomRepository.GetQuery(x => itemIDs.Contains(x.ProductAssemblyId)).ToList(), "ExceptionShielding");

            var billOfMaterials = new List<BillOfMaterial>();
            foreach (var itemId in itemIDs)
            {
                var boms = _bomRepository.GetBillOfMaterialsbyAssemblyId(itemId);
                if (boms!=null && boms.Count >0)
                    billOfMaterials.AddRange(boms);
            }
            

            return BillOfMaterialMapper.ToPresentationModels(billOfMaterials).AsQueryable();

        }
        


        [HttpPost]
        public IActionResult AddBOM([FromBody]  BillOfMaterialPM bomPM)
        {
            var billOfMaterial = BillOfMaterialMapper.FromPresentationModel(bomPM);
            billOfMaterial.CreatedOn = DateTime.Today;

            _exceptionManager.Process(() =>
            {
                _bomRepository.Insert(billOfMaterial);               
            }, "ExceptionShielding");

            BillOfMaterialMapper.UpdatePresentationModel(bomPM, billOfMaterial);
            return new OkObjectResult(bomPM);
        }

        [HttpPut]
        public void UpdateBOM([FromBody] BillOfMaterialPM bomPM)
        {
            var billOfMaterial = BillOfMaterialMapper.FromPresentationModel(bomPM);
            var existingBillOfMaterial = _exceptionManager.Process(() => _bomRepository.GetById(billOfMaterial.Id), "ExceptionShielding");

            if (existingBillOfMaterial!=null)
            {
                existingBillOfMaterial.Bomlevel = billOfMaterial.Bomlevel;
                existingBillOfMaterial.ComponentId = billOfMaterial.ComponentId;
                existingBillOfMaterial.PerAssemblyQty = billOfMaterial.PerAssemblyQty;
                existingBillOfMaterial.ProductAssemblyId = billOfMaterial.ProductAssemblyId;
                existingBillOfMaterial.Uomcode = billOfMaterial.Uomcode;
                existingBillOfMaterial.CreatedBy = billOfMaterial.CreatedBy;

                _exceptionManager.Process(() =>
                {
                    _bomRepository.Update(existingBillOfMaterial);
                }, "ExceptionShielding");
            }
           
        }

        [HttpDelete("{id}")]
        public void DeleteBOM(int id)
        {
            try
            {
                _bomRepository.Delete(id);
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
        [Route("[action]/{id}")]
        public IQueryable<BillOfMaterialPM> GetBillOfMaterialsbyAssemblyId(int id)
        {
            //IEnumerable<BillOfMaterial> billOfMaterials = _exceptionManager.Process(() => _bomRepository.GetQuery(bom => bom.ProductAssemblyId == id), "ExceptionShielding");
            var billOfMaterials = _bomRepository.GetBillOfMaterialsbyAssemblyId(id);
            var boms = BillOfMaterialMapper.ToPresentationModels(billOfMaterials).ToList ();

            GetBoMsbyItemId(boms);

            //foreach (var bom in boms)
            //{
            //    var nextBoMs = _bomRepository.GetBillOfMaterialsbyAssemblyId(bom.ComponentId);
            //    if (nextBoMs!=null && nextBoMs.Count >0)
            //        bom.Components = BillOfMaterialMapper.ToPresentationModels(nextBoMs).ToList();
            //}
            return boms.AsQueryable();
        }


        [HttpGet]
        [Route("[action]/{id}/{category}")]
        public IQueryable<BillOfMaterialPM> GetBillofMaterialsByProductId(int id, string category)
        {
            IEnumerable<BillOfMaterial> billOfMaterials;

            if (category.Equals("M"))
                billOfMaterials = _exceptionManager.Process(() => _bomRepository.GetQuery(bom => bom.ProductAssemblyId == id && bom.Component.Category.Equals("RawMaterial")), "ExceptionShielding");
            else
                billOfMaterials = _exceptionManager.Process(() => _bomRepository.GetQuery(bom => bom.ProductAssemblyId == id && !bom.Component.Category.Equals("RawMaterial")), "ExceptionShielding");

            return BillOfMaterialMapper.ToPresentationModels(billOfMaterials).AsQueryable();
        }



        [HttpGet]
        [Route("[action]/{componentId}/{assemblyId}")]
        public BillOfMaterialPM GetBillOfMaterialByItemAndKitId(int componentId, int assemblyId)
        {
            var billOfMaterial = _exceptionManager.Process(() => _bomRepository.GetQuery(bom => bom.ProductAssemblyId == assemblyId && bom.ComponentId == componentId).FirstOrDefault(), "ExceptionShielding");
            return BillOfMaterialMapper.ToPresentationModel(billOfMaterial);
        }

        [HttpGet]
        [Route("[action]")]
        public IQueryable<BillOfMaterialPM> GetBillOfMaterialsByKitTypes([FromBody] string kitTypeIds)
        {
            IEnumerable<BillOfMaterial> billOfMaterials = _exceptionManager.Process(() => _bomRepository.GetQuery(bom => kitTypeIds.Contains(bom.ProductAssemblyId.ToString ())), "ExceptionShielding");
            return BillOfMaterialMapper.ToPresentationModels(billOfMaterials).AsQueryable();
        }


        [HttpPut]
        [Route("[action]")]
        public void UpdatePartTypesForKitType([FromBody] AssemblyPM assemblyPM )
        {

            var kitTypeId = assemblyPM.Id;

            var assignedPartIds = assemblyPM.Components.Select(x => x.assignedPartId).ToList();

            var quantities = assemblyPM.Components.Select(x => x.quantity).ToList();

            var sequences = assemblyPM.Components.Select(x => x.quantity).ToList();

            int[] partIds = _bomRepository.GetQuery(bom => bom.ProductAssemblyId == kitTypeId && bom.Component.Category != "RawMaterial").Select(bom => bom.ComponentId).ToArray();

            int[] removedParts = partIds.Except(assignedPartIds).ToArray();

            foreach (int removedPart in removedParts)
            {
                var a = _bomRepository.GetQuery(bom => bom.ProductAssemblyId == kitTypeId && bom.ComponentId == removedPart).FirstOrDefault();
                if (a!=null) _bomRepository.Delete(a.Id);
            }
           
            for (int i = 0; i < assignedPartIds.Count(); i++)
            {
                int i1 = assignedPartIds[i];
                BillOfMaterial billOfMaterial = _bomRepository.GetQuery(bom => bom.ProductAssemblyId == kitTypeId && bom.ComponentId == i1).FirstOrDefault();

                if (billOfMaterial != null)
                {
                    billOfMaterial.PerAssemblyQty = quantities[i];
                    billOfMaterial.Uomcode = i.ToString("000");
                    billOfMaterial.CreatedBy = sequences[i].ToString();
                    _bomRepository.Update(billOfMaterial);
                }
                else
                {
                    _bomRepository.Insert(new BillOfMaterial
                    {
                        ProductAssemblyId = kitTypeId,
                        ComponentId = assignedPartIds[i],
                        PerAssemblyQty = quantities[i],
                        //UOMCode = "1",
                        Uomcode = i.ToString("000"),
                        CreatedBy = sequences[i].ToString(),
                    });
                }

            }
        }


        #region Auxiliary methods
        private void GetBoMsbyItemId(IEnumerable<BillOfMaterialPM> boms)
        {
            //var billOfMaterials = _bomRepository.GetBillOfMaterialsbyAssemblyId(id);


            foreach (var bom in boms)
            {
                var nextBoMs = _bomRepository.GetBillOfMaterialsbyAssemblyId(bom.ComponentId);
                if (nextBoMs != null && nextBoMs.Count > 0)
                {
                    bom.Components = BillOfMaterialMapper.ToPresentationModels(nextBoMs).ToList();
                    GetBoMsbyItemId(bom.Components);
                }
            }

            #endregion
        }
    }
}
