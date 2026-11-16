using Avalonia;
using Avalonia.Media;
using IndustrialControls.Primitives;

namespace IndustrialControls.Controls;

/// <summary>
/// 电池电量指示器：电量填充、低电量/危险配色、充电闪电标识与百分比文字。
/// </summary>
public class BatteryIndicator : InstrumentBase
{
    /// <summary>电池外壳颜色。</summary>
    public static readonly StyledProperty<IBrush?> BodyBrushProperty =
        AvaloniaProperty.Register<BatteryIndicator, IBrush?>(nameof(BodyBrush), IndustrialPalette.Hub);

    /// <summary>边框颜色。</summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<BatteryIndicator, IBrush?>(nameof(BorderBrush), IndustrialPalette.Dim);

    /// <summary>边框粗度。</summary>
    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(nameof(BorderThickness), 2d);

    /// <summary>外壳圆角。</summary>
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(nameof(CornerRadius), 8d);

    /// <summary>正极帽颜色。</summary>
    public static readonly StyledProperty<IBrush?> TerminalBrushProperty =
        AvaloniaProperty.Register<BatteryIndicator, IBrush?>(nameof(TerminalBrush), IndustrialPalette.Dim);

    /// <summary>正极帽宽度。</summary>
    public static readonly StyledProperty<double> TerminalWidthProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(nameof(TerminalWidth), 7d);

    /// <summary>正极帽高度占电池高度的比例。</summary>
    public static readonly StyledProperty<double> TerminalHeightRatioProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(nameof(TerminalHeightRatio), 0.4d);

    /// <summary>电量填充与边框之间的内边距。</summary>
    public static readonly StyledProperty<double> FillPaddingProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(nameof(FillPadding), 3d);

    /// <summary>正常电量颜色。</summary>
    public static readonly StyledProperty<IBrush?> FillBrushProperty =
        AvaloniaProperty.Register<BatteryIndicator, IBrush?>(nameof(FillBrush), IndustrialPalette.Green);

    /// <summary>低电量颜色。</summary>
    public static readonly StyledProperty<IBrush?> LowBrushProperty =
        AvaloniaProperty.Register<BatteryIndicator, IBrush?>(nameof(LowBrush), IndustrialPalette.Amber);

    /// <summary>危险电量颜色。</summary>
    public static readonly StyledProperty<IBrush?> CriticalBrushProperty =
        AvaloniaProperty.Register<BatteryIndicator, IBrush?>(nameof(CriticalBrush), IndustrialPalette.Red);

    /// <summary>是否根据电量自动切换低电量/危险颜色。</summary>
    public static readonly StyledProperty<bool> UseLevelColorsProperty =
        AvaloniaProperty.Register<BatteryIndicator, bool>(nameof(UseLevelColors), true);

    /// <summary>低电量阈值（0~1）。</summary>
    public static readonly StyledProperty<double> LowThresholdProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(nameof(LowThreshold), 0.3d);

    /// <summary>危险阈值（0~1）。</summary>
    public static readonly StyledProperty<double> CriticalThresholdProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(nameof(CriticalThreshold), 0.15d);

    /// <summary>是否正在充电。</summary>
    public static readonly StyledProperty<bool> IsChargingProperty =
        AvaloniaProperty.Register<BatteryIndicator, bool>(nameof(IsCharging));

    /// <summary>充电时是否显示闪电标识。</summary>
    public static readonly StyledProperty<bool> ShowChargingBoltProperty =
        AvaloniaProperty.Register<BatteryIndicator, bool>(nameof(ShowChargingBolt), true);

    /// <summary>闪电标识颜色。</summary>
    public static readonly StyledProperty<IBrush?> ChargingBoltBrushProperty =
        AvaloniaProperty.Register<BatteryIndicator, IBrush?>(nameof(ChargingBoltBrush), IndustrialPalette.Text);

    /// <summary>是否显示电量文字。</summary>
    public static readonly StyledProperty<bool> ShowValueTextProperty =
        AvaloniaProperty.Register<BatteryIndicator, bool>(nameof(ShowValueText), true);

    /// <summary>电量文字格式。</summary>
    public static readonly StyledProperty<string?> ValueFormatProperty =
        AvaloniaProperty.Register<BatteryIndicator, string?>(nameof(ValueFormat), "F0");

    /// <summary>电量文字后缀。</summary>
    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<BatteryIndicator, string?>(nameof(Unit), "%");

    /// <summary>电量文字字号。</summary>
    public static readonly StyledProperty<double> ValueFontSizeProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(nameof(ValueFontSize), 16d);

