namespace BF1.ServerAdminTools.Assets.Styles.Attached;

public class Text
{
    public static string GetValue(DependencyObject obj)
    {
        return (string)obj.GetValue(ValueProperty);
    }

    public static void SetValue(DependencyObject obj, string value)
    {
        obj.SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.RegisterAttached("Value", typeof(string), typeof(Text), new PropertyMetadata(default));
}
