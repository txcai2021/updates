using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Diagnostics;
using System.Text;
using RabbitMQ.Client;
using System.IO;

namespace SIMTech.APS.Scheduling.API.Controllers
{
    using SIMTech.APS.Scheduling.API.Repository;
    using SIMTech.APS.Scheduling.API.Mappers;
    using SIMTech.APS.Scheduling.API.Models;
    using SIMTech.APS.Scheduling.API.PresentationModels;
    using SIMTech.APS.WorkOrder.API.PresentationModels;
    using SIMTech.APS.Utilities;
    using SIMTech.APS.Models;
    using SIMTech.APS.Setting.API.Models;
    using SIMTech.APS.DPS.Engine;
    using SIMTech.APS.PresentationModels;
    using SIMTech.APS.DPS.Engine.Model;


    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScheduleDetailRepository _scheduleDetailRepository;

        public ScheduleController(IScheduleRepository SchedulingRepository, IScheduleDetailRepository SchedulingDetailRepository)
        {
            _scheduleRepository = SchedulingRepository;
            _scheduleDetailRepository = SchedulingDetailRepository;
        }

        #region APIs
        //GET: api/Role
        //[HttpGet]
        //public async Task<IEnumerable<Schedule>> GetAllSchedulings() => await _scheduleRepository.GetAll();


        //[HttpGet]
        //[Route("{id}")]
        //public Schedule GetSchedulingById(int id) => _scheduleRepository.GetById(id);

        //[HttpPost]
        //public void AddScheduling([FromBody] Schedule Scheduling) => _scheduleRepository.Insert(Scheduling);


        //[HttpPut]
        //public void UpdateScheduling([FromBody] Schedule Scheduling) => _scheduleRepository.Update(Scheduling);

