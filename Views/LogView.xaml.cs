using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.Data;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// LogView.xaml 的交互逻辑
/// </summary>
public partial class LogView : UserControl
{
    public static Action<BreakRuleInfo> _dAddKickOKLog;
    public static Action<BreakRuleInfo> _dAddKickNOLog;

    public static Action<ChangeTeamInfo> _dAddChangeTeamInfo;

    public LogView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.ClosingDisposeEvent += MainWindow_ClosingDisposeEvent;

        _dAddKickOKLog = AddKickOKLog;
        _dAddKickNOLog = AddKickNOLog;

        _dAddChangeTeamInfo = AddChangeTeamLog;
    }

    private void MainWindow_ClosingDisposeEvent()
    {

    }

    /////////////////////////////////////////////////////

    private void AppendKickOKLog(string msg)
    {
        TextBox_KickOKLog.AppendText(msg + "\n");
    }

    private void AppendKickNOLog(string msg)
    {
        TextBox_KickNOLog.AppendText(msg + "\n");
    }

    private void AppendChangeTeamLog(string msg)
    {
        TextBox_ChangeTeamLog.AppendText(msg + "\n");
    }

    /////////////////////////////////////////////////////

    /// <summary>
    /// 追加踢人成功日志
    /// </summary>
    /// <param name="info"></param>
    private void AddKickOKLog(BreakRuleInfo info)
    {
        this.Dispatcher.Invoke(() =>
        {
            if (TextBox_KickOKLog.LineCount >= 1000)
                TextBox_KickOKLog.Clear();

            AppendKickOKLog($"操作时间: {DateTime.Now}");
            AppendKickOKLog($"玩家ID: {info.Name}");
            AppendKickOKLog($"玩家数字ID: {info.PersonaId}");
            AppendKickOKLog($"踢出理由: {info.Reason}");
            AppendKickOKLog($"状态: {info.Status}\n");

            SQLiteHelper.AddLog2SQLite("kick_ok", info);
        });
    }


    /// <summary>
    /// 追加踢人失败日志
    /// </summary>
    /// <param name="info"></param>
    private void AddKickNOLog(BreakRuleInfo info)
    {
        this.Dispatcher.Invoke(() =>
        {
            if (TextBox_KickNOLog.LineCount >= 1000)
                TextBox_KickNOLog.Clear();

            AppendKickNOLog($"操作时间: {DateTime.Now}");
            AppendKickNOLog($"玩家ID: {info.Name}");
            AppendKickNOLog($"玩家数字ID: {info.PersonaId}");
            AppendKickNOLog($"踢出理由: {info.Reason}");
            AppendKickNOLog($"状态: {info.Status}\n");

            SQLiteHelper.AddLog2SQLite("kick_no", info);
        });
    }

    /// <summary>
    /// 追加更换队伍日志
    /// </summary>
    /// <param name="info"></param>
    private void AddChangeTeamLog(ChangeTeamInfo info)
    {
        this.Dispatcher.Invoke(() =>
        {
            if (TextBox_ChangeTeamLog.LineCount >= 1000)
                TextBox_ChangeTeamLog.Clear();

            AppendChangeTeamLog($"操作时间: {DateTime.Now}");
            AppendChangeTeamLog($"玩家等级: {info.Rank}");
            AppendChangeTeamLog($"玩家ID: {info.Name}");
            AppendChangeTeamLog($"玩家数字ID: {info.PersonaId}");
            AppendChangeTeamLog($"队伍比分: {info.Team1Score} - {info.Team2Score}");
            AppendChangeTeamLog($"状态: {info.Status}\n");

            SQLiteHelper.AddLog2SQLite(info);
        });

        RobotView._dSendChangeTeamInfo(info);
    }

    private void MenuItem_ClearKickOKLog_Click(object sender, RoutedEventArgs e)
    {
        TextBox_KickOKLog.Clear();
        NotifierHelper.Show(NotiferType.Success, "清空踢人成功日志成功");
    }

    private void MenuItem_ClearKickNOLog_Click(object sender, RoutedEventArgs e)
    {
        TextBox_KickNOLog.Clear();
        NotifierHelper.Show(NotiferType.Success, "清空踢人失败日志成功");
    }

    private void MenuItem_ClearChangeTeamLog_Click(object sender, RoutedEventArgs e)
    {
        TextBox_ChangeTeamLog.Clear();
        NotifierHelper.Show(NotiferType.Success, "清空更换队伍日志成功");
    }
}
