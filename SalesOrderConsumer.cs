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
using Microsoft.AspNetCore.SignalR;


namespace SIMTech.APS.Integration.RabbitMQ
{
    using Models;
    using SIMTech.APS.Utilities;
    using SIMTech.APS.SalesOrder.API.PresentationModels;
    using SIMTech.APS.SalesOrder.API.Enums;
    using SIMTech.APS.Customer.API.PresentationModels;
    using SIMTech.APS.WorkOrder.API.PresentationModels;
    using SIMTech.APS.WorkOrder.API.Enums;
    using SIMTech.APS.Integration.API.Signalr;

    public class SalesOrderConsumer : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private string _exchangeName;
        private IHubContext<SignalrHub> _hub;


        public SalesOrderConsumer(ILoggerFactory loggerFactory, IHubContext<SignalrHub> hub)
        {
            this._logger = loggerFactory.CreateLogger<SalesOrderConsumer>();
            this._hub = hub;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
           
            Console.WriteLine("InitRabbitMQ for sales order");


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
            _queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE_SALESORDER") ?? "rps-salesorder";
            _exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "cpps-rps";          
            _channel.QueueDeclare(_queueName, true, false, false, null);
            //_channel.QueueDeclare(_queueName, true, false, false, null);
            //_channel.QueueBind(_queueName, _exchangeName, "rps-customer", null);
            //_channel.BasicQos(0, 1, false);

            ApiGetCustomerByName("001");
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
            _logger.LogInformation($"sales order consumer received {content}");

            
            try
            {
                var salesOrderCS = JsonConvert.DeserializeObject<SalesOrderCS>(content);

                if (salesOrderCS!=null)
                {
                    _logger.LogInformation($"after received salesorder: {salesOrderCS.OrderId}/{salesOrderCS.OrderNo}/{salesOrderCS.CustomerId}/{salesOrderCS.CustomerName}");

                    var result = ApiAddSalesOrder(salesOrderCS);

                    if (result > 0)
                    {
                        _logger.LogInformation("The salesorder has been added succesfully");

                        if (salesOrderCS.OrderItems.Where (x=>x.Priority == "VERYURGENT").Count() >0)
                        {
                            Console.WriteLine("Sending urgent orders alerts");

                            var description = "Very Urgent Order:" + salesOrderCS.OrderNo + "is received from customer: " + salesOrderCS.CustomerName;

                            var alert = new AlertPM() { Id = 0, Title = "Very Urgent Order", AlertType = "UrgentOrder", Action = "Release", Description = description, CreatedBy = "OrderApp" };

                            var content1 = new System.Net.Http.StringContent(JsonConvert.SerializeObject(alert), System.Text.Encoding.UTF8, "application/json");

                            var msg = await content1.ReadAsStringAsync();

                            await _hub.Clients.All.SendAsync("SendAlerts", "Server", msg);
                        }
                       
                    }
                       
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in DeserializeObject:"+e.Message );
                if (e.InnerException !=null) Console.WriteLine(e.InnerException.Message );
            }

            //try
            //{
            //    await _hub.Clients.All.SendAsync("SendAlerts", "Server", content);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Error in sending sales order alerts");
            //    Console.WriteLine(e.Message);
            //    if (e.InnerException.Message != null) Console.WriteLine(e.InnerException.Message);
            //}


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

        private int ApiAddSalesOrder(SalesOrderCS salesOrder)
        {
            int orderId = 0;

            var customerId = ApiGetCustomerByName(salesOrder.CustomerName);

            if (customerId == 0)
            {
                Console.WriteLine("Invalid customer id!");
                return 0;
            }
            //var customerId = salesOrder.CustomerId;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_SALESORDER_URL");

            var salesOrderPM = new SalesOrderPM() { SalesOrderNumber = salesOrder.OrderNo , OrderType=ESalesOrderType.StandardOrder, PurchaseOrderNumber=salesOrder.OrderNo,Status =0, CustomerId =customerId ,OrderDate =DateTime.Today , DueDate =DateTime.MaxValue };
            var cnt = 0;
            Console.WriteLine("OrderNo:" + salesOrderPM.SalesOrderNumber);
            var salesOrderLines = new List<SalesOrderLinePM>();
            foreach (var lineItem in salesOrder.OrderItems)
            {
                cnt++;
                //var salesOrderLinePM = new SalesOrderLinePM() { Quantity = lineItem.Quantity, ProductId = lineItem.ProductId, LineNumber = cnt, Status = ESalesOrderLineStatus.Pending, DueDate = salesOrderPM.OrderDate.AddDays(7), UrgentFlag = WorkOrder.API.Enums.EWorkOrderUrgentFlag.Urgent, Remarks = "Color: " + lineItem.Color + "Scent: " + lineItem.Scent };
                var urgentFlag =lineItem.Priority.ToUpper() == "VERYURGENT" ? EWorkOrderUrgentFlag.VeryUrgent : (lineItem.Priority.ToUpper() == "URGENT" ? EWorkOrderUrgentFlag.Urgent : EWorkOrderUrgentFlag.Standard);
                var salesOrderLinePM = new SalesOrderLinePM() { Quantity = lineItem.Quantity, ProductId = lineItem.ProductId, LineNumber = lineItem.LineNumber ?? cnt, Status = ESalesOrderLineStatus.Pending, DueDate = lineItem.DueDate??salesOrderPM.OrderDate.AddDays(7), UrgentFlag = urgentFlag, Remarks = "Color: " + lineItem.Color + "Scent: " + lineItem.Scent };

                Console.WriteLine("LineNumber:" + (salesOrderLinePM.LineNumber??0).ToString());
                Console.WriteLine("UrgentFlag:" + salesOrderLinePM.UrgentFlag.ToString());
                Console.WriteLine("DueDate:" + (salesOrderLinePM.DueDate ?? DateTime.MinValue).ToString());
                salesOrderLines.Add(salesOrderLinePM);               
            }
            salesOrderPM.SalesOrderLines = salesOrderLines;
            if (salesOrderPM.SalesOrderLines.Count()>0) salesOrderPM.DueDate = salesOrderPM.SalesOrderLines.Max(x => x.DueDate??DateTime.MaxValue);
            Console.WriteLine("Sales Order Master DueDate:" + salesOrderPM.DueDate.ToString());


            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.PostAsync<SalesOrderPM, SalesOrderPM>(apiBaseUrl,"", salesOrderPM).Result;
                    
                    if (result != null)
                    {
                        orderId = result.Id;
                        Console.WriteLine("After call post api, sales order Id:"+orderId.ToString());
                        var workOrderId = ApiGenerateWorkOrder(result);
                        if (workOrderId > 0) Console.WriteLine("Work Order has been generated succesfully");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in calling api to add sales order");
                    _logger.LogInformation( e.Message+e.InnerException!=null? e.InnerException.Message:""); 
                }
            }

            return orderId;
        }

        private int ApiGetCustomerByName(string  customerName)
        {
            int customerId = 0;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.Get<CustomerPM>(apiBaseUrl, $"Name/{customerName}");
                    if (result != null) customerId = result.Id;
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"{ e.Message}");
                }
            }

            Console.WriteLine(customerId.ToString ());
            return customerId;

        }

        private int ApiGenerateWorkOrder(SalesOrderPM salesOrder)
        {
            int workOrderId = 0;
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");

            Console.WriteLine(salesOrder.Id.ToString ());
            //var wo = new WorkOrderCreation() { SalesOrderId = salesOrder.Id, LocationId = salesOrder.LocationId, RequestedBy="CS"};
            var wo = new WorkOrderCreation() { SalesOrderId = salesOrder.Id, RequestedBy = "CS" };
            wo.Details = new List<WorkOrderCreationDetails>();
            foreach (var sol in salesOrder.SalesOrderLines)
            {
                var wod = new WorkOrderCreationDetails() {SalesOrderDetailId = sol.Id, Quantity= sol.Quantity };
                wo.Details.Add(wod);
            }


            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.PostAsync<WorkOrderCreation,int>(apiBaseUrl, "Generation",wo);
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