using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using System.Net.Http.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient //not necessary for an interface
{
    private readonly HttpClient _httpClient;

    public ProductsMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/product-id/{productID}");
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
                throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
            }
        }


        ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>(); // reads response as an object of userDto

        if (product == null)
        {
            throw new ArgumentException("Invalid Product ID");
        }

        return product;
    }
}
