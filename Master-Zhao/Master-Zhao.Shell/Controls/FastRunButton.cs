using Master_Zhao.Config.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Controls
{
 
    public class FastRunButton : Button
    {
        private const string PART_MAINGRID = "grid";

        //TODO icon image buffer

        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register("ImagePath", typeof(string), typeof(FastRunButton));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(FastRunButton));
        public static readonly DependencyProperty ShadowRadiusProperty = DependencyProperty.Register("ShadowRadius", typeof(float), typeof(FastRunButton));
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(float), typeof(FastRunButton));
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register("Radius", typeof(float), typeof(FastRunButton));
        public static readonly DependencyProperty HostCanvasProperty = DependencyProperty.Register("HostCanvas", typeof(Canvas), typeof(FastRunButton));
        public static readonly DependencyProperty ContentRadiusXProperty = DependencyProperty.Register("ContentRadiusX", typeof(float), typeof(FastRunButton));
        public static readonly DependencyProperty ContentRadiusYProperty = DependencyProperty.Register("ContentRadiusY", typeof(float), typeof(FastRunButton));
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(Point), typeof(FastRunButton));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(FastRunButton));       

        private UIElementCollection uIElementCollection;

        public string ImagePath
        {
            get => GetValue(ImagePathProperty).ToString();
            set => SetValue(ImagePathProperty, value);
        }

        public string Title
        {
            get => GetValue(TitleProperty).ToString();
            set => SetValue(TitleProperty, value);
        }

        public float ShadowRadius
        {
            get => float.Parse(GetValue(ShadowRadiusProperty).ToString());
            set => SetValue(ShadowRadiusProperty, value);
        }

        public float Angle
        {
            get => float.Parse(GetValue(AngleProperty).ToString());
            set => SetValue(AngleProperty, value);
        }

        public float Radius
        {
            get => float.Parse(GetValue(RadiusProperty).ToString());
            set => SetValue(RadiusProperty, value);
        }

        public Canvas HostCanvas
        {
            get => GetValue(HostCanvasProperty) as Canvas;
            set => SetValue(HostCanvasProperty, value);
        }

        public float ContentRadiusX
        {
            get => float.Parse(GetValue(ContentRadiusXProperty).ToString());
            set => SetValue(ContentRadiusXProperty, value);
        }

        public float ContentRadiusY
        {
            get => float.Parse(GetValue(ContentRadiusYProperty).ToString());
            set => SetValue(ContentRadiusYProperty, value);
        }

        public Point Center
        {
            get => (Point)(GetValue(CenterProperty));
            set => SetValue(CenterProperty, value);
        }
    
        public bool IsSelected
        {
            get => (bool)(GetValue(IsSelectedProperty));
            set => SetValue(IsSelectedProperty, value);
        }

        public FastRunItem FastRunItem { get; set; }

        public UIElementCollection Children
        {
            get
            {
                if (uIElementCollection == null)
                    uIElementCollection = CreateUIElementCollection(this);
                return uIElementCollection;
            }
        }

        private UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            return new UIElementCollection(this, logicalParent);
        }

        static FastRunButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FastRunButton), new FrameworkPropertyMetadata(typeof(FastRunButton)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitPos();
        }

        private void InitPos()
        {
            if (HostCanvas == null)
                return;

            var index = HostCanvas.Children.IndexOf(this);
            SetPos(index,HostCanvas.Width, HostCanvas.Height);
        }

        private void SetPos(int index,double width,double height)
        {
            switch(index)
            {
                case 0:
                    Canvas.SetLeft(this, (width - this.Width) / 2);
                    Canvas.SetTop(this,0);
                    //StartAnimation(Center.X, (width - this.Width) / 2, TimeSpan.FromSeconds(1), this, "(Canvas.Left)");
                    StartAnimation(Center.Y, 0, TimeSpan.FromSeconds(0.2), this, "(Canvas.Top)");
                    break;
                case 1:
                    Canvas.SetRight(this, 0);
                    Canvas.SetTop(this, (height - this.Height )/2);
                    StartAnimation(Center.X, 0, TimeSpan.FromSeconds(0.2), this, "(Canvas.Right)");
                    break;
                case 2:
                    Canvas.SetLeft(this, (width - this.Width) / 2);
                    Canvas.SetBottom(this, 0);
                    StartAnimation(Center.Y, 0, TimeSpan.FromSeconds(0.2), this, "(Canvas.Bottom)");
                    break;
                case 3:
                    Canvas.SetLeft(this, 0);
                    Canvas.SetTop(this, (height - this.Height) / 2);
                    StartAnimation(Center.X, 0, TimeSpan.FromSeconds(0.2), this, "(Canvas.Left)");
                    break;
            }
        }

        private void StartAnimation(double from, double to, TimeSpan duration, Button target,string propertyPath)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = from;
            doubleAnimation.To = to;
            doubleAnimation.Duration = new Duration(duration); 
            storyboard.Children.Add(doubleAnimation);
            Storyboard.SetTarget(storyboard, target);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(propertyPath));
            storyboard.Begin();
        }
    }
}
