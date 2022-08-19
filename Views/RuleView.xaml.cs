using BF1.ServerAdminTools.Models.Rule;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.Data;
using BF1.ServerAdminTools.Features.Config;
using BF1.ServerAdminTools.Features.Client;
using BF1.ServerAdminTools.Features.API;
using BF1.ServerAdminTools.Features.API.RespJson;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// RuleView.xaml 的交互逻辑
/// </summary>
public partial class RuleView : UserControl
{
    private List<RuleConfig> RuleConfigs { get; set; } = new();

    public RuleTeamModel RuleTeam1Model { get; set; } = new();
    public RuleTeamModel RuleTeam2Model { get; set; } = new();

    public ObservableCollection<RuleWeaponModel> DataGrid_RuleWeaponModels { get; set; } = new();
    public ObservableCollection<string> ComboBox_ConfigNames { get; set; } = new();

    /// <summary>
    /// 是否已经执行
    /// </summary>
    private bool isHasBeenExec = false;
    /// <summary>
    /// 是否执行应用规则
    /// </summary>
    private bool isApplyRule = false;

    public RuleView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.ClosingDisposeEvent += MainWindow_ClosingDisposeEvent;

        // 添加武器信息列表
        foreach (var item in WeaponData.AllWeaponInfo)
        {
            DataGrid_RuleWeaponModels.Add(new RuleWeaponModel()
            {
                Class = item.Class,
                Name = item.Chinese,
                English = item.English,
                Team1 = false,
                Team2 = false
            });
        }

        var thread0 = new Thread(AutoKickLifeBreakPlayer)
        {
            IsBackground = true
        };
        thread0.Start();

        var therad1 = new Thread(CheckState)
        {
            IsBackground = true
        };
        therad1.Start();

        if (!File.Exists(FileUtil.F_Rule_Path))
        {
            for (int i = 0; i < 10; i++)
            {
                RuleConfigs.Add(new RuleConfig()
                {
                    RuleName = $"自定义规则 {i}",
                    RuleInfos = new RuleConfig.RuleInfo()
                    {
                        Team1Normal = new RuleConfig.RuleInfo.Normal()
                        {
                            MaxKill = 0,
                            KDFlag = 0,
                            MaxKD = 0.00f,
                            KPMFlag = 0,
                            MaxKPM = 0.00f,
                            MinRank = 0,
                            MaxRank = 0,
                            LifeMaxKD = 0.00f,
                            LifeMaxKPM = 0.00f,
                            LifeMaxWeaponStar = 0,
                            LifeMaxVehicleStar = 0
                        },
                        Team2Normal = new RuleConfig.RuleInfo.Normal()
                        {
                            MaxKill = 0,
                            KDFlag = 0,
                            MaxKD = 0.00f,
                            KPMFlag = 0,
                            MaxKPM = 0.00f,
                            MinRank = 0,
                            MaxRank = 0,
                            LifeMaxKD = 0.00f,
                            LifeMaxKPM = 0.00f,
                            LifeMaxWeaponStar = 0,
                            LifeMaxVehicleStar = 0
                        },
                        Team1Weapon = new List<string>() { },
                        Team2Weapon = new List<string>() { },
                        BlackList = new List<string>() { },
                        WhiteList = new List<string>() { }
                    }
                });
            }

            File.WriteAllText(FileUtil.F_Rule_Path, JsonUtil.JsonSeri(RuleConfigs));
        }

