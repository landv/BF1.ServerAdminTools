using BF1.ServerAdminTools.Properties;

using Notification.Wpf;
using Notification.Wpf.Controls;
using Notification.Wpf.Constants;

namespace BF1.ServerAdminTools.Common.Helper;

public static class NotifierHelp
{
    private static readonly NotificationManager __NotificationManager = new();

    private const string AreaName = "WindowArea";
    private static TimeSpan ExpirationTime = TimeSpan.FromSeconds(2);

    static NotifierHelp()
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
    }

    public static void Show(string title, string message)
    {
        var clickContent = new NotificationContent
        {
            Title = title,
            Message = message,
            Type = NotificationType.Notification,
            CloseOnClick = true,
            TrimType = NotificationTextTrimType.Trim,
            RowsCount = 1,
        };

        __NotificationManager.Show(clickContent, AreaName, ExpirationTime);
    }
}
