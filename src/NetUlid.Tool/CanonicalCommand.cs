namespace NetUlid.Tool;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

internal sealed class CanonicalCommand : Command, ICommandHandler
{
    private static readonly Argument<string> ValueArgument = new("value", "A binary form to convert");

    public CanonicalCommand()
        : base("cano", "Convert a binary form to canonical form")
    {
        this.Add(ValueArgument);
        this.Handler = this;
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var value = context.ParseResult.GetValueForArgument(ValueArgument);
        var domain = new Ulid(Convert.FromHexString(value));

        Console.WriteLine(domain.ToString());

        return Task.FromResult(0);
    }
}
