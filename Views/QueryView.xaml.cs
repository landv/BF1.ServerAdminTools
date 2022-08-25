using BF1.ServerAdminTools.Models;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.API;
using BF1.ServerAdminTools.Features.API.RespJson;
using BF1.ServerAdminTools.Features.Data;
using BF1.ServerAdminTools.Features.Utils;

using RestSharp;
using CommunityToolkit.Mvvm.Input;
using System.Xml.Linq;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// QueryView.xaml 的交互逻辑
/// </summary>
public partial class QueryView : UserControl
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

    public QueryModel QueryModel { get; set; } = new();
    public ObservableCollection<string> PlayerDataOC { get; set; } = new();
    public ObservableCollection<WeaponStats> WeaponStatsOC { get; set; } = new();
    public ObservableCollection<VehicleStats> VehicleStatsOC { get; set; } = new();

    private int count = 0;

    public RelayCommand QueryPlayerCommand { get; set; }

    public QueryView()
    {
        InitializeComponent();
        this.DataContext = this;

        QueryPlayerCommand = new(QueryPlayer);

        QueryModel.LoadingVisibility = Visibility.Collapsed;

        QueryModel.PlayerName = "CrazyZhang666";
    }

    private async void QueryPlayer()
    {
        AudioUtil.ClickSound();

        if (string.IsNullOrEmpty(QueryModel.PlayerName))
        {
            NotifierHelper.Show(NotifierType.Warning, "目标玩家名字为空，操作取消");
            return;
        }

        if (!string.IsNullOrEmpty(Globals.Remid) && !string.IsNullOrEmpty(Globals.Sid))
        {
            QueryModel.LoadingVisibility = Visibility.Visible;
            ClearData();
            NotifierHelper.Show(NotifierType.Information, "正在查询中，请稍后...");

            var str = "https://accounts.ea.com/connect/auth?response_type=token&locale=zh_CN&client_id=ORIGIN_JS_SDK&redirect_uri=nucleus%3Arest";
            var options = new RestClientOptions(str)
            {
                MaxTimeout = 5000,
                FollowRedirects = false
            };

            var client = new RestClient(options);
            var request = new RestRequest()
                .AddHeader("Cookie", $"remid={Globals.Remid};sid={Globals.Sid};");

            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JsonNode jNode = JsonNode.Parse(response.Content);
                var access_token = jNode["access_token"].GetValue<string>();

                str = $"https://gateway.ea.com/proxy/identity/personas?namespaceName=cem_ea_id&displayName={QueryModel.PlayerName}";
                options = new RestClientOptions(str)
                {
                    MaxTimeout = 5000,
                    FollowRedirects = false
                };

                client = new RestClient(options);
                request = new RestRequest()
                   .AddHeader("X-Expand-Results", true)
                   .AddHeader("Authorization", $"Bearer {access_token}");

                response = await client.ExecuteGetAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    jNode = JsonNode.Parse(response.Content);
                    if (jNode["personas"]!["persona"] != null)
                    {
                        var personaId = jNode["personas"]!["persona"][0]["personaId"].GetValue<long>();
                        QueryRecord(personaId);
                    }
                    else
                    {
                        QueryModel.LoadingVisibility = Visibility.Collapsed;
                        NotifierHelper.Show(NotifierType.Warning, $"玩家 {QueryModel.PlayerName} 不存在");
                    }
                }
                else
                {
                    QueryModel.LoadingVisibility = Visibility.Collapsed;
                    NotifierHelper.Show(NotifierType.Error, "网络请求错误");
                }
            }
            else
            {
                QueryModel.LoadingVisibility = Visibility.Collapsed;
                NotifierHelper.Show(NotifierType.Error, "玩家Cookies失效，请尝试刷新");
            }
        }
        else
        {
            NotifierHelper.Show(NotifierType.Error, "操作失败，玩家Remid或Sid为空");
        }
    }

    private void ClearData()
    {
        count = 0;

        QueryModel.Avatar = string.Empty;
        QueryModel.Emblem = string.Empty;

        QueryModel.PersonaId = string.Empty;
        QueryModel.Rank = string.Empty;
        QueryModel.PlayTime = string.Empty;
        QueryModel.PlayingServer = string.Empty;

        PlayerDataOC.Clear();
        WeaponStatsOC.Clear();
        VehicleStatsOC.Clear();
    }

    private void QueryRecord(long personaId)
    {
        DetailedStats(personaId);

        GetWeapons(personaId);
        GetVehicles(personaId);

        GetPersonas(personaId);
    }

    private void IsAllFinish()
    {
        count++;

        if (count == 4)
        {
            QueryModel.LoadingVisibility = Visibility.Collapsed;
        }
    }

    private async void GetPersonas(long personaId)
    {
        var result = await BF1API.GetPersonasByIds(personaId);
        if (result.IsSuccess)
        {
            JsonNode jNode = JsonNode.Parse(result.Message);
            QueryModel.Avatar = jNode["result"]![$"{personaId}"]!["avatar"].GetValue<string>();
            QueryModel.PersonaId = jNode["result"]![$"{personaId}"]!["personaId"].GetValue<string>();

            QueryModel.Rank = $"等级 : 0";

            IsAllFinish();
        }

        result = await BF1API.GetEquippedEmblem(personaId);
        if (result.IsSuccess)
        {
            JsonNode jNode = JsonNode.Parse(result.Message);
            if (jNode["result"] != null)
            {
                var img = jNode["result"].GetValue<string>();
                QueryModel.Emblem = img.Replace("[SIZE]", "256").Replace("[FORMAT]", "png");
            }
        }

        result = await BF1API.GetServersByPersonaIds(personaId);
        if (result.IsSuccess)
        {
            JsonNode jNode = JsonNode.Parse(result.Message);

            var obj = jNode["result"]![$"{personaId}"];
            if (obj != null)
            {
                var name = obj["name"].GetValue<string>();
                QueryModel.PlayingServer= $"正在游玩 : {name}";
            }
            else
            {
                QueryModel.PlayingServer = $"正在游玩 : 无";
            }
        }
    }

    private async void DetailedStats(long personaId)
    {
        var result = await BF1API.DetailedStatsByPersonaId(personaId);
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

    private async void GetWeapons(long personaId)
    {
        var result = await BF1API.GetWeaponsByPersonaId(personaId);
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

    private async void GetVehicles(long personaId)
    {
        var result = await BF1API.GetVehiclesByPersonaId(personaId);
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
