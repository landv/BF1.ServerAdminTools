using BF1.ServerAdminTools.Models;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;
using BF1.ServerAdminTools.Features.API;
using BF1.ServerAdminTools.Features.Core;
using BF1.ServerAdminTools.Features.Chat;

using Chinese;
using BF1.ServerAdminTools.Features.Client;

namespace BF1.ServerAdminTools;

/// <summary>
/// LoadWindow.xaml 的交互逻辑
/// </summary>
public partial class LoadWindow : Window
{
    public LoadModel LoadModel { get; set; } = new();

    public LoadWindow()
    {
        InitializeComponent();
    }

    private void Window_Auth_Loaded(object sender, RoutedEventArgs e)
    {
        this.DataContext = this;

        Task.Run(() =>
        {
            try
            {
                LoadModel.LoadState = "正在初始化工具，请稍后...";

                // 客户端程序版本号
                LoadModel.VersionInfo = CoreUtil.ClientVersionInfo.ToString();
                // 最后编译时间
                LoadModel.BuildDate = CoreUtil.ClientBuildTime.ToString();

                LoggerHelper.Info("开始初始化程序...");
                LoggerHelper.Info($"当前程序版本号 {CoreUtil.ClientVersionInfo}");
                LoggerHelper.Info($"当前程序最后编译时间 {CoreUtil.ClientBuildTime}");

                ProcessUtil.CloseThirdProcess();

                // 检测目标程序有没有启动
                if (!ProcessUtil.IsBf1Run())
                {
                    LoadModel.LoadState = $"未发现《战地1》游戏进程！程序即将关闭";
                    LoggerHelper.Error("未发现战地1进程");
                    Task.Delay(2000).Wait();

                    this.Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                        return;
                    });
                }

                // 初始化
                if (Memory.Initialize(CoreUtil.TargetAppName))
                {
                    LoggerHelper.Info("战地1内存模块初始化成功");
                }
                else
                {
                    LoadModel.LoadState = $"战地1内存模块初始化失败！程序即将关闭";
                    LoggerHelper.Error("战地1内存模块初始化失败");
                    Task.Delay(2000).Wait();

                    this.Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                        return;
                    });
                }

                BF1API.Init();
                LoggerHelper.Info("战地1API模块初始化成功");

                ImageData.InitDict();
                LoggerHelper.Info("本地图片缓存库初始化成功");

                ChineseConverter.ToTraditional("免费，跨平台，开源！");
                LoggerHelper.Info("简繁翻译库初始化成功");

                // 创建文件夹
                Directory.CreateDirectory(FileUtil.D_Cache_Path);
                Directory.CreateDirectory(FileUtil.D_Config_Path);
                Directory.CreateDirectory(FileUtil.D_Data_Path);
                Directory.CreateDirectory(FileUtil.D_Log_Path);
                Directory.CreateDirectory(FileUtil.D_Robot_Path);

                // 释放必要文件
                FileUtil.ExtractResFile(FileUtil.Resource_Path + "config.yml", FileUtil.D_Robot_Path + "\\config.yml");
                FileUtil.ExtractResFile(FileUtil.Resource_Path + "go-cqhttp.exe", FileUtil.D_Robot_Path + "\\go-cqhttp.exe");

                SQLiteHelper.Initialize();
                LoggerHelper.Info($"SQLite数据库初始化成功");

                ChatMsg.AllocateMemory();
                LoggerHelper.Info($"中文聊天指针分配成功 0x{ChatMsg.GetAllocateMemoryAddress():x}");

                this.Dispatcher.Invoke(() =>
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    // 转移主程序控制权
                    Application.Current.MainWindow = mainWindow;
                    // 关闭初始化窗口
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                LoadModel.LoadState = "发生了未知异常！程序即将关闭";
                LoggerHelper.Error($"发生了未知异常", ex);
                Task.Delay(2000).Wait();

                this.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }
        });
    }
}
