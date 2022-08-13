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

    private string _serverGameID;
    /// <summary>
    /// 服务器GameID
    /// </summary>
    public string ServerGameID
    {
        get => _serverGameID;
        set => SetProperty(ref _serverGameID, value);
    }

    private string _serverGameMode;
    /// <summary>
    /// 服务器地图游戏模式
    /// </summary>
    public string ServerGameMode
    {
        get => _serverGameMode;
        set => SetProperty(ref _serverGameMode, value);
    }

    private string _serverMapName;
    /// <summary>
    /// 服务器地图名称
    /// </summary>
    public string ServerMapName
    {
        get => _serverMapName;
        set => SetProperty(ref _serverMapName, value);
    }

    private string _serverMapImg;
    /// <summary>
    /// 服务器地图预览图
    /// </summary>
    public string ServerMapImg
    {
        get => _serverMapImg;
        set => SetProperty(ref _serverMapImg, value);
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

    private int _team1Flag;
    /// <summary>
    /// 队伍1从旗帜获取的得分
    /// </summary>
    public int Team1Flag
    {
        get => _team1Flag;
        set => SetProperty(ref _team1Flag, value);
    }

    private int _team1Kill;
    /// <summary>
    /// 队伍1从击杀获取的得分
    /// </summary>
    public int Team1Kill
    {
        get => _team1Kill;
        set => SetProperty(ref _team1Kill, value);
    }

    private string _team1Img;
    /// <summary>
    /// 队伍1图片
    /// </summary>
    public string Team1Img
    {
        get => _team1Img;
        set => SetProperty(ref _team1Img, value);
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

    private int _team2Flag;
    /// <summary>
    /// 队伍2从旗帜获取的得分
    /// </summary>
    public int Team2Flag
    {
        get => _team2Flag;
        set => SetProperty(ref _team2Flag, value);
    }

    private int _team2Kill;
    /// <summary>
    /// 队伍2从击杀获取的得分
    /// </summary>
    public int Team2Kill
    {
        get => _team2Kill;
        set => SetProperty(ref _team2Kill, value);
    }

    private string _team2Img;
    /// <summary>
    /// 队伍2图片
    /// </summary>
    public string Team2Img
    {
        get => _team2Img;
        set => SetProperty(ref _team2Img, value);
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
