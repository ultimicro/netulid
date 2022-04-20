namespace NetUlid.Benchmarks;

using BenchmarkDotNet.Attributes;

public class ToString
{
    private readonly NetUlid.Ulid netulid;
    private readonly NUlid.Ulid nulid;
    private readonly System.Ulid ulid;

    public ToString()
    {
        this.netulid = global::NetUlid.Ulid.Generate();
        this.nulid = global::NUlid.Ulid.NewUlid();
        this.ulid = global::System.Ulid.NewUlid();
    }

    [Benchmark]
    public string NetUlid() => this.netulid.ToString();

    [Benchmark]
    public string NUlid() => this.nulid.ToString();

    [Benchmark]
    public string Ulid() => this.ulid.ToString();
}
