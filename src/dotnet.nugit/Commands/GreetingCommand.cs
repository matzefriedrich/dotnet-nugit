namespace dotnet.nugit.Commands
{
    public sealed class GreetingCommand
    {
        public async Task<int> GreetAsync(string name, bool polite)
        {
            if (polite) Console.WriteLine($"Good day {name}");
            else Console.WriteLine($"Hello {name}");
            return await Task.FromResult(0);
        }
    }
}