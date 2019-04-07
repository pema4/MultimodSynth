using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using WavesData;

namespace BetterSynth
{
    internal class OscillatorsManager : INotifyPropertyChanged
    {
        const float MaximumTime = 10;
        
        private Plugin plugin;
        private string parameterPrefix;
        private List<Oscillator> oscillators;
        private float sampleRate;
        private float pitchMultiplier;
        private float waveTablePosition;
        private WaveTable waveTable = Utilities.WaveTables[0];
        private WaveTableLookup currentWave = Utilities.WaveTables[0][0f];

        public OscillatorsManager(Plugin plugin, string parameterPrefix)
        {
            this.plugin = plugin;
            this.parameterPrefix = parameterPrefix;
            oscillators = new List<Oscillator>();
            InitializeParameters();

            plugin.Opened += (sender, e) => sampleRate = plugin.AudioProcessor.SampleRate;
        }

        public Oscillator CreateNewOscillator()
        {
            var res = new Oscillator(plugin);

            res.CurrentWave = currentWave;
            res.PitchMultiplier = pitchMultiplier;

            oscillators.Add(res);
            return res;
        }

        public void RemoveOscillator(Oscillator oscillator)
        {
            if (oscillators.Contains(oscillator))
                oscillators.Remove(oscillator);
        }

        private void InitializeParameters()
        {
            var factory = new ParameterFactory(plugin, "oscs");

            PitchManager = factory.CreateParameterManager(
                name: parameterPrefix + "_PCH",
                defaultValue: 0.5f,
                valueChangedHandler: SetPitch);
            CreateRedirection(PitchManager, nameof(PitchManager));

            WaveTableManager = factory.CreateParameterManager(
                name: parameterPrefix + "_WT",
                valueChangedHandler: SetWaveTable);
            CreateRedirection(WaveTableManager, nameof(WaveTableManager));

            WaveTablePositionManager = factory.CreateParameterManager(
                name: parameterPrefix + "_WP",
                valueChangedHandler: SetWaveTablePosition);
            CreateRedirection(WaveTablePositionManager, nameof(WaveTablePositionManager));
        }

        public VstParameterManager PitchManager { get; private set; }

        public VstParameterManager WaveTableManager { get; private set; }

        public VstParameterManager WaveTablePositionManager { get; private set; }

        private void SetPitch(float value)
        {
            pitchMultiplier = (float)Math.Pow(2, 4 * value - 2);

            foreach (var oscillator in oscillators)
                oscillator.PitchMultiplier = pitchMultiplier;
        }

        private void SetWaveTable(float value)
        {
            int idx = (int)(value * (Utilities.WaveTables.Length - 1));
            waveTable = Utilities.WaveTables[idx];

            SetWaveTablePosition(waveTablePosition);
        }

        private void SetWaveTablePosition(float value)
        {
            waveTablePosition = value;
            currentWave = waveTable[value];
            
            foreach (var oscillator in oscillators)
                oscillator.CurrentWave = currentWave;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CreateRedirection(VstParameterManager manager, string managerName)
        {
            manager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "CurrentValue")
                    OnPropertyChanged(managerName);
            };
        }
    }
}