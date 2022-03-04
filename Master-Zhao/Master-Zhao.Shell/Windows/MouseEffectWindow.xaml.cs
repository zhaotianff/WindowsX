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

namespace Master_Zhao.Shell.Windows
{
    /// <summary>
    /// Interaction logic for MouseEffectWindow.xaml
    /// </summary>
    public partial class MouseEffectWindow : Window
    {
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
            Random r = new Random();
            new System.Threading.Thread(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(10);

                    Dispatcher.Invoke(() =>
                    {
                        var point = new PInvoke.POINT();
                        PInvoke.User32.GetCursorPos(ref point);
                        this.canvas.Children.Add(new MyVisualHost(new Point(point.x, point.y)));
                    });
                }
            }).Start();
        }
    }

    public class MyVisualHost : FrameworkElement
    {
        private readonly VisualCollection children;

        protected override int VisualChildrenCount => children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index > children.Count)
                throw new ArgumentOutOfRangeException();

            return children[index];
        }

        public MyVisualHost(Point point)
        {
            children = new VisualCollection(this) { CreateDrawingVisualEllipsed(point) };
            CreateDrawingVisualEllipsed(point);
        }

        private DrawingVisual CreateDrawingVisualEllipsed(Point point)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext dc = drawingVisual.RenderOpen();
            for (int i = 0; i < 10; i++)
            {
                Color color = new Color();
                color.A = (byte)(50 - (5 * i));
                color.R = 0;
                color.G = 0;
                color.B = 0;
                dc.DrawEllipse(new SolidColorBrush(color), null, point, 1 + i, 1 + i);
            }
            dc.Close();
            return drawingVisual;
        }
    }
}
