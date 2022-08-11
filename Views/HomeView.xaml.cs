using CommunityToolkit.Mvvm.Messaging;

namespace BF1.ServerAdminTools.Views;

/// <summary>
/// HomeView.xaml 的交互逻辑
/// </summary>
public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<string, string>(this, "Notice", (s, e) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                TextBox_Notice.Text = e;
            });
        });

        WeakReferenceMessenger.Default.Register<string, string>(this, "Change", (s, e) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                TextBox_Change.Text = e;
            });
        });
    }
}
