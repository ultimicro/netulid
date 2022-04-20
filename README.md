# NetUlid

This is a dependency-free, high quality and high performance [ULID](https://github.com/ulid/spec) implementation for .NET. It is 100% conformance to the
specifications.

| Package                        | Version                                                                                                                                   |
| ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- |
| NetUlid                        | [![Nuget](https://img.shields.io/nuget/v/NetUlid)](https://www.nuget.org/packages/NetUlid/)                                               |
| NetUlid.Tool                   | [![Nuget](https://img.shields.io/nuget/v/NetUlid.Tool)](https://www.nuget.org/packages/NetUlid.Tool)                                      |

## Benchmark

The following is a result of benchmark agains [Ulid](https://github.com/Cysharp/Ulid) and [NUlid](https://github.com/RobThree/NUlid):

```ini
BenchmarkDotNet=v0.13.1, OS=arch
AMD Ryzen 7 PRO 4750U with Radeon Graphics, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.102
  [Host]     : .NET 6.0.2 (6.0.222.11801), X64 RyuJIT
  DefaultJob : .NET 6.0.2 (6.0.222.11801), X64 RyuJIT
```

|                                Type |           Method |        Mean |     Error |    StdDev |  Gen 0 | Allocated |
|------------------------------------ |----------------- |------------:|----------:|----------:|-------:|----------:|
|                      ParseCanonical |          NetUlid |  33.5171 ns | 0.2185 ns | 0.2044 ns |      - |         - |
|                      ParseCanonical |             Ulid |  24.1547 ns | 0.0795 ns | 0.0744 ns |      - |         - |
|                      ParseCanonical |            NUlid | 138.4496 ns | 0.1844 ns | 0.1725 ns | 0.0019 |     168 B |
|                            ToString |          NetUlid |  29.6122 ns | 0.1598 ns | 0.1495 ns | 0.0010 |      80 B |
|                            ToString |             Ulid |  35.0151 ns | 0.3129 ns | 0.2927 ns | 0.0010 |      80 B |
|                            ToString |            NUlid |  99.5217 ns | 0.1938 ns | 0.1718 ns | 0.0043 |     360 B |
|                              Equals |          NetUlid |   3.7180 ns | 0.0023 ns | 0.0019 ns |      - |         - |
|                              Equals |             Ulid |   0.9967 ns | 0.0123 ns | 0.0115 ns |      - |         - |
|                              Equals |            NUlid |  30.2570 ns | 0.0865 ns | 0.0810 ns | 0.0010 |      80 B |
|                           CompareTo |          NetUlid |   3.1449 ns | 0.0059 ns | 0.0055 ns |      - |         - |
|                           CompareTo |             Ulid |   4.3326 ns | 0.0313 ns | 0.0293 ns |      - |         - |
|                           CompareTo |            NUlid |  16.3404 ns | 0.0404 ns | 0.0378 ns | 0.0005 |      40 B |
|                             GetTime | NetUlidTimestamp |   5.6709 ns | 0.0040 ns | 0.0038 ns |      - |         - |
|                             GetTime |          NetUlid |   7.5541 ns | 0.0293 ns | 0.0274 ns |      - |         - |
|                             GetTime |             Ulid |  29.6340 ns | 0.0055 ns | 0.0052 ns |      - |         - |
|                             GetTime |            NUlid |  38.6922 ns | 0.1105 ns | 0.1034 ns | 0.0007 |      64 B |
|                 ConstructFromBinary |          NetUlid |   4.8743 ns | 0.0331 ns | 0.0277 ns |      - |         - |
|                 ConstructFromBinary |             Ulid |   2.2938 ns | 0.0037 ns | 0.0034 ns |      - |         - |
|                 ConstructFromBinary |            NUlid |   8.5336 ns | 0.0006 ns | 0.0005 ns |      - |         - |
| ConstructFromTimestampAndRandomness |          NetUlid |  10.4408 ns | 0.2397 ns | 0.2243 ns |      - |         - |
| ConstructFromTimestampAndRandomness |             Ulid |   8.3920 ns | 0.0023 ns | 0.0019 ns |      - |         - |

The reasons for no ULID generating benchmark is because both `Ulid` and `NUlid` fail to conforming to the specifications when timestamp is the same as
previously generated.

## When to use ULID

You should use ULID if you plan to migrate your data to the distributed database in the future and your data requires
sorting. What sorting mean not only display sorting but also including data paging. If your data don't need sorting
ability you should stick with `Guid`. Keep in mind that ULID will expose data creation time automatically to anyone who
can access it.

## Different from GUID

The main difference with `Guid` is ULID is sortable, which will make it shine on distributed system. You can think it is
a data type to replace auto-increment in SQL, but for distributed database like Cassandra.

## Different from UUID version 1

UUID version 1 (AKA timestamp UUID) require a special logic on the database in order to sort it correctly while ULID is
not.

## Different from Twitter Snowflake

ULID and Twitter Snowflake is very similar. In term of functionalities Twitter Snowflake is better due to it can embed
additional data. So if you need to embed additional data to the value you should use Twitter Snowflake instead.

## Usage

### Generate a new ULID with current time

```csharp
using NetUlid;

var ulid = Ulid.Generate();
```

### Create a ULID with specified timestamp and randomness

```csharp
using NetUlid;

var ulid = new Ulid(timestamp, randomness);
```

### Create a ULID from binary representation

```csharp
using NetUlid;

var ulid = new Ulid(binary);
```

### Compare two ULIDs

```csharp
if (ulid1 > ulid2)
{
    // Do something.
}
else if (ulid1 < ulid2)
{
    // Do something.
}
else if (ulid1 == ulid2)
{
    // Do something.
}
```

### Get a raw binary representation

```csharp
var binary = ulid.ToByteArray();
```

### Get canonical representation

```csharp
var canonical = ulid.ToString();
```

### Create a ULID from canonical representation

```csharp
using NetUlid;

var ulid = Ulid.Parse(canonical);
```

## Integration with other libraries

### ASP.NET Core

The `Ulid` structure can be using as argument on the action (e.g. query string, route parameter, etc.) without any additional works. The input will be accepted
as canonical form.

### System.Text.Json

The `Ulid` structure has `JsonConverterAttribute` applied so that mean it can be using with `System.Text.Json` without any additional works.

## CLI Tool

We also provide [.NET Tool](https://www.nuget.org/packages/NetUlid.Tool) for ULID related tasks. Follow the guide how to
use .NET Tool [here](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) for the instructions how to
install and use it.

## The Ulid structure

The struct itself is in a binary form. What is mean is it can be interop with unmanaged code directly.

## License

MIT
