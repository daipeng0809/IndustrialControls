using System.Collections.Concurrent;
using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace IndustrialControls.Primitives;

/// <summary>
/// 无状态绘图辅助方法。所有角度均为“从 3 点钟方向开始、顺时针为正”的屏幕角度（单位：度）。
/// </summary>
internal static class DrawHelper
{
    /// <summary>验证失败的格式串缓存：避免每帧重复抛 <see cref="FormatException"/>。</summary>
    private static readonly ConcurrentDictionary<string, byte> InvalidFormats = new(StringComparer.Ordinal);

    /// <summary>把极坐标转换为屏幕坐标。</summary>
    public static Point PolarPoint(Point center, double radius, double angleDegrees)
    {
        double radians = angleDegrees * Math.PI / 180.0;
        return new Point(
            center.X + radius * Math.Cos(radians),
            center.Y + radius * Math.Sin(radians));
    }

    /// <summary>数值在量程内的百分比（0~1）。</summary>
    public static double Normalize(double value, double minimum, double maximum)
        => maximum <= minimum ? 0 : Math.Clamp((value - minimum) / (maximum - minimum), 0, 1);

    /// <summary>数值对应的角度。</summary>
    public static double AngleForValue(double value, double minimum, double maximum, double startAngle, double sweepAngle)
        => startAngle + sweepAngle * Normalize(value, minimum, maximum);

    public static bool IsFullCircle(double sweepAngle) => Math.Abs(sweepAngle) >= 359.999;

    /// <summary>创建一段圆弧几何（仅描边）。完整圆请直接用 DrawEllipse，避免起终点重合。</summary>
    public static StreamGeometry CreateArc(Point center, double radius, double startAngle, double sweepAngle)
    {
        var geometry = new StreamGeometry();
        // 必须按符号截断：直接取绝对值会把逆时针圆弧翻转为顺时针
        double sweep = Math.Clamp(sweepAngle, -359.999, 359.999);
        if (Math.Abs(sweep) < 0.001 || radius <= 0)
        {
            return geometry;
        }

        Point start = PolarPoint(center, radius, startAngle);
        Point end = PolarPoint(center, radius, startAngle + sweep);

        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(start, false);
            context.ArcTo(
                end,
                new Size(radius, radius),
                0,
                Math.Abs(sweep) > 180,
                sweep >= 0 ? SweepDirection.Clockwise : SweepDirection.CounterClockwise);
            context.EndFigure(false);
        }

        return geometry;
    }

    /// <summary>创建闭合多边形。</summary>
    public static StreamGeometry CreatePolygon(IReadOnlyList<Point> points)
    {
        var geometry = new StreamGeometry();
        if (points.Count < 3)
        {
            return geometry;
        }

        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(points[0], true);
            for (int i = 1; i < points.Count; i++)
            {
                context.LineTo(points[i]);
            }

            context.EndFigure(true);
        }

        return geometry;
    }

    /// <summary>创建文本；文本为空、画刷为空或字号非法时返回 null。</summary>
    public static FormattedText? CreateText(string? text, FontFamily? fontFamily, FontWeight fontWeight,
        double fontSize, IBrush? brush)
    {
        if (string.IsNullOrEmpty(text) || brush is null || fontSize <= 0 || double.IsNaN(fontSize))
        {
            return null;
        }

        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily ?? FontFamily.Default, FontStyle.Normal, fontWeight, FontStretch.Normal),
            fontSize,
            brush);
    }

    /// <summary>以某个点为中心绘制文本。</summary>
    public static void DrawTextCentered(DrawingContext context, FormattedText text, Point center)
        => context.DrawText(text, new Point(center.X - text.Width / 2, center.Y - text.Height / 2));

    /// <summary>按格式字符串格式化数值，非法格式不会抛出异常。</summary>
    public static string FormatValue(double value, string? format)
    {
        string actualFormat = string.IsNullOrWhiteSpace(format) ? "F1" : format;
        if (InvalidFormats.ContainsKey(actualFormat))
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        try
        {
            return value.ToString(actualFormat, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            InvalidFormats.TryAdd(actualFormat, 0);
            return value.ToString(CultureInfo.CurrentCulture);
        }
    }

    /// <summary>按 DPI 无关的线宽创建画笔。</summary>
    public static Pen CreatePen(IBrush? brush, double thickness, PenLineCap lineCap = PenLineCap.Flat,
        PenLineJoin lineJoin = PenLineJoin.Miter)
        => new(brush, thickness, null, lineCap, lineJoin);
}
