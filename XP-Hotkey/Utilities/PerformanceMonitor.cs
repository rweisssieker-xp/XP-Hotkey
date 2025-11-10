using System.Collections.Concurrent;
using System.Diagnostics;

namespace XP_Hotkey.Utilities;

public class PerformanceMonitor
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<long>> _measurements = new();
    private const int MaxMeasurements = 100;

    public void StartMeasurement(string operationId, out Stopwatch stopwatch)
    {
        stopwatch = Stopwatch.StartNew();
    }

    public void EndMeasurement(string operationId, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        var queue = _measurements.GetOrAdd(operationId, _ => new ConcurrentQueue<long>());
        queue.Enqueue(elapsedMs);

        // Keep only last 100 measurements
        while (queue.Count > MaxMeasurements)
        {
            queue.TryDequeue(out _);
        }
    }

    public double GetAverageLatency(string operationId)
    {
        if (!_measurements.TryGetValue(operationId, out var measurements))
            return 0;

        var snapshot = measurements.ToArray();
        return snapshot.Length > 0 ? snapshot.Average() : 0;
    }

    public long GetMaxLatency(string operationId)
    {
        if (!_measurements.TryGetValue(operationId, out var measurements))
            return 0;

        var snapshot = measurements.ToArray();
        return snapshot.Length > 0 ? snapshot.Max() : 0;
    }

    public void ClearMeasurements(string? operationId = null)
    {
        if (operationId == null)
        {
            _measurements.Clear();
        }
        else
        {
            _measurements.TryRemove(operationId, out _);
        }
    }
}

