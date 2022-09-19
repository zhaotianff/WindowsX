using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Master_Zhao.Shell.Controls
{
    public class DrawingImageControl : FrameworkElement
    {
        private DrawingVisual drawingVisual;
        private string imagePath;
        private MouseButtonEventHandler clickHandler;

        public string ImagePath 
        { 
            get => imagePath;
            set
            {
                imagePath = value;
                CreateDrawingVisualImage(Width, Height, imagePath);
            } 
        }

        public MouseButtonEventHandler ClickHandler 
        {
            get => clickHandler; 
            set
            {
                clickHandler = value;
                if (clickHandler != null)
                    MouseDown += clickHandler;
            }
        }

        public DrawingImageControl(double width,double height,string path,MouseButtonEventHandler handler)
        {
            this.Width = width;
            this.Height = height;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;

            CreateDrawingVisualImage(width, height, path);

            if(handler != null)
                MouseDown += handler;
        }

        public DrawingImageControl()
        {

        }

        public void CreateDrawingVisualImage(double width, double height, string path)
        {
          
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(new BitmapImage(new Uri(path, UriKind.Absolute)), new Rect(0, 0, width, height));
            drawingContext.Close();
            this.drawingVisual = drawingVisual;
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            return drawingVisual;
        }
    }
}
