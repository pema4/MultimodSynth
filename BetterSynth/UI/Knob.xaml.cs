using Jacobi.Vst.Framework;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MultimodSynth.UI
{
    /// <summary>
    /// Interaction logic for Knob.xaml
    /// </summary>
    public partial class Knob : UserControl, INotifyPropertyChanged
    {
        private Point startMousePosition;
        private Point screenMousePosition;
        private bool leftButtonPressed;
        private double startValue;
        private Brush stroke;
        private string shortLabel;
        private double minimum;
        private double maximum;
        private double value = double.NaN;
        private double delta;
        private VstParameterManager manager;
        private string displayValue;

        public Knob()
        {
            InitializeComponent();
            valuePopup.CustomPopupPlacementCallback += PopupPlacementCallback;
        }

        private CustomPopupPlacement[] PopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            return new[]
            {
                new CustomPopupPlacement
                {
                    PrimaryAxis = PopupPrimaryAxis.Horizontal,
                    Point = new Point
                    {
                        X = (targetSize.Width - popupSize.Width) / 2,
                        Y = targetSize.Height,
                    }
                }
            };
        }

        public void AttachTo(VstParameterManager manager, Brush stroke, Func<double, string> valueConverter = null)
        {
            manager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(manager.CurrentValue) || e.PropertyName == nameof(manager.ActiveParameter))
                    SetValue(manager.CurrentValue);
            };

            this.manager = manager;
            DisplayValueConverter = valueConverter;
            Minimum = manager.ParameterInfo.MinInteger;
            Maximum = manager.ParameterInfo.MaxInteger;
            Stroke = stroke;
            SetValue(manager.CurrentValue);
            delta = (Maximum - Minimum) / 400;
        }

        public void UpdateDisplayValue()
        {
            var temp = DisplayValueConverter?.Invoke(value) ?? value.ToString("F3");
            DisplayValue = temp;
            var normalizedValue = (value - Minimum) / (Maximum - Minimum);
            Dispatcher.Invoke(() => Rotate(normalizedValue));
        }

        private void UpdateParameterValue()
        {
            var parameter = manager.ActiveParameter;
            parameter.Value = (float)value;

            var hostAutomation = manager.HostAutomation;
            if (hostAutomation != null)
            {
                hostAutomation.BeginEditParameter(parameter);
                manager.HostAutomation.NotifyParameterValueChanged(parameter);
            }
        }

        public Brush Stroke
        {
            get => stroke;
            set
            {
                if (stroke != value)
                {
                    stroke = value;
                    OnPropertyChanged(nameof(Stroke));
                }
            }
        }

        public double Minimum
        {
            get => minimum;
            set
            {
                if (minimum != value)
                {
                    minimum = value;
                    OnPropertyChanged(nameof(Minimum));
                }
            }
        }

        public double Maximum
        {
            get => maximum;
            set
            {
                if (maximum != value)
                {
                    maximum = value;
                    OnPropertyChanged(nameof(Maximum));
                }
            }
        }

        public void SetValue(double newValue)
        {
            if (newValue != value)
            {
                if (newValue < Minimum)
                    newValue = Minimum;
                else if (newValue > Maximum)
                    newValue = Maximum;
                value = newValue;
                UpdateParameterValue();
                UpdateDisplayValue();
            }
        }

        public string DisplayValue
        {
            get => displayValue;
            set
            {
                if (displayValue != value)
                {
                    displayValue = value;
                    OnPropertyChanged(nameof(DisplayValue));
                }
            }
        }

        public Func<double, string> DisplayValueConverter { get; set; }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                leftButtonPressed = true;
                startValue = value;
                startMousePosition = e.GetPosition(this);
                screenMousePosition = PointToScreen(startMousePosition);
                Mouse.OverrideCursor = Cursors.None;
                ((UIElement)sender).CaptureMouse().ToString();
            }
        }

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                leftButtonPressed = false;
                SetCursorPos((int)screenMousePosition.X, (int)screenMousePosition.Y);
                Mouse.OverrideCursor = null;
                ((UIElement)sender).ReleaseMouseCapture();
            }
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftButtonPressed)
            {
                SetValue(startValue - delta * (e.GetPosition(this).Y - startMousePosition.Y));
            }
        }

        protected virtual void Rotate(double normalizedValue)
        {
            double angle = Math.PI * (1.25 - 1.5 * normalizedValue);
            var newPoint = new Point
            {
                X = 20 + 16 * Math.Cos(angle),
                Y = 20 - 16 * Math.Sin(angle),
            };
            if (angle < Math.PI / 4)
                coloredArc.IsLargeArc = true;
            else
                coloredArc.IsLargeArc = false;
            coloredArc.Point = newPoint;
            rotateTransform.Angle = 270 * normalizedValue;
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }
    }
}
