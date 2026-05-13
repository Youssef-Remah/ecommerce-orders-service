using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies
{
    public class UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger, IPollyPolicies policies) : IUsersMicroservicePolicies
    {
        public IAsyncPolicy<HttpResponseMessage> GetWrappedPolicy()
        {
            var retryPolicy = policies.GetRetryPolicy(5);
            var circuitBreakerPolicy = policies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));
            var timeoutPolicy = policies.GetTimeoutPolicy(TimeSpan.FromSeconds(5));

            var wrappedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

            return wrappedPolicy;
        }
    }
}
