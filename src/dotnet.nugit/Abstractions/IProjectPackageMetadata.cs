namespace dotnet.nugit.Abstractions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProjectPackageMetadata
    {
        IProjectPackageMetadata AddDefaultValues(string packagePropertyName, params string?[] projectName);

        Task WriteNuspecFileAsync(Stream target, CancellationToken cancellationToken);

        bool Validate(out ICollection<ValidationResult> validationResults);
    }
}