using System.Globalization;
namespace AcademiaDoZe.Presentation.AppMaui.Converters;
public class DateOnlyToDateTimeConverter : IValueConverter
{
public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
{
    if (value is DateOnly dateOnly)
    return dateOnly.ToDateTime(TimeOnly.MinValue);
    return BindableProperty.UnsetValue;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
    if (value is DateTime dateTime)
    return DateOnly.FromDateTime(dateTime);
    return BindableProperty.UnsetValue;
}
}