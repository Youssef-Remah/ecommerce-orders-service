using BusinessLogicLayer.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients
{
    public class UsersMicroserviceClient(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<UserDto?> GetUserByUserId(Guid id)
        {
            var response = await _httpClient.GetAsync($"/api/Users/{id}");

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
                    //Approach 1: throw exception
                    //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");

                    //Approach 2: return dummy data
                    return new UserDto(Guid.Empty,
                                       Email: "Temprarily UnAvailable",
                                       Name: "Temprarily UnAvailable",
                                       Gender: "Temprarily UnAvailable");
                }
            }

            var user = await response.Content.ReadFromJsonAsync<UserDto>();

            return user ?? throw new ArgumentException("Invalid User Id");
        }
    }
}
