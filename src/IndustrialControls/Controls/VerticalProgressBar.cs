using Avalonia;
using Avalonia.Media;
using IndustrialControls.Primitives;

namespace IndustrialControls.Controls;

/// <summary>
/// 竖向进度条：常用于液位、料位、湿度等场景，支持分段显示、刻度、圆角与渐变填充。
/// </summary>
public class VerticalProgressBar : InstrumentBase
{
    /// <summary>填充方向。</summary>
    public static readonly StyledProperty<BarDirection> DirectionProperty =
        AvaloniaProperty.Register<VerticalProgressBar, BarDirection>(nameof(Direction), BarDirection.BottomToTop);

    /// <summary>条体宽度。</summary>
    public static readonly StyledProperty<double> BarWidthProperty =
        AvaloniaProperty.Register<VerticalProgressBar, double>(nameof(BarWidth), 22d);

    /// <summary>圆角半径。</summary>
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<VerticalProgressBar, double>(nameof(CornerRadius), 8d);

    /// <summary>轨道颜色。</summary>
    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<VerticalProgressBar, IBrush?>(nameof(TrackBrush), IndustrialPalette.Track);

    /// <summary>填充颜色，可使用渐变。</summary>
    public static readonly StyledProperty<IBrush?> ProgressBrushProperty =
        AvaloniaProperty.Register<VerticalProgressBar, IBrush?>(nameof(ProgressBrush), IndustrialPalette.Accent);

    /// <summary>是否带柔光。</summary>
    public static readonly StyledProperty<bool> ProgressGlowProperty =
        AvaloniaProperty.Register<VerticalProgressBar, bool>(nameof(ProgressGlow), true);

    /// <summary>分段数量；0 表示连续填充。</summary>
    public static readonly StyledProperty<int> SegmentCountProperty =
        AvaloniaProperty.Register<VerticalProgressBar, int>(nameof(SegmentCount), 0);

    /// <summary>分段之间的间距（像素）。</summary>
    public static readonly StyledProperty<double> SegmentGapProperty =
        AvaloniaProperty.Register<VerticalProgressBar, double>(nameof(SegmentGap), 4d);

    /// <summary>是否在顶部显示数值。</summary>
    public static readonly StyledProperty<bool> ShowValueTextProperty =
        AvaloniaProperty.Register<VerticalProgressBar, bool>(nameof(ShowValueText), true);

    /// <summary>数值格式。</summary>
    public static readonly StyledProperty<string?> ValueFormatProperty =
        AvaloniaProperty.Register<VerticalProgressBar, string?>(nameof(ValueFormat), "F0");

    /// <summary>数值后缀（例如 %、L、cm）。</summary>
    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<VerticalProgressBar, string?>(nameof(Unit));

    /// <summary>数值字号。</summary>
    public static readonly StyledProperty<double> ValueFontSizeProperty =
        AvaloniaProperty.Register<VerticalProgressBar, double>(nameof(ValueFontSize), 18d);

    /// <summary>数值颜色。</summary>
    public static readonly StyledProperty<IBrush?> ValueBrushProperty =
        AvaloniaProperty.Register<VerticalProgressBar, IBrush?>(nameof(ValueBrush), IndustrialPalette.Text);

    /// <summary>数值字体。</summary>
    public static readonly StyledProperty<FontFamily?> ValueFontFamilyProperty =
        AvaloniaProperty.Register<VerticalProgressBar, FontFamily?>(nameof(ValueFontFamily));

    /// <summary>数值字重。</summary>
    public static readonly StyledProperty<FontWeight> ValueFontWeightProperty =
        AvaloniaProperty.Register<VerticalProgressBar, FontWeight>(nameof(ValueFontWeight), FontWeight.SemiBold);

    /// <summary>是否显示右侧刻度。</summary>
    public static readonly StyledProperty<bool> ShowScaleProperty =
        AvaloniaProperty.Register<VerticalProgressBar, bool>(nameof(ShowScale));

    /// <summary>刻度数量（包含两端）。</summary>
    public static readonly StyledProperty<int> ScaleCountProperty =
        AvaloniaProperty.Register<VerticalProgressBar, int>(nameof(ScaleCount), 5);

    /// <summary>刻度线长度。</summary>
    public static readonly StyledProperty<double> TickLengthProperty =
        AvaloniaProperty.Register<VerticalProgressBar, double>(nameof(TickLength), 6d);

    /// <summary>刻度线宽。</summary>
    public static readonly StyledProperty<double> TickWidthProperty =
        AvaloniaProperty.Register<VerticalProgressBar, double>(nameof(TickWidth), 1.5d);

    /// <summary>刻度线颜色。</summary>
    public static readonly StyledProperty<IBrush?> TickBrushProperty =
        AvaloniaProperty.Register<VerticalProgressBar, IBrush?>(nameof(TickBrush), IndustrialPalette.Tick);

