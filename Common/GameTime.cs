using System.Diagnostics;
using System;

namespace Common
{
    public class GameTime
    {
        public TimeSpan TimeOfDay { get; private set; }
        public double SecondsPerSecond { get; set; } = 60 * 60; // Default is real-time

        public long TotalElapsed { get; private set; }
        public long FrameCount { get; private set; }
        public long Elapsed => _stopwatch.ElapsedMilliseconds;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public GameTime()
        {
            TimeOfDay = new TimeSpan(0); // Starting at midnight
        }

        public void StartFrame()
        {
            _stopwatch.Restart();
        }

        public void EndFrame()
        {
            long elapsedMilliseconds = Elapsed;
            TotalElapsed += elapsedMilliseconds;
            FrameCount++;

            UpdateTimeOfDay(elapsedMilliseconds);
        }

        private void UpdateTimeOfDay(long elapsedMilliseconds)
        {
            var elapsedGameSeconds = elapsedMilliseconds / 1000.0 * SecondsPerSecond;
            TimeOfDay = TimeOfDay.Add(TimeSpan.FromSeconds(elapsedGameSeconds));

            // Wrap around if it goes over 24 hours
            if (TimeOfDay.TotalHours >= 24)
                TimeOfDay = TimeOfDay.Subtract(TimeSpan.FromHours(24));
        }

        public double CalculateAverageFps()
        {
            return FrameCount == 0 ? 0 : (double) FrameCount / TotalElapsed * 1000.0;
        }

        public void Reset()
        {
            _stopwatch.Reset();
            FrameCount = 0;
            TotalElapsed = 0;
            TimeOfDay = TimeSpan.Zero;
        }
    }
}
