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
    

    public class MachineBlockoutConsumer : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private string _exchangeName;
       

        public MachineBlockoutConsumer(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<MachineBlockoutConsumer>();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            
            Console.WriteLine("InitRabbitMQ for MachineBlockout");

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
            _queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE_MACHINE") ?? "rps-machineblockout";
            _exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "cpps-rps";          
            _channel.QueueDeclare(_queueName, true, false, false, null);
            //_channel.QueueDeclare(_queueName, true, false, false, null);
            //_channel.QueueBind(_queueName, _exchangeName, "rps-MachineBlockout", null);
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
            _logger.LogInformation($"machine blockout consumer received {content}");

            try
            {
                var machineBlockout = JsonConvert.DeserializeObject<MachineBlockOut>(content);

                _logger.LogInformation($"after received MachineBlockout: {machineBlockout.machine_id}/{machineBlockout.machine_name}/{machineBlockout.time_of_occurs}//{machineBlockout.end_time}");

                //await Task.Delay(3000);

                var result = ApiAddMachineBlockout(machineBlockout);

                if (result > 0) _logger.LogInformation("The MachineBlockout has been added succesfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException.Message !=null ) Console.WriteLine(e.InnerException.Message);
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

        private int ApiAddMachineBlockout(MachineBlockOut machineBlockout)
        {
            int machineBlockoutId = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_RESOURCE_URL");

            var machineBlockoutPM = new MachineBlockOutPM() { EquipmentID = machineBlockout.machine_id, EquipmentName = machineBlockout.machine_name, StartDate = machineBlockout.start_time ?? DateTime.Now, EndDate = machineBlockout.end_time ?? DateTime.Now.AddHours(1), Remarks = machineBlockout.reasons_for_failure, BlockOutType = "PM", CreatedBy="PDM"};
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var task = HttpHelper.PostAsync<MachineBlockOutPM, MachineBlockOutPM>(apiBaseUrl,"Blockout", machineBlockoutPM);

                    task.Wait();

                    if (task.Result != null) machineBlockoutId = task.Result.EquipmentBlockOutID;
                }
                catch (Exception e)
                { 
                    _logger.LogInformation($"{ e.Message}");
                    if (e.InnerException!=null) _logger.LogInformation($"{ e.InnerException.Message}");
                }
            }

            return machineBlockoutId;
        }
    }
}