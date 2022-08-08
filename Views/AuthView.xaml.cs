using BF1.ServerAdminTools.Common.Data;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.API;
using BF1.ServerAdminTools.Features.API.RespJson;

using RestSharp;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// AuthView.xaml 的交互逻辑
/// </summary>
public partial class AuthView : UserControl
{
    public static Action _AutoRefreshSID;

    public AuthView()
    {
        InitializeComponent();

        MainWindow.ClosingDisposeEvent += MainWindow_ClosingDisposeEvent;

        var timerAutoRefresh = new Timer
        {
            AutoReset = true,
            Interval = 43200000
        };
        timerAutoRefresh.Elapsed += TimerAutoRefresh_Elapsed;
        timerAutoRefresh.Start();

        _AutoRefreshSID = AutoRefresh;
    }

    private void MainWindow_ClosingDisposeEvent()
    {

    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessUtil.OpenLink(e.Uri.OriginalString);
        e.Handled = true;
    }

    private void AutoRefresh()
    {
        LoggerHelper.Info($"调用刷新SessionID功能成功");
        TimerAutoRefresh_Elapsed(null, null);
    }

    private async void TimerAutoRefresh_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(Globals.Remid))
            {
                var str = "https://accounts.ea.com/connect/auth?response_type=code&locale=zh_CN&client_id=sparta-backend-as-user-pc";
                var options = new RestClientOptions(str)
                {
                    MaxTimeout = 5000,
                    FollowRedirects = false
                };

                var client = new RestClient(options);
                var request = new RestRequest()
                    .AddHeader("Cookie", $"remid={Globals.Remid}");

                LoggerHelper.Info($"当前Remin为 {Globals.Remid}");

                var response = await client.ExecuteGetAsync(request);
                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    string code = response.Headers.ToList()
                        .Find(x => x.Name == "Location")
                        .Value.ToString();

                    LoggerHelper.Info($"当前Location为 {code}");

                    if (code.Contains("http://127.0.0.1/success?code="))
                    {
                        Globals.Remid = response.Cookies[0].Value;
                        Globals.Sid = response.Cookies[1].Value;

                        LoggerHelper.Info($"当前Remid为 {Globals.Remid}");
                        LoggerHelper.Info($"当前Sid为 {Globals.Sid}");

                        IniHelper.WriteString("Globals", "Remid", Globals.Remid, FileUtil.F_Settings_Path);
                        IniHelper.WriteString("Globals", "Sid", Globals.Sid, FileUtil.F_Settings_Path);

                        code = code.Replace("http://127.0.0.1/success?code=", "");
                        var result = await BF1API.GetEnvIdViaAuthCode(code);

                        if (result.IsSuccess)
                        {
                            var envIdViaAuthCode = JsonUtil.JsonDese<EnvIdViaAuthCode>(result.Message);
                            Globals.SessionId = envIdViaAuthCode.result.sessionId;
                            LoggerHelper.Info($"刷新SessionID成功 {Globals.SessionId}");
                        }
                        else
                        {
                            LoggerHelper.Error($"刷新SessionID失败，code无效 {code}");
                        }
                    }
                    else
                    {
                        LoggerHelper.Error($"刷新SessionID失败，code错误 {code}");
                    }
                }
                else
                {
                    LoggerHelper.Error($"刷新SessionID失败，玩家Remid不正确 {Globals.Remid}");
                }
            }
            else
            {
                LoggerHelper.Error($"刷新SessionID失败，玩家Remid为空");
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.Error($"刷新SessionID失败", ex);
        }
    }

    private async void Button_VerifyPlayerSessionId_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        if (!string.IsNullOrEmpty(Globals.SessionId))
        {
            TextBlock_CheckSessionIdStatus.Text = "正在验证中，请等待...";
            TextBlock_CheckSessionIdStatus.Background = Brushes.Gray;
            MainWindow._SetOperatingState(2, "正在验证中，请等待...");

            await BF1API.SetAPILocale();
            var result = await BF1API.GetWelcomeMessage();

            if (result.IsSuccess)
            {
                var welcomeMsg = JsonUtil.JsonDese<WelcomeMsg>(result.Message);

                var msg = ChsUtil.ToSimplifiedChinese(welcomeMsg.result.firstMessage);

                TextBlock_CheckSessionIdStatus.Text = msg;
                TextBlock_CheckSessionIdStatus.Background = Brushes.Green;

                MainWindow._SetOperatingState(1, $"验证成功 {msg}  |  耗时: {result.ExecTime:0.00} 秒");
            }
            else
            {
                TextBlock_CheckSessionIdStatus.Text = "验证失败";
                TextBlock_CheckSessionIdStatus.Background = Brushes.Red;

                MainWindow._SetOperatingState(3, $"验证失败 {result.Message}  |  耗时: {result.ExecTime:0.00} 秒");
            }
        }
        else
        {
            MainWindow._SetOperatingState(2, "请先获取玩家SessionID后，再执行本操作");
        }
    }

}
