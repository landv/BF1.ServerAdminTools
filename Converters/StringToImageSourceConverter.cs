namespace BF1.ServerAdminTools.Converters;

public class StringToImageSourceConverter : IValueConverter
{
    #region Converter
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string path = (string)value;
        if (!string.IsNullOrEmpty(path))
        {
            return new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
        }
        else
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
    #endregion
}
