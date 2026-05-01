using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Mappers;
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
            services.AddHttpClient<UsersMicroserviceClient>(client =>
            {
                client.BaseAddress = new Uri($"http://{configuration["UsersMicroserviceDomain"]}:{configuration["UsersMicroservicePort"]}");
            });
            services.AddHttpClient<ProductsMicroserviceClient>(client =>
            {
                client.BaseAddress = new Uri($"http://{configuration["ProductsMicroserviceDomain"]}:{configuration["ProductsMicroservicePort"]}");
            });
            return services;
        }
    }
}
