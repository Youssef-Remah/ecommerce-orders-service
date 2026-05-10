using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies
{
    public class UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger) : IUsersMicroservicePolicies
    {
        public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                               .WaitAndRetryAsync(
                                retryCount: 5, //Number of retries
                                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Delay between retries
                                onRetry: (outcome, timespan, retryAttempt, context) =>
                                {
                                    logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
                                });
            return policy;
        }
    }
}
