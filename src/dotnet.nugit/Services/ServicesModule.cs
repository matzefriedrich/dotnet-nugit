namespace dotnet.nugit.Services
{
    using System;
    using System.IO.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using nugit.Abstractions;
    using Tasks;

    internal sealed class ServicesModule : IModule
    {
        public void LoadModule(IServiceCollection services)
        {
            services.AddSingleton<IWorkspaceEnvironment, CurrentDirectoryWorkspaceEnvironment>();
            services.AddSingleton<IVariablesService, VariablesService>();
            services.AddTransient<IDotNetUtility, DotNetUtility>();
            services.AddTransient<IFindFilesService, FindFilesService>();
            services.AddTransient<INuGetFeedConfigurationService, LocalNuGetFeedConfigurationService>();
            services.AddTransient<INuGetConfigurationAccessService, NuGetConfigurationAccessService>();
            services.AddTransient<INugitWorkspace, NugitWorkspace>();
            services.AddTransient<VariableAccessor, NugitHomeVariableAccessor>();
            services.AddTransient<ILibGit2SharpAdapter, LibGit2SharpAdapter>();

            // Register task factories
            services.AddTransient<Func<OpenRepositoryTask>>(provider => provider.GetRequiredService<OpenRepositoryTask>);
            services.AddTransient<Func<FindAndBuildProjectsTask>>(provider => provider.GetRequiredService<FindAndBuildProjectsTask>);
        }
    }
}