    /// <summary>电量文字颜色。</summary>
    public static readonly StyledProperty<IBrush?> ValueBrushProperty =
        AvaloniaProperty.Register<BatteryIndicator, IBrush?>(nameof(ValueBrush), IndustrialPalette.Text);

    /// <summary>电量文字字体。</summary>
    public static readonly StyledProperty<FontFamily?> ValueFontFamilyProperty =
        AvaloniaProperty.Register<BatteryIndicator, FontFamily?>(nameof(ValueFontFamily));

    /// <summary>电量文字字重。</summary>
    public static readonly StyledProperty<FontWeight> ValueFontWeightProperty =
        AvaloniaProperty.Register<BatteryIndicator, FontWeight>(nameof(ValueFontWeight), FontWeight.SemiBold);

    private Rect _bodyRect;
    private Rect _fillArea;
    private double _corner;
    private Pen? _borderPen;
    private StreamGeometry? _boltGeometry;
    private FormattedText? _valueText;
    private string? _valueTextContent;
    private double _valueTextSize;
    private FontWeight _valueTextWeight;
    private FontFamily? _valueTextFamily;
    private IBrush? _valueTextBrush;

    /// <summary>电池外壳颜色。</summary>
    public IBrush? BodyBrush
    {
        get => GetValue(BodyBrushProperty);
        set => SetValue(BodyBrushProperty, value);
    }

    /// <summary>边框颜色。</summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>边框粗度。</summary>
    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>外壳圆角。</summary>
    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>正极帽颜色。</summary>
    public IBrush? TerminalBrush
    {
        get => GetValue(TerminalBrushProperty);
        set => SetValue(TerminalBrushProperty, value);
    }

    /// <summary>正极帽宽度。</summary>
    public double TerminalWidth
    {
        get => GetValue(TerminalWidthProperty);
        set => SetValue(TerminalWidthProperty, value);
    }

    /// <summary>正极帽高度比例。</summary>
    public double TerminalHeightRatio
    {
        get => GetValue(TerminalHeightRatioProperty);
        set => SetValue(TerminalHeightRatioProperty, value);
    }

    /// <summary>电量填充内边距。</summary>
    public double FillPadding
    {
        get => GetValue(FillPaddingProperty);
        set => SetValue(FillPaddingProperty, value);
    }

