namespace dotnet.nugit.Abstractions
{
    public interface IDotNetProject
    {
        string? GetProjectFilePath();

        string? GetNuspecBasePath();
        
        string? GetNuspecFilePath();
        
        IProjectPackageMetadata DerivePackageSpec();
    }
}