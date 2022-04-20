namespace NetUlid.Benchmarks;

using System;
using BenchmarkDotNet.Attributes;

public class GetTime
{
    private readonly global::NetUlid.Ulid netulid;
    private readonly global::NUlid.Ulid nulid;
    private readonly global::System.Ulid ulid;

    public GetTime()
    {
        var bin = global::NetUlid.Ulid.Generate().ToByteArray();

        this.netulid = new(bin);
        this.nulid = new(bin);
        this.ulid = new(bin);
    }

    [Benchmark]
    public long NetUlidTimestamp() => this.netulid.Timestamp;

    [Benchmark]
    public DateTime NetUlid() => this.netulid.Time;

    [Benchmark]
    public DateTimeOffset NUlid() => this.nulid.Time;

    [Benchmark]
    public DateTimeOffset Ulid() => this.ulid.Time;
}
