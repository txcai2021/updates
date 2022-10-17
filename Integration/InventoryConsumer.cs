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
    using SIMTech.APS.PresentationModels;
    using SIMTech.APS.Customer.API.PresentationModels;
    using SIMTech.APS.Integration.API.Signalr;

    public class InventoryConsumer : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private string _exchangeName;
        private IHubContext<SignalrHub> _hub;


        public InventoryConsumer(ILoggerFactory loggerFactory, IHubContext<SignalrHub> hub)
        {
            this._logger = loggerFactory.CreateLogger<InventoryConsumer>();
            this._hub = hub;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
           
            Console.WriteLine("InitRabbitMQ for Inventory");

            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT")),                
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME")??"guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")??"guest",
                AutomaticRecoveryEnabled = true

            };

            var vHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? string.Empty;
            if (vHost != string.Empty) factory.VirtualHost = vHost;

            Console.WriteLine(factory.HostName + ":" + factory.Port + "/" + factory.VirtualHost);

            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            _queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE_INVENTORY") ?? "rps-inventory";
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
            _logger.LogInformation($"consumer received {content}");

            var inventoryPlan = new InventoryPlan();
            try
            {
                inventoryPlan = JsonConvert.DeserializeObject<InventoryPlan>(content);
            }
            catch (Exception e) { 
                Console.WriteLine(e.Message);  
            }
            
            if (inventoryPlan !=null)
            {
                _logger.LogInformation($"after received inventory plan: {inventoryPlan.FromWarehouse}/{inventoryPlan.ToWarehouse}/{inventoryPlan.SalesOrderNo}");


                //await Task.Delay(3000);
                if (inventoryPlan.RawMaterialRebalacingPlan!=null)
                {
                    int n = 1;

                    var description = "";

                    foreach (var invPlan in inventoryPlan.RawMaterialRebalacingPlan)
                    {
                        var material = new Material()
                        {
                            LocationId = 0,
                            LocationName = inventoryPlan.RequestedByFactoryID,
                            DateIn = inventoryPlan.RebalancingPlanGenerationDate,
                            Remarks = "From warehouse: " + inventoryPlan.FromWarehouse + "To warehouse: " + inventoryPlan.ToWarehouse,
                            PartId = invPlan.RawMaterialId,
                            PartNo = invPlan.RawMaterialNo,
                            CompletedQty = (decimal)invPlan.QuantityToTransfer
                        };

                        var result = ApiAddInventory(material);

                        if (result > 0)
                        {
                            n = result;
                            _logger.LogInformation("The material:" + material.PartNo + " has been added succesfully");
                             description += "Received raw material for " + material.PartNo + ", Quantity:" + material.CompletedQty + ", " + material.Remarks+ Environment.NewLine;
                        }
                    }
                    if (description != "")
                    {
                        Console.WriteLine("Sending inventory rebalancing alerts");

                        var alert = new AlertPM() { Id = n, Title = "Inventory Rebalancing", AlertType = "RawMaterial", Action = "Release", Description = description, CreatedBy = "IPS" };

                        var content1 = new System.Net.Http.StringContent(JsonConvert.SerializeObject(alert), System.Text.Encoding.UTF8, "application/json");

                        var msg = await content1.ReadAsStringAsync();

                        await _hub.Clients.All.SendAsync("SendAlerts", "Server", msg);
                    }

                }


                
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

        private int ApiAddInventory(Material material)
        {
            int inventoryId = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_INVENTORY_URL");

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.PostAsync<Material, Material>(apiBaseUrl, "RM", material).Result;
                    if (result != null) inventoryId = result.Id;
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"{ e.Message}");
                    if (e.InnerException!=null) _logger.LogInformation($"{ e.InnerException.Message}");
                }
            }

            return inventoryId;
        }

        private int ApiUpdateWorkOrderMaterial(Material material)
        {

            int result = 0;
           
            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WORKORDER_URL");

            var a = new IdNamePM() { Id = material.PartId, Name = material.PartNo, Float1 = (float)material.CompletedQty };

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                     result = HttpHelper.PostAsync<IdNamePM,int>(apiBaseUrl, "Material", a).Result;
                    
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"{ e.Message}");
                }
            }

            return result;
        }
    }
}