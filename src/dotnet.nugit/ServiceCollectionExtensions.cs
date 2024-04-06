namespace dotnet.nugit
{
    using System.CommandLine.Extensions;
    using Abstractions;
    using Commands;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandLineApplication(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton(new CommandLineApplication())
                .AddModule<ServicesModule>()
                .AddModule<CommandsModule>();

            return services;
        }

        private static IServiceCollection AddModule<T>(this IServiceCollection services) where T : IModule, new()
        {
            ArgumentNullException.ThrowIfNull(services);

            var module = new T();
            module.LoadModule(services);
            return services;
        }
    }
}