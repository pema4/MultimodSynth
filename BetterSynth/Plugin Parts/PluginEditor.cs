using BetterSynth.UI;
using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using System;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace BetterSynth
{
    internal class PluginEditor : IVstPluginEditor
    {
        private Plugin plugin;
        private HwndSource hwndSource;
        private Rectangle? bounds;
        private EditorView instance;

        public PluginEditor(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public VstKnobMode KnobMode { get; set; }

        public bool KeyDown(byte ascii, VstVirtualKey virtualKey, VstModifierKeys modifers)
        {
            return false;
        }

        public bool KeyUp(byte ascii, VstVirtualKey virtualKey, VstModifierKeys modifers)
        {
            return false;
        }

        public void ProcessIdle()
        {
        }

        /// <summary>
        /// Returns the bounding rectangle of the Control.
        /// </summary>
        /// <remarks>The same size as in design-time.</remarks>
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
        /// Opens and attaches the Control to the <paramref name="hWnd"/>.
        /// </summary>
        /// <param name="hWnd">The native win32 handle to the main window of the host.</param>
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
        /// https://stackoverflow.com/questions/3286175/how-do-i-convert-a-wpf-size-to-physical-pixels
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
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
        /// Closes and destroys the Control.
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