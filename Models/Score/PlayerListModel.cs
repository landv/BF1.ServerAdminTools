using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models.Score;

public class PlayerListModel : ObservableObject, IComparable<PlayerListModel>
{
    private int _index;
    /// <summary>
    /// 序号
    /// </summary>
    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
    }

    ///////////////////////////////////////////////////////////////////////

    private string _clan;
    /// <summary>
    /// 玩家战队
    /// </summary>
    public string Clan
    {
        get => _clan;
        set => SetProperty(ref _clan, value);
    }

    private string _name;
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private long _personaId;
    /// <summary>
    /// 玩家数字ID
    /// </summary>
    public long PersonaId
    {
        get => _personaId;
        set => SetProperty(ref _personaId, value);
    }

    private string _squadId;
    /// <summary>
    /// 玩家小队ID
    /// </summary>
    public string SquadId
    {
        get => _squadId;
        set => SetProperty(ref _squadId, value);
    }

    ///////////////////////////////////////////////////////////////////////

    private string _admin;
    /// <summary>
    /// 管理员
    /// </summary>
    public string Admin
    {
        get => _admin;
        set => SetProperty(ref _admin, value);
    }

    private string _vip;
    /// <summary>
    /// VIP
    /// </summary>
    public string VIP
    {
        get => _vip;
        set => SetProperty(ref _vip, value);
    }

    ///////////////////////////////////////////////////////////////////////

    private int _rank;
    /// <summary>
    /// 等级
    /// </summary>
    public int Rank
    {
        get => _rank;
        set => SetProperty(ref _rank, value);
    }

    private int _kill;
    /// <summary>
    /// 击杀
    /// </summary>
    public int Kill
    {
        get => _kill;
        set => SetProperty(ref _kill, value);
    }

    private int _dead;
    /// <summary>
    /// 死亡
    /// </summary>
    public int Dead
    {
        get => _dead;
        set => SetProperty(ref _dead, value);
    }

    private string _kd;
    /// <summary>
    /// KD
    /// </summary>
    public string KD
    {
        get => _kd;
        set => SetProperty(ref _kd, value);
    }

    private string _kpm;
    /// <summary>
    /// KPM
    /// </summary>
    public string KPM
    {
        get => _kpm;
        set => SetProperty(ref _kpm, value);
    }

    private int _score;
    /// <summary>
    /// 得分
    /// </summary>
    public int Score
    {
        get => _score;
        set => SetProperty(ref _score, value);
    }

    ///////////////////////////////////////////////////////////////////////

    private string _weaponS0;
    /// <summary>
    /// 武器槽0
    /// </summary>
    public string WeaponS0
    {
        get => _weaponS0;
        set => SetProperty(ref _weaponS0, value);
    }

    private string _weaponS1;
    /// <summary>
    /// 武器槽1
    /// </summary>
    public string WeaponS1
    {
        get => _weaponS1;
        set => SetProperty(ref _weaponS1, value);
    }

    private string _weaponS2;
    /// <summary>
    /// 武器槽2
    /// </summary>
    public string WeaponS2
    {
        get => _weaponS2;
        set => SetProperty(ref _weaponS2, value);
    }

    private string _weaponS3;
    /// <summary>
    /// 武器槽3
    /// </summary>
    public string WeaponS3
    {
        get => _weaponS3;
        set => SetProperty(ref _weaponS3, value);
    }

    private string _weaponS4;
    /// <summary>
    /// 武器槽4
    /// </summary>
    public string WeaponS4
    {
        get => _weaponS4;
        set => SetProperty(ref _weaponS4, value);
    }

    private string _weaponS5;
    /// <summary>
    /// 武器槽5
    /// </summary>
    public string WeaponS5
    {
        get => _weaponS5;
        set => SetProperty(ref _weaponS5, value);
    }

    private string _weaponS6;
    /// <summary>
    /// 武器槽6
    /// </summary>
    public string WeaponS6
    {
        get => _weaponS6;
        set => SetProperty(ref _weaponS6, value);
    }

    private string _weaponS7;
    /// <summary>
    /// 武器槽0
    /// </summary>
    public string WeaponS7
    {
        get => _weaponS7;
        set => SetProperty(ref _weaponS7, value);
    }

    public int CompareTo(PlayerListModel other)
    {
        return other.Score.CompareTo(Score);
    }
}
