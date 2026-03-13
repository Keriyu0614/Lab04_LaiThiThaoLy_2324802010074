using Messaging.Common.Connection;
using Microsoft.Extensions.DependencyInjection;

namespace Messaging.Common.Extensions
{
    /// <summary>[STATIC CLASS] Extension method: builder.Services.AddRabbitMq(...)</summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMq(
            this IServiceCollection services,
            string hostName, string userName, string password, string vhost)
        {
            var manager = new ConnectionManager(hostName, userName, password, vhost);
            var connection = manager.GetConnection();
            var channel = connection.CreateModel();

            services.AddSingleton(manager);
            services.AddSingleton(connection);
            services.AddSingleton(channel);     // IModel - shared bởi Publisher + Consumer
            return services;
        }
    }
}
