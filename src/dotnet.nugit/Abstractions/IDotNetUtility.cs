namespace dotnet.nugit.Abstractions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDotNetUtility
    {
        Task BuildAsync(IDotNetProject project, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        Task<bool> TryPackAsync(IDotNetProject project, string packageTargetFolderPath, PackOptions options, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}