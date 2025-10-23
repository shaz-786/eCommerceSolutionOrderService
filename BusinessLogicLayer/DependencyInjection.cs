using eCommerce.ordersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.ordersMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.OrderMicroservice.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        //TO DO: Add business logic layer services into the IoC container

        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();//it adds all validators in assembly that inherit from abstractvalidator class
        services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);// first get the type of mapping profile class, and then we can asccess the containing assembly
        services.AddScoped<IOrdersService, OrdersService>();
        
        return services;
    }
}