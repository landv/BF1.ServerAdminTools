using BF1.ServerAdminTools.Models;
using BF1.ServerAdminTools.Models.Score;
using BF1.ServerAdminTools.Windows;
using BF1.ServerAdminTools.Extension;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.API;
using BF1.ServerAdminTools.Features.Core;
using BF1.ServerAdminTools.Features.Data;
using BF1.ServerAdminTools.Features.Utils;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// ScoreView.xaml 的交互逻辑
/// </summary>
public partial class ScoreView : UserControl
{
    private List<PlayerData> PlayerList_All = new();
    private List<PlayerData> PlayerList_Team0 = new();
    private List<PlayerData> PlayerList_Team1 = new();
    private List<PlayerData> PlayerList_Team2 = new();

    public static List<PlayerData> PlayerDatas_Team1 = new();
    public static List<PlayerData> PlayerDatas_Team2 = new();

    // 正在执行踢人请求的玩家列表，保留指定时间秒数
    private List<BreakRuleInfo> Kicking_PlayerList = new();

    public ServerInfoModel ServerInfoModel { get; set; } = new();
    public PlayerOtherModel PlayerOtherModel { get; set; } = new();

    /// <summary>
    /// 绑定UI队伍1动态数据集合，用于更新DataGrid
    /// </summary>
    public ObservableCollection<PlayerListModel> DataGrid_PlayerList_Team1 { get; set; } = new();
    /// <summary>
    /// 绑定UI队伍2动态数据集合，用于更新DataGrid
    /// </summary>
    public ObservableCollection<PlayerListModel> DataGrid_PlayerList_Team2 { get; set; } = new();

    /// <summary>
    /// 最大玩家数量
    /// </summary>
    private const int MaxPlayer = 74;

    private struct StatisticData
    {
        public int MaxPlayerCount;
        public int PlayerCount;
        public int Rank150PlayerCount;

        public int AllKillCount;
        public int AllDeadCount;
    }
    private StatisticData _statisticData_Team1;
    private StatisticData _statisticData_Team2;

    private struct ServerInfo
    {
        public long OffsetTemp;

        public string Name;
        public long GameID;
        public float Time;

        public string GameMode;
        public string MapName;

        public int MaxScore;

        public int Team1Score;
        public int Team2Score;

        public int Team1Kill;
        public int Team2Kill;

        public int Team1Flag;
        public int Team2Flag;

        public string Team1Img;
        public string Team2Img;
    }
    private ServerInfo _serverInfo;

    private struct DataGridSelcContent
    {
        public bool IsOK;
        public int TeamID;
        public int Rank;
        public string Name;
        public long PersonaId;
    }
    private DataGridSelcContent _dataGridSelcContent;

    ///////////////////////////////////////////////////////

    public ScoreView()
    {
        InitializeComponent();
        this.DataContext = this;

        var thread0 = new Thread(UpdateServerInfo)
        {
            IsBackground = true
        };
        thread0.Start();

        var thread1 = new Thread(UpdatePlayerList)
        {
            IsBackground = true
        };
        thread1.Start();
    }

