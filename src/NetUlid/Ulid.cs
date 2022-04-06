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
    private static GenerationData? lastGeneration;
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
        this.data[0x00] = buffer[2];
        this.data[0x01] = buffer[3];
        this.data[0x02] = buffer[4];
        this.data[0x03] = buffer[5];
        this.data[0x04] = buffer[6];
        this.data[0x05] = buffer[7];
        this.data[0x06] = randomness[0];
        this.data[0x07] = randomness[1];
        this.data[0x08] = randomness[2];
        this.data[0x09] = randomness[3];
        this.data[0x0A] = randomness[4];
        this.data[0x0B] = randomness[5];
        this.data[0x0C] = randomness[6];
        this.data[0x0D] = randomness[7];
        this.data[0x0E] = randomness[8];
        this.data[0x0F] = randomness[9];
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

        for (var i = 0; i < 16; i++)
        {
            this.data[i] = binary[i];
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
    public DateTime Time => new DateTime((this.Timestamp * TimeSpan.TicksPerMillisecond) + UnixEpochTicks, DateTimeKind.Utc);

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

        // We don't generate this inside a lock due to most of the time the timestamp will be different.
        Span<byte> randomness = stackalloc byte[10];

        RandomNumberGenerator.Fill(randomness);

        // Check if the current time is the same as last generation.
        if (lastGeneration == null)
        {
            lastGeneration = new GenerationData()
            {
                Timestamp = timestamp,
            };
        }
        else if (lastGeneration.Timestamp == timestamp)
        {
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

        var c = GetValue(0);

        if (c > 7)
        {
            // The first character can contains only 3 bits.
            throw new FormatException();
        }

        // Decode.
        Span<byte> data = stackalloc byte[16];

        data[0x0] = (byte)((c << 5) | GetValue(1));                                     // |00[111|11111|][11111111][11111111][11111111][11111111][11111111]
        data[0x1] = (byte)((GetValue(2) << 3) | ((c = GetValue(3)) >> 2));              // 00[11111111][|11111|111][11|111111][11111111][11111111][11111111]
        data[0x2] = (byte)((c << 6) | (GetValue(4) << 1) | ((c = GetValue(5)) >> 4));   // 00[11111111][11111|111][11|11111|1][1111|1111][11111111][11111111]
        data[0x3] = (byte)((c << 4) | ((c = GetValue(6)) >> 1));                        // 00[11111111][11111111][1111111|1][1111|1111][1|1111111][11111111]
        data[0x4] = (byte)((c << 7) | (GetValue(7) << 2) | ((c = GetValue(8)) >> 3));   // 00[11111111][11111111][11111111][1111|1111][1|11111|11][111|11111]
        data[0x5] = (byte)((c << 5) | GetValue(9));                                     // 00[11111111][11111111][11111111][11111111][111111|11][111|11111|]

        data[0x6] = (byte)((GetValue(10) << 3) | ((c = GetValue(11)) >> 2));            // [|11111|111][11|111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
        data[0x7] = (byte)((c << 6) | (GetValue(12) << 1) | ((c = GetValue(13)) >> 4)); // [11111|111][11|11111|1][1111|1111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
        data[0x8] = (byte)((c << 4) | ((c = GetValue(14)) >> 1));                       // [11111111][1111111|1][1111|1111][1|1111111][11111111][11111111][11111111][11111111][11111111][11111111]
        data[0x9] = (byte)((c << 7) | (GetValue(15) << 2) | ((c = GetValue(16)) >> 3)); // [11111111][11111111][1111|1111][1|11111|11][111|11111][11111111][11111111][11111111][11111111][11111111]
        data[0xA] = (byte)((c << 5) | GetValue(17));                                    // [11111111][11111111][11111111][111111|11][111|11111|][11111111][11111111][11111111][11111111][11111111]
        data[0xB] = (byte)((GetValue(18) << 3) | ((c = GetValue(19)) >> 2));            // [11111111][11111111][11111111][11111111][11111111][|11111|111][11|111111][11111111][11111111][11111111]
        data[0xC] = (byte)((c << 6) | (GetValue(20) << 1) | ((c = GetValue(21)) >> 4)); // [11111111][11111111][11111111][11111111][11111111][11111|111][11|11111|1][1111|1111][11111111][11111111]
        data[0xD] = (byte)((c << 4) | ((c = GetValue(22)) >> 1));                       // [11111111][11111111][11111111][11111111][11111111][11111111][1111111|1][1111|1111][1|1111111][11111111]
        data[0xE] = (byte)((c << 7) | (GetValue(23) << 2) | ((c = GetValue(24)) >> 3)); // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][1111|1111][1|11111|11][111|11111]
        data[0xF] = (byte)((c << 5) | GetValue(25));                                    // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111111|11][111|11111|]

        return new Ulid(data);

        byte GetValue(int i)
        {
            byte v;

            try
            {
                v = InverseBase32[s[i]];
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new FormatException($"{s} is not a valid ULID.", ex);
            }

            return v == 255 ? throw new FormatException($"{s} is not a valid ULID.") : v;
        }
    }

    public int CompareTo(Ulid other)
    {
        for (var i = 0; i < 16; i++)
        {
            var l = this.data[i];
            var r = other.data[i];

            if (l < r)
            {
                return -1;
            }
            else if (l > r)
            {
                return 1;
            }
        }

        return 0;
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
        for (var i = 0; i < 16; i++)
        {
            if (this.data[i] != other.data[i])
            {
                return false;
            }
        }

        return true;
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

        for (var i = 0; i < 16; i++)
        {
            output[i] = this.data[i];
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

        // Encode timestamp.
        result[0] = Base32[this.data[0] >> 5];                                  // |00[111|11111][11111111][11111111][11111111][11111111][11111111]
        result[1] = Base32[this.data[0] & 0x1F];                                // 00[111|11111|][11111111][11111111][11111111][11111111][11111111]
        result[2] = Base32[this.data[1] >> 3];                                  // 00[11111111][|11111|111][11111111][11111111][11111111][11111111]
        result[3] = Base32[((this.data[1] & 0x7) << 2) | (this.data[2] >> 6)];  // 00[11111111][11111|111][11|111111][11111111][11111111][11111111]
        result[4] = Base32[(this.data[2] >> 1) & 0x1F];                         // 00[11111111][11111111][11|11111|1][11111111][11111111][11111111]
        result[5] = Base32[((this.data[2] & 0x1) << 4) | (this.data[3] >> 4)];  // 00[11111111][11111111][1111111|1][1111|1111][11111111][11111111]
        result[6] = Base32[((this.data[3] & 0xF) << 1) | (this.data[4] >> 7)];  // 00[11111111][11111111][11111111][1111|1111][1|1111111][11111111]
        result[7] = Base32[(this.data[4] >> 2) & 0x1F];                         // 00[11111111][11111111][11111111][11111111][1|11111|11][11111111]
        result[8] = Base32[((this.data[4] & 0x3) << 3) | (this.data[5] >> 5)];  // 00[11111111][11111111][11111111][11111111][111111|11][111|11111]
        result[9] = Base32[this.data[5] & 0x1F];                                // 00[11111111][11111111][11111111][11111111][11111111][111|11111|]

        // Encode randomness.
        result[10] = Base32[(this.data[6] >> 3) & 0x1F];                            // [|11111|111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
        result[11] = Base32[((this.data[6] & 0x7) << 2) | (this.data[7] >> 6)];     // [11111|111][11|111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
        result[12] = Base32[(this.data[7] >> 1) & 0x1F];                            // [11111111][11|11111|1][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
        result[13] = Base32[((this.data[7] & 0x1) << 4) | (this.data[8] >> 4)];     // [11111111][1111111|1][1111|1111][11111111][11111111][11111111][11111111][11111111][11111111][11111111]
        result[14] = Base32[((this.data[8] & 0xF) << 1) | (this.data[9] >> 7)];     // [11111111][11111111][1111|1111][1|1111111][11111111][11111111][11111111][11111111][11111111][11111111]
        result[15] = Base32[(this.data[9] >> 2) & 0x1F];                            // [11111111][11111111][11111111][1|11111|11][11111111][11111111][11111111][11111111][11111111][11111111]
        result[16] = Base32[((this.data[9] & 0x3) << 3) | (this.data[10] >> 5)];    // [11111111][11111111][11111111][111111|11][111|11111][11111111][11111111][11111111][11111111][11111111]
        result[17] = Base32[this.data[10] & 0x1F];                                  // [11111111][11111111][11111111][11111111][111|11111|][11111111][11111111][11111111][11111111][11111111]

        result[18] = Base32[(this.data[11] >> 3) & 0x1F];                           // [11111111][11111111][11111111][11111111][11111111][|11111|111][11111111][11111111][11111111][11111111]
        result[19] = Base32[((this.data[11] & 0x7) << 2) | (this.data[12] >> 6)];   // [11111111][11111111][11111111][11111111][11111111][11111|111][11|111111][11111111][11111111][11111111]
        result[20] = Base32[(this.data[12] >> 1) & 0x1F];                           // [11111111][11111111][11111111][11111111][11111111][11111111][11|11111|1][11111111][11111111][11111111]
        result[21] = Base32[((this.data[12] & 0x1) << 4) | (this.data[13] >> 4)];   // [11111111][11111111][11111111][11111111][11111111][11111111][1111111|1][1111|1111][11111111][11111111]
        result[22] = Base32[((this.data[13] & 0xF) << 1) | (this.data[14] >> 7)];   // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][1111|1111][1|1111111][11111111]
        result[23] = Base32[(this.data[14] >> 2) & 0x1F];                           // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][1|11111|11][11111111]
        result[24] = Base32[((this.data[14] & 0x3) << 3) | (this.data[15] >> 5)];   // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111111|11][111|11111]
        result[25] = Base32[this.data[15] & 0x1F];                                  // [11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][11111111][111|11111|]

        return new string(result);
    }

    private sealed class GenerationData
    {
        public long Timestamp { get; set; }

        public byte[] Randomness { get; } = new byte[10];
    }
}
