using eCommerce.OrderMicroservice.BusinessLogicLayer;
using eCommerce.OrderMicroservice.DataAccessLayer;
using eCommerce.OrdersMicroservice.API.Middleware;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
using FluentValidation.AspNetCore;
using Polly;

var builder = WebApplication.CreateBuilder(args);

//Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers();

//FluentValidations, if autovalidation, no need gtpo validate in controller, it has been done in services explicitly
builder.Services.AddFluentValidationAutoValidation();

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


////Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();
builder.Services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();

// we dont use add transiengt or addscoped
//adding custom http client for usersMicroserviceclient
builder.Services.AddHttpClient<UsersMicroserviceClient>(client => {
    //new Uri("http://localhost:9090"); 9090 is the exposed port in docker file for user microservice
    //builder.Configuration to read from config
    //$ for string interpolation
    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
})
  //  .AddPolicyHandler(
  //  // polly policy only to intervene in presence of error response
  //Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
  //.WaitAndRetryAsync(
  //   retryCount: 5, //Number of retries
  //   sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2), // Delay between retries
  //   onRetry: (outcome, timespan, retryAttempt, context) => // at evefry retry this will execute
  //   {
  //       //TO DO: add logs
  //   })
  //);
  //intsead of above create a reusable policy that can be reused, also will  make program.cs less cluttered
//.AddPolicyHandler(
//    // this will recive policy from the class we have created
////builder you have the service collection  - build service provider - inject get RequiredService, this method returns an instance of UserMicroservicePolicy
//   builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy()
//  )
//  .AddPolicyHandler(
//   builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCircuitBreakerPolicy()
//  )
//  .AddPolicyHandler(
//   builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetTimeoutPolicy())
//   ;
 .AddPolicyHandler(
   builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCombinedPolicy())
   ;


// ebvironment variables from launchsettings or docker file will be replaced by placeholders here
// two ways to read environment variable Environment.GetEnvironment, or reda from configuration
//in builder object we have configuration property, we can read config through this , read specific value assuming this environment variable is provided
//at run time builder.Configuration["UsersMicroserviceName"] will be replavced nby local host, aand builder.Configuration["UsersMicroservicePort"] by 9090

builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
}).AddPolicyHandler(
   builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroservicePolicies>().GetFallbackPolicy())
 .AddPolicyHandler(
   builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroservicePolicies>().GetBulkheadIsolationPolicy())

  ;



var app = builder.Build();


app.UseExceptionHandlingMiddleware();
app.UseRouting();

//Cors
app.UseCors();

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//Endpoints
app.MapControllers();


app.Run();