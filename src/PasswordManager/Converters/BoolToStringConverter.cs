using System.Globalization;
using System.Windows.Data;

namespace PasswordManager.Converters;

public class BoolToStringConverter : IValueConverter
{
    public string TrueValue { get; set; } = "True";
    public string FalseValue { get; set; } = "False";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Support ConverterParameter format: "TrueText|FalseText"
            if (parameter is string paramStr && paramStr.Contains('|'))
            {
                var parts = paramStr.Split('|');
                return boolValue ? parts[0] : parts[1];
            }
            return boolValue ? TrueValue : FalseValue;
        }
        return FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
