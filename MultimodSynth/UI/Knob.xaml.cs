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
        /// <summary>
        /// Показывает, как сильно меняется значение при перемещении на один пиксель.
        /// </summary>
        private const double Delta = 0.0015;

        /// <summary>
        /// Начальная позиция мыши в момент её захвата относительно элемента.
        /// </summary>
        private Point startMousePosition;

        /// <summary>
        /// Начальная позиция мыши в момент её захвата относительно экрана.
        /// </summary>
        private Point startScreenMousePosition;

        /// <summary>
        /// Расстояние по вертикали, которое прошла мышь с момента её захвата.
        /// </summary>
        private double accumulatedValue = 0;

        /// <summary>
        /// Показывает, нажата ли левая кнопка мыши.
        /// </summary>
        private bool leftButtonPressed;

        /// <summary>
        /// Текущее значение.
        /// </summary>
        private double value = double.NaN;

        /// <summary>
        /// Кисть, используемая для
        /// </summary>
        private Brush stroke;

        /// <summary>
        /// Связанный с ручкой объект типа VstParameterManager.
        /// </summary>
        private VstParameterManager manager;

        /// <summary>
        /// Текущее отображаемое значение.
        /// </summary>
        private string displayValue;

        /// <summary>
        /// Значение, отображаемое на всплывающей подсказке.
        /// </summary>
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

        /// <summary>
        /// Функция для преобразования текущего значения в строку.
        /// </summary>
        public Func<double, string> DisplayValueConverter { get; set; }

        /// <summary>
        /// Кисть, используемая для создания цветной метки на кнопке и цветной дуги.
        /// </summary>
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


        /// <summary>
        /// Инициализирует новый объект типа Knob.
        /// </summary>
        public Knob()
        {
            InitializeComponent();
            valuePopup.CustomPopupPlacementCallback += PopupPlacementCallback;
        }

        /// <summary>
        /// Метод, возвращающий позиции для всплывающей подсказки.
        /// </summary>
        /// <param name="popupSize"></param>
        /// <param name="targetSize"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Метод, привязывающий ручку к объекту VstParameterManager.
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="stroke"></param>
        /// <param name="valueConverter"></param>
        public void AttachTo(VstParameterManager manager, Brush stroke, Func<double, string> valueConverter = null)
        {
            manager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(manager.CurrentValue) || e.PropertyName == nameof(manager.ActiveParameter))
                    SetValue(manager.CurrentValue);
            };

            this.manager = manager;
            DisplayValueConverter = valueConverter;
            Stroke = stroke;
            SetValue(manager.CurrentValue);
        }

        /// <summary>
        /// Метод, обновляющий значение на всплывающей подсказке.
        /// </summary>
        public void UpdateDisplayValue()
        {
            var temp = DisplayValueConverter?.Invoke(value) ?? value.ToString("F3");
            DisplayValue = temp;
            Dispatcher.Invoke(() => Rotate(value));
        }

        /// <summary>
        /// Метод, обновляющий значение привязанного параметра.
        /// </summary>
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

        /// <summary>
        /// Устанавливает новое значение.
        /// </summary>
        /// <param name="newValue"></param>
        public void SetValue(double newValue)
        {
            if (newValue != value)
            {
                if (newValue < 0)
                    newValue = 0;
                else if (newValue > 1)
                    newValue = 1;
                value = newValue;
                UpdateParameterValue();
                UpdateDisplayValue();
            }
        }

        /// <summary>
        /// Поворачивает ручку в соответствии с переданным значением.
        /// </summary>
        /// <param name="normalizedValue"></param>
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

        /// <summary>
        /// Обработчик события нажатия на ручку.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                leftButtonPressed = true;
                accumulatedValue = value;
                startMousePosition = e.GetPosition(this);
                startScreenMousePosition = PointToScreen(startMousePosition);
                Mouse.OverrideCursor = Cursors.None;
                ((UIElement)sender).CaptureMouse().ToString();
            }
        }

        /// <summary>
        /// Обработчик события отпускания кнопки мыши.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                leftButtonPressed = false;
                Mouse.OverrideCursor = null;
                ((UIElement)sender).ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Обработчик события движения мыши.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftButtonPressed)
            {
                var delta = (startMousePosition.Y - e.GetPosition(this).Y) * Delta;
                accumulatedValue += delta;
                SetCursorPos((int)startScreenMousePosition.X, (int)startScreenMousePosition.Y);
                SetValue(accumulatedValue);
            }
        }


        /// <summary>
        /// Метод, передвигающий курсор мыши на указанные координаты.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Событие, показывающее, что значения некоторых свойств изменились.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Метод, вызывающий событие PropertyChanged с переданными параметрами.
        /// </summary>
        /// <param name="v"></param>
        private void OnPropertyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }
    }
}
