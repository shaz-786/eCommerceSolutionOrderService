using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using eCommerce.OrdersMicroservie.DataAccessLayer.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace eCommerce.OrderMicroservice.DataAccessLayer;

public static class DependencyInjection
{

    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        //TO DO: Add data access layer services into the IoC container

        string connectionStringTemplate = configuration.GetConnectionString("MongoDB")!;
        string connectionString = connectionStringTemplate
          .Replace("$MONGO_HOST", Environment.GetEnvironmentVariable("MONGODB_HOST"))
          .Replace("$MONGO_PORT", Environment.GetEnvironmentVariable("MONGODB_PORT"));
        // in mongo, it is not necessary to set environmnet variable username and password using docker image, it may be necessary in production
        //must add a service of IMongoClient type
        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));// mongoclient maitains connection polling internally and
        //needs nto be instantiated only first time on first request
        //whenever we try to inject mongoclient type of object , it should be supplied as a service instance

        //inject service of IMongoDatabase type
        services.AddScoped<IMongoDatabase>(provider =>
        {
            IMongoClient client = provider.GetRequiredService<IMongoClient>();// get the service created above, and use it to access the database
            return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE")); // first time if db doesnot exist, it will  be created
            //it will retrurn object of imongoDatabase typr
            //the db willbe automatically be created at run time so no need to cretae it
        });

        //IMongoDatabase willbe used as DI in repository to access database
        services.AddScoped<IOrdersRepository, OrdersRepository>(); // prefeerred scopes as ImongoDb is also added as coped
      
        return services;
    }
}