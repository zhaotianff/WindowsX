using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public class FollowingDotEffect : MouseCursorEffectAbstract
    {
        public FollowingDotEffect(Point point) : base(point)
        {
            FixedCount = 1;
        }

        public override DrawingVisual CreateDrawingVisualEllipsed(Point point)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext dc = drawingVisual.RenderOpen();
            for (int i = 0; i < 1; i++)
            {
                Color color = new Color();
                color.A = 128;
                color.R = 0;
                color.G = 128;
                color.B = 255;
                dc.DrawEllipse(new SolidColorBrush(color), null, point, 15, 15);
            }
            dc.Close();
            return drawingVisual;
        }
    }
}
