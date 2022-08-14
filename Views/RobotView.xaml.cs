using BF1.ServerAdminTools.Common.Utils;

using RestSharp;
using Websocket.Client;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// RobotView.xaml 的交互逻辑
/// </summary>
public partial class RobotView : UserControl
{
    private Uri url = new("ws://127.0.0.1:8080");
    private WebsocketClient websocketClient = null;

    private static RestClient client = null;

    public RobotView()
    {
        InitializeComponent();
        MainWindow.ClosingDisposeEvent += MainWindow_ClosingDisposeEvent;

        var options = new RestClientOptions("http://127.0.0.1:5700")
        {
            ThrowOnAnyError = true,
            MaxTimeout = 5000
        };
        client = new RestClient(options);
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
            MsgBoxUtil.Warning("请不要重复打开，go-cqhttp 程序已经在运行了");
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
        if (!ProcessUtil.IsAppRun("go-cqhttp"))
        {
            MsgBoxUtil.Warning("请先启动 go-cqhttp 程序");
            return;
        }

        if (websocketClient != null)
        {
            MsgBoxUtil.Warning("请不要重复打开，Websocket 程序已经在运行了");
            return;
        }
        else
        {
            websocketClient = new(url);
            websocketClient.ReconnectTimeout = TimeSpan.FromMinutes(5);
            websocketClient.ReconnectionHappened.Subscribe(info => AppendLog($"客户端重新连接, 类型: {info.Type}"));

            websocketClient
                .MessageReceived
                .Where(msg => msg.Text != null)
                .Subscribe(msg => Message(msg));
            websocketClient.Start();
        }
    }

    private void Button_StopWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        if (websocketClient != null)
        {
            websocketClient.Dispose();
            websocketClient = null;
            AppendLog("客户端WebsocketServer链接关闭");
        }
    }

    private void Message(ResponseMessage msg)
    {
        var jNode = JsonNode.Parse(msg.Text);
        if (jNode["post_type"].GetValue<string>() == "meta_event")
        {
            return;
        }

        if (jNode["post_type"].GetValue<string>() == "message")
        {
            if (jNode["message_type"].GetValue<string>() == "group")
            {
                var group_id = jNode["group_id"].GetValue<int>();
                var raw_message = jNode["raw_message"].GetValue<string>();

                SendGroupMsg(group_id, raw_message);
            }
        }

        AppendLog($"收到信息: {msg}");
    }

    /// <summary>
    /// 发送群消息
    /// </summary>
    /// <param name="group_id"></param>
    /// <param name="message"></param>
    private void SendGroupMsg(int group_id, string message)
    {
        var request = new RestRequest("/send_msg")
            .AddQueryParameter("group_id", group_id)
            .AddQueryParameter("message", message)
            .AddQueryParameter("auto_escape", false);

        client.ExecuteGetAsync(request);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="user_id"></param>
    /// <param name="message"></param>
    private void SendMsg(int user_id, string message)
    {
        var request = new RestRequest("/send_msg")
            .AddQueryParameter("user_id", user_id)
            .AddQueryParameter("message", message)
            .AddQueryParameter("auto_escape", false);

        client.ExecuteGetAsync(request);
    }
}