    /// <summary>刻度数字格式。</summary>
    public static readonly StyledProperty<string?> ScaleLabelFormatProperty =
        AvaloniaProperty.Register<VerticalProgressBar, string?>(nameof(ScaleLabelFormat), "F0");

    /// <summary>刻度数字字号。</summary>
    public static readonly StyledProperty<double> ScaleLabelFontSizeProperty =
        AvaloniaProperty.Register<VerticalProgressBar, double>(nameof(ScaleLabelFontSize), 11d);

    /// <summary>刻度数字颜色。</summary>
    public static readonly StyledProperty<IBrush?> ScaleLabelBrushProperty =
        AvaloniaProperty.Register<VerticalProgressBar, IBrush?>(nameof(ScaleLabelBrush), IndustrialPalette.Label);

    /// <summary>刻度数字字体。</summary>
    public static readonly StyledProperty<FontFamily?> ScaleLabelFontFamilyProperty =
        AvaloniaProperty.Register<VerticalProgressBar, FontFamily?>(nameof(ScaleLabelFontFamily));

    private const double ScaleLabelArea = 38d;
    private const int MaxSegments = 256;
    private const int MaxScaleCount = 64;

    private Rect _barRect;
    private StreamGeometry? _ticksGeometry;
    private Pen? _tickPen;
    private FormattedText[]? _scaleLabels;
    private FormattedText? _valueText;
    private string? _valueTextContent;
    private double _valueTextSize;
    private FontWeight _valueTextWeight;
    private FontFamily? _valueTextFamily;
    private IBrush? _valueTextBrush;
    private double _valueCenterY;

    /// <summary>填充方向。</summary>
    public BarDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    /// <summary>条体宽度。</summary>
    public double BarWidth
    {
        get => GetValue(BarWidthProperty);
        set => SetValue(BarWidthProperty, value);
    }

    /// <summary>圆角半径。</summary>
    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>轨道颜色。</summary>
    public IBrush? TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    /// <summary>填充颜色。</summary>
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

    /// <summary>分段数量；0 表示连续填充。</summary>
    public int SegmentCount
    {
        get => GetValue(SegmentCountProperty);
        set => SetValue(SegmentCountProperty, value);
    }

    /// <summary>分段之间的间距。</summary>
    public double SegmentGap
    {
        get => GetValue(SegmentGapProperty);
        set => SetValue(SegmentGapProperty, value);
    }

    /// <summary>是否在顶部显示数值。</summary>
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

    /// <summary>数值后缀。</summary>
    public string? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
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

    /// <summary>是否显示右侧刻度。</summary>
    public bool ShowScale
    {
        get => GetValue(ShowScaleProperty);
        set => SetValue(ShowScaleProperty, value);
    }

    /// <summary>刻度数量。</summary>
    public int ScaleCount
    {
        get => GetValue(ScaleCountProperty);
        set => SetValue(ScaleCountProperty, value);
    }

    /// <summary>刻度线长度。</summary>
    public double TickLength
    {
        get => GetValue(TickLengthProperty);
        set => SetValue(TickLengthProperty, value);
    }

    /// <summary>刻度线宽。</summary>
    public double TickWidth
    {
        get => GetValue(TickWidthProperty);
        set => SetValue(TickWidthProperty, value);
    }

    /// <summary>刻度线颜色。</summary>
    public IBrush? TickBrush
    {
        get => GetValue(TickBrushProperty);
        set => SetValue(TickBrushProperty, value);
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

    /// <inheritdoc />
    protected override Size DefaultSize => new(72, 240);

    /// <inheritdoc />
    protected override void RenderCore(DrawingContext context)
    {
        Size size = Bounds.Size;
        if (size.Width < 8 || size.Height < 16)
        {
            return;
        }

        if (IsStaticCacheInvalid(size))
        {
            RebuildStatic(size);
            MarkStaticCacheBuilt(size);
        }

        double ratio = NormalizedValue;
        double barWidth = _barRect.Width;
        double corner = Math.Clamp(CornerRadius, 0, barWidth / 2);

        if (TrackBrush is not null)
        {
            context.DrawRectangle(TrackBrush, null, _barRect, corner, corner);
        }

        if (ProgressBrush is not null)
        {
            int segments = Math.Clamp(SegmentCount, 0, MaxSegments);
            if (segments > 1)
            {
                DrawSegments(context, segments, ratio, corner);
            }
            else
            {
                DrawContinuous(context, ratio, corner);
            }
        }

        if (ShowScale && _ticksGeometry is not null && _tickPen is not null)
        {
            context.DrawGeometry(null, _tickPen, _ticksGeometry);

            if (_scaleLabels is not null)
            {
                double labelX = _barRect.Right + Math.Max(2, TickLength) + 5;
                for (int i = 0; i < _scaleLabels.Length; i++)
                {
                    if (_scaleLabels[i] is null)
                    {
                        continue;
                    }

                    double ratio2 = _scaleLabels.Length == 1 ? 0 : (double)i / (_scaleLabels.Length - 1);
                    double y = Direction == BarDirection.BottomToTop
                        ? _barRect.Bottom - _barRect.Height * ratio2
                        : _barRect.Top + _barRect.Height * ratio2;
                    context.DrawText(_scaleLabels[i],
                        new Point(labelX, y - _scaleLabels[i].Height / 2));
                }
            }
        }

        if (ShowValueText && ValueBrush is not null && ValueFontSize > 0)
        {
            string content = DrawHelper.FormatValue(Value, ValueFormat) + Unit;
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
                DrawHelper.DrawTextCentered(context, _valueText, new Point(_barRect.Center.X, _valueCenterY));
            }
        }
    }

