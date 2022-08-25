using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models;

public class QueryModel : ObservableObject
{
    private Visibility _loadingVisibility;
    /// <summary>
    /// 加载动画
    /// </summary>
    public Visibility LoadingVisibility
    {
        get => _loadingVisibility;
        set => SetProperty(ref _loadingVisibility, value);
    }

    //////////////////////////////////////

    private string _avatar;
    /// <summary>
    /// 玩家头像
    /// </summary>
    public string Avatar
    {
        get => _avatar;
        set => SetProperty(ref _avatar, value);
    }

    private string _emblem;
    /// <summary>
    /// 玩家图章
    /// </summary>
    public string Emblem
    {
        get => _emblem;
        set => SetProperty(ref _emblem, value);
    }

    private string _playerName;
    /// <summary>
    /// 玩家名称
    /// </summary>
    public string PlayerName
    {
        get => _playerName;
        set => SetProperty(ref _playerName, value);
    }

    private string _personaId;
    /// <summary>
    /// 玩家数字ID
    /// </summary>
    public string PersonaId
    {
        get => _personaId;
        set => SetProperty(ref _personaId, value);
    }

    private string _rank;
    /// <summary>
    /// 玩家等级
    /// </summary>
    public string Rank
    {
        get => _rank;
        set => SetProperty(ref _rank, value);
    }

    private string _playTime;
    /// <summary>
    /// 玩家游玩时间
    /// </summary>
    public string PlayTime
    {
        get => _playTime;
        set => SetProperty(ref _playTime, value);
    }

    //////////////////////////////////////

    private string _playingServer;
    /// <summary>
    /// 玩家正在游玩服务器
    /// </summary>
    public string PlayingServer
    {
        get => _playingServer;
        set => SetProperty(ref _playingServer, value);
    }
}
