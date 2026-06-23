using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQProductNameUpdateHostedService : BackgroundService
    {
        private readonly IRabbitMQProductNameUpdateConsumer _consumer;

        public RabbitMQProductNameUpdateHostedService(IRabbitMQProductNameUpdateConsumer consumer)
        {
            _consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _consumer.Consume();

                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);

                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
        public override void Dispose()
        {
            _consumer.Dispose();
            base.Dispose();
        }
    }
}
