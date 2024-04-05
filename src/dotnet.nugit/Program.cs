using System.CommandLine;
using System.CommandLine.Extensions;

namespace dotnet.nugit;

using Commands;
using Microsoft.Extensions.DependencyInjection;

internal static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandLineApplication();

        var services = new ServiceCollection();
        services.AddSingleton<CommandLineApplication>().AddCommands();

        using ServiceProvider provider = services.BuildServiceProvider();

        app.Command("env", "Lists environment variables and their values.", env =>
        {
            var handler = provider.GetRequiredService<ListEnvironmentVariablesCommand>();
            env.OnExecute(async () => await handler.ListEnvironmentVariablesAsync());
        });
        
        app.Command("greeting", "Greets the specified person.", greeting =>
        {
            var handler = provider.GetRequiredService<GreetingCommand>();
            greeting.Option<string>("--name", "The person´s name.", ArgumentArity.ExactlyOne)
                .Option<bool>("--polite")
                .OnExecute(async (string name, bool polite) => await handler.GreetAsync(name, polite));
        });

        return app.Execute(args);
    }
}