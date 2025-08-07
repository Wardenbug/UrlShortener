using FluentAssertions;
using UrlShortener.Api.Services;

namespace UrlShortener.UnitTests.Services;
public class Base62ConverterTests
{
    [Fact]
    public void Encode_Should_ReturnCorrectValue()
    {
        // Arrange
        var converter = new Base62Converter();

        // Act
        string result = converter.Encode(0);

        // Assert
        result.Should().Be("a");
    }

    [Fact]
    public void Decode_Should_DistinguishBetween_CapitalsAndLowercase()
    {
        // Arrange
        var converter = new Base62Converter();

        // Act
        uint lowerA = converter.Decode("a"); // 'a' => 0
        uint upperA = converter.Decode("A"); // 'A' => 26

        // Assert
        lowerA.Should().NotBe(upperA);
    }

    [Fact]
    public void Decode_ShouldReturnZero_WhenInputLengthExceeds11Characters()
    {
        // Arrange
        var converter = new Base62Converter();

        // Act
        uint result = converter.Decode("aaaaaaaaaaaa");

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Decode_ShouldReturnZero_WhenInputIsEmpty()
    {
        // Arrange
        var converter = new Base62Converter();

        // Act
        uint result = converter.Decode("");

        // Assert
        result.Should().Be(0);
    }


    [Fact]
    public void Decode_Should_ReturnCorrectValue()
    {
        // Arrange
        var converter = new Base62Converter();

        // Act
        uint result = converter.Decode("a");

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(62)]
    [InlineData(999999)]
    [InlineData(uint.MaxValue)]
    public void EncodeDecode_RoundTrip_ShouldReturnOriginalValue(uint value)
    {
        // Arrange
        var converter = new Base62Converter();

        // Act
        string encoded = converter.Encode(value);
        uint result = converter.Decode(encoded);

        // Assert
        result.Should().Be(value);
    }
}
