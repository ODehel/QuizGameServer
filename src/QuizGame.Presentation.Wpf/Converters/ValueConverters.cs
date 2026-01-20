using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QuizGame.Presentation.Wpf.Converters;

public class NullToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null ? 1.0 : 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToInverseOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null ? 0.0 : 1.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LogTypeToColorConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length > 0 && values[0] != null)
        {
            var logType = values[0].ToString();
            return logType switch
            {
                "Info" => new SolidColorBrush(Color.FromRgb(47, 62, 80)),      // #2F3E50
                "Success" => new SolidColorBrush(Color.FromRgb(39, 174, 96)),   // #27AE60 - Vert
                "Error" => new SolidColorBrush(Color.FromRgb(231, 76, 60)),     // #E74C3C - Rouge
                _ => new SolidColorBrush(Color.FromRgb(127, 140, 141))          // #7F8C8D - Gris
            };
        }

        return new SolidColorBrush(Color.FromRgb(127, 140, 141));
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
