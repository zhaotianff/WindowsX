using WindowsX.Shell.Infrastructure.MouseEffect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WindowsX.Shell.Windows
{
    /// <summary>
    /// Interaction logic for MouseEffectWindow.xaml
    /// </summary>
    public partial class MouseEffectWindow : Window
    {
        private bool isRunning = false;
        private int fixedCount = 1;
        private MouseEffectType currentEffectType = MouseEffectType.FollowingDot;

        public MouseEffectWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            var extendedStyle = PInvoke.User32.GetWindowLong(handle, PInvoke.User32.GWL_EXSTYLE);
            PInvoke.User32.SetWindowLong(handle, PInvoke.User32.GWL_EXSTYLE, extendedStyle | PInvoke.User32.WS_EX_TRANSPARENT);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartMouseEffect();
        }

        public void StartMouseEffect()
        {
            isRunning = true;

            Task.Factory.StartNew(async () =>
            {
                while (isRunning)
                {
                    await Task.Delay(10);

                    Dispatcher.Invoke(() =>
                    {
                        var point = new PInvoke.POINT();
                        PInvoke.User32.GetCursorPos(ref point);

                        while (canvas.Children.Count >= fixedCount)
                        {
                            canvas.Children.RemoveAt(0);
                        }

                        this.canvas.Children.Add(GetMouseCursorEffect(new Point(point.x, point.y)));
                    });
                }
            },TaskCreationOptions.LongRunning);

        }

        public void StopMouseEffect()
        {
            isRunning = false;
        }

        public void UpdateMouseEffectType(MouseEffectType mouseEffectType)
        {
            currentEffectType = mouseEffectType;
        }

        public MouseCursorEffectAbstract GetMouseCursorEffect(Point point)
        {
            switch(currentEffectType)
            {
                case MouseEffectType.FollowingDot:
                    fixedCount = FollowingDotEffect.FixedCount;
                    return new FollowingDotEffect(point);
                case MouseEffectType.CursorTrail:
                    fixedCount = CursorTrailEffect.FixedCount;
                    return new CursorTrailEffect(point);
                default:
                    fixedCount = FollowingDotEffect.FixedCount;
                    return new FollowingDotEffect(point);
            }
        }
    }
}
