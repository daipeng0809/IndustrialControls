using Avalonia;
using Avalonia.Media;

namespace IndustrialControls;

/// <summary>
/// 仪表盘上的彩色分区（例如：正常区/警告区/危险区）。
/// </summary>
/// <remarks>
/// 修改分区属性后，所属 <see cref="Controls.Gauge"/> 会自动刷新。
/// </remarks>
public class GaugeRange : AvaloniaObject
{
    /// <summary>分区起始值。</summary>
    public static readonly StyledProperty<double> FromProperty =
        AvaloniaProperty.Register<GaugeRange, double>(nameof(From));

    /// <summary>分区结束值。</summary>
    public static readonly StyledProperty<double> ToProperty =
        AvaloniaProperty.Register<GaugeRange, double>(nameof(To));

    /// <summary>分区颜色。</summary>
    public static readonly StyledProperty<IBrush?> BrushProperty =
        AvaloniaProperty.Register<GaugeRange, IBrush?>(nameof(Brush));

    /// <summary>分区在仪表盘外圈的粗度；<see cref="double.NaN"/> 表示使用 Gauge.RangeThickness。</summary>
    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<GaugeRange, double>(nameof(Thickness), double.NaN);

    /// <summary>分区起始值。</summary>
    public double From
    {
        get => GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    /// <summary>分区结束值。</summary>
    public double To
    {
        get => GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    /// <summary>分区颜色。</summary>
    public IBrush? Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    /// <summary>分区在仪表盘外圈的粗度；NaN 表示使用 Gauge.RangeThickness。</summary>
    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }
}
