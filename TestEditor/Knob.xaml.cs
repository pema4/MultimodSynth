using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BetterSynth.UI
{
    /// <summary>
    /// Interaction logic for Knob.xaml
    /// </summary>
    public partial class Knob : UserControl
    {
        private Point startMousePosition;
        private Point screenMousePosition;
        private bool leftButtonPressed;
        
        public Knob()
        {
            InitializeComponent();
        }
        /*
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(Knob), new PropertyMetadata(Brushes.Red));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(Knob), new PropertyMetadata("Knob"));



        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minumum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minumum", typeof(double), typeof(Knob),
                new PropertyMetadata(
                    0.0, 
                    OnMinumumPropertyChanged));

        private static void OnMinumumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Knob k = d as Knob;
            k.CoerceValue(MaximumProperty);
            k.CoerceValue(ValueProperty);
        }
        
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(Knob), 
                new PropertyMetadata(
                    1.0,
                    OnMaximumPropertyChanged,
                    CoerceMaximumProperty));

        private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var k = d as Knob;
            k.CoerceValue(ValueProperty);
        }

        private static object CoerceMaximumProperty(DependencyObject d, object baseValue)
        {
            var k = d as Knob;
            double maximum = (double)baseValue;
            if (maximum < k.Minimum)
                maximum = k.Minimum;
            return maximum;
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Knob), 
                new PropertyMetadata(
                    0.0,
                    null,
                    CoerceValueProperty));

        private static object CoerceValueProperty(DependencyObject d, object baseValue)
        {
            var k = d as Knob;
            var value = (double)baseValue;
            if (value < k.Minimum)
                value = k.Minimum;
            else if (value > k.Maximum)
                value = k.Maximum;
            return value;
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                leftButtonPressed = true;
                startMousePosition = e.GetPosition(this);
                screenMousePosition = PointToScreen(startMousePosition);
                Mouse.OverrideCursor = Cursors.None;
                ((UIElement)sender).CaptureMouse();
            }
        }

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                leftButtonPressed = false;
                Mouse.OverrideCursor = null;
                ((UIElement)sender).ReleaseMouseCapture();
            }
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftButtonPressed)
            {
                var delta = e.GetPosition(this).Y - startMousePosition.Y;
                Value -= delta / 800;
                Rotate(Value / (Maximum - Minimum));
                SetCursorPos((int)screenMousePosition.X, (int)screenMousePosition.Y);
            }
        }

        private void Rotate(double normalizedValue)
        {
            double angle = Math.PI * (1.25 - 1.5 * normalizedValue);
            var newPoint = new Point
            {
                X = 20 + 16 * Math.Cos(angle),
                Y = 20 - 16 * Math.Sin(angle),
            };
            if (angle < Math.PI / 4)
                coloredArc.IsLargeArc = true;
            else if (angle < 3 * Math.PI / 4)
            {
                coloredArc.IsLargeArc = false;
                emptyArc.IsLargeArc = false;
            }
            else
                emptyArc.IsLargeArc = true;
            coloredArc.Point = newPoint;
            emptyArc.Point = newPoint;
            rotateTransform.Angle = 270 * normalizedValue;
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);



        public int MyProperty
        {
            get { return (int)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("MyProperty", typeof(int), typeof(Knob), new PropertyMetadata(0));

    */
    }
}
