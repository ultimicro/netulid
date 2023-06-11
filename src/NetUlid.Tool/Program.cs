namespace NetUlid.Tool;

using System.CommandLine;
using System.Threading.Tasks;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var root = new RootCommand("ULID tool for .NET Core.")
        {
            new GenerateCommand(),
        };

        return await root.InvokeAsync(args);
    }
}
