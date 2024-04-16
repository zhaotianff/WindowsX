using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsX.Shell.Controls
{
    public class PercentageBar : Control
    {
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(float), typeof(PercentageBar),new PropertyMetadata(100f));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(float), typeof(PercentageBar),new PropertyMetadata(50f));

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(Brush), typeof(PercentageBar),new PropertyMetadata(Brushes.Green));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(PercentageBar), new PropertyMetadata(Brushes.Black));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(PercentageBar),new PropertyMetadata(""));


        public float Maximum
        {
            get => (float)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public float Value
        {
            get => (float)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        static PercentageBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PercentageBar),
                new FrameworkPropertyMetadata(typeof(PercentageBar)));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var percentage = this.Value / this.Maximum;
            var barWidth = this.Width - 60;
            var fillWidth = barWidth * percentage;
            drawingContext.DrawGeometry(Brushes.Transparent, new Pen(Stroke, 1), new RectangleGeometry() { Rect = new Rect(0, 0, barWidth, this.Height) });
            drawingContext.DrawGeometry(Fill, new Pen(Stroke, 0), new RectangleGeometry() { Rect = new Rect(0, 0, fillWidth, this.Height) });
            FormattedText formattedText = new FormattedText(Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 12, Brushes.Black, 1);
            drawingContext.DrawText(formattedText, new Point(this.Width - 55, (this.Height - formattedText.Height) / 2));
        }
    }
}
