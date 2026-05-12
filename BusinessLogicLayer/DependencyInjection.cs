using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Mappers;
using BusinessLogicLayer.Policies;
using BusinessLogicLayer.ServiceInterfaces;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogicLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
            services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);
            services.AddScoped<IOrdersService, OrdersService>();
            services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();
            services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();

            services.AddHttpClient<UsersMicroserviceClient>(client =>
            {
                client.BaseAddress = new Uri($"http://{configuration["UsersMicroserviceDomain"]}:{configuration["UsersMicroservicePort"]}");
            }).AddPolicyHandler(services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy())
              .AddPolicyHandler(services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCircuitBreakerPolicy());

            services.AddHttpClient<ProductsMicroserviceClient>(client =>
            {
                client.BaseAddress = new Uri($"http://{configuration["ProductsMicroserviceDomain"]}:{configuration["ProductsMicroservicePort"]}");
            }).AddPolicyHandler(services.BuildServiceProvider().GetRequiredService<IProductsMicroservicePolicies>().GetFallBackPolicy());
            
            return services;
        }
    }
}
