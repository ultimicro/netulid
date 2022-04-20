namespace NetUlid;

using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a Universally Unique Lexicographically Sortable Identifier (ULID).
/// </summary>
/// <remarks>
/// This is an implementation of https://github.com/ulid/spec.
/// </remarks>
[JsonConverter(typeof(UlidJsonConverter))]
[TypeConverter(typeof(UlidConverter))]
public unsafe struct Ulid : IComparable, IComparable<Ulid>, IEquatable<Ulid>
{
    /// <summary>
    /// Represents the largest possible value of timestamp part.
    /// </summary>
    public const long MaxTimestamp = 0xFFFFFFFFFFFF;

    /// <summary>
    /// Represents the smallest possible value of timestamp part.
    /// </summary>
    public const long MinTimestamp = 0;

    /// <summary>
    /// A read-only instance of <see cref="Ulid"/> whose value is all zeros.
    /// </summary>
    public static readonly Ulid Null = default;

    private const string Base32 = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    private static readonly long UnixEpochTicks = DateTime.UnixEpoch.Ticks;
    private static readonly byte[] InverseBase32 = new byte[]
    {
        255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // controls
        255, // space
        255, // !
        255, // "
        255, // #
        255, // $
        255, // %
        255, // &
        255, // '
        255, // (
        255, // )
        255, // *
        255, // +
        255, // ,
        255, // -
        255, // .
        255, // /
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, // 0-9
        255, 255, 255, 255, 255, 255, 255, // :-@
        10, 11, 12, 13, 14, 15, 16, 17, // A-H
        1, // I
        18, 19, // J-K
        1, // L
        20, 21, // M-N
        0, // O
        22, 23, 24, 25, 26, // P-T
        255, // U
        27, 28, 29, 30, 31, // V-Z
        255, 255, 255, 255, 255, 255, // [-`
        10, 11, 12, 13, 14, 15, 16, 17, // a-h
        1, // i
        18, 19, // j-k
        1, // l
        20, 21, // m-n
        0, // o
        22, 23, 24, 25, 26, // p-t
        255, // u
        27, 28, 29, 30, 31, // v-z
    };

    [ThreadStatic]
    private static Generation? lastGeneration;
    private fixed byte data[16];

