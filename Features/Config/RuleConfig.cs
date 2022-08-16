namespace BF1.ServerAdminTools.Features.Config;

public class RuleConfig
{
    public string RuleName;
    public RuleInfo RuleInfos;
    public class RuleInfo
    {
        public List<Normal> Team1Normal;
        public List<Normal> Team2Normal;
        public List<Weapon> Team1Weapon;
        public List<Weapon> Team2Weapon;
        public List<string> BlackList;
        public List<string> WhiteList;
        public class Normal
        {
            public string MaxKill;
            public string KDFlag;
            public float MaxKD;
            public string KPMFlag;
            public float MaxKPM;
            public string MinRank;
            public string MaxRank;
            public float LifeMaxKD;
            public float LifeMaxKPM;
            public string LifeMaxWeaponStar;
            public string LifeMaxVehicleStar;
        }
        public class Weapon
        {
            public string English;
            public string Chinese;
        }
    }
}
