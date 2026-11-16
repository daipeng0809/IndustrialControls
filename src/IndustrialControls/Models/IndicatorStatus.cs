namespace IndustrialControls;

/// <summary>
/// 设备/指示灯状态，决定状态灯的默认颜色。
/// </summary>
public enum IndicatorStatus
{
    /// <summary>熄灭（灰色）。</summary>
    Off,

    /// <summary>正常/运行（绿色）。</summary>
    Normal,

    /// <summary>警告（琥珀色）。</summary>
    Warning,

    /// <summary>告警/故障（红色）。</summary>
    Alarm,

    /// <summary>维护/待机（蓝色）。</summary>
    Maintenance,
}
