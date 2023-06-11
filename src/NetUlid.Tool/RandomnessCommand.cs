namespace NetUlid.Tool;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

internal sealed class RandomnessCommand : Command, ICommandHandler
{
    private static readonly Argument<string> ValueArgument = new("value", "A ULID to get randomness");

    public RandomnessCommand()
        : base("rand", "Get randomness part")
    {
        this.Add(ValueArgument);
        this.Handler = this;
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var value = context.ParseResult.GetValueForArgument(ValueArgument);
        var domain = value.Length == 32 ? new Ulid(Convert.FromHexString(value)) : Ulid.Parse(value);

        Console.WriteLine(Convert.ToHexString(domain.Randomness));

        return Task.FromResult(0);
    }
}
