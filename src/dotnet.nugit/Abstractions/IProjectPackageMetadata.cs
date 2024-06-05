namespace dotnet.nugit.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProjectPackageMetadata
    {
        IProjectPackageMetadata AddDefaultValues(string packagePropertyName, params string?[] projectName);

        Task WriteNuspecFileAsync(string path, CancellationToken cancellationToken);
    }
}