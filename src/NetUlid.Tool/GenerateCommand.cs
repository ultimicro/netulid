namespace NetUlid.Tool
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;

    internal sealed class GenerateCommand : Command, ICommandHandler
    {
        public GenerateCommand()
            : base("gen", "Generate a new ULID")
        {
            this.Handler = this;
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            Console.WriteLine(Ulid.Generate());
            return Task.FromResult(0);
        }
    }
}
