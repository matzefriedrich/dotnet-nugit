namespace dotnet.nugit
{
    using Commands;
    using Microsoft.Extensions.DependencyInjection;

    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            services.AddTransient<GreetingCommand>();
            services.AddTransient<ListEnvironmentVariablesCommand>();
            return services;
        }
    }
}