namespace dotnet.nugit.Abstractions
{
    using System.IO;

    /// <summary>
    ///     Provides functionality to read and write the NuGet configuration.
    /// </summary>
    public interface INuGetConfigurationAccessService
    {
        TextReader GetNuGetConfigReader();

        TextWriter GetNuGetConfigWriter();
    }
}