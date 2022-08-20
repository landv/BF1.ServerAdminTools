namespace BF1.ServerAdminTools.Features.Data;

public static class Globals
{
    public static string Remid = string.Empty;
    public static string Sid = string.Empty;

    public static bool IsUseMode1 = true;

    public static string SessionId_Mode1 = string.Empty;
    public static string SessionId_Mode2 = string.Empty;

    public static string SessionId
    {
        get
        {
            return IsUseMode1 ? SessionId_Mode1 : SessionId_Mode2;
        }
    }

    public static string GameId = string.Empty;
    public static string ServerId = string.Empty;
    public static string PersistedGameId = string.Empty;

    public static bool IsRuleSetRight = false;

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 保存队伍1限制武器名称列表
    /// </summary>
    public static List<string> Custom_WeaponList_Team1 = new();
    /// <summary>
    /// 保存队伍2限制武器名称列表
    /// </summary>
    public static List<string> Custom_WeaponList_Team2 = new();

    /// <summary>
    /// 自定义黑名单玩家列表
    /// </summary>
    public static List<string> Custom_BlackList = new();
    /// <summary>
    /// 自定义白名单玩家列表
    /// </summary>
    public static List<string> Custom_WhiteList = new();

    /// <summary>
    /// 服务器管理员，PID
    /// </summary>
    public static List<string> Server_AdminList_PID = new();
    /// <summary>
    /// 服务器管理员，名称
    /// </summary>
    public static List<string> Server_AdminList_Name = new();
    /// <summary>
    /// 服务器VIP
    /// </summary>
    public static List<string> Server_VIPList = new();

    /// <summary>
    /// 保存违规玩家列表信息
    /// </summary>
    public static List<BreakRuleInfo> BreakRuleInfo_PlayerList = new();

    /// <summary>
    /// 观战玩家列表
    /// </summary>
    public static List<SpectatorInfo> Server_SpectatorList = new();

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 是否自动踢出违规玩家
    /// </summary>
    public static bool AutoKickBreakPlayer = false;

    /// <summary>
    /// 是否显示中文武器名称
    /// </summary>
    public static bool IsShowCHSWeaponName = true;
}
