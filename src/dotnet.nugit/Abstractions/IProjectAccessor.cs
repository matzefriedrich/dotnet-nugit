namespace dotnet.nugit.Abstractions
{
    public interface IProjectAccessor
    {
        IProjectPackageMetadata DeriveNuspec();
    }
}