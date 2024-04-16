namespace dotnet.nugit.Services
{
    using Abstractions;
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class ServicesModule : IModule
    {
        public void LoadModule(IServiceCollection services)
        {
            services.AddSingleton<IWorkspaceEnvironment, CurrentDirectoryWorkspaceEnvironment>();
            services.AddSingleton<IVariablesService, VariablesService>();
            services.AddTransient<IDotNetUtility, DotNetUtility>();
            services.AddTransient<IFindFilesService, FindFilesService>();
            services.AddTransient<INuGetFeedService, LocalNuGetFeedService>();
            services.AddTransient<INuGetInfoService, NuGetInfoService>();
            services.AddTransient<INugitWorkspace, NugitWorkspace>();
            services.AddTransient<VariableAccessor, NugitHomeVariableAccessor>();
            services.AddTransient<ILibGit2SharpAdapter, LibGit2SharpAdapter>();
        }
    }
}