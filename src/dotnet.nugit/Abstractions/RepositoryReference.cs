namespace dotnet.nugit.Abstractions
{
    public class RepositoryReference
    {
        public string RepositoryType { get; init; } = "git";
        public string? RepositoryUrl { get; init; }
        public string? Hash { get; init; } = null;
        public string RepositoryPath { get; init; } = "/";
        public string? Tag { get; init; } = null;
    }
}