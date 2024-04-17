namespace dotnet.nugit.Services
{
    public sealed class CurrentDirectoryWorkspaceEnvironment : DirectoryWorkspaceEnvironment
    {
        protected override string DirectoryPath()
        {
            return Environment.CurrentDirectory;
        }
    }
}