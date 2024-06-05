namespace dotnet.nugit.Abstractions
{
    public interface IMsBuildToolPathLocator
    {
        bool TryLocateMsBuildToolsPath(out string? path);
    }
}