using Example.Commands;
using Spectre.Cli;
using System.Threading.Tasks;

namespace Example
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var app = new CommandApp<DefaultCommand>();
            app.Configure(config =>
            {
                config.SetApplicationName("dotnet example");
            });

            return await app.RunAsync(args);
        }
    }
}
