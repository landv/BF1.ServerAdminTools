using BF1.ServerAdminTools.Common.Utils;

using Websocket.Client;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// RobotView.xaml 的交互逻辑
/// </summary>
public partial class RobotView : UserControl
{
    private bool _isBusy = false;

    public RobotView()
    {
        InitializeComponent();
        MainWindow.ClosingDisposeEvent += MainWindow_ClosingDisposeEvent;
    }

    private void MainWindow_ClosingDisposeEvent()
    {
        ProcessUtil.CloseProcess("go-cqhttp");
    }

    private void AppendLog(string txt)
    {
        this.Dispatcher.Invoke(() =>
        {
            TextBox_ConsoleLog.AppendText($"{txt}\r\n");
        });
    }

    private void Button_RunGoCqHttpServer_Click(object sender, RoutedEventArgs e)
    {
        if (ProcessUtil.IsAppRun("go-cqhttp"))
        {
            MsgBoxUtil.Information("请不要重复打开，go-cqhttp 程序已经在运行了");
            return;
        }

        var process = new Process();
        process.StartInfo.FileName = FileUtil.D_Robot_Path + "\\go-cqhttp.exe";
        process.StartInfo.CreateNoWindow = false;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.WorkingDirectory = FileUtil.D_Robot_Path;
        process.StartInfo.Arguments = "-faststart";
        process.Start();
    }

    private void Button_RunWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy)
        {
            MsgBoxUtil.Information("请不要重复打开，Websocket 程序已经在运行了");
            return;
        }

        _isBusy = true;

        Task.Run(() =>
        {
            var exitEvent = new ManualResetEvent(false);
            var url = new Uri("ws://127.0.0.1:8080");

            using (var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromMinutes(5);
                client.ReconnectionHappened.Subscribe(info =>
                AppendLog($"重新连接了, 类型: {info.Type}"));

                client.MessageReceived.Subscribe(msg => AppendLog($"收到信息: {msg}"));
                client.Start();

                exitEvent.WaitOne();
            }
        });
    }
}
