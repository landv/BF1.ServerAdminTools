using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models.Rule;

public class RuleTeamModel : ObservableObject
{
    private int _maxKill;
    /// <summary>
    /// 最大击杀
    /// </summary>
    public int MaxKill
    {
        get => _maxKill;
        set => SetProperty(ref _maxKill, value);
    }

    private int _kDFlag;
    /// <summary>
    /// 计算KD标志
    /// </summary>
    public int KDFlag
    {
        get => _kDFlag;
        set => SetProperty(ref _kDFlag, value);
    }

    private float _maxKD;
    /// <summary>
    /// 最大KD
    /// </summary>
    public float MaxKD
    {
        get => _maxKD;
        set => SetProperty(ref _maxKD, value);
    }

    private int _kPMFlag;
    /// <summary>
    /// 计算KPM标志
    /// </summary>
    public int KPMFlag
    {
        get => _kPMFlag;
        set => SetProperty(ref _kPMFlag, value);
    }

    private float _maxKPM;
    /// <summary>
    /// 最大KPM
    /// </summary>
    public float MaxKPM
    {
        get => _maxKPM;
        set => SetProperty(ref _maxKPM, value);
    }

    private int _minRank;
    /// <summary>
    /// 最低等级
    /// </summary>
    public int MinRank
    {
        get => _minRank;
        set => SetProperty(ref _minRank, value);
    }

    private int _maxRank;
    /// <summary>
    /// 最低等级
    /// </summary>
    public int MaxRank
    {
        get => _maxRank;
        set => SetProperty(ref _maxRank, value);
    }

    private float _lifeMaxKD;
    /// <summary>
    /// 最大生涯KD
    /// </summary>
    public float LifeMaxKD
    {
        get => _lifeMaxKD;
        set => SetProperty(ref _lifeMaxKD, value);
    }

    private float _lifeMaxKPM;
    /// <summary>
    /// 最大生涯KPM
    /// </summary>
    public float LifeMaxKPM
    {
        get => _lifeMaxKPM;
        set => SetProperty(ref _lifeMaxKPM, value);
    }

    private int _lifeMaxWeaponStar;
    /// <summary>
    /// 最大生涯武器星数
    /// </summary>
    public int LifeMaxWeaponStar
    {
        get => _lifeMaxWeaponStar;
        set => SetProperty(ref _lifeMaxWeaponStar, value);
    }

    private int _lifeMaxVehicleStar;
    /// <summary>
    /// 最大生涯载具星数
    /// </summary>
    public int LifeMaxVehicleStar
    {
        get => _lifeMaxVehicleStar;
        set => SetProperty(ref _lifeMaxVehicleStar, value);
    }
}
