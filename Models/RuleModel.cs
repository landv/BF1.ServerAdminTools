using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models;

public class RuleModel : ObservableObject
{
    private long _groupID;
    /// <summary>
    /// 目标QQ群号
    /// </summary>
    public long GroupID
    {
        get => _groupID;
        set => SetProperty(ref _groupID, value);
    }
}
