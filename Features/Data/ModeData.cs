namespace BF1.ServerAdminTools.Features.Data;

public static class ModeData
{
    public struct ModeName
    {
        public string English;
        public string Chinese;
    }
        
    /// <summary>
    /// 地图模式数据
    /// </summary>
    public readonly static List<ModeName> AllModeInfo = new()
    {
        new() { English="ID_M_GAMEMODE_ZONECONTROL", Chinese="空降补给" },
        new() { English="ID_M_GAMEMODE_AIRASSAULT", Chinese="空中突袭" },
        new() { English="ID_M_GAMEMODE_TUGOFWAR", Chinese="前线" },
        new() { English="ID_M_GAMEMODE_DOMINATION", Chinese="抢攻" },
        new() { English="ID_M_GAMEMODE_BREAKTHROUGH", Chinese="闪击行动" },
        new() { English="ID_M_GAMEMODE_RUSH", Chinese="突袭" },
        new() { English="ID_M_GAMEMODE_TEAMDEATHMATCH", Chinese="团队死斗" },
        new() { English="ID_M_GAMEMODE_BREAKTHROUGHLARGE", Chinese="行动模式" },
        new() { English="ID_M_GAMEMODE_POSSESSION", Chinese="战争信鸽" },
        new() { English="ID_M_GAMEMODE_CONQUEST", Chinese="征服" },
    };
}
