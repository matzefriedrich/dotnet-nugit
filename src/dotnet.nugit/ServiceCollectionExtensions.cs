namespace dotnet.nugit
{
    using Abstractions;
    using Commands;
    using Microsoft.Extensions.DependencyInjection;

    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            services.AddTransient<GreetingCommand>();
            services.AddTransient<ListEnvironmentVariablesCommand>();
            services.AddTransient<VariableAccessor, NugitHomeVariableAccessor>();
            services.AddSingleton<IVariablesService, VariablesService>();
            return services;
        }
    }
}