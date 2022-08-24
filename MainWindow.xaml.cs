using BF1.ServerAdminTools.Views;
using BF1.ServerAdminTools.Common.Data;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Windows.Kits;
using BF1.ServerAdminTools.Features.Core;
using BF1.ServerAdminTools.Features.Chat;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BF1.ServerAdminTools;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow
{
    public delegate void ClosingDispose();
    public static event ClosingDispose ClosingDisposeEvent;

    public static MainWindow ThisMainWindow;

    public RelayCommand<string> NavigateCommand { get; private set; }

    ///////////////////////////////////////////////////////

    private HomeView HomeView { get; set; } = new();
    private ServerView ServerView { get; set; } = new();
    private AuthView AuthView { get; set; } = new();
    private ScoreView ScoreView { get; set; } = new();
    private DetailView DetailView { get; set; } = new();
    private RuleView RuleView { get; set; } = new();
    private LogView LogView { get; set; } = new();
    private ChatView ChatView { get; set; } = new();
    private RobotView RobotView { get; set; } = new();
    private OptionView OptionView { get; set; } = new();
    private AboutView AboutView { get; set; } = new();

    ///////////////////////////////////////////////////////

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Main_Loaded(object sender, RoutedEventArgs e)
    {
        this.DataContext = this;
        ThisMainWindow = this;

        NavigateCommand = new(Navigate);
        // 首页导航
        ContentControl_Main.Content = HomeView;

        ////////////////////////////////

        Title = CoreUtil.MainAppWindowName + CoreUtil.ClientVersionInfo
            + " - 最后编译时间 : " + File.GetLastWriteTime(Process.GetCurrentProcess().MainModule.FileName);

        ////////////////////////////////

        var therad0 = new Thread(InitThread)
        {
            IsBackground = true
        };
        therad0.Start();
    }

    private void Window_Main_Closing(object sender, CancelEventArgs e)
    {
        // 关闭事件
        ClosingDisposeEvent();
        LoggerHelper.Info($"调用关闭事件成功");

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
        CoreUtil.FlushDNSCache();
        LoggerHelper.Info("刷新DNS缓存成功");

        LoggerHelper.Info($"正在检测版本更新...");
        this.Dispatcher.Invoke(() =>
        {
            NotifierHelper.Show(NotiferType.Notification, $"正在检测版本更新...");
        });

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
                this.Dispatcher.Invoke(() =>
                {
                    NotifierHelper.Show(NotiferType.Notification, $"当前已是最新版本 {CoreUtil.ServerVersionInfo}");
                });
            }
        }
    }

    /// <summary>
    /// View页面导航
    /// </summary>
    /// <param name="viewName"></param>
    private void Navigate(string viewName)
    {
        if (viewName == null || string.IsNullOrEmpty(viewName))
            return;

        switch (viewName)
        {
            case "HomeView":
                ContentControl_Main.Content = HomeView;
                break;
            case "ServerView":
                ContentControl_Main.Content = ServerView;
                break;
            case "AuthView":
                ContentControl_Main.Content = AuthView;
                break;
            case "ScoreView":
                ContentControl_Main.Content = ScoreView;
                break;
            case "DetailView":
                ContentControl_Main.Content = DetailView;
                break;
            case "RuleView":
                ContentControl_Main.Content = RuleView;
                break;
            case "LogView":
                ContentControl_Main.Content = LogView;
                break;
            case "ChatView":
                ContentControl_Main.Content = ChatView;
                break;
            case "RobotView":
                ContentControl_Main.Content = RobotView;
                break;
            case "OptionView":
                ContentControl_Main.Content = OptionView;
                break;
            case "AboutView":
                ContentControl_Main.Content = AboutView;
                break;
        }
    }
}
