namespace dotnet.nugit.Services.Workspace
{
    using System;
    using dotnet.nugit.Abstractions;
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class WindowsServicesModule : IModule
    {
        public void LoadModule(IServiceCollection services)
        {
            if (OperatingSystem.IsWindows() == false)
                return;
            
            services.AddTransient<IMsBuildToolPathLocator, VisualStudioToolPathLocator>();
        }
    }
}