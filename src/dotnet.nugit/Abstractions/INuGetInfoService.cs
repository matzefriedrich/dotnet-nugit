namespace dotnet.nugit.Abstractions
{
    public interface INuGetInfoService
    {
        TextReader GetNuGetConfigReader();

        TextWriter GetNuGetConfigWriter();
    }
}