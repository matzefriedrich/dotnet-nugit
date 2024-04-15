namespace dotnet.nugit
{
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Serilog.Core;

    internal static class Program
    {
        public static int Main(string[] args)
        {
            Logger logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var services = new ServiceCollection();
            services
                .AddLogging(builder => builder.AddSerilog(logger))
                .AddCommandLineApplication();

            using ServiceProvider provider = services.BuildServiceProvider();
            var app = provider.GetRequiredService<ApplicationService>();

            return app.Execute(args);
        }
    }
}