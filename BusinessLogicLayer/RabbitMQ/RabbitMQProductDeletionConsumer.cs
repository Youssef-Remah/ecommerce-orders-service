using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQProductDeletionConsumer : IRabbitMQProductDeletionConsumer, IDisposable
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private ConnectionFactory connectionFactory;
        private readonly ILogger<RabbitMQProductDeletionConsumer> _logger;

        public RabbitMQProductDeletionConsumer(IConfiguration configuration, ILogger<RabbitMQProductDeletionConsumer> logger)
        {
            _configuration = configuration;

            string hostname = _configuration["RabbitMQ_HostName"]!;
            string username = _configuration["RabbitMQ_UserName"]!;
            string password = _configuration["RabbitMQ_Password"]!;
            string port = _configuration["RabbitMQ_Port"]!;

            connectionFactory = new ConnectionFactory()
            {
                HostName = hostname,
                UserName = username,
                Password = password,
                Port = Convert.ToInt32(port)
            };

            _logger = logger;
        }
        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }

        public void Consume()
        {
            string routingKey = "product.delete";
            string queueName = "orders.product.delete.queue";
            
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            //Create exchange
            string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

            //Create message queue
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            //Bind the message to exchange
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, args) =>
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                if(message != null)
                {
                    var productDeletionUpdateMessage = JsonSerializer.Deserialize<ProductDeletionMessage>(message);
                    _logger.LogInformation($"Product is deleted: {productDeletionUpdateMessage?.ProductID}, Name: {productDeletionUpdateMessage?.ProductName}");
                }
            };
            _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
        }

    }
}
