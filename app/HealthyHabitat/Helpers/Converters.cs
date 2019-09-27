namespace HealthyHabitat.Helpers
{
    using HealthyHabitat.Controls;
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;

    public class AngleToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double angle = (double)value;

            return (angle / 359.999) * 100;
        }
        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            double percentage = (double)value;

            return percentage * 359.999;
        }
    }
    public class AngleToPointConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double angle = (double)values[0];
            double radius = (double)values[1];
            double stroke = (double)values[2];
            double piang = angle * Math.PI / 180;

            double px = Math.Sin(piang) * (radius - stroke / 2) + radius;
            double py = -Math.Cos(piang) * (radius - stroke / 2) + radius;
            return new Point(px, py);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AngleToIsLargeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double angle = (double)value;

            return angle > 180;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StrokeToStartPointConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double radius = (double)values[0];
            double stroke = (double)values[1];
            return new Point(radius, stroke / 2);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class RadiusToSizeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double radius = (double)values[0];
            double stroke = (double)values[1];
            return new Size(radius - stroke / 2, radius - stroke / 2);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class RadiusToCenterConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double radius = (double)value;
            return new Point(radius, radius);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class RadiusToDiameter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double radius = (double)value;
            return 2 * radius;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class InnerRadiusConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double radius = (double)values[0];
            double stroke = (double)values[1];
            StrokeMode mode = (StrokeMode) values[2];

            switch (mode)
            {
                case StrokeMode.Inside:
                    return radius - stroke;
                case StrokeMode.Outside:
                    return radius;
                default:
                    return radius - stroke / 2;
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StrokeLineCapConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double progress && progress > 0)
            {
                PenLineCap cap = (PenLineCap) values[1];
                return cap;
            }
            return PenLineCap.Flat;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool visibility = false;
            if (value is bool)
            {
                visibility = (bool)value;
            }
            return visibility ? Visibility.Visible : Visibility.Hidden;
        }
        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return (visibility == Visibility.Visible);
        }
    }
}
