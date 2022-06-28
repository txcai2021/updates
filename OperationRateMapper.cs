using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIMTech.APS.Operation.API.Mappers
{
    using SIMTech.APS.Operation.API.Models;
    using SIMTech.APS.Operation.API.PresentationModels;

    /// <summary>
    /// Maps  PMs (Presentation Models) to  BOs (Business Objects) and vice versa.
    /// </summary>
    public static class OperationRateMapper
    {

        /// <summary>
        /// Transforms list of operationresource BOs list of operationresource PMs.
        /// </summary>
        /// <param name="operationresources">List of operationresource BOs.</param>
        /// <returns>List of operationresource PMs.</returns>
        public static IList<OperationRatePM> ToPresentationModels(IEnumerable<OperationRate> operationRates)
        {
            if (operationRates == null) return null;

            return operationRates.Select(u => ToPresentationModel(u)).ToList();
        }

        /// <summary>
        /// Transforms operationresource BO to operationresource PM.
        /// </summary>
        /// <param name="operationresource">operationresource BO.</param>
        /// <returns>operationresource PM.</returns>
        public static OperationRatePM ToPresentationModel(OperationRate operationRate)
        {

            if (operationRate == null) return null;

            return new OperationRatePM
            {

                Id = operationRate.Id,

                RouteId = operationRate.RouteId,
                RouteOperationId = operationRate.RouteOperationId,
                OperationId = operationRate.OperationId,
                ResourceId = operationRate.ResourceId,
                ItemId = operationRate.ItemId,                
                Cost = operationRate.Cost,
                Pretime = operationRate.Pretime ,
                Posttime = operationRate.Posttime,
                ProductionRate = operationRate.RunTime ,
                //ProdRateUoM = operationRate.UOM ?? 249,
                ProdRateUoM = operationRate.Uom,  
                isDefault = operationRate.IsDefault ?? false,                 
                Comment = operationRate.Instruction,
                CreatedDate = operationRate.CreatedOn                 
            };
        }

         
        /// <summary>
        /// Transforms list of operationresource PMs list of operationresource BOs.
        /// </summary>
        /// <param name="operationresourcePMs">List of operationresource PMs.</param>
        /// <returns>List of operationresource BOs.</returns>
        public static IList<OperationRate> FromPresentationModels(IEnumerable<OperationRatePM> operationRatePMs)
        {
            if (operationRatePMs == null) return null;
            return operationRatePMs.Select(u => FromPresentationModel(u)).ToList();
        }

        /// <summary>
        /// Transforms operationresource PM to operationresource BO.
        /// </summary>
        /// <param name="operationresourcePM">operationresource PM.</param>
        /// <returns>operationresource BO.</returns>
        public static OperationRate FromPresentationModel(OperationRatePM operationRatePM)
        {
            if (operationRatePM == null) return null;

            return new OperationRate
            {
                Id = operationRatePM.Id,
                RouteId = operationRatePM.RouteId,
                RouteOperationId = operationRatePM.RouteOperationId,
                ItemId = operationRatePM.ItemId,
                OperationId = operationRatePM.OperationId,
                ResourceId = operationRatePM.ResourceId,

                Cost = operationRatePM.Cost,
                Pretime = operationRatePM.Pretime,
                Posttime = operationRatePM.Posttime,
                RunTime = operationRatePM.ProductionRate,
                Uom = operationRatePM.ProdRateUoM,
                IsDefault = operationRatePM.isDefault,
                Instruction = operationRatePM.Comment,
                CreatedOn = operationRatePM.CreatedDate,
                ModifiedBy = (operationRatePM.ItemId == 0 && operationRatePM.RouteId == 0) ? operationRatePM.ItemName : ""
            };
        }

    }
}
