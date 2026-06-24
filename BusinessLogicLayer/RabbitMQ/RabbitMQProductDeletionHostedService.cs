using Microsoft.Extensions.Hosting;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQProductDeletionHostedService : BackgroundService
    {
        private readonly IRabbitMQProductDeletionConsumer _consumer;

        public RabbitMQProductDeletionHostedService(IRabbitMQProductDeletionConsumer consumer)
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

                    await Task.Delay(Timeout.Infinite, stoppingToken);
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
