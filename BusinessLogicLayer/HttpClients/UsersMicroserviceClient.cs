using Amazon.Runtime.Internal.Util;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient //not necessary for an interface
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }


    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        try // to catch the circuit breaker exception and sned friendly message
        {
            //Polly policy will catch the response befor eresponse reaches HttpResponsemessage response variable
            //as mentioned in Policy.HandleResult<HttpResponseMessage> in program.cs
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");
            // ig getting internal server error , then that means it sis inside microservice
            //in intermediate window , type new StreamReader(response.Content.ReadAsStream()).ReadToEnd() to know the intrenal error message, after debug point on next line
            if (!response.IsSuccessStatusCode)// anything starting not with 2 is error response
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    //fault data sent instaed
                    return new UserDTO(
                        PersonName: "Temporarily Unavailable",
                        Email: "Temporarily Unavailable",
                        Gender: "Temporarily Unavailable",
                        UserID: Guid.Empty);
                }
            }


            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>(); // reads response as an object of userDto

            if (user == null)
            {
                throw new ArgumentException("Invalid User ID");
            }

            return user;
        }
        catch(BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Request failed because of circuit breaker is in Open state. Returning dummy data.");
            return new UserDTO(
                      PersonName: "Temporarily Unavailable (circuit breaker)",
                      Email: "Temporarily Unavailable (circuit breaker)",
                      Gender: "Temporarily Unavailable (circuit breaker)",
                      UserID: Guid.Empty);

        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timeout occurred while fetching user data. Returning dummy data");

            return new UserDTO(
                    PersonName: "Temporarily Unavailable (timeout)",
                    Email: "Temporarily Unavailable (timeout)",
                    Gender: "Temporarily Unavailable (timeout)",
                    UserID: Guid.Empty);
        }
    }
}
