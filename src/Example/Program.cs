using Example.Commands;
using Spectre.Cli;
using System.Threading.Tasks;

namespace Example
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var app = new CommandApp<RunCommand>();
            app.Configure(config =>
            {
                config.AddCommand<RunCommand>("run");
                config.AddCommand<ListCommand>("list");
            });

            return await app.RunAsync(args);
        }
    }
}
