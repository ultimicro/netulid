namespace NetUlid.Tests
{
    using System;
    using System.Buffers;
    using Xunit;

    public sealed class UlidTests
    {
        [Fact]
        public void Constructor_WithSpecifiedTimestampAndRandomness_BinaryRepresentationShouldInExpectedForm()
        {
            var subject = new Ulid(0xFFFFFFFFFF, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
            var result = subject.ToByteArray();

            Assert.Equal(new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }, result);
        }

        [Fact]
        public void Constructor_WithBinaryRepresentation_ResultShouldHaveTheSameBinaryRepresentation()
        {
            var binary = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };
            var subject = new Ulid(binary);
            var result = subject.ToByteArray();

            Assert.Equal(binary, result);
        }

        [Fact]
        public void Time_FromInstanceSpecifiedTimestampInGenerate_ShouldReturnDateTimeRepresentThatTimestamp()
        {
            var subject = Ulid.Generate(1620900032009);
            var result = subject.Time;

            Assert.Equal(new DateTime(2021, 5, 13, 10, 0, 32, 9, DateTimeKind.Utc), result);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Fact]
        public void Time_FromNullInstance_ShouldReturnUnixEpoch()
        {
            var result = Ulid.Null.Time;

            Assert.Equal(DateTime.UnixEpoch, result);
            Assert.Equal(DateTimeKind.Utc, result.Kind);
        }

        [Theory]
        [InlineData(Ulid.MinTimestamp)]
        [InlineData(Ulid.MaxTimestamp)]
        [InlineData(1L)]
        public void Timestamp_FromInstanceSpecifiedTimestampInGenerate_ShouldReturnTheSameValue(long timestamp)
        {
            var subject = Ulid.Generate(timestamp);

            Assert.Equal(timestamp, subject.Timestamp);
        }

        [Fact]
        public void Timestamp_FromNullInstance_ShouldReturnZero()
        {
            Assert.Equal(0, Ulid.Null.Timestamp);
        }

        [Fact]
        public void Randomness_FromNullInstance_ShouldReturnZeroBytes()
        {
            Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, Ulid.Null.Randomness);
        }

        [Fact]
        public void Randomness_FromNonNullInstance_ShouldReturnCorrespondingRandomness()
        {
            var rand = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };
            var subject = new Ulid(1616419012412, rand);

            Assert.Equal(rand, subject.Randomness);
        }

        [Fact]
        public void Generate_WithTimestampSameAsPrevious_ShouldIncreaseRandomness()
        {
            var timestamp = 1616419012412;
            var result1 = Ulid.Generate(timestamp);
            var result2 = Ulid.Generate(timestamp);

            Assert.Equal(result1.ToByteArray()[..^2], result2.ToByteArray()[..^2]);
            Assert.NotEqual(result1, result2);
            Assert.True(result1 < result2);
        }

        [Fact]
        public void Parse_WithValidCanonicalRepresentation_ShouldReturnCorrespondingValue()
        {
            var source = Ulid.Generate();
            var subject = Ulid.Parse(source.ToString());

            Assert.Equal(source, subject);
        }

        [Fact]
        public void Equals_WithSameValue_ShouldReturnTrue()
        {
            var subject = Ulid.Generate();
            var copy = subject;

            Assert.Equal(subject, copy);
        }

        [Fact]
        public void GetHashCode_WithSameValue_ShouldReturnSameValue()
        {
            var subject = Ulid.Generate();
            var h1 = subject.GetHashCode();
            var h2 = subject.GetHashCode();

            Assert.Equal(h1, h2);
        }

        [Fact]
        public void Write_WithEnoughOutput_ShouldCopyBinaryToOutput()
        {
            var binary = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };
            var subject = new Ulid(binary);
            using var result = MemoryPool<byte>.Shared.Rent(16);

            subject.Write(result.Memory.Span);

            Assert.Equal(binary, result.Memory.Span.Slice(0, 16).ToArray());
        }

        [Fact]
        public void ToString_OnNonNull_ShouldReturnCanonicalRepresentation()
        {
            // Test vector from https://github.com/ulid/javascript/blob/master/test.js
            var subject = new Ulid(1469918176385, new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
            var result = subject.ToString();

            Assert.StartsWith("01ARYZ6S41", result);
            Assert.Equal(26, result.Length);
        }
    }
}
