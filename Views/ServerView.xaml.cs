﻿using BF1.ServerAdminTools.Models;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.Data;
using BF1.ServerAdminTools.Features.Utils;
using BF1.ServerAdminTools.Features.API;
using BF1.ServerAdminTools.Features.API.RespJson;

using CommunityToolkit.Mvvm.Input;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// ServerView.xaml 的交互逻辑
/// </summary>
public partial class ServerView : UserControl
{
    public class GameserversItem
    {
        public string gameId { get; set; }
        public string guid { get; set; }
        public string protocolVersion { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public bool ranked { get; set; }
        public int queryCurrent { get; set; }
        public int queryMax { get; set; }
        public int soldierCurrent { get; set; }
        public int soldierMax { get; set; }
        public int spectatorCurrent { get; set; }
        public int spectatorMax { get; set; }
        public string mapName { get; set; }
        public string mapNamePretty { get; set; }
        public string mapMode { get; set; }
        public string mapModePretty { get; set; }
        public string mapImageUrl { get; set; }
        public string game { get; set; }
        public string platform { get; set; }
        public bool passwordProtected { get; set; }
        public string ip { get; set; }
        public string pingSiteAlias { get; set; }
        public bool isFavorite { get; set; }
        public string favoriteStar { get; set; }
        public bool custom { get; set; }
        public string preset { get; set; }
        public int tickRate { get; set; }
        public string serverType { get; set; }
        public string experience { get; set; }
        public string officialExperienceId { get; set; }
    }

    public ServerModel ServerModel { get; set; } = new();
    public ObservableCollection<GameserversItem> ServersItems { get; set; } = new();

    public RelayCommand QueryServerCommand { get; private set; }
    public RelayCommand<string> ServerInfoCommand { get; private set; }

    public ServerView()
    {
        InitializeComponent();
        this.DataContext = this;

        QueryServerCommand = new RelayCommand(QueryServer);
        ServerInfoCommand = new RelayCommand<string>(ServerInfo);

        ServerModel.LoadingVisibility = Visibility.Collapsed;
    }

    private async void QueryServer()
    {
        AudioUtil.ClickSound();

        if (string.IsNullOrEmpty(ServerModel.ServerName))
        {
            NotifierHelper.Show(NotifierType.Warning, $"请输入正确的服务器名称");
            return;
        }

        if (!string.IsNullOrEmpty(Globals.Remid) && !string.IsNullOrEmpty(Globals.Sid))
        {
            ServersItems.Clear();
            ServerModel.LoadingVisibility = Visibility.Visible;

            ServerModel.ServerName = ServerModel.ServerName.Trim();

            NotifierHelper.Show(NotifierType.Information, $"正在查询服务器 {ServerModel.ServerName} 数据中...");

            var result = await BF1API.SearchServers(ServerModel.ServerName);
            if (result.IsSuccess)
            {
                var searchServers = JsonUtil.JsonDese<SearchServers>(result.Message);
                var gameservers = searchServers.result.gameservers.OrderByDescending(c => c.slots.Soldier.current);

                foreach (var item in gameservers)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        ServersItems.Add(new()
                        {
                            gameId = item.gameId,
                            name = item.name,
                            description = item.description,
                            mapModePretty = ChsUtil.ToSimplifiedChinese(item.mapModePretty),
                            mapNamePretty = ChsUtil.ToSimplifiedChinese(item.mapNamePretty),
                            mapImageUrl = PlayerUtil.GetTempImagePath(item.mapImageUrl, "maps"),
                            queryCurrent = item.slots.Queue.current,
                            soldierCurrent = item.slots.Soldier.current,
                            soldierMax = item.slots.Soldier.max,
                            spectatorCurrent = item.slots.Spectator.current,
                            platform = new Random().Next(25, 45).ToString(),
                            favoriteStar = item.isFavorite ? "\xe634" : ""
                        });
                    }));
                }

                NotifierHelper.Show(NotifierType.Success, $"服务器 {ServerModel.ServerName} 数据查询成功  |  耗时: {result.ExecTime:0.00} 秒");
            }
            else
            {
                NotifierHelper.Show(NotifierType.Error, $"服务器 {ServerModel.ServerName} 数据查询失败  |  耗时: {result.ExecTime:0.00} 秒");
            }

            ServerModel.LoadingVisibility = Visibility.Collapsed;
        }
        else
        {
            NotifierHelper.Show(NotifierType.Error, "操作失败，玩家Remid或Sid为空");
        }
    }

    private void ServerInfo(string gameid)
    {

    }
}
