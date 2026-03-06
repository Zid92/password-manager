#nullable disable
using System.Globalization;
using PasswordManager.Converters;

namespace PasswordManager.Tests.Converters;

public class BoolToStringConverterTests
{
    private readonly BoolToStringConverter _sut;

    public BoolToStringConverterTests()
    {
        _sut = new BoolToStringConverter();
    }

    [Fact]
    public void Convert_TrueWithoutParameter_ReturnsDefaultTrueValue()
    {
        var result = _sut.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("True", result);
    }

    [Fact]
    public void Convert_FalseWithoutParameter_ReturnsDefaultFalseValue()
    {
        var result = _sut.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("False", result);
    }

    [Fact]
    public void Convert_TrueWithParameter_ReturnsFirstPart()
    {
        var result = _sut.Convert(true, typeof(string), "Enabled|Disabled", CultureInfo.InvariantCulture);

        Assert.Equal("Enabled", result);
    }

    [Fact]
    public void Convert_FalseWithParameter_ReturnsSecondPart()
    {
        var result = _sut.Convert(false, typeof(string), "Enabled|Disabled", CultureInfo.InvariantCulture);

        Assert.Equal("Disabled", result);
    }

    [Theory]
    [InlineData("On|Off", true, "On")]
    [InlineData("On|Off", false, "Off")]
    [InlineData("Active|Inactive", true, "Active")]
    [InlineData("Active|Inactive", false, "Inactive")]
    [InlineData("1|0", true, "1")]
    [InlineData("1|0", false, "0")]
    public void Convert_WithVariousParameters_ReturnsExpected(string parameter, bool input, string expected)
    {
        var result = _sut.Convert(input, typeof(string), parameter, CultureInfo.InvariantCulture);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_WithInvalidParameter_UsesDefault()
    {
        var result = _sut.Convert(true, typeof(string), "InvalidNoSeparator", CultureInfo.InvariantCulture);

        Assert.Equal("True", result);
    }

    [Fact]
    public void Convert_NullValue_ReturnsFalseValue()
    {
        var result = _sut.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("False", result);
    }

    [Fact]
    public void Convert_NonBoolValue_ReturnsFalseValue()
    {
        var result = _sut.Convert("not a bool", typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("False", result);
    }

    [Fact]
    public void Convert_WithCustomTrueAndFalseValues_ReturnsCustomValues()
    {
        var converter = new BoolToStringConverter
        {
            TrueValue = "Yes",
            FalseValue = "No"
        };

        var trueResult = converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);
        var falseResult = converter.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("Yes", trueResult);
        Assert.Equal("No", falseResult);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(
            () => _sut.ConvertBack("Yes", typeof(bool), null, CultureInfo.InvariantCulture));
    }
}
