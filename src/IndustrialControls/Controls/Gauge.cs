using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using IndustrialControls.Primitives;

namespace IndustrialControls.Controls;

/// <summary>
/// 工业仪表盘：可自定义扇形刻度范围、刻度密度、彩色分区、指针形状/长度/颜色，以及标题、单位、数值文字的位置、字号、颜色、字体。
/// </summary>
/// <remarks>
/// <para>角度约定：0° 指向 3 点钟方向，顺时针为正。默认从 150°（左下）顺时针扫过 240°（到右下），即常见的开口向下的工业表盘。</para>
/// <para>性能：刻度、刻度数字、色带几何会被缓存；只有 <see cref="InstrumentBase.Value"/> 变化时重绘指针与数值文字。</para>
/// </remarks>
/// <example>
/// <code>
/// &lt;ic:Gauge Minimum="0" Maximum="100" Value="42" Unit="℃" Title="温度"&gt;
///   &lt;ic:Gauge.Ranges&gt;
///     &lt;ic:GaugeRange From="0" To="60" Brush="#34D399" /&gt;
///     &lt;ic:GaugeRange From="60" To="85" Brush="#FBBF24" /&gt;
///     &lt;ic:GaugeRange From="85" To="100" Brush="#F87171" /&gt;
///   &lt;/ic:Gauge.Ranges&gt;
/// &lt;/ic:Gauge&gt;
/// </code>
/// </example>
public class Gauge : InstrumentBase
{
    #region 数值与文字

    /// <summary>数值格式字符串，默认 "F1"。</summary>
    public static readonly StyledProperty<string?> ValueFormatProperty =
        AvaloniaProperty.Register<Gauge, string?>(nameof(ValueFormat), "F1");

    /// <summary>单位文字（例如 ℃、%RH、m/s），位置由 <see cref="UnitOffset"/> 控制。</summary>
    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<Gauge, string?>(nameof(Unit));

    /// <summary>标题文字，位置由 <see cref="TitleOffset"/> 控制。</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Gauge, string?>(nameof(Title));

    /// <summary>是否显示数值文字。</summary>
    public static readonly StyledProperty<bool> ShowValueTextProperty =
        AvaloniaProperty.Register<Gauge, bool>(nameof(ShowValueText), true);

