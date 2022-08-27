using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models;

public class DetailModel : ObservableObject
{
    private string _serverName;
    /// <summary>
    /// 服务器名称
    /// </summary>
    public string ServerName
    {
        get => _serverName;
        set => SetProperty(ref _serverName, value);
    }

    private string _serverDescription;
    /// <summary>
    /// 服务器描述
    /// </summary>
    public string ServerDescription
    {
        get => _serverDescription;
        set => SetProperty(ref _serverDescription, value);
    }

    private string _serverID;
    /// <summary>
    /// 服务器ServerID
    /// </summary>
    public string ServerID
    {
        get => _serverID;
        set => SetProperty(ref _serverID, value);
    }

    private string _serverGameID;
    /// <summary>
    /// 服务器GameID
    /// </summary>
    public string ServerGameID
    {
        get => _serverGameID;
        set => SetProperty(ref _serverGameID, value);
    }

    private string _serverOwnerName;
    /// <summary>
    /// 服主ID
    /// </summary>
    public string ServerOwnerName
    {
        get => _serverOwnerName;
        set => SetProperty(ref _serverOwnerName, value);
    }

    private string _serverOwnerPersonaId;
    /// <summary>
    /// 服主数字ID
    /// </summary>
    public string ServerOwnerPersonaId
    {
        get => _serverOwnerPersonaId;
        set => SetProperty(ref _serverOwnerPersonaId, value);
    }

    private string _serverOwnerImage;
    /// <summary>
    /// 服主头像
    /// </summary>
    public string ServerOwnerImage
    {
        get => _serverOwnerImage;
        set => SetProperty(ref _serverOwnerImage, value);
    }
}
