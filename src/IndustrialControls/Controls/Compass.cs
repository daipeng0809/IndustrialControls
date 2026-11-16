using Avalonia;
using Avalonia.Media;
using IndustrialControls.Primitives;

namespace IndustrialControls.Controls;

/// <summary>
/// 指南针/风向盘：Value 表示航向角（0° 为正北，顺时针），可显示方位文字与角度。
/// </summary>
/// <remarks>
/// 用于表示风向时，通常把“风从哪个方向吹来”的角度写入 <see cref="InstrumentBase.Value"/>（例如 45 表示东北风）。
/// </remarks>
public class Compass : InstrumentBase
{
    /// <summary>是否显示东南西北方位文字。</summary>
    public static readonly StyledProperty<bool> ShowCardinalLabelsProperty =
        AvaloniaProperty.Register<Compass, bool>(nameof(ShowCardinalLabels), true);

    /// <summary>方位文字是否使用中文（北/东北/东……）。</summary>
    public static readonly StyledProperty<bool> UseChineseCardinalLabelsProperty =
        AvaloniaProperty.Register<Compass, bool>(nameof(UseChineseCardinalLabels));

    /// <summary>方位文字字号。</summary>
    public static readonly StyledProperty<double> CardinalLabelFontSizeProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(CardinalLabelFontSize), 13d);

    /// <summary>方位文字颜色。</summary>
    public static readonly StyledProperty<IBrush?> CardinalLabelBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(CardinalLabelBrush), IndustrialPalette.Text);

    /// <summary>方位文字字体。</summary>
    public static readonly StyledProperty<FontFamily?> CardinalLabelFontFamilyProperty =
        AvaloniaProperty.Register<Compass, FontFamily?>(nameof(CardinalLabelFontFamily));

    /// <summary>圆周刻度数量（默认 72，即每 5° 一格）。</summary>
    public static readonly StyledProperty<int> TickCountProperty =
        AvaloniaProperty.Register<Compass, int>(nameof(TickCount), 72);

    /// <summary>每隔几个刻度绘制一个主刻度（默认 6，即每 30°）。0 表示全部为副刻度。</summary>
    public static readonly StyledProperty<int> MajorTickEveryProperty =
        AvaloniaProperty.Register<Compass, int>(nameof(MajorTickEvery), 6);

    /// <summary>主刻度长度。</summary>
    public static readonly StyledProperty<double> MajorTickLengthProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(MajorTickLength), 12d);

    /// <summary>副刻度长度。</summary>
    public static readonly StyledProperty<double> MinorTickLengthProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(MinorTickLength), 6d);

    /// <summary>主刻度线宽。</summary>
    public static readonly StyledProperty<double> MajorTickWidthProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(MajorTickWidth), 2d);

    /// <summary>副刻度线宽。</summary>
    public static readonly StyledProperty<double> MinorTickWidthProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(MinorTickWidth), 1d);

    /// <summary>主刻度颜色。</summary>
    public static readonly StyledProperty<IBrush?> MajorTickBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(MajorTickBrush), IndustrialPalette.MajorTick);

    /// <summary>副刻度颜色。</summary>
    public static readonly StyledProperty<IBrush?> MinorTickBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(MinorTickBrush), IndustrialPalette.Tick);

    /// <summary>表盘底色。</summary>
    public static readonly StyledProperty<IBrush?> DialBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(DialBrush), IndustrialPalette.Track);

    /// <summary>外圈颜色。</summary>
    public static readonly StyledProperty<IBrush?> BezelBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(BezelBrush), IndustrialPalette.Dim);

    /// <summary>外圈粗度。</summary>
    public static readonly StyledProperty<double> BezelThicknessProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(BezelThickness), 2d);

    /// <summary>是否显示指针。</summary>
    public static readonly StyledProperty<bool> ShowNeedleProperty =
        AvaloniaProperty.Register<Compass, bool>(nameof(ShowNeedle), true);

    /// <summary>指北针颜色。</summary>
    public static readonly StyledProperty<IBrush?> NeedleBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(NeedleBrush), IndustrialPalette.Needle);

    /// <summary>指南针颜色。</summary>
    public static readonly StyledProperty<IBrush?> SouthNeedleBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(SouthNeedleBrush), IndustrialPalette.Muted);

    /// <summary>指针长度比例（0~1，相对于表盘半径）。</summary>
    public static readonly StyledProperty<double> NeedleLengthProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(NeedleLength), 0.62d);

    /// <summary>指针宽度。</summary>
    public static readonly StyledProperty<double> NeedleWidthProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(NeedleWidth), 6d);

    /// <summary>指针轴心半径。</summary>
    public static readonly StyledProperty<double> NeedleHubRadiusProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(NeedleHubRadius), 8d);

    /// <summary>指针轴心颜色。</summary>
    public static readonly StyledProperty<IBrush?> NeedleHubBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(NeedleHubBrush), IndustrialPalette.Hub);

    /// <summary>是否显示角度文字。</summary>
    public static readonly StyledProperty<bool> ShowHeadingTextProperty =
        AvaloniaProperty.Register<Compass, bool>(nameof(ShowHeadingText), true);

    /// <summary>角度格式。</summary>
    public static readonly StyledProperty<string?> HeadingFormatProperty =
        AvaloniaProperty.Register<Compass, string?>(nameof(HeadingFormat), "F0");

    /// <summary>是否在角度后显示 °。</summary>
    public static readonly StyledProperty<bool> ShowDegreeSymbolProperty =
        AvaloniaProperty.Register<Compass, bool>(nameof(ShowDegreeSymbol), true);

    /// <summary>角度文字字号。</summary>
    public static readonly StyledProperty<double> HeadingFontSizeProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(HeadingFontSize), 22d);

    /// <summary>角度文字颜色。</summary>
    public static readonly StyledProperty<IBrush?> HeadingBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(HeadingBrush), IndustrialPalette.Text);

    /// <summary>角度文字字体。</summary>
    public static readonly StyledProperty<FontFamily?> HeadingFontFamilyProperty =
        AvaloniaProperty.Register<Compass, FontFamily?>(nameof(HeadingFontFamily));

    /// <summary>角度文字字重。</summary>
    public static readonly StyledProperty<FontWeight> HeadingFontWeightProperty =
        AvaloniaProperty.Register<Compass, FontWeight>(nameof(HeadingFontWeight), FontWeight.Bold);

    /// <summary>角度文字相对圆心偏移。</summary>
    public static readonly StyledProperty<Point> HeadingOffsetProperty =
        AvaloniaProperty.Register<Compass, Point>(nameof(HeadingOffset), new Point(0, 42));

    /// <summary>是否显示方位名称（如“东北”）。</summary>
    public static readonly StyledProperty<bool> ShowDirectionTextProperty =
        AvaloniaProperty.Register<Compass, bool>(nameof(ShowDirectionText), true);

    /// <summary>方位名称字号。</summary>
    public static readonly StyledProperty<double> DirectionFontSizeProperty =
        AvaloniaProperty.Register<Compass, double>(nameof(DirectionFontSize), 12d);

    /// <summary>方位名称颜色。</summary>
    public static readonly StyledProperty<IBrush?> DirectionBrushProperty =
        AvaloniaProperty.Register<Compass, IBrush?>(nameof(DirectionBrush), IndustrialPalette.Muted);

    /// <summary>方位名称字体。</summary>
    public static readonly StyledProperty<FontFamily?> DirectionFontFamilyProperty =
        AvaloniaProperty.Register<Compass, FontFamily?>(nameof(DirectionFontFamily));

    /// <summary>方位名称相对圆心偏移。</summary>
    public static readonly StyledProperty<Point> DirectionOffsetProperty =
        AvaloniaProperty.Register<Compass, Point>(nameof(DirectionOffset), new Point(0, 66));

    private static readonly string[] EnglishDirections = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
    private static readonly string[] ChineseDirections = { "北", "东北", "东", "东南", "南", "西南", "西", "西北" };

    private Point _center;
    private double _innerRadius;
    private Pen? _bezelPen;
    private Pen? _majorTickPen;
    private Pen? _minorTickPen;
    private StreamGeometry? _majorTicksGeometry;
    private StreamGeometry? _minorTicksGeometry;
    private StreamGeometry? _northNeedleGeometry;
    private StreamGeometry? _southNeedleGeometry;
    private FormattedText[]? _cardinalLabels;
    private Point[]? _cardinalLabelPositions;
    private FormattedText? _headingText;
    private string? _headingContent;
    private double _headingTextSize;
    private FormattedText? _directionText;
    private string? _directionContent;
    private double _directionTextSize;

    static Compass()
    {
        MaximumProperty.OverrideDefaultValue<Compass>(360d);
    }

    /// <summary>是否显示东南西北方位文字。</summary>
    public bool ShowCardinalLabels
    {
        get => GetValue(ShowCardinalLabelsProperty);
        set => SetValue(ShowCardinalLabelsProperty, value);
    }

    /// <summary>方位文字是否使用中文。</summary>
    public bool UseChineseCardinalLabels
    {
        get => GetValue(UseChineseCardinalLabelsProperty);
        set => SetValue(UseChineseCardinalLabelsProperty, value);
    }

    /// <summary>方位文字字号。</summary>
    public double CardinalLabelFontSize
    {
        get => GetValue(CardinalLabelFontSizeProperty);
        set => SetValue(CardinalLabelFontSizeProperty, value);
    }

    /// <summary>方位文字颜色。</summary>
    public IBrush? CardinalLabelBrush
    {
        get => GetValue(CardinalLabelBrushProperty);
        set => SetValue(CardinalLabelBrushProperty, value);
    }

    /// <summary>方位文字字体。</summary>
    public FontFamily? CardinalLabelFontFamily
    {
        get => GetValue(CardinalLabelFontFamilyProperty);
        set => SetValue(CardinalLabelFontFamilyProperty, value);
    }

    /// <summary>圆周刻度数量。</summary>
    public int TickCount
    {
        get => GetValue(TickCountProperty);
        set => SetValue(TickCountProperty, value);
    }

    /// <summary>每隔几个刻度绘制一个主刻度。</summary>
    public int MajorTickEvery
    {
        get => GetValue(MajorTickEveryProperty);
        set => SetValue(MajorTickEveryProperty, value);
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

    /// <summary>表盘底色。</summary>
    public IBrush? DialBrush
    {
        get => GetValue(DialBrushProperty);
        set => SetValue(DialBrushProperty, value);
    }

    /// <summary>外圈颜色。</summary>
    public IBrush? BezelBrush
    {
        get => GetValue(BezelBrushProperty);
        set => SetValue(BezelBrushProperty, value);
    }

    /// <summary>外圈粗度。</summary>
    public double BezelThickness
    {
        get => GetValue(BezelThicknessProperty);
        set => SetValue(BezelThicknessProperty, value);
    }

    /// <summary>是否显示指针。</summary>
    public bool ShowNeedle
    {
        get => GetValue(ShowNeedleProperty);
        set => SetValue(ShowNeedleProperty, value);
    }

    /// <summary>指北针颜色。</summary>
    public IBrush? NeedleBrush
    {
        get => GetValue(NeedleBrushProperty);
        set => SetValue(NeedleBrushProperty, value);
    }

    /// <summary>指南针颜色。</summary>
    public IBrush? SouthNeedleBrush
    {
        get => GetValue(SouthNeedleBrushProperty);
        set => SetValue(SouthNeedleBrushProperty, value);
    }

    /// <summary>指针长度比例。</summary>
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

    /// <summary>是否显示角度文字。</summary>
    public bool ShowHeadingText
    {
        get => GetValue(ShowHeadingTextProperty);
        set => SetValue(ShowHeadingTextProperty, value);
    }

    /// <summary>角度格式。</summary>
    public string? HeadingFormat
    {
        get => GetValue(HeadingFormatProperty);
        set => SetValue(HeadingFormatProperty, value);
    }

    /// <summary>是否在角度后显示 °。</summary>
    public bool ShowDegreeSymbol
    {
        get => GetValue(ShowDegreeSymbolProperty);
        set => SetValue(ShowDegreeSymbolProperty, value);
    }

    /// <summary>角度文字字号。</summary>
    public double HeadingFontSize
    {
        get => GetValue(HeadingFontSizeProperty);
        set => SetValue(HeadingFontSizeProperty, value);
    }

    /// <summary>角度文字颜色。</summary>
    public IBrush? HeadingBrush
    {
        get => GetValue(HeadingBrushProperty);
        set => SetValue(HeadingBrushProperty, value);
    }

    /// <summary>角度文字字体。</summary>
    public FontFamily? HeadingFontFamily
    {
        get => GetValue(HeadingFontFamilyProperty);
        set => SetValue(HeadingFontFamilyProperty, value);
    }

    /// <summary>角度文字字重。</summary>
    public FontWeight HeadingFontWeight
    {
        get => GetValue(HeadingFontWeightProperty);
        set => SetValue(HeadingFontWeightProperty, value);
    }

    /// <summary>角度文字相对圆心偏移。</summary>
    public Point HeadingOffset
    {
        get => GetValue(HeadingOffsetProperty);
        set => SetValue(HeadingOffsetProperty, value);
    }

    /// <summary>是否显示方位名称。</summary>
    public bool ShowDirectionText
    {
        get => GetValue(ShowDirectionTextProperty);
        set => SetValue(ShowDirectionTextProperty, value);
    }

    /// <summary>方位名称字号。</summary>
    public double DirectionFontSize
    {
        get => GetValue(DirectionFontSizeProperty);
        set => SetValue(DirectionFontSizeProperty, value);
    }

    /// <summary>方位名称颜色。</summary>
    public IBrush? DirectionBrush
    {
        get => GetValue(DirectionBrushProperty);
        set => SetValue(DirectionBrushProperty, value);
    }

    /// <summary>方位名称字体。</summary>
    public FontFamily? DirectionFontFamily
    {
        get => GetValue(DirectionFontFamilyProperty);
        set => SetValue(DirectionFontFamilyProperty, value);
    }

    /// <summary>方位名称相对圆心偏移。</summary>
    public Point DirectionOffset
    {
        get => GetValue(DirectionOffsetProperty);
        set => SetValue(DirectionOffsetProperty, value);
    }

    /// <summary>当前航向角（0~360，正北为 0，顺时针）。</summary>
    public double Heading => NormalizeHeading(Value);

    /// <inheritdoc />
    protected override Size DefaultSize => new(200, 200);

    /// <inheritdoc />
    protected override void RenderCore(DrawingContext context)
    {
        Size size = Bounds.Size;
        if (size.Width < 16 || size.Height < 16)
        {
            return;
        }

        if (IsStaticCacheInvalid(size))
        {
            RebuildStatic(size);
            MarkStaticCacheBuilt(size);
        }

        double bezel = Math.Clamp(BezelThickness, 0, _innerRadius * 0.2);
        double dialRadius = Math.Max(1, _innerRadius - bezel);

        if (DialBrush is not null)
        {
            context.DrawEllipse(DialBrush, null, _center, dialRadius, dialRadius);
        }

        if (_bezelPen is not null)
        {
            context.DrawEllipse(null, _bezelPen, _center, dialRadius + bezel / 2, dialRadius + bezel / 2);
        }

        if (_minorTicksGeometry is not null && _minorTickPen is not null)
        {
            context.DrawGeometry(null, _minorTickPen, _minorTicksGeometry);
        }

        if (_majorTicksGeometry is not null && _majorTickPen is not null)
        {
            context.DrawGeometry(null, _majorTickPen, _majorTicksGeometry);
        }

        if (ShowCardinalLabels && _cardinalLabels is not null && _cardinalLabelPositions is not null)
        {
            for (int i = 0; i < _cardinalLabels.Length; i++)
            {
                if (_cardinalLabels[i] is null)
                {
                    continue;
                }

                DrawHelper.DrawTextCentered(context, _cardinalLabels[i], _cardinalLabelPositions[i]);
            }
        }

        if (ShowNeedle)
        {
            DrawNeedle(context);
        }

        if (ShowHeadingText && HeadingBrush is not null && HeadingFontSize > 0)
        {
            string content = DrawHelper.FormatValue(Heading, HeadingFormat) + (ShowDegreeSymbol ? "°" : string.Empty);
            if (_headingText is null
                || !string.Equals(content, _headingContent, StringComparison.Ordinal)
                || _headingTextSize != HeadingFontSize)
            {
                _headingText = DrawHelper.CreateText(
                    content, HeadingFontFamily, HeadingFontWeight, HeadingFontSize, HeadingBrush);
                _headingContent = content;
                _headingTextSize = HeadingFontSize;
            }

            if (_headingText is not null)
            {
                DrawHelper.DrawTextCentered(context, _headingText, _center + HeadingOffset);
            }
        }

        if (ShowDirectionText && DirectionBrush is not null && DirectionFontSize > 0)
        {
            string content = GetDirectionName(Heading);
            if (_directionText is null
                || !string.Equals(content, _directionContent, StringComparison.Ordinal)
                || _directionTextSize != DirectionFontSize)
            {
                _directionText = DrawHelper.CreateText(
                    content, DirectionFontFamily, FontWeight.Normal, DirectionFontSize, DirectionBrush);
                _directionContent = content;
                _directionTextSize = DirectionFontSize;
            }

            if (_directionText is not null)
            {
                DrawHelper.DrawTextCentered(context, _directionText, _center + DirectionOffset);
            }
        }
    }

    private void DrawNeedle(DrawingContext context)
    {
        double angle = NormalizeHeading(Value) - 90;
        double radians = angle * Math.PI / 180.0;

        // 指针几何按“指向 3 点钟方向”缓存，绘制时只做旋转 + 平移，避免每帧分配几何
        using (context.PushTransform(
                   Matrix.CreateRotation(radians) * Matrix.CreateTranslation(_center.X, _center.Y)))
        {
            if (NeedleBrush is not null && _northNeedleGeometry is not null)
            {
                context.DrawGeometry(NeedleBrush, null, _northNeedleGeometry);
            }

            if (SouthNeedleBrush is not null && _southNeedleGeometry is not null)
            {
                context.DrawGeometry(SouthNeedleBrush, null, _southNeedleGeometry);
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
        _northNeedleGeometry = null;
        _southNeedleGeometry = null;

        double length = Math.Max(4, _innerRadius * Math.Clamp(NeedleLength, 0.1, 1.0));
        double halfWidth = Math.Max(1, NeedleWidth / 2);
        double southLength = length * 0.55;

        _northNeedleGeometry = DrawHelper.CreatePolygon(new[]
        {
            new Point(length, 0),
            new Point(0, halfWidth),
            new Point(0, -halfWidth),
        });

        _southNeedleGeometry = DrawHelper.CreatePolygon(new[]
        {
            new Point(-southLength, 0),
            new Point(0, halfWidth * 0.72),
            new Point(0, -halfWidth * 0.72),
        });
    }

    private static double NormalizeHeading(double heading)
    {
        double normalized = heading % 360;
        return normalized < 0 ? normalized + 360 : normalized;
    }

    private string GetDirectionName(double heading)
    {
        int index = (int)Math.Round(NormalizeHeading(heading) / 45.0, MidpointRounding.AwayFromZero) % 8;
        return UseChineseCardinalLabels ? ChineseDirections[index] : EnglishDirections[index];
    }

    private void RebuildStatic(Size size)
    {
        _center = new Point(size.Width / 2, size.Height / 2);
        _innerRadius = Math.Max(8, Math.Min(size.Width, size.Height) / 2 - 2);
        double bezel = Math.Clamp(BezelThickness, 0, _innerRadius * 0.2);
        double dialRadius = Math.Max(1, _innerRadius - bezel);

        _bezelPen = bezel > 0 && BezelBrush is not null ? DrawHelper.CreatePen(BezelBrush, bezel) : null;
        _majorTickPen = MajorTickBrush is null
            ? null
            : DrawHelper.CreatePen(MajorTickBrush, Math.Max(0.5, MajorTickWidth), PenLineCap.Round);
        _minorTickPen = MinorTickBrush is null
            ? null
            : DrawHelper.CreatePen(MinorTickBrush, Math.Max(0.5, MinorTickWidth), PenLineCap.Round);

        BuildTicks(dialRadius);
        BuildCardinalLabels();
        BuildNeedle();

        _headingText = null;
        _headingContent = null;
        _directionText = null;
        _directionContent = null;
    }

    private void BuildTicks(double dialRadius)
    {
        _majorTicksGeometry = null;
        _minorTicksGeometry = null;

        int count = Math.Clamp(TickCount, 0, 720);
        if (count < 2)
        {
            return;
        }

        double outer = Math.Max(4, dialRadius - 6);
        int majorEvery = Math.Clamp(MajorTickEvery, 0, 720);
        double majorLength = Math.Max(0, MajorTickLength);
        double minorLength = Math.Max(0, MinorTickLength);

        var majorGeometry = new StreamGeometry();
        var minorGeometry = new StreamGeometry();

        using (StreamGeometryContext majorContext = majorGeometry.Open())
        using (StreamGeometryContext minorContext = minorGeometry.Open())
        {
            for (int i = 0; i < count; i++)
            {
                double angle = -90 + 360.0 * i / count;
                bool isMajor = majorEvery > 0 && i % majorEvery == 0;
                if (isMajor)
                {
                    AddTick(majorContext, outer, angle, majorLength);
                }
                else
                {
                    AddTick(minorContext, outer, angle, minorLength);
                }
            }
        }

        _majorTicksGeometry = majorGeometry;
        _minorTicksGeometry = minorGeometry;
    }

    private void AddTick(StreamGeometryContext context, double outer, double angle, double length)
    {
        if (length <= 0.01)
        {
            return;
        }

        Point start = DrawHelper.PolarPoint(_center, outer, angle);
        Point end = DrawHelper.PolarPoint(_center, Math.Max(1, outer - length), angle);
        context.BeginFigure(start, false);
        context.LineTo(end);
        context.EndFigure(false);
    }

    private void BuildCardinalLabels()
    {
        _cardinalLabels = null;
        _cardinalLabelPositions = null;
        if (!ShowCardinalLabels || CardinalLabelBrush is null || CardinalLabelFontSize <= 0)
        {
            return;
        }

        string[] names = UseChineseCardinalLabels
            ? new[] { "北", "东", "南", "西" }
            : new[] { "N", "E", "S", "W" };

        double labelRadius = Math.Max(2, _innerRadius - 8 - Math.Max(MajorTickLength, MinorTickLength) - 8);
        var labels = new FormattedText[names.Length];
        var positions = new Point[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            FormattedText? text = DrawHelper.CreateText(
                names[i], CardinalLabelFontFamily, FontWeight.SemiBold, CardinalLabelFontSize, CardinalLabelBrush);

            if (text is not null)
            {
                labels[i] = text;
            }

            positions[i] = DrawHelper.PolarPoint(_center, labelRadius, -90 + i * 90);
        }

        _cardinalLabels = labels;
        _cardinalLabelPositions = positions;
    }
}