    /// <summary>数值字号。</summary>
    public static readonly StyledProperty<double> ValueFontSizeProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(ValueFontSize), 28d);

    /// <summary>数值颜色。</summary>
    public static readonly StyledProperty<IBrush?> ValueBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(ValueBrush), IndustrialPalette.Text);

    /// <summary>数值字体。</summary>
    public static readonly StyledProperty<FontFamily?> ValueFontFamilyProperty =
        AvaloniaProperty.Register<Gauge, FontFamily?>(nameof(ValueFontFamily));

    /// <summary>数值字重。</summary>
    public static readonly StyledProperty<FontWeight> ValueFontWeightProperty =
        AvaloniaProperty.Register<Gauge, FontWeight>(nameof(ValueFontWeight), FontWeight.SemiBold);

    /// <summary>数值文字相对表盘中心的偏移（像素）。</summary>
    public static readonly StyledProperty<Point> ValueOffsetProperty =
        AvaloniaProperty.Register<Gauge, Point>(nameof(ValueOffset), new Point(0, 26));

    /// <summary>标题字号。</summary>
    public static readonly StyledProperty<double> TitleFontSizeProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(TitleFontSize), 13d);

    /// <summary>标题颜色。</summary>
    public static readonly StyledProperty<IBrush?> TitleBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(TitleBrush), IndustrialPalette.Muted);

    /// <summary>标题字体。</summary>
    public static readonly StyledProperty<FontFamily?> TitleFontFamilyProperty =
        AvaloniaProperty.Register<Gauge, FontFamily?>(nameof(TitleFontFamily));

    /// <summary>标题相对表盘中心的偏移（像素）。</summary>
    public static readonly StyledProperty<Point> TitleOffsetProperty =
        AvaloniaProperty.Register<Gauge, Point>(nameof(TitleOffset), new Point(0, -58));

    /// <summary>单位字号。</summary>
    public static readonly StyledProperty<double> UnitFontSizeProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(UnitFontSize), 13d);

    /// <summary>单位颜色。</summary>
    public static readonly StyledProperty<IBrush?> UnitBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(UnitBrush), IndustrialPalette.Muted);

    /// <summary>单位字体。</summary>
    public static readonly StyledProperty<FontFamily?> UnitFontFamilyProperty =
        AvaloniaProperty.Register<Gauge, FontFamily?>(nameof(UnitFontFamily));

    /// <summary>单位相对表盘中心的偏移（像素）。</summary>
    public static readonly StyledProperty<Point> UnitOffsetProperty =
        AvaloniaProperty.Register<Gauge, Point>(nameof(UnitOffset), new Point(0, 60));

    #endregion

    #region 刻度范围与轨道

    /// <summary>起始角度（度，0° 为 3 点钟方向，顺时针为正）。</summary>
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(StartAngle), 150d);

    /// <summary>扫过角度（度，顺时针为正）。</summary>
    public static readonly StyledProperty<double> SweepAngleProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(SweepAngle), 240d);

    /// <summary>刻度盘轨道粗度。</summary>
    public static readonly StyledProperty<double> TrackThicknessProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(TrackThickness), 12d);

    /// <summary>刻度盘轨道颜色。</summary>
    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(TrackBrush), IndustrialPalette.Track);

    /// <summary>是否绘制进度弧（从最小值扫到当前值）。</summary>
    public static readonly StyledProperty<bool> ShowProgressProperty =
        AvaloniaProperty.Register<Gauge, bool>(nameof(ShowProgress), true);

    /// <summary>进度弧颜色，可设为渐变。</summary>
    public static readonly StyledProperty<IBrush?> ProgressBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(ProgressBrush), IndustrialPalette.Accent);

    /// <summary>进度弧是否带柔光。</summary>
    public static readonly StyledProperty<bool> ProgressGlowProperty =
        AvaloniaProperty.Register<Gauge, bool>(nameof(ProgressGlow), true);

    #endregion

    #region 刻度与刻度文字

    /// <summary>是否显示刻度线。</summary>
    public static readonly StyledProperty<bool> ShowTickMarksProperty =
        AvaloniaProperty.Register<Gauge, bool>(nameof(ShowTickMarks), true);

    /// <summary>主刻度数量（包含量程两端），默认 11。</summary>
    public static readonly StyledProperty<int> MajorTickCountProperty =
        AvaloniaProperty.Register<Gauge, int>(nameof(MajorTickCount), 11);

    /// <summary>每个主刻度之间的副刻度数量。</summary>
    public static readonly StyledProperty<int> MinorTicksPerMajorProperty =
        AvaloniaProperty.Register<Gauge, int>(nameof(MinorTicksPerMajor), 4);

    /// <summary>主刻度长度。</summary>
    public static readonly StyledProperty<double> MajorTickLengthProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(MajorTickLength), 12d);

    /// <summary>副刻度长度。</summary>
    public static readonly StyledProperty<double> MinorTickLengthProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(MinorTickLength), 6d);

    /// <summary>主刻度线宽。</summary>
    public static readonly StyledProperty<double> MajorTickWidthProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(MajorTickWidth), 2d);

    /// <summary>副刻度线宽。</summary>
    public static readonly StyledProperty<double> MinorTickWidthProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(MinorTickWidth), 1d);

    /// <summary>主刻度颜色。</summary>
    public static readonly StyledProperty<IBrush?> MajorTickBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(MajorTickBrush), IndustrialPalette.MajorTick);

    /// <summary>副刻度颜色。</summary>
    public static readonly StyledProperty<IBrush?> MinorTickBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(MinorTickBrush), IndustrialPalette.Tick);

    /// <summary>是否显示刻度数字。</summary>
    public static readonly StyledProperty<bool> ShowScaleLabelsProperty =
        AvaloniaProperty.Register<Gauge, bool>(nameof(ShowScaleLabels), true);

    /// <summary>刻度数字格式，默认 "F0"。</summary>
    public static readonly StyledProperty<string?> ScaleLabelFormatProperty =
        AvaloniaProperty.Register<Gauge, string?>(nameof(ScaleLabelFormat), "F0");

    /// <summary>刻度数字字号。</summary>
    public static readonly StyledProperty<double> ScaleLabelFontSizeProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(ScaleLabelFontSize), 11d);

    /// <summary>刻度数字颜色。</summary>
    public static readonly StyledProperty<IBrush?> ScaleLabelBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(ScaleLabelBrush), IndustrialPalette.Label);

    /// <summary>刻度数字字体。</summary>
    public static readonly StyledProperty<FontFamily?> ScaleLabelFontFamilyProperty =
        AvaloniaProperty.Register<Gauge, FontFamily?>(nameof(ScaleLabelFontFamily));

    /// <summary>刻度数字相对刻度线内端的径向间距。</summary>
    public static readonly StyledProperty<double> ScaleLabelOffsetProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(ScaleLabelOffset), 8d);

    #endregion

    #region 指针

    /// <summary>是否显示指针。</summary>
    public static readonly StyledProperty<bool> ShowNeedleProperty =
        AvaloniaProperty.Register<Gauge, bool>(nameof(ShowNeedle), true);

    /// <summary>指针形状。</summary>
    public static readonly StyledProperty<NeedleShape> NeedleShapeProperty =
        AvaloniaProperty.Register<Gauge, NeedleShape>(nameof(NeedleShape), NeedleShape.Tapered);

    /// <summary>指针长度比例（0~1，相对于刻度内圈）。</summary>
    public static readonly StyledProperty<double> NeedleLengthProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(NeedleLength), 0.88d);

    /// <summary>指针宽度。</summary>
    public static readonly StyledProperty<double> NeedleWidthProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(NeedleWidth), 6d);

    /// <summary>指针尾部长度比例（相对于刻度内圈），风筝形/三角形使用。</summary>
    public static readonly StyledProperty<double> NeedleBackLengthProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(NeedleBackLength), 0.16d);

    /// <summary>指针颜色。</summary>
    public static readonly StyledProperty<IBrush?> NeedleBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(NeedleBrush), IndustrialPalette.Needle);

    /// <summary>指针轴心半径。</summary>
    public static readonly StyledProperty<double> NeedleHubRadiusProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(NeedleHubRadius), 9d);

    /// <summary>指针轴心颜色。</summary>
    public static readonly StyledProperty<IBrush?> NeedleHubBrushProperty =
        AvaloniaProperty.Register<Gauge, IBrush?>(nameof(NeedleHubBrush), IndustrialPalette.Hub);

    #endregion

    #region 彩色分区

    /// <summary>表盘外圈色带粗度。</summary>
    public static readonly StyledProperty<double> RangeThicknessProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(RangeThickness), 4d);

    /// <summary>相邻色带之间的角度间隔（度）。</summary>
    public static readonly StyledProperty<double> RangeGapProperty =
        AvaloniaProperty.Register<Gauge, double>(nameof(RangeGap), 2d);

    #endregion

    private readonly List<GaugeRange> _subscribedRanges = new();
    private readonly List<RangeVisual> _rangeVisuals = new();

    private Point _center;
    private double _trackRadius;
    private double _tickOuterRadius;
    private double _labelRadius;
    private double _needleRadius;
    private StreamGeometry? _trackGeometry;
    private StreamGeometry? _majorTicksGeometry;
    private StreamGeometry? _minorTicksGeometry;
    private MutableArc? _progressArc;
    private StreamGeometry? _needleGeometry;
    private FormattedText[]? _scaleLabels;
    private Point[]? _scaleLabelPositions;
    private FormattedText? _titleText;
    private FormattedText? _unitText;
    private Pen? _trackPen;
    private Pen? _progressPen;
    private Pen? _glowPen;
    private Pen? _majorTickPen;
    private Pen? _minorTickPen;
    private Pen? _needleLinePen;
    private FormattedText? _valueText;
    private string? _valueTextContent;
    private double _valueTextSize;
    private FontWeight _valueTextWeight;
    private FontFamily? _valueTextFamily;
    private IBrush? _valueTextBrush;

    private readonly record struct RangeVisual(StreamGeometry Geometry, Pen Pen);

    /// <summary>创建仪表盘。</summary>
    public Gauge()
    {
        Ranges.CollectionChanged += OnRangesChanged;
    }

    /// <summary>彩色分区集合，例如正常区/警告区/危险区。</summary>
    public AvaloniaList<GaugeRange> Ranges { get; } = new();

    /// <summary>数值格式字符串，默认 "F1"。</summary>
    public string? ValueFormat
    {
        get => GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    /// <summary>单位文字（例如 ℃、%RH、m/s）。</summary>
    public string? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    /// <summary>标题文字。</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>是否显示数值文字。</summary>
    public bool ShowValueText
    {
        get => GetValue(ShowValueTextProperty);
        set => SetValue(ShowValueTextProperty, value);
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

    /// <summary>数值文字相对表盘中心的偏移（像素）。</summary>
    public Point ValueOffset
    {
        get => GetValue(ValueOffsetProperty);
        set => SetValue(ValueOffsetProperty, value);
    }

    /// <summary>标题字号。</summary>
    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    /// <summary>标题颜色。</summary>
    public IBrush? TitleBrush
    {
        get => GetValue(TitleBrushProperty);
        set => SetValue(TitleBrushProperty, value);
    }

    /// <summary>标题字体。</summary>
    public FontFamily? TitleFontFamily
    {
        get => GetValue(TitleFontFamilyProperty);
        set => SetValue(TitleFontFamilyProperty, value);
    }

    /// <summary>标题相对表盘中心的偏移（像素）。</summary>
    public Point TitleOffset
    {
        get => GetValue(TitleOffsetProperty);
        set => SetValue(TitleOffsetProperty, value);
    }

    /// <summary>单位字号。</summary>
    public double UnitFontSize
    {
        get => GetValue(UnitFontSizeProperty);
        set => SetValue(UnitFontSizeProperty, value);
    }

    /// <summary>单位颜色。</summary>
    public IBrush? UnitBrush
    {
        get => GetValue(UnitBrushProperty);
        set => SetValue(UnitBrushProperty, value);
    }

    /// <summary>单位字体。</summary>
    public FontFamily? UnitFontFamily
    {
        get => GetValue(UnitFontFamilyProperty);
        set => SetValue(UnitFontFamilyProperty, value);
    }

    /// <summary>单位相对表盘中心的偏移（像素）。</summary>
    public Point UnitOffset
    {
        get => GetValue(UnitOffsetProperty);
        set => SetValue(UnitOffsetProperty, value);
    }

    /// <summary>起始角度（度，0° 为 3 点钟方向，顺时针为正）。</summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>扫过角度（度，顺时针为正）。</summary>
    public double SweepAngle
    {
        get => GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    /// <summary>刻度盘轨道粗度。</summary>
    public double TrackThickness
    {
        get => GetValue(TrackThicknessProperty);
        set => SetValue(TrackThicknessProperty, value);
    }

    /// <summary>刻度盘轨道颜色。</summary>
    public IBrush? TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    /// <summary>是否绘制进度弧。</summary>
    public bool ShowProgress
    {
        get => GetValue(ShowProgressProperty);
        set => SetValue(ShowProgressProperty, value);
    }

    /// <summary>进度弧颜色。</summary>
    public IBrush? ProgressBrush
    {
        get => GetValue(ProgressBrushProperty);
        set => SetValue(ProgressBrushProperty, value);
    }

    /// <summary>进度弧是否带柔光。</summary>
    public bool ProgressGlow
    {
        get => GetValue(ProgressGlowProperty);
        set => SetValue(ProgressGlowProperty, value);
    }

    /// <summary>是否显示刻度线。</summary>
    public bool ShowTickMarks
    {
        get => GetValue(ShowTickMarksProperty);
        set => SetValue(ShowTickMarksProperty, value);
    }

    /// <summary>主刻度数量（包含量程两端）。</summary>
    public int MajorTickCount
    {
        get => GetValue(MajorTickCountProperty);
        set => SetValue(MajorTickCountProperty, value);
    }

    /// <summary>每个主刻度之间的副刻度数量。</summary>
    public int MinorTicksPerMajor
    {
        get => GetValue(MinorTicksPerMajorProperty);
        set => SetValue(MinorTicksPerMajorProperty, value);
    }

    /// <summary>主刻度长度。</summary>
    public double MajorTickLength
    {
        get => GetValue(MajorTickLengthProperty);
        set => SetValue(MajorTickLengthProperty, value);
    }

    /// <summary>副刻度长度。</summary>
    public double MinorTickLength
    {
        get => GetValue(MinorTickLengthProperty);
        set => SetValue(MinorTickLengthProperty, value);
    }

    /// <summary>主刻度线宽。</summary>
    public double MajorTickWidth
    {
        get => GetValue(MajorTickWidthProperty);
        set => SetValue(MajorTickWidthProperty, value);
    }

    /// <summary>副刻度线宽。</summary>
    public double MinorTickWidth
    {
        get => GetValue(MinorTickWidthProperty);
        set => SetValue(MinorTickWidthProperty, value);
    }

    /// <summary>主刻度颜色。</summary>
    public IBrush? MajorTickBrush
    {
        get => GetValue(MajorTickBrushProperty);
        set => SetValue(MajorTickBrushProperty, value);
    }

    /// <summary>副刻度颜色。</summary>
    public IBrush? MinorTickBrush
    {
        get => GetValue(MinorTickBrushProperty);
        set => SetValue(MinorTickBrushProperty, value);
    }

    /// <summary>是否显示刻度数字。</summary>
    public bool ShowScaleLabels
    {
        get => GetValue(ShowScaleLabelsProperty);
        set => SetValue(ShowScaleLabelsProperty, value);
    }

    /// <summary>刻度数字格式。</summary>
    public string? ScaleLabelFormat
    {
        get => GetValue(ScaleLabelFormatProperty);
        set => SetValue(ScaleLabelFormatProperty, value);
    }

    /// <summary>刻度数字字号。</summary>
    public double ScaleLabelFontSize
    {
        get => GetValue(ScaleLabelFontSizeProperty);
        set => SetValue(ScaleLabelFontSizeProperty, value);
    }

    /// <summary>刻度数字颜色。</summary>
    public IBrush? ScaleLabelBrush
    {
        get => GetValue(ScaleLabelBrushProperty);
        set => SetValue(ScaleLabelBrushProperty, value);
    }

    /// <summary>刻度数字字体。</summary>
    public FontFamily? ScaleLabelFontFamily
    {
        get => GetValue(ScaleLabelFontFamilyProperty);
        set => SetValue(ScaleLabelFontFamilyProperty, value);
    }

    /// <summary>刻度数字相对刻度线内端的径向间距。</summary>
    public double ScaleLabelOffset
    {
        get => GetValue(ScaleLabelOffsetProperty);
        set => SetValue(ScaleLabelOffsetProperty, value);
    }

    /// <summary>是否显示指针。</summary>
    public bool ShowNeedle
    {
        get => GetValue(ShowNeedleProperty);
        set => SetValue(ShowNeedleProperty, value);
    }

    /// <summary>指针形状。</summary>
    public NeedleShape NeedleShape
    {
        get => GetValue(NeedleShapeProperty);
        set => SetValue(NeedleShapeProperty, value);
    }

    /// <summary>指针长度比例（0~1）。</summary>
    public double NeedleLength
    {
        get => GetValue(NeedleLengthProperty);
        set => SetValue(NeedleLengthProperty, value);
    }

    /// <summary>指针宽度。</summary>
    public double NeedleWidth
    {
        get => GetValue(NeedleWidthProperty);
        set => SetValue(NeedleWidthProperty, value);
    }

    /// <summary>指针尾部长度比例。</summary>
    public double NeedleBackLength
    {
        get => GetValue(NeedleBackLengthProperty);
        set => SetValue(NeedleBackLengthProperty, value);
    }

    /// <summary>指针颜色。</summary>
    public IBrush? NeedleBrush
    {
        get => GetValue(NeedleBrushProperty);
        set => SetValue(NeedleBrushProperty, value);
    }

    /// <summary>指针轴心半径。</summary>
    public double NeedleHubRadius
    {
        get => GetValue(NeedleHubRadiusProperty);
        set => SetValue(NeedleHubRadiusProperty, value);
    }

    /// <summary>指针轴心颜色。</summary>
    public IBrush? NeedleHubBrush
    {
        get => GetValue(NeedleHubBrushProperty);
        set => SetValue(NeedleHubBrushProperty, value);
    }

    /// <summary>表盘外圈色带粗度。</summary>
    public double RangeThickness
    {
        get => GetValue(RangeThicknessProperty);
        set => SetValue(RangeThicknessProperty, value);
    }

    /// <summary>相邻色带之间的角度间隔（度）。</summary>
    public double RangeGap
    {
        get => GetValue(RangeGapProperty);
        set => SetValue(RangeGapProperty, value);
    }

    private void OnRangesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (GaugeRange range in _subscribedRanges)
        {
            range.PropertyChanged -= OnRangePropertyChanged;
        }

        _subscribedRanges.Clear();
        foreach (GaugeRange? range in Ranges)
        {
            if (range is null)
            {
                continue;
            }

            range.PropertyChanged += OnRangePropertyChanged;
            _subscribedRanges.Add(range);
        }

        InvalidateStatic();
    }

    private void OnRangePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) => InvalidateStatic();

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

        if (TrackBrush is not null && TrackThickness > 0 && _trackPen is not null)
        {
            if (fullCircle)
            {
                context.DrawEllipse(null, _trackPen, _center, _trackRadius, _trackRadius);
            }
            else if (_trackGeometry is not null)
            {
                context.DrawGeometry(null, _trackPen, _trackGeometry);
            }
        }

        if (ShowProgress && normalized > 0.0005 && _progressPen is not null)
        {
            if (fullCircle && normalized >= 0.9995)
            {
                context.DrawEllipse(null, _progressPen, _center, _trackRadius, _trackRadius);
            }
            else if (_progressArc is not null)
            {
                // 复用同一个几何对象，每帧只更新端点，避免分配
                _progressArc.Update(_center, _trackRadius, StartAngle, SweepAngle * normalized);
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

        foreach (RangeVisual visual in _rangeVisuals)
        {
            context.DrawGeometry(null, visual.Pen, visual.Geometry);
        }

        if (ShowTickMarks)
        {
            if (_minorTicksGeometry is not null && _minorTickPen is not null)
            {
                context.DrawGeometry(null, _minorTickPen, _minorTicksGeometry);
            }

            if (_majorTicksGeometry is not null && _majorTickPen is not null)
            {
                context.DrawGeometry(null, _majorTickPen, _majorTicksGeometry);
            }
        }

        if (ShowScaleLabels && _scaleLabels is not null && _scaleLabelPositions is not null)
        {
            for (int i = 0; i < _scaleLabels.Length; i++)
            {
                if (_scaleLabels[i] is null)
                {
                    continue;
                }

                DrawHelper.DrawTextCentered(context, _scaleLabels[i], _scaleLabelPositions[i]);
            }
        }

        if (_titleText is not null)
        {
            DrawHelper.DrawTextCentered(context, _titleText, _center + TitleOffset);
        }

        if (_unitText is not null)
        {
            DrawHelper.DrawTextCentered(context, _unitText, _center + UnitOffset);
        }

        if (ShowNeedle)
        {
            DrawNeedle(context);
        }

        if (ShowValueText && ValueBrush is not null)
        {
            DrawValueText(context);
        }
    }

    private void DrawNeedle(DrawingContext context)
    {
        if (NeedleBrush is not null)
        {
            double angle = DrawHelper.AngleForValue(Value, Minimum, Maximum, StartAngle, SweepAngle);
            double radians = angle * Math.PI / 180.0;

            // 指针几何按“指向 3 点钟方向”缓存，绘制时只做旋转 + 平移，避免每帧分配几何
            using (context.PushTransform(
                       Matrix.CreateRotation(radians) * Matrix.CreateTranslation(_center.X, _center.Y)))
            {
                if (NeedleShape == NeedleShape.Line)
                {
                    if (_needleLinePen is not null)
                    {
                        double tipLength = _needleRadius * Math.Clamp(NeedleLength, 0.05, 1.0);
                        double backLength = _needleRadius * Math.Clamp(NeedleBackLength, 0, 0.8);
                        context.DrawLine(_needleLinePen, new Point(-backLength, 0), new Point(tipLength, 0));
                    }
                }
                else if (_needleGeometry is not null)
                {
                    context.DrawGeometry(NeedleBrush, null, _needleGeometry);
                }
            }
        }

        if (NeedleHubRadius > 0)
        {
            context.DrawEllipse(NeedleHubBrush, null, _center, NeedleHubRadius, NeedleHubRadius);

            if (NeedleBrush is not null)
            {
                double inner = NeedleHubRadius * 0.34;
                context.DrawEllipse(NeedleBrush, null, _center, inner, inner);
            }
        }
    }

    private void BuildNeedle()
    {
        _needleGeometry = null;
        if (NeedleShape == NeedleShape.Line)
        {
            return;
        }

        double tipLength = _needleRadius * Math.Clamp(NeedleLength, 0.05, 1.0);
        double backLength = _needleRadius * Math.Clamp(NeedleBackLength, 0, 0.8);
        double halfWidth = Math.Max(0.5, NeedleWidth / 2);

        if (NeedleShape == NeedleShape.Triangle)
        {
            _needleGeometry = DrawHelper.CreatePolygon(new[]
            {
                new Point(-backLength, halfWidth),
                new Point(-backLength, -halfWidth),
                new Point(tipLength, 0),
            });
            return;
        }

        _needleGeometry = DrawHelper.CreatePolygon(new[]
        {
            new Point(-backLength, 0),
            new Point(0, halfWidth),
            new Point(tipLength, 0),
            new Point(0, -halfWidth),
        });
    }

    private void DrawValueText(DrawingContext context)
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

    private void RebuildStatic(Size size)
    {
        double outerRadius = Math.Max(1, Math.Min(size.Width, size.Height) / 2 - 2);
        _center = new Point(size.Width / 2, size.Height / 2);

        double trackThickness = Math.Max(0.5, TrackThickness);
        double rangeThickness = Math.Max(0.5, RangeThickness);
        bool hasRanges = Ranges.Count > 0 && RangeThickness > 0;
        double rangeRadius = outerRadius - rangeThickness / 2;
        double trackRadius = hasRanges
            ? rangeRadius - rangeThickness / 2 - 5 - trackThickness / 2
            : outerRadius - 5 - trackThickness / 2;

        _trackRadius = Math.Max(1, trackRadius);
        _tickOuterRadius = Math.Max(2, _trackRadius - trackThickness / 2 - 6);
        _labelRadius = Math.Max(2, _tickOuterRadius - Math.Max(0, MajorTickLength) - ScaleLabelOffset);
        _needleRadius = Math.Max(2, _tickOuterRadius - Math.Max(MajorTickLength, MinorTickLength) - 3);

        _trackPen = TrackBrush is null ? null : DrawHelper.CreatePen(TrackBrush, trackThickness, PenLineCap.Flat);
        _progressPen = ProgressBrush is null ? null : DrawHelper.CreatePen(ProgressBrush, trackThickness, PenLineCap.Round);
        _glowPen = ProgressBrush is null
            ? null
            : DrawHelper.CreatePen(ProgressBrush, trackThickness + Math.Max(6, trackThickness * 0.9), PenLineCap.Round);
        _majorTickPen = MajorTickBrush is null
            ? null
            : DrawHelper.CreatePen(MajorTickBrush, Math.Max(0.5, MajorTickWidth), PenLineCap.Round);
        _minorTickPen = MinorTickBrush is null
            ? null
            : DrawHelper.CreatePen(MinorTickBrush, Math.Max(0.5, MinorTickWidth), PenLineCap.Round);
        _needleLinePen = NeedleBrush is null
            ? null
            : DrawHelper.CreatePen(NeedleBrush, Math.Max(1, NeedleWidth), PenLineCap.Round, PenLineJoin.Round);

        _progressArc = new MutableArc(_trackRadius);

        _trackGeometry = DrawHelper.IsFullCircle(SweepAngle)
            ? null
            : DrawHelper.CreateArc(_center, _trackRadius, StartAngle, SweepAngle);

        BuildTicks();
        BuildRanges(rangeRadius, rangeThickness);
        BuildScaleLabels();
        BuildNeedle();

        _titleText = DrawHelper.CreateText(Title, TitleFontFamily, FontWeight.Normal, TitleFontSize, TitleBrush);
        _unitText = DrawHelper.CreateText(Unit, UnitFontFamily, FontWeight.Normal, UnitFontSize, UnitBrush);

        _valueText = null;
        _valueTextContent = null;
    }

    private void BuildTicks()
    {
        _majorTicksGeometry = null;
        _minorTicksGeometry = null;

        int majorCount = Math.Clamp(MajorTickCount, 0, 200);
        if (!ShowTickMarks || majorCount < 2 || Math.Abs(SweepAngle) < 0.01)
        {
            return;
        }

        int minors = Math.Clamp(MinorTicksPerMajor, 0, 50);
        double majorLength = Math.Max(0, MajorTickLength);
        double minorLength = Math.Max(0, MinorTickLength);

        var majorGeometry = new StreamGeometry();
        var minorGeometry = new StreamGeometry();

        using (StreamGeometryContext majorContext = majorGeometry.Open())
        using (StreamGeometryContext minorContext = minorGeometry.Open())
        {
            double step = SweepAngle / (majorCount - 1);
            for (int i = 0; i < majorCount; i++)
            {
                double angle = StartAngle + step * i;
                AddTick(majorContext, angle, majorLength);

                if (i == majorCount - 1 || minors == 0)
                {
                    continue;
                }

                for (int j = 1; j <= minors; j++)
                {
                    double minorAngle = angle + step * j / (minors + 1);
                    AddTick(minorContext, minorAngle, minorLength);
                }
            }
        }

        _majorTicksGeometry = majorGeometry;
        _minorTicksGeometry = minorGeometry;
    }

    private void AddTick(StreamGeometryContext context, double angle, double length)
    {
        if (length <= 0.01)
        {
            return;
        }

        Point start = DrawHelper.PolarPoint(_center, _tickOuterRadius, angle);
        Point end = DrawHelper.PolarPoint(_center, _tickOuterRadius - length, angle);
        context.BeginFigure(start, false);
        context.LineTo(end);
        context.EndFigure(false);
    }

    private void BuildRanges(double rangeRadius, double rangeThickness)
    {
        _rangeVisuals.Clear();
        // 反向量程下 Math.Clamp 会抛异常，直接跳过色带
        if (Ranges.Count == 0 || RangeThickness <= 0 || Math.Abs(SweepAngle) < 0.01 || Maximum <= Minimum)
        {
            return;
        }

        double gap = Math.Max(0, RangeGap);

        foreach (GaugeRange? range in Ranges)
        {
            if (range is null)
            {
                continue;
            }

            double from = Math.Clamp(range.From, Minimum, Maximum);
            double to = Math.Clamp(range.To, Minimum, Maximum);
            if (to <= from)
            {
                continue;
            }

            // 逆时针表盘（SweepAngle < 0）下角度递减，先排序再留间隙
            double angleFrom = DrawHelper.AngleForValue(from, Minimum, Maximum, StartAngle, SweepAngle);
            double angleTo = DrawHelper.AngleForValue(to, Minimum, Maximum, StartAngle, SweepAngle);
            double start = Math.Min(angleFrom, angleTo) + gap / 2;
            double end = Math.Max(angleFrom, angleTo) - gap / 2;
            if (end - start < 0.2)
            {
                continue;
            }

            IBrush brush = range.Brush ?? MajorTickBrush ?? IndustrialPalette.Green;
            double thickness = double.IsFinite(range.Thickness) && range.Thickness > 0
                ? range.Thickness
                : rangeThickness;

            StreamGeometry geometry = DrawHelper.CreateArc(_center, rangeRadius, start, end - start);
            _rangeVisuals.Add(new RangeVisual(geometry, DrawHelper.CreatePen(brush, thickness, PenLineCap.Round)));
        }
    }

    private void BuildScaleLabels()
    {
        _scaleLabels = null;
        _scaleLabelPositions = null;

        int count = Math.Clamp(MajorTickCount, 0, 200);
        if (!ShowScaleLabels || count < 2 || ScaleLabelBrush is null || ScaleLabelFontSize <= 0)
        {
            return;
        }

        var labels = new FormattedText[count];
        var positions = new Point[count];
        for (int i = 0; i < count; i++)
        {
            double ratio = (double)i / (count - 1);
            double value = Minimum + (Maximum - Minimum) * ratio;
            string text = DrawHelper.FormatValue(value, ScaleLabelFormat);
            FormattedText? formatted = DrawHelper.CreateText(
                text, ScaleLabelFontFamily, FontWeight.Normal, ScaleLabelFontSize, ScaleLabelBrush);

            if (formatted is not null)
            {
                labels[i] = formatted;
            }

            // 位置随尺寸/角度固定，预先算好，绘制时不再重复三角函数
            positions[i] = DrawHelper.PolarPoint(_center, _labelRadius, StartAngle + SweepAngle * ratio);
        }

        _scaleLabels = labels;
        _scaleLabelPositions = positions;
    }
}
