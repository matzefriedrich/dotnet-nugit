namespace dotnet.nugit.Abstractions
{
    public struct PackOptions
    {
        /// <summary>
        ///     Gets or sets a value indicating the version-suffix to set for the package.
        /// </summary>
        /// <remarks>
        ///     If the project´s Version-property is defined and has a value, the value specified in
        ///     <see cref="VersionSuffix" /> is ignored by the dotnet pack command.
        /// </remarks>
        public string? VersionSuffix { get; init; }
    }
}