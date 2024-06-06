namespace dotnet.nugit.Services.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Loader;

    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly Dictionary<AssemblyName, Assembly> assemblies = new();

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return this.assemblies.GetValueOrDefault(assemblyName)!;
        }

        public void CacheAssembly(AssemblyName assemblyName, Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assemblyName);
            ArgumentNullException.ThrowIfNull(assembly);

            this.assemblies.Add(assemblyName, assembly);
        }
    }
}