        // GET: api/Route
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SchedulePM>>> GetSchedules()
        {
            //var schedules = (await _scheduleRepository.GetQueryAsync(x => x.Id > 0)).ToList();

            var schedules = (await _scheduleRepository.GetQueryAsync(x => x.Id > 0)).OrderByDescending(x=>x.CreatedOn).ToList();

            var schedulePMs = ScheduleHistoryMapper.ToPresentationModels(schedules);

            return Ok(schedulePMs);

        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SchedulePM>>> GetScheduleDetails(int id)
        {
            var schedule = (await _scheduleRepository.GetSchedules(id)).FirstOrDefault();

            //var schedule = _scheduleRepository.GetById(id);

            var schedulePM = ScheduleHistoryMapper.ToPresentationModel(schedule);

            return Ok(schedulePM);

        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<int>> AddSchedule([FromBody] Schedule schedule)
        {
            try
            {
                await _scheduleRepository.InsertAsync(schedule);
            }
            catch (Exception e)
            {
                Console.WriteLine("In AddSchedule:" + e.Message);
                if (e.InnerException !=null) Console.WriteLine("In AddSchedule:" + e.InnerException.Message );
            }
            


            return Ok(schedule.Id);
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            //var route = await _routeRepository.GetByIdAsync(id);
            var schedule = (await _scheduleRepository.GetByIdAsync(id));

            if (schedule == null)
            {
                return NotFound();
            }

            try
            {

                var scheduleDetailIds = (await _scheduleDetailRepository.GetQueryAsync(x => x.ScheduleId == id)).Select(x => x.Id).ToList();


                foreach (var scheduleDetailId in scheduleDetailIds)
                {
                    _scheduleDetailRepository.Delete(scheduleDetailId, false);
                }
                _scheduleDetailRepository.Save();

                await _scheduleRepository.DeleteAsync(schedule);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                else
                    throw e.InnerException;
            }


            return Ok(id);
        }


        [HttpGet("WorkOrders/{unitId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<WorkOrderPM>>> GetAllSchedulingWO(int unitId)
        {
            var workOrders = await ApiGetWorkOrdersForScheduling(unitId);
            return workOrders.ToList();
        }

        [HttpGet("Version")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<int>> GetMaxVersion()
        {
            //var schedule =  (await _scheduleRepository.GetQueryAsync(x => x.Id > 0)).OrderByDescending(x=>x.Id).FirstOrDefault();

            var schedule = (await _scheduleRepository.GetQueryAsync(x => x.Id > 0 && x.CreatedOn>=DateTime.Parse ("2022-01-01"))).OrderByDescending(x => x.Id).FirstOrDefault();

            return Ok(schedule==null? 0 : schedule.Version);
        }


        [HttpGet("Objectives")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IEnumerable<RulePM> GetSchedulingObjectives()
        {

            var objectives = SchedulingEngine.GetObjectiveList();

            //var objectives = new List<Entity>();

            var objectiveSetting = ApiGetOptionSettingByName("CanShowSchedule3Rules");
            if (objectiveSetting == "T")
            {
                List<string> res3 = GetPredefineRules();

                if (res3 != null && res3.Count >= 0)
                {
                    objectives = objectives.Where(r => res3.Contains(r.Description)).ToList();
                }
            }


            return RuleMapper.ToPresentationModels(objectives).AsQueryable();
        }

        [HttpGet("Rules")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IEnumerable<RulePM> GetSchedulingDispatchRules()
        {
            var rules = SchedulingEngine.GetDispatchRuleList();
            return RuleMapper.ToPresentationModels(rules).AsQueryable();
        }

        [HttpGet("OTP/{startDate}/{endDate}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<BasePM>>> GetScheduleOTP(DateTime startDate, DateTime endDate)
        {

            var a = _scheduleDetailRepository.GetQuery(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate && (x.ScheduleType == "X" || x.ScheduleType == "R")).GroupBy(x => new { x.WorkOrderId, x.WorkOrderNumber, x.WorkOrderDueDate })
                    .Select(grp => new BasePM() { Id = grp.Key.WorkOrderId, Name = grp.Key.WorkOrderNumber,  Code = "L", Value = (grp.Max(x => x.EndDate) - grp.Key.WorkOrderDueDate).ToString() }).ToList().OrderBy(x => x.Name);

            var b = _scheduleDetailRepository.GetQuery(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate && x.ScheduleType == "U").GroupBy(x => new { x.WorkOrderId, x.WorkOrderNumber, x.MaxString1 })
                   .Select(grp => new BasePM() { Id = grp.Key.WorkOrderId, Name = grp.Key.WorkOrderNumber, Description = grp.Key.MaxString1 , Value ="0", Code ="U"}).OrderBy(x => x.Name).ToList();

            foreach (var c in a.Where(x=>x.Value.Contains ("-")))
            {
                c.Code = "O";
            }

            return Ok(a.Concat(b));
        }

        [HttpGet("MachineUtilization/{startDate}/{endDate}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<BasePM>>> GetScheduleMachineUtilization(DateTime startDate, DateTime endDate)
        {

            var job1 = _scheduleDetailRepository.GetQuery(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate && x.ScheduleType != "A" && x.ScheduleType != "U").OrderBy(x => x.StartDate).FirstOrDefault();

            var job2 = _scheduleDetailRepository.GetQuery(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate && x.ScheduleType != "A" && x.ScheduleType != "U").OrderByDescending(x => x.EndDate).FirstOrDefault();

            if (job1 != null) startDate = job1.StartDate;
            if (job2 != null) endDate = job2.EndDate;


            var a = _scheduleDetailRepository.GetQuery(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate).GroupBy(x => new { x.EquipmentId, x.EquipmentName })
                  .Select(grp => new BasePM() { Id = grp.Key.EquipmentId, Name = grp.Key.EquipmentName, Value = (grp.Sum(x => x.RunTime)).ToString() }).OrderBy(x => x.Name).ToList();


            var totalTime = (endDate - startDate).TotalSeconds;

            var b = _scheduleDetailRepository.GetQuery(x => x.CreatedOn >= startDate && x.CreatedOn <= endDate && x.ScheduleType!="A" && x.ScheduleType != "U").GroupBy(x => new {x.EquipmentId, x.EquipmentName })
                   .Select(grp => new BasePM() { Id = grp.Key.EquipmentId, Name = grp.Key.EquipmentName, Value = (grp.Sum(x => x.RunTime) ).ToString() }).OrderBy(x => x.Name).ToList();


            foreach (var c in b  )
            {
                var runTime = Convert.ToSingle(c.Value);
                var d=a.Where(x => x.Id == c.Id).FirstOrDefault();
                if (d!=null)
                {
                    totalTime = Convert.ToSingle(d.Value);
                }
                c.Value = (runTime / totalTime).ToString();
                 
            }

            return Ok(b);
        }

        [HttpPost("RunSchedule")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public  async Task<IActionResult> RunScheduler([FromBody] ScheduleParameters scheduleParameters)
        {
            var locationName = scheduleParameters.locationName;
            var startDate = scheduleParameters.startDate;
            var endDate = scheduleParameters.endDate;
            var dispatchRule = scheduleParameters.dispatchRule;
            var objective = scheduleParameters.objective;
            var wip = scheduleParameters.wip;
            var command = scheduleParameters.command;

            double frozenPeriod = 0.00d;

            var frozenSetting = ApiGetOptionSettingByName("Schedule_FrozenPeriod");
            if (frozenSetting != "") double.TryParse(frozenSetting, out frozenPeriod);
            if (frozenPeriod <= 0)
            {
                frozenPeriod = 0.00d;
            }

            //if (locationName.IndexOf(' ') > 0)
            //{
            //    locationName = (char)34 + locationName + (char)34;
            //}

            int exitCode = 0;

            if (command)
            {
                string Para = string.Format("{0} {1} {2} {3} {4} {5} {6}", locationName, startDate.ToString("yyyy-MM-ddTHH:mm:ss"), endDate.ToString("yyyy-MM-ddTHH:mm:ss"), dispatchRule, objective, wip, frozenPeriod);

                string execPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (execPath.Substring(0, 4).ToUpper() == "FILE")
                    execPath = execPath.Substring(6);
                var fileName = execPath + @"\SIMTech.APS.DPS.Engine.Console.exe";
                //var fileName = execPath + @"\..\..\..\SharedLib\SIMTech.APS.DPS.Engine.Console.exe";

                exitCode = ExecuteCommand(fileName, Para, 1000000);

                //rubbatchSimlab(execPath + @"\..\..\..\SharedLib","SIMTech.APS.DPS.Engine.Console.exe");
                //rubbatchSimlab(execPath , "SIMTech.APS.DPS.Engine.Console.exe");


            }
            else
            {
                var engine = new SchedulingEngine(locationName, startDate, endDate, dispatchRule, objective, wip, frozenPeriod);
                Console.WriteLine("In calling engie, start date:" + startDate.ToString());

                exitCode = engine.Run();
            }

            var autoRun = ApiGetOptionSettingByName("AutoRun");
            Console.WriteLine("Auto Run Flag:" +autoRun + ", scheduleId:"+ exitCode.ToString());
            if (autoRun == "T" && exitCode > 0)
            {
                Console.WriteLine("Auto dispatch for schedule Id:" + exitCode.ToString ());
                await Task.Delay(1500);
                await ConfirmSchedule(exitCode);
            }

            return Ok(exitCode > 0 ? true : false);

        }

        [HttpPut("ConfirmSchedule/{scheduleId}")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmSchedule(int scheduleId)
        {

            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule.Confirmed==1)
            {
                Console.WriteLine("Confirmed already");
                return Ok(1);
            }



            schedule.Confirmed = 1;
            _scheduleRepository.Update(schedule);

          
            //generate pporderoute first
            GeneratePPOrderRoute(scheduleId);

            var updateStatus = ApiGetOptionSettingByName("UpdateWorkOrderStatus");
          

            if (updateStatus == "T")
            {
                var result = await _scheduleRepository.TrackingDispatchWOs(scheduleId);

                return Ok(scheduleId);

            }


            var rabbit_enable = Environment.GetEnvironmentVariable("RABBITMQ_ENABLE");

            if (rabbit_enable == null || rabbit_enable != "100") return Ok(scheduleId);


            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT")),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME"),
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD"),
                AutomaticRecoveryEnabled = true
            };

            var vHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? string.Empty;
            if (vHost != string.Empty) factory.VirtualHost = vHost;

            Console.WriteLine(factory.HostName + ":" + factory.Port + "/" + factory.VirtualHost);


            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "cpps-rps";

                if (exchangeName != string.Empty) channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);

                var queueNameList = Environment.GetEnvironmentVariable("RABBITMQ_QUEUES") ?? "rps-schedule";
                var queueNames = queueNameList.Split(",");

                foreach (var queueName in queueNames)
                {
                    channel.QueueDeclare(queue: queueName,
                                       durable: true,
                                       exclusive: false,
                                       autoDelete: false,
                                       arguments: null);
                    channel.QueueBind(queueName, exchangeName, "rps-schedule", null);
                }

                var schedulePM = ScheduleHistoryMapper.ToPresentationModel(schedule);
                schedulePM.ScheduleDetails = new List<ScheduleDetailPM>();
                var body = Encoding.Default.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(schedulePM));

                //var body = Encoding.Default.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(schedule));

                Console.WriteLine("befor publish:"+schedule.Id.ToString ());
                channel.BasicPublish(exchange: exchangeName,
                                        routingKey: "rps-schedule",
                                        basicProperties: null,
                                        body: body);
                Console.WriteLine("after publish:" + schedule.Id.ToString());

                var woIds = (await _scheduleDetailRepository.GetQueryAsync(x => x.ScheduleId == schedule.Id && x.ScheduleType == "X")).Select(x => x.WorkOrderId).Distinct ().ToArray();

               
                ApiUpdateWorkOrdersStatus(woIds,"Dispatched");
            }

          
            return Ok(scheduleId);
        }

        [HttpGet("WIP")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<List<WIP>> GetWIPs()
        {

            var wipRequest = new WIPRequest() { WOID="", WOStatus=new List<string>()};
            wipRequest.WOStatus.Add("Processing");
            wipRequest.WOStatus.Add("Queuing");
            wipRequest.WOStatus.Add("Pending");
            

            var wips = ApiGetWIP(wipRequest);

            return Ok(wips);
        }


        [HttpPost("RunSimlab/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<bool> RunDiSimLab(string filename)
        {
            var a =_scheduleRepository.GetQuery(x => x.Id > 0).OrderByDescending(x => x.Id).FirstOrDefault();

            if (a == null) return Ok(false);

            var scheduleDetails =_scheduleDetailRepository.GetQuery(x => x.ScheduleId == a.Id && x.ScheduleType == "x").ToList();

            //workorder, partname, begindate,quantity, duedate

            var schedules = new List<ScheduleCPPS24>();

            var woNumbers = scheduleDetails.OrderBy(x=>x.WorkOrderNumber).Select(x => x.WorkOrderNumber).Distinct().ToList();

            foreach (var wo in woNumbers)
            {
                var scheduleCPPS = new ScheduleCPPS24() {WorkOrderNumber =wo };

                var schedule= scheduleDetails.Where(x => x.WorkOrderNumber == wo).OrderBy(x => x.StartDate).FirstOrDefault();

              

                if (schedule!=null)
                {
                    scheduleCPPS.PartNumber = schedule.ItemName;
                    scheduleCPPS.BeginDate = schedule.StartDate;
                    //var qty = scheduleDetails.Where(x => x.WorkOrderNumber == wo  ).Sum(x => x.Quantity);
                    scheduleCPPS.Quantity = schedule.Quantity;
                    scheduleCPPS.DueDate = schedule.WorkOrderDueDate; 
                }

                schedules.Add(scheduleCPPS);
            }


           
            var result = ApiGenerateGAPFiles(schedules);

            if (result>0)
            {
                //result = ApiRunGAP(1);
                result = ApiRunGAP(2);
                return Ok(result == 0);
            }

            return Ok(false);

        }

        [HttpPost("RunGAP/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ProductLotSizeRecommendation> RunGAP(string fileName)
        {
           
            var pLotSize = ApiGetGAPProductLotSize();

            if (fileName == "GAPSMOM" && pLotSize.TryLotSize>0)
            {
                var productLotsizes = new List<BasePM>();

                foreach (var partName in pLotSize.LotSizeReport.Select (x=>x.PartName ).Distinct ().ToList())
                {
                    Console.WriteLine(partName + ":" + pLotSize.TryLotSize);
                    var a = new BasePM() { Name = partName, Value = pLotSize.TryLotSize.ToString() };

                    productLotsizes.Add(a);
                }


                ApiUpdateProductLotSize(productLotsizes);
            }

                

            return Ok(pLotSize);
        }

        [HttpGet("DeleteDemoData/{flag}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteDemoData(int flag)
        {
            var result = await _scheduleRepository.DeleteDemoData(flag);

            return Ok(result);

        }

        //[HttpGet("DeleteDemoData")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public async Task<ActionResult> DeleteDemoData()
        //{
        //    var result = await _scheduleRepository.DeleteDemoData(0);

        //    return Ok(result);

        //}


        #endregion
        #region Private methods

        private int ExecuteCommand(string command, string arguments, int timeout)
        {
           
            // Prepare the process to run
            ProcessStartInfo start = new ProcessStartInfo();
            // Enter in the command line arguments, everything you would enter after the executable name itself           
            start.Arguments = arguments;
            // Enter the executable to run, including the complete path
            start.FileName = command;
            // Do you want to show a console window?
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = true;

            int exitCode;

            
            // Run the external process & wait for it to finish
            using (var proc = System.Diagnostics.Process.Start(start))
            {
                proc.WaitForExit(timeout);

                // Retrieve the app's exit code
                exitCode = proc.ExitCode;
            }

            return exitCode;
        }

        private List<string> GetPredefineRules()
        {
            List<string> res = new List<string>();
            
            var codeType =ApiGetCodeByName("ScheduleRules");

            if (codeType!=null)
            {                  
                var codedetailist = codeType.CodeDetails;
                if (codedetailist != null && codedetailist.Count > 0)
                {
                    foreach (var objrule in codedetailist)
                    {
                        if (!string.IsNullOrEmpty(objrule.Description))
                        {
                            res.Add(objrule.Description);
                        }

                    }
                }
            }

            if (res.Count < 3)              
            {
                res.Add("Minimize Late Jobs");
                res.Add("Minimize Average Flow Time");
                res.Add("Maximize Utilization");
            }
            return res;
        }


        private IList<ReportSchedulePM> GetWP24GAP()
        {
            int scheduleid = 0;

            var schedule = _scheduleRepository.GetQuery(r => r.Id > 0).OrderByDescending(d => d.Id).FirstOrDefault();
            if (schedule!=null)
            {
                scheduleid = schedule.Id;
            }

            var scheduleReport = new DPSReport1()
            {
                LoginId = string.Empty,
                WorkOrderNumber = string.Empty,
                MachineName = string.Empty,
                ReportTypeId = 15,
                ScheduleId = scheduleid,
                SearchTypeId = 0,
                StartDate = DateTime.Now.AddHours(-1),
                EndDate = DateTime.Now.AddMonths(2),
                SearchCustomer = string.Empty,
                SearchPO = string.Empty,
                SearchWO = string.Empty,
                Date1 = DateTime.Now,
                Date2 = DateTime.Now,
                Search1 = "WOREPORT",
                Search2 = string.Empty,
                SearchPart = string.Empty
            };

            //var resultDB = _inventoryRepository.ExecuteStoredProcedure(scheduleReport);
            var resultDB = new List<DPSReport>();
            IList<ReportSchedulePM> reportList = ReportScheduleDetailsMapper.ToPresentationModels(resultDB).ToList();
            IList<ReportSchedulePM> resList = reportList;
            return resList;
        }




        private void UpdateDsimLabexcel(IList<ReportSchedulePM> reslist)
        {
            if (reslist == null) return;
            if (reslist.Count == 0) return;
            string fileName = string.Empty;
            string execPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (execPath.Substring(0, 4).ToUpper() == "FILE")
                execPath = execPath.Substring(6);
            fileName = execPath + @"\..\UploadedFiles\" + "wafer_start_schedule.csv";
            string[] saLines = new string[reslist.Count + 2];
            StringBuilder sbline = new StringBuilder();
            sbline.Append("FAB");
            sbline.Append(",");
            sbline.Append("LOT_ID");
            sbline.Append(",");
            sbline.Append("LOT_TYPE");
            sbline.Append(",");
            sbline.Append("PRODUCT");
            sbline.Append(",");
            sbline.Append("START_TIME");
            sbline.Append(",");
            sbline.Append("LOT_SIZE");
            sbline.Append(",");
            sbline.Append("DUE_DATE");
            sbline.Append(",");
            sbline.Append("PRIORITY");
            saLines[0] = sbline.ToString();
            int i = 1;
            foreach (var obj in reslist)
            {
                sbline = new StringBuilder();
                sbline.Append("SIMTECH");
                sbline.Append(",");
                sbline.Append("LOT_");
                sbline.Append(obj.WorkOrderNumber);
                sbline.Append(",");
                sbline.Append("Y");
                sbline.Append(",");
                sbline.Append(obj.PartNumber);
                sbline.Append(",");
                if (obj.BeginDate > DateTime.Now.AddHours(1) && (obj.BeginDate.Hour == 12 || obj.BeginDate.Hour == 0))
                {
                    sbline.Append(obj.BeginDate.ToString("dd/MM/yyyy"));
                    sbline.Append(" 00:00:00");
                }
                else
                {
                    sbline.Append(obj.BeginDate.ToString("dd/MM/yyyy hh:mm:ss"));
                }

                sbline.Append(",");
                sbline.Append(obj.Quantity.ToString());
                sbline.Append(",");


                sbline.Append((obj.Date1 ?? DateTime.Now).ToString("dd/MM/yyyy"));
                sbline.Append(" 00:00:00");

                sbline.Append(",");
                sbline.Append("0");
                saLines[i] = sbline.ToString();
                i++;

            }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                foreach (String line in saLines)
                    writer.WriteLine(line);
            }

        }

        private void WriteDSIMConfigFile(IList<ReportSchedulePM> reslist, bool issamefolder)
        {
            int noofdays = 40;
            if (reslist == null) return;
            if (reslist.Count == 0) return;
            string fileName = string.Empty;
            string execPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (execPath.Substring(0, 4).ToUpper() == "FILE")
                execPath = execPath.Substring(6);
            if (issamefolder)
            {
                fileName = execPath + @"\..\UploadedFiles\" + "model_config.csv";
            }
            else
            {
                fileName = execPath + @"\..\UploadedFiles\dsimlab\input\" + "model_config.csv";
            }


            DateTime dtMin = DateTime.Now;
            if (reslist != null & reslist.Count > 0)
            {
                dtMin = reslist.Min(r => r.BeginDate);
            }
            string[] saLines = new string[15];
            StringBuilder sbline = new StringBuilder();
            sbline.Append("PARAMETER_NAME");
            sbline.Append(",");
            sbline.Append("PARAMETER_VALUE");

            saLines[0] = sbline.ToString();
            sbline = new StringBuilder();
            sbline.Append("DISPATCH_WIP_THRESHOLD,100000");
            saLines[1] = sbline.ToString();

            sbline = new StringBuilder();
            sbline.Append("DISPATCH_INTERVAL,0.25");
            saLines[2] = sbline.ToString();


            sbline = new StringBuilder();
            sbline.Append("START_TIME,");
            sbline.Append(dtMin.ToString("dd/MM/yyyy"));
            sbline.Append(" 00:00:00");
            saLines[3] = sbline.ToString();


            sbline = new StringBuilder();
            sbline.Append("END_TIME,");
            sbline.Append(dtMin.AddDays(noofdays).ToString("dd/MM/yyyy"));
            sbline.Append(" 00:00:00");
            saLines[4] = sbline.ToString();

            sbline = new StringBuilder();
            sbline.Append("REPORT_INTERVAL_HOUR,8");
            saLines[5] = sbline.ToString();

            sbline = new StringBuilder();
            sbline.Append("RANDOM_SEED,134");
            saLines[6] = sbline.ToString();


            sbline = new StringBuilder();
            sbline.Append("START_PRINT_TRACE,");
            sbline.Append(dtMin.ToString("dd/MM/yyyy"));
            sbline.Append(" 00:00:00");
            saLines[7] = sbline.ToString();

            sbline = new StringBuilder();
            sbline.Append("END_PRINT_TRACE,");
            sbline.Append(dtMin.AddDays(noofdays).ToString("dd/MM/yyyy"));
            sbline.Append(" 00:00:00");
            saLines[8] = sbline.ToString();

            sbline = new StringBuilder();
            sbline.Append("REPORT_START_TIME,");
            sbline.Append(new DateTime(dtMin.Year, dtMin.Month, dtMin.Day, 0, 0, 0).ToString("dd/MM/yyyy"));
            sbline.Append(" 00:00:00");
            saLines[9] = sbline.ToString();

            sbline = new StringBuilder();
            sbline.Append("STATISTICS_RESET,1");
            saLines[10] = sbline.ToString();

            sbline = new StringBuilder();
            sbline.Append("ALTERNATIVE_STEP_BY_RATE,0");
            saLines[11] = sbline.ToString();

            sbline = new StringBuilder();
            sbline.Append("DISPATCH_DLL,DispatchPolicy.dll");
            saLines[12] = sbline.ToString();

            sbline = new StringBuilder();
            sbline.Append("RESTART_SCRAP_LOT,0");
            saLines[13] = sbline.ToString();

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                foreach (String line in saLines)
                    writer.WriteLine(line);
            }

        }

        static void rubbatchSimlab(string sFolder, string batchfile)
        {
            System.Diagnostics.Process proc = null;
            try
            {

                proc = new System.Diagnostics.Process();
                proc.StartInfo.WorkingDirectory = sFolder;
                proc.StartInfo.FileName = batchfile;
                proc.StartInfo.CreateNoWindow = false;
                proc.Start();
                proc.WaitForExit();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace.ToString());
            }
        }

        
        private void RecommendReleaseDates()
        {
            string fileName = string.Empty;
            string execPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (execPath.Substring(0, 4).ToUpper() == "FILE")
                execPath = execPath.Substring(6);
            fileName = execPath + @"\..\UploadedFiles\IMPROVED_WAFER_START_SCHEDULE\" + "wafer_start_schedule.csv";
            List<string> listWOno = new List<string>();
            List<string> listBDates = new List<string>();
            using (var reader = new StreamReader(fileName))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (values != null && values.Count() > 4)
                    {
                        listWOno.Add(values[1]);
                        listBDates.Add(values[4]);
                    }

                }
            }

            if (listWOno != null && listWOno.Count > 0
                && listBDates != null && listBDates.Count > 0)
            {
                DateTime dt = DateTime.Now;
                string trimChars = "LOT_";
                for (int i = 0; i < listWOno.Count; i++)
                {
                    DateTime.TryParse(listBDates[i], out dt);
                    if (dt.Year > 2000)
                    {
                        ApiUpdateReleaseDate(listWOno[i].TrimStart(trimChars.ToCharArray()), dt);
                    }

                }

            }
        }


        #endregion


        #region  API call of other services

        private void GeneratePPOrderRoute(int scheduleId)
        {
           
            var scheduledWOs =_scheduleDetailRepository.GetQuery(x => x.ScheduleId == scheduleId && x.ScheduleType == "X").ToList();

            var jobs =scheduledWOs.GroupBy(x => new { x.WorkOrderNumber, x.OperationName, x.Sequence, x.EquipmentName, x.ItemName, x.RouteId })
                .Select(grp => new {grp.Key,duration =grp.Sum(x=>x.RunTime ), quantity = grp.Sum(x => x.Quantity), startDate = grp.Min(x => x.StartDate), endDate = grp.Max(x => x.EndDate) }).ToList ();

            var pporderRoutes = new List<PporderRoute>();
            foreach (var job in jobs)
            {
                var ppOrderRoute = new PporderRoute() {
                    Ppid = 0,
                    PartNo = job.Key.ItemName, 
                    RouteId = job.Key.RouteId??0,
                    SeqNo = job.Key.Sequence??0, 
                    CentreId = job.Key.OperationName, 
                    MacCode = job.Key.EquipmentName, 
                    MacType="INHOUSE", 
                    Remark= job.Key.WorkOrderNumber, 
                    StartDate =job.startDate, 
                    EndDate = job.endDate, 
                    Duration = job.duration,
                    Qty = job.quantity, 
                    ScheduleId = scheduleId, 
                    MaterialIdList="" 
                };

                pporderRoutes.Add(ppOrderRoute);


                //   update #tmpSchedule set MacType = CASE WHEN b.[Type] = 1 THEN 'INHOUSE' WHEN b.[Type] = 2 THEN 'QC' WHEN b.[Type] = 3 THEN 'SUBCON' WHEN b.[Type] = 3 THEN 'SUBCON'WHEN b.[Type] = 4 THEN 'AutoMated' END
                // from #tmpSchedule a, [Resource].Equipment b
                //where a.EquipmentName = b.EquipmentName

                //   update #tmpSchedule set remarks = b.Instruction
                // from #tmpSchedule a, [Process].[RouteOperation] b
                //where a.[Sequence] = b.[Sequence] and a.RouteID = b.RouteId


          //      SELECT @MaterialIds = Categroy from process.Operation          
          //where OperationName = @OperationName and Version = 1 and IsActive = 1
          //print convert(char(3),@Sequence) +':' + @MaterialIds


          //if (@MaterialIds <> 'INHOUSE')
          //          BEGIN
          //                UPDATE Integration.PPOrderRoute SET materialIdList = @MaterialIds WHERE PPID = @PPOrderID and SeqNo = @Sequence



            }

            ApiInsertPPOrderRoute(pporderRoutes);

        }

        private string ApiGetToken()
        {
            string token = "";
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_LOGIN_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    token = HttpHelper.Get<String>(apiBaseUrl, "");
                }
                catch { }
            }

            return token;
        }

        private string ApiGetOptionSettingByName(string optionName)
        {
            Option option = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SETTING_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    option = HttpHelper.Get<Option>(apiBaseUrl, $"Name/{optionName}");
                }
                catch { }
            }

            return option == null ? "" : option.DefaultSetting;
        }

        private CodeType ApiGetCodeByName(string codeName)
        {
            CodeType codeType = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CODE_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    codeType = HttpHelper.Get<CodeType>(apiBaseUrl, $"Name/{codeName}");
                }
                catch { }
            }

            return codeType;
        }

        private async Task<IEnumerable<WorkOrderPM>> ApiGetWorkOrdersForScheduling(int locationId)
        {
            IEnumerable<WorkOrderPM> workOrders = new List<WorkOrderPM>();
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");
           
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                if (apiBaseUrl.Substring(apiBaseUrl.Length - 1) != "/") apiBaseUrl += "/";

                try
                {
                    workOrders = await HttpHelper.GetAsync<List<WorkOrderPM>>(apiBaseUrl, $"Schedule/{locationId}");                 
                }
                catch { }
            }


            return workOrders;
        }

        private int ApiRunGAP(int value)
        {
            int exitCode = 0;
           
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_GAP_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {                    
                        exitCode = HttpHelper.Get<int>(apiBaseUrl, $"{value}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException!=null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return exitCode;
        }

        private int ApiGenerateGAPFiles(List<ScheduleCPPS24> schedules)
        {
            int exitCode = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_GAP_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var task = HttpHelper.PostAsync<List<ScheduleCPPS24>>(apiBaseUrl,"", schedules);
                    task.Wait();
                    exitCode = task.Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return exitCode;
        }

        private ProductLotSizeRecommendation ApiGetGAPProductLotSize()
        {
            var lotSize = new ProductLotSizeRecommendation();

            Console.WriteLine("1.In ApiGetGAPProductLotSize");

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_GAP_URL");

            Console.WriteLine("2:" + apiBaseUrl);

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    Console.WriteLine("3.Before calling api");
                    lotSize = HttpHelper.Get<ProductLotSizeRecommendation>(apiBaseUrl, "");
                    Console.WriteLine("4.After calling api:" + lotSize.LotSizeReport.Count());
                }
                catch (Exception e)
                {
                    Console.WriteLine("5." + e.Message);
                    if (e.InnerException != null) Console.WriteLine("6." + e.InnerException.Message);
                }
            }

            return lotSize;
        }

        private List<ProductLotSize> ApiGetGAPProductLotSize1()
        {
            List<ProductLotSize> lotSize = null;

            Console.WriteLine("1.In ApiGetGAPProductLotSize");

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_GAP_URL");

            Console.WriteLine("2:" + apiBaseUrl);

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    Console.WriteLine("3.Before calling api");
                    lotSize = HttpHelper.Get<List<ProductLotSize>>(apiBaseUrl, "");
                    Console.WriteLine("4.After calling api:" + lotSize.Count());
                }
                catch (Exception e)
                {
                    Console.WriteLine("5." + e.Message);
                    if (e.InnerException != null) Console.WriteLine("6." + e.InnerException.Message);
                }
            }

            return lotSize ?? new List<ProductLotSize>();
        }

