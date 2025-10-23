using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using System.Net.Http.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient //not necessary for an interface
{
    private readonly HttpClient _httpClient;

    public UsersMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

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
                throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
            }
        }


        UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>(); // reads response as an object of userDto

        if (user == null)
        {
            throw new ArgumentException("Invalid User ID");
        }

        return user;
    }
}
