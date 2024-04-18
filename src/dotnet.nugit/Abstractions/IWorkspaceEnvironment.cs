namespace dotnet.nugit.Abstractions
{
    using System.IO;

    /// <summary>
    ///     Provides functionality to create <see cref="TextReader" /> and <see cref="TextWriter" /> objects that can be used
    ///     to read and alter workspace configuration files.
    /// </summary>
    public interface IWorkspaceEnvironment
    {
        /// <summary>
        ///     Creates a reader object that can read the NuGit configuration data for the current workspace.
        /// </summary>
        /// <returns>
        ///     Returns a <see cref="TextReader" /> object, or <see cref="TextReader.Null" /> if the configuration file does
        ///     not exist.
        /// </returns>
        TextReader CreateConfigurationFileReader();

        /// <summary>
        ///     Creates a writer object that can writer that can write NuGit configuration data to the current workspace.
        /// </summary>
        /// <returns>
        ///     Returns a <see cref="TextWriter" /> object, or <see cref="TextWriter.Null" /> if the configuration file does
        ///     not exist, or is not writable.
        /// </returns>
        TextWriter GetConfigurationFileWriter();
    }
}