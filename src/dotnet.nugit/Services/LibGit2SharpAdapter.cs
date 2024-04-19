namespace dotnet.nugit.Services
{
    using System;
    using System.Threading;
    using Abstractions;
    using LibGit2Sharp;
    using Microsoft.Extensions.Logging;
    using Resources;

    internal sealed class LibGit2SharpAdapter(
        ILogger<LibGit2SharpAdapter> logger) : ILibGit2SharpAdapter
    {
        private readonly ILogger<LibGit2SharpAdapter> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ManualResetEventSlim resetEventSlim = new();

        public bool TryCloneRepository(string cloneUrl, string repositoryPath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(cloneUrl)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(cloneUrl));
            if (string.IsNullOrWhiteSpace(repositoryPath)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(repositoryPath));

            this.resetEventSlim.Reset();

            var cloneOptions = new CloneOptions
            {
                RecurseSubmodules = true,
                FetchOptions =
                {
                    RepositoryOperationCompleted = this.RepositoryOperationCompleted
                }
            };

            try
            {
                Repository.Clone(cloneUrl, repositoryPath, cloneOptions);
                this.resetEventSlim.Wait(cancellationToken);
                return true;
            }
            catch (OperationCanceledException e)
            {
                this.logger.LogError(e, "The clone operation has timed out.");
            }
            catch (LibGit2SharpException e)
            {
                this.logger.LogError(e, "Failed to clone the given repository.");
            }

            return false;
        }

        public IRepository OpenRepository(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(Resources.ArgumentException_Value_cannot_be_null_or_whitespace, nameof(path));

            return new Repository(path);
        }

        private void RepositoryOperationCompleted(RepositoryOperationContext context)
        {
            this.logger.LogInformation("Completed operating on the repository.");
            this.resetEventSlim.Set();
        }
    }
}