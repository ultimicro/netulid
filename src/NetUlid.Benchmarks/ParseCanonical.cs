namespace NetUlid.Benchmarks;

using BenchmarkDotNet.Attributes;

public class ParseCanonical
{
    private readonly string canonical;

    public ParseCanonical()
    {
        this.canonical = global::NetUlid.Ulid.Generate().ToString();
    }

    [Benchmark]
    public global::NetUlid.Ulid NetUlid() => global::NetUlid.Ulid.Parse(this.canonical);

    [Benchmark]
    public global::NUlid.Ulid NUlid() => global::NUlid.Ulid.Parse(this.canonical);

    [Benchmark]
    public System.Ulid Ulid() => System.Ulid.Parse(this.canonical);
}
