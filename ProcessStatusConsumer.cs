using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace SIMTech.APS.Integration.RabbitMQ
{
    using Models;
    using SIMTech.APS.Utilities;
    using SIMTech.APS.PresentationModels;   
    using SIMTech.APS.SalesOrder.API.PresentationModels;
    using SIMTech.APS.SalesOrder.API.Enums;
    using SIMTech.APS.Customer.API.PresentationModels;
    using SIMTech.APS.WorkOrder.API.PresentationModels;
    using SIMTech.APS.WorkOrder.API.Enums;

    public class ProcessStatusConsumer : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private string _exchangeName;


        public ProcessStatusConsumer(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<ProcessStatusConsumer>();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            
            Console.WriteLine("InitRabbitMQ for process update");

            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT")),                
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME")??"guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")??"guest"

            };

            var vHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? string.Empty;
            if (vHost != string.Empty) factory.VirtualHost = vHost;

            Console.WriteLine(factory.HostName + ":" + factory.Port + "/" + factory.VirtualHost);

            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            _queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE_TS_PROCESS") ?? "rps-ts-process";
            _exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "cpps-rps";          
            _channel.QueueDeclare(_queueName, true, false, false, null);
            //_channel.QueueDeclare(_queueName, true, false, false, null);
            //_channel.QueueBind(_queueName, _exchangeName, "rps-customer", null);
            //_channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
        
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
            // received message  
            var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

            // handle the received message  
            HandleMessageAsync(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(_queueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(string content)
        {
            // we just print this message   
            _logger.LogInformation($"process consumer received {content}");
           

            try
            {
                var process = JsonConvert.DeserializeObject<ProcessUpdate>(content);

                _logger.LogInformation($"after received process update: {process.WOID}/{process.OpSeq}/{process.WOProcessStatus}");

                var result = ApiUpdateWorkOrderStatus(process);


                if (result > 0)    _logger.LogInformation("The work order status has been added updatead succesfully");

                if (process.ScrapQty >0)
                {
                    result = ApiAddSalesOrder(process);
                    if (result > 0) _logger.LogInformation("sales order for scape has been added updatead succesfully");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
            }
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
  
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        private int ApiUpdateWorkOrderStatus(ProcessUpdate processUpdate)
        {
            int result = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");           
            _logger.LogInformation("In ApiUpdateWorkOrderStatus/RPS_WORKORDER_URL:" + apiBaseUrl ?? "");

            var woStatus = new IdNamePM() { Name =processUpdate.WOID , Category = "Process", String1 =processUpdate.WOProcessStatus,  String2=processUpdate.ProcessName, Int1=processUpdate.CompletedQty,Int2=processUpdate.ScrapQty };

            _logger.LogInformation("work order number:" + woStatus.Name + ",category:" + woStatus.Category + ",status:" + woStatus.String1 + ",currentOP:" + woStatus.String2 + ",completedQty:" + woStatus.Int1.ToString() + ",scrapQty:" + woStatus.Int2.ToString());


            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    _logger.LogInformation("Before ApiUpdateWorkOrderStatus call");
                    var task = HttpHelper.PostAsync<IdNamePM, int>(apiBaseUrl, "Status", woStatus);
                    if (task!=null)
                    {
                        task.Wait();
                        result = task.Result;
                        _logger.LogInformation("After calling API, status:"+result.ToString ());
                    }
                    
                }
                catch (Exception e)
                { 
                    _logger.LogInformation($"{ e.Message}"); 
                    if (e.InnerException !=null ) _logger.LogInformation($"{ e.InnerException.Message}");
                }
            }

            return result;
        }

        private int ApiAddSalesOrder(ProcessUpdate process)
        {
            int orderId = 0;

            var customerId = 16;

            

            var wo = ApiGetWorkOrder(process.WOID);
            if (wo == null)
            {
                Console.WriteLine("Work Order cannot be found:"+process.WOID);
                return 0;
            }
            Console.WriteLine(wo.ProductId);

            var so = ApiGetSalesOrders(process.WOID);

            if (so == null || string.IsNullOrWhiteSpace(so.SalesOrderNumber))
            {
                Console.WriteLine("Sales Order cannot be found");
                return 0;
            }


            var salesOrderPM = new SalesOrderPM() { SalesOrderNumber = so.SalesOrderNumber+"-Scrap", OrderType = ESalesOrderType.ScrapOrder, PurchaseOrderNumber = so.SalesOrderNumber, Status = 0, CustomerId = customerId, OrderDate = DateTime.Today, DueDate = DateTime.MaxValue, Comment="Work Order:"+process.WOID+",Scrap Qty:" + process.ScrapQty };
            var cnt = 0;
            Console.WriteLine("OrderNo:" + salesOrderPM.SalesOrderNumber);
            var salesOrderLines = new List<SalesOrderLinePM>();
             
            
            var salesOrderLinePM = new SalesOrderLinePM() { Quantity = process.ScrapQty, ProductId = wo.ProductId, LineNumber = so.LineNumber ?? cnt, Status = ESalesOrderLineStatus.Pending, DueDate = so.DueDate ?? salesOrderPM.OrderDate.AddDays(7), UrgentFlag = so.UrgentFlag, Remarks = "Makeup for scape" };

            Console.WriteLine("LineNumber:" + (salesOrderLinePM.LineNumber ?? 0).ToString());
            Console.WriteLine("UrgentFlag:" + salesOrderLinePM.UrgentFlag.ToString());
            Console.WriteLine("DueDate:" + (salesOrderLinePM.DueDate ?? DateTime.MinValue).ToString());
            salesOrderLines.Add(salesOrderLinePM);
            
            salesOrderPM.SalesOrderLines = salesOrderLines;
            if (salesOrderPM.SalesOrderLines.Count() > 0) salesOrderPM.DueDate = salesOrderPM.SalesOrderLines.Max(x => x.DueDate ?? DateTime.MaxValue);
            Console.WriteLine("Sales Order Master DueDate:" + salesOrderPM.DueDate.ToString());

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SALESORDER_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.PostAsync<SalesOrderPM, SalesOrderPM>(apiBaseUrl, "", salesOrderPM).Result;

                    if (result != null)
                    {
                        orderId = result.Id;
                        Console.WriteLine("After call post api, sales order Id:" + orderId.ToString());
                        var workOrderId = ApiGenerateWorkOrder(result);
                        if (workOrderId > 0) Console.WriteLine("Work Order has been generated succesfully");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in calling api to add sales order");
                    _logger.LogInformation(e.Message + e.InnerException != null ? e.InnerException.Message : "");
                }
            }

            return orderId;
        }

        private SalesOrderPM ApiGetSalesOrder(int soId)
        {
            SalesOrderPM saleOrder=null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    saleOrder = HttpHelper.Get<SalesOrderPM>(apiBaseUrl, $"{soId}");                 
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"{ e.Message}");
                }
            }

            return saleOrder;

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
                    _logger.LogInformation($"{ e.Message}");
                }
            }

            return workOrder;
        }

       

        private SalesOrderLinePM ApiGetSalesOrders(string workOrderNumber)
        {
            SalesOrderLinePM saleOrder = null;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.Get<SalesOrderLinePM>(apiBaseUrl, $"SalesOrder/{workOrderNumber}");
                     
                    if (result != null) saleOrder = result;
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"{ e.Message}");
                    if (e.InnerException.Message != null) _logger.LogInformation($"{ e.InnerException.Message}" );
                }
            }

            return saleOrder;

        }

        private int ApiGenerateWorkOrder(SalesOrderPM salesOrder)
        {
            int workOrderId = 0;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");

            Console.WriteLine(salesOrder.Id.ToString());
            //var wo = new WorkOrderCreation() { SalesOrderId = salesOrder.Id, LocationId = salesOrder.LocationId, RequestedBy="CS"};
            var wo = new WorkOrderCreation() { SalesOrderId = salesOrder.Id, RequestedBy = "CS" };
            wo.Details = new List<WorkOrderCreationDetails>();
            foreach (var sol in salesOrder.SalesOrderLines)
            {
                var wod = new WorkOrderCreationDetails() { SalesOrderDetailId = sol.Id, Quantity = sol.Quantity };
                wo.Details.Add(wod);
            }


            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.PostAsync<WorkOrderCreation, int>(apiBaseUrl, "Generation", wo);
                    if (result != null) workOrderId = result.Id;
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"{ e.Message}");
                }
            }

            Console.WriteLine(workOrderId.ToString());
            return workOrderId;

        }
    }
}
