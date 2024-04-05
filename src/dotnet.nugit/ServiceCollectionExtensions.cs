namespace dotnet.nugit
{
    using Abstractions;
    using Microsoft.Extensions.DependencyInjection;

    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModule<T>(this IServiceCollection services) where T : IModule, new()
        {
            ArgumentNullException.ThrowIfNull(services);

            var module = new T();
            module.LoadModule(services);
            return services;
        }
    }
}