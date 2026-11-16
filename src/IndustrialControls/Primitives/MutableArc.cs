using Avalonia;
using Avalonia.Media;

namespace IndustrialControls.Primitives;

/// <summary>
/// 可复用的圆弧几何：原地修改同一个 <see cref="PathGeometry"/> 的端点来绘制动态进度弧，
/// 避免每帧分配新的几何对象与绘图上下文。
/// </summary>
/// <remarks>
/// <para>仅在 UI 线程使用；绘制前调用 <see cref="Update"/> 刷新参数。</para>
/// <para>
/// 安全性：<see cref="PathGeometry"/> 会跟踪其 <see cref="PathFigure"/> 与 <see cref="ArcSegment"/> 的属性变化
/// 并自动失效内部缓存，因此重复绘制同一几何、控件尺寸变化后重建、控件释放都是安全的。
/// 本类不订阅任何外部事件，也不持有非托管资源。
/// </para>
/// </remarks>
internal sealed class MutableArc
{
    private readonly PathGeometry _geometry = new();
    private readonly PathFigure _figure;
    private readonly ArcSegment _arc;

    private Point _lastCenter;
    private double _lastRadius = double.NaN;
    private double _lastStartAngle = double.NaN;
    private double _lastSweepAngle = double.NaN;
    private bool _lastWasDegenerate;
    private bool _hasState;

    public MutableArc(double radius)
    {
        double r = NormalizeRadius(radius);
        _arc = new ArcSegment
        {
            Size = new Size(r, r),
            SweepDirection = SweepDirection.Clockwise,
        };

        _figure = new PathFigure
        {
            IsClosed = false,
            IsFilled = false,
            Segments = new PathSegments { _arc },
        };

        _geometry.Figures = new PathFigures { _figure };
    }

    /// <summary>被复用的几何对象。</summary>
    public PathGeometry Geometry => _geometry;

    /// <summary>更新圆弧的圆心、半径与扫描范围。</summary>
    public void Update(Point center, double radius, double startAngle, double sweepAngle)
    {
        bool valid = double.IsFinite(center.X)
                     && double.IsFinite(center.Y)
                     && double.IsFinite(radius)
                     && radius > 0
                     && double.IsFinite(startAngle)
                     && double.IsFinite(sweepAngle);

        // 完整圆请使用 DrawEllipse；这里把扫描角限制在 (-360, 360) 内，避免起终点重合
        double sweep = valid ? Math.Clamp(sweepAngle, -359.999, 359.999) : 0;
        bool degenerate = !valid || Math.Abs(sweep) < 0.001;

        if (_hasState
            && _lastCenter == center
            && _lastRadius.Equals(radius)
            && _lastStartAngle.Equals(startAngle)
            && _lastSweepAngle.Equals(sweep)
            && _lastWasDegenerate == degenerate)
        {
            // 参数没有变化：跳过无效修改，避免让几何缓存反复失效
            return;
        }

        _hasState = true;
        _lastCenter = center;
        _lastRadius = radius;
        _lastStartAngle = startAngle;
        _lastSweepAngle = sweep;
        _lastWasDegenerate = degenerate;

        if (degenerate)
        {
            // 退化为同一点：不产生可见描边，也不会残留上一帧的圆弧
            Point safeCenter = double.IsFinite(center.X) && double.IsFinite(center.Y) ? center : default;
            _figure.StartPoint = safeCenter;
            _arc.Point = safeCenter;
            _arc.IsLargeArc = false;
            return;
        }

        double r = NormalizeRadius(radius);
        var size = new Size(r, r);
        if (_arc.Size != size)
        {
            _arc.Size = size;
        }

        _figure.StartPoint = DrawHelper.PolarPoint(center, r, startAngle);
        _arc.Point = DrawHelper.PolarPoint(center, r, startAngle + sweep);
        _arc.IsLargeArc = Math.Abs(sweep) > 180;
        _arc.SweepDirection = sweep >= 0 ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;
    }

    private static double NormalizeRadius(double radius)
        => double.IsFinite(radius) && radius > 0 ? radius : 0.01;
}
