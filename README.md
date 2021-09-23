# NetUlid

This is a dependency-free, high quality and high performance [ULID](https://github.com/ulid/spec) implementation for
.NET 5. It is 100% conformance to the specifications.

| Package                        | Version                                                                                                                                   |
| ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- |
| NetUlid                        | [![Nuget](https://img.shields.io/nuget/v/NetUlid)](https://www.nuget.org/packages/NetUlid/)                                               |
| NetUlid.Npgsql                 | [![Nuget](https://img.shields.io/nuget/v/NetUlid.Npgsql)](https://www.nuget.org/packages/NetUlid.Npgsql/)                                 |
| NetUlid.Npgsql.EntityFramework | [![Nuget](https://img.shields.io/nuget/v/NetUlid.Npgsql.EntityFramework)](https://www.nuget.org/packages/NetUlid.Npgsql.EntityFramework/) |
| NetUlid.Tool                   | [![Nuget](https://img.shields.io/nuget/v/NetUlid.Tool)](https://www.nuget.org/packages/NetUlid.Tool)                                      |

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

The `Ulid` structure can be using as argument on the action (e.g. query string, route parameter, etc.) without any
additional works. The input will be accepted as canonical form.

### System.Text.Json

The `Ulid` structure has `JsonConverterAttribute` applied so that mean it can be using with `System.Text.Json` without
any additional works.

### Entity Framework Core

#### PostgreSQL via Npgsql

First you need to install
[NetUlid.Npgsql.EntityFramework](https://www.nuget.org/packages/NetUlid.Npgsql.EntityFramework/). Then you can
register `Ulid` support with the following code:

```csharp
using NetUlid;
using Npgsql;

public void ConfigureServices(IServiceCollection services)
{
    NpgsqlConnection.GlobalTypeMapper.UseNetUlid();
    
    services.AddDbContext<YourDbContext>(options =>
    {
        options.UseNpgsql("Your connection string", options =>
        {
            options.UseNetUlid();
        });
    });
}
```

Now you can use `Ulid` on the EF model. The data type on the PostgreSQL will be `bytea`. The query is fully
support `Ulid` comparison like `.Where(r => r.Id >= ulid)`.

## CLI Tool

We also provide [.NET Tool](https://www.nuget.org/packages/NetUlid.Tool) for ULID related tasks. Follow the guide how to
use .NET Tool [here](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) for the instructions how to
install and use it.

## The Ulid structure

The struct itself is in a binary form. What is mean is it can be interop with unmanaged code directly.

## License

MIT
