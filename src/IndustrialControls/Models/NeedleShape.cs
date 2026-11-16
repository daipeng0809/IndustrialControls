namespace IndustrialControls;

/// <summary>
/// 仪表指针的形状。
/// </summary>
public enum NeedleShape
{
    /// <summary>带圆头的粗线。</summary>
    Line,

    /// <summary>三角形。</summary>
    Triangle,

    /// <summary>风筝形（尾部收窄，工业仪表常用）。</summary>
    Tapered,
}
