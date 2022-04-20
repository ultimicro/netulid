namespace NetUlid.Benchmarks;

using BenchmarkDotNet.Attributes;

public class ConstructFromBinary
{
    private readonly byte[] bin;

    public ConstructFromBinary()
    {
        this.bin = global::NetUlid.Ulid.Generate().ToByteArray();
    }

    [Benchmark]
    public global::NetUlid.Ulid NetUlid() => new(this.bin);

    [Benchmark]
    public global::NUlid.Ulid NUlid() => new(this.bin);

    [Benchmark]
    public global::System.Ulid Ulid() => new(this.bin);
}
