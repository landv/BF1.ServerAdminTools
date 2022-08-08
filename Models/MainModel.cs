﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1.ServerAdminTools.Models;

public class MainModel : ObservableObject
{
    private string _appRunTime;
    /// <summary>
    /// 程序运行时间
    /// </summary>
    public string AppRunTime
    {
        get { return _appRunTime; }
        set { _appRunTime = value; OnPropertyChanged(); }
    }
}
