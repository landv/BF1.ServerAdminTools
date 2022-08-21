using BF1.ServerAdminTools.Models;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Features.API;
using BF1.ServerAdminTools.Features.API.RespJson;
using BF1.ServerAdminTools.Features.Utils;

namespace BF1.ServerAdminTools.Windows;

/// <summary>
/// QueryRecordWindow.xaml 的交互逻辑
/// </summary>
public partial class QueryRecordWindow : Window
{
    public class WeaponStats
    {
        public string name { get; set; }
        public string imageUrl { get; set; }
        public string star { get; set; }

        public int kills { get; set; }
        public string killsPerMinute { get; set; }

        public int headshots { get; set; }
        public string headshotsVKills { get; set; }

        public int shots { get; set; }
        public int hits { get; set; }
        public string hitsVShots { get; set; }

        public string hitVKills { get; set; }
        public string time { get; set; }
    }

    public class VehicleStats
    {
        public string name { get; set; }
        public string imageUrl { get; set; }
        public string star { get; set; }

        public int kills { get; set; }
        public string killsPerMinute { get; set; }

        public int destroyed { get; set; }
        public string time { get; set; }
    }

    public string PlayerName { get; set; }
    public long PersonaId { get; set; }
    public int Rank { get; set; }

    public QueryModel QueryModel { get; set; } = new();
    public ObservableCollection<string> PlayerDataOC { get; set; } = new();
    public ObservableCollection<WeaponStats> WeaponStatsOC { get; set; } = new();
    public ObservableCollection<VehicleStats> VehicleStatsOC { get; set; } = new();

    private int count = 0;

    public QueryRecordWindow(string playerName, long personaId, int rank)
    {
        InitializeComponent();

        PlayerName = playerName;
        PersonaId = personaId;
        Rank = rank;
    }

    private void Window_QueryRecord_Loaded(object sender, RoutedEventArgs e)
    {
        this.DataContext = this;

        this.Title = $"{this.Title} - 玩家ID : {PlayerName} - 数字ID : {PersonaId}";

        if (PersonaId != 0)
        {
            QueryModel.LoadingVisibility = Visibility.Visible;
            count = 0;

            GetPersonas();
            DetailedStats();
            GetWeapons();
            GetVehicles();
        }
    }

    private void Window_QueryRecord_Closing(object sender, CancelEventArgs e)
    {

    }

    private void IsAllFinish()
    {
        count++;

        if (count == 4)
        {
            QueryModel.LoadingVisibility = Visibility.Collapsed;
        }
    }

    private async void GetPersonas()
    {
        QueryModel.Avatar = string.Empty;
        QueryModel.PlayerName = string.Empty;
        QueryModel.PersonaId = string.Empty;
        QueryModel.Rank = string.Empty;

        var result = await BF1API.GetPersonasByIds(PersonaId);
        if (result.IsSuccess)
        {
            JsonNode jNode = JsonNode.Parse(result.Message);
            QueryModel.Avatar = jNode["result"]![$"{PersonaId}"]!["avatar"].GetValue<string>();

            QueryModel.PlayerName = PlayerName;
            QueryModel.PersonaId = $"{PersonaId}";
            QueryModel.Rank = $"等级 : {Rank}";

            IsAllFinish();
        }
    }

    private async void DetailedStats()
    {
        PlayerDataOC.Clear();

        QueryModel.PlayTime = string.Empty;

        var result = await BF1API.DetailedStatsByPersonaId(PersonaId);
        if (result.IsSuccess)
        {
            var detailed = JsonUtil.JsonDese<DetailedStats>(result.Message);

            var basic = detailed.result.basicStats;
            QueryModel.PlayTime = $"时长 : {PlayerUtil.GetPlayTime(basic.timePlayed)}";

            IsAllFinish();

            await Task.Run(() =>
            {
                AddPlayerInfo($"KD : {PlayerUtil.GetPlayerKD(basic.kills, basic.deaths):0.00}");
                AddPlayerInfo($"KPM : {basic.kpm}");
                AddPlayerInfo($"SPM : {basic.spm}");

                AddPlayerInfo($"命中率 : {detailed.result.accuracyRatio * 100:0.00}%");
                AddPlayerInfo($"爆头率 : {PlayerUtil.GetPlayerPercentage(detailed.result.headShots, basic.kills)}");
                AddPlayerInfo($"爆头数 : {detailed.result.headShots}");

                AddPlayerInfo($"最高连续击杀数 : {detailed.result.highestKillStreak}");
                AddPlayerInfo($"最远爆头距离 : {detailed.result.longestHeadShot}");
                AddPlayerInfo($"最佳兵种 : {detailed.result.favoriteClass}");

                AddPlayerInfo("");

                AddPlayerInfo($"击杀 : {basic.kills}");
                AddPlayerInfo($"死亡 : {basic.deaths}");
                AddPlayerInfo($"协助击杀数 : {detailed.result.killAssists}");

                AddPlayerInfo($"仇敌击杀数 : {detailed.result.avengerKills}");
                AddPlayerInfo($"救星击杀数 : {detailed.result.saviorKills}");
                AddPlayerInfo($"急救数 : {detailed.result.revives}");
                AddPlayerInfo($"治疗分 : {detailed.result.heals}");
                AddPlayerInfo($"修理分 : {detailed.result.repairs}");

                AddPlayerInfo("");

                AddPlayerInfo($"胜利场数 : {basic.wins}");
                AddPlayerInfo($"战败场数 : {basic.losses}");
                AddPlayerInfo($"胜率 : {PlayerUtil.GetPlayerPercentage(basic.wins, detailed.result.roundsPlayed)}");
                AddPlayerInfo($"技巧值 : {basic.skill}");
                AddPlayerInfo($"游戏总场数 : {detailed.result.roundsPlayed}");
                AddPlayerInfo($"取得狗牌数 : {detailed.result.dogtagsTaken}");

                AddPlayerInfo($"小隊分数 : {detailed.result.squadScore}");
                AddPlayerInfo($"奖励分数 : {detailed.result.awardScore}");
                AddPlayerInfo($"加成分数 : {detailed.result.bonusScore}");
            });
        }
    }

