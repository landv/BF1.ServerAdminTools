﻿namespace BF1.ServerAdminTools.Common.Utils;

public static class MsgBoxUtil
{
    /// <summary>
    /// 通用信息弹窗，Information
    /// </summary>
    public static void Information(string msg)
    {
        MessageBox.Show(msg, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 通用警告弹窗，Warning
    /// </summary>
    public static void Warning(string msg)
    {
        MessageBox.Show(msg, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>
    /// 通用错误弹窗，Error
    /// </summary>
    public static void Error(string msg)
    {
        MessageBox.Show(msg, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// 通用异常弹窗，Exception
    /// </summary>
    public static void Exception(Exception ex)
    {
        MessageBox.Show("发生了未知的错误，查看错误提示以获取更多信息\n\n" +
            "错误信息 : \n" + ex.Message,
            "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
