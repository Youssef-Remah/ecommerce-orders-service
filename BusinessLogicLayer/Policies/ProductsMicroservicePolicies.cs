using BusinessLogicLayer.DTOs;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.Policies
{
    public class ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger) : IProductsMicroservicePolicies
    {
        public IAsyncPolicy<HttpResponseMessage> GetBulkHeadIsolationPolicy()
        {
            AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
                                                              maxParallelization: 2, //Allows up to 2 concurrent requests
                                                              maxQueuingActions: 40, //Queue up to 40 additional requests
                                                              onBulkheadRejectedAsync: (context) =>
                                                              {
                                                                  logger.LogWarning("BulkheadIsolation triggered. Can't send any more requests since the queue is full");
                                                                  throw new BulkheadRejectedException("Bulkhead queue is full");
                                                              });
            return policy;
        }

        public IAsyncPolicy<HttpResponseMessage> GetFallBackPolicy()
        {
            var policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                               .FallbackAsync(async (context) =>
                               {
                                   logger.LogWarning("Fallback triggered: The request failed, returning dummy data");

                                   var product = new ProductDto(ProductID: Guid.Empty,
                                                                ProductName: "Temporarily Unavailable (fallback)",
                                                                Category: "Temporarily Unavailable (fallback)",
                                                                UnitPrice: 0,
                                                                QuantityInStock: 0);

                                   var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                                   {
                                       Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
                                   };

                                   return response;
                               });

            return policy;
        }

        public IAsyncPolicy<HttpResponseMessage> GetWrappedPolicy()
        {
            var fallBackPolicy = GetFallBackPolicy();
            var bulkHeadPolicy = GetBulkHeadIsolationPolicy();

            var wrappedPolicy = Policy.WrapAsync(fallBackPolicy, bulkHeadPolicy);

            return wrappedPolicy;
        }
    }
}
