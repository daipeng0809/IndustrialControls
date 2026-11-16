using Avalonia;
using Avalonia.Media;
using IndustrialControls.Primitives;

namespace IndustrialControls.Controls;

/// <summary>
/// 扇形进度条：环形/扇形进度弧 + 中心数值文字，支持渐变画刷、圆角端点与柔光。
/// </summary>
/// <remarks>角度约定与 <see cref="Gauge"/> 相同：0° 指向 3 点钟方向，顺时针为正。默认从 135° 扫过 270°（开口向下）。</remarks>
public class SectorProgressBar : InstrumentBase
{
    /// <summary>起始角度（度）。</summary>
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<SectorProgressBar, double>(nameof(StartAngle), 135d);

    /// <summary>扫过角度（度）。</summary>
    public static readonly StyledProperty<double> SweepAngleProperty =
        AvaloniaProperty.Register<SectorProgressBar, double>(nameof(SweepAngle), 270d);

    /// <summary>进度弧粗度。</summary>
    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<SectorProgressBar, double>(nameof(Thickness), 14d);

    /// <summary>轨道颜色。</summary>
    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<SectorProgressBar, IBrush?>(nameof(TrackBrush), IndustrialPalette.Track);

    /// <summary>进度弧颜色，可使用渐变。</summary>
    public static readonly StyledProperty<IBrush?> ProgressBrushProperty =
        AvaloniaProperty.Register<SectorProgressBar, IBrush?>(nameof(ProgressBrush), IndustrialPalette.Accent);

    /// <summary>是否带柔光。</summary>
    public static readonly StyledProperty<bool> ProgressGlowProperty =
        AvaloniaProperty.Register<SectorProgressBar, bool>(nameof(ProgressGlow), true);

    /// <summary>进度弧端点是否为圆角。</summary>
    public static readonly StyledProperty<bool> RoundCapsProperty =
        AvaloniaProperty.Register<SectorProgressBar, bool>(nameof(RoundCaps), true);

    /// <summary>是否显示中心数值。</summary>
    public static readonly StyledProperty<bool> ShowValueTextProperty =
        AvaloniaProperty.Register<SectorProgressBar, bool>(nameof(ShowValueText), true);

    /// <summary>数值格式，默认 "F0"。</summary>
    public static readonly StyledProperty<string?> ValueFormatProperty =
        AvaloniaProperty.Register<SectorProgressBar, string?>(nameof(ValueFormat), "F0");

    /// <summary>数值字号。</summary>
    public static readonly StyledProperty<double> ValueFontSizeProperty =
        AvaloniaProperty.Register<SectorProgressBar, double>(nameof(ValueFontSize), 30d);

    /// <summary>数值颜色。</summary>
    public static readonly StyledProperty<IBrush?> ValueBrushProperty =
        AvaloniaProperty.Register<SectorProgressBar, IBrush?>(nameof(ValueBrush), IndustrialPalette.Text);

    /// <summary>数值字体。</summary>
    public static readonly StyledProperty<FontFamily?> ValueFontFamilyProperty =
        AvaloniaProperty.Register<SectorProgressBar, FontFamily?>(nameof(ValueFontFamily));

    /// <summary>数值字重。</summary>
    public static readonly StyledProperty<FontWeight> ValueFontWeightProperty =
        AvaloniaProperty.Register<SectorProgressBar, FontWeight>(nameof(ValueFontWeight), FontWeight.Bold);

    /// <summary>数值相对圆心偏移（像素）。</summary>
    public static readonly StyledProperty<Point> ValueOffsetProperty =
        AvaloniaProperty.Register<SectorProgressBar, Point>(nameof(ValueOffset), new Point(0, -4));

    /// <summary>说明文字（可为空）。</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<SectorProgressBar, string?>(nameof(Label));

    /// <summary>说明文字字号。</summary>
    public static readonly StyledProperty<double> LabelFontSizeProperty =
        AvaloniaProperty.Register<SectorProgressBar, double>(nameof(LabelFontSize), 12d);

