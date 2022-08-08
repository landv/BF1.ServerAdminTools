using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models;

public class ServerModel : ObservableObject
{
    private string serverName;
    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName
    {
        get => serverName;
        set => SetProperty(ref serverName, value);
    }

    private Visibility loadingVisibility;
    /// <summary>
    /// 加载动画
    /// </summary>
    public Visibility LoadingVisibility
    {
        get => loadingVisibility;
        set => SetProperty(ref loadingVisibility, value);
    }
}