    /// <summary>
    /// Initializes a new instance of the <see cref="Ulid"/> structure by using the specified timestamp and randomness.
    /// </summary>
    /// <param name="timestamp">
    /// The milliseconds since January 1, 1970 12:00 AM UTC.
    /// </param>
    /// <param name="randomness">
    /// The 80-bits cryptographically randomness.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="timestamp"/> is lower than <see cref="MinTimestamp"/> or greater than <see cref="MaxTimestamp"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="randomness"/> is not 10 bytes exactly.
    /// </exception>
    public Ulid(long timestamp, ReadOnlySpan<byte> randomness)
    {
        // Sanity checks.
        if (timestamp < MinTimestamp || timestamp > MaxTimestamp)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp));
        }

        if (randomness.Length != 10)
        {
            throw new ArgumentException("The value must be 10 bytes exactly.", nameof(randomness));
        }

        // Convert timestamp to big-endian.
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(buffer, timestamp);

        // Write data.
        fixed (byte* p = this.data)
        {
            buffer[2..].CopyTo(new Span<byte>(p, 6));
            randomness.CopyTo(new Span<byte>(p + 6, 10));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ulid"/> struct from the specified binary representation.
    /// </summary>
    /// <param name="binary">
    /// A <see cref="ReadOnlySpan{T}"/> containing binary representation.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="binary"/> is not 16 bytes exactly.
    /// </exception>
    public Ulid(ReadOnlySpan<byte> binary)
    {
        if (binary.Length != 16)
        {
            throw new ArgumentException("The value must be 16 bytes exactly.", nameof(binary));
        }

        fixed (void* p = this.data)
        {
            binary.CopyTo(new Span<byte>(p, 16));
        }
    }

    /// <summary>
    /// Gets time of this instance.
    /// </summary>
    /// <value>
    /// Time of this instance, in UTC.
    /// </value>
    /// <remarks>
    /// <see cref="DateTime.Kind"/> of the returned value will be <see cref="DateTimeKind.Utc"/>.
    /// </remarks>
    public DateTime Time => new(UnixEpochTicks + (this.Timestamp * TimeSpan.TicksPerMillisecond), DateTimeKind.Utc);

    /// <summary>
    /// Gets timestamp of this instance.
    /// </summary>
    /// <value>
    /// Timestamp of this instance, in milliseconds since January 1, 1970 12:00 AM UTC.
    /// </value>
    public long Timestamp
    {
        get
        {
            Span<byte> data = stackalloc byte[8];

            data[2] = this.data[0];
            data[3] = this.data[1];
            data[4] = this.data[2];
            data[5] = this.data[3];
            data[6] = this.data[4];
            data[7] = this.data[5];

            return BinaryPrimitives.ReadInt64BigEndian(data);
        }
    }

    public static bool operator ==(Ulid left, Ulid right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Ulid left, Ulid right)
    {
        return !(left == right);
    }

    public static bool operator <(Ulid left, Ulid right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Ulid left, Ulid right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Ulid left, Ulid right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Ulid left, Ulid right)
    {
        return left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Create a new <see cref="Ulid"/> with the current time as a timestamp.
    /// </summary>
    /// <returns>
    /// An <see cref="Ulid"/> with the current time as a timestamp and cryptographically randomness.
    /// </returns>
    /// <exception cref="OverflowException">
    /// The generate operation result in the same timestamp as the last generated value and the randomness incrementing is overflow.
    /// </exception>
    public static Ulid Generate() => Generate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

    /// <summary>
    /// Create a new <see cref="Ulid"/> with the specified timestamp.
    /// </summary>
    /// <param name="timestamp">
    /// Timestamp to use.
    /// </param>
    /// <returns>
    /// An <see cref="Ulid"/> with the specified timestamp and cryptographically randomness.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="timestamp"/> is less than <see cref="MinTimestamp"/> or greater than <see cref="MaxTimestamp"/>.
    /// </exception>
    /// <exception cref="OverflowException">
    /// <paramref name="timestamp"/> is the same as the last generated value and the randomness incrementing is overflow.
    /// </exception>
    public static Ulid Generate(long timestamp)
    {
        if (timestamp < MinTimestamp || timestamp > MaxTimestamp)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp));
        }

        // Generate randomness.
        Span<byte> randomness = stackalloc byte[10];

        RandomNumberGenerator.Fill(randomness);

        // Check if the current time is the same as last generation.
        if (lastGeneration == null)
        {
            lastGeneration = new Generation()
            {
                Timestamp = timestamp,
            };
        }
        else if (lastGeneration.Timestamp == timestamp)
        {
            // Increase randomness by one.
            var r = new BigInteger(lastGeneration.Randomness, true, true);
            var n = ++r;

            randomness.Fill(0);

            if (!n.TryWriteBytes(randomness, out _, true, true))
            {
                throw new OverflowException();
            }
        }
        else
        {
            lastGeneration.Timestamp = timestamp;
        }

        randomness.CopyTo(lastGeneration.Randomness);

        return new Ulid(timestamp, randomness);
    }

    /// <summary>
    /// Create an <see cref="Ulid"/> from canonical representation.
    /// </summary>
    /// <param name="s">
    /// Canonical representation to convert.
    /// </param>
    /// <returns>
    /// An <see cref="Ulid"/> whose value the same as <paramref name="s"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not a valid canonical representation.
    /// </exception>
    public static Ulid Parse(string s)
    {
        // Sanity check.
        if (s.Length != 26)
        {
            throw new FormatException();
        }

        // Convert base32.
        Span<byte> values = stackalloc byte[26];

        try
        {
            for (var i = 0; i < 26; i++)
            {
                if ((values[i] = InverseBase32[s[i]]) == 255)
                {
                    throw new FormatException($"{s} is not a valid ULID.");
                }
            }
        }
        catch (IndexOutOfRangeException ex)
        {
            throw new FormatException($"{s} is not a valid ULID.", ex);
        }

        if (values[0] > 7)
        {
            // The first character can contains only 3 bits.
            throw new FormatException();
        }

        // Decode.
        Span<byte> data = stackalloc byte[16];

        data[0x0] = (byte)((values[0] << 5) | values[1]);                               // |00[111|11111|][11111111][11111111][11111111][11111111][11111111]
        data[0x1] = (byte)((values[2] << 3) | (values[3] >> 2));                        // 00[11111111][|11111|111][11|111111][11111111][11111111][11111111]
        data[0x2] = (byte)((values[3] << 6) | (values[4] << 1) | (values[5] >> 4));     // 00[11111111][11111|111][11|11111|1][1111|1111][11111111][11111111]
        data[0x3] = (byte)((values[5] << 4) | (values[6] >> 1));                        // 00[11111111][11111111][1111111|1][1111|1111][1|1111111][11111111]
        data[0x4] = (byte)((values[6] << 7) | (values[7] << 2) | (values[8] >> 3));     // 00[11111111][11111111][11111111][1111|1111][1|11111|11][111|11111]
        data[0x5] = (byte)((values[8] << 5) | values[9]);                               // 00[11111111][11111111][11111111][11111111][111111|11][111|11111|]

        data[0x6] = (byte)((values[10] << 3) | (values[11] >> 2));                      // [|11111|111][11|111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
        data[0x7] = (byte)((values[11] << 6) | (values[12] << 1) | (values[13] >> 4));  // [11111|111][11|11111|1][1111|1111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
        data[0x8] = (byte)((values[13] << 4) | (values[14] >> 1));                      // [11111111][1111111|1][1111|1111][1|1111111][11111111][11111111][11111111][11111111][11111111][11111111]
        data[0x9] = (byte)((values[14] << 7) | (values[15] << 2) | (values[16] >> 3));  // [11111111][11111111][1111|1111][1|11111|11][111|11111][11111111][11111111][11111111][11111111][11111111]
        data[0xA] = (byte)((values[16] << 5) | values[17]);                             // [11111111][11111111][11111111][111111|11][111|11111|][11111111][11111111][11111111][11111111][11111111]
        data[0xB] = (byte)((values[18] << 3) | (values[19] >> 2));                      // [11111111][11111111][11111111][11111111][11111111][|11111|111][11|111111][11111111][11111111][11111111]
        data[0xC] = (byte)((values[19] << 6) | (values[20] << 1) | (values[21] >> 4));  // [11111111][11111111][11111111][11111111][11111111][11111|111][11|11111|1][1111|1111][11111111][11111111]
        data[0xD] = (byte)((values[21] << 4) | (values[22] >> 1));                      // [11111111][11111111][11111111][11111111][11111111][11111111][1111111|1][1111|1111][1|1111111][11111111]
        data[0xE] = (byte)((values[22] << 7) | (values[23] << 2) | (values[24] >> 3));  // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][1111|1111][1|11111|11][111|11111]
        data[0xF] = (byte)((values[24] << 5) | values[25]);                             // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111111|11][111|11111|]

        return new Ulid(data);
    }

    public int CompareTo(Ulid other)
    {
        fixed (void* p = this.data)
        {
            return new ReadOnlySpan<byte>(p, 16).SequenceCompareTo(new(other.data, 16));
        }
    }

    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            return 1;
        }
        else if (obj.GetType() != this.GetType())
        {
            throw new ArgumentException($"The value is not an instance of {this.GetType()}.", nameof(obj));
        }

        return this.CompareTo((Ulid)obj);
    }

    public bool Equals(Ulid other)
    {
        fixed (void* p = this.data)
        {
            return new ReadOnlySpan<byte>(p, 16).SequenceEqual(new(other.data, 16));
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((Ulid)obj);
    }

    public override int GetHashCode()
    {
        // https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-overriding-gethashcode
        var result = unchecked((int)2166136261);

        for (var i = 0; i < 16; i++)
        {
            result = (result * 16777619) ^ this.data[i];
        }

        return result;
    }

    /// <summary>
    /// Copy binary representation of this <see cref="Ulid"/> to the specified <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="output">
    /// The <see cref="Span{T}"/> to receive binary representation.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Not enough space in <paramref name="output"/>.
    /// </exception>
    public void Write(Span<byte> output)
    {
        if (output.Length < 16)
        {
            throw new ArgumentException("The size of buffer is not enough.", nameof(output));
        }

        fixed (void* p = this.data)
        {
            new ReadOnlySpan<byte>(p, 16).CopyTo(output);
        }
    }

    /// <summary>
    /// Converts the current value to binary representation.
    /// </summary>
    /// <returns>
    /// The binary representation of this value.
    /// </returns>
    public byte[] ToByteArray()
    {
        var result = new byte[16];

        for (var i = 0; i < 16; i++)
        {
            result[i] = this.data[i];
        }

        return result;
    }

    public override string ToString()
    {
        Span<char> result = stackalloc char[26];

        fixed (char* base32 = Base32)
        {
            // Encode timestamp.
            result[0] = base32[this.data[0] >> 5];                                  // |00[111|11111][11111111][11111111][11111111][11111111][11111111]
            result[1] = base32[this.data[0] & 0x1F];                                // 00[111|11111|][11111111][11111111][11111111][11111111][11111111]
            result[2] = base32[this.data[1] >> 3];                                  // 00[11111111][|11111|111][11111111][11111111][11111111][11111111]
            result[3] = base32[((this.data[1] & 0x7) << 2) | (this.data[2] >> 6)];  // 00[11111111][11111|111][11|111111][11111111][11111111][11111111]
            result[4] = base32[(this.data[2] >> 1) & 0x1F];                         // 00[11111111][11111111][11|11111|1][11111111][11111111][11111111]
            result[5] = base32[((this.data[2] & 0x1) << 4) | (this.data[3] >> 4)];  // 00[11111111][11111111][1111111|1][1111|1111][11111111][11111111]
            result[6] = base32[((this.data[3] & 0xF) << 1) | (this.data[4] >> 7)];  // 00[11111111][11111111][11111111][1111|1111][1|1111111][11111111]
            result[7] = base32[(this.data[4] >> 2) & 0x1F];                         // 00[11111111][11111111][11111111][11111111][1|11111|11][11111111]
            result[8] = base32[((this.data[4] & 0x3) << 3) | (this.data[5] >> 5)];  // 00[11111111][11111111][11111111][11111111][111111|11][111|11111]
            result[9] = base32[this.data[5] & 0x1F];                                // 00[11111111][11111111][11111111][11111111][11111111][111|11111|]

            // Encode randomness.
            result[10] = base32[(this.data[6] >> 3) & 0x1F];                            // [|11111|111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
            result[11] = base32[((this.data[6] & 0x7) << 2) | (this.data[7] >> 6)];     // [11111|111][11|111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
            result[12] = base32[(this.data[7] >> 1) & 0x1F];                            // [11111111][11|11111|1][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
            result[13] = base32[((this.data[7] & 0x1) << 4) | (this.data[8] >> 4)];     // [11111111][1111111|1][1111|1111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
            result[14] = base32[((this.data[8] & 0xF) << 1) | (this.data[9] >> 7)];     // [11111111][11111111][1111|1111][1|1111111][11111111][11111111][11111111][11111111][11111111][11111111]
            result[15] = base32[(this.data[9] >> 2) & 0x1F];                            // [11111111][11111111][11111111][1|11111|11][11111111][11111111][11111111][11111111][11111111][11111111]
            result[16] = base32[((this.data[9] & 0x3) << 3) | (this.data[10] >> 5)];    // [11111111][11111111][11111111][111111|11][111|11111][11111111][11111111][11111111][11111111][11111111]
            result[17] = base32[this.data[10] & 0x1F];                                  // [11111111][11111111][11111111][11111111][111|11111|][11111111][11111111][11111111][11111111][11111111]

            result[18] = base32[(this.data[11] >> 3) & 0x1F];                           // [11111111][11111111][11111111][11111111][11111111][|11111|111][11111111][11111111][11111111][11111111]
            result[19] = base32[((this.data[11] & 0x7) << 2) | (this.data[12] >> 6)];   // [11111111][11111111][11111111][11111111][11111111][11111|111][11|111111][11111111][11111111][11111111]
            result[20] = base32[(this.data[12] >> 1) & 0x1F];                           // [11111111][11111111][11111111][11111111][11111111][11111111][11|11111|1][11111111][11111111][11111111]
            result[21] = base32[((this.data[12] & 0x1) << 4) | (this.data[13] >> 4)];   // [11111111][11111111][11111111][11111111][11111111][11111111][1111111|1][1111|1111][11111111][11111111]
            result[22] = base32[((this.data[13] & 0xF) << 1) | (this.data[14] >> 7)];   // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][1111|1111][1|1111111][11111111]
            result[23] = base32[(this.data[14] >> 2) & 0x1F];                           // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][1|11111|11][11111111]
            result[24] = base32[((this.data[14] & 0x3) << 3) | (this.data[15] >> 5)];   // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111111|11][111|11111]
            result[25] = base32[this.data[15] & 0x1F];                                  // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111|11111|]
        }

        return new string(result);
    }

    private sealed class Generation
    {
        public long Timestamp { get; set; }

        public byte[] Randomness { get; } = new byte[10];
    }
}
