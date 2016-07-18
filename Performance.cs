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
    public const int PacketSkipCount = 100;
    public static Performance Instance { get; } = new Performance();

    // Use a linked list for quick append
    private LinkedList<long> MeasurementLog = new LinkedList<long>();
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

    public void StopSession()
    {
      sessionStopwatch.Stop();
    }

    public void AddMeasurement(long elapsed)
    {
      MeasurementLog.AddLast(elapsed);
    }

    public void CountPacket(int size = 0)
    {
      if (sessionStopwatch != null)
      {
        packetCount++;
        bytesCount += size;
      }
    }

    public void PrintSummary()
    {
      //var log = MeasurementLog.Skip(PacketSkipCount).ToArray();
      //var avg = log.Average();
      //var sqAvg = log.Select(x => x * x).Average();
      //var stdev = Math.Sqrt(sqAvg - (avg * avg));
      var timeFactor = 1000D / TimeSpan.TicksPerMillisecond;

      Console.WriteLine("======================");
      Console.WriteLine("||  Timing results  ||");
      Console.WriteLine("======================");
      //Console.WriteLine();
      //Console.WriteLine("Discarded first {0} measurements.", PacketSkipCount);
      //Console.WriteLine();
      //Console.WriteLine("Processing time per packet:");
      //Console.WriteLine("------------------------------");
      //Console.WriteLine("Number of measurements: {0}", log.Length);
      //Console.WriteLine("Average:                {0}μs", avg * timeFactor);
      //Console.WriteLine("Standard Dev:           {0}μs", stdev * timeFactor);
      Console.WriteLine();
      Console.WriteLine("Session statistics:");
      Console.WriteLine("------------------------------");
      Console.WriteLine("Number of packets processed: {0}", packetCount);
      Console.WriteLine("Duration:                    {0}μs", sessionStopwatch.Elapsed.Ticks * timeFactor);
      if (packetCount > 0)
      {
        Console.WriteLine("Average time per packet:     {0}μs", sessionStopwatch.Elapsed.Ticks / packetCount * timeFactor);
        Console.WriteLine("Average throughput:          {0} packets/s", packetCount / sessionStopwatch.Elapsed.TotalSeconds);
      }
    }
  }
}
