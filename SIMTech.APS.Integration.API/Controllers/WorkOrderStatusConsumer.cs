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
    public class WorkOrderStatusConsumer : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private string _exchangeName;

        public WorkOrderStatusConsumer(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<WorkOrderStatusConsumer>();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
           
            Console.WriteLine("InitRabbitMQ for work order update");

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
            _queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE_TS_WORKORDER") ?? "rps-ts-workorder";
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
            _logger.LogInformation($"work order status consumer received {content}");

            try
            {
                var workOrder = JsonConvert.DeserializeObject<WorkOrderUpdate>(content);

                _logger.LogInformation($"after received status of work order:  {workOrder.WOID}/{workOrder.WOStatus}/{workOrder.CompletedQty}");

                var result = ApiUpdateWorkOrderStatus(workOrder);


                if (result > 0) _logger.LogInformation("The work order status has been added updatead succesfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException !=null) Console.WriteLine(e.InnerException.Message);
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

        private int ApiUpdateWorkOrderStatus(WorkOrderUpdate workUpdate)
        {
            int result = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_WorkOrder_URL");

            var woStatus = new IdNamePM() { Name = workUpdate.WOID, Category = "WorkOrder", String1 = workUpdate.WOStatus, Int1 = workUpdate.CompletedQty };

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var task = HttpHelper.PostAsync<IdNamePM, int>(apiBaseUrl, "Status", woStatus);
                    if (task != null)
                    {
                        task.Wait();
                        result = task.Result;
                    }

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