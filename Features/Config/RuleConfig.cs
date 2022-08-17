namespace BF1.ServerAdminTools.Features.Config;

public class RuleConfig
{
    public string RuleName;
    public RuleInfo RuleInfos;
    public class RuleInfo
    {
        public Normal Team1Normal;
        public Normal Team2Normal;
        public List<string> Team1Weapon;
        public List<string> Team2Weapon;
        public List<string> BlackList;
        public List<string> WhiteList;
        public class Normal
        {
            public int MaxKill;
            public int KDFlag;
            public float MaxKD;
            public int KPMFlag;
            public float MaxKPM;
            public int MinRank;
            public int MaxRank;
            public float LifeMaxKD;
            public float LifeMaxKPM;
            public int LifeMaxWeaponStar;
            public int LifeMaxVehicleStar;
        }
    }
}