        private int ApiUpdateProductLotSize(List<BasePM> productLotSizes)
        {
            

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_PRODUCT_URL");
            Console.WriteLine("In ApiUpdateProductLotSize:" + apiBaseUrl);

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    HttpHelper.PutAsync<List<BasePM>>(apiBaseUrl, "LotSize",productLotSizes);
                    Console.WriteLine("After update lotsize");
                    //Task.WaitAll();

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return 1;
        }

        private List<WIP> ApiGetWIP(WIPRequest wipRequest)
        {
            List<WIP> WIPs = null;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_TS_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var task = HttpHelper.PostAsync<WIPRequest, WIPResponse>(apiBaseUrl, "get/getschedule", wipRequest);
                    task.Wait ();                
                    var result = task.Result;
                    Console.WriteLine(result.Message);
                    Console.WriteLine(result.StatusCode);
                    if (result.Data != null) WIPs = result.Data;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return WIPs??new List<WIP>() ;
        }

        private Scrap ApiGetScrapQty(ScrapRequest scrapRequest)
        {
            Scrap scrap = null;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_GAP_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var task = HttpHelper.PostAsync<ScrapRequest, Scrap>(apiBaseUrl, "get/getscrap", scrapRequest);
                    task.Wait();
                    scrap = task.Result;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return scrap;
        }

