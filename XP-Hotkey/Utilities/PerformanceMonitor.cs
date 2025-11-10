using System.Collections.Concurrent;
using System.Diagnostics;

namespace XP_Hotkey.Utilities;

public class PerformanceMonitor
{
    private readonly ConcurrentDictionary<string, List<long>> _measurements = new();
    private readonly object _lock = new();

    public void StartMeasurement(string operationId, out Stopwatch stopwatch)
    {
        stopwatch = Stopwatch.StartNew();
    }

    public void EndMeasurement(string operationId, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        _measurements.AddOrUpdate(
            operationId,
            new List<long> { elapsedMs },
            (key, list) =>
            {
                lock (_lock)
                {
                    list.Add(elapsedMs);
                    if (list.Count > 100) // Keep only last 100 measurements
                    {
                        list.RemoveAt(0);
                    }
                }
                return list;
            });
    }

    public double GetAverageLatency(string operationId)
    {
        if (!_measurements.TryGetValue(operationId, out var measurements) || measurements.Count == 0)
            return 0;

        lock (_lock)
        {
            return measurements.Average();
        }
    }

    public long GetMaxLatency(string operationId)
    {
        if (!_measurements.TryGetValue(operationId, out var measurements) || measurements.Count == 0)
            return 0;

        lock (_lock)
        {
            return measurements.Max();
        }
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

