using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies
{
    public class PollyPolicies(ILogger<UsersMicroservicePolicies> logger) : IPollyPolicies
    {
        public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
        {
            var policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                                                                 .WaitAndRetryAsync(
                                                                    retryCount: retryCount, //Number of retries
                                                                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Delay between retries
                                                                    onRetry: (outcome, timespan, retryAttempt, context) =>
                                                                    {
                                                                        logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
                                                                    });
            return policy;
        }


        public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            var policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                                                                          .CircuitBreakerAsync(
                                                                             handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking, //Threshold for failed requests
                                                                             durationOfBreak: durationOfBreak, // Waiting time to be in "Open" state
                                                                             onBreak: (outcome, timespan) =>
                                                                             {
                                                                                 logger.LogInformation($"Circuit breaker opened for {timespan.TotalMinutes} minutes due to consecutive 3 failures. The subsequent requests will be blocked");
                                                                             },
                                                                             onReset: () => {
                                                                                 logger.LogInformation($"Circuit breaker closed. The subsequent requests will be allowed.");
                                                                             });
            return policy;
        }


        public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
        {
            var policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);

            return policy;
        }
    }
}
