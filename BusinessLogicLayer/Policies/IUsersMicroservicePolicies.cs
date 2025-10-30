using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IUsersMicroservicePolicies
{
    //represrnts any type of policies - asyncPolicy
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
    IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy();
    IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy();
    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
}