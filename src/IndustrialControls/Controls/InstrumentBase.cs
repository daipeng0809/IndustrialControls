using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using IndustrialControls.Primitives;

namespace IndustrialControls.Controls;

/// <summary>
/// 所有工业控件的基类：提供数值/量程、渲染缓存版本、以及“线程安全 + 按帧合并”的刷新机制。
/// </summary>
/// <remarks>
/// <para>
/// 高频数据场景：可以从任意线程直接给 <see cref="Value"/> 赋值。控件内部会把后台线程的写入封送到 UI 线程，
/// 并合并到下一个渲染帧，因此即使 1 秒内写入上万次，也不会淹没 UI 线程或触发跨线程异常。
/// </para>
/// <para>
/// 静态部分（刻度、刻度文字、色带等）会被缓存，只有数值变化时才重绘指针/进度等动态部分。
/// </para>
/// </remarks>
public abstract class InstrumentBase : Control
{
    /// <summary>当前值。可从任意线程赋值。</summary>
    public static readonly DirectProperty<InstrumentBase, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<InstrumentBase, double>(
            nameof(Value), o => o.Value, (o, v) => o.Value = v, 0d, BindingMode.TwoWay);

    /// <summary>量程下限。</summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<InstrumentBase, double>(nameof(Minimum), 0d);

    /// <summary>量程上限。</summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<InstrumentBase, double>(nameof(Maximum), 100d);

    private readonly object _valueGate = new();
    private double _value;
    private double _pendingValue;
    private bool _hasPendingValue;
    private int _valuePostQueued;
    private int _renderQueued;
    private int _staticVersion;
    private int _builtStaticVersion = -1;
    private Size _builtSize;

    static InstrumentBase()
    {
        FocusableProperty.OverrideDefaultValue<InstrumentBase>(false);
    }

    /// <summary>当前值。可从任意线程赋值：后台线程写入会被封送并按帧合并。</summary>
    public double Value
    {
        // 后台线程可能读取该字段：使用 Volatile 读避免长时间读到陈旧值
        get => Volatile.Read(ref _value);
        set
        {
            if (Dispatcher.CheckAccess())
            {
                // 丢弃尚未应用的后台旧值，保证“最后一次写入”生效
                lock (_valueGate)
                {
                    _hasPendingValue = false;
                }

                SetAndRaise(ValueProperty, ref _value, value);
                return;
            }

            lock (_valueGate)
            {
                _pendingValue = value;
                _hasPendingValue = true;
            }

            // 同一时刻最多只有一个待处理的封送任务；
            // Dispatcher 关闭时 Post 会静默丢弃任务，此时应用正在退出，无需处理
            if (Interlocked.Exchange(ref _valuePostQueued, 1) == 0)
            {
                Dispatcher.Post(FlushPendingValue, DispatcherPriority.Render);
            }
        }
    }

    /// <summary>量程下限。</summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>量程上限。</summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>无可用空间时的默认尺寸。</summary>
    protected virtual Size DefaultSize => new(160, 160);

    /// <summary>当前值在量程内的比例（0~1）。</summary>
    protected double NormalizedValue => DrawHelper.Normalize(Value, Minimum, Maximum);

    /// <summary>静态渲染缓存的版本号；非 <see cref="Value"/> 的属性变化会使其递增。</summary>
    protected int StaticVersion => Volatile.Read(ref _staticVersion);

    /// <summary>
    /// 请求重绘。可从任意线程调用：如果不在 UI 线程，会把请求合并后投递到所属 Dispatcher，
    /// 保证同一时刻最多只有一个待处理的刷新请求。
    /// </summary>
    public void RequestRender()
    {
        Dispatcher dispatcher = Dispatcher;
        if (dispatcher.CheckAccess())
        {
            // UI 线程上直接重绘，省去一次原子操作
            InvalidateVisual();
            return;
        }

        if (Interlocked.Exchange(ref _renderQueued, 1) != 0)
        {
            return;
        }

        dispatcher.Post(
            () =>
            {
                Volatile.Write(ref _renderQueued, 0);
                InvalidateVisual();
            },
            DispatcherPriority.Render);
    }

    /// <summary>把后台线程写入的最新值应用到 UI 线程。</summary>
    private void FlushPendingValue()
    {
        Interlocked.Exchange(ref _valuePostQueued, 0);

        double value;
        lock (_valueGate)
        {
            if (!_hasPendingValue)
            {
                return;
            }

            _hasPendingValue = false;
            value = _pendingValue;
        }

        SetAndRaise(ValueProperty, ref _value, value);
    }

    /// <summary>让静态缓存（刻度、文字等）失效并请求重绘。修改 <c>Ranges</c> 一类的集合后调用。</summary>
    protected void InvalidateStatic()
    {
        Interlocked.Increment(ref _staticVersion);
        RequestRender();
    }

    /// <summary>判断静态缓存是否需要重建（尺寸或版本变化）。</summary>
    protected bool IsStaticCacheInvalid(Size size)
        => _builtSize != size || _builtStaticVersion != StaticVersion;

    /// <summary>标记静态缓存已按指定尺寸重建。</summary>
    protected void MarkStaticCacheBuilt(Size size)
    {
        _builtSize = size;
        _builtStaticVersion = StaticVersion;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Value 是高频变化的属性：只重绘动态部分，不重建刻度/文字缓存。
        if (!ReferenceEquals(change.Property, ValueProperty))
        {
            Interlocked.Increment(ref _staticVersion);
        }

        RequestRender();
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        Size fallback = DefaultSize;
        double width = double.IsInfinity(availableSize.Width) ? fallback.Width : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? fallback.Height : availableSize.Height;
        return new Size(width, height);
    }

    /// <inheritdoc />
    public sealed override void Render(DrawingContext context) => RenderCore(context);

    /// <summary>执行实际绘制。只应在 UI 线程调用。</summary>
    protected abstract void RenderCore(DrawingContext context);
}