    private void DrawContinuous(DrawingContext context, double ratio, double corner)
    {
        if (ratio <= 0.0005)
        {
            return;
        }

        double height = Math.Max(1, _barRect.Height * ratio);
        double y = Direction == BarDirection.BottomToTop ? _barRect.Bottom - height : _barRect.Top;
        var rect = new Rect(_barRect.X, y, _barRect.Width, height);
        double radius = Math.Min(corner, height / 2);

        if (ProgressGlow && ratio < 0.995)
        {
            using (context.PushOpacity(0.22))
            {
                context.DrawRectangle(ProgressBrush, null, rect.Inflate(2), radius + 2, radius + 2);
            }
        }

        context.DrawRectangle(ProgressBrush, null, rect, radius, radius);
    }

    private void DrawSegments(DrawingContext context, int segments, double ratio, double corner)
    {
        double gap = Math.Max(0, SegmentGap);
        double available = _barRect.Height - gap * (segments - 1);
        if (available <= 0)
        {
            return;
        }

        double segmentHeight = available / segments;
        double radius = Math.Min(corner, segmentHeight / 2);

        for (int i = 0; i < segments; i++)
        {
            // 从填充起点开始数为第 i 段
            double y = Direction == BarDirection.BottomToTop
                ? _barRect.Bottom - (i + 1) * segmentHeight - i * gap
                : _barRect.Top + i * (segmentHeight + gap);

            bool active = ratio * segments > i;

            if (!active)
            {
                break;
            }

            var rect = new Rect(_barRect.X, y, _barRect.Width, segmentHeight);
            context.DrawRectangle(ProgressBrush, null, rect, radius, radius);
        }
    }

    private void RebuildStatic(Size size)
    {
        double valueArea = ShowValueText ? Math.Max(ValueFontSize * 1.8, 22) : 0;
        double scaleArea = ShowScale ? ScaleLabelArea : 0;
        double usableWidth = Math.Max(4, size.Width - scaleArea);
        double barWidth = Math.Clamp(BarWidth, 4, Math.Max(4, usableWidth - 8));
        double barX = Math.Max(0, (usableWidth - barWidth) / 2);
        double top = valueArea + 2;
        double height = Math.Max(4, size.Height - top - 2);

        _barRect = new Rect(barX, top, barWidth, height);
        _valueCenterY = valueArea / 2;

        _tickPen = TickBrush is null ? null : DrawHelper.CreatePen(TickBrush, Math.Max(0.5, TickWidth), PenLineCap.Round);
        _ticksGeometry = null;
        _scaleLabels = null;

        int scaleCount = Math.Clamp(ScaleCount, 0, MaxScaleCount);
        if (!ShowScale || scaleCount < 2 || ScaleLabelBrush is null || ScaleLabelFontSize <= 0)
        {
            _valueText = null;
            _valueTextContent = null;
            return;
        }

        var geometry = new StreamGeometry();
        var labels = new FormattedText[scaleCount];
        double tickLength = Math.Max(0, TickLength);

        using (StreamGeometryContext tickContext = geometry.Open())
        {
            for (int i = 0; i < scaleCount; i++)
            {
                double ratio = (double)i / (scaleCount - 1);
                double y = Direction == BarDirection.BottomToTop
                    ? _barRect.Bottom - _barRect.Height * ratio
                    : _barRect.Top + _barRect.Height * ratio;

                double x0 = _barRect.Right + 2;
                double x1 = x0 + tickLength;
                tickContext.BeginFigure(new Point(x0, y), false);
                tickContext.LineTo(new Point(x1, y));
                tickContext.EndFigure(false);

                double value = Minimum + (Maximum - Minimum) * ratio;
                FormattedText? label = DrawHelper.CreateText(
                    DrawHelper.FormatValue(value, ScaleLabelFormat),
                    ScaleLabelFontFamily,
                    FontWeight.Normal,
                    ScaleLabelFontSize,
                    ScaleLabelBrush);

                if (label is not null)
                {
                    labels[i] = label;
                }
            }
        }

        _ticksGeometry = geometry;
        _scaleLabels = labels;
        _valueText = null;
        _valueTextContent = null;
    }
}
