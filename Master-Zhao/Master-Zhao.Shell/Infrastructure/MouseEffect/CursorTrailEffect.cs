using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace Master_Zhao.Shell.Infrastructure.MouseEffect
{
    public class CursorTrailEffect : MouseCursorEffectAbstract
    {

        public CursorTrailEffect(Point point) :base(point)
        {
            FixedCount = 50;
        }

        public override DrawingVisual CreateDrawingVisualEllipsed(Point point)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext dc = drawingVisual.RenderOpen();
            for (int i = 0; i < 10; i++)
            {
                Color color = new Color();
                color.A = 20;
                color.R = 0;
                color.G = 128;
                color.B = 255;
                dc.DrawEllipse(new SolidColorBrush(color), null, point, 10 - i, 10 - i);
            }
            dc.Close();
            return drawingVisual;
        }
    }
}
