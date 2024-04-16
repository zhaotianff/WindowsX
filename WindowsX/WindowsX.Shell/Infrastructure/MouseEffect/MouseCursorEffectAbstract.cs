using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WindowsX.Shell.Infrastructure.MouseEffect
{
    public abstract class MouseCursorEffectAbstract : UIElement
    {
        private readonly VisualCollection children;
        public static int FixedCount { get; internal set; } = 1;

        protected override int VisualChildrenCount => children.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index > children.Count)
                throw new ArgumentOutOfRangeException();

            return children[index];
        }

        public MouseCursorEffectAbstract(Point point)
        {
            children = new VisualCollection(this) { CreateDrawingVisualEllipsed(point) };
            CreateDrawingVisualEllipsed(point);
        }

        public abstract DrawingVisual CreateDrawingVisualEllipsed(Point point);
    }
}
