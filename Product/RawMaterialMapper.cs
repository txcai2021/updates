using System.Collections.Generic;
using System.Linq;

using System;

namespace SIMTech.APS.Product.Web.Mappers
{
    using SIMTech.APS.Product.API.Models;
    using SIMTech.APS.Product.API.PresentationModels;
    using SIMTech.APS.Product.API.Enums;
    public class RawMaterialMapper
    {
        public static IEnumerable<RawMaterialPM> ToPresentationModels(IEnumerable<Item> items)
        {
            if (items == null) return null;
            return items.Select(ToPresentationModel);
        }

        public static RawMaterialPM ToPresentationModel(Item item)
        {
            if (item == null) return null;

            RawMaterialPM itemPM = new RawMaterialPM
            {
                Id = item.Id,
                //Type = item.Type,
                //RawMaterialCategory = string.IsNullOrEmpty(item.Category) ? string.Empty : item.Category,

                RawMaterialName = string.IsNullOrEmpty(item.ItemName) ? string.Empty : item.ItemName,
                Price = item.UnitPrice,
                RawMaterialDescription = string.IsNullOrEmpty(item.Description) ? string.Empty : item.Description,

                SupplierId = item.Int1 ?? 0,
                LinkedPartId  = item.Int2,

                Diameter = string.IsNullOrEmpty(item.String1) ? string.Empty : item.String1,
                InnerDiameter = string.IsNullOrEmpty(item.String2) ? string.Empty : item.String2,
                Length = string.IsNullOrEmpty(item.String3) ? string.Empty : item.String3,
                OuterDiameter = string.IsNullOrEmpty(item.String4) ? string.Empty : item.String4,
                Thickness = string.IsNullOrEmpty(item.String5) ? string.Empty : item.String5,
                Width = string.IsNullOrEmpty(item.String6) ? string.Empty : item.String6,
                Remarks = string.IsNullOrEmpty(item.String7) ? string.Empty : item.String7,
                Grade = string.IsNullOrEmpty(item.String8) ? string.Empty : item.String8,
                RawMaterialType = string.IsNullOrEmpty(item.String9) ? string.Empty : item.String9,
                //RawMaterialType = (ERawMaterialType)(item.Int2 ?? 0),

                Available = item.Flag1,
                Inactive = string.IsNullOrEmpty(item.Group3) ? false : (item.Group3 == "T" ? true : false),
                
                UoM = string.IsNullOrEmpty(item.Group1) ? string.Empty : item.Group1,
                Currency = string.IsNullOrEmpty(item.Group2) ? string.Empty : item.Group2,

                ProductionLoss = (decimal?)item.Float1, 
            };

            return itemPM;
        }

        public static IEnumerable<Item> FromPresentationModels(IEnumerable<RawMaterialPM> itemPms)
        {
            if (itemPms == null) return null;
            return itemPms.Select(FromPresentationModel);
        }

        public static Item FromPresentationModel(RawMaterialPM itemPM)
        {
            if (itemPM == null) return null;

            return new Item
            {
                Id = itemPM.Id,

                ItemName = itemPM.RawMaterialName,
                //Category = itemPM.RawMaterialCategory,
                //Type = itemPM.Type,
                Description = itemPM.RawMaterialDescription,
                UnitPrice = itemPM.Price,

                String1 = itemPM.Diameter,
                String2 = itemPM.InnerDiameter,
                String3 = itemPM.Length,
                String4 = itemPM.OuterDiameter,
                String5 = itemPM.Thickness,
                String6 = itemPM.Width,
                String7 = itemPM.Remarks,
                String8 = itemPM.Grade,
                String9 = itemPM.RawMaterialType ,
                MaxString1 = itemPM.SupplierName,
                
                Group1 = itemPM.UoM,
                Group2 = itemPM.Currency,
                Group3 = itemPM.Inactive ? "T" : "F", 
                Flag1 = itemPM.Available,

                Int1 = itemPM.SupplierId,
                Int2 = itemPM.LinkedPartId,
                //Int2 = (byte)itemPM.RawMaterialType,

              
                Float1 = (double?)itemPM.ProductionLoss, 
            };
        }

        public static void UpdatePresentationModel(RawMaterialPM itemPM, Item item)
        {
            if (itemPM == null || item == null) return;

            itemPM.Id = item.Id;

            itemPM.RawMaterialName = string.IsNullOrEmpty(item.ItemName) ? string.Empty : item.ItemName;
            //itemPM.RawMaterialCategory = string.IsNullOrEmpty(item.Category) ? string.Empty : item.Category;
            //itemPM.Type = item.Type;
            itemPM.RawMaterialDescription = string.IsNullOrEmpty(item.Description) ? string.Empty : item.Description;
            itemPM.Price = item.UnitPrice;

            itemPM.Diameter = string.IsNullOrEmpty(item.String1) ? string.Empty : item.String1;
            itemPM.InnerDiameter = string.IsNullOrEmpty(item.String2) ? string.Empty : item.String2;
            itemPM.Length = string.IsNullOrEmpty(item.String3) ? string.Empty : item.String3;
            itemPM.OuterDiameter = string.IsNullOrEmpty(item.String4) ? string.Empty : item.String4;
            itemPM.Thickness = string.IsNullOrEmpty(item.String5) ? string.Empty : item.String5;
            itemPM.Width = string.IsNullOrEmpty(item.String6) ? string.Empty : item.String6;
            itemPM.Remarks = string.IsNullOrEmpty(item.String7) ? string.Empty : item.String7;
            itemPM.Grade = string.IsNullOrEmpty(item.String8) ? string.Empty : item.String8;
            itemPM.RawMaterialType = string.IsNullOrEmpty(item.String9) ? string.Empty : item.String9;

            itemPM.Available = item.Flag1;

            itemPM.SupplierId = item.Int1 ?? 0;
            itemPM.LinkedPartId = item.Int2;

            //itemPM.RawMaterialType = (ERawMaterialType)(item.Int2 ?? 0);
          
            itemPM.UoM = string.IsNullOrEmpty(item.Group1) ? string.Empty : item.Group1;
            itemPM.Currency = string.IsNullOrEmpty(item.Group2) ? string.Empty : item.Group2;
            itemPM.Inactive = string.IsNullOrEmpty(item.Group3) ? false : (item.Group3 == "T" ? true : false);

          
            itemPM.ProductionLoss = (decimal?)item.Float1;
        }
    }
}
