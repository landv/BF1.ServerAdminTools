namespace BF1.ServerAdminTools.Features.Data;

public static class MapData
{
    public struct MapName
    {
        public string English;
        public string Chinese;
    }

    /// <summary>
    /// 地图数据
    /// </summary>
    public readonly static List<MapName> AllMapInfo = new()
    {
        new() { English="ID_M_LEVEL_MENU", Chinese="菜单" },
        new() { English="ID_M_MP_LEVEL_MOUNTAINFORT", Chinese="格拉巴山" },
        new() { English="ID_M_MP_LEVEL_FOREST", Chinese="阿尔贡森林" },
        new() { English="ID_M_MP_LEVEL_ITALIANCOAST", Chinese="帝国边境" },
        new() { English="ID_M_MP_LEVEL_CHATEAU", Chinese="流血宴厅" },
        new() { English="ID_M_MP_LEVEL_SCAR", Chinese="圣康坦的伤痕" },
        new() { English="ID_M_MP_LEVEL_DESERT", Chinese="西奈沙漠" },
        new() { English="ID_M_MP_LEVEL_AMIENS", Chinese="亚眠" },
        new() { English="ID_M_MP_LEVEL_SUEZ", Chinese="苏伊士" },
        new() { English="ID_M_MP_LEVEL_FAOFORTRESS", Chinese="法欧堡" },
        new() { English="ID_M_MP_LEVEL_GIANT", Chinese="庞然暗影" },
        new() { English="ID_M_MP_LEVEL_FIELDS", Chinese="苏瓦松" },
        new() { English="ID_M_MP_LEVEL_GRAVEYARD", Chinese="决裂" },
        new() { English="ID_M_MP_LEVEL_UNDERWORLD", Chinese="法乌克斯要塞" },
        new() { English="ID_M_MP_LEVEL_VERDUN", Chinese="凡尔登高地" },
        new() { English="ID_M_MP_LEVEL_TRENCH", Chinese="尼维尔之夜" },
        new() { English="ID_M_MP_LEVEL_SHOVELTOWN", Chinese="攻占托尔" },
        new() { English="ID_M_MP_LEVEL_BRIDGE", Chinese="勃鲁西洛夫关口" },
        new() { English="ID_M_MP_LEVEL_ISLANDS", Chinese="阿尔比恩" },
        new() { English="ID_M_MP_LEVEL_RAVINES", Chinese="武普库夫山口" },
        new() { English="ID_M_MP_LEVEL_VALLEY", Chinese="加利西亚" },
        new() { English="ID_M_MP_LEVEL_TSARITSYN", Chinese="察里津" },
        new() { English="ID_M_MP_LEVEL_VOLGA", Chinese="窝瓦河" },
        new() { English="ID_M_MP_LEVEL_BEACHHEAD", Chinese="海丽丝岬" },
        new() { English="ID_M_MP_LEVEL_HARBOR", Chinese="泽布吕赫" },
        new() { English="ID_M_MP_LEVEL_NAVAL", Chinese="黑尔戈兰湾" },
        new() { English="ID_M_MP_LEVEL_RIDGE", Chinese="阿奇巴巴" },
        new() { English="ID_M_MP_LEVEL_OFFENSIVE", Chinese="索姆河" },
        new() { English="ID_M_MP_LEVEL_HELL", Chinese="帕斯尚尔" },
        new() { English="ID_M_MP_LEVEL_RIVER", Chinese="卡波雷托" },
        new() { English="ID_M_MP_LEVEL_ALPS", Chinese="剃刀边缘" },
        new() { English="ID_M_MP_LEVEL_BLITZ", Chinese="伦敦的呼唤：夜袭" },
        new() { English="ID_M_MP_LEVEL_LONDON", Chinese="伦敦的呼唤：灾祸" },

    };
}
