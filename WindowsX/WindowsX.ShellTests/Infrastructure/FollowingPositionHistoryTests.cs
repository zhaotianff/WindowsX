using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Windows;
using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Shell.Windows;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    public class FollowingPositionHistoryTests
    {
        [TestMethod]
        public void ZeroDelayReturnsCurrentPositionWithoutHistory()
        {
            var history = new FollowingPositionHistory();
            Assert.AreEqual(new Point(10, 20), history.Update(new Point(10, 20), TimeSpan.Zero, 0));
            Assert.AreEqual(new Point(100, 200), history.Update(new Point(100, 200), TimeSpan.FromMilliseconds(10), 0));
            Assert.AreEqual(0, history.SampleCount);
        }

        [TestMethod]
        public void DelayUsesTimestampInterpolationInsteadOfUpdateInterval()
        {
            var history = new FollowingPositionHistory();
            Assert.AreEqual(new Point(0, 0), history.Update(new Point(0, 0), TimeSpan.Zero, 100));
            Assert.AreEqual(new Point(0, 0), history.Update(new Point(100, 200), TimeSpan.FromMilliseconds(100), 100));
            Assert.AreEqual(new Point(50, 100), history.Update(new Point(150, 300), TimeSpan.FromMilliseconds(150), 100));
            Assert.AreEqual(new Point(100, 200), history.Update(new Point(200, 400), TimeSpan.FromMilliseconds(200), 100));
        }

        [DataTestMethod]
        [DataRow(10.0)]
        [DataRow(16.0)]
        [DataRow(33.0)]
        [DataRow(150.0)]
        public void DelayIsIndependentOfFrameInterval(double interval)
        {
            var history = new FollowingPositionHistory();
            for (var i = 0; i < 100; i++)
            {
                var time = i * interval;
                var point = history.Update(new Point(time, time * 2), TimeSpan.FromMilliseconds(time), 100);
                Assert.AreEqual(Math.Max(0, time - 100), point.X, 0.0001);
                Assert.AreEqual(Math.Max(0, time - 100) * 2, point.Y, 0.0001);
            }
        }

        [TestMethod]
        public void StationaryCursorIsReachedAfterDelay()
        {
            var history = new FollowingPositionHistory();
            history.Update(new Point(0, 0), TimeSpan.Zero, 100);
            history.Update(new Point(100, 0), TimeSpan.FromMilliseconds(50), 100);
            Assert.AreEqual(new Point(0, 0), history.Update(new Point(100, 0), TimeSpan.FromMilliseconds(100), 100));
            Assert.AreEqual(new Point(50, 0), history.Update(new Point(100, 0), TimeSpan.FromMilliseconds(125), 100));
            Assert.AreEqual(new Point(100, 0), history.Update(new Point(100, 0), TimeSpan.FromMilliseconds(150), 100));
            Assert.AreEqual(new Point(100, 0), history.Update(new Point(100, 0), TimeSpan.FromSeconds(10), 100));
            Assert.IsTrue(history.SampleCount <= 2);
        }

        [TestMethod]
        public void ChangingDelayOrClearingDiscardsOldPositions()
        {
            var history = new FollowingPositionHistory();
            history.Update(new Point(0, 0), TimeSpan.Zero, 100);
            history.Update(new Point(100, 0), TimeSpan.FromMilliseconds(100), 100);
            Assert.AreEqual(new Point(200, 0), history.Update(new Point(200, 0), TimeSpan.FromMilliseconds(200), 300));
            Assert.AreEqual(1, history.SampleCount);
            Assert.AreEqual(new Point(250, 0), history.Update(new Point(250, 0), TimeSpan.FromMilliseconds(250), 0));
            Assert.AreEqual(0, history.SampleCount);
            history.Update(new Point(300, 0), TimeSpan.FromMilliseconds(300), 100);
            history.Clear();
            Assert.AreEqual(0, history.SampleCount);
            Assert.AreEqual(new Point(500, 0), history.Update(new Point(500, 0), TimeSpan.FromMilliseconds(500), 100));
        }

        [TestMethod]
        public void DuplicateTimestampsDoNotAccumulateAndClockRewindResetsHistory()
        {
            var history = new FollowingPositionHistory();
            history.Update(new Point(10, 20), TimeSpan.FromSeconds(1), 100);
            for (var i = 0; i < 100; i++)
                history.Update(new Point(10, 20), TimeSpan.FromSeconds(1), 100);
            Assert.AreEqual(1, history.SampleCount);
            Assert.AreEqual(new Point(50, 60), history.Update(new Point(50, 60), TimeSpan.FromMilliseconds(10), 100));
            Assert.AreEqual(1, history.SampleCount);
        }

        [TestMethod]
        public void LongRunningHistoryOnlyRetainsDelayWindow()
        {
            var history = new FollowingPositionHistory();
            for (var i = 0; i < 10000; i++)
            {
                var point = history.Update(new Point(i, 0), TimeSpan.FromMilliseconds(i), 1000);
                Assert.AreEqual(Math.Max(0, i - 1000), point.X, 0.0001);
                Assert.IsTrue(history.SampleCount <= 1001);
            }
        }

        [DataTestMethod]
        [DataRow(-1.0)]
        [DataRow(1001.0)]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        public void InvalidDelayIsRejected(double delay)
        {
            var history = new FollowingPositionHistory();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => history.Update(new Point(), TimeSpan.Zero, delay));
            Assert.AreEqual(0, history.SampleCount);
        }

        [DataTestMethod]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("switch")]
        [DataRow("close")]
        public void WindowLifecycleClearsFollowingHistory(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    var field = typeof(MouseEffectWindow).GetField("followingPositionHistory", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    var history = (FollowingPositionHistory)field.GetValue(window)!;
                    history.Update(new Point(10, 10), TimeSpan.Zero, 100);
                    history.Update(new Point(100, 100), TimeSpan.FromMilliseconds(50), 100);
                    Assert.IsTrue(history.SampleCount > 0);
                    if (action == "start") window.StartMouseEffect();
                    else if (action == "stop") window.StopMouseEffect();
                    else if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.CursorTrail);
                    else window.Close();
                    Assert.AreEqual(0, history.SampleCount);
                }
                finally { window.Close(); }
            });
        }

        private static void RunSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { failure = ex; }
                finally { System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null)
                Assert.Fail(failure.ToString());
        }
    }
}
