namespace dotnet.nugit.Abstractions
{
    using Microsoft.Extensions.DependencyInjection;

    internal interface IModule
    {
        void LoadModule(IServiceCollection services);
    }
}