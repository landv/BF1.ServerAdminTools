using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Features.Chat;
using BF1.ServerAdminTools.Features.Core;
using BF1.ServerAdminTools.Features.Config;
using BF1.ServerAdminTools.Features.Data;

using RestSharp;
using Websocket.Client;

using System.Drawing;
using System.Drawing.Imaging;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// RobotView.xaml 的交互逻辑
/// </summary>
public partial class RobotView : UserControl
{
    private readonly Uri url = new("ws://127.0.0.1:8080");
    private static WebsocketClient websocketClient = null;
    private static RestClient client = null;

    public static Action<ChangeTeamInfo> _dSendChangeTeamInfo;

    private RobotConfig RobotConfig;

    private List<long> QQGroupList = new();

    public RobotView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.ClosingDisposeEvent += MainWindow_ClosingDisposeEvent;

        var options = new RestClientOptions("http://127.0.0.1:5700")
        {
            ThrowOnAnyError = true,
            MaxTimeout = 5000
        };
        client = new RestClient(options);

        _dSendChangeTeamInfo = SendChangeTeamLogToQQ;

        //////////////////////////////////////////////////////////////////

        if (File.Exists(FileUtil.F_Robot_Path))
        {
            using (var streamReader = new StreamReader(FileUtil.F_Robot_Path))
            {
                RobotConfig = JsonUtil.JsonDese<RobotConfig>(streamReader.ReadToEnd());

                TextBox_QQGroupID.Text = RobotConfig.QQGroupID.ToString();

                CheckBox_IgnoreQQGroupLimit.IsChecked = RobotConfig.IsIgnoreQQGroupLimit;
                CheckBox_IgnoreQQGroupMemberLimit.IsChecked = RobotConfig.IsIgnoreQQGroupMemberLimit;

                CheckBox_IsSendChangeTeam.IsChecked = RobotConfig.IsSendChangeTeam;

                foreach (var item in RobotConfig.QQGroupMemberID)
                {
                    ListBox_QQGroupMemberIDs.Items.Add(item);
                }
            }
        }
        else
        {
            RobotConfig = new RobotConfig()
            {
                IsIgnoreQQGroupLimit = false,
                IsIgnoreQQGroupMemberLimit = false,
                QQGroupID = 0,
                QQGroupMemberID = new List<long>() { },
                IsSendChangeTeam = false
            };
        }
    }

    private void MainWindow_ClosingDisposeEvent()
    {
        ProcessUtil.CloseThirdProcess();

        SaveRobotConfig();
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
        AudioUtil.ClickSound();

        if (ProcessUtil.IsAppRun("go-cqhttp"))
        {
            AppendLog("请不要重复打开，go-cqhttp 程序已经在运行了");
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
        AudioUtil.ClickSound();

        if (!ProcessUtil.IsAppRun("go-cqhttp"))
        {
            AppendLog("请先启动 go-cqhttp 程序");
            return;
        }

        if (websocketClient != null)
        {
            AppendLog("请不要重复打开，Websocket 程序已经在运行了");
            return;
        }
        else
        {
            SaveRobotConfig();

            //////////////////////////////////////////////////////////////////

            websocketClient = new(url)
            {
                ReconnectTimeout = TimeSpan.FromMinutes(5)
            };
            websocketClient.ReconnectionHappened.Subscribe(async info =>
            {
                AppendLog($"客户端重新连接, 类型: {info.Type}");
                QQGroupList.Clear();

                var request = new RestRequest("/get_group_list");
                var result = await client.ExecuteGetAsync(request);
                var jNode = JsonNode.Parse(result.Content);
                if (jNode["data"] is JsonArray ja)
                {
                    for (int i = 0; i < ja.Count; i++)
                    {
                        QQGroupList.Add(ja[i]["group_id"].GetValue<long>());
                    }
                }
            });

            websocketClient
                .MessageReceived
                .Where(msg => msg.Text != null)
                .Subscribe(msg => MessageHandling(msg));
            websocketClient.Start();
        }
    }

    private void Button_StopWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        if (websocketClient != null)
        {
            websocketClient.Dispose();
            websocketClient = null;
            AppendLog("客户端WebsocketServer链接关闭");
        }
    }

    /// <summary>
    /// 接收的消息处理
    /// </summary>
    /// <param name="msg"></param>
    private void MessageHandling(ResponseMessage msg)
    {
        var jNode = JsonNode.Parse(msg.Text);
        // 过滤心跳消息
        if (jNode["post_type"].GetValue<string>() == "meta_event")
            return;

        if (jNode["post_type"].GetValue<string>() == "message")
        {
            if (jNode["message_type"].GetValue<string>() == "group")
            {
                var user_id = jNode["user_id"].GetValue<long>();
                var group_id = jNode["group_id"].GetValue<long>();

                if (!RobotConfig.IsIgnoreQQGroupLimit)
                {
                    if (group_id != RobotConfig.QQGroupID)
                        return;

                    if (!RobotConfig.IsIgnoreQQGroupMemberLimit)
                    {
                        if (!RobotConfig.QQGroupMemberID.Contains(user_id))
                            return;
                    }
                }

                var raw_message = jNode["raw_message"].GetValue<string>().Trim();

                if (raw_message.StartsWith("#中文聊天"))
                {
                    AppendLog($"收到群信息: {msg}");

                    raw_message = raw_message.Replace("#中文聊天", "").Trim();
                    if (!string.IsNullOrEmpty(raw_message))
                    {
                        SendChatChsRetrunImg(group_id, raw_message);
                    }
                    else
                    {
                        SendGroupMsg(group_id, "错误：请发送正确的中文聊天内容！");
                    }
                }

                if (raw_message.Equals("#屏幕截图"))
                {
                    AppendLog($"收到群信息: {msg}");

                    GetPrintScreen(group_id);
                }

                if (raw_message.Equals("#得分板截图"))
                {
                    AppendLog($"收到群信息: {msg}");

                    GetScorePrintScreen(group_id);
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 发送换边通知到QQ群
    /// </summary>
    /// <param name="info"></param>
    private void SendChangeTeamLogToQQ(ChangeTeamInfo info)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== 换边通知 ===");
        sb.AppendLine($"操作时间: {DateTime.Now}");
        sb.AppendLine($"玩家等级: {info.Rank}");
        sb.AppendLine($"玩家ID: {info.Name}");
        sb.AppendLine($"玩家数字ID: {info.PersonaId}");
        sb.AppendLine($"队伍比分: {info.Team1Score} 🆚 {info.Team2Score}");
        sb.Append($"状态: {info.Status}");

        if (RobotConfig.IsSendChangeTeam)
        {
            if (QQGroupList.Contains(RobotConfig.QQGroupID))
            {
                SendGroupMsg(RobotConfig.QQGroupID, sb.ToString());
            }
        }
    }

    /// <summary>
    /// 发送战地1中文聊天并返回聊天截图
    /// </summary>
    /// <param name="group_id"></param>
    /// <param name="message"></param>
    private void SendChatChsRetrunImg(long group_id, string message)
    {
        ChatHelper.SendTextToBf1Game(message);

        var windowData = Memory.GetGameWindowData();
        windowData.Width /= 3;

        var bitmap = new Bitmap(windowData.Width, windowData.Height);
        var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(new Point(windowData.Left, windowData.Top), new Point(0, 0), new Size(windowData.Width, windowData.Height));

        var file = $"BF1#{DateTime.Now:yyyyMMdd_HH-mm-ss-ffff}.png";
        var path = $"{FileUtil.D_Robot_Path}\\data\\images\\{file}";
        bitmap.Save(path, ImageFormat.Png);
        graphics.Dispose();

        SendGroupMsg(group_id, $"[CQ:image,file={file}]");
    }

    /// <summary>
    /// 获取战地1屏幕截图
    /// </summary>
    private void GetPrintScreen(long group_id)
    {
        Memory.SetForegroundWindow();
        Thread.Sleep(50);

        var windowData = Memory.GetGameWindowData();

        var bitmap = new Bitmap(windowData.Width, windowData.Height);
        var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(new Point(windowData.Left, windowData.Top), new Point(0, 0), new Size(windowData.Width, windowData.Height));

        var file = $"BF1#{DateTime.Now:yyyyMMdd_HH-mm-ss-ffff}.png";
        var path = $"{FileUtil.D_Robot_Path}\\data\\images\\{file}";
        bitmap.Save(path, ImageFormat.Png);
        graphics.Dispose();

        SendGroupMsg(group_id, $"[CQ:image,file={file}]");
    }

    /// <summary>
    /// 获取战地1得分板屏幕截图
    /// </summary>
    private void GetScorePrintScreen(long group_id)
    {
        Task.Run(() =>
        {
            Memory.SetForegroundWindow();
            Task.Delay(50).Wait();

            var windowData = Memory.GetGameWindowData();

            var bitmap = new Bitmap(windowData.Width, windowData.Height);
            var graphics = Graphics.FromImage(bitmap);

            WinAPI.Keybd_Event(WinVK.TAB, WinAPI.MapVirtualKey(WinVK.TAB, 0), 0, 0);
            Task.Delay(2000).Wait();
            graphics.CopyFromScreen(new Point(windowData.Left, windowData.Top), new Point(0, 0), new Size(windowData.Width, windowData.Height));
            Task.Delay(50).Wait();
            WinAPI.Keybd_Event(WinVK.TAB, WinAPI.MapVirtualKey(WinVK.TAB, 0), 2, 0);

            var file = $"BF1#{DateTime.Now:yyyyMMdd_HH-mm-ss-ffff}.png";
            var path = $"{FileUtil.D_Robot_Path}\\data\\images\\{file}";
            bitmap.Save(path, ImageFormat.Png);
            graphics.Dispose();

            SendGroupMsg(group_id, $"[CQ:image,file={file}]");
        });
    }

    /// <summary>
    /// 发送群消息
    /// </summary>
    /// <param name="group_id"></param>
    /// <param name="message"></param>
    private void SendGroupMsg(long group_id, string message)
    {
        var request = new RestRequest("/send_msg")
            .AddQueryParameter("group_id", group_id)
            .AddQueryParameter("message", message)
            .AddQueryParameter("auto_escape", false);

        client.ExecuteGetAsync(request);
    }

    /// <summary>
    /// 发送普通消息
    /// </summary>
    /// <param name="user_id"></param>
    /// <param name="message"></param>
    private void SendMsg(long user_id, string message)
    {
        var request = new RestRequest("/send_msg")
            .AddQueryParameter("user_id", user_id)
            .AddQueryParameter("message", message)
            .AddQueryParameter("auto_escape", false);

        client.ExecuteGetAsync(request);
    }

    ////////////////////////////////////////////////////////////////////////////

    private void Button_ChangeGoCqHttpQQID_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        ProcessUtil.CloseProcess("go-cqhttp");

        if (websocketClient != null)
        {
            websocketClient.Dispose();
            websocketClient = null;
            AppendLog("客户端WebsocketServer链接关闭");
        }

        FileUtil.DelectDir("C:\\ProgramData\\BF1 Server\\Robot");

        FileUtil.ExtractResFile(FileUtil.Resource_Path + "config.yml", FileUtil.D_Robot_Path + "\\config.yml");
        FileUtil.ExtractResFile(FileUtil.Resource_Path + "go-cqhttp.exe", FileUtil.D_Robot_Path + "\\go-cqhttp.exe");

        MsgBoxUtil.Information("操作成功，请重新启动QQ机器人服务");
    }

    private void Button_ClearImageCache_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        FileUtil.DelectDir("C:\\ProgramData\\BF1 Server\\Robot\\data\\images");

        MsgBoxUtil.Information("清理图片缓存成功");
    }

    private void Button_OpenGoCqHttpPath_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        ProcessUtil.OpenLink(FileUtil.D_Robot_Path);
    }

    private void Button_AddQQGMID_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        var qq = TextBox_AddQQGroupMemberID.Text.Trim();

        if (CommonUtil.IsNumber(qq))
        {
            ListBox_QQGroupMemberIDs.Items.Add(qq);
            TextBox_AddQQGroupMemberID.Clear();
        }
    }

    private void Button_RemoveQQGMID_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        var index = ListBox_QQGroupMemberIDs.SelectedIndex;
        if (index != -1)
        {
            ListBox_QQGroupMemberIDs.Items.RemoveAt(index);
        }
    }

    private void Button_ClearQQGMID_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        ListBox_QQGroupMemberIDs.Items.Clear();
    }

    private void Button_SaveRobotConfig_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        SaveRobotConfig();
    }

    /// <summary>
    /// 保存机器人配置文件
    /// </summary>
    private void SaveRobotConfig()
    {
        RobotConfig.QQGroupMemberID.Clear();
        if (ListBox_QQGroupMemberIDs.Items.Count != 0)
        {
            foreach (var item in ListBox_QQGroupMemberIDs.Items)
            {
                RobotConfig.QQGroupMemberID.Add(long.Parse(item.ToString()));
            }
        }

        var qqGroup = TextBox_QQGroupID.Text.Trim();
        RobotConfig.QQGroupID = string.IsNullOrEmpty(qqGroup) ? 0 : long.Parse(qqGroup);

        RobotConfig.IsIgnoreQQGroupLimit = CheckBox_IgnoreQQGroupLimit.IsChecked == true ? true : false;
        RobotConfig.IsIgnoreQQGroupMemberLimit = CheckBox_IgnoreQQGroupMemberLimit.IsChecked == true ? true : false;

        RobotConfig.IsSendChangeTeam = CheckBox_IsSendChangeTeam.IsChecked == true ? true : false;

        File.WriteAllText(FileUtil.F_Robot_Path, JsonUtil.JsonSeri(RobotConfig));
    }
}