        if (File.Exists(FileUtil.F_Rule_Path))
        {
            using (var streamReader = new StreamReader(FileUtil.F_Rule_Path))
            {
                RuleConfigs = JsonUtil.JsonDese<List<RuleConfig>>(streamReader.ReadToEnd());

                foreach (var item in RuleConfigs)
                {
                    ComboBox_ConfigNames.Add(item.RuleName);
                }

                ApplyRuleByIndex(0);
            }
        }
    }

    private void MainWindow_ClosingDisposeEvent()
    {
        File.WriteAllText(FileUtil.F_Rule_Path, JsonUtil.JsonSeri(RuleConfigs));
    }

    /// <summary>
    /// 应用规则
    /// </summary>
    /// <param name="index"></param>
    private void ApplyRuleByIndex(int index)
    {
        var rule = RuleConfigs[index].RuleInfos;

        RuleTeam1Model.MaxKill = rule.Team1Normal.MaxKill;
        RuleTeam1Model.KDFlag = rule.Team1Normal.KDFlag;
        RuleTeam1Model.MaxKD = rule.Team1Normal.MaxKD;
        RuleTeam1Model.KPMFlag = rule.Team1Normal.KPMFlag;
        RuleTeam1Model.MaxKPM = rule.Team1Normal.MaxKPM;
        RuleTeam1Model.MinRank = rule.Team1Normal.MinRank;
        RuleTeam1Model.MaxRank = rule.Team1Normal.MaxRank;
        RuleTeam1Model.LifeMaxKD = rule.Team1Normal.LifeMaxKD;
        RuleTeam1Model.LifeMaxKPM = rule.Team1Normal.LifeMaxKPM;
        RuleTeam1Model.LifeMaxWeaponStar = rule.Team1Normal.LifeMaxWeaponStar;
        RuleTeam1Model.LifeMaxVehicleStar = rule.Team1Normal.LifeMaxVehicleStar;

        RuleTeam2Model.MaxKill = rule.Team2Normal.MaxKill;
        RuleTeam2Model.KDFlag = rule.Team2Normal.KDFlag;
        RuleTeam2Model.MaxKD = rule.Team2Normal.MaxKD;
        RuleTeam2Model.KPMFlag = rule.Team2Normal.KPMFlag;
        RuleTeam2Model.MaxKPM = rule.Team2Normal.MaxKPM;
        RuleTeam2Model.MinRank = rule.Team2Normal.MinRank;
        RuleTeam2Model.MaxRank = rule.Team2Normal.MaxRank;
        RuleTeam2Model.LifeMaxKD = rule.Team2Normal.LifeMaxKD;
        RuleTeam2Model.LifeMaxKPM = rule.Team2Normal.LifeMaxKPM;
        RuleTeam2Model.LifeMaxWeaponStar = rule.Team2Normal.LifeMaxWeaponStar;
        RuleTeam2Model.LifeMaxVehicleStar = rule.Team2Normal.LifeMaxVehicleStar;

        ListBox_Custom_BlackList.Items.Clear();
        foreach (var item in rule.BlackList)
        {
            ListBox_Custom_BlackList.Items.Add(item);
        }

        ListBox_Custom_WhiteList.Items.Clear();
        foreach (var item in rule.WhiteList)
        {
            ListBox_Custom_WhiteList.Items.Add(item);
        }

        for (int i = 0; i < DataGrid_RuleWeaponModels.Count; i++)
        {
            var item = DataGrid_RuleWeaponModels[i];

            var v1 = rule.Team1Weapon.IndexOf(item.English);
            if (v1 != -1)
                item.Team1 = true;
            else
                item.Team1 = false;

            var v2 = rule.Team2Weapon.IndexOf(item.English);
            if (v2 != -1)
                item.Team2 = true;
            else
                item.Team2 = false;
        }
    }

    /// <summary>
    /// 保存规则
    /// </summary>
    /// <param name="index"></param>
    private void SaveRuleByIndex(int index)
    {
        var rule = RuleConfigs[index].RuleInfos;

        rule.Team1Normal.MaxKill = RuleTeam1Model.MaxKill;
        rule.Team1Normal.KDFlag = RuleTeam1Model.KDFlag;
        rule.Team1Normal.MaxKD = RuleTeam1Model.MaxKD;
        rule.Team1Normal.KPMFlag = RuleTeam1Model.KPMFlag;
        rule.Team1Normal.MaxKPM = RuleTeam1Model.MaxKPM;
        rule.Team1Normal.MinRank = RuleTeam1Model.MinRank;
        rule.Team1Normal.MaxRank = RuleTeam1Model.MaxRank;
        rule.Team1Normal.LifeMaxKD = RuleTeam1Model.LifeMaxKD;
        rule.Team1Normal.LifeMaxKPM = RuleTeam1Model.LifeMaxKPM;
        rule.Team1Normal.LifeMaxWeaponStar = RuleTeam1Model.LifeMaxWeaponStar;
        rule.Team1Normal.LifeMaxVehicleStar = RuleTeam1Model.LifeMaxVehicleStar;

        rule.Team2Normal.MaxKill = RuleTeam2Model.MaxKill;
        rule.Team2Normal.KDFlag = RuleTeam2Model.KDFlag;
        rule.Team2Normal.MaxKD = RuleTeam2Model.MaxKD;
        rule.Team2Normal.KPMFlag = RuleTeam2Model.KPMFlag;
        rule.Team2Normal.MaxKPM = RuleTeam2Model.MaxKPM;
        rule.Team2Normal.MinRank = RuleTeam2Model.MinRank;
        rule.Team2Normal.MaxRank = RuleTeam2Model.MaxRank;
        rule.Team2Normal.LifeMaxKD = RuleTeam2Model.LifeMaxKD;
        rule.Team2Normal.LifeMaxKPM = RuleTeam2Model.LifeMaxKPM;
        rule.Team2Normal.LifeMaxWeaponStar = RuleTeam2Model.LifeMaxWeaponStar;
        rule.Team2Normal.LifeMaxVehicleStar = RuleTeam2Model.LifeMaxVehicleStar;

        rule.BlackList.Clear();
        foreach (string item in ListBox_Custom_BlackList.Items)
        {
            rule.BlackList.Add(item);
        }

        rule.WhiteList.Clear();
        foreach (string item in ListBox_Custom_WhiteList.Items)
        {
            rule.WhiteList.Add(item);
        }

        rule.Team1Weapon.Clear();
        rule.Team2Weapon.Clear();
        for (int i = 0; i < DataGrid_RuleWeaponModels.Count; i++)
        {
            var item = DataGrid_RuleWeaponModels[i];
            if (item.Team1)
                rule.Team1Weapon.Add(item.English);

            if (item.Team2)
                rule.Team2Weapon.Add(item.English);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////

    private void CheckState()
    {
        while (true)
        {
            if (string.IsNullOrEmpty(Globals.GameId))
            {
                if (!isHasBeenExec)
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        if (CheckBox_RunAutoKick.IsChecked == true)
                        {
                            CheckBox_RunAutoKick.IsChecked = false;
                            Globals.AutoKickBreakPlayer = false;
                        }
                    });

                    isHasBeenExec = true;
                }
            }

            Thread.Sleep(1000);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 自动踢出生涯违规玩家
    /// </summary>
    private void AutoKickLifeBreakPlayer()
    {
        while (true)
        {
            // 自动踢出违规玩家
            if (Globals.AutoKickBreakPlayer)
            {
                var team1Player = JsonSerializer.Deserialize<List<PlayerData>>(JsonSerializer.Serialize(ScoreView.PlayerDatas_Team1));
                var team2Player = JsonSerializer.Deserialize<List<PlayerData>>(JsonSerializer.Serialize(ScoreView.PlayerDatas_Team2));

                foreach (var item in team1Player)
                {
                    CheckBreakLifePlayer(item);
                }

                foreach (var item in team2Player)
                {
                    CheckBreakLifePlayer(item);
                }
            }

            Thread.Sleep(5000);
        }
    }

    /// <summary>
    /// 检查生涯违规玩家
    /// </summary>
    /// <param name="data"></param>
    private async void CheckBreakLifePlayer(PlayerData data)
    {
        // 跳过管理员
        if (Globals.Server_AdminList_PID.Contains(data.PersonaId.ToString()))
            return;

        // 跳过白名单玩家
        if (Globals.Custom_WhiteList.Contains(data.Name))
            return;

        var resultTemp = await BF1API.DetailedStatsByPersonaId(data.PersonaId.ToString());
        if (resultTemp.IsSuccess)
        {
            var detailedStats = JsonUtil.JsonDese<DetailedStats>(resultTemp.Message);

            // 拿到该玩家的生涯数据
            int kills = detailedStats.result.basicStats.kills;
            int deaths = detailedStats.result.basicStats.deaths;

            float kd = (float)Math.Round((double)kills / deaths, 2);
            float kpm = detailedStats.result.basicStats.kpm;

            //int weaponStar = (int)detailedStats.result.gameStats.tunguska.highlightsByType.weapon[0].highlightDetails.stats.values.kills;
            //int vehicleStar = (int)detailedStats.result.gameStats.tunguska.highlightsByType.vehicle[0].highlightDetails.stats.values.kills;

            //weaponStar = weaponStar / 100;
            //vehicleStar = vehicleStar / 100;

            // 限制玩家生涯KD
            if (ServerRule.Team1.LifeMaxKD != 0 && kd > ServerRule.Team1.LifeMaxKD)
            {
                AutoKickPlayer(new BreakRuleInfo
                {
                    Name = data.Name,
                    PersonaId = data.PersonaId,
                    Reason = $"Life KD Limit {ServerRule.Team1.LifeMaxKD:0.00}"
                });

                return;
            }

            // 限制玩家生涯KPM
            if (ServerRule.Team1.LifeMaxKPM != 0 && kpm > ServerRule.Team1.LifeMaxKPM)
            {
                AutoKickPlayer(new BreakRuleInfo
                {
                    Name = data.Name,
                    PersonaId = data.PersonaId,
                    Reason = $"Life KPM Limit {ServerRule.Team1.LifeMaxKPM:0.00}"
                });

                return;
            }

            //// 限制玩家武器星级
            //if (ServerRule.LifeMaxWeaponStar != 0 && weaponStar > ServerRule.LifeMaxWeaponStar)
            //{
            //    AutoKickPlayer(new BreakRuleInfo
            //    {
            //        Name = data.Name,
            //        PersonaId = data.PersonaId,
            //        Reason = $"Life Weapon Star Limit {ServerRule.LifeMaxWeaponStar:0}"
            //    });

            //    return;
            //}

            //// 限制玩家载具星级
            //if (ServerRule.LifeMaxVehicleStar != 0 && vehicleStar > ServerRule.LifeMaxVehicleStar)
            //{
            //    AutoKickPlayer(new BreakRuleInfo
            //    {
            //        Name = data.Name,
            //        PersonaId = data.PersonaId,
            //        Reason = $"Life Vehicle Star Limit {ServerRule.LifeMaxVehicleStar:0}"
            //    });

            //    return;
            //}
        }
    }

    /// <summary>
    /// 自动踢出普通违规玩家
    /// </summary>
    /// <param name="info"></param>
    private async void AutoKickPlayer(BreakRuleInfo info)
    {
        var result = await BF1API.AdminKickPlayer(info.PersonaId.ToString(), info.Reason);
        if (result.IsSuccess)
        {
            info.Status = "踢出成功";
            info.Time = DateTime.Now;
            LogView._dAddKickOKLog(info);
        }
        else
        {
            info.Status = "踢出失败 " + result.Message;
            info.Time = DateTime.Now;
            LogView._dAddKickNOLog(info);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////

    private void AppendLog(string msg)
    {
        TextBox_RuleLog.AppendText(msg + "\n");
    }

    private void Button_ApplyRule_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        TextBox_RuleLog.Clear();

        AppendLog("===== 操作时间 =====");
        AppendLog("");
        AppendLog($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}");
        AppendLog("");

        ServerRule.Team1.MaxKill = RuleTeam1Model.MaxKill;
        ServerRule.Team1.KDFlag = RuleTeam1Model.KDFlag;
        ServerRule.Team1.MaxKD = RuleTeam1Model.MaxKD;
        ServerRule.Team1.KPMFlag = RuleTeam1Model.KPMFlag;
        ServerRule.Team1.MaxKPM = RuleTeam1Model.MaxKPM;
        ServerRule.Team1.MinRank = RuleTeam1Model.MinRank;
        ServerRule.Team1.MaxRank = RuleTeam1Model.MaxRank;

        ServerRule.Team2.MaxKill = RuleTeam2Model.MaxKill;
        ServerRule.Team2.KDFlag = RuleTeam2Model.KDFlag;
        ServerRule.Team2.MaxKD = RuleTeam2Model.MaxKD;
        ServerRule.Team2.KPMFlag = RuleTeam2Model.KPMFlag;
        ServerRule.Team2.MaxKPM = RuleTeam2Model.MaxKPM;
        ServerRule.Team2.MinRank = RuleTeam2Model.MinRank;
        ServerRule.Team2.MaxRank = RuleTeam2Model.MaxRank;

        ServerRule.Team1.LifeMaxKD = RuleTeam1Model.LifeMaxKD;
        ServerRule.Team1.LifeMaxKPM = RuleTeam1Model.LifeMaxKPM;
        ServerRule.Team1.LifeMaxWeaponStar = RuleTeam1Model.LifeMaxWeaponStar;
        ServerRule.Team1.LifeMaxVehicleStar = RuleTeam1Model.LifeMaxVehicleStar;

        ServerRule.Team2.LifeMaxKD = RuleTeam2Model.LifeMaxKD;
        ServerRule.Team2.LifeMaxKPM = RuleTeam2Model.LifeMaxKPM;
        ServerRule.Team2.LifeMaxWeaponStar = RuleTeam2Model.LifeMaxWeaponStar;
        ServerRule.Team2.LifeMaxVehicleStar = RuleTeam2Model.LifeMaxVehicleStar;

        /////////////////////////////////////////////////////////////////////////////

        if (ServerRule.Team1.MinRank >= ServerRule.Team1.MaxRank && ServerRule.Team1.MinRank != 0 && ServerRule.Team1.MaxRank != 0)
        {
            Globals.IsRuleSetRight = false;
            isApplyRule = false;

            AppendLog($"队伍1 限制等级规则设置不正确");
            AppendLog("");

            NotifierHelper.Show(NotiferType.Warning, $"队伍1 限制等级规则设置不正确");

            return;
        }

        if (ServerRule.Team2.MinRank >= ServerRule.Team2.MaxRank && ServerRule.Team2.MinRank != 0 && ServerRule.Team2.MaxRank != 0)
        {
            Globals.IsRuleSetRight = false;
            isApplyRule = false;

            AppendLog($"队伍2 限制等级规则设置不正确");
            AppendLog("");

            NotifierHelper.Show(NotiferType.Warning, $"队伍2 限制等级规则设置不正确");

            return;
        }

        /////////////////////////////////////////////////////////////////////////////

        // 清空限制武器列表
        Globals.Custom_WeaponList_Team1.Clear();
        Globals.Custom_WeaponList_Team2.Clear();
        // 添加自定义限制武器
        foreach (var item in DataGrid_RuleWeaponModels)
        {
            if (item.Team1)
            {
                Globals.Custom_WeaponList_Team1.Add(item.English);
            }

            if (item.Team2)
            {
                Globals.Custom_WeaponList_Team2.Add(item.English);
            }
        }

        // 清空黑名单列表
        Globals.Custom_BlackList.Clear();
        // 添加自定义黑名单列表
        foreach (var item in ListBox_Custom_BlackList.Items)
        {
            Globals.Custom_BlackList.Add(item as string);
        }

        // 清空白名单列表
        Globals.Custom_WhiteList.Clear();
        // 添加自定义白名单列表
        foreach (var item in ListBox_Custom_WhiteList.Items)
        {
            Globals.Custom_WhiteList.Add(item as string);
        }

        if (CheckBox_RunAutoKick.IsChecked == true)
        {
            CheckBox_RunAutoKick.IsChecked = false;
            Globals.AutoKickBreakPlayer = false;
        }

        Globals.IsRuleSetRight = true;
        isApplyRule = true;

        AppendLog($"成功提交当前规则，请重新启动自动踢人功能");
        AppendLog("");

        NotifierHelper.Show(NotiferType.Success, $"应用当前规则成功，请点击<查询当前规则>检验规则是否正确");

        Task.Run(() =>
        {
            Task.Delay(10000).Wait();

            isApplyRule = false;
        });
    }

    private void Button_QueryRule_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        TextBox_RuleLog.Clear();

        AppendLog("===== 查询时间 =====");
        AppendLog("");
        AppendLog($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}");
        AppendLog("");

        AppendLog("==== 队伍1 ====");
        AppendLog("");
        AppendLog($"玩家最高击杀限制 : {ServerRule.Team1.MaxKill}");

        AppendLog($"计算玩家KD的最低击杀数 : {ServerRule.Team1.KDFlag}");
        AppendLog($"玩家最高KD限制 : {ServerRule.Team1.MaxKD}");

        AppendLog($"计算玩家KPM的最低击杀数 : {ServerRule.Team1.KPMFlag}");
        AppendLog($"玩家最高KPM限制 : {ServerRule.Team1.MaxKPM}");

        AppendLog($"玩家最低等级限制 : {ServerRule.Team1.MinRank}");
        AppendLog($"玩家最高等级限制 : {ServerRule.Team1.MaxRank}");

        AppendLog($"玩家最高生涯KD限制 : {ServerRule.Team1.LifeMaxKD}");
        AppendLog($"玩家最高生涯KPM限制 : {ServerRule.Team1.LifeMaxKPM}");

        AppendLog($"玩家最高生涯武器星数限制 : {ServerRule.Team1.LifeMaxWeaponStar}");
        AppendLog($"玩家最高生涯载具星数限制 : {ServerRule.Team1.LifeMaxVehicleStar}");
        AppendLog("");

        AppendLog("==== 队伍2 ====");
        AppendLog("");
        AppendLog($"玩家最高击杀限制 : {ServerRule.Team2.MaxKill}");

        AppendLog($"计算玩家KD的最低击杀数 : {ServerRule.Team2.KDFlag}");
        AppendLog($"玩家最高KD限制 : {ServerRule.Team2.MaxKD}");

        AppendLog($"计算玩家KPM的最低击杀数 : {ServerRule.Team2.KPMFlag}");
        AppendLog($"玩家最高KPM限制 : {ServerRule.Team2.MaxKPM}");

        AppendLog($"玩家最低等级限制 : {ServerRule.Team2.MinRank}");
        AppendLog($"玩家最高等级限制 : {ServerRule.Team2.MaxRank}");

        AppendLog($"玩家最高生涯KD限制 : {ServerRule.Team2.LifeMaxKD}");
        AppendLog($"玩家最高生涯KPM限制 : {ServerRule.Team2.LifeMaxKPM}");

        AppendLog($"玩家最高生涯武器星数限制 : {ServerRule.Team2.LifeMaxWeaponStar}");
        AppendLog($"玩家最高生涯载具星数限制 : {ServerRule.Team2.LifeMaxVehicleStar}");

        AppendLog("\n");

        AppendLog($"========== 队伍1 禁武器列表 ==========");
        AppendLog("");
        foreach (var item in Globals.Custom_WeaponList_Team1)
        {
            AppendLog($"武器名称 {Globals.Custom_WeaponList_Team1.IndexOf(item) + 1} : {item}");
        }
        AppendLog("\n");

        AppendLog($"========== 队伍2 禁武器列表 ==========");
        AppendLog("");
        foreach (var item in Globals.Custom_WeaponList_Team2)
        {
            AppendLog($"武器名称 {Globals.Custom_WeaponList_Team2.IndexOf(item) + 1} : {item}");
        }
        AppendLog("\n");

        AppendLog($"========== 黑名单列表 ==========");
        AppendLog("");
        foreach (var item in Globals.Custom_BlackList)
        {
            AppendLog($"玩家ID {Globals.Custom_BlackList.IndexOf(item) + 1} : {item}");
        }
        AppendLog("\n");

        AppendLog($"========== 白名单列表 ==========");
        AppendLog("");
        foreach (var item in Globals.Custom_WhiteList)
        {
            AppendLog($"玩家ID {Globals.Custom_WhiteList.IndexOf(item) + 1} : {item}");
        }
        AppendLog("\n");

        NotifierHelper.Show(NotiferType.Success, $"查询当前规则成功，请点击<检查违规玩家>测试是否正确");
    }

    private void Button_CheckBreakRulePlayer_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        TextBox_RuleLog.Clear();

        AppendLog("===== 查询时间 =====");
        AppendLog("");
        AppendLog($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}");
        AppendLog("");

        int index = 1;
        AppendLog($"========== 违规类型 : 限制玩家最高击杀 ==========");
        AppendLog("");
        foreach (var item in Globals.BreakRuleInfo_PlayerList)
        {
            if (item.Reason.Contains("Kill Limit"))
            {
                AppendLog($"玩家ID {index++} : {item.Name}");
            }
        }
        AppendLog("\n");

        index = 1;
        AppendLog($"========== 违规类型 : 限制玩家最高KD ==========");
        AppendLog("");
        foreach (var item in Globals.BreakRuleInfo_PlayerList)
        {
            if (item.Reason.Contains("KD Limit"))
            {
                AppendLog($"玩家ID {index++} : {item.Name}");
            }
        }
        AppendLog("\n");

        index = 1;
        AppendLog($"========== 违规类型 : 限制玩家最高KPM ==========");
        AppendLog("");
        foreach (var item in Globals.BreakRuleInfo_PlayerList)
        {
            if (item.Reason.Contains("KPM Limit"))
            {
                AppendLog($"玩家ID {index++} : {item.Name}");
            }
        }
        AppendLog("\n");

        index = 1;
        AppendLog($"========== 违规类型 : 限制玩家等级范围 ==========");
        AppendLog("");
        foreach (var item in Globals.BreakRuleInfo_PlayerList)
        {
            if (item.Reason.Contains("Rank Limit"))
            {
                AppendLog($"玩家ID {index++} : {item.Name}");
            }
        }
        AppendLog("\n");

        index = 1;
        AppendLog($"========== 违规类型 : 限制玩家使用武器 ==========");
        AppendLog("");
        foreach (var item in Globals.BreakRuleInfo_PlayerList)
        {
            if (item.Reason.Contains("Weapon Limit"))
            {
                AppendLog($"玩家ID {index++} : {item.Name}");
            }
        }
        AppendLog("\n");

        NotifierHelper.Show(NotiferType.Success, $"检查违规玩家成功，如果符合规则就可以勾选<激活自动踢出违规玩家>了");
    }

    private void Button_Add_BlackList_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        if (TextBox_BlackList_PlayerName.Text != "")
        {
            bool isContains = false;

            foreach (var item in ListBox_Custom_BlackList.Items)
            {
                if ((item as string) == TextBox_BlackList_PlayerName.Text)
                {
                    isContains = true;
                }
            }

            if (!isContains)
            {
                ListBox_Custom_BlackList.Items.Add(TextBox_BlackList_PlayerName.Text);

                NotifierHelper.Show(NotiferType.Success, $"添加 {TextBox_BlackList_PlayerName.Text} 到黑名单列表成功");
                TextBox_BlackList_PlayerName.Text = "";
            }
            else
            {
                NotifierHelper.Show(NotiferType.Warning, $"该项 {TextBox_BlackList_PlayerName.Text} 已经存在了，请不要重复添加");
                TextBox_BlackList_PlayerName.Text = "";
            }
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, $"待添加黑名单玩家ID为空，添加操作取消");
        }
    }

    private void Button_Remove_BlackList_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        if (ListBox_Custom_BlackList.SelectedIndex != -1)
        {
            NotifierHelper.Show(NotiferType.Success, $"从黑名单列表删除（{ListBox_Custom_BlackList.SelectedItem}）成功");
            ListBox_Custom_BlackList.Items.Remove(ListBox_Custom_BlackList.SelectedItem);
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, $"请正确选中你要删除的玩家ID或自定义黑名单列表为空，删除操作取消");
        }
    }

    private void Button_Clear_BlackList_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        // 清空黑名单列表
        Globals.Custom_BlackList.Clear();
        ListBox_Custom_BlackList.Items.Clear();

        NotifierHelper.Show(NotiferType.Success, $"清空黑名单列表成功");
    }

    private void Button_Add_WhiteList_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        if (TextBox_WhiteList_PlayerName.Text != "")
        {
            bool isContains = false;

            foreach (var item in ListBox_Custom_WhiteList.Items)
            {
                if ((item as string) == TextBox_WhiteList_PlayerName.Text)
                {
                    isContains = true;
                }
            }

            if (!isContains)
            {
                ListBox_Custom_WhiteList.Items.Add(TextBox_WhiteList_PlayerName.Text);

                NotifierHelper.Show(NotiferType.Success, $"添加 {TextBox_WhiteList_PlayerName.Text} 到白名单列表成功");

                TextBox_WhiteList_PlayerName.Text = "";
            }
            else
            {
                NotifierHelper.Show(NotiferType.Warning, $"该项 {TextBox_WhiteList_PlayerName.Text} 已经存在了，请不要重复添加");
                TextBox_WhiteList_PlayerName.Text = "";
            }
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, $"待添加白名单玩家ID为空，添加操作取消");
        }
    }

    private void Button_Remove_WhiteList_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        if (ListBox_Custom_WhiteList.SelectedIndex != -1)
        {
            NotifierHelper.Show(NotiferType.Success, $"从白名单列表删除（{ListBox_Custom_WhiteList.SelectedItem}）成功");
            ListBox_Custom_WhiteList.Items.Remove(ListBox_Custom_WhiteList.SelectedItem);
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, $"请正确选中你要删除的玩家ID或自定义白名单列表为空，删除操作取消");
        }
    }

    private void Button_Clear_WhiteList_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        // 清空白名单列表
        Globals.Custom_WhiteList.Clear();
        ListBox_Custom_WhiteList.Items.Clear();

        NotifierHelper.Show(NotiferType.Success, $"清空白名单列表成功");
    }

    /// <summary>
    /// 检查自动踢人环境是否合格
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CheckKickEnv()
    {
        TextBox_RuleLog.Clear();

        NotifierHelper.Show(NotiferType.Information, $"正在检查环境...");

        AppendLog("===== 操作时间 =====");
        AppendLog("");
        AppendLog($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}");

        AppendLog("");
        AppendLog("正在检查玩家是否应用规则...");
        if (!isApplyRule)
        {
            AppendLog("❌ 玩家没有正确应用规则，请点击应用当前规则，操作取消");
            NotifierHelper.Show(NotiferType.Warning, $"环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AppendLog("✔ 玩家已正确应用规则");
        }

        AppendLog("");
        AppendLog("正在检查 SessionId 是否正确...");
        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            AppendLog("❌ SessionId为空，请先获取SessionId，操作取消");
            NotifierHelper.Show(NotiferType.Warning, $"环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AppendLog("✔ SessionId 检查正确");
        }

        AppendLog("");
        AppendLog("正在检查 SessionId 是否有效...");
        var result = await BF1API.GetWelcomeMessage();
        if (!result.IsSuccess)
        {
            AppendLog("❌ SessionId 已过期，请刷新SessionId，操作取消");
            NotifierHelper.Show(NotiferType.Warning, $"环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AppendLog("✔ SessionId 检查有效");
        }

        AppendLog("");
        AppendLog("正在检查 GameId 是否正确...");
        if (string.IsNullOrEmpty(Globals.GameId))
        {
            AppendLog("❌ GameId 为空，请先进入服务器，操作取消");
            NotifierHelper.Show(NotiferType.Warning, $"环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AppendLog("✔ GameId检查正确");
        }

        AppendLog("");
        AppendLog("正在检查 服务器管理员列表 是否正确...");
        if (Globals.Server_AdminList_PID.Count == 0)
        {
            AppendLog("❌ 服务器管理员列表 为空，请先获取当前服务器详情数据，操作取消");
            NotifierHelper.Show(NotiferType.Warning, $"环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AppendLog("✔ 服务器管理员列表 检查正确");
        }

        AppendLog("");
        AppendLog("正在检查 玩家是否为当前服务器管理...");
        var welcomeMsg = JsonUtil.JsonDese<WelcomeMsg>(result.Message);
        var firstMessage = welcomeMsg.result.firstMessage;
        string playerName = firstMessage.Substring(0, firstMessage.IndexOf("，"));
        if (!Globals.Server_AdminList_Name.Contains(playerName))
        {
            AppendLog("❌ 玩家不是当前服务器管理，请确认服务器是否选择正确，操作取消");
            NotifierHelper.Show(NotiferType.Warning, $"环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AppendLog("✔ 已确认玩家为当前服务器管理");
        }

        return true;
    }

    // 开启自动踢人
    private async void CheckBox_RunAutoKick_Click(object sender, RoutedEventArgs e)
    {
        if (CheckBox_RunAutoKick.IsChecked == true)
        {
            // 检查自动踢人环境
            if (await CheckKickEnv())
            {
                AppendLog("");
                AppendLog("环境检查完毕，自动踢人已开启");

                isHasBeenExec = false;

                Globals.AutoKickBreakPlayer = true;
                NotifierHelper.Show(NotiferType.Success, $"自动踢人开启成功");
            }
            else
            {
                Globals.AutoKickBreakPlayer = false;
                CheckBox_RunAutoKick.IsChecked = false;
            }
        }
        else
        {
            Globals.AutoKickBreakPlayer = false;
            NotifierHelper.Show(NotiferType.Success, $"自动踢人关闭成功");
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessUtil.OpenLink(e.Uri.OriginalString);
        e.Handled = true;
    }

    private void Button_OpenConfigurationFolder_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        ProcessUtil.OpenLink(FileUtil.Default_Path);
    }

    private async void ManualKickPlayer(BreakRuleInfo info)
    {
        // 跳过管理员
        if (!Globals.Server_AdminList_PID.Contains(info.Name))
        {
            // 白名单玩家不踢出
            if (!Globals.Custom_WhiteList.Contains(info.Name))
            {
                var result = await BF1API.AdminKickPlayer(info.PersonaId.ToString(), info.Reason);

                if (result.IsSuccess)
                {
                    info.Status = "踢出成功";
                    LogView._dAddKickOKLog(info);
                }
                else
                {
                    info.Status = "踢出失败 " + result.Message;
                    LogView._dAddKickNOLog(info);
                }
            }
        }
    }

    private async void Button_ManualKickBreakRulePlayer_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        // 检查自动踢人环境
        if (await CheckKickEnv())
        {
            AppendLog("");
            AppendLog("环境检查完毕，执行手动踢人操作成功，请查看日志了解执行结果");

            for (int i = 0; i < Globals.BreakRuleInfo_PlayerList.Count; i++)
            {
                ManualKickPlayer(Globals.BreakRuleInfo_PlayerList[i]);
            }

            var team1Player = JsonSerializer.Deserialize<List<PlayerData>>(JsonSerializer.Serialize(ScoreView.PlayerDatas_Team1));
            var team2Player = JsonSerializer.Deserialize<List<PlayerData>>(JsonSerializer.Serialize(ScoreView.PlayerDatas_Team2));

            foreach (var item in team1Player)
            {
                CheckBreakLifePlayer(item);
            }

            foreach (var item in team2Player)
            {
                CheckBreakLifePlayer(item);
            }

            NotifierHelper.Show(NotiferType.Success, "执行手动踢人操作成功，请查看日志了解执行结果");
        }
    }

    private async void Button_CheckKickEnv_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        // 检查自动踢人环境
        if (await CheckKickEnv())
        {
            AppendLog("");
            AppendLog("环境检查完毕，自动踢人可以开启");

            NotifierHelper.Show(NotiferType.Success, $"环境检查完毕，自动踢人可以开启");
        }
    }

    private void Button_ReNameRule_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        var name = TextBox_ReNameRule.Text.Trim();
        if (string.IsNullOrEmpty(name))
            return;

        var index = ComboBox_CustomConfigName.SelectedIndex;
        if (index == -1)
            return;

        ComboBox_ConfigNames[index] = name;
        RuleConfigs[index].RuleName = name;

        ComboBox_CustomConfigName.SelectedIndex = index;
    }

    private void Button_SaveCurrentRule_Click(object sender, RoutedEventArgs e)
    {
        AudioUtil.ClickSound();

        var index = ComboBox_CustomConfigName.SelectedIndex;
        if (index == -1)
            return;

        SaveRuleByIndex(index);
    }

    private void ComboBox_CustomConfigName_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var index = ComboBox_CustomConfigName.SelectedIndex;
        if (index == -1)
            return;

        ApplyRuleByIndex(index);
    }
}
