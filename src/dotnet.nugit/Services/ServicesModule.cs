namespace dotnet.nugit.Services
{
    using System;
    using Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Tasks;
    using Workspace;

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

            ConfigureTaskServices(services);
            ConfigureWorkspaceServices(services);
        }

        private static void ConfigureTaskServices(IServiceCollection services)
        {
            services.AddTransient<IOpenRepositoryTask, OpenRepositoryTask>();
            services.AddTransient<Func<IOpenRepositoryTask>>(provider => provider.GetRequiredService<IOpenRepositoryTask>);

            services.AddTransient<IFindAndBuildProjectsTask, FindAndBuildProjectsTask>();
            services.AddTransient<Func<IFindAndBuildProjectsTask>>(provider => provider.GetRequiredService<IFindAndBuildProjectsTask>);

            services.AddTransient<IBuildRepositoryPackagesTask, BuildRepositoryPackagesTask>();
            services.AddTransient<Func<IBuildRepositoryPackagesTask>>(provider => provider.GetRequiredService<IBuildRepositoryPackagesTask>);
        }
        
        private static void ConfigureWorkspaceServices(IServiceCollection services)
        {
            services.AddSingleton<IMsBuildToolsLocator, MsBuildToolsLocatorService>();
            services.AddTransient<IProjectWorkspaceManager, ProjectWorkspaceManager>();

            services.AddModule<LinuxServicesModule>();
            services.AddModule<WindowsServicesModule>();
        }
    }
}