    /// <summary>
    /// 更新服务器信息
    /// </summary>
    private void UpdateServerInfo()
    {
        while (true)
        {
            //////////////////////////////// 服务器数据获取 ////////////////////////////////

            // 服务器名称
            _serverInfo.Name = Memory.ReadString(Memory.GetBaseAddress() + Offsets.ServerName_Offset, Offsets.ServerName, 64);
            _serverInfo.Name = string.IsNullOrEmpty(_serverInfo.Name) ? "未知" : _serverInfo.Name;
            // 服务器地图名称
            _serverInfo.MapName = Memory.ReadString(Offsets.OFFSET_CLIENTGAMECONTEXT, Offsets.ServerMapName, 64);
            _serverInfo.MapName = string.IsNullOrEmpty(_serverInfo.MapName) ? "未知" : _serverInfo.MapName;
            // 服务器游戏模式
            _serverInfo.GameMode = Memory.ReadString(Memory.GetBaseAddress() + Offsets.ServerID_Offset, Offsets.ServerGameMode, 64);

            // 服务器时间
            _serverInfo.Time = Memory.Read<float>(Memory.GetBaseAddress() + Offsets.ServerTime_Offset, Offsets.ServerTime);

            //////////////////////////////// 服务器数据整理 ////////////////////////////////

            ServerInfoModel.ServerName = _serverInfo.Name;
            ServerInfoModel.ServerGameID = _serverInfo.GameID.ToString();
            ServerInfoModel.ServerMapName = PlayerUtil.GetMapChsName(_serverInfo.MapName);
            ServerInfoModel.ServerMapImg = PlayerUtil.GetMapPrevImage(_serverInfo.MapName);

            _serverInfo.GameMode = PlayerUtil.GetGameMode(_serverInfo.GameMode);
            if (_serverInfo.MapName == "未知" || ServerInfoModel.ServerMapName == "大厅菜单")
                ServerInfoModel.ServerGameMode = "未知";
            else
                ServerInfoModel.ServerGameMode = _serverInfo.GameMode;

            PlayerUtil.GetTeamImage(_serverInfo.MapName, out _serverInfo.Team1Img, out _serverInfo.Team2Img);
            ServerInfoModel.Team1Img = _serverInfo.Team1Img;
            ServerInfoModel.Team2Img = _serverInfo.Team2Img;

            ServerInfoModel.ServerTime = PlayerUtil.SecondsToMMSS(_serverInfo.Time);

            // 当服务器模式为征服时，下列数据才有效
            if (_serverInfo.GameMode == "征服")
            {
                // 比分数据地址
                _serverInfo.OffsetTemp = Memory.Read<long>(Memory.GetBaseAddress() + Offsets.ServerScore_Offset, Offsets.ServerScoreTeam);
                // 最大比分
                _serverInfo.MaxScore = Memory.Read<int>(_serverInfo.OffsetTemp + 0x120);
                // 队伍1、队伍2分数
                _serverInfo.Team1Score = Memory.Read<int>(_serverInfo.OffsetTemp + 0xE8);
                _serverInfo.Team2Score = Memory.Read<int>(_serverInfo.OffsetTemp + 0x118);
                // 队伍1、队伍2从击杀获取得分
                _serverInfo.Team1Kill = Memory.Read<int>(_serverInfo.OffsetTemp + 0x230);
                _serverInfo.Team2Kill = Memory.Read<int>(_serverInfo.OffsetTemp + 0x238);
                // 队伍1、队伍2从旗帜获取得分
                _serverInfo.Team1Flag = Memory.Read<int>(_serverInfo.OffsetTemp + 0x250);
                _serverInfo.Team2Flag = Memory.Read<int>(_serverInfo.OffsetTemp + 0x258);

                //////////////////////////////// 修正服务器得分数据 ////////////////////////////////

                _serverInfo.Team1Score = PlayerUtil.FixedServerScore(_serverInfo.Team1Score);
                _serverInfo.Team2Score = PlayerUtil.FixedServerScore(_serverInfo.Team2Score);

                if (_serverInfo.MaxScore != 0)
                {
                    var scale = _serverInfo.MaxScore / 1000.0f;
                    ServerInfoModel.Team1ScoreWidth = PlayerUtil.FixedServerScore(_serverInfo.Team1Score / (8 * scale));
                    ServerInfoModel.Team2ScoreWidth = PlayerUtil.FixedServerScore(_serverInfo.Team2Score / (8 * scale));
                }
                else
                {
                    ServerInfoModel.Team1ScoreWidth = 0;
                    ServerInfoModel.Team2ScoreWidth = 0;
                }

                ServerInfoModel.Team1Score = _serverInfo.Team1Score.ToString();
                ServerInfoModel.Team2Score = _serverInfo.Team2Score.ToString();

                ServerInfoModel.Team1Flag = PlayerUtil.FixedServerScore(_serverInfo.Team1Flag);
                ServerInfoModel.Team1Kill = PlayerUtil.FixedServerScore(_serverInfo.Team1Kill);

                ServerInfoModel.Team2Flag = PlayerUtil.FixedServerScore(_serverInfo.Team2Flag);
                ServerInfoModel.Team2Kill = PlayerUtil.FixedServerScore(_serverInfo.Team2Kill);
            }

            ////////////////////////////////////////////////////////////////////////////////

            // 如果玩家没有进入服务器，要进行一些数据清理
            if (ServerInfoModel.ServerMapName == "大厅菜单")
            {
                // 清理服务器ID（GameID）
                _serverInfo.GameID = 0;
                Globals.GameId = string.Empty;

                Globals.Server_AdminList_PID.Clear();
                Globals.Server_AdminList_Name.Clear();
                Globals.Server_VIPList.Clear();
            }
            else
            {
                // 服务器数字ID
                _serverInfo.GameID = Memory.Read<long>(Memory.GetBaseAddress() + Offsets.ServerID_Offset, Offsets.ServerGameID);
                Globals.GameId = _serverInfo.GameID.ToString();
            }

            ////////////////////////////////////////////////////////////////////////////////

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 更新玩家列表
    /// </summary>
    private void UpdatePlayerList()
    {
        while (true)
        {
            //////////////////////////////// 数据初始化 ////////////////////////////////

            PlayerList_All.Clear();
            PlayerList_Team0.Clear();
            PlayerList_Team1.Clear();
            PlayerList_Team2.Clear();

            Globals.Server_SpectatorList.Clear();

            var _weaponSlot = new string[8] { "", "", "", "", "", "", "", "" };

            _statisticData_Team1.MaxPlayerCount = 0;
            _statisticData_Team1.PlayerCount = 0;
            _statisticData_Team1.Rank150PlayerCount = 0;
            _statisticData_Team1.AllKillCount = 0;
            _statisticData_Team1.AllDeadCount = 0;

            _statisticData_Team2.MaxPlayerCount = 0;
            _statisticData_Team2.PlayerCount = 0;
            _statisticData_Team2.Rank150PlayerCount = 0;
            _statisticData_Team2.AllKillCount = 0;
            _statisticData_Team2.AllDeadCount = 0;

            Globals.BreakRuleInfo_PlayerList.Clear();

            //////////////////////////////// 自己数据 ////////////////////////////////

            var _myBaseAddress = Player.GetLocalPlayer();

            var _myTeamId = Memory.Read<int>(_myBaseAddress + 0x1C34);
            PlayerOtherModel.MySelfTeamID = $"队伍ID : {_myTeamId}";

            var _myPlayerName = Memory.ReadString(_myBaseAddress + 0x2156, 64);
            PlayerOtherModel.MySelfName = string.IsNullOrEmpty(_myPlayerName) ? "玩家ID : 未知" : $"玩家ID : {_myPlayerName}";

            //////////////////////////////// 玩家数据 ////////////////////////////////

            for (int i = 0; i < MaxPlayer; i++)
            {
                var _baseAddress = Player.GetPlayerById(i);
                if (!Memory.IsValid(_baseAddress))
                    continue;

                var _mark = Memory.Read<byte>(_baseAddress + 0x1D7C);
                var _teamId = Memory.Read<int>(_baseAddress + 0x1C34);
                var _spectator = Memory.Read<byte>(_baseAddress + 0x1C31);
                var _personaId = Memory.Read<long>(_baseAddress + 0x38);
                var _squadId = Memory.Read<int>(_baseAddress + 0x1E50);
                var _name = Memory.ReadString(_baseAddress + 0x2156, 64);
                if (string.IsNullOrEmpty(_name))
                    continue;

                var _pClientVehicleEntity = Memory.Read<long>(_baseAddress + 0x1D38);
                if (Memory.IsValid(_pClientVehicleEntity))
                {
                    var _pVehicleEntityData = Memory.Read<long>(_pClientVehicleEntity + 0x30);
                    _weaponSlot[0] = Memory.ReadString(_pVehicleEntityData + 0x2F8, new int[] { 0x00 }, 64);

                    for (int j = 1; j < 8; j++)
                    {
                        _weaponSlot[j] = "";
                    }
                }
                else
                {
                    var _pClientSoldierEntity = Memory.Read<long>(_baseAddress + 0x1D48);
                    var _pClientSoldierWeaponComponent = Memory.Read<long>(_pClientSoldierEntity + 0x698);
                    var _m_handler = Memory.Read<long>(_pClientSoldierWeaponComponent + 0x8A8);

                    for (int j = 0; j < 8; j++)
                    {
                        var offset0 = Memory.Read<long>(_m_handler + j * 0x8);

                        offset0 = Memory.Read<long>(offset0 + 0x4A30);
                        offset0 = Memory.Read<long>(offset0 + 0x20);
                        offset0 = Memory.Read<long>(offset0 + 0x38);
                        offset0 = Memory.Read<long>(offset0 + 0x20);

                        _weaponSlot[j] = Memory.ReadString(offset0, 64);
                    }
                }

                var index = PlayerList_All.FindIndex(val => val.Name == _name);
                if (index == -1)
                {
                    PlayerList_All.Add(new PlayerData()
                    {
                        Mark = _mark,
                        TeamId = _teamId,
                        Spectator = _spectator,
                        Clan = PlayerUtil.GetPlayerTargetName(_name, true),
                        Name = PlayerUtil.GetPlayerTargetName(_name, false),
                        PersonaId = _personaId,
                        SquadId = PlayerUtil.GetSquadChsName(_squadId),

                        Rank = 0,
                        Kill = 0,
                        Dead = 0,
                        Score = 0,

                        KD = 0,
                        KPM = 0,

                        WeaponS0 = _weaponSlot[0],
                        WeaponS1 = _weaponSlot[1],
                        WeaponS2 = _weaponSlot[2],
                        WeaponS3 = _weaponSlot[3],
                        WeaponS4 = _weaponSlot[4],
                        WeaponS5 = _weaponSlot[5],
                        WeaponS6 = _weaponSlot[6],
                        WeaponS7 = _weaponSlot[7],
                    });
                }
            }

            //////////////////////////////// 得分板数据 ////////////////////////////////

            var _pClientScoreBA = Memory.Read<long>(Memory.GetBaseAddress() + 0x39EB8D8);
            _pClientScoreBA = Memory.Read<long>(_pClientScoreBA + 0x68);

            for (int i = 0; i < MaxPlayer; i++)
            {
                _pClientScoreBA = Memory.Read<long>(_pClientScoreBA);
                var _pClientScoreOffset = Memory.Read<long>(_pClientScoreBA + 0x10);
                if (!Memory.IsValid(_pClientScoreBA))
                    continue;

                var _mark = Memory.Read<byte>(_pClientScoreOffset + 0x300);
                var _rank = Memory.Read<int>(_pClientScoreOffset + 0x304);
                if (_rank == 0)
                    continue;
                var _kill = Memory.Read<int>(_pClientScoreOffset + 0x308);
                var _dead = Memory.Read<int>(_pClientScoreOffset + 0x30C);
                var _score = Memory.Read<int>(_pClientScoreOffset + 0x314);

                var index = PlayerList_All.FindIndex(val => val.Mark == _mark);
                if (index != -1)
                {
                    PlayerList_All[index].Rank = _rank;
                    PlayerList_All[index].Kill = _kill;
                    PlayerList_All[index].Dead = _dead;
                    PlayerList_All[index].Score = _score;
                    PlayerList_All[index].KD = PlayerUtil.GetPlayerKD(_kill, _dead);
                    PlayerList_All[index].KPM = PlayerUtil.GetPlayerKPM(_kill, PlayerUtil.SecondsToMinute(_serverInfo.Time));
                }
            }

            //////////////////////////////// 队伍数据整理 ////////////////////////////////

            foreach (var item in PlayerList_All)
            {
                item.Admin = PlayerUtil.CheckAdminVIP(item.PersonaId.ToString(), Globals.Server_AdminList_PID);
                item.VIP = PlayerUtil.CheckAdminVIP(item.PersonaId.ToString(), Globals.Server_VIPList);

                switch (item.TeamId)
                {
                    case 0:
                        PlayerList_Team0.Add(item);
                        break;
                    case 1:
                        PlayerList_Team1.Add(item);
                        // 检查队伍1违规玩家
                        CheckTeam1PlayerIsBreakRule(item);
                        break;
                    case 2:
                        PlayerList_Team2.Add(item);
                        // 检查队伍2违规玩家
                        CheckTeam2PlayerIsBreakRule(item);
                        break;
                }
            }

            // 观战玩家信息
            foreach (var item in PlayerList_Team0)
            {
                Globals.Server_SpectatorList.Add(new SpectatorInfo()
                {
                    Name = item.Name,
                    PersonaId = item.PersonaId.ToString(),
                });
            }

            // 队伍1数据统计
            foreach (var item in PlayerList_Team1)
            {
                // 统计当前服务器玩家数量
                if (item.Rank != 0)
                {
                    _statisticData_Team1.MaxPlayerCount++;
                }

                // 统计当前服务器存活玩家数量
                if (item.WeaponS0 != "" ||
                    item.WeaponS1 != "" ||
                    item.WeaponS2 != "" ||
                    item.WeaponS3 != "" ||
                    item.WeaponS4 != "" ||
                    item.WeaponS5 != "" ||
                    item.WeaponS6 != "" ||
                    item.WeaponS7 != "")
                {
                    _statisticData_Team1.PlayerCount++;
                }

                // 统计当前服务器150级玩家数量
                if (item.Rank == 150)
                {
                    _statisticData_Team1.Rank150PlayerCount++;
                }

                // 总击杀数统计
                _statisticData_Team1.AllKillCount += item.Kill;
                // 总死亡数统计
                _statisticData_Team1.AllDeadCount += item.Dead;
            }

            // 队伍2数据统计
            foreach (var item in PlayerList_Team2)
            {
                // 统计当前服务器玩家数量
                if (item.Rank != 0)
                {
                    _statisticData_Team2.MaxPlayerCount++;
                }

                // 统计当前服务器存活玩家数量
                if (item.WeaponS0 != "" ||
                    item.WeaponS1 != "" ||
                    item.WeaponS2 != "" ||
                    item.WeaponS3 != "" ||
                    item.WeaponS4 != "" ||
                    item.WeaponS5 != "" ||
                    item.WeaponS6 != "" ||
                    item.WeaponS7 != "")
                {
                    _statisticData_Team2.PlayerCount++;
                }

                // 统计当前服务器150级玩家数量
                if (item.Rank == 150)
                {
                    _statisticData_Team2.Rank150PlayerCount++;
                }

                // 总击杀数统计
                _statisticData_Team2.AllKillCount += item.Kill;
                // 总死亡数统计
                _statisticData_Team2.AllDeadCount += item.Dead;
            }

            // 是否显示中文武器名称
            if (Globals.IsShowCHSWeaponName)
            {
                for (int i = 0; i < PlayerList_Team1.Count; i++)
                {
                    PlayerList_Team1[i].WeaponS0 = PlayerUtil.GetWeaponChsName(PlayerList_Team1[i].WeaponS0);
                    PlayerList_Team1[i].WeaponS1 = PlayerUtil.GetWeaponChsName(PlayerList_Team1[i].WeaponS1);
                    PlayerList_Team1[i].WeaponS2 = PlayerUtil.GetWeaponChsName(PlayerList_Team1[i].WeaponS2);
                    PlayerList_Team1[i].WeaponS3 = PlayerUtil.GetWeaponChsName(PlayerList_Team1[i].WeaponS3);
                    PlayerList_Team1[i].WeaponS4 = PlayerUtil.GetWeaponChsName(PlayerList_Team1[i].WeaponS4);
                    PlayerList_Team1[i].WeaponS5 = PlayerUtil.GetWeaponChsName(PlayerList_Team1[i].WeaponS5);
                    PlayerList_Team1[i].WeaponS6 = PlayerUtil.GetWeaponChsName(PlayerList_Team1[i].WeaponS6);
                    PlayerList_Team1[i].WeaponS7 = PlayerUtil.GetWeaponChsName(PlayerList_Team1[i].WeaponS7);
                }

                for (int i = 0; i < PlayerList_Team2.Count; i++)
                {
                    PlayerList_Team2[i].WeaponS0 = PlayerUtil.GetWeaponChsName(PlayerList_Team2[i].WeaponS0);
                    PlayerList_Team2[i].WeaponS1 = PlayerUtil.GetWeaponChsName(PlayerList_Team2[i].WeaponS1);
                    PlayerList_Team2[i].WeaponS2 = PlayerUtil.GetWeaponChsName(PlayerList_Team2[i].WeaponS2);
                    PlayerList_Team2[i].WeaponS3 = PlayerUtil.GetWeaponChsName(PlayerList_Team2[i].WeaponS3);
                    PlayerList_Team2[i].WeaponS4 = PlayerUtil.GetWeaponChsName(PlayerList_Team2[i].WeaponS4);
                    PlayerList_Team2[i].WeaponS5 = PlayerUtil.GetWeaponChsName(PlayerList_Team2[i].WeaponS5);
                    PlayerList_Team2[i].WeaponS6 = PlayerUtil.GetWeaponChsName(PlayerList_Team2[i].WeaponS6);
                    PlayerList_Team2[i].WeaponS7 = PlayerUtil.GetWeaponChsName(PlayerList_Team2[i].WeaponS7);
                }
            }

            //////////////////////////////// 统计信息数据 ////////////////////////////////

            ServerInfoModel.Team1Info = $"已部署/队伍1人数 : {_statisticData_Team1.PlayerCount} / {_statisticData_Team1.MaxPlayerCount}  |  150等级人数 : {_statisticData_Team1.Rank150PlayerCount}  |  总击杀数 : {_statisticData_Team1.AllKillCount}  |  总死亡数 : {_statisticData_Team1.AllDeadCount}";
            ServerInfoModel.Team2Info = $"已部署/队伍2人数 : {_statisticData_Team2.PlayerCount} / {_statisticData_Team2.MaxPlayerCount}  |  150等级人数 : {_statisticData_Team2.Rank150PlayerCount}  |  总击杀数 : {_statisticData_Team2.AllKillCount}  |  总死亡数 : {_statisticData_Team2.AllDeadCount}";

            PlayerOtherModel.ServerPlayerCountInfo = $"服务器总人数 : {_statisticData_Team1.MaxPlayerCount + _statisticData_Team2.MaxPlayerCount}";

            ////////////////////////////////////////////////////////////////////////////////

            this.Dispatcher.Invoke(() =>
            {
                UpdateDataGridTeam1();
                UpdateDataGridTeam2();

                DataGrid_PlayerList_Team1.Sort();
                DataGrid_PlayerList_Team2.Sort();
            });

            ////////////////////////////////////////////////////////////////////////////////

            // 自动踢出违规玩家
            AutoKickBreakPlayer();

            ////////////////////////////////////////////////////////////////////////////////

            // 检测换边玩家
            CheckPlayerChangeTeam();

            ////////////////////////////////////////////////////////////////////////////////

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 动态更新 DataGrid 队伍1
    /// </summary>
    private void UpdateDataGridTeam1()
    {
        if (PlayerList_Team1.Count == 0 && DataGrid_PlayerList_Team1.Count != 0)
        {
            DataGrid_PlayerList_Team1.Clear();
        }

        if (PlayerList_Team1.Count != 0)
        {
            // 更新DataGrid中现有的玩家数据，并把DataGrid中已经不在服务器的玩家清除
            for (int i = 0; i < DataGrid_PlayerList_Team1.Count; i++)
            {
                int index = PlayerList_Team1.FindIndex(val => val.Name == DataGrid_PlayerList_Team1[i].Name);
                if (index != -1)
                {
                    DataGrid_PlayerList_Team1[i].Rank = PlayerList_Team1[index].Rank;
                    DataGrid_PlayerList_Team1[i].Clan = PlayerList_Team1[index].Clan;
                    DataGrid_PlayerList_Team1[i].Admin = PlayerList_Team1[index].Admin;
                    DataGrid_PlayerList_Team1[i].VIP = PlayerList_Team1[index].VIP;
                    DataGrid_PlayerList_Team1[i].SquadId = PlayerList_Team1[index].SquadId;
                    DataGrid_PlayerList_Team1[i].Kill = PlayerList_Team1[index].Kill;
                    DataGrid_PlayerList_Team1[i].Dead = PlayerList_Team1[index].Dead;
                    DataGrid_PlayerList_Team1[i].KD = PlayerList_Team1[index].KD.ToString("0.00");
                    DataGrid_PlayerList_Team1[i].KPM = PlayerList_Team1[index].KPM.ToString("0.00");
                    DataGrid_PlayerList_Team1[i].Score = PlayerList_Team1[index].Score;
                    DataGrid_PlayerList_Team1[i].WeaponS0 = PlayerList_Team1[index].WeaponS0;
                    DataGrid_PlayerList_Team1[i].WeaponS1 = PlayerList_Team1[index].WeaponS1;
                    DataGrid_PlayerList_Team1[i].WeaponS2 = PlayerList_Team1[index].WeaponS2;
                    DataGrid_PlayerList_Team1[i].WeaponS3 = PlayerList_Team1[index].WeaponS3;
                    DataGrid_PlayerList_Team1[i].WeaponS4 = PlayerList_Team1[index].WeaponS4;
                    DataGrid_PlayerList_Team1[i].WeaponS5 = PlayerList_Team1[index].WeaponS5;
                    DataGrid_PlayerList_Team1[i].WeaponS6 = PlayerList_Team1[index].WeaponS6;
                    DataGrid_PlayerList_Team1[i].WeaponS7 = PlayerList_Team1[index].WeaponS7;
                }
                else
                {
                    DataGrid_PlayerList_Team1.RemoveAt(i);
                }
            }

            // 增加DataGrid没有的玩家数据
            for (int i = 0; i < PlayerList_Team1.Count; i++)
            {
                int index = DataGrid_PlayerList_Team1.ToList().FindIndex(val => val.Name == PlayerList_Team1[i].Name);
                if (index == -1)
                {
                    DataGrid_PlayerList_Team1.Add(new PlayerListModel()
                    {
                        Rank = PlayerList_Team1[i].Rank,
                        Clan = PlayerList_Team1[i].Clan,
                        Name = PlayerList_Team1[i].Name,
                        PersonaId = PlayerList_Team1[i].PersonaId,
                        Admin = PlayerList_Team1[i].Admin,
                        VIP = PlayerList_Team1[i].VIP,
                        SquadId = PlayerList_Team1[i].SquadId,
                        Kill = PlayerList_Team1[i].Kill,
                        Dead = PlayerList_Team1[i].Dead,
                        KD = PlayerList_Team1[i].KD.ToString("0.00"),
                        KPM = PlayerList_Team1[i].KPM.ToString("0.00"),
                        Score = PlayerList_Team1[i].Score,
                        WeaponS0 = PlayerList_Team1[i].WeaponS0,
                        WeaponS1 = PlayerList_Team1[i].WeaponS1,
                        WeaponS2 = PlayerList_Team1[i].WeaponS2,
                        WeaponS3 = PlayerList_Team1[i].WeaponS3,
                        WeaponS4 = PlayerList_Team1[i].WeaponS4,
                        WeaponS5 = PlayerList_Team1[i].WeaponS5,
                        WeaponS6 = PlayerList_Team1[i].WeaponS6,
                        WeaponS7 = PlayerList_Team1[i].WeaponS7
                    });

                }
            }

            // 修正序号
            for (int i = 0; i < DataGrid_PlayerList_Team1.Count; i++)
            {
                DataGrid_PlayerList_Team1[i].Index = i + 1;
            }
        }
    }

    /// <summary>
    /// 动态更新 DataGrid 队伍2
    /// </summary>
    private void UpdateDataGridTeam2()
    {
        if (PlayerList_Team2.Count == 0 && DataGrid_PlayerList_Team2.Count != 0)
        {
            DataGrid_PlayerList_Team2.Clear();
        }

        if (PlayerList_Team2.Count != 0)
        {
            // 更新DataGrid中现有的玩家数据，并把DataGrid中已经不在服务器的玩家清除
            for (int i = 0; i < DataGrid_PlayerList_Team2.Count; i++)
            {
                int index = PlayerList_Team2.FindIndex(val => val.Name == DataGrid_PlayerList_Team2[i].Name);
                if (index != -1)
                {
                    DataGrid_PlayerList_Team2[i].Rank = PlayerList_Team2[index].Rank;
                    DataGrid_PlayerList_Team2[i].Clan = PlayerList_Team2[index].Clan;
                    DataGrid_PlayerList_Team2[i].Admin = PlayerList_Team2[index].Admin;
                    DataGrid_PlayerList_Team2[i].VIP = PlayerList_Team2[index].VIP;
                    DataGrid_PlayerList_Team2[i].SquadId = PlayerList_Team2[index].SquadId;
                    DataGrid_PlayerList_Team2[i].Kill = PlayerList_Team2[index].Kill;
                    DataGrid_PlayerList_Team2[i].Dead = PlayerList_Team2[index].Dead;
                    DataGrid_PlayerList_Team2[i].KD = PlayerList_Team2[index].KD.ToString("0.00");
                    DataGrid_PlayerList_Team2[i].KPM = PlayerList_Team2[index].KPM.ToString("0.00");
                    DataGrid_PlayerList_Team2[i].Score = PlayerList_Team2[index].Score;
                    DataGrid_PlayerList_Team2[i].WeaponS0 = PlayerList_Team2[index].WeaponS0;
                    DataGrid_PlayerList_Team2[i].WeaponS1 = PlayerList_Team2[index].WeaponS1;
                    DataGrid_PlayerList_Team2[i].WeaponS2 = PlayerList_Team2[index].WeaponS2;
                    DataGrid_PlayerList_Team2[i].WeaponS3 = PlayerList_Team2[index].WeaponS3;
                    DataGrid_PlayerList_Team2[i].WeaponS4 = PlayerList_Team2[index].WeaponS4;
                    DataGrid_PlayerList_Team2[i].WeaponS5 = PlayerList_Team2[index].WeaponS5;
                    DataGrid_PlayerList_Team2[i].WeaponS6 = PlayerList_Team2[index].WeaponS6;
                    DataGrid_PlayerList_Team2[i].WeaponS7 = PlayerList_Team2[index].WeaponS7;
                }
                else
                {
                    DataGrid_PlayerList_Team2.RemoveAt(i);
                }
            }

            // 增加DataGrid没有的玩家数据
            for (int i = 0; i < PlayerList_Team2.Count; i++)
            {
                int index = DataGrid_PlayerList_Team2.ToList().FindIndex(val => val.Name == PlayerList_Team2[i].Name);
                if (index == -1)
                {
                    DataGrid_PlayerList_Team2.Add(new PlayerListModel()
                    {
                        Rank = PlayerList_Team2[i].Rank,
                        Clan = PlayerList_Team2[i].Clan,
                        Name = PlayerList_Team2[i].Name,
                        PersonaId = PlayerList_Team2[i].PersonaId,
                        Admin = PlayerList_Team2[i].Admin,
                        VIP = PlayerList_Team2[i].VIP,
                        SquadId = PlayerList_Team2[i].SquadId,
                        Kill = PlayerList_Team2[i].Kill,
                        Dead = PlayerList_Team2[i].Dead,
                        KD = PlayerList_Team2[i].KD.ToString("0.00"),
                        KPM = PlayerList_Team2[i].KPM.ToString("0.00"),
                        Score = PlayerList_Team2[i].Score,
                        WeaponS0 = PlayerList_Team2[i].WeaponS0,
                        WeaponS1 = PlayerList_Team2[i].WeaponS1,
                        WeaponS2 = PlayerList_Team2[i].WeaponS2,
                        WeaponS3 = PlayerList_Team2[i].WeaponS3,
                        WeaponS4 = PlayerList_Team2[i].WeaponS4,
                        WeaponS5 = PlayerList_Team2[i].WeaponS5,
                        WeaponS6 = PlayerList_Team2[i].WeaponS6,
                        WeaponS7 = PlayerList_Team2[i].WeaponS7
                    });
                }
            }

            // 修正序号
            for (int i = 0; i < DataGrid_PlayerList_Team2.Count; i++)
            {
                DataGrid_PlayerList_Team2[i].Index = i + 1;
            }
        }
    }

    #region 检查违规
    /// <summary>
    /// 检查队伍1玩家是否违规
    /// </summary>
    /// <param name="playerData"></param>
    private void CheckTeam1PlayerIsBreakRule(PlayerData playerData)
    {
        int index = Globals.BreakRuleInfo_PlayerList.FindIndex(val => val.PersonaId == playerData.PersonaId);
        if (index == -1)
        {
            // 限制玩家击杀
            if (playerData.Kill > ServerRule.Team1.MaxKill && ServerRule.Team1.MaxKill != 0)
            {
                Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                {
                    Name = playerData.Name,
                    PersonaId = playerData.PersonaId,
                    Reason = $"Kill Limit {ServerRule.Team1.MaxKill:0}"
                });

                return;
            }

            // 计算玩家KD最低击杀数
            if (playerData.Kill > ServerRule.Team1.KDFlag && ServerRule.Team1.KDFlag != 0)
            {
                // 限制玩家KD
                if (playerData.KD > ServerRule.Team1.MaxKD && ServerRule.Team1.MaxKD != 0.00f)
                {
                    Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                    {
                        Name = playerData.Name,
                        PersonaId = playerData.PersonaId,
                        Reason = $"KD Limit {ServerRule.Team1.MaxKD:0.00}"
                    });
                }

                return;
            }

            // 计算玩家KPM比条件
            if (playerData.Kill > ServerRule.Team1.KPMFlag && ServerRule.Team1.KPMFlag != 0)
            {
                // 限制玩家KPM
                if (playerData.KPM > ServerRule.Team1.MaxKPM && ServerRule.Team1.MaxKPM != 0.00f)
                {
                    Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                    {
                        Name = playerData.Name,
                        PersonaId = playerData.PersonaId,
                        Reason = $"KPM Limit {ServerRule.Team1.MaxKPM:0.00}"
                    });
                }

                return;
            }

            // 限制玩家最低等级
            if (playerData.Rank < ServerRule.Team1.MinRank && ServerRule.Team1.MinRank != 0 && playerData.Rank != 0)
            {
                Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                {
                    Name = playerData.Name,
                    PersonaId = playerData.PersonaId,
                    Reason = $"Min Rank Limit {ServerRule.Team1.MinRank:0}"
                });

                return;
            }

            // 限制玩家最高等级
            if (playerData.Rank > ServerRule.Team1.MaxRank && ServerRule.Team1.MaxRank != 0 && playerData.Rank != 0)
            {
                Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                {
                    Name = playerData.Name,
                    PersonaId = playerData.PersonaId,
                    Reason = $"Max Rank Limit {ServerRule.Team1.MaxRank:0}"
                });

                return;
            }

            // 从武器规则里遍历限制武器名称
            for (int i = 0; i < Globals.Custom_WeaponList_Team1.Count; i++)
            {
                var item = Globals.Custom_WeaponList_Team1[i];

                // K 弹
                if (item == "_KBullet")
                {
                    if (playerData.WeaponS0.Contains("_KBullet") ||
                        playerData.WeaponS1.Contains("_KBullet") ||
                        playerData.WeaponS2.Contains("_KBullet") ||
                        playerData.WeaponS3.Contains("_KBullet") ||
                        playerData.WeaponS4.Contains("_KBullet") ||
                        playerData.WeaponS5.Contains("_KBullet") ||
                        playerData.WeaponS6.Contains("_KBullet") ||
                        playerData.WeaponS7.Contains("_KBullet"))
                    {
                        Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                        {
                            Name = playerData.Name,
                            PersonaId = playerData.PersonaId,
                            Reason = $"Weapon Limit K Bullet"
                        });

                        return;
                    }
                }

                // 步枪手榴弹（破片）
                if (item == "_RGL_Frag")
                {
                    if (playerData.WeaponS0.Contains("_RGL_Frag") ||
                        playerData.WeaponS1.Contains("_RGL_Frag") ||
                        playerData.WeaponS2.Contains("_RGL_Frag") ||
                        playerData.WeaponS3.Contains("_RGL_Frag") ||
                        playerData.WeaponS4.Contains("_RGL_Frag") ||
                        playerData.WeaponS5.Contains("_RGL_Frag") ||
                        playerData.WeaponS6.Contains("_RGL_Frag") ||
                        playerData.WeaponS7.Contains("_RGL_Frag"))
                    {
                        Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                        {
                            Name = playerData.Name,
                            PersonaId = playerData.PersonaId,
                            Reason = $"Weapon Limit RGL Frag"
                        });

                        return;
                    }
                }

