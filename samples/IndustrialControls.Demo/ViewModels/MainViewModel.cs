using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using IndustrialControls;

namespace IndustrialControls.Demo.ViewModels;

/// <summary>示例主视图模型：用定时器模拟工业现场数据。</summary>
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

    private double _temperature = 23.6;
    private double _humidity = 56.0;
    private double _windSpeed = 4.2;
    private double _windDirection = 45;
    private double _pressure = 101.3;
    private double _batteryLevel = 78;
    private bool _isCharging;
    private IndicatorStatus _deviceStatus = IndicatorStatus.Normal;
    private string _lastUpdateText = "最后更新 --:--:--";

    private double _demoValue = 62;
    private double _demoStartAngle = 150;
    private double _demoSweepAngle = 240;
    private double _demoTrackThickness = 12;
    private double _demoNeedleLength = 0.88;

    public MainViewModel()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public double Temperature
    {
        get => _temperature;
        private set => Set(ref _temperature, value);
    }

    public double Humidity
    {
        get => _humidity;
        private set => Set(ref _humidity, value);
    }

    public double WindSpeed
    {
        get => _windSpeed;
        private set => Set(ref _windSpeed, value);
    }

    public double WindDirection
    {
        get => _windDirection;
        private set => Set(ref _windDirection, value);
    }

    /// <summary>大气压（kPa）。</summary>
    public double Pressure
    {
        get => _pressure;
        private set => Set(ref _pressure, value);
    }

    public double BatteryLevel
    {
        get => _batteryLevel;
        private set => Set(ref _batteryLevel, value);
    }

    public bool IsCharging
    {
        get => _isCharging;
        private set => Set(ref _isCharging, value);
    }

    public IndicatorStatus DeviceStatus
    {
        get => _deviceStatus;
        private set => Set(ref _deviceStatus, value);
    }

    public string LastUpdateText
    {
        get => _lastUpdateText;
        private set => Set(ref _lastUpdateText, value);
    }

    public double DemoValue
    {
        get => _demoValue;
        set => Set(ref _demoValue, value);
    }

    public double DemoStartAngle
    {
        get => _demoStartAngle;
        set => Set(ref _demoStartAngle, value);
    }

    public double DemoSweepAngle
    {
        get => _demoSweepAngle;
        set => Set(ref _demoSweepAngle, value);
    }

    public double DemoTrackThickness
    {
        get => _demoTrackThickness;
        set => Set(ref _demoTrackThickness, value);
    }

    public double DemoNeedleLength
    {
        get => _demoNeedleLength;
        set => Set(ref _demoNeedleLength, value);
    }

    private void Tick()
    {
        Temperature = Walk(Temperature, 0.4, 18, 32);
        Humidity = Walk(Humidity, 1.0, 35, 85);
        WindSpeed = Walk(WindSpeed, 0.35, 0.3, 14);
        WindDirection = (WindDirection + _random.NextDouble() * 12 - 6 + 360) % 360;
        Pressure = Walk(Pressure, 0.05, 99.5, 102.5);
        BatteryLevel = Walk(BatteryLevel, 0.1, 5, 100);

        if (_random.NextDouble() < 0.004)
        {
            IsCharging = !IsCharging;
        }

        LastUpdateText = $"最后更新 {DateTime.Now:HH:mm:ss}";
    }

    private double Walk(double value, double maxDelta, double min, double max)
    {
        double next = value + (_random.NextDouble() * 2 - 1) * maxDelta;
        return Math.Clamp(next, min, max);
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