    /// <summary>正常电量颜色。</summary>
    public IBrush? FillBrush
    {
        get => GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    /// <summary>低电量颜色。</summary>
    public IBrush? LowBrush
    {
        get => GetValue(LowBrushProperty);
        set => SetValue(LowBrushProperty, value);
    }

    /// <summary>危险电量颜色。</summary>
    public IBrush? CriticalBrush
    {
        get => GetValue(CriticalBrushProperty);
        set => SetValue(CriticalBrushProperty, value);
    }

    /// <summary>是否根据电量自动切换颜色。</summary>
    public bool UseLevelColors
    {
        get => GetValue(UseLevelColorsProperty);
        set => SetValue(UseLevelColorsProperty, value);
    }

    /// <summary>低电量阈值。</summary>
    public double LowThreshold
    {
        get => GetValue(LowThresholdProperty);
        set => SetValue(LowThresholdProperty, value);
    }

    /// <summary>危险阈值。</summary>
    public double CriticalThreshold
    {
        get => GetValue(CriticalThresholdProperty);
        set => SetValue(CriticalThresholdProperty, value);
    }

    /// <summary>是否正在充电。</summary>
    public bool IsCharging
    {
        get => GetValue(IsChargingProperty);
        set => SetValue(IsChargingProperty, value);
    }

    /// <summary>充电时是否显示闪电标识。</summary>
    public bool ShowChargingBolt
    {
        get => GetValue(ShowChargingBoltProperty);
        set => SetValue(ShowChargingBoltProperty, value);
    }

    /// <summary>闪电标识颜色。</summary>
    public IBrush? ChargingBoltBrush
    {
        get => GetValue(ChargingBoltBrushProperty);
        set => SetValue(ChargingBoltBrushProperty, value);
    }

    /// <summary>是否显示电量文字。</summary>
    public bool ShowValueText
    {
        get => GetValue(ShowValueTextProperty);
        set => SetValue(ShowValueTextProperty, value);
    }

    /// <summary>电量文字格式。</summary>
    public string? ValueFormat
    {
        get => GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    /// <summary>电量文字后缀。</summary>
    public string? Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    /// <summary>电量文字字号。</summary>
    public double ValueFontSize
    {
        get => GetValue(ValueFontSizeProperty);
        set => SetValue(ValueFontSizeProperty, value);
    }

    /// <summary>电量文字颜色。</summary>
    public IBrush? ValueBrush
    {
        get => GetValue(ValueBrushProperty);
        set => SetValue(ValueBrushProperty, value);
    }

    /// <summary>电量文字字体。</summary>
    public FontFamily? ValueFontFamily
    {
        get => GetValue(ValueFontFamilyProperty);
        set => SetValue(ValueFontFamilyProperty, value);
    }

    /// <summary>电量文字字重。</summary>
    public FontWeight ValueFontWeight
    {
        get => GetValue(ValueFontWeightProperty);
        set => SetValue(ValueFontWeightProperty, value);
    }

    /// <inheritdoc />
    protected override Size DefaultSize => new(120, 52);

    /// <inheritdoc />
    protected override void RenderCore(DrawingContext context)
    {
        Size size = Bounds.Size;
        if (size.Width < 16 || size.Height < 10)
        {
            return;
        }

        if (IsStaticCacheInvalid(size))
        {
            RebuildStatic(size);
            MarkStaticCacheBuilt(size);
        }

        // 正极帽
        double terminalWidth = Math.Clamp(TerminalWidth, 2, Math.Max(2, size.Width * 0.18));
        double terminalHeight = Math.Max(4, _bodyRect.Height * Math.Clamp(TerminalHeightRatio, 0.1, 1));
        var terminalRect = new Rect(
            _bodyRect.Right + 2,
            _bodyRect.Center.Y - terminalHeight / 2,
            terminalWidth,
            terminalHeight);
        double terminalRadius = Math.Min(3, terminalWidth / 2);
        context.DrawRectangle(TerminalBrush, null, terminalRect, terminalRadius, terminalRadius);

        // 外壳
        context.DrawRectangle(BodyBrush, _borderPen, _bodyRect, _corner, _corner);

        // 电量填充
        double ratio = NormalizedValue;
        double fillWidth = _fillArea.Width * ratio;
        if (fillWidth > 0.5)
        {
            IBrush? fill = ResolveFillBrush(ratio);
            if (fill is not null)
            {
                var fillRect = new Rect(_fillArea.X, _fillArea.Y, fillWidth, _fillArea.Height);
                double fillRadius = Math.Min(Math.Min(3, _fillArea.Height / 2), fillWidth / 2);
                context.DrawRectangle(fill, null, fillRect, fillRadius, fillRadius);
            }
        }

        // 充电闪电
        if (IsCharging && ShowChargingBolt && _boltGeometry is not null && ChargingBoltBrush is not null)
        {
            context.DrawGeometry(ChargingBoltBrush, null, _boltGeometry);
        }

        // 电量文字
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
                DrawHelper.DrawTextCentered(context, _valueText, _bodyRect.Center);
            }
        }
    }

    private IBrush? ResolveFillBrush(double ratio)
    {
        if (!UseLevelColors)
        {
            return FillBrush;
        }

        if (ratio <= Math.Clamp(CriticalThreshold, 0, 1))
        {
            return CriticalBrush;
        }

        if (ratio <= Math.Clamp(LowThreshold, 0, 1))
        {
            return LowBrush;
        }

        return FillBrush;
    }

    private void RebuildStatic(Size size)
    {
        double terminalWidth = Math.Clamp(TerminalWidth, 2, Math.Max(2, size.Width * 0.18));
        double bodyWidth = Math.Max(8, size.Width - terminalWidth - 2);
        _bodyRect = new Rect(0, 0, bodyWidth, size.Height);
        _corner = Math.Clamp(CornerRadius, 0, Math.Min(_bodyRect.Width, _bodyRect.Height) / 2);

        double border = Math.Clamp(BorderThickness, 0, 8);
        double inset = border + Math.Clamp(FillPadding, 0, 12);
        _fillArea = new Rect(
            _bodyRect.X + inset,
            _bodyRect.Y + inset,
            Math.Max(0, _bodyRect.Width - inset * 2),
            Math.Max(0, _bodyRect.Height - inset * 2));

        _borderPen = border > 0 && BorderBrush is not null ? DrawHelper.CreatePen(BorderBrush, border) : null;

        // 闪电：以电池中心为原点、按高度缩放
        double scale = Math.Min(_bodyRect.Height * 0.5, _bodyRect.Width * 0.28);
        double cx = _bodyRect.Center.X;
        double cy = _bodyRect.Center.Y;
        _boltGeometry = DrawHelper.CreatePolygon(new[]
        {
            new Point(cx + scale * 0.08, cy - scale),
            new Point(cx - scale * 0.42, cy + scale * 0.08),
            new Point(cx - scale * 0.04, cy + scale * 0.08),
            new Point(cx - scale * 0.12, cy + scale),
            new Point(cx + scale * 0.42, cy - scale * 0.12),
            new Point(cx + scale * 0.04, cy - scale * 0.12),
        });

        _valueText = null;
        _valueTextContent = null;
    }
}