                // 步枪手榴弹（烟雾）
                if (item == "_RGL_Smoke")
                {
                    if (playerData.WeaponS0.Contains("_RGL_Smoke") ||
                        playerData.WeaponS1.Contains("_RGL_Smoke") ||
                        playerData.WeaponS2.Contains("_RGL_Smoke") ||
                        playerData.WeaponS3.Contains("_RGL_Smoke") ||
                        playerData.WeaponS4.Contains("_RGL_Smoke") ||
                        playerData.WeaponS5.Contains("_RGL_Smoke") ||
                        playerData.WeaponS6.Contains("_RGL_Smoke") ||
                        playerData.WeaponS7.Contains("_RGL_Smoke"))
                    {
                        Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                        {
                            Name = playerData.Name,
                            PersonaId = playerData.PersonaId,
                            Reason = $"Weapon Limit RGL Smoke"
                        });

                        return;
                    }
                }

                // 步枪手榴弹（高爆）
                if (item == "_RGL_HE")
                {
                    if (playerData.WeaponS0.Contains("_RGL_HE") ||
                        playerData.WeaponS1.Contains("_RGL_HE") ||
                        playerData.WeaponS2.Contains("_RGL_HE") ||
                        playerData.WeaponS3.Contains("_RGL_HE") ||
                        playerData.WeaponS4.Contains("_RGL_HE") ||
                        playerData.WeaponS5.Contains("_RGL_HE") ||
                        playerData.WeaponS6.Contains("_RGL_HE") ||
                        playerData.WeaponS7.Contains("_RGL_HE"))
                    {
                        Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                        {
                            Name = playerData.Name,
                            PersonaId = playerData.PersonaId,
                            Reason = $"Weapon Limit RGL HE"
                        });

                        return;
                    }
                }

                if (playerData.WeaponS0 == item ||
                    playerData.WeaponS1 == item ||
                    playerData.WeaponS2 == item ||
                    playerData.WeaponS3 == item ||
                    playerData.WeaponS4 == item ||
                    playerData.WeaponS5 == item ||
                    playerData.WeaponS6 == item ||
                    playerData.WeaponS7 == item)
                {
                    Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                    {
                        Name = playerData.Name,
                        PersonaId = playerData.PersonaId,
                        Reason = $"Weapon Limit {PlayerUtil.GetWeaponShortTxt(item)}"
                    });

                    return;
                }
            }

            // 黑名单
            for (int i = 0; i < Globals.Custom_BlackList.Count; i++)
            {
                var item = Globals.Custom_BlackList[i];
                if (playerData.Name == item)
                {
                    Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                    {
                        Name = playerData.Name,
                        PersonaId = playerData.PersonaId,
                        Reason = "Server Black List"
                    });

                    return;
                }
            }
        }
    }

    /// <summary>
    /// 检查队伍2玩家是否违规
    /// </summary>
    /// <param name="playerData"></param>
    private void CheckTeam2PlayerIsBreakRule(PlayerData playerData)
    {
        int index = Globals.BreakRuleInfo_PlayerList.FindIndex(val => val.PersonaId == playerData.PersonaId);
        if (index == -1)
        {
            // 限制玩家击杀
            if (playerData.Kill > ServerRule.Team2.MaxKill && ServerRule.Team2.MaxKill != 0)
            {
                Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                {
                    Name = playerData.Name,
                    PersonaId = playerData.PersonaId,
                    Reason = $"Kill Limit {ServerRule.Team2.MaxKill:0}"
                });

                return;
            }

            // 计算玩家KD最低击杀数
            if (playerData.Kill > ServerRule.Team2.KDFlag && ServerRule.Team2.KDFlag != 0)
            {
                // 限制玩家KD
                if (playerData.KD > ServerRule.Team2.MaxKD && ServerRule.Team2.MaxKD != 0.00f)
                {
                    Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                    {
                        Name = playerData.Name,
                        PersonaId = playerData.PersonaId,
                        Reason = $"KD Limit {ServerRule.Team2.MaxKD:0.00}"
                    });
                }

                return;
            }

            // 计算玩家KPM比条件
            if (playerData.Kill > ServerRule.Team2.KPMFlag && ServerRule.Team2.KPMFlag != 0)
            {
                // 限制玩家KPM
                if (playerData.KPM > ServerRule.Team2.MaxKPM && ServerRule.Team2.MaxKPM != 0.00f)
                {
                    Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                    {
                        Name = playerData.Name,
                        PersonaId = playerData.PersonaId,
                        Reason = $"KPM Limit {ServerRule.Team2.MaxKPM:0.00}"
                    });
                }

                return;
            }

            // 限制玩家最低等级
            if (playerData.Rank < ServerRule.Team2.MinRank && ServerRule.Team2.MinRank != 0 && playerData.Rank != 0)
            {
                Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                {
                    Name = playerData.Name,
                    PersonaId = playerData.PersonaId,
                    Reason = $"Min Rank Limit {ServerRule.Team2.MinRank:0}"
                });

                return;
            }

            // 限制玩家最高等级
            if (playerData.Rank > ServerRule.Team2.MaxRank && ServerRule.Team2.MaxRank != 0 && playerData.Rank != 0)
            {
                Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                {
                    Name = playerData.Name,
                    PersonaId = playerData.PersonaId,
                    Reason = $"Max Rank Limit {ServerRule.Team2.MaxRank:0}"
                });

                return;
            }

            // 从武器规则里遍历限制武器名称
            for (int i = 0; i < Globals.Custom_WeaponList_Team2.Count; i++)
            {
                var item = Globals.Custom_WeaponList_Team2[i];

                // K 弹
                if (item == "_KBullet")
                {
                    if (playerData.WeaponS0.Contains("_KBullet") ||
                        playerData.WeaponS1.Contains("_KBullet") ||
                        playerData.WeaponS2.Contains("_KBullet") ||
                        playerData.WeaponS3.Contains("_KBullet") ||
                        playerData.WeaponS4.Contains("_KBullet") ||
                        playerData.WeaponS5.Contains("_KBullet") ||
                        playerData.WeaponS6.Contains("_KBullet") ||
                        playerData.WeaponS7.Contains("_KBullet"))
                    {
                        Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                        {
                            Name = playerData.Name,
                            PersonaId = playerData.PersonaId,
                            Reason = $"Weapon Limit K Bullet"
                        });

                        return;
                    }
                }

                // 步枪手榴弹（破片）
                if (item == "_RGL_Frag")
                {
                    if (playerData.WeaponS0.Contains("_RGL_Frag") ||
                        playerData.WeaponS1.Contains("_RGL_Frag") ||
                        playerData.WeaponS2.Contains("_RGL_Frag") ||
                        playerData.WeaponS3.Contains("_RGL_Frag") ||
                        playerData.WeaponS4.Contains("_RGL_Frag") ||
                        playerData.WeaponS5.Contains("_RGL_Frag") ||
                        playerData.WeaponS6.Contains("_RGL_Frag") ||
                        playerData.WeaponS7.Contains("_RGL_Frag"))
                    {
                        Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                        {
                            Name = playerData.Name,
                            PersonaId = playerData.PersonaId,
                            Reason = $"Weapon Limit RGL Frag"
                        });

                        return;
                    }
                }

                // 步枪手榴弹（烟雾）
                if (item == "_RGL_Smoke")
                {
                    if (playerData.WeaponS0.Contains("_RGL_Smoke") ||
                        playerData.WeaponS1.Contains("_RGL_Smoke") ||
                        playerData.WeaponS2.Contains("_RGL_Smoke") ||
                        playerData.WeaponS3.Contains("_RGL_Smoke") ||
                        playerData.WeaponS4.Contains("_RGL_Smoke") ||
                        playerData.WeaponS5.Contains("_RGL_Smoke") ||
                        playerData.WeaponS6.Contains("_RGL_Smoke") ||
                        playerData.WeaponS7.Contains("_RGL_Smoke"))
                    {
                        Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                        {
                            Name = playerData.Name,
                            PersonaId = playerData.PersonaId,
                            Reason = $"Weapon Limit RGL Smoke"
                        });

                        return;
                    }
                }

                // 步枪手榴弹（高爆）
                if (item == "_RGL_HE")
                {
                    if (playerData.WeaponS0.Contains("_RGL_HE") ||
                        playerData.WeaponS1.Contains("_RGL_HE") ||
                        playerData.WeaponS2.Contains("_RGL_HE") ||
                        playerData.WeaponS3.Contains("_RGL_HE") ||
                        playerData.WeaponS4.Contains("_RGL_HE") ||
                        playerData.WeaponS5.Contains("_RGL_HE") ||
                        playerData.WeaponS6.Contains("_RGL_HE") ||
                        playerData.WeaponS7.Contains("_RGL_HE"))
                    {
                        Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                        {
                            Name = playerData.Name,
                            PersonaId = playerData.PersonaId,
                            Reason = $"Weapon Limit RGL HE"
                        });

                        return;
                    }
                }

                if (playerData.WeaponS0 == item ||
                    playerData.WeaponS1 == item ||
                    playerData.WeaponS2 == item ||
                    playerData.WeaponS3 == item ||
                    playerData.WeaponS4 == item ||
                    playerData.WeaponS5 == item ||
                    playerData.WeaponS6 == item ||
                    playerData.WeaponS7 == item)
                {
                    Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                    {
                        Name = playerData.Name,
                        PersonaId = playerData.PersonaId,
                        Reason = $"Weapon Limit {PlayerUtil.GetWeaponShortTxt(item)}"
                    });

                    return;
                }
            }

            // 黑名单
            for (int i = 0; i < Globals.Custom_BlackList.Count; i++)
            {
                var item = Globals.Custom_BlackList[i];
                if (playerData.Name == item)
                {
                    Globals.BreakRuleInfo_PlayerList.Add(new BreakRuleInfo
                    {
                        Name = playerData.Name,
                        PersonaId = playerData.PersonaId,
                        Reason = "Server Black List"
                    });

                    return;
                }
            }
        }
    }

    /// <summary>
    /// 自动踢出违规玩家
    /// </summary>
    private void AutoKickBreakPlayer()
    {
        // 自动踢出违规玩家开关
        if (Globals.AutoKickBreakPlayer)
        {
            // 遍历违规玩家列表
            for (int i = 0; i < Globals.BreakRuleInfo_PlayerList.Count; i++)
            {
                var item = Globals.BreakRuleInfo_PlayerList[i];
                item.Flag = -1;

                // 跳过管理员
                if (!Globals.Server_AdminList_PID.Contains(item.PersonaId.ToString()))
                {
                    // 跳过白名单玩家
                    if (!Globals.Custom_WhiteList.Contains(item.Name))
                    {
                        // 先检查踢出玩家是否在 正在踢人 列表中
                        int index = Kicking_PlayerList.FindIndex(var => var.PersonaId == item.PersonaId);
                        if (index == -1)
                        {
                            // 该玩家不在 正在踢人 列表中
                            item.Flag = 0;
                            item.Status = "正在踢人中...";
                            item.Time = DateTime.Now;
                            Kicking_PlayerList.Add(item);

                            // 执行踢人请求
                            AutoKickPlayer(item);
                        }
                    }
                }
            }

            for (int i = 0; i < Kicking_PlayerList.Count; i++)
            {
                if (Kicking_PlayerList.Count != 0)
                {
                    // 如果超过15秒，清空列表
                    if (CoreUtil.DiffSeconds(Kicking_PlayerList[i].Time, DateTime.Now) > 10)
                    {
                        Kicking_PlayerList.Clear();
                        break;
                    }
                }

                if (Kicking_PlayerList.Count != 0 && Kicking_PlayerList[i].Flag == 0)
                {
                    // 如果超过3秒，移除 正在踢人 玩家
                    if (CoreUtil.DiffSeconds(Kicking_PlayerList[i].Time, DateTime.Now) > 3)
                    {
                        Kicking_PlayerList.RemoveAt(i);
                    }
                }

                if (Kicking_PlayerList.Count != 0 && Kicking_PlayerList[i].Flag == 1)
                {
                    // 如果超过10秒，移除 踢出成功 玩家
                    if (CoreUtil.DiffSeconds(Kicking_PlayerList[i].Time, DateTime.Now) > 10)
                    {
                        Kicking_PlayerList.RemoveAt(i);
                    }
                }

                if (Kicking_PlayerList.Count != 0 && Kicking_PlayerList[i].Flag == 2)
                {
                    // 如果超过5秒，移除 踢出失败 玩家
                    if (CoreUtil.DiffSeconds(Kicking_PlayerList[i].Time, DateTime.Now) > 5)
                    {
                        Kicking_PlayerList.RemoveAt(i);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 自动踢出玩家
    /// </summary>
    /// <param name="info"></param>
    private async void AutoKickPlayer(BreakRuleInfo info)
    {
        var result = await BF1API.AdminKickPlayer(info.PersonaId.ToString(), info.Reason);

        if (result.IsSuccess)
        {
            info.Flag = 1;
            info.Status = "踢出成功";
            info.Time = DateTime.Now;

            LogView._dAddKickOKLog(info);
        }
        else
        {
            info.Flag = 2;
            info.Status = "踢出失败 " + result.Message;
            info.Time = DateTime.Now;

            LogView._dAddKickNOLog(info);
        }

    }
    #endregion

    /// <summary>
    /// 手动踢出玩家
    /// </summary>
    /// <param name="reason"></param>
    private async void KickPlayer(string reason)
    {
        if (!string.IsNullOrEmpty(Globals.SessionId))
        {
            if (_dataGridSelcContent.IsOK)
            {
                NotifierHelper.Show(NotiferType.Information, $"正在踢出玩家 {_dataGridSelcContent.Name} 中...");

                var result = await BF1API.AdminKickPlayer(_dataGridSelcContent.PersonaId.ToString(), reason);
                if (result.IsSuccess)
                {
                    NotifierHelper.Show(NotiferType.Success, $"踢出玩家 {_dataGridSelcContent.Name} 成功  |  耗时: {result.ExecTime:0.00} 秒");
                }
                else
                {
                    NotifierHelper.Show(NotiferType.Error, $"踢出玩家 {_dataGridSelcContent.Name} 失败 {result.Message}  |  耗时: {result.ExecTime:0.00} 秒");
                }
            }
            else
            {
                NotifierHelper.Show(NotiferType.Warning, "请选择正确的玩家");
            }
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "请先获取玩家SessionID");
        }
    }

    /// <summary>
    /// 检测换边玩家
    /// </summary>
    private void CheckPlayerChangeTeam()
    {
        // 如果玩家没有进入服务器，不检测换边情况
        if (string.IsNullOrEmpty(Globals.GameId))
            return;

        // 如果双方玩家人数都为0，不检测换边情况
        if (PlayerList_Team1.Count == 0 && PlayerList_Team2.Count == 0)
            return;

        // 第一次初始化
        if (PlayerDatas_Team1.Count == 0 && PlayerDatas_Team2.Count == 0)
        {
            PlayerDatas_Team1 = CopyList(PlayerList_Team1);
            PlayerDatas_Team2 = CopyList(PlayerList_Team2);
            return;
        }

        // 变量保存的队伍1玩家列表
        foreach (var item in PlayerDatas_Team1)
        {
            // 查询这个玩家是否在目前的队伍2中
            int index = PlayerList_Team2.FindIndex(var => var.PersonaId == item.PersonaId);
            if (index != -1)
            {
                LogView._dAddChangeTeamInfo(new ChangeTeamInfo()
                {
                    Rank = item.Rank,
                    Name = item.Name,
                    PersonaId = item.PersonaId,
                    Team1Score = _serverInfo.Team1Score,
                    Team2Score = _serverInfo.Team2Score,
                    Status = "队伍1 ➡ 队伍2",
                    Time = DateTime.Now
                });
                break;
            }
        }

        // 变量保存的队伍2玩家列表
        foreach (var item in PlayerDatas_Team2)
        {
            // 查询这个玩家是否在目前的队伍1中
            int index = PlayerList_Team1.FindIndex(var => var.PersonaId == item.PersonaId);
            if (index != -1)
            {
                LogView._dAddChangeTeamInfo(new ChangeTeamInfo()
                {
                    Rank = item.Rank,
                    Name = item.Name,
                    PersonaId = item.PersonaId,
                    Team1Score = _serverInfo.Team1Score,
                    Team2Score = _serverInfo.Team2Score,
                    Status = "队伍1 ⬅ 队伍2",
                    Time = DateTime.Now
                });
                break;
            }
        }

        // 更新保存的数据
        PlayerDatas_Team1 = CopyList(PlayerList_Team1);
        PlayerDatas_Team2 = CopyList(PlayerList_Team2);
    }

    /// <summary>
    /// List深复制
    /// </summary>
    /// <param name="originalList"></param>
    /// <returns></returns>
    private List<PlayerData> CopyList(List<PlayerData> originalList)
    {
        List<PlayerData> list = new();
        foreach (var item in originalList)
        {
            PlayerData data = new()
            {
                Rank = item.Rank,
                Name = item.Name,
                PersonaId = item.PersonaId
            };
            list.Add(data);
        }
        return list;
    }

    #region 右键菜单事件
    private void MenuItem_Admin_KickPlayer_Custom_Click(object sender, RoutedEventArgs e)
    {
        // 右键菜单 踢出玩家 - 自定义理由
        if (!string.IsNullOrEmpty(Globals.SessionId))
        {
            if (_dataGridSelcContent.IsOK)
            {
                var customKickWindow = new CustomKickWindow(_dataGridSelcContent.Name, _dataGridSelcContent.PersonaId.ToString())
                {
                    Owner = MainWindow.ThisMainWindow
                };
                customKickWindow.ShowDialog();
            }
            else
            {
                NotifierHelper.Show(NotiferType.Warning, "请选择正确的玩家");
            }
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "请先获取玩家SessionID");
        }
    }

    private void MenuItem_Admin_KickPlayer_OffensiveBehavior_Click(object sender, RoutedEventArgs e)
    {
        // 右键菜单 踢出玩家 - 攻击性行为
        KickPlayer("OFFENSIVEBEHAVIOR");
    }

    private void MenuItem_Admin_KickPlayer_Latency_Click(object sender, RoutedEventArgs e)
    {
        // 右键菜单 踢出玩家 - 延迟
        KickPlayer("LATENCY");
    }

    private void MenuItem_Admin_KickPlayer_RuleViolation_Click(object sender, RoutedEventArgs e)
    {
        // 右键菜单 踢出玩家 - 违反规则
        KickPlayer("RULEVIOLATION");
    }

    private void MenuItem_Admin_KickPlayer_General_Click(object sender, RoutedEventArgs e)
    {
        // 右键菜单 踢出玩家 - 其他
        KickPlayer("GENERAL");
    }

    private async void MenuItem_Admin_ChangePlayerTeam_Click(object sender, RoutedEventArgs e)
    {
        // 右键菜单 更换玩家队伍
        if (!string.IsNullOrEmpty(Globals.SessionId))
        {
            if (_dataGridSelcContent.IsOK)
            {
                NotifierHelper.Show(NotiferType.Information, $"正在更换玩家 {_dataGridSelcContent.Name} 队伍中...");

                var result = await BF1API.AdminMovePlayer(_dataGridSelcContent.PersonaId.ToString(), _dataGridSelcContent.TeamID.ToString());
                if (result.IsSuccess)
                {
                    NotifierHelper.Show(NotiferType.Success, $"更换玩家 {_dataGridSelcContent.Name} 队伍成功  |  耗时: {result.ExecTime:0.00} 秒");
                }
                else
                {
                    NotifierHelper.Show(NotiferType.Error, $"更换玩家 {_dataGridSelcContent.Name} 队伍失败 {result.Message}  |  耗时: {result.ExecTime:0.00} 秒");
                }
            }
            else
            {
                NotifierHelper.Show(NotiferType.Warning, "请选择正确的玩家，操作取消");
            }
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "请先获取玩家SessionID后，再执行本操作");
        }
    }

    private void MenuItem_CopyPlayerName_Click(object sender, RoutedEventArgs e)
    {
        if (_dataGridSelcContent.IsOK)
        {
            // 复制玩家ID（无队标）
            Clipboard.SetText(_dataGridSelcContent.Name);
            NotifierHelper.Show(NotiferType.Success, $"复制玩家ID {_dataGridSelcContent.Name} 到剪切板成功");
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "请选择正确的玩家，操作取消");
        }
    }

    private void MenuItem_CopyPlayerName_PID_Click(object sender, RoutedEventArgs e)
    {
        if (_dataGridSelcContent.IsOK)
        {
            // 复制玩家数字ID
            Clipboard.SetText(_dataGridSelcContent.PersonaId.ToString());
            NotifierHelper.Show(NotiferType.Success, $"复制玩家数字ID {_dataGridSelcContent.PersonaId} 到剪切板成功");
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "请选择正确的玩家，操作取消");
        }
    }

    private void MenuItem_QueryPlayerRecord_Click(object sender, RoutedEventArgs e)
    {
        if (_dataGridSelcContent.IsOK)
        {
            // 查询玩家战绩
            var queryRecordWindow = new QueryRecordWindow(_dataGridSelcContent.Name, _dataGridSelcContent.PersonaId, _dataGridSelcContent.Rank);
            queryRecordWindow.Show();
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "请选择正确的玩家，操作取消");
        }
    }

    private void MenuItem_QueryPlayerRecordWeb_BT_Click(object sender, RoutedEventArgs e)
    {
        // 查询玩家战绩（BT）
        if (_dataGridSelcContent.IsOK)
        {
            string playerName = _dataGridSelcContent.Name;

            ProcessUtil.OpenLink(@"https://battlefieldtracker.com/bf1/profile/pc/" + playerName);
            NotifierHelper.Show(NotiferType.Success, $"查询玩家（{_dataGridSelcContent.Name}）战绩成功，请前往浏览器查看");
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "请选择正确的玩家，操作取消");
        }
    }

    private void MenuItem_QueryPlayerRecordWeb_GT_Click(object sender, RoutedEventArgs e)
    {
        // 查询玩家战绩（GT）
        if (_dataGridSelcContent.IsOK)
        {
            string playerName = _dataGridSelcContent.Name;

            ProcessUtil.OpenLink(@"https://gametools.network/stats/pc/name/" + playerName + "?game=bf1");
            NotifierHelper.Show(NotiferType.Success, $"查询玩家（{_dataGridSelcContent.Name}）战绩成功，请前往浏览器查看");
        }
        else
        {
            NotifierHelper.Show(NotiferType.Warning, "请选择正确的玩家，操作取消");
        }
    }

    private void MenuItem_ClearScoreSort_Click(object sender, RoutedEventArgs e)
    {
        // 清理得分板标题排序

        Dispatcher.BeginInvoke(new Action(delegate
        {
            CollectionViewSource.GetDefaultView(DataGrid_Team1.ItemsSource).SortDescriptions.Clear();
            CollectionViewSource.GetDefaultView(DataGrid_Team2.ItemsSource).SortDescriptions.Clear();

            NotifierHelper.Show(NotiferType.Success, "清理得分板标题排序成功（默认为玩家得分从高到低排序）");
        }));
    }

    private void MenuItem_ShowWeaponNameZHCN_Click(object sender, RoutedEventArgs e)
    {
        // 显示中文武器名称（参考）
        var item = sender as MenuItem;
        if (item != null)
        {
            if (item.IsChecked)
            {
                Globals.IsShowCHSWeaponName = true;
                NotifierHelper.Show(NotiferType.Success, $"当前得分板正在显示中文武器名称");
            }
            else
            {
                Globals.IsShowCHSWeaponName = false;
                NotifierHelper.Show(NotiferType.Success, $"当前得分板正在显示英文武器名称");
            }
        }
    }
    #endregion

    #region DataGrid相关方法
    private void DataGrid_Team1_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = DataGrid_Team1.SelectedItem as PlayerListModel;
        if (item != null)
        {
            _dataGridSelcContent.IsOK = true;
            _dataGridSelcContent.TeamID = 1;
            _dataGridSelcContent.Rank = item.Rank;
            _dataGridSelcContent.Name = item.Name;
            _dataGridSelcContent.PersonaId = item.PersonaId;
        }
        else
        {
            _dataGridSelcContent.IsOK = false;
            _dataGridSelcContent.TeamID = -1;
            _dataGridSelcContent.Rank = -1;
            _dataGridSelcContent.Name = string.Empty;
            _dataGridSelcContent.PersonaId = -1;
        }

        Update_DateGrid_Selection();
    }

    private void DataGrid_Team2_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = DataGrid_Team2.SelectedItem as PlayerListModel;
        if (item != null)
        {
            _dataGridSelcContent.IsOK = true;
            _dataGridSelcContent.TeamID = 2;
            _dataGridSelcContent.Rank = item.Rank;
            _dataGridSelcContent.Name = item.Name;
            _dataGridSelcContent.PersonaId = item.PersonaId;
        }
        else
        {
            _dataGridSelcContent.IsOK = false;
            _dataGridSelcContent.TeamID = -1;
            _dataGridSelcContent.Rank = -1;
            _dataGridSelcContent.Name = string.Empty;
            _dataGridSelcContent.PersonaId = -1;
        }

        Update_DateGrid_Selection();
    }

    private void Update_DateGrid_Selection()
    {
        var sb = new StringBuilder();

        if (_dataGridSelcContent.IsOK)
        {
            sb.Append($"玩家ID : {_dataGridSelcContent.Name}");
            sb.Append($"  |  玩家队伍ID : {_dataGridSelcContent.TeamID}");
            sb.Append($"  |  玩家等级 : {_dataGridSelcContent.Rank}");
            sb.Append($"  |  更新时间 : {DateTime.Now}");
        }
        else
        {
            sb.Append($"当前未选中任何玩家");
            sb.Append($"  |  更新时间 : {DateTime.Now}");
        }

        TextBlock_DataGridSelectionContent.Text = sb.ToString();
    }
    #endregion
}