    /// <summary>说明文字颜色。</summary>
    public static readonly StyledProperty<IBrush?> LabelBrushProperty =
        AvaloniaProperty.Register<SectorProgressBar, IBrush?>(nameof(LabelBrush), IndustrialPalette.Muted);

    /// <summary>说明文字字体。</summary>
    public static readonly StyledProperty<FontFamily?> LabelFontFamilyProperty =
        AvaloniaProperty.Register<SectorProgressBar, FontFamily?>(nameof(LabelFontFamily));

    /// <summary>说明文字相对圆心偏移（像素）。</summary>
    public static readonly StyledProperty<Point> LabelOffsetProperty =
        AvaloniaProperty.Register<SectorProgressBar, Point>(nameof(LabelOffset), new Point(0, 28));

    private Point _center;
    private double _radius;
    private StreamGeometry? _trackGeometry;
    private MutableArc? _progressArc;
    private Pen? _trackPen;
    private Pen? _progressPen;
    private Pen? _glowPen;
    private FormattedText? _labelText;
    private FormattedText? _valueText;
    private string? _valueTextContent;
    private double _valueTextSize;
    private FontWeight _valueTextWeight;
    private FontFamily? _valueTextFamily;
    private IBrush? _valueTextBrush;

    /// <summary>起始角度（度）。</summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>扫过角度（度）。</summary>
    public double SweepAngle
    {
        get => GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    /// <summary>进度弧粗度。</summary>
    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    /// <summary>轨道颜色。</summary>
    public IBrush? TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    /// <summary>进度弧颜色。</summary>
    public IBrush? ProgressBrush
    {
        get => GetValue(ProgressBrushProperty);
        set => SetValue(ProgressBrushProperty, value);
    }

    /// <summary>是否带柔光。</summary>
    public bool ProgressGlow
    {
        get => GetValue(ProgressGlowProperty);
        set => SetValue(ProgressGlowProperty, value);
    }

    /// <summary>进度弧端点是否为圆角。</summary>
    public bool RoundCaps
    {
        get => GetValue(RoundCapsProperty);
        set => SetValue(RoundCapsProperty, value);
    }

    /// <summary>是否显示中心数值。</summary>
    public bool ShowValueText
    {
        get => GetValue(ShowValueTextProperty);
        set => SetValue(ShowValueTextProperty, value);
    }

    /// <summary>数值格式。</summary>
    public string? ValueFormat
    {
        get => GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    /// <summary>数值字号。</summary>
    public double ValueFontSize
    {
        get => GetValue(ValueFontSizeProperty);
        set => SetValue(ValueFontSizeProperty, value);
    }

    /// <summary>数值颜色。</summary>
    public IBrush? ValueBrush
    {
        get => GetValue(ValueBrushProperty);
        set => SetValue(ValueBrushProperty, value);
    }

    /// <summary>数值字体。</summary>
    public FontFamily? ValueFontFamily
    {
        get => GetValue(ValueFontFamilyProperty);
        set => SetValue(ValueFontFamilyProperty, value);
    }

    /// <summary>数值字重。</summary>
    public FontWeight ValueFontWeight
    {
        get => GetValue(ValueFontWeightProperty);
        set => SetValue(ValueFontWeightProperty, value);
    }

    /// <summary>数值相对圆心偏移（像素）。</summary>
    public Point ValueOffset
    {
        get => GetValue(ValueOffsetProperty);
        set => SetValue(ValueOffsetProperty, value);
    }

    /// <summary>说明文字。</summary>
    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>说明文字字号。</summary>
    public double LabelFontSize
    {
        get => GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    /// <summary>说明文字颜色。</summary>
    public IBrush? LabelBrush
    {
        get => GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    /// <summary>说明文字字体。</summary>
    public FontFamily? LabelFontFamily
    {
        get => GetValue(LabelFontFamilyProperty);
        set => SetValue(LabelFontFamilyProperty, value);
    }

    /// <summary>说明文字相对圆心偏移（像素）。</summary>
    public Point LabelOffset
    {
        get => GetValue(LabelOffsetProperty);
        set => SetValue(LabelOffsetProperty, value);
    }

    /// <inheritdoc />
    protected override void RenderCore(DrawingContext context)
    {
        Size size = Bounds.Size;
        if (size.Width < 8 || size.Height < 8)
        {
            return;
        }

        if (IsStaticCacheInvalid(size))
        {
            RebuildStatic(size);
            MarkStaticCacheBuilt(size);
        }

        bool fullCircle = DrawHelper.IsFullCircle(SweepAngle);
        double normalized = NormalizedValue;

        if (_trackPen is not null)
        {
            if (fullCircle)
            {
                context.DrawEllipse(null, _trackPen, _center, _radius, _radius);
            }
            else if (_trackGeometry is not null)
            {
                context.DrawGeometry(null, _trackPen, _trackGeometry);
            }
        }

        if (normalized > 0.0005 && _progressPen is not null)
        {
            if (fullCircle && normalized >= 0.9995)
            {
                context.DrawEllipse(null, _progressPen, _center, _radius, _radius);
            }
            else if (_progressArc is not null)
            {
                // 复用同一个几何对象，每帧只更新端点，避免分配
                _progressArc.Update(_center, _radius, StartAngle, SweepAngle * normalized);
                PathGeometry progress = _progressArc.Geometry;

                if (ProgressGlow && _glowPen is not null)
                {
                    using (context.PushOpacity(0.18))
                    {
                        context.DrawGeometry(null, _glowPen, progress);
                    }
                }

                context.DrawGeometry(null, _progressPen, progress);
            }
        }

        if (ShowValueText && ValueBrush is not null && ValueFontSize > 0)
        {
            string content = DrawHelper.FormatValue(Value, ValueFormat);
            if (_valueText is null
                || !string.Equals(content, _valueTextContent, StringComparison.Ordinal)
                || _valueTextSize != ValueFontSize
                || _valueTextWeight != ValueFontWeight
                || !Equals(_valueTextFamily, ValueFontFamily)
                || !ReferenceEquals(_valueTextBrush, ValueBrush))
            {
                _valueText = DrawHelper.CreateText(content, ValueFontFamily, ValueFontWeight, ValueFontSize, ValueBrush);
                _valueTextContent = content;
                _valueTextSize = ValueFontSize;
                _valueTextWeight = ValueFontWeight;
                _valueTextFamily = ValueFontFamily;
                _valueTextBrush = ValueBrush;
            }

            if (_valueText is not null)
            {
                DrawHelper.DrawTextCentered(context, _valueText, _center + ValueOffset);
            }
        }

        if (_labelText is not null)
        {
            DrawHelper.DrawTextCentered(context, _labelText, _center + LabelOffset);
        }
    }

    private void RebuildStatic(Size size)
    {
        double thickness = Math.Max(0.5, Thickness);
        _center = new Point(size.Width / 2, size.Height / 2);
        _radius = Math.Max(1, Math.Min(size.Width, size.Height) / 2 - 2 - thickness / 2);

        PenLineCap cap = RoundCaps ? PenLineCap.Round : PenLineCap.Flat;
        _trackPen = TrackBrush is null ? null : DrawHelper.CreatePen(TrackBrush, thickness, cap);
        _progressPen = ProgressBrush is null ? null : DrawHelper.CreatePen(ProgressBrush, thickness, cap);
        _glowPen = ProgressBrush is null
            ? null
            : DrawHelper.CreatePen(ProgressBrush, thickness + Math.Max(6, thickness * 0.9), PenLineCap.Round);

        _progressArc = new MutableArc(_radius);

        _trackGeometry = DrawHelper.IsFullCircle(SweepAngle)
            ? null
            : DrawHelper.CreateArc(_center, _radius, StartAngle, SweepAngle);

        _labelText = DrawHelper.CreateText(Label, LabelFontFamily, FontWeight.Normal, LabelFontSize, LabelBrush);
        _valueText = null;
        _valueTextContent = null;
    }
}
