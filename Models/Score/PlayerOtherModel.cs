using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models.Score;

public class PlayerOtherModel : ObservableObject
{
    private string _mySelfName;
    /// <summary>
    /// 玩家自己ID
    /// </summary>
    public string MySelfName
    {
        get => _mySelfName;
        set => SetProperty(ref _mySelfName, value);
    }

    private string _mySelfTeamID;
    /// <summary>
    /// 玩家自己队伍ID
    /// </summary>
    public string MySelfTeamID
    {
        get => _mySelfTeamID;
        set => SetProperty(ref _mySelfTeamID, value);
    }

    private string _serverPlayerCountInfo;
    /// <summary>
    /// 服务器人数信息
    /// </summary>
    public string ServerPlayerCountInfo
    {
        get => _serverPlayerCountInfo;
        set => SetProperty(ref _serverPlayerCountInfo, value);
    }
}
