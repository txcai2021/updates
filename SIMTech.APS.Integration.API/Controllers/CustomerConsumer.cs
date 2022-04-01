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
    using SIMTech.APS.Customer.API.PresentationModels;
    public class CustomerConsumer : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private string _exchangeName;
     

        public CustomerConsumer(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<CustomerConsumer>();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
           
            Console.WriteLine("InitRabbitMQ for Customer");

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
            _queueName = Environment.GetEnvironmentVariable("RABBITMQ_QUEUE_CUSTOMER") ?? "rps-customer";
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


            var customerCS = JsonConvert.DeserializeObject<CustomerCS>(content);
         
            _logger.LogInformation($"after received customer: {customerCS.CustomerId}/{customerCS.CustomerCode}/{customerCS.CustomerName}");
           
            //await Task.Delay(3000);

            var result =ApiAddCustomer(customerCS);

            if (result>0) _logger.LogInformation("The customer has been added succesfully");

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

        private int ApiAddCustomer(CustomerCS customer)
        {
            int customerId = 0;

            var apiBaseUrl = Environment.GetEnvironmentVariable("RPS_CUSTOMER_URL");

            var customerPM = new CustomerPM() { Name =customer.CustomerName , Code=customer.CustomerCode, Description=customer.CustomerId.ToString (), Category ="Customer"};

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                try
                {
                    var result = HttpHelper.PostAsync<CustomerPM, CustomerPM>(apiBaseUrl,"", customerPM).Result;
                    if (result!=null) customerId = result.Id;
                }
                catch (Exception e)
                { 
                    _logger.LogInformation($"{ e.Message}"); 
                }
            }

            return customerId;
        }
    }
}