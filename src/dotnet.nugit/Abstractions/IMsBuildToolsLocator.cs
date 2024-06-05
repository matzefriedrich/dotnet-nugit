namespace dotnet.nugit.Abstractions
{
    public interface IMsBuildToolsLocator
    {
        void Initialize();
        
        string? LocateMsBuildToolsPath();
    }
}