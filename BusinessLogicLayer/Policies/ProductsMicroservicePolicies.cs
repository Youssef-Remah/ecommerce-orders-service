using BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Logging;
using Polly;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.Policies
{
    public class ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger) : IProductsMicroservicePolicies
    {
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

                                   var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                                   {
                                       Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
                                   };

                                   return response;
                               });

            return policy;
        }
    }
}
