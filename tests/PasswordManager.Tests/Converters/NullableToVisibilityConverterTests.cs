using System.Globalization;
using System.Windows;
using PasswordManager.Converters;

namespace PasswordManager.Tests.Converters;

public class NullableToVisibilityConverterTests
{
    private readonly NullableToVisibilityConverter _sut;

    public NullableToVisibilityConverterTests()
    {
        _sut = new NullableToVisibilityConverter();
    }

    [Fact]
    public void Convert_Null_ReturnsCollapsed()
    {
        var result = _sut.Convert(null, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_NonNullObject_ReturnsVisible()
    {
        var result = _sut.Convert(new object(), typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_EmptyString_ReturnsCollapsed()
    {
        var result = _sut.Convert("", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_NonEmptyString_ReturnsVisible()
    {
        var result = _sut.Convert("hello", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_WhitespaceString_ReturnsVisible()
    {
        var result = _sut.Convert("   ", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_Integer_ReturnsVisible()
    {
        var result = _sut.Convert(42, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_Zero_ReturnsVisible()
    {
        var result = _sut.Convert(0, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_False_ReturnsVisible()
    {
        var result = _sut.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_EmptyList_ReturnsVisible()
    {
        var result = _sut.Convert(new List<int>(), typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(
            () => _sut.ConvertBack(Visibility.Visible, typeof(object), null!, CultureInfo.InvariantCulture));
    }
}
