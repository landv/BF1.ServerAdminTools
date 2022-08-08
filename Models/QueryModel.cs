using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models;

public class QueryModel : ObservableObject
{
    private string _playerName;
    /// <summary>
    /// 玩家名称
    /// </summary>
    public string PlayerName
    {
        get => _playerName;
        set => SetProperty(ref _playerName, value);
    }

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

    private string _userName;
    /// <summary>
    /// 玩家名称
    /// </summary>
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
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

    private string _rankImg;
    /// <summary>
    /// 玩家等级图片
    /// </summary>
    public string RankImg
    {
        get => _rankImg;
        set => SetProperty(ref _rankImg, value);
    }

    private string _playerTime;
    /// <summary>
    /// 玩家游玩时间
    /// </summary>
    public string PlayerTime
    {
        get => _playerTime;
        set => SetProperty(ref _playerTime, value);
    }
}
