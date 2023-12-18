using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Master_Zhao.Shell.Infrastructure.MouseEffect
{
    public class FollowingDotEffect : FrameworkElement
    {
        public static int fixedCount = 1;

        private readonly VisualCollection children;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index > children.Count)
                throw new ArgumentOutOfRangeException();

            return children[index];
        }

        public FollowingDotEffect(Point point)
        {
            children = new VisualCollection(this) { CreateDrawingVisualEllipsed(point) };
            CreateDrawingVisualEllipsed(point);
        }

        public DrawingVisual CreateDrawingVisualEllipsed(Point point)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext dc = drawingVisual.RenderOpen();
            Color color = new Color();
            color.A = 20;
            color.R = 0;
            color.G = 128;
            color.B = 255;
            dc.DrawEllipse(new SolidColorBrush(color), null, point, 20, 20);
            dc.Close();
            return drawingVisual;
        }
    }
}
