namespace dotnet.nugit.Abstractions
{
    using System;

    internal static class ApplicationVariableNames
    {
        public static readonly string NugitHome = "NUGIT_HOME";

        public static string GetNugitHomeDirectoryPath(this IVariablesService variablesService)
        {
            ArgumentNullException.ThrowIfNull(variablesService);
            if (variablesService.TryGetVariable(NugitHome, out string? value) == false || string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"The {NugitHome} variable is not set.");

            return value;
        }
    }
}