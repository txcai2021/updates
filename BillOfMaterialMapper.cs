using System.Collections.Generic;
using System.Linq;
using System;

namespace SIMTech.APS.Product.API.Mappers
{
    using SIMTech.APS.Product.API.Models;
    using SIMTech.APS.Product.API.PresentationModels;
    public class BillOfMaterialMapper
    {
        public static IEnumerable<BillOfMaterialPM> ToPresentationModels(IEnumerable<BillOfMaterial> billOfMaterials)
        {
            if (billOfMaterials == null) return null;
            return billOfMaterials.Select(ToPresentationModel);
        }

        public static BillOfMaterialPM ToPresentationModel(BillOfMaterial billOfMaterial)
        {
            if (billOfMaterial == null) return null;

            int sequence = 0;

            if (!string.IsNullOrWhiteSpace (billOfMaterial.CreatedBy))
            {
                Int32.TryParse(billOfMaterial.CreatedBy, out sequence);
            }

            return new BillOfMaterialPM
            {
                BomLevel = billOfMaterial.Bomlevel,
                ComponentId = billOfMaterial.ComponentId,
                Id = billOfMaterial.Id,
                PerAssemblyQuantity = billOfMaterial.PerAssemblyQty,
                ProductAssemblyId = billOfMaterial.ProductAssemblyId,
                UomCode = billOfMaterial.Uomcode,
                ProductName = billOfMaterial.Component==null?"":billOfMaterial.Component.ItemName,
                Category = billOfMaterial.Component == null ? "" : billOfMaterial.Component.Category,
                Remarks = billOfMaterial.ModifiedBy ,
                Sequence = sequence,
            };
        }

        public static BillOfMaterial FromPresentationModel(BillOfMaterialPM billOfMaterialPM)
        {
            if (billOfMaterialPM == null) return null;

            return new BillOfMaterial
            {
                Bomlevel = billOfMaterialPM.BomLevel,
                ComponentId = billOfMaterialPM.ComponentId,
                Id = billOfMaterialPM.Id,
                PerAssemblyQty = billOfMaterialPM.PerAssemblyQuantity,
                ProductAssemblyId = billOfMaterialPM.ProductAssemblyId,
                Uomcode = billOfMaterialPM.UomCode,
                ModifiedBy = billOfMaterialPM.Remarks,
                CreatedBy = billOfMaterialPM.Sequence.ToString()
            };
        }

        public static void UpdatePresentationModel(BillOfMaterialPM billOfMaterialPM, BillOfMaterial billOfMaterial)
        {
            if (billOfMaterialPM == null || billOfMaterial == null) return;

            billOfMaterialPM.BomLevel = billOfMaterial.Bomlevel;
            billOfMaterialPM.ComponentId = billOfMaterial.ComponentId;
            billOfMaterialPM.Id = billOfMaterial.Id;
            billOfMaterialPM.PerAssemblyQuantity = billOfMaterial.PerAssemblyQty;
            billOfMaterialPM.ProductAssemblyId = billOfMaterial.ProductAssemblyId;
            billOfMaterialPM.UomCode = billOfMaterial.Uomcode;
            billOfMaterialPM.Remarks = billOfMaterial.ModifiedBy;
        }
    }
}
