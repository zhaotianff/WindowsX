using System;
using System.Collections.Generic;
using System.Windows;
using WindowsX.Config.Model;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public sealed class FollowingPositionHistory
    {
        private readonly Queue<(TimeSpan Time, Point Position)> samples = new Queue<(TimeSpan, Point)>();
        private (TimeSpan Time, Point Position)? previous;
        private TimeSpan? lastTimestamp;
        private double lastDelay;

        public int SampleCount => samples.Count + (previous.HasValue ? 1 : 0);

        public Point Update(Point cursor, TimeSpan timestamp, double delayMilliseconds)
        {
            if (!double.IsFinite(delayMilliseconds) || delayMilliseconds < 0 || delayMilliseconds > MouseEffectSetting.MaxFollowingDelayMilliseconds)
                throw new ArgumentOutOfRangeException(nameof(delayMilliseconds));
            if (!double.IsFinite(cursor.X) || !double.IsFinite(cursor.Y))
                throw new ArgumentOutOfRangeException(nameof(cursor));
            if (timestamp < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timestamp));

            if (delayMilliseconds == 0)
            {
                Clear();
                return cursor;
            }

            if (delayMilliseconds != lastDelay || (lastTimestamp.HasValue && timestamp < lastTimestamp.Value))
                Clear();

            lastDelay = delayMilliseconds;
            if (!lastTimestamp.HasValue || timestamp > lastTimestamp.Value)
            {
                samples.Enqueue((timestamp, cursor));
                lastTimestamp = timestamp;
            }

            var targetTime = timestamp - TimeSpan.FromMilliseconds(delayMilliseconds);
            while (samples.Count > 0 && samples.Peek().Time <= targetTime)
                previous = samples.Dequeue();

            if (samples.Count == 0)
                return previous.Value.Position;
            if (!previous.HasValue)
                return samples.Peek().Position;

            var next = samples.Peek();
            var before = previous.Value;
            var progress = (targetTime - before.Time).TotalSeconds / (next.Time - before.Time).TotalSeconds;
            return before.Position + (next.Position - before.Position) * progress;
        }

        public void Clear()
        {
            samples.Clear();
            previous = null;
            lastTimestamp = null;
            lastDelay = 0;
        }
    }
}
