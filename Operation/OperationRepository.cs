using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;



namespace SIMTech.APS.Operation.API.Repository
{
    using SIMTech.APS.Operation.API.Models;
    using SIMTech.APS.Operation.API.DBContext;
    using SIMTech.APS.Repository;

    public class OperationRepository : Repository<Operation>,  IOperationRepository
    {
        private readonly OperationContext _dbContext;
        public OperationRepository(OperationContext context) : base(context) { _dbContext = context; }


        public async Task<IEnumerable<Operation>> GetOperationsbyResource(int resourceId)
        {
            //var operations =  await _dbContext.Operations.Include (o=>o.OperationResources).Where(x => x.Version ==1 && (resourceId==0 ||x.OperationResources.Any (a=>a.ResourceId ==resourceId ))).ToListAsync();

            var operations = _dbContext.Operations.Include(o => o.OperationResources).Where(x => x.Version == 1);

            if (resourceId > 0)
                operations = operations.Where(x=>x.OperationResources.Any(a => a.ResourceId == resourceId));

            return await operations.ToListAsync();
        }

        public async Task<IEnumerable<Operation>> GetOperations(int operationId = 0, bool template = true)
        {

            var opearionts = _dbContext.Operations.Include(o => o.OperationResources).Include(r => r.OperationParameters).Where(x => x.Id > 0);

            if (operationId > 0)
            {
                opearionts = opearionts.Where(x => x.Id == operationId);
            }
            else if (template)
            {
                opearionts = opearionts.Where(x => x.Version == 1);
            }


            return await opearionts.ToListAsync();
        }

        public async Task<IEnumerable<Operation>> GetOperations(string operationIds)
        {

            var operationList = operationIds.Split(",").Select (x=>int.Parse (x)).ToList();

            //var operations = _dbContext.Operations.Include(o => o.OperationResources).Include(r => r.OperationParameters).Where(x => operationIds.Contains (x.Id.ToString () ));
            var operations = _dbContext.Operations.Include(o => o.OperationResources).Include(r => r.OperationParameters).Where(x => operationList.Contains(x.Id));

            foreach (var operation in operations)
            {
                foreach (var or in operation.OperationResources)
                    or.Operation = null;

                foreach (var op in operation.OperationParameters)
                    op.Operation = null;

            }
            return await operations.ToListAsync();
        }

        public async Task<int> CopyOperation(int operationId, int routeOperationId, string instruction=null, string  remarks=null, string pictureId=null)
        {
            var operation = await _dbContext.Operations.Include(x=>x.OperationParameters).Include(x=>x.OperationResources).SingleOrDefaultAsync(x=>x.Id==operationId);

            if (operation == null) return 0;

            var maxVersionNo = await _dbContext.Operations.Where(x => x.OperationName == operation.OperationName).MaxAsync(x => x.Version ?? 0);

            var copiedOperation = new Operation()
            {
                OperationName = operation.OperationName,
                Type = operation.Type,
                Categroy = operation.Categroy,
                Description = operation.Description,
                Instruction = operation.Instruction,
                Version = maxVersionNo + 1,
                LocationId = operation.LocationId,
                IsActive = operation.IsActive,
                SizeMin = operation.SizeMin,
                SizeMultiple = operation.SizeMultiple,
                Cost = operation.Cost,
                Pretime = operation.Pretime,
                Posttime = operation.Posttime,
                Duration = operation.Duration,
                DurationPer = operation.DurationPer,
                Uom = operation.Uom
            };

            if (instruction != null) copiedOperation.Instruction = instruction;
            if (remarks != null) copiedOperation.CreatedBy = remarks;
            if (pictureId != null) copiedOperation.ModifiedBy = pictureId;

            if (operation.OperationParameters.Count() > 0)
            {
                copiedOperation.OperationParameters = new List<OperationParameter>();
                foreach (var op in operation.OperationParameters)
                {
                    var x = new OperationParameter()
                    {
                        ParamterId = op.ParamterId,
                        Value = op.Value,
                        MinValue = op.MinValue,
                        MaxValue = op.MaxValue,
                        Uom = op.Uom,
                        NoofReading = op.NoofReading,
                        MaxString1 = op.MaxString1,
                        MaxString2 = op.MaxString2,
                        Int1 = op.Int1,
                        Int2 = op.Int2,
                        Int3 = op.Int3,
                        Int4 = op.Int4,
                        Float1 = op.Float1,
                        Float2 = op.Float2
                    };
                    copiedOperation.OperationParameters.Add(x);
                }
            }

            if (operation.OperationResources.Count() > 0)
            {             
                copiedOperation.OperationResources = new List<OperationResource>();
                foreach (var opr in operation.OperationResources)
                {
                    var x = new OperationResource()
                    {
                        ResourceId = opr.ResourceId,
                        Instruction = opr.Instruction,
                        Cost = opr.Cost,
                        Pretime = opr.Pretime,
                        Posttime = opr.Posttime,
                        Duration = opr.Duration,
                        DurationPer = opr.DurationPer,
                        IsDefault = false
                    };                   
                    copiedOperation.OperationResources.Add(x);
                }
                var defaultMac = operation.OperationResources.FirstOrDefault(x => x.IsDefault ?? false);
                if (defaultMac == null)
                {
                    copiedOperation.OperationResources.First().IsDefault = true;
                }
                else
                {
                    copiedOperation.OperationResources.First(x => x.ResourceId == defaultMac.ResourceId).IsDefault = true;
                }
            }


            var opRates = _dbContext.OperationRates.Where(x => x.RouteOperationId == routeOperationId && x.OperationId == operationId).ToList();
            
            var defaultMc = opRates.FirstOrDefault(x => x.IsDefault ?? false);
            var defaultMc1 = copiedOperation.OperationResources.FirstOrDefault(x => x.ResourceId == (defaultMc==null?0:defaultMc.ResourceId));

            if (defaultMc!=null)
            {
                var defaultMc2 = copiedOperation.OperationResources.First(x => x.IsDefault ?? false);
                if (defaultMc1.ResourceId!=defaultMc2.ResourceId)
                {
                    defaultMc1.IsDefault = true;
                    defaultMc2.IsDefault = false;
                }             
            }

            foreach (var opRate in opRates)
            {
                var oprationResource=copiedOperation.OperationResources.FirstOrDefault(x => x.ResourceId == opRate.ResourceId);
                if (oprationResource!=null)
                {
                    oprationResource.Duration = opRate.RunTime;
                    oprationResource.DurationPer = opRate.Uom;
                    oprationResource.Pretime = opRate.Pretime;
                    oprationResource.Posttime = opRate.Posttime;
                    oprationResource.Cost = opRate.Cost;
                }

            }

            //_dbContext.Operations.Include(x => x.OperationParameters).Include(x => x.OperationResources).Add(copiedOperation);
            _dbContext.Operations.Add(copiedOperation);
            await _dbContext.SaveChangesAsync();

            return copiedOperation.Id;
        }

    }
}
