namespace NetUlid.Tool;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

internal sealed class BinaryCommand : Command, ICommandHandler
{
    private static readonly Argument<string> ValueArgument = new("value", "A canonical form to convert");

    public BinaryCommand()
        : base("bin", "Convert a canonical form to binary form")
    {
        this.Add(ValueArgument);
        this.Handler = this;
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var value = context.ParseResult.GetValueForArgument(ValueArgument);
        var domain = Ulid.Parse(value);

        Console.WriteLine(Convert.ToHexString(domain.ToByteArray()));

        return Task.FromResult(0);
    }
}
