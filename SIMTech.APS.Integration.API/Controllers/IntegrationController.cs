using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;


namespace SIMTech.APS.Integration.API.Controllers
{
    using SIMTech.APS.Integration.API.Repository;
    using SIMTech.APS.Integration.API.Models;
    using SIMTech.APS.WorkOrder.API.PresentationModels;


    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly IPPOrderRepository _integrationRepository;

        public IntegrationController(IPPOrderRepository integrationRepository)
        {
            _integrationRepository = integrationRepository;
        }

		


		//GET: api/Role
		[HttpGet("DispatchList")]
        public async Task<IEnumerable<Pporder>> GetAllPPOrders() => await _integrationRepository.GetAll();


		[HttpGet]
		[Route("DispatchList/{schedueId}")]
		public async Task<IEnumerable<Pporder>> GetPPOrderByScheduleId(int schedueId)
		{
			//await _integrationRepository.GetbyId(schedueId);
			return await _integrationRepository.GetbyScheduleId(schedueId);
		}

		


        [HttpPost("ReleaseWO")]
        public async Task GeneratePPOrder([FromBody] WorkOrderIntegrationPM wo)
        {
			var ppOrder = new Pporder() 
			{ 
				GoldContent = "", 
				Size = "", 
				BasicChainType = wo.ProductFamily, 
				AllocWeight = (decimal)wo.Quantity, 
				Description = "", 
				Type = "1", 
				IssueDate = wo.IssueDate, 
				Status = "Pending" 
			};
         
			var ppOrderDetail = new PporderDetail()
			{				 
				//Ppid = ppOrder.Id,
				PartNo = wo.ProductNo,
				SublotId = wo.WorkOrderNumber,
				SalesOrderId =wo.Id,
				SalesOrderDetId = wo.SalesOrderDetailId,
				Weight = (decimal)wo.Quantity,
				Remark = "",
				EstCompletionDate = wo.DueDate
			};

			await _integrationRepository.DeleteByWorkOrder(wo.Id);


			ppOrder.PporderDetails.Add(ppOrderDetail);
            await _integrationRepository.Insert<Pporder>(ppOrder);

            await GeneratePPRoute(wo.RouteId, ppOrder.Id, wo.ProductNo);
        }

		[HttpPost("PPRoute")]
		public async Task<int> GeneratePPOrderRouteForDPS([FromBody] List<PporderRoute> ppRoutes)
		{
			Console.WriteLine(ppRoutes.Count ());

			if (ppRoutes.Count == 0) return 0;

			var scheduleId =ppRoutes.First().ScheduleId ;

			await _integrationRepository.DeleteByScheduleId(scheduleId??0);


			foreach (var ppRoute in ppRoutes)
            {
				var ppid =_integrationRepository.GetPPId(ppRoute.Remark);
				ppRoute.Ppid = ppid;
				await _integrationRepository.Insert<PporderRoute>(ppRoute);
            }

			return ppRoutes.Count;


		}


		[HttpDelete("UnreleaseWO/{id}")]
        public async Task UnreleaseWorkOrder(int id)
        {
            await _integrationRepository.DeleteByWorkOrder(id);
		}

        // DELETE api/<IntegrationController>/5
        //[HttpDelete("{id}")]
        //public async Task DeletePPorder(int id) => await _integrationRepository.Delete(id);

        private async Task GeneratePPRoute(int routeId, int ppOrderId, string productNo)
        {
			//exec sp_CheckDefaultMachine @RouteID

			//create table #tmpRoute (id int IDENTITY(1,1) NOT NULL ,RouteID int NOT NULL,OperationID int NOT NULL, Subrouteid int,Sequence int NOT NULL, instruction nvarchar(max))

			//insert #tmpRoute select RouteID,OperationID,Subrouteid,Sequence,instruction 
			//	from[process].RouteOperation where routeid = @routeid and(DefaultResourceID is null or DefaultResourceID = 1) order by Sequence



			//	INSERT[integration].PPOrderRoute
			//(
			//	[PPID],
			//	[PartNo],
			//	[RouteID],
			//	[SeqNo],
			//	[CentreID],
			//	[MacCode] ,
			//	[MacType] ,
			//	[Remark] ,
			//	[MacGroup],
			//	[AttributeGroup]
			//)
			//SELECT @PPOrderID, @PartNo, a.RouteID,a.id, b.OperationName, d.EquipmentName,   CASE WHEN d.[Type] = 1 THEN 'INHOUSE' WHEN d.[Type] = 2 THEN 'QC'
			//	WHEN d.[Type] = 3 THEN 'SUBCON'  WHEN d.[Type] = 4 THEN 'AutoMated' END AS McType,b.instruction,c.DurationPer ,c.Duration
			//FROM #tmpRoute a, [process].Operation b, [process].OperationResource c, [Resource].Equipment d
			//WHERE a.operationID = b.OperationID
			//	AND b.operationID = c.operationID
			//	AND c.ResourceID = d.equipmentID
			//	AND c.isDefault = 1



			//EXEC sp_UpdatePPRouteMaterial @PPOrderID, @RouteID

			//SELECT @WOID = SalesOrderID FROM[Integration].PPOrderDetail WHERE PPID = @PPOrderID
			//SELECT @WONumber = WorkOrderNumber FROM[Order].WorkOrder WHERE WorkOrderId = @WOID



			//INSERT[integration].WorkorderMac
			//(
			//	[WOID],
			//	[PartNo],
			//	[RouteID],
			//	[SeqNo],
			//	[CentreID],
			//	[MacCode] ,
			//	[MacType] ,
			//	[Remark] ,
			//	[MacGroup],
			//	[AttributeGroup]
			//)
			//SELECT @WOID, @PartNo, a.RouteID,a.id, b.OperationName, d.EquipmentName,  CASE WHEN d.[Type] = 1 THEN 'INHOUSE' WHEN d.[Type] = 2 THEN 'QC'
			//WHEN d.[Type] = 3 THEN 'SUBCON' WHEN d.[Type] = 4 THEN 'AutoMated' END AS McType,'',c.DurationPer,c.Duration
			//FROM #tmpRoute a, [process].Operation b, [process].OperationResource c, [Resource].Equipment d
			//WHERE a.operationID = b.OperationID
			//	AND b.operationID = c.operationID
			//	AND c.ResourceID = d.equipmentID




			//	Drop table #tmpRoute

			//for model factory

			//SELECT @MaterialIds = Categroy from process.Operation
	  //		  where OperationName = @OperationName and Version = 1 and IsActive = 1

		 // print convert(char(3),@Sequence) +':' + @MaterialIds

		 // if (@MaterialIds <> 'INHOUSE')
			//	BEGIN
			//		  UPDATE Integration.PPOrderRoute SET materialIdList = @MaterialIds WHERE PPID = @PPOrderID and SeqNo = @Sequence



		}









    }
}
