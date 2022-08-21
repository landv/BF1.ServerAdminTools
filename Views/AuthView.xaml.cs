using BF1.ServerAdminTools.Windows;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.Core;
using BF1.ServerAdminTools.Features.Data;
using BF1.ServerAdminTools.Features.API;
using BF1.ServerAdminTools.Features.API.RespJson;
using BF1.ServerAdminTools.Features.Config;

using RestSharp;
using CommunityToolkit.Mvvm.Messaging;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// AuthView.xaml 的交互逻辑
/// </summary>
public partial class AuthView : UserControl
{
    private AuthConfig AuthConfig { get; set; } = new();

    public ObservableCollection<string> ComboBox_ConfigNames { get; set; } = new();

    private WebView2Window WebView2Window = null;

    public AuthView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.ClosingDisposeEvent += MainWindow_ClosingDisposeEvent;

        var timerAutoRefreshMode1 = new Timer
        {
            AutoReset = true,
            Interval = TimeSpan.FromMinutes(5).TotalMilliseconds
        };
        timerAutoRefreshMode1.Elapsed += TimerAutoRefreshMode1_Elapsed;
        timerAutoRefreshMode1.Start();

        var timerAutoRefreshMode2 = new Timer
        {
            AutoReset = true,
            Interval = TimeSpan.FromMinutes(30).TotalMilliseconds
        };
        timerAutoRefreshMode2.Elapsed += TimerAutoRefreshMode2_Elapsed;
        timerAutoRefreshMode2.Start();

        Task.Run(() =>
        {
            TimerAutoRefreshMode1_Elapsed(null, null);
            TimerAutoRefreshMode2_Elapsed(null, null);
        });

