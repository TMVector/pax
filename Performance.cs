using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Pax
{
  // FIXME this class doesn't worry about concurrency
  internal sealed class Performance
  {
    public static Performance Instance { get; } = new Performance();

    // Use a linked list for quick append
    private LinkedList<Measurement> MeasurementLog = new LinkedList<Measurement>();
    private Stopwatch sessionStopwatch;
    private int packetCount = 0;
    private int bytesCount = 0;

    private Performance()
    {
      if (!Stopwatch.IsHighResolution)
        Console.WriteLine("Warning: Stopwatch not high resolution");
      Console.WriteLine("Timer accurate to within {0} nanoseconds", (1000L * 1000L * 1000L) / Stopwatch.Frequency);
    }

    public void StartSession()
    {
      sessionStopwatch = Stopwatch.StartNew();
    }

    public long GetTimestamp()
    {
      return sessionStopwatch.ElapsedTicks;
    }

    public void StopSession()
    {
      sessionStopwatch.Stop();
    }

    public void AddMeasurement(long startTimestamp, long endTimestamp, int packetLength)
    {
      MeasurementLog.AddLast(new Measurement() { Start = startTimestamp, Elapsed = endTimestamp - startTimestamp, PacketLength = packetLength });
    }

    public void CountPacket(int size = 0)
    {
      if (sessionStopwatch != null)
      {
        packetCount++;
        bytesCount += size;
      }
    }

    private string TimeString(long nanoseconds)
    {
      if (nanoseconds > 1000 * 1000 * 1000)
        return nanoseconds / 1000D / 1000 / 1000 + "s";
      else if (nanoseconds > 1000 * 1000)
        return nanoseconds / 1000D / 1000 + "ms";
      else if (nanoseconds > 1000)
        return nanoseconds / 1000D + "μs";
      else
        return nanoseconds + "ns";
    }
    private string DataString(long bytes)
    {
      if (bytes > 1024 * 1024 * 1024)
        return bytes / 1024D / 1024 / 1024 + "GB";
      else if (bytes > 1024 * 1024)
        return bytes / 1024D / 1024 + "MB";
      else if (bytes > 1024)
        return bytes / 1024D + "KB";
      else
        return bytes + "B";
    }
    public void PrintSummary()
    {
      var timeFactor = (double)(1M * 1000 * 1000 / Stopwatch.Frequency); // ticks to μs
      var data = bytesCount;
      var ticks = sessionStopwatch.ElapsedTicks;
      var count = packetCount;

      Console.WriteLine("======================");
      Console.WriteLine("||  Timing results  ||");
      Console.WriteLine("======================");
      Console.WriteLine();
      Console.WriteLine("Timer frequency: {0} ticks/s", Stopwatch.Frequency);
      Console.WriteLine("Scaling to μs: {0} μs", timeFactor);

      int skip = 100, leave = 50;
      if (MeasurementLog.Count > skip + leave)
      {
        count = MeasurementLog.Count - skip - leave;
        Console.WriteLine("Discarded first {0} and last {1} measurements.", skip, leave);
        var log = MeasurementLog.Skip(skip).Take(count).ToArray();
        var times = log.Select(m => m.Elapsed);
        var avgTime = times.Average() * timeFactor;
        var sqAvgTime = times.Select(x => x * x).Average() * timeFactor * timeFactor;
        var stdevTime = Math.Sqrt(sqAvgTime - (avgTime * avgTime));
        Console.WriteLine();
        Console.WriteLine("Processing time per packet:");
        Console.WriteLine("------------------------------");
        Console.WriteLine("Number of measurements: {0}", log.Length);
        Console.WriteLine("Average:                {0}μs", avgTime);
        Console.WriteLine("Standard Dev:           {0}μs", stdevTime);
        Console.WriteLine();

        data = log.Select(m => m.PacketLength).Sum();
        ticks = log.Last().Start + log.Last().Elapsed - log.First().Start;
      }

      var sessionTime = ticks * timeFactor;
      var sessionTimeS = ticks * (timeFactor / 1000 / 1000);
      Console.WriteLine("Session statistics:");
      Console.WriteLine("------------------------------");
      Console.WriteLine("Number of packets processed: {0}", count);
      Console.WriteLine("Amount of data processed:    {0}B / {1}", data, DataString(data));
      Console.WriteLine("Duration:                    {0}μs / {1}", sessionTime, TimeString((long)sessionTime * 1000));
      if (packetCount > 0)
      {
        Console.WriteLine("Average time per packet:     {0}μs", sessionTime / count);
        Console.WriteLine("Average throughput:          {0} packets/s", packetCount / sessionTimeS);
        Console.WriteLine("Average throughput:          {0}B/s / {1}/s", bytesCount / sessionTimeS, DataString((long)(bytesCount / sessionTimeS)));
      }
    }

    private struct Measurement // FIXME is class better?
    {
      public long Start;
      public long Elapsed;
      public int PacketLength;
    }
  }
}
