namespace dotnet.nugit.Services.Workspace
{
    using System;
    using Abstractions;
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class WindowsServicesModule : IModule
    {
        public void LoadModule(IServiceCollection services)
        {
            if (OperatingSystem.IsWindows() == false)
                return;

            services.AddSingleton<IMsBuildToolPathLocator, VisualStudioToolPathLocator>();
        }
    }
}