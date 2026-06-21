using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQProductNameUpdateConsumer : IRabbitMQProductNameUpdateConsumer, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly string hostname, username, password, port;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;

        public RabbitMQProductNameUpdateConsumer(IConfiguration configuration, ILogger<RabbitMQProductNameUpdateConsumer> logger)
        {
            _configuration = configuration;

            hostname = _configuration["RabbitMQ_HostName"]!;
            username = _configuration["RabbitMQ_UserName"]!;
            password = _configuration["RabbitMQ_Password"]!;
            port = _configuration["RabbitMQ_Port"]!;

            var connectionFactory = new ConnectionFactory()
            {
                HostName = hostname,
                UserName = username,
                Password = password,
                Port = Convert.ToInt32(port)
            };
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _logger = logger;
        }
        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }

        public void Consumer()
        {
            string routingKey = "product.update.name";
            string queueName = "orders.product.update.name.queue";

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
                    var productNameUpdateMessage = JsonSerializer.Deserialize<ProductNameUpdateMessage>(message);
                    _logger.LogInformation($"Product name updated: {productNameUpdateMessage.ProductID}, New name: {productNameUpdateMessage.NewName}");
                }
            };
            _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
        }

    }
}
