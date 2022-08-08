using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models;

public class ServerInfoModel : ObservableObject
{
    private string _serverName;
    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName
    {
        get => _serverName;
        set => SetProperty(ref _serverName, value);
    }

    private string _serverTime;
    /// <summary>
    /// 服务器时间
    /// </summary>
    public string ServerTime
    {
        get => _serverTime;
        set => SetProperty(ref _serverTime, value);
    }

    ///////////////////////////////////////////////////////////////////////

    private string _team1Score;
    /// <summary>
    /// 队伍1比分
    /// </summary>
    public string Team1Score
    {
        get => _team1Score;
        set => SetProperty(ref _team1Score, value);
    }

    private double _team1ScoreWidth;
    /// <summary>
    /// 队伍1比分，图形宽度
    /// </summary>
    public double Team1ScoreWidth
    {
        get => _team1ScoreWidth;
        set => SetProperty(ref _team1ScoreWidth, value);
    }

    private string _team1ScoreFlag;
    /// <summary>
    /// 队伍1从旗帜获取的得分
    /// </summary>
    public string Team1FromeFlag
    {
        get => _team1ScoreFlag;
        set => SetProperty(ref _team1ScoreFlag, value);
    }

    private string _team1ScoreKill;
    /// <summary>
    /// 队伍1从击杀获取的得分
    /// </summary>
    public string Team1FromeKill
    {
        get => _team1ScoreKill;
        set => SetProperty(ref _team1ScoreKill, value);
    }

    private string _team1Info;
    /// <summary>
    /// 队伍1信息
    /// </summary>
    public string Team1Info
    {
        get => _team1Info;
        set => SetProperty(ref _team1Info, value);
    }

    ///////////////////////////////////////////////////////////////////////

    private string _team2Score;
    /// <summary>
    /// 队伍2比分
    /// </summary>
    public string Team2Score
    {
        get => _team2Score;
        set => SetProperty(ref _team2Score, value);
    }

    private double _team2ScoreWidth;
    /// <summary>
    /// 队伍2比分，图形宽度
    /// </summary>
    public double Team2ScoreWidth
    {
        get => _team2ScoreWidth;
        set => SetProperty(ref _team2ScoreWidth, value);
    }

    private string _team2ScoreFlag;
    /// <summary>
    /// 队伍2从旗帜获取的得分
    /// </summary>
    public string Team2FromeFlag
    {
        get => _team2ScoreFlag;
        set => SetProperty(ref _team2ScoreFlag, value);
    }

    private string _team2ScoreKill;
    /// <summary>
    /// 队伍2从击杀获取的得分
    /// </summary>
    public string Team2FromeKill
    {
        get => _team2ScoreKill;
        set => SetProperty(ref _team2ScoreKill, value);
    }

    private string _team2Info;
    /// <summary>
    /// 队伍2信息
    /// </summary>
    public string Team2Info
    {
        get => _team2Info;
        set => SetProperty(ref _team2Info, value);
    }

    ///////////////////////////////////////////////////////////////////////
}
