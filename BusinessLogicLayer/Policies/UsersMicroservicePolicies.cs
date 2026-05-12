using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies
{
    public class UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger) : IUsersMicroservicePolicies
    {
        public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            var policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                   .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5, //failed request threshold
                    durationOfBreak: TimeSpan.FromMinutes(2), //Time span between Open and Half-Open statuses
                    onBreak: (outcome, timespan) =>
                    {
                        logger.LogInformation($"Circuit breaker opened for {timespan.TotalMinutes} minutes" +
                            $" due to 5 consecutive failures. The subsequent requests will be blocked");
                    },
                    onReset: () => { logger.LogInformation("Circuit breaker closed. The subsequent requests will be allowed."); });

            return policy;
        }

        public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                               .WaitAndRetryAsync(
                                retryCount: 3, //Number of retries
                                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Delay between retries
                                onRetry: (outcome, timespan, retryAttempt, context) =>
                                {
                                    logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
                                });

            return policy;
        }
    }
}
