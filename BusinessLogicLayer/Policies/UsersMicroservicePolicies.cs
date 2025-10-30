using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;

    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }


    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
      .WaitAndRetryAsync(
         retryCount: 5, //Number of retries
         sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)), // Delay between retries, intsead of settingto 2, use exponential backoff
         //first time delay will be 2 power 1, then 2 poer 2, and onwards
         onRetry: (outcome, timespan, retryAttempt, context) =>
         {
             _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
         });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
      .CircuitBreakerAsync(
         handledEventsAllowedBeforeBreaking: 3, //Number of retries, afetr 3 failed request , the c]ircuit will open
         durationOfBreak: TimeSpan.FromMinutes(2), // Delay between retries
         onBreak: (outcome, timespan) => // moving from closed top open state
         {
             _logger.LogInformation($"Circuit breaker opened for {timespan.TotalMinutes} minutes due to consecutive 3 failures. The subsequent requests will be blocked");
         },
         onReset: () => { // molving from halopen sdtate to closed statre
             _logger.LogInformation($"Circuit breaker closed. The subsequent requests will be allowed.");
         });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(1500));// 1500 milliseconds or 1.5 second is the time we are willing to wait

        return policy;
    }
    //good practice to combine
    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        //this combined one is so that it nis done in the sma esequence as required, could be possible another dev is working on program.cs, and you are working on crferating policies
        var retryPolicy = GetRetryPolicy();
        var circuitBreakerPolicy = GetCircuitBreakerPolicy();
        var timeoutPolicy = GetTimeoutPolicy();

        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy); //this governs the order of policies
        return wrappedPolicy;
    }
}