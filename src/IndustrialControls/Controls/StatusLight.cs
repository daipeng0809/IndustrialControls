using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using IndustrialControls.Primitives;

namespace IndustrialControls.Controls;

/// <summary>
/// 设备状态灯：支持常亮、呼吸（脉冲）效果与右侧说明文字，用于表示运行/警告/告警等状态。
/// </summary>
public class StatusLight : Control
{
    /// <summary>状态级别，决定默认颜色。</summary>
    public static readonly StyledProperty<IndicatorStatus> StatusProperty =
        AvaloniaProperty.Register<StatusLight, IndicatorStatus>(nameof(Status), IndicatorStatus.Off);

    /// <summary>是否显示呼吸（脉冲）光晕。</summary>
    public static readonly StyledProperty<bool> IsPulsingProperty =
        AvaloniaProperty.Register<StatusLight, bool>(nameof(IsPulsing));

    /// <summary>灯珠直径。</summary>
    public static readonly StyledProperty<double> DiameterProperty =
        AvaloniaProperty.Register<StatusLight, double>(nameof(Diameter), 16d);

    /// <summary>是否显示光晕。</summary>
    public static readonly StyledProperty<bool> ShowHaloProperty =
        AvaloniaProperty.Register<StatusLight, bool>(nameof(ShowHalo), true);

    /// <summary>说明文字。</summary>
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<StatusLight, string?>(nameof(Label));

    /// <summary>说明文字字号。</summary>
    public static readonly StyledProperty<double> LabelFontSizeProperty =
        AvaloniaProperty.Register<StatusLight, double>(nameof(LabelFontSize), 13d);

    /// <summary>说明文字颜色。</summary>
    public static readonly StyledProperty<IBrush?> LabelBrushProperty =
        AvaloniaProperty.Register<StatusLight, IBrush?>(nameof(LabelBrush), IndustrialPalette.Text);

    /// <summary>说明文字字体。</summary>
    public static readonly StyledProperty<FontFamily?> LabelFontFamilyProperty =
        AvaloniaProperty.Register<StatusLight, FontFamily?>(nameof(LabelFontFamily));

    /// <summary>灯珠与文字之间的间距。</summary>
    public static readonly StyledProperty<double> LabelGapProperty =
        AvaloniaProperty.Register<StatusLight, double>(nameof(LabelGap), 8d);

    private static readonly ImmutableSolidColorBrush OffBrush = new(Color.FromArgb(0xFF, 0x64, 0x74, 0x8B));
    private static readonly ImmutableSolidColorBrush NormalBrush = IndustrialPalette.Green;
    private static readonly ImmutableSolidColorBrush WarningBrush = IndustrialPalette.Amber;
    private static readonly ImmutableSolidColorBrush AlarmBrush = IndustrialPalette.Red;
    private static readonly ImmutableSolidColorBrush MaintenanceBrush = IndustrialPalette.Blue;

    private DispatcherTimer? _timer;
    private bool _isAttached;
    private double _pulsePhase;
    private FormattedText? _labelText;
    private string? _labelContent;
    private double _labelSize;
    private FontFamily? _labelFamily;
    private IBrush? _labelBrush;

    static StatusLight()
    {
        FocusableProperty.OverrideDefaultValue<StatusLight>(false);
        ClipToBoundsProperty.OverrideDefaultValue<StatusLight>(false);
    }

    /// <summary>状态级别。</summary>
    public IndicatorStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>是否显示呼吸（脉冲）光晕。</summary>
    public bool IsPulsing
    {
        get => GetValue(IsPulsingProperty);
        set => SetValue(IsPulsingProperty, value);
    }

