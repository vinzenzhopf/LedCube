using System;
using System.Linq;

namespace LedCube.Core.Util;

public class StatisticList
{
    public double[] Data { get; }
    public int Samples { get; }

    public double Last { get; private set; }
    public double Mean { get; private set; }
    public double Min { get; private set; }
    public double Max { get; private set; }
    public double StdDev { get; private set; }
    public double StdErr { get; private set; }
    
    public double Median { get; private set;}
    public double Pct99 { get; private set; }
    public double Pct95 { get; private set; }
    public double Pct05 { get; private set; }

    public int CurrentSamples => _currentSamples;

    private int _head;
    private int _currentSamples;

    public StatisticList(int samples)
    {
        Samples = samples;
        Data = new double[Samples];
        _head = 0;
        _currentSamples = 0;
    }

    public void Reset()
    {
        _head = 0;
        _currentSamples = 0;
        for(var i = 0; i < Samples; i++)
        {
            Data[i] = 0;
        }
        Last = 0;
        Min = 0;
        Max = 0;
        Mean = 0;
        StdDev = 0;
        StdErr = 0;
        Median = 0;
        Pct99 = 0;
        Pct95 = 0;
        Pct05 = 0;
    }

    public void AddValue(double v)
    {
        Last = v;
        Data[_head] = v;
        _head = (_head + 1) % Samples;
        if (_currentSamples < Samples)
        {
            _currentSamples++;
        }
        UpdateStats();
    }

    private void UpdateStats()
    {
        var ordered = Data.Take(_currentSamples).Order().ToArray();
        var sum = 0.0;
        var min = double.MaxValue;
        var max = double.MinValue;
        for (var i = 0; i < _currentSamples; i++)
        {
            sum += ordered[i];
            if (min > ordered[i]) min = ordered[i];
            if (max < ordered[i]) max = ordered[i];
        }
        var mean = sum / _currentSamples;
        var varianceSum = 0.0;
        for (var i = 0; i < _currentSamples; i++)
        {
            varianceSum += Math.Pow(ordered[i] - mean, 2);
        }
        var variance = varianceSum / _currentSamples;
        var stdDev = Math.Sqrt(variance);
        var stdErr = stdDev / Math.Sqrt(_currentSamples);
        
        Min = min;
        Max = max;
        Mean = mean;
        StdDev = stdDev;
        StdErr = stdErr;
        Median = Percentile(0.5, ref ordered);
        Pct99 = Percentile(0.99, ref ordered);
        Pct95 = Percentile(0.95, ref ordered);
        Pct05 = Percentile(0.05, ref ordered);
    }

    public static double Percentile(double d, ref double[] arr)
    {
        if (d is >= 1 or <= 0)
            throw new ArgumentException($"{nameof(d)} is out of range!");
        if (arr.Length == 1)
            return arr[0];
        var pos = arr.Length * d;
        var i = (int)pos;
        if ((pos % 1) == 0)
            return (arr[i-1] + arr[i]) / 2;
        return arr[i];
    }
}