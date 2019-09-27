namespace HealthyHabitat.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    public class CircularProgressBar : ProgressBar
    {
        public CircularProgressBar()
        {
            this.ValueChanged += CircularProgressBar_ValueChanged;
            this.SizeChanged += CircularProgressBar_SizeChanged;

        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == CircularProgressBar.RadiusProperty)
            {
                Width = Height = Radius * 2;
            }
            base.OnPropertyChanged(e);
        }
        void CircularProgressBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Radius = Math.Min(ActualWidth, ActualHeight) / 2;
        }
        void CircularProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CircularProgressBar bar = sender as CircularProgressBar;
            double currentAngle = bar.Angle;
            double targetAngle = e.NewValue / bar.Maximum * 359.999;
            double duration = Math.Abs(currentAngle - targetAngle) / 359.999 * 500;
            DoubleAnimation anim = new DoubleAnimation(currentAngle, targetAngle, TimeSpan.FromMilliseconds(duration > 0 ? duration : 10));

            bar.BeginAnimation(CircularProgressBar.AngleProperty, anim, HandoffBehavior.Compose);

            this.Percentage = bar.Angle / 359.999;
        }

        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Percentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(0.0));

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Angle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(0.0));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(CircularProgressBar), new PropertyMetadata(1.0));

        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(CircularProgressBar), new PropertyMetadata(10d));

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(50.0));


        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(CircularProgressBar), new PropertyMetadata(Brushes.Transparent));

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(CircularProgressBar), new PropertyMetadata(Brushes.LightGray));

        public PenLineCap StartLineCap
        {
            get { return (PenLineCap)GetValue(StartLineCapProperty); }
            set { SetValue(StartLineCapProperty, value); }
        }
        public static readonly DependencyProperty StartLineCapProperty =
            DependencyProperty.Register(nameof(StartLineCap), typeof(PenLineCap), typeof(CircularProgressBar), new PropertyMetadata(PenLineCap.Flat));


        public PenLineCap EndLineCap
        {
            get { return (PenLineCap)GetValue(EndLineCapProperty); }
            set { SetValue(EndLineCapProperty, value); }
        }
        public static readonly DependencyProperty EndLineCapProperty =
            DependencyProperty.Register(nameof(EndLineCap), typeof(PenLineCap), typeof(CircularProgressBar), new PropertyMetadata(PenLineCap.Round));

        public StrokeMode StrokeMode
        {
            get { return (StrokeMode) GetValue(StrokeModeProperty); }
            set { SetValue(StrokeModeProperty, value); }
        }
        public static readonly DependencyProperty StrokeModeProperty =
            DependencyProperty.Register(nameof(StrokeMode), typeof(StrokeMode), typeof(CircularProgressBar), new PropertyMetadata(StrokeMode.Middle));

    }

    public enum StrokeMode
    {
        Middle,
        Inside,
        Outside,
    }
}
