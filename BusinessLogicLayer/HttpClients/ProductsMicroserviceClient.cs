using BusinessLogicLayer.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients
{
    public class ProductsMicroserviceClient(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<ProductDto?> GetProductById(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/products/search/product-id/{id}");

            if(!response.IsSuccessStatusCode)
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
    }
}
