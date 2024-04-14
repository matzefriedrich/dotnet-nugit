namespace dotnet.nugit.Services
{
    using Abstractions;
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class ServicesModule : IModule
    {
        public void LoadModule(IServiceCollection services)
        {
            services.AddTransient<VariableAccessor, NugitHomeVariableAccessor>();
            services.AddSingleton<IVariablesService, VariablesService>();
            services.AddTransient<INuGetFeedService, LocalNuGetFeedService>();
            services.AddTransient<INuGetInfoService, NuGetInfoService>();
            services.AddTransient<IFindFilesService, FindFilesService>();
        }
    }
}