using BF1.ServerAdminTools.Models;
using BF1.ServerAdminTools.Common.Utils;
using BF1.ServerAdminTools.Common.Helper;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// OptionView.xaml 的交互逻辑
/// </summary>
public partial class OptionView : UserControl
{
    public OptionModel OptionModel { get; set; } = new();

    // 声明一个变量，用于存储软件开始运行的时间
    private DateTime Origin_DateTime;

    public OptionView()
    {
        InitializeComponent();
        this.DataContext = this;

        OptionModel.AppRunTime = "运行时间 : Loading...";

        // 获取当前时间，存储到对于变量中
        Origin_DateTime = DateTime.Now;

        var thread0 = new Thread(UpdateState);
        thread0.IsBackground = true;
        thread0.Start();

        var temp = IniHelper.ReadString("Options", "AudioIndex", "", FileUtil.F_Settings_Path);
        if (!string.IsNullOrEmpty(temp))
            AudioUtil.ClickSoundIndex = Convert.ToInt32(temp);

        switch (AudioUtil.ClickSoundIndex)
        {
            case 0:
                RadioButton_ClickAudioSelect0.IsChecked = true;
                break;
            case 1:
                RadioButton_ClickAudioSelect1.IsChecked = true;
                break;
            case 2:
                RadioButton_ClickAudioSelect2.IsChecked = true;
                break;
            case 3:
                RadioButton_ClickAudioSelect3.IsChecked = true;
                break;
            case 4:
                RadioButton_ClickAudioSelect4.IsChecked = true;
                break;
            case 5:
                RadioButton_ClickAudioSelect5.IsChecked = true;
                break;
        }

        MainWindow.ClosingDisposeEvent += MainWindow_ClosingDisposeEvent;
    }

    private void MainWindow_ClosingDisposeEvent()
    {
        IniHelper.WriteString("Options", "AudioIndex", AudioUtil.ClickSoundIndex.ToString(), FileUtil.F_Settings_Path);
    }

    private void RadioButton_ClickAudioSelect_Click(object sender, RoutedEventArgs e)
    {
        string str = (sender as RadioButton).Content.ToString();

        switch (str)
        {
            case "无":
                AudioUtil.ClickSoundIndex = 0;
                break;
            case "提示音1":
                AudioUtil.ClickSoundIndex = 1;
                AudioUtil.ClickSound();
                break;
            case "提示音2":
                AudioUtil.ClickSoundIndex = 2;
                AudioUtil.ClickSound();
                break;
            case "提示音3":
                AudioUtil.ClickSoundIndex = 3;
                AudioUtil.ClickSound();
                break;
            case "提示音4":
                AudioUtil.ClickSoundIndex = 4;
                AudioUtil.ClickSound();
                break;
            case "提示音5":
                AudioUtil.ClickSoundIndex = 5;
                AudioUtil.ClickSound();
                break;
        }
    }

    private void UpdateState()
    {
        while (true)
        {
            // 获取软件运行时间
            OptionModel.AppRunTime = "运行时间 : " + CoreUtil.ExecDateDiff(Origin_DateTime, DateTime.Now);

            if (!ProcessUtil.IsAppRun(CoreUtil.TargetAppName))
            {
                this.Dispatcher.Invoke(() =>
                {
                    MainWindow.ThisMainWindow.Close();
                });
                return;
            }

            Thread.Sleep(1000);
        }
    }
}
