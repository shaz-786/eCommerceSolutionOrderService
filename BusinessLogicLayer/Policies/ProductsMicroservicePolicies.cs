using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger;

    public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
          maxParallelization: 2, //Allows up to 2 concurrent requests to dependedncy serice which is product service
          maxQueuingActions: 40, //Queue up to 40 additional requests, after executing 2, the next two will be taken from queue as FIFO
          onBulkheadRejectedAsync: (context) => { // in case additional req
              _logger.LogWarning("BulkheadIsolation triggered. Can't send any more requests since the queue is full");
              // up to you to throw exception, good as it can be cayched in products microservoce client
              throw new BulkheadRejectedException("Bulkhead queue is full");
          }
          );

        return policy;
    }


    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
          .FallbackAsync(async (context) =>
          {
              _logger.LogWarning("Fallback triggered: The request failed, returning dummy data");

              ProductDTO product = new ProductDTO(ProductID: Guid.Empty,
                                        ProductName: "Temporarily Unavailable (fallback)",
                                        Category: "Temporarily Unavailable (fallback)",
                                        UnitPrice: 0,
                                        QuantityInStock: 0
                                        );
            //must manually create response, so the client will get this response instead of error response
              var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
              {
                  Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
              };

              return response;
          });

        return policy;
    }
}