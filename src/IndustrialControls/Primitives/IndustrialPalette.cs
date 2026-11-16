using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace IndustrialControls.Primitives;

/// <summary>
/// 控件的默认配色（深色工业风格），全部为不可变画刷，可安全跨控件共享。
/// </summary>
internal static class IndustrialPalette
{
    public static readonly ImmutableSolidColorBrush Track = new(Color.FromArgb(0xFF, 0x1E, 0x29, 0x3B));
    public static readonly ImmutableSolidColorBrush Tick = new(Color.FromArgb(0xFF, 0x64, 0x74, 0x8B));
    public static readonly ImmutableSolidColorBrush MajorTick = new(Color.FromArgb(0xFF, 0xCB, 0xD5, 0xE1));
    public static readonly ImmutableSolidColorBrush Label = new(Color.FromArgb(0xFF, 0x94, 0xA3, 0xB8));
    public static readonly ImmutableSolidColorBrush Text = new(Color.FromArgb(0xFF, 0xF8, 0xFA, 0xFC));
    public static readonly ImmutableSolidColorBrush Muted = new(Color.FromArgb(0xFF, 0x94, 0xA3, 0xB8));
    public static readonly ImmutableSolidColorBrush Accent = new(Color.FromArgb(0xFF, 0x22, 0xD3, 0xEE));
    public static readonly ImmutableSolidColorBrush Needle = new(Color.FromArgb(0xFF, 0xFF, 0x5A, 0x5F));
    public static readonly ImmutableSolidColorBrush Hub = new(Color.FromArgb(0xFF, 0x0B, 0x12, 0x20));
    public static readonly ImmutableSolidColorBrush Green = new(Color.FromArgb(0xFF, 0x34, 0xD3, 0x99));
    public static readonly ImmutableSolidColorBrush Amber = new(Color.FromArgb(0xFF, 0xFB, 0xBF, 0x24));
    public static readonly ImmutableSolidColorBrush Red = new(Color.FromArgb(0xFF, 0xF8, 0x71, 0x71));
    public static readonly ImmutableSolidColorBrush Blue = new(Color.FromArgb(0xFF, 0x60, 0xA5, 0xFA));
    public static readonly ImmutableSolidColorBrush Dim = new(Color.FromArgb(0xFF, 0x47, 0x55, 0x69));
}