        //private Scrap ApiMachineEST(ScrapRequest scrapRequest)
        //{
        //    Scrap scrap = null;

        //    var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_GAP_URL");

        //    if (!string.IsNullOrWhiteSpace(apiBaseUrl))
        //    {
        //        try
        //        {
        //            var task = HttpHelper.PostAsync<ScrapRequest, Scrap>(apiBaseUrl, "get/getscrap", scrapRequest);
        //            task.Wait();
        //            scrap = task.Result;

        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.Message);
        //            if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
        //        }
        //    }

        //    return scrap;
        //}

        private int ApiInsertPPOrderRoute(List<PporderRoute> ppRoutes)
        {
            int result = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INTEGRATION_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var task =HttpHelper.PostAsync<List<PporderRoute>>(apiBaseUrl, "PPRoute", ppRoutes);
                    task.Wait();
                    result = task.Result;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return result;
        }

        private int ApiUpdateWorkOrdersStatus(int[] woIds, string status)
        {
            int result = 0;
          
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl) && woIds.Count()>0)
            {
                try
                {
                    var task = HttpHelper.PostAsync<int[]>(apiBaseUrl, $"Status/{status}", woIds);
                    task.Wait();
                    result = task.Result;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
                }
            }

            return result;

        }


        private void ApiUpdateReleaseDate(string sWoNO, DateTime dtRelease)
        {
            //var workOrders = _exceptionManager.Process(() => _orderRepository.GetQuery<WorkOrder>(wo => wo.WorkOrderNumber == sWoNO), "ExceptionShielding");
            //if (workOrders == null) return;
            //foreach (var workOrder in workOrders)
            //{
            //    workOrder.Date1 = dtRelease;
            //    if (workOrder.Date2 == null)
            //    {
            //        workOrder.Date2 = workOrder.Date1;
            //    }

            //}
            //var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SALESORDER_URL");

            //if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            //{
            //    try
            //    {
            //        HttpHelper.PutAsync<SalesOrderLinePM>(apiBaseUrl, $"DetailId/{sol.Id}", sol);
            //    }
            //    catch { }
            //}
        }
        #endregion







    }
}
