namespace BF1.ServerAdminTools.Features.Data;

public static class MapData
{
    public struct MapName
    {
        public string ID;
        public string Chinese;
        public string English;
    }

    /// <summary>
    /// 地图数据
    /// </summary>
    public readonly static List<MapName> AllMapInfo = new()
    {
        new() { ID="Levels/MP/MP_Amiens/MP_Amiens", Chinese="亚眠", English="Amiens" },
        new() { ID="Levels/MP/MP_ItalianCoast/MP_ItalianCoast", Chinese="帝国边境", English="ItalianCoast" },
        new() { ID="Xpack1-3/Levels/MP_ShovelTown/MP_ShovelTown", Chinese="攻占托尔", English="ShovelTown" },
        new() { ID="Levels/MP/MP_MountainFort/MP_MountainFort", Chinese="格拉巴山", English="MountainFort" },
        new() { ID="Xpack1/Levels/MP_Graveyard/MP_Graveyard", Chinese="决裂", English="Graveyard" },
        new() { ID="Levels/MP/MP_FaoFortress/MP_FaoFortress", Chinese="法欧堡", English="Graveyard" },
        new() { ID="Levels/MP/MP_Chateau/MP_Chateau", Chinese="流血宴厅", English="Chateau" },
        new() { ID="Levels/MP/MP_Scar/MP_Scar", Chinese="圣康坦的伤痕", English="Scar" },
        new() { ID="Levels/MP/MP_Desert/MP_Desert", Chinese="西奈沙漠", English="Desert" },
        new() { ID="Levels/MP/MP_Forest/MP_Forest", Chinese="阿尔贡森林", English="Forest" },
        new() { ID="Xpack0/Levels/MP/MP_Giant/MP_Giant", Chinese="庞然暗影", English="Giant" },
        new() { ID="Xpack1/Levels/MP_Verdun/MP_Verdun", Chinese="凡尔登高地", English="Verdun" },
        new() { ID="Xpack1-3/Levels/MP_Trench/MP_Trench", Chinese="尼维尔之夜", English="Trench" },
        new() { ID="Xpack1/Levels/MP_Underworld/MP_Underworld", Chinese="法乌克斯要塞", English="Underworld" },
        new() { ID="Xpack1/Levels/MP_Fields/MP_Fields", Chinese="苏瓦松", English="Fields" },
        new() { ID="Xpack2/Levels/MP/MP_Valley/MP_Valley", Chinese="加利西亚", English="Valley" },
        new() { ID="Xpack2/Levels/MP/MP_Bridge/MP_Bridge", Chinese="勃鲁西洛夫关口", English="Bridge" },
        new() { ID="Xpack2/Levels/MP/MP_Tsaritsyn/MP_Tsaritsyn", Chinese="察里津", English="Tsaritsyn" },
        new() { ID="Xpack2/Levels/MP/MP_Ravines/MP_Ravines", Chinese="武普库夫山口", English="Ravines" },
        new() { ID="Xpack2/Levels/MP/MP_Volga/MP_Volga", Chinese="窝瓦河", English="Volga" },
        new() { ID="Xpack2/Levels/MP/MP_Islands/MP_Islands", Chinese="阿尔比恩", English="Islands" },
        new() { ID="Xpack3/Levels/MP/MP_Beachhead/MP_Beachhead", Chinese="海丽丝岬", English="qqqqq" },
        new() { ID="XPack3/Levels/MP/MP_Harbor/MP_Harbor", Chinese="泽布吕赫", English="Harbor" },
        new() { ID="Xpack3/Levels/MP/MP_Ridge/MP_Ridge", Chinese="阿奇巴巴", English="Ridge" },
        new() { ID="Xpack3/Levels/MP/MP_Naval/MP_Naval", Chinese="黑尔戈兰湾", English="Naval" },
        new() { ID="XPack4/Levels/MP/MP_River/MP_River", Chinese="卡波雷托", English="River" },
        new() { ID="XPack4/Levels/MP/MP_Hell/MP_Hell", Chinese="帕斯尚尔", English="Hell" },
        new() { ID="XPack4/Levels/MP/MP_Offensive/MP_Offensive", Chinese="索姆河", English="Offensive" },
    };
}
