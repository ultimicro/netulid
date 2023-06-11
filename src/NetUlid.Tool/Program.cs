namespace NetUlid.Tool;

using System.CommandLine;
using System.Threading.Tasks;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var root = new RootCommand("ULID tool for .NET.")
        {
            new GenerateCommand(),
            new FromCommand(),
            new BinaryCommand(),
            new CanonicalCommand(),
            new TimeCommand(),
            new RandomnessCommand(),
        };

        return await root.InvokeAsync(args);
    }
}
