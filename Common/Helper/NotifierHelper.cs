using BF1.ServerAdminTools.Properties;

using Notification.Wpf;
using Notification.Wpf.Controls;
using Notification.Wpf.Constants;

namespace BF1.ServerAdminTools.Common.Helper;

public static class NotifierHelper
{
    private static readonly NotificationManager __NotificationManager = new();

    private const string AreaName = "WindowArea";
    private static TimeSpan ExpirationTime = TimeSpan.FromSeconds(2);

    static NotifierHelper()
    {
        Resources.Culture = Thread.CurrentThread.CurrentUICulture;

        NotificationConstants.MessagePosition = NotificationPosition.BottomCenter;
        NotificationConstants.NotificationsOverlayWindowMaxCount = 5;

        NotificationConstants.MinWidth = 700D;
        NotificationConstants.MaxWidth = 700D;

        NotificationConstants.FontName = "微软雅黑";
        NotificationConstants.TitleSize = 14;
        NotificationConstants.MessageSize = 12;
        NotificationConstants.MessageTextAlignment = TextAlignment.Left;
        NotificationConstants.TitleTextAlignment = TextAlignment.Left;

        NotificationConstants.DefaultBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444"));
        NotificationConstants.DefaultForegroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));

        NotificationConstants.InformationBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909399"));
        NotificationConstants.SuccessBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#67C23A"));
        NotificationConstants.WarningBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6A23C"));
        NotificationConstants.ErrorBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F56C6C"));
    }

    /// <summary>
    /// 显示Toast通知
    /// </summary>
    /// <param name="type"></param>
    /// <param name="message"></param>
    public static void Show(NotiferType type, string message)
    {
        string title;
        switch (type)
        {
            case NotiferType.None:
                title = "";
                break;
            case NotiferType.Information:
                title = "信息";
                break;
            case NotiferType.Success:
                title = "成功";
                break;
            case NotiferType.Warning:
                title = "警告";
                break;
            case NotiferType.Error:
                title = "错误";
                break;
            case NotiferType.Notification:
                title = "通知";
                break;
            default:
                title = "";
                break;
        }

        var clickContent = new NotificationContent
        {
            Title = title,
            Message = message,
            Type = (NotificationType)type,
            TrimType = NotificationTextTrimType.NoTrim,
        };

        __NotificationManager.Show(clickContent, AreaName, ExpirationTime, null, null, true, false);
    }
}

public enum NotiferType
{
    /// <summary>
    /// 无
    /// </summary>
    None,
    /// <summary>
    /// 信息
    /// </summary>
    Information,
    /// <summary>
    /// 成功
    /// </summary>
    Success,
    /// <summary>
    /// 警告
    /// </summary>
    Warning,
    /// <summary>
    /// 错误
    /// </summary>
    Error,
    /// <summary>
    /// 通知
    /// </summary>
    Notification
}
