using MultimodSynth.UI;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MultimodSynth
{
    /// <summary>
    /// Реализация интерфейса IVstPluginEditor, отвечающего за создание пользовательского интерфейса.
    /// </summary>
    class PluginEditor : IVstPluginEditor
    {
        /// <summary>
        /// Ссылка на плагин, которому принадлежит этот компонент.
        /// </summary>
        private Plugin plugin;

        /// <summary>
        /// Объект типа HwndSource.
        /// </summary>
        private HwndSource hwndSource;

        /// <summary>
        /// Текущие границы окна редактора.
        /// </summary>
        private Rectangle? bounds;

        /// <summary>
        /// Ссылка на окно редактора.
        /// </summary>
        private EditorView instance;

        /// <summary>
        /// Инициализирует новый объект типа PluginEditor, принадлежащий переданному плагину.
        /// </summary>
        /// <param name="plugin"></param>
        public PluginEditor(Plugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Тип переключателей редактора.
        /// </summary>
        public VstKnobMode KnobMode { get; set; }

        /// <summary>
        /// Обрабатывает нажатия клавиш (не используется).
        /// </summary>
        /// <param name="ascii"></param>
        /// <param name="virtualKey"></param>
        /// <param name="modifers"></param>
        /// <returns></returns>
        public bool KeyDown(byte ascii, VstVirtualKey virtualKey, VstModifierKeys modifers)
        {
            return false;
        }

        /// <summary>
        /// Обрабатывает отпускания клавиш клавиатуры (не используется).
        /// </summary>
        /// <param name="ascii"></param>
        /// <param name="virtualKey"></param>
        /// <param name="modifers"></param>
        /// <returns></returns>
        public bool KeyUp(byte ascii, VstVirtualKey virtualKey, VstModifierKeys modifers)
        {
            return false;
        }

        /// <summary>
        /// Метод, вызываемый хостом, когда процессор бездействует (не используется).
        /// </summary>
        public void ProcessIdle()
        {
        }

        /// <summary>
        /// Возвращает размеры текущего окна.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                if (bounds != null)
                    return bounds.Value;

                if (instance == null)
                {
                    instance = new EditorView();
                    instance.BindToPlugin(plugin);
                }
                var size = GetElementPixelSize(instance);
                bounds = new Rectangle(0, 0, (int)size.Width, (int)size.Height);
                return bounds.Value;
            }
        }

        /// <summary>
        /// Открывает и прикрепляет элемент управления к переданному hWnd.
        /// </summary>
        public void Open(IntPtr hWnd)
        {
            if (instance == null)
            {
                instance = new EditorView();
                instance.BindToPlugin(plugin);
            }

            HwndSourceParameters hwndParams = new HwndSourceParameters("Better Synth");
            hwndParams.ParentWindow = hWnd;
            hwndParams.Height = Bounds.Height;
            hwndParams.Width = Bounds.Width;
            hwndParams.WindowStyle = 0x10000000 | 0x40000000; // WS_VISIBLE|WS_CHILD

            hwndSource = new HwndSource(hwndParams);
            hwndSource.RootVisual = instance;
        }

        /// <summary>
        /// Возвращает размер элемента в экранных пикселях.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <seealso cref="https://stackoverflow.com/questions/3286175/how-do-i-convert-a-wpf-size-to-physical-pixels"/>
        private System.Windows.Size GetElementPixelSize(UIElement element)
        {
            Matrix transformToDevice;
            var source = PresentationSource.FromVisual(element);
            if (source != null)
                transformToDevice = source.CompositionTarget.TransformToDevice;
            else
                using (var sauce = new HwndSource(new HwndSourceParameters()))
                    transformToDevice = sauce.CompositionTarget.TransformToDevice;

            if (element.DesiredSize == new System.Windows.Size())
                element.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));

            return (System.Windows.Size)transformToDevice.Transform((Vector)element.DesiredSize);
        }

        /// <summary>
        /// Закрывает редактор.
        /// </summary>
        public void Close()
        {
            if (hwndSource != null)
            {
                hwndSource.Dispose();
                hwndSource = null;
            }

            instance = null;
        }
    }
}