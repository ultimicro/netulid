namespace NetUlid.Benchmarks;

using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;

public class ConstructFromTimestampAndRandomness
{
    private readonly DateTimeOffset time;
    private readonly long timestamp;
    private readonly byte[] randomness;

    public ConstructFromTimestampAndRandomness()
    {
        this.time = DateTimeOffset.UtcNow;
        this.timestamp = this.time.ToUnixTimeMilliseconds();
        this.randomness = new byte[10];

        RandomNumberGenerator.Fill(this.randomness);
    }

    [Benchmark]
    public global::NetUlid.Ulid NetUlid() => new(this.timestamp, this.randomness);

    [Benchmark]
    public global::System.Ulid Ulid() => System.Ulid.NewUlid(this.time, this.randomness);
}
