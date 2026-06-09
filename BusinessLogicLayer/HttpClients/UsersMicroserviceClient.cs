using BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BusinessLogicLayer.HttpClients
{
    public class UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<UserDto?> GetUserByUserId(Guid id)
        {
            try
            {
                var cacheKey = $"user:{id}";
                string? cachedUser = await distributedCache.GetStringAsync(cacheKey);

                if (cachedUser != null)
                {
                    return JsonSerializer.Deserialize<UserDto>(cachedUser);
                }

                var response = await _httpClient.GetAsync($"/gateway/Users/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        var fallBackUser = await response.Content.ReadFromJsonAsync<UserDto>();

                        if(fallBackUser == null)
                            throw new NotImplementedException();

                        return fallBackUser;
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

                if (user == null)
                    throw new ArgumentException("Invalid User Id");

                var serializedUser = JsonSerializer.Serialize(user);
                var cacheOptions = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(5))
                                                                     .SetSlidingExpiration(TimeSpan.FromSeconds(100));

                await distributedCache.SetStringAsync(cacheKey, serializedUser, cacheOptions);

                return user;
            }
            catch (BrokenCircuitException ex)
            {
                logger.LogInformation(ex, "Request failed since circuit breaker is in open state. Returning dummy data.");

                return new UserDto(Guid.Empty,
                   Email: "Temprarily UnAvailable",
                   Name: "Temprarily UnAvailable",
                   Gender: "Temprarily UnAvailable");
            }
            catch (TimeoutRejectedException ex)
            {
                logger.LogInformation(ex, "Timeout occurred while fetching user data. Returning dummy data.");

                return new UserDto(Guid.Empty,
                   Email: "Temprarily UnAvailable (timeout)",
                   Name: "Temprarily UnAvailable (timeout)",
                   Gender: "Temprarily UnAvailable (timeout)");
            }
        }
    }
}
