using BF1.ServerAdminTools.Views;
using BF1.ServerAdminTools.Models;
using BF1.ServerAdminTools.Common.Data;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.Core;
using BF1.ServerAdminTools.Features.Chat;
using BF1.ServerAdminTools.Windows.Kits;

using CommunityToolkit.Mvvm.Messaging;

namespace BF1.ServerAdminTools;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 主窗口全局提示信息委托
    /// </summary>
    public static Action<int, string> _SetOperatingState;

    public delegate void ClosingDispose();
    public static event ClosingDispose ClosingDisposeEvent;

    public static MainWindow ThisMainWindow;

    public MainModel MainModel { get; set; } = new();

    // 声明一个变量，用于存储软件开始运行的时间
    private DateTime Origin_DateTime;

    ///////////////////////////////////////////////////////

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Main_Loaded(object sender, RoutedEventArgs e)
    {
        // 提示信息委托
        _SetOperatingState = SetOperatingState;

        MainModel.AppRunTime = "运行时间 : Loading...";

        ThisMainWindow = this;

        ////////////////////////////////

        Title = CoreUtil.MainAppWindowName + CoreUtil.ClientVersionInfo
            + " - 最后编译时间 : " + File.GetLastWriteTime(Process.GetCurrentProcess().MainModule.FileName);

        // 获取当前时间，存储到对于变量中
        Origin_DateTime = DateTime.Now;

        ////////////////////////////////

        var thread0 = new Thread(UpdateState);
        thread0.IsBackground = true;
        thread0.Start();

        var therad1 = new Thread(InitThread);
        therad1.IsBackground = true;
        therad1.Start();

        this.DataContext = this;
    }

    private void Window_Main_Closing(object sender, CancelEventArgs e)
    {
        // 关闭事件
        ClosingDisposeEvent();
        LoggerHelper.Info($"调用关闭事件成功");

        // 写入SessionId
        IniHelper.WriteString("Globals", "SessionId", Globals.SessionId, FileUtil.F_Settings_Path);
        LoggerHelper.Info($"保存配置文件成功");

        SQLiteHelper.CloseConnection();
        LoggerHelper.Info($"关闭数据库链接成功");

        ChatMsg.FreeMemory();
        LoggerHelper.Info($"释放中文聊天指针内存成功");
        Memory.CloseHandle();
        LoggerHelper.Info($"释放目标进程句柄成功");

        Application.Current.Shutdown();
        LoggerHelper.Info($"程序关闭\n\n");
    }

    private async void InitThread()
    {
        // 读取SessionId
        Globals.SessionId = IniHelper.ReadString("Globals", "SessionId", "", FileUtil.F_Settings_Path);

        // 调用刷新SessionID功能
        LoggerHelper.Info($"开始调用刷新SessionID功能");
        WeakReferenceMessenger.Default.Send("", "RefreshData");

        ///////////////////////////////////////////////////////////

        CoreUtil.FlushDNSCache();
        LoggerHelper.Info("刷新DNS缓存成功");

        LoggerHelper.Info($"正在检测版本更新...");
        // 获取版本更新
        var webConfig = HttpHelper.HttpClientGET(CoreUtil.Config_Address).Result;
        if (!string.IsNullOrEmpty(webConfig))
        {
            var updateInfo = JsonUtil.JsonDese<UpdateInfo>(webConfig);

            CoreUtil.ServerVersionInfo = new Version(updateInfo.Version);
            CoreUtil.Notice_Address = updateInfo.Address.Notice;
            CoreUtil.Change_Address = updateInfo.Address.Change;

            // 获取最新公告
            await HttpHelper.HttpClientGET(CoreUtil.Notice_Address).ContinueWith((t) =>
            {
                if (t != null)
                    WeakReferenceMessenger.Default.Send(t.Result, "Notice");
                else
                    WeakReferenceMessenger.Default.Send("获取最新公告内容失败！", "Notice");
            });
            // 获取更新日志
            await HttpHelper.HttpClientGET(CoreUtil.Change_Address).ContinueWith((t) =>
            {
                if (t != null)
                    WeakReferenceMessenger.Default.Send(t.Result, "Change");
                else
                    WeakReferenceMessenger.Default.Send("获取更新日志信息失败！", "Change");
            });

            if (CoreUtil.ServerVersionInfo > CoreUtil.ClientVersionInfo)
            {
                LoggerHelper.Info($"发现新版本 {CoreUtil.ServerVersionInfo}");

                this.Dispatcher.Invoke(() =>
                {
                    if (MessageBox.Show($"检测到新版本已发布，是否立即前往更新？                   " +
                        $"\n\n{updateInfo.Latest.Date}\n{updateInfo.Latest.Change}\n\n强烈建议大家使用最新版本呢！",
                        "发现新版本", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                    {
                        var UpdateWindow = new UpdateWindow(updateInfo)
                        {
                            Owner = this
                        };
                        UpdateWindow.ShowDialog();
                    }
                });
            }
            else
            {
                LoggerHelper.Info($"当前已是最新版本 {CoreUtil.ServerVersionInfo}");
            }
        }
    }

    private void UpdateState()
    {
        while (true)
        {
            // 获取软件运行时间
            MainModel.AppRunTime = "运行时间 : " + CoreUtil.ExecDateDiff(Origin_DateTime, DateTime.Now);

            if (!ProcessUtil.IsAppRun(CoreUtil.TargetAppName))
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
                return;
            }

            Thread.Sleep(1000);
        }
    }

    #region 常用方法
    /// <summary>
    /// 提示信息，绿色信息1，灰色警告2，红色错误3
    /// </summary>
    /// <param name="index">绿色信息1，灰色警告2，红色错误3</param>
    /// <param name="str">消息内容</param>
    private void SetOperatingState(int index, string str)
    {
        if (index == 1)
        {
            Border_OperateState.Background = Brushes.Green;
            TextBlock_OperateState.Text = $"信息 : {str}";
        }
        else if (index == 2)
        {
            Border_OperateState.Background = Brushes.Gray;
            TextBlock_OperateState.Text = $"警告 : {str}";
        }
        else if (index == 3)
        {
            Border_OperateState.Background = Brushes.Red;
            TextBlock_OperateState.Text = $"错误 : {str}";
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessUtil.OpenLink(e.Uri.OriginalString);
        e.Handled = true;
    }
    #endregion
}
