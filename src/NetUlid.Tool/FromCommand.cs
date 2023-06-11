namespace NetUlid.Tool;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.Threading.Tasks;

internal sealed class FromCommand : Command, ICommandHandler
{
    private static readonly Argument<string> TimeArgument = new("time", "The milliseconds since January 1, 1970 12:00 AM UTC");
    private static readonly Argument<string> RandomnessArgument = new("rand", "The 80-bits cryptographically randomness");

    public FromCommand()
        : base("from", "Create a ULID from the specified time and randomness")
    {
        this.Add(TimeArgument);
        this.Add(RandomnessArgument);
        this.Handler = this;
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var time = context.ParseResult.GetValueForArgument(TimeArgument);
        var rand = context.ParseResult.GetValueForArgument(RandomnessArgument);
        var timestamp = long.Parse(time, CultureInfo.InvariantCulture);
        var randomness = Convert.FromHexString(rand);
        var domain = new Ulid(timestamp, randomness);

        Console.WriteLine(domain.ToString());

        return Task.FromResult(0);
    }
}