    private async void GetWeapons()
    {
        WeaponStatsOC.Clear();

        var result = await BF1API.GetWeaponsByPersonaId(PersonaId);
        if (result.IsSuccess)
        {
            var getWeapons = JsonUtil.JsonDese<GetWeapons>(result.Message);

            var weapons = new List<WeaponStats>();
            foreach (var res in getWeapons.result)
            {
                foreach (var wea in res.weapons)
                {
                    if (wea.stats.values.kills == 0)
                        continue;

                    weapons.Add(new WeaponStats()
                    {
                        name = ChsUtil.ToSimplifiedChinese(wea.name),
                        imageUrl = PlayerUtil.GetTempImagePath(wea.imageUrl, "weapons2"),
                        star = PlayerUtil.GetKillStar((int)wea.stats.values.kills),
                        kills = (int)wea.stats.values.kills,
                        killsPerMinute = PlayerUtil.GetPlayerKPM(wea.stats.values.kills, wea.stats.values.seconds),
                        headshots = (int)wea.stats.values.headshots,
                        headshotsVKills = PlayerUtil.GetPlayerPercentage(wea.stats.values.headshots, wea.stats.values.kills),
                        shots = (int)wea.stats.values.shots,
                        hits = (int)wea.stats.values.hits,
                        hitsVShots = PlayerUtil.GetPlayerPercentage(wea.stats.values.hits, wea.stats.values.shots),
                        hitVKills = $"{wea.stats.values.hits / wea.stats.values.kills:0.00}",
                        time = PlayerUtil.GetPlayTime(wea.stats.values.seconds)
                    });
                }
            }

            weapons.Sort((a, b) => b.kills.CompareTo(a.kills));

            IsAllFinish();

            await Task.Run(() =>
            {
                foreach (var item in weapons)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        WeaponStatsOC.Add(item);
                    }));
                }
            });
        }
    }

    private async void GetVehicles()
    {
        VehicleStatsOC.Clear();

        var result = await BF1API.GetVehiclesByPersonaId(PersonaId);
        if (result.IsSuccess)
        {
            var getVehicles = JsonUtil.JsonDese<GetVehicles>(result.Message);

            var vehicles = new List<VehicleStats>();
            foreach (var res in getVehicles.result)
            {
                foreach (var veh in res.vehicles)
                {
                    if (veh.stats.values.kills == 0)
                        continue;

                    vehicles.Add(new VehicleStats()
                    {
                        name = ChsUtil.ToSimplifiedChinese(veh.name),
                        imageUrl = PlayerUtil.GetTempImagePath(veh.imageUrl, "vehicles2"),
                        star = PlayerUtil.GetKillStar((int)veh.stats.values.kills),
                        kills = (int)veh.stats.values.kills,
                        killsPerMinute = PlayerUtil.GetPlayerKPM(veh.stats.values.kills, veh.stats.values.seconds),
                        destroyed = (int)veh.stats.values.destroyed,
                        time = PlayerUtil.GetPlayTime(veh.stats.values.seconds)
                    });
                }
            }

            vehicles.Sort((a, b) => b.kills.CompareTo(a.kills));

            IsAllFinish();

            await Task.Run(() =>
            {
                foreach (var item in vehicles)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        VehicleStatsOC.Add(item);
                    }));
                }
            });
        }
    }

    private void AddPlayerInfo(string str)
    {
        this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
        {
            PlayerDataOC.Add(str);
        }));
    }
}
