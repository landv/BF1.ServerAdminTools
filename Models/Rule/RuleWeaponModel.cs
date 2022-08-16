using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models.Rule;

public class RuleWeaponModel : ObservableObject
{
    private string _class;
    /// <summary>
    /// 分类
    /// </summary>
    public string Class
    {
        get => _class;
        set => SetProperty(ref _class, value);
    }

    private string _name;
    /// <summary>
    /// 名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _english;
    /// <summary>
    /// 英文名称
    /// </summary>
    public string English
    {
        get => _english;
        set => SetProperty(ref _english, value);
    }

    private bool _team1;
    /// <summary>
    /// 队伍1限制
    /// </summary>
    public bool Team1
    {
        get => _team1;
        set => SetProperty(ref _team1, value);
    }

    private bool _team2;
    /// <summary>
    /// 队伍2限制
    /// </summary>
    public bool Team2
    {
        get => _team2;
        set => SetProperty(ref _team2, value);
    }
}
