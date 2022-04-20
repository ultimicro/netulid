namespace NetUlid.Benchmarks;

using BenchmarkDotNet.Attributes;

public class CompareTo
{
    private readonly global::NetUlid.Ulid netulid1;
    private readonly global::NetUlid.Ulid netulid2;
    private readonly global::NUlid.Ulid nulid1;
    private readonly global::NUlid.Ulid nulid2;
    private readonly global::System.Ulid ulid1;
    private readonly global::System.Ulid ulid2;

    public CompareTo()
    {
        var first = global::NetUlid.Ulid.Generate().ToByteArray();
        var second = global::NetUlid.Ulid.Generate().ToByteArray();

        this.netulid1 = new(first);
        this.netulid2 = new(second);
        this.nulid1 = new(first);
        this.nulid2 = new(second);
        this.ulid1 = new(first);
        this.ulid2 = new(second);
    }

    [Benchmark]
    public int NetUlid() => this.netulid1.CompareTo(this.netulid2);

    [Benchmark]
    public int NUlid() => this.nulid1.CompareTo(this.nulid2);

    [Benchmark]
    public int Ulid() => this.ulid1.CompareTo(this.ulid2);
}
