using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Threading;
using IndustrialControls.Controls;

namespace IndustrialControls.Demo.Views;

public partial class StressTestView : UserControl
{
    private const double BatchesPerSecond = 500;

    private readonly Gauge[] _gauges;
    private readonly SectorProgressBar[] _sectors;
    private readonly VerticalProgressBar[] _bars;
    private readonly DispatcherTimer _statsTimer;
    private readonly DispatcherTimer _heartbeatTimer;

    private CancellationTokenSource? _cts;
    private Thread? _worker;
    private long _writes;
    private long _lastWrites;
    private int _heartbeats;
    private int _lastHeartbeats;

    public StressTestView()
    {
        InitializeComponent();

        _gauges = new[] { G1, G2, G3, G4, G5, G6 };
        _sectors = new[] { S1, S2, S3 };
        _bars = new[] { V1, V2, V3 };

        _heartbeatTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _heartbeatTimer.Tick += (_, _) => _heartbeats++;
        _heartbeatTimer.Start();

        _statsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _statsTimer.Tick += (_, _) => UpdateStats();
        _statsTimer.Start();

        RunToggle.IsCheckedChanged += (_, _) =>
        {
            if (RunToggle.IsChecked == true)
            {
                StartWorker();
                RunToggle.Content = "停止压测";
            }
            else
            {
                StopWorker();
                RunToggle.Content = "开始压测";
            }
        };
    }

    private void StartWorker()
    {
        if (_worker is { IsAlive: true })
        {
            return;
        }

        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;
        _worker = new Thread(() => RunLoop(token))
        {
            IsBackground = true,
            Name = "IndustrialControls.StressTest",
            Priority = ThreadPriority.BelowNormal,
        };
        _worker.Start();
    }

    private void StopWorker()
    {
        _cts?.Cancel();
        Thread? worker = _worker;
        if (worker is not null && worker.IsAlive)
        {
            worker.Join(500);
        }

        _worker = null;
        _cts?.Dispose();
        _cts = null;
    }

    /// <summary>后台线程：以固定批次频率直接写入控件值，模拟高数据量。</summary>
    private void RunLoop(CancellationToken token)
    {
        var watch = Stopwatch.StartNew();
        double nextBatch = 0;
        double interval = 1.0 / BatchesPerSecond;
        double phase = 0;

        while (!token.IsCancellationRequested)
        {
            double now = watch.Elapsed.TotalSeconds;
            if (now < nextBatch)
            {
                Thread.SpinWait(200);
                continue;
            }

            nextBatch = now + interval;
            phase += 0.08;

            long writes = 0;
            for (int i = 0; i < _gauges.Length; i++)
            {
                _gauges[i].Value = 50 + 40 * Math.Sin(phase + i * 0.7);
                writes++;
            }

            for (int i = 0; i < _sectors.Length; i++)
            {
                _sectors[i].Value = 50 + 45 * Math.Sin(phase * 1.2 + i * 1.3);
                writes++;
            }

            for (int i = 0; i < _bars.Length; i++)
            {
                _bars[i].Value = 50 + 42 * Math.Cos(phase * 0.9 + i * 1.1);
                writes++;
            }

            Interlocked.Add(ref _writes, writes);
        }
    }

    private void UpdateStats()
    {
        long writes = Interlocked.Read(ref _writes);
        long writeDelta = writes - _lastWrites;
        _lastWrites = writes;

        int heartbeatDelta = _heartbeats - _lastHeartbeats;
        _lastHeartbeats = _heartbeats;

        WritesText.Text = $"控件写入 {writeDelta:N0} 次/秒";
        HeartbeatText.Text = $"UI 心跳 {heartbeatDelta} 次/秒";
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        // 切换标签页时停止压测，并把按钮状态复位，避免“已停止但按钮显示运行中”
        RunToggle.IsChecked = false;
        StopWorker();
        _statsTimer.Stop();
        _heartbeatTimer.Stop();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!_statsTimer.IsEnabled)
        {
            _statsTimer.Start();
        }

        if (!_heartbeatTimer.IsEnabled)
        {
            _heartbeatTimer.Start();
        }
    }
}
