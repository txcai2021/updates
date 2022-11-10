using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using SIMTech.APS.Utilities;
using SIMTech.APS.PresentationModels;


namespace SIMTech.APS.Integration.API.Controllers
{
    using SIMTech.APS.Integration.API.Repository;
    using SIMTech.APS.Integration.API.Models;
    using SIMTech.APS.WorkOrder.API.PresentationModels;
	using SIMTech.APS.Integration.API.Signalr;


	[Route("api/[controller]")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly IPPOrderRepository _integrationRepository;
		private IHubContext<SignalrHub> _hub;

		public IntegrationController(IPPOrderRepository integrationRepository, IHubContext<SignalrHub> hub)
        {
            _integrationRepository = integrationRepository;
			_hub = hub;

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

		[HttpGet]
		[Route("Alerts/{msg}")]
		public async Task<string> SendAlerts(string  msg)
		{
			//await _integrationRepository.GetbyId(schedueId);
			await _hub.Clients.All.SendAsync("SendAlerts", "Server", msg);
			return msg;
		}


        [HttpPost("ReleaseWOs/{genRoute}")]
        public async Task GeneratePPOrders(string genRoute,[FromBody] List<WorkOrderIntegrationPM> wos)
        {

            foreach (var wo in wos)
            {
                await GeneratePPOrder1(wo, (genRoute!="F"));
            }

        }


        [HttpPost("ReleaseWO")]
        public async Task GeneratePPOrder([FromBody] WorkOrderIntegrationPM wo)
        {
            await GeneratePPOrder1(wo);

        }

		[HttpPost("PPRoute")]
		public async Task<int> GeneratePPOrderRouteForDPS([FromBody] List<PporderRoute> ppRoutes)
		{

            Console.WriteLine(ppRoutes.Count());

            if (ppRoutes.Count == 0) return 0;

            var scheduleId = ppRoutes.First().ScheduleId;

            await _integrationRepository.DeleteByScheduleId(scheduleId ?? 0);


            foreach (var ppRoute in ppRoutes)
            {
                var ppid = _integrationRepository.GetPPId(ppRoute.Remark);
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

        [HttpDelete("UnreleaseWOs/{woIds}")]
        public async Task UnreleaseWorkOrder(string woIds)
        {
            var ids= woIds.Split(",");
            foreach (var id1 in ids)
            {
                await _integrationRepository.DeleteByWorkOrder(Int32.Parse(id1));
            }
           
        }
 
        private async Task GeneratePPOrder1(WorkOrderIntegrationPM wo, bool generateRoute=true)
        {
            Console.WriteLine("Generate PP Order:"+wo.WorkOrderNumber+"/"+ generateRoute.ToString ());
            await _integrationRepository.DeleteByWorkOrderNumber(wo.WorkOrderNumber);

            var ppOrder = new Pporder()
            {
                GoldContent = "",
                Size = "",
                BasicChainType = wo.ProductFamily,
                AllocWeight = wo.Priority,
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
                SalesOrderId = wo.Id,
                SalesOrderDetId = wo.SalesOrderDetailId,
                Weight = (decimal)wo.Quantity,
                Remark = "",
                EstCompletionDate = wo.DueDate
            };

            //await _integrationRepository.DeleteByWorkOrder(wo.Id);

            //await _integrationRepository.DeleteByWorkOrderNumber(wo.WorkOrderNumber);


            ppOrder.PporderDetails.Add(ppOrderDetail);
            await _integrationRepository.Insert<Pporder>(ppOrder);


            if (generateRoute)
            {
                await Task.Delay(200);

                Console.WriteLine("GeneratePPRoute:" + wo.RouteId.ToString() + "/" + ppOrder.Id.ToString() + "/" + wo.ProductNo);

                var result = await _integrationRepository.GeneratePPRoute(wo.RouteId, ppOrder.Id, wo.ProductNo);
            }
           
        }

        private async Task GeneratePPRoute(int routeId, int ppOrderId, string productNo)
        {
            //int operationId;
            //int resourceId;
            //int count = 0;



            //select OperationID, n = 0 into #tmpDefaultMc
            //	 from[process].RouteOperation where routeid = @RouteID


              //   update #tmpDefaultMc set n=n1 from #tmpDefaultMc a,
            	 // (select b.OperationID, n1 = count(*)

              //   from #tmpDefaultMc a, [process].OperationResource b 
            	 //where a.OperationId = b.OperationID and b.IsDefault = 1

              //   group by b.OperationID) b where a.OperationId = b.OperationID





            //	DECLARE operation_cursor  CURSOR FOR
            //	select OperationId, n from #tmpDefaultMc where n!=1
            //	 --select b.OperationID, n = count(*)
            //	 --from[process].RouteOperation a, [process].OperationResource b
            //	--where routeid = @routeid and a.OperationId = b.OperationID and b.IsDefault = 1
            //	   --group by b.OperationID having count(*) != 1



            //	OPEN operation_cursor
            //	FETCH NEXT FROM operation_cursor INTO @OperationID, @Count
            //	WHILE(@@fetch_status <> -1)
            //	BEGIN

            //			print @OperationID

            //			if (@Count > 1)
            //				BEGIN

            //					set @ResourceID = 0
            //				set @ResourceID = (SELECT TOP 1 ResourceID from[process].OperationResource where IsDefault = 1 and OperationID = @OperationID)
            //				print @Count
            //				print @ResourceID

            //				if (@ResourceID > 0)
            //				BEGIN
            //					update[process].OperationResource set IsDefault = 0
            //						where OperationID = @OperationID and ResourceID!= @ResourceID
            //				END
            //			END
            //			ELSE
            //			BEGIN
            //				set @ResourceID = 0
            //				set @ResourceID = (SELECT TOP 1 ResourceID from[process].OperationResource a, [resource].Equipment b where a.OperationID = @OperationID and a.ResourceID = b.EquipmentId order by EquipmentName )
            //				print '0'
            //				print @ResourceID

            //				if (@ResourceID > 0)
            //				BEGIN
            //					update[process].OperationResource set IsDefault = 1
            //						where OperationID = @OperationID and ResourceID = @ResourceID
            //				END
            //			END




            //		FETCH NEXT FROM operation_cursor INTO @OperationID, @Count
            //	END
            //	CLOSE operation_cursor
            //	DEALLOCATE operation_cursor

            //	drop table #tmpDefaultMc

            //END







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


		private WorkOrderPM ApiGetWorkOrder(string workOrderNumber)
		{
			WorkOrderPM workOrder = null;
			var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");
			if (!string.IsNullOrWhiteSpace(apiBaseUrl))
			{
				try
				{
					workOrder = HttpHelper.Get<WorkOrderPM>(apiBaseUrl, $"WorkOrderNumber/{workOrderNumber}");
				}
				catch (Exception e)
				{
					Console.WriteLine($"{ e.Message}");
				}
			}

			return workOrder;
		}








	}
}
