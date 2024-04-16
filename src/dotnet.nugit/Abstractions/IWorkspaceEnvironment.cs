namespace dotnet.nugit.Abstractions
{
    public interface IWorkspaceEnvironment
    {
        TextReader CreateConfigurationFileReader();

        TextWriter GetConfigurationFileWriter();
    }
}