using System.Globalization;
using System.Windows;
using PasswordManager.Converters;

namespace PasswordManager.Tests.Converters;

public class InverseBoolToVisConverterTests
{
    private readonly InverseBoolToVisConverter _sut;

    public InverseBoolToVisConverterTests()
    {
        _sut = new InverseBoolToVisConverter();
    }

    [Fact]
    public void Convert_True_ReturnsCollapsed()
    {
        var result = _sut.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_False_ReturnsVisible()
    {
        var result = _sut.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_NonBool_ReturnsVisible()
    {
        var result = _sut.Convert("not a bool", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_Null_ReturnsVisible()
    {
        var result = _sut.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void Convert_Integer_ReturnsVisible()
    {
        var result = _sut.Convert(42, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(
            () => _sut.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture));
    }
}
