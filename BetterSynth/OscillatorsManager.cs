using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;

namespace BetterSynth
{
    internal class OscillatorsManager : ManagerOfManagers
    {
        const float MaximumTime = 10;
        
        private Plugin plugin;
        private string parameterPrefix;
        private List<Oscillator> oscillators;
        private float sampleRate;
        private float pitchSemi;
        private float pitchFine;
        private float pitchMultiplierTarget;
        private OnePoleLowpassFilter pitchMultiplierFilter = new OnePoleLowpassFilter();
        private float waveTablePositionTarget;
        private float waveTablePosition;
        private OnePoleLowpassFilter waveTablePositionFilter = new OnePoleLowpassFilter();
        private WaveTable waveTable;
        private int waveTableIndex;
        private float pitchMultiplier;
        private bool isPitchMultiplierChanging;
        private bool isWaveTablePositionChanging;

        public OscillatorsManager(Plugin plugin, string parameterPrefix)
        {
            this.plugin = plugin;
            this.parameterPrefix = parameterPrefix;
            oscillators = new List<Oscillator>();
            waveTable = Utilities.WaveTables[waveTableIndex].Clone();
            InitializeParameters();

            plugin.Opened += (sender, e) => sampleRate = plugin.AudioProcessor.SampleRate;
        }

        public Oscillator CreateNewOscillator()
        {
            var res = new Oscillator(plugin);

            res.PitchMultiplier = pitchMultiplierTarget;
            res.WaveTable = waveTable;

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

            PitchSemiManager = factory.CreateParameterManager(
                name: parameterPrefix + "_SEMI",
                label: "Oct",
                minValue: -36,
                maxValue: 36,
                defaultValue: 0,
                valueChangedHandler: SetPitchSemi);
            CreateRedirection(PitchSemiManager, nameof(PitchSemiManager));

            PitchFineManager = factory.CreateParameterManager(
                name: parameterPrefix + "_FINE",
                label: "semitones",
                minValue: -100,
                maxValue: 100,
                defaultValue: 0,
                valueChangedHandler: SetPitchFine);
            CreateRedirection(PitchFineManager, nameof(PitchFineManager));

            WaveTableManager = factory.CreateParameterManager(
                name: parameterPrefix + "_WT",
                valueChangedHandler: SetWaveTable);
            CreateRedirection(WaveTableManager, nameof(WaveTableManager));

            WaveTablePositionManager = factory.CreateParameterManager(
                name: parameterPrefix + "_WP",
                valueChangedHandler: SetWaveTablePosition);
            CreateRedirection(WaveTablePositionManager, nameof(WaveTablePositionManager));
        }

        public VstParameterManager PitchSemiManager { get; private set; }

        public VstParameterManager PitchFineManager { get; private set; }

        public VstParameterManager WaveTableManager { get; private set; }

        public VstParameterManager WaveTablePositionManager { get; private set; }

        private void SetPitchSemi(float value)
        {
            pitchSemi = (float)Math.Pow(2, value / 12);
            pitchMultiplierTarget = pitchSemi * pitchFine;
            isPitchMultiplierChanging = true;
        }

        private void SetPitchFine(float value)
        {
            pitchFine = (float)Math.Pow(2, value / 1200);
            pitchMultiplierTarget = pitchSemi * pitchFine;
            isPitchMultiplierChanging = true;
        }

        private void SetWaveTable(float value)
        {
            int index = (int)(value * Utilities.WaveTables.Length);
            if (index == Utilities.WaveTables.Length)
                index -= 1;

            if (index != waveTableIndex)
            {
                waveTableIndex = index;

                waveTable = Utilities.WaveTables[index].Clone();
                waveTable.Position = waveTablePositionTarget;

                foreach (var oscillator in oscillators)
                    oscillator.WaveTable = waveTable;
            }
        }

        private void SetWaveTablePosition(float value)
        {
            waveTablePositionTarget = value;
            isWaveTablePositionChanging = true;
        }

        private void UpdateWaveTablePosition()
        {
            var newValue = waveTablePositionFilter.Process(waveTablePositionTarget);
            if (newValue != waveTablePosition)
            {
                waveTablePosition = newValue;
                waveTable.Position = waveTablePosition;
            }
            else
                isWaveTablePositionChanging = false;
        }

        private void UpdatePitchMultiplier()
        {
            var newValue = pitchMultiplierFilter.Process(pitchMultiplierTarget);
            if (newValue != pitchMultiplier)
            {
                pitchMultiplier = newValue;
                foreach (var oscillator in oscillators)
                    oscillator.PitchMultiplier = pitchMultiplier;
            }
            else
                isPitchMultiplierChanging = false;
        }

        public void Process()
        {
            if (isPitchMultiplierChanging)
                UpdatePitchMultiplier();

            if (isWaveTablePositionChanging)
                UpdateWaveTablePosition();
        }
    }
}