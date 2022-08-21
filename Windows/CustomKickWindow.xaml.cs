using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Features.API;

namespace BF1.ServerAdminTools.Windows;

/// <summary>
/// CustomKickWindow.xaml 的交互逻辑
/// </summary>
public partial class CustomKickWindow : Window
{
    public string PlayerName { get; set; }
    public long PersonaId { get; set; }

    public CustomKickWindow(string playerName, long personaId)
    {
        InitializeComponent();
        this.DataContext = this;

        PlayerName = playerName;
        PersonaId = personaId;
    }

    private async void Button_KickPlayer_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        NotifierHelper.Show(NotiferType.Information, $"正在踢出玩家 {PlayerName} 中...");

        var reason = ChsUtil.ToTraditionalChinese(TextBox_CustomReason.Text.Trim());
        var result = await BF1API.AdminKickPlayer(PersonaId, reason);
        if (result.IsSuccess)
        {
            NotifierHelper.Show(NotiferType.Success, $"踢出玩家 {PlayerName} 成功  |  耗时: {result.ExecTime:0.00} 秒");
            this.Close();
        }
        else
        {
            NotifierHelper.Show(NotiferType.Error, $"踢出玩家 {PlayerName} 失败 {result.Message}  |  耗时: {result.ExecTime:0.00} 秒");
        }
    }
}
