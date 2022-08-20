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
    private List<AuthConfig> AuthConfigs { get; set; } = new();

    public ObservableCollection<string> ComboBox_ConfigNames { get; set; } = new();

    private WebView2Window WebView2Window = null;

    private bool _isUseMode1 = true;
    private string _sessionIdMode1 = string.Empty;
    private string _sessionIdMode2 = string.Empty;

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
            });
        });

        if (!File.Exists(FileUtil.F_Auth_Path))
        {
            for (int i = 0; i < 10; i++)
            {
                AuthConfigs.Add(new AuthConfig()
                {
                    AuthName = $"自定义授权 {i}",
                    Sid = "",
                    Remid = "",
                    SessionId = ""
                });
            }

            File.WriteAllText(FileUtil.F_Auth_Path, JsonUtil.JsonSeri(AuthConfigs));
        }

        if (File.Exists(FileUtil.F_Auth_Path))
        {
            using (var streamReader = new StreamReader(FileUtil.F_Auth_Path))
            {
                AuthConfigs = JsonUtil.JsonDese<List<AuthConfig>>(streamReader.ReadToEnd());

                foreach (var item in AuthConfigs)
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

        File.WriteAllText(FileUtil.F_Auth_Path, JsonUtil.JsonSeri(AuthConfigs));
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessUtil.OpenLink(e.Uri.OriginalString);
        e.Handled = true;
    }

    private void TimerAutoRefreshMode1_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (_isUseMode1)
        {
            var str = Search.SearchMemory(Offsets.SessionIDMask);
            if (str != string.Empty)
            {
                Globals.SessionId = str;
                _sessionIdMode1 = Globals.SessionId;
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
        if (!_isUseMode1)
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
                                Globals.SessionId = envIdViaAuthCode.result.sessionId;
                                _sessionIdMode2 = Globals.SessionId;
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
        var auth = AuthConfigs[index];

        Globals.Sid = auth.Sid;
        Globals.Remid = auth.Remid;
        Globals.SessionId = auth.SessionId;

        _sessionIdMode1 = Globals.SessionId;

        TextBox_Remid.Text = Globals.Remid;
        TextBox_Sid.Text = Globals.Sid;
        TextBox_SessionId.Text = Globals.SessionId;
    }

    /// <summary>
    /// 保存授权
    /// </summary>
    /// <param name="index"></param>
    private void SaveAuthByIndex(int index)
    {
        var auth = AuthConfigs[index];

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
                Globals.SessionId = str.Result;
                _sessionIdMode1 = Globals.SessionId;
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
        AuthConfigs[index].AuthName = name;

        ComboBox_CustomConfigName.SelectedIndex = index;
    }

    private void Button_SaveCurrentAuth_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        var index = ComboBox_CustomConfigName.SelectedIndex;
        if (index == -1)
            return;

        SaveAuthByIndex(index);
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

    private async void Button_GetPlayerSessionID_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        if (string.IsNullOrEmpty(TextBox_Remid.Text) || string.IsNullOrEmpty(TextBox_Sid.Text))
        {
            NotifierHelper.Show(NotiferType.Warning, $"Remid或Sid为空，请先获取获取玩家Remid和Sid后再执行本操作");
            return;
        }

        NotifierHelper.Show(NotiferType.Information, "正在获取AuthCode");

        string url = "https://accounts.ea.com/connect/auth?response_type=code&locale=zh_CN&client_id=sparta-backend-as-user-pc";
        var options = new RestClientOptions(url)
        {
            MaxTimeout = 5000,
            FollowRedirects = false
        };

        var client = new RestClient(options);
        var request = new RestRequest()
            .AddHeader("Cookie", $"remid={Globals.Remid};{Globals.Sid};");

        var response = await client.ExecuteGetAsync(request);
        if (response.StatusCode != HttpStatusCode.Redirect)
        {
            NotifierHelper.Show(NotiferType.Error, $"EA连接失败 {response.StatusCode}");
            return;
        }

        if (response.Headers == null)
        {
            NotifierHelper.Show(NotiferType.Error, $"EA连接失败: Headers null");
            return;
        }

        var list = response.Headers.Where(a => a.Name == "Location").Select(a => a.Value?.ToString());
        if (!list.Any())
        {
            NotifierHelper.Show(NotiferType.Error, $"EA连接失败: Location null");
            return;
        }

        string location = list.First();
        if (location == null)
        {
            NotifierHelper.Show(NotiferType.Error, $"EA连接失败:Location null");
            return;
        }

        if (location.Contains("http://127.0.0.1/success?code="))
        {
            string code = location.Replace("http://127.0.0.1/success?code=", "");
            NotifierHelper.Show(NotiferType.Information, $"正在获取SessionId，Code为: {code}");

            if (response.Cookies["remid"] != null)
            {
                Globals.Remid = response.Cookies["remid"].Value;
            }
            if (response.Cookies["sid"] != null)
            {
                Globals.Sid = response.Cookies["sid"].Value;
            }

            var result = await BF1API.GetEnvIdViaAuthCode(code);
            if (result.IsSuccess)
            {
                var envIdViaAuthCode = JsonUtil.JsonDese<EnvIdViaAuthCode>(result.Message);
                Globals.SessionId = envIdViaAuthCode.result.sessionId;
                _sessionIdMode2 = Globals.SessionId;
                NotifierHelper.Show(NotiferType.Success, $"获取SessionID成功  |  耗时: {result.ExecTime:0.00} 秒");

                TextBox_Remid.Text = Globals.Remid;
                TextBox_Sid.Text = Globals.Sid;
                TextBox_SessionId.Text = Globals.SessionId;
            }
            else
            {
                NotifierHelper.Show(NotiferType.Error, $"获取SessionID失败 {result.Message}  |  耗时: {result.ExecTime:0.00} 秒");
            }
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, $"Cookie已失效，正在启动网页登录程序");
            GetPlayerRemidSid();
        }
    }

    private void RadioButton_Mode12_Click(object sender, RoutedEventArgs e)
    {
        if (RadioButton_Mode1.IsChecked == true)
        {
            _isUseMode1 = true;
            Globals.SessionId = _sessionIdMode1;
        }
        else
        {
            _isUseMode1 = false;
            Globals.SessionId = _sessionIdMode2;
        }
    }
}
