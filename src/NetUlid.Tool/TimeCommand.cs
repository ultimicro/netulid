namespace NetUlid.Tool;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.Threading.Tasks;

internal sealed class TimeCommand : Command, ICommandHandler
{
    private static readonly Argument<string> ValueArgument = new("value", "A ULID to get time");

    public TimeCommand()
        : base("time", "Get time part in UTC")
    {
        this.Add(ValueArgument);
        this.Handler = this;
    }

    public Task<int> InvokeAsync(InvocationContext context)
    {
        var value = context.ParseResult.GetValueForArgument(ValueArgument);
        var domain = value.Length == 32 ? new Ulid(Convert.FromHexString(value)) : Ulid.Parse(value);

        Console.WriteLine(domain.Time.ToString(CultureInfo.InvariantCulture));

        return Task.FromResult(0);
    }
}
