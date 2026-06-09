using BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BusinessLogicLayer.HttpClients
{
    public class ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<ProductDto?> GetProductById(Guid id)
        {
            try
            {
                string? cacheKey = $"product:{id}";
                string? cachedProduct = await distributedCache.GetStringAsync(cacheKey);

                if(cachedProduct != null)
                {
                    return JsonSerializer.Deserialize<ProductDto>(cachedProduct);
                }

                var response = await _httpClient.GetAsync($"gateway/products/search/product-id/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        var productFromFallBack = await response.Content.ReadFromJsonAsync<ProductDto>() ?? throw new NotImplementedException("Fallback policy is not implemented");

                        return productFromFallBack;
                    }
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    }
                }

                var product = await response.Content.ReadFromJsonAsync<ProductDto>();

                if(product == null)
                    throw new ArgumentException("Invalid Product Id");

                var serializedProduct = JsonSerializer.Serialize(product);
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                                                                     .SetSlidingExpiration(TimeSpan.FromSeconds(100));

                await distributedCache.SetStringAsync(cacheKey, serializedProduct, cacheOptions);

                return product;
            }
            catch (BulkheadRejectedException ex)
            {
                logger.LogError(ex, "Bulkhead isolation blocks the request since the request queue is full");

                return new ProductDto(
                  ProductID: Guid.NewGuid(),
                  ProductName: "Temporarily Unavailable (Bulkhead)",
                  Category: "Temporarily Unavailable (Bulkhead)",
                  UnitPrice: 0,
                  QuantityInStock: 0);
            }
        }
    }
}
