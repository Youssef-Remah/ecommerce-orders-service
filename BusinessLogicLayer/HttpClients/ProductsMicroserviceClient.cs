using BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients
{
    public class ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<ProductDto?> GetProductById(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/search/product-id/{id}");

                if (!response.IsSuccessStatusCode)
                {
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

                return product ?? throw new ArgumentException("Invalid Product Id");
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
