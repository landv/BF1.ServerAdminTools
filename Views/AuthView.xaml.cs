using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.Core;
using BF1.ServerAdminTools.Features.Data;
using BF1.ServerAdminTools.Features.API;
using BF1.ServerAdminTools.Features.API.RespJson;

using CommunityToolkit.Mvvm.Messaging;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// AuthView.xaml 的交互逻辑
/// </summary>
public partial class AuthView : UserControl
{
    public AuthView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.ClosingDisposeEvent += MainWindow_ClosingDisposeEvent;

        var timerAutoRefresh = new Timer
        {
            AutoReset = true,
            Interval = TimeSpan.FromMinutes(5).TotalMilliseconds
        };
        timerAutoRefresh.Elapsed += TimerAutoRefresh_Elapsed;
        timerAutoRefresh.Start();

        WeakReferenceMessenger.Default.Register<string, string>(this, "RefreshData", (s, e) =>
        {
            LoggerHelper.Info($"调用刷新SessionID功能成功");
            TimerAutoRefresh_Elapsed(null, null);
        });
    }

    private void MainWindow_ClosingDisposeEvent()
    {

    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessUtil.OpenLink(e.Uri.OriginalString);
        e.Handled = true;
    }

    private void TimerAutoRefresh_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            var str = Search.SearchMemory(Offsets.SessionIDMask);
            if (str != string.Empty)
            {
                Globals.SessionId = str;
                LoggerHelper.Info($"获取SessionID成功 {Globals.SessionId}");
            }
            else
            {
                LoggerHelper.Error($"获取SessionID失败");
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.Error($"获取SessionID失败", ex);
        }
    }

    private async void Button_VerifyPlayerSessionId_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        if (!string.IsNullOrEmpty(Globals.SessionId))
        {
            TextBlock_CheckSessionIdStatus.Text = "正在验证中，请等待...";
            TextBlock_CheckSessionIdStatus.Background = Brushes.Gray;
            NotifierHelper.Show(NotiferType.Information, "正在验证中，请等待...");

            await BF1API.SetAPILocale();
            var result = await BF1API.GetWelcomeMessage();

            if (result.IsSuccess)
            {
                var welcomeMsg = JsonUtil.JsonDese<WelcomeMsg>(result.Message);

                var msg = ChsUtil.ToSimplifiedChinese(welcomeMsg.result.firstMessage);

                TextBlock_CheckSessionIdStatus.Text = msg;
                TextBlock_CheckSessionIdStatus.Background = Brushes.Green;

                NotifierHelper.Show(NotiferType.Success, $"验证成功 {msg}  |  耗时: {result.ExecTime:0.00} 秒");
            }
            else
            {
                TextBlock_CheckSessionIdStatus.Text = "验证失败";
                TextBlock_CheckSessionIdStatus.Background = Brushes.Red;

                NotifierHelper.Show(NotiferType.Error, $"验证失败 {result.Message}  |  耗时: {result.ExecTime:0.00} 秒");
            }
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "请先获取玩家SessionID后，再执行本操作");
        }
    }

    private void Button_GetPlayerSessionId_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        try
        {
            Task.Run(() =>
            {
                NotifierHelper.Show(NotiferType.Information, "正在获取中，请等待...");

                var str = Search.SearchMemory(Offsets.SessionIDMask);
                if (str != string.Empty)
                {
                    Globals.SessionId = str;
                    NotifierHelper.Show(NotiferType.Success, $"获取玩家SessionID成功");
                }
                else
                {
                    LoggerHelper.Error($"获取玩家SessionID失败");
                    NotifierHelper.Show(NotiferType.Error, $"获取玩家SessionID失败");
                }
            });
        }
        catch (Exception ex)
        {
            LoggerHelper.Error($"获取玩家SessionID失败", ex);
        }
    }
}
