using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MultimodSynth.UI
{
    /// <summary>
    /// Представляет собой окно редактора плагина.
    /// </summary>
    public partial class EditorView : UserControl
    {
        /// <summary>
        /// Ссылка на плагин, связанный с этим редактором.
        /// </summary>
        private Plugin plugin;

        /// <summary>
        /// Метод, привязывающий параметры плагина к редактору.
        /// </summary>
        /// <param name="plugin"></param>
        internal void BindToPlugin(Plugin plugin)
        {
            this.plugin = plugin;
            InitializeComponent();
            BindParameters(plugin.AudioProcessor.Routing);
            BindKeyboard(plugin.MidiProcessor);
        }

        /// <summary>
        /// Метод, привязывающий параметры плагина к редактору.
        /// </summary>
        /// <param name="routing"></param>
        private void BindParameters(Routing routing)
        {
            BindOscA(routing.VoicesManager.OscAManager, routing.VoicesManager.OscAVolumeEnvelopeManager);
            BindOscB(routing.VoicesManager.OscBManager, routing.VoicesManager.OscBVolumeEnvelopeManager);
            BindFilter(routing.VoicesManager.FilterManager, routing.VoicesManager.FilterCutoffEnvelopeManager);
            BindMasterSettings(routing);
            BindDistortion(routing.DistortionManager);
            BindDelay(routing.DelayManager);
        }

        /// <summary>
        /// Метод, привязывающий параметры первого осциллятора плагина к редактору.
        /// </summary>
        /// <param name="oscA"></param>
        /// <param name="envA"></param>
        private void BindOscA(OscillatorsManager oscA, EnvelopesManager envA)
        {
            var color = (SolidColorBrush)Resources["oscAKnobColor"];

            // OscA
            APitchSemi.AttachTo(oscA.PitchSemiManager, color,
                Converters.SemitonesToString);

            APitchFine.AttachTo(oscA.PitchFineManager, color,
                Converters.CentsToString);

            ATimbre.AttachTo(oscA.WaveTableManager, color,
                Converters.WaveTableToString);

            // OscA envelope
            AAttack.AttachTo(envA.AttackTimeManager, color,
                Converters.EnvelopeTimeToString);

            ADecay.AttachTo(envA.DecayTimeManager, color,
                Converters.EnvelopeTimeToString);

            ASustain.AttachTo(envA.SustainLevelManager, color,
                Converters.PercentsToString);

            ARelease.AttachTo(envA.ReleaseTimeManager, color,
                Converters.EnvelopeTimeToString);

            AAmp.AttachTo(envA.EnvelopeAmplitudeManager, color,
                Converters.PercentsToString);

            AAttackCurve.AttachTo(envA.AttackCurveManager, color,
                Converters.EnvelopeCurveToString);

            ADecayReleaseCurve.AttachTo(envA.DecayReleaseCurveManager, color,
                Converters.EnvelopeCurveToString);
        }

        /// <summary>
        /// Метод, привязывающий параметры второго осциллятора плагина к редактору.
        /// </summary>
        /// <param name="oscA"></param>
        /// <param name="envA"></param>
        private void BindOscB(OscillatorsManager oscB, EnvelopesManager envB)
        {
            var color = (SolidColorBrush)Resources["oscBKnobColor"];

            // OscA
            BPitchSemi.AttachTo(oscB.PitchSemiManager, color,
                Converters.SemitonesToString);

            BPitchFine.AttachTo(oscB.PitchFineManager, color,
                Converters.CentsToString);

            BTimbre.AttachTo(oscB.WaveTableManager, color,
                Converters.WaveTableToString);

            // OscA envelope
            BAttack.AttachTo(envB.AttackTimeManager, color,
                Converters.EnvelopeTimeToString);

            BDecay.AttachTo(envB.DecayTimeManager, color,
                Converters.EnvelopeTimeToString);

            BSustain.AttachTo(envB.SustainLevelManager, color,
                Converters.PercentsToString);

            BRelease.AttachTo(envB.ReleaseTimeManager, color,
                Converters.EnvelopeTimeToString);

            BAmp.AttachTo(envB.EnvelopeAmplitudeManager, color,
                Converters.PercentsToString);

            BAttackCurve.AttachTo(envB.AttackCurveManager, color,
                Converters.EnvelopeCurveToString);

            BDecayReleaseCurve.AttachTo(envB.DecayReleaseCurveManager, color,
                Converters.EnvelopeCurveToString);
        }

        /// <summary>
        /// Метод, привязывающий параметры фильтра плагина к редактору.
        /// </summary>
        private void BindFilter(FiltersManager filter, EnvelopesManager env)
        {
            var color = (Brush)Resources["filterKnobColor"];

            // Filter
            FilterCutoff.AttachTo(filter.CutoffManager, color,
                Converters.FilterCutoffMultiplierToString);

            FilterResonanse.AttachTo(filter.CurveManager, color,
                Converters.PercentsToString);

            FilterType.AttachTo(filter.FilterTypeManager, color,
                Converters.FilterTypeToString);

            FilterKeyTracking.AttachTo(filter.TrackingCoeffManager, color,
                Converters.PercentsToString);

            // Filter envelope
            FilterAttack.AttachTo(env.AttackTimeManager, color,
                Converters.EnvelopeTimeToString);

            FilterDecay.AttachTo(env.DecayTimeManager, color,
                Converters.EnvelopeTimeToString);

            FilterSustain.AttachTo(env.SustainLevelManager, color,
                Converters.PercentsToString);

            FilterRelease.AttachTo(env.ReleaseTimeManager, color,
                Converters.EnvelopeTimeToString);

            FilterEnvelopeAmp.AttachTo(env.EnvelopeAmplitudeManager, color,
                Converters.PercentsToString);

            FilterAttackCurve.AttachTo(env.AttackCurveManager, color,
                Converters.EnvelopeCurveToString);

            FilterDecayReleaseCurve.AttachTo(env.DecayReleaseCurveManager, color,
                Converters.EnvelopeCurveToString);
        }

        /// <summary>
        /// Метод, привязывающий некие другие параметры плагина к редактору.
        /// </summary>
        private void BindMasterSettings(Routing routing)
        {
            var color = (Brush)Resources["masterKnobColor"];

            MasterVolume.AttachTo(routing.MasterVolumeManager, color,
                Converters.PercentsToString);

            Oversampling.AttachTo(routing.OversamplingOrderManager, color,
                Converters.OversamplingOrderToString);

            ModulationType.AttachTo(routing.VoicesManager.ModulationTypeManager, color,
                Converters.ModulationTypeToString);
        }

        /// <summary>
        /// Метод, привязывающий параметры эффекта дисторшн плагина к редактору.
        /// </summary>
        private void BindDistortion(DistortionManager distortion)
        {
            var color = (Brush)Resources["distortionKnobColor"];

            DistortionMode.AttachTo(distortion.ModeManager, color,
                Converters.DistortionModeToString);

            DistortionAmount.AttachTo(distortion.AmountManager, color,
                Converters.PercentsToString);

            DistortionAmp.AttachTo(distortion.AmpManager, color,
                Converters.DistortionAmpToString);

            DistortionAsymmetry.AttachTo(distortion.AsymmetryManager, color,
                Converters.AsymmetryToString);

            DistortionLowpass.AttachTo(distortion.LowPassCutoffManager, color,
                Converters.DistortionLowpassCutoffToString);

            DistortionMix.AttachTo(distortion.MixManager, color,
                Converters.PercentsToString);
        }

        /// <summary>
        /// Метод, привязывающий параметры эффекта дилэй плагина к редактору.
        /// </summary>
        private void BindDelay(DelayManager delay)
        {
            var color = (Brush)Resources["delayKnobColor"];

            DelayTime.AttachTo(delay.TimeManager, color,
                Converters.DelayTimeToString);

            DelayFeedback.AttachTo(delay.FeedbackManager, color,
                Converters.PercentsToString);

            DelayMix.AttachTo(delay.MixManager, color,
                Converters.PercentsToString);

            DelayStereoMode.AttachTo(delay.ModeManager, color,
                Converters.DelayModeToString);

            DelayStereoAmount.AttachTo(delay.StereoAmountManager, color,
                Converters.StereoAmountToString);

            DelayInvert.AttachTo(delay.InvertManager, color,
                Converters.InvertToString);

            DelayLfoDepth.AttachTo(delay.LfoDepthManager, color,
                Converters.PercentsToString);

            DelayLfoRate.AttachTo(delay.LfoRateManager, color,
                Converters.DelayLfoRateToString);
        }

        /// <summary>
        /// Метод, привязывающий плагин к клавиатуре редактора.
        /// </summary>
        /// <param name="midiProcessor"></param>
        private void BindKeyboard(MidiProcessor midiProcessor)
        {
            Dictionary<int, Rectangle> keys = new Dictionary<int, Rectangle>();
            for (int i = 36; i <= 36 + 64; ++i)
            {
                var key = new Rectangle();
                keys[i] = key;
                key.Visibility = Visibility.Visible;
                keysStackPanel.Children.Add(key);
                switch (i % 12)
                {
                    case 0:
                        key.Style = (Style)keysStackPanel.Resources["whiteKey"];
                        key.Fill = (Brush)Resources["keyOctaveStartColor"];
                        break;
                    case 2: case 4: case 7: case 9: case 11:
                        key.Style = (Style)keysStackPanel.Resources["whiteKeyAfterBlack"];
                        break;
                    case 5:
                        key.Style = (Style)keysStackPanel.Resources["whiteKey"];
                        break;
                    case 1: case 3: case 6: case 8: case 10:
                        key.Style = (Style)keysStackPanel.Resources["blackKey"];
                        break;
                }
                
                var keyNumber = i;

                key.MouseLeftButtonDown += (sender, e) =>
                {
                    var rect = (Rectangle)sender;
                    var velocity = e.GetPosition(rect).Y / rect.ActualHeight * 127;
                    plugin.MidiProcessor.PressNoteFromUI((byte)keyNumber, (byte)velocity);
                };

                key.MouseEnter += (sender, e) =>
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        var rect = (Rectangle)sender;
                        var velocity = e.GetPosition(rect).Y / rect.ActualHeight * 127;
                        plugin.MidiProcessor.PressNoteFromUI((byte)keyNumber, (byte)velocity);
                    }
                };

                key.MouseLeave += (sender, e) =>
                {
                    plugin.MidiProcessor.ReleaseNoteFromUI((byte)keyNumber, 0);
                };
                
                key.MouseLeftButtonUp += (sender, e) =>
                {
                    plugin.MidiProcessor.ReleaseNoteFromUI((byte)keyNumber, 0);
                };
            }

            midiProcessor.NoteOn += (sender, e) =>
            {
                if (keys.ContainsKey(e.Note.NoteNo))
                    Dispatcher.InvokeAsync(() => keys[e.Note.NoteNo].Opacity = 0.3);
            };

            midiProcessor.NoteOff += (sender, e) =>
            {
                if (keys.ContainsKey(e.Note.NoteNo))
                    Dispatcher.InvokeAsync(() => keys[e.Note.NoteNo].Opacity = 1);
            };
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку "Сохранить".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog()
            {
                Title = "Save preset file...",
                Filter = "synth preset file|*.spreset",
                DefaultExt = ".spreset",
            };
            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var fs = new FileStream(fileDialog.FileName, FileMode.OpenOrCreate))
                    {
                        var parameters = plugin.Programs.ActiveProgram.Parameters;
                        Utilities.WriteParameters(fs, parameters);
                    }
                }
                catch (Exception ex) when (
                    ex is IOException ||
                    ex is System.Security.SecurityException)
                {
                    MessageBox.Show(
                        $"Preset saving error:\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.None);
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку "Открыть".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog()
            {
                Title = "Open preset file...",
                Filter = "synth preset file|*.spreset",
                DefaultExt = ".spreset",
            };
            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var fs = new FileStream(fileDialog.FileName, FileMode.Open))
                    {
                        var parameters = plugin.Programs.ActiveProgram.Parameters;
                        Utilities.ReadParameters(fs, parameters);
                    }
                }
                catch (Exception ex) when (
                    ex is IOException ||
                    ex is System.Security.SecurityException ||
                    ex is ArgumentException)
                {
                    MessageBox.Show(
                        "Preset opening error.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.None);
                }
            }
        }
    }
}
