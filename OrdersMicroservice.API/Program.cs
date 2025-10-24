using eCommerce.OrderMicroservice.BusinessLogicLayer;
using eCommerce.OrderMicroservice.DataAccessLayer;
using eCommerce.OrdersMicroservice.API.Middleware;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using FluentValidation.AspNetCore;

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


//Cors
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

// we dont use add transiengt or addscoped
//adding custom http client for usersMicroserviceclient
builder.Services.AddHttpClient<UsersMicroserviceClient>(client => {
    //new Uri("http://localhost:9090"); 9090 is the exposed port in docker file for user microservice
    //builder.Configuration to read from config
    //$ for string interpolation
    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
});// ebvironment variables from launchsettings or docker file will be replaced by placeholders here
// two ways to read environment variable Environment.GetEnvironment, or reda from configuration
//in builder object we have configuration property, we can read config through this , read specific value assuming this environment variable is provided
//at run time builder.Configuration["UsersMicroserviceName"] will be replavced nby local host, aand builder.Configuration["UsersMicroservicePort"] by 9090

builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
});

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