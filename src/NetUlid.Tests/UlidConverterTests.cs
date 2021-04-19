namespace NetUlid.Tests
{
    using System.ComponentModel;
    using Xunit;

    public sealed class UlidConverterTests
    {
        private readonly TypeConverter subject = TypeDescriptor.GetConverter(typeof(Ulid));

        [Fact]
        public void CanConvertFrom_WithSourceTypeIsString_ShouldReturnTrue()
        {
            Assert.True(this.subject.CanConvertFrom(typeof(string)));
        }

        [Fact]
        public void CanConvertTo_WithDestinationTypeIsString_ShouldReturnTrue()
        {
            Assert.True(this.subject.CanConvertTo(typeof(string)));
        }

        [Fact]
        public void ConvertFromString_WithValidCanonicalRepresentation_ShouldReturnCorrespondingUlid()
        {
            var source = Ulid.Generate();
            var result = this.subject.ConvertFromString(source.ToString());

            Assert.Equal(source, result);
        }

        [Fact]
        public void ConvertToString_WithAnyUlid_ShouldReturnCanonicalRepresentation()
        {
            var source = Ulid.Generate();
            var result = this.subject.ConvertToString(source);

            Assert.Equal(source.ToString(), result);
        }
    }
}