    /// <summary>灯珠直径。</summary>
    public double Diameter
    {
        get => GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    /// <summary>是否显示光晕。</summary>
    public bool ShowHalo
    {
        get => GetValue(ShowHaloProperty);
        set => SetValue(ShowHaloProperty, value);
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

    /// <summary>灯珠与文字之间的间距。</summary>
    public double LabelGap
    {
        get => GetValue(LabelGapProperty);
        set => SetValue(LabelGapProperty, value);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        double diameter = double.IsFinite(Diameter) ? Math.Max(4, Diameter) : 16;
        FormattedText? label = EnsureLabelText();

        double desiredWidth = diameter + 4;
        if (label is not null)
        {
            desiredWidth += Math.Max(0, LabelGap) + label.Width + 2;
        }

        double width = double.IsInfinity(availableSize.Width) ? Math.Max(24, desiredWidth) : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height)
            ? Math.Max(24, diameter + 8)
            : availableSize.Height;
        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsPulsingProperty || change.Property == StatusProperty)
        {
            UpdatePulseTimer();
        }

        // 文字或灯珠尺寸变化会影响期望尺寸
        if (change.Property == LabelProperty
            || change.Property == LabelFontSizeProperty
            || change.Property == LabelFontFamilyProperty
            || change.Property == LabelGapProperty
            || change.Property == DiameterProperty)
        {
            InvalidateMeasure();
        }

        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        UpdatePulseTimer();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttached = false;
        UpdatePulseTimer();
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        Size size = Bounds.Size;
        if (size.Width <= 0 || size.Height <= 0)
        {
            return;
        }

        double diameter = double.IsFinite(Diameter)
            ? Math.Clamp(Diameter, 4, Math.Max(4, Math.Min(size.Width, size.Height)))
            : 16;
        double radius = diameter / 2;
        var center = new Point(radius + 2, size.Height / 2);
        IBrush brush = ResolveBrush(Status);
        bool active = Status != IndicatorStatus.Off;

        double pulse = IsPulsing ? (Math.Sin(_pulsePhase) + 1) / 2 : 1;
        double haloScale = IsPulsing ? 1.15 + pulse * 0.55 : 1.55;
        double haloOpacity = IsPulsing ? 0.35 + pulse * 0.45 : 0.75;

        if (ShowHalo && active)
        {
            DrawHalo(context, brush, center, radius, haloScale, haloOpacity);
        }

        context.DrawEllipse(brush, null, center, radius, radius);

        // 高光，让灯珠看起来是发光的实体
        var highlightCenter = new Point(center.X - radius * 0.28, center.Y - radius * 0.3);
        using (context.PushOpacity(0.45))
        {
            context.DrawEllipse(Brushes.White, null, highlightCenter, radius * 0.3, radius * 0.3);
        }

        FormattedText? label = EnsureLabelText();
        if (label is not null)
        {
            double textX = center.X + radius + Math.Max(0, LabelGap);
            double textY = center.Y - label.Height / 2;
            context.DrawText(label, new Point(textX, textY));
        }
    }

    /// <summary>按当前属性获取（必要时重建）说明文字，供测量与绘制共用。</summary>
    private FormattedText? EnsureLabelText()
    {
        string? content = Label;
        if (LabelBrush is null || string.IsNullOrEmpty(content) || LabelFontSize <= 0)
        {
            _labelText = null;
            _labelContent = null;
            return null;
        }

        if (_labelText is null
            || !string.Equals(content, _labelContent, StringComparison.Ordinal)
            || _labelSize != LabelFontSize
            || !Equals(_labelFamily, LabelFontFamily)
            || !ReferenceEquals(_labelBrush, LabelBrush))
        {
            _labelText = DrawHelper.CreateText(
                content, LabelFontFamily, FontWeight.Normal, LabelFontSize, LabelBrush);
            _labelContent = content;
            _labelSize = LabelFontSize;
            _labelFamily = LabelFontFamily;
            _labelBrush = LabelBrush;
        }

        return _labelText;
    }

    private static void DrawHalo(DrawingContext context, IBrush brush, Point center, double radius,
        double scale, double opacity)
    {
        DrawRing(context, brush, center, radius * (scale * 0.75), opacity * 0.2);
        DrawRing(context, brush, center, radius * scale, opacity * 0.12);
        DrawRing(context, brush, center, radius * (scale * 1.3), opacity * 0.05);
    }

    private static void DrawRing(DrawingContext context, IBrush brush, Point center, double radius, double opacity)
    {
        if (radius <= 0 || opacity <= 0)
        {
            return;
        }

        using (context.PushOpacity(opacity))
        {
            context.DrawEllipse(brush, null, center, radius, radius);
        }
    }

    private void UpdatePulseTimer()
    {
        bool shouldRun = _isAttached && IsPulsing && Status != IndicatorStatus.Off;
        if (!shouldRun)
        {
            _timer?.Stop();
            return;
        }

        if (_timer is null)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            _timer.Tick += (_, _) =>
            {
                _pulsePhase += 0.12;
                InvalidateVisual();
            };
        }

        _timer.Start();
    }

    private static IBrush ResolveBrush(IndicatorStatus status) => status switch
    {
        IndicatorStatus.Normal => NormalBrush,
        IndicatorStatus.Warning => WarningBrush,
        IndicatorStatus.Alarm => AlarmBrush,
        IndicatorStatus.Maintenance => MaintenanceBrush,
        _ => OffBrush,
    };
}