        WeakReferenceMessenger.Default.Register<string, string>(this, "SendRemidSid", (s, e) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                TextBox_Remid.Text = Globals.Remid;
                TextBox_Sid.Text = Globals.Sid;
                TextBox_SessionId.Text = Globals.SessionId;

                SaveAuthConfig();
            });
        });

        if (!File.Exists(FileUtil.F_Auth_Path))
        {
            AuthConfig.IsUseMode1 = true;
            AuthConfig.AuthInfos = new();

            for (int i = 0; i < 10; i++)
            {
                AuthConfig.AuthInfos.Add(new AuthConfig.AuthInfo()
                {
                    AuthName = $"自定义授权 {i}",
                    Sid = "",
                    Remid = "",
                    SessionId = ""
                });
            }

            File.WriteAllText(FileUtil.F_Auth_Path, JsonUtil.JsonSeri(AuthConfig));
        }

        if (File.Exists(FileUtil.F_Auth_Path))
        {
            using (var streamReader = new StreamReader(FileUtil.F_Auth_Path))
            {
                AuthConfig = JsonUtil.JsonDese<AuthConfig>(streamReader.ReadToEnd());

                Globals.IsUseMode1 = AuthConfig.IsUseMode1;
                if (AuthConfig.IsUseMode1)
                {
                    RadioButton_Mode1.IsChecked = true;
                    RadioButton_Mode2.IsChecked = false;
                }
                else
                {
                    RadioButton_Mode1.IsChecked = false;
                    RadioButton_Mode2.IsChecked = true;
                }

                foreach (var item in AuthConfig.AuthInfos)
                {
                    ComboBox_ConfigNames.Add(item.AuthName);
                }

                ApplyAuthByIndex(0);
            }
        }
    }

    private void MainWindow_ClosingDisposeEvent()
    {
        WebView2Window?.Close();

        AuthConfig.IsUseMode1 = Globals.IsUseMode1;
        File.WriteAllText(FileUtil.F_Auth_Path, JsonUtil.JsonSeri(AuthConfig));
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessUtil.OpenLink(e.Uri.OriginalString);
        e.Handled = true;
    }

    private void TimerAutoRefreshMode1_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (Globals.IsUseMode1)
        {
            var str = Search.SearchMemory(Offsets.SessionIDMask);
            if (str != string.Empty)
            {
                Globals.SessionId_Mode1 = str;
                LoggerHelper.Info($"获取SessionID成功 {Globals.SessionId}");
            }
            else
            {
                LoggerHelper.Error($"获取SessionID失败");
            }
        }
    }

    private async void TimerAutoRefreshMode2_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (!Globals.IsUseMode1)
        {
            try
            {
                if (!string.IsNullOrEmpty(Globals.Remid) && !string.IsNullOrEmpty(Globals.Sid))
                {
                    var str = "https://accounts.ea.com/connect/auth?response_type=code&locale=zh_CN&client_id=sparta-backend-as-user-pc";
                    var options = new RestClientOptions(str)
                    {
                        MaxTimeout = 5000,
                        FollowRedirects = false
                    };

                    var client = new RestClient(options);
                    var request = new RestRequest()
                        .AddHeader("Cookie", $"remid={Globals.Remid};sid={Globals.Sid};");

                    LoggerHelper.Info($"当前Remin为 {Globals.Remid}");
                    LoggerHelper.Info($"当前Sid为 {Globals.Sid}");

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

                            code = code.Replace("http://127.0.0.1/success?code=", "");
                            var result = await BF1API.GetEnvIdViaAuthCode(code);
                            if (result.IsSuccess)
                            {
                                var envIdViaAuthCode = JsonUtil.JsonDese<EnvIdViaAuthCode>(result.Message);
                                Globals.SessionId_Mode2 = envIdViaAuthCode.result.sessionId;
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
                    LoggerHelper.Error($"刷新SessionID失败，玩家Remid或Sid为空");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"刷新SessionID失败", ex);
            }
        }
    }

    /// <summary>
    /// 应用授权
    /// </summary>
    /// <param name="index"></param>
    private void ApplyAuthByIndex(int index)
    {
        var auth = AuthConfig.AuthInfos[index];

        Globals.Sid = auth.Sid;
        Globals.Remid = auth.Remid;
        Globals.SessionId_Mode2 = auth.SessionId;

        TextBox_Remid.Text = Globals.Remid;
        TextBox_Sid.Text = Globals.Sid;
        TextBox_SessionId.Text = Globals.SessionId_Mode2;
    }

    /// <summary>
    /// 保存授权
    /// </summary>
    private void SaveAuthConfig()
    {
        var index = ComboBox_CustomConfigName.SelectedIndex;
        if (index == -1)
            return;

        var auth = AuthConfig.AuthInfos[index];

        auth.Sid = Globals.Sid;
        auth.Remid = Globals.Remid;
        auth.SessionId = Globals.SessionId;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////

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

    private void Button_ReadPlayerSessionId_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        NotifierHelper.Show(NotiferType.Information, "正在获取中，请等待...");

        Task.Run(() =>
        {
            return Search.SearchMemory(Offsets.SessionIDMask);
        }).ContinueWith((str) =>
        {
            if (str.Result != string.Empty)
            {
                Globals.SessionId_Mode1 = str.Result;
                NotifierHelper.Show(NotiferType.Success, $"获取玩家SessionID成功 {Globals.SessionId}");
            }
            else
            {
                LoggerHelper.Error($"获取玩家SessionID失败");
                NotifierHelper.Show(NotiferType.Error, $"获取玩家SessionID失败，请尝试刷新伺服器列表");
            }
        });
    }

    private void Button_ReNameAuth_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        var name = TextBox_ReNameAuth.Text.Trim();
        if (string.IsNullOrEmpty(name))
            return;

        var index = ComboBox_CustomConfigName.SelectedIndex;
        if (index == -1)
            return;

        ComboBox_ConfigNames[index] = name;
        AuthConfig.AuthInfos[index].AuthName = name;

        ComboBox_CustomConfigName.SelectedIndex = index;
    }

    private void Button_SaveCurrentAuth_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        SaveAuthConfig();
    }

    private void ComboBox_CustomConfigName_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var index = ComboBox_CustomConfigName.SelectedIndex;
        if (index == -1)
            return;

        ApplyAuthByIndex(index);
    }

    private void GetPlayerRemidSid()
    {
        if (CoreUtil.IsWebView2DependencyInstalled())
        {
            if (WebView2Window == null)
            {
                WebView2Window = new WebView2Window();
                WebView2Window.Show();
            }
            else
            {
                if (WebView2Window.IsVisible)
                {
                    WebView2Window.Topmost = true;
                    WebView2Window.Topmost = false;
                    WebView2Window.WindowState = WindowState.Normal;
                }
                else
                {
                    WebView2Window = null;
                    WebView2Window = new WebView2Window();
                    WebView2Window.Show();
                }
            }
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "未安装WebView2对应依赖，请安装依赖或手动获取Cookie");
        }
    }

    private void Button_GetPlayerRemidSid_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        GetPlayerRemidSid();
    }

    private void RadioButton_Mode12_Click(object sender, RoutedEventArgs e)
    {
        Globals.IsUseMode1 = RadioButton_Mode1.IsChecked == true;
    }
}
