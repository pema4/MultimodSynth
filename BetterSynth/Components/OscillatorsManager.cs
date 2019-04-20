using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BetterSynth
{
    internal class OscillatorsManager : AudioComponentWithParameters
    {
        const float MaximumTime = 10;
        
        private List<Oscillator> oscillators;
        private float pitchFine;
        private float pitchSemi;
        private float pitchMultiplier;
        private ParameterFilter pitchMultiplierFilter;
        private WaveTableOscillator waveTable = Utilities.WaveTables[0];

        public VstParameterManager PitchSemiManager { get; private set; }

        public VstParameterManager PitchFineManager { get; private set; }

        public VstParameterManager WaveTableManager { get; private set; }

        public OscillatorsManager(
            Plugin plugin,
            string parameterPrefix,
            string parameterCategory = "oscillators") :
                base(plugin, parameterPrefix, parameterCategory)
        {
            oscillators = new List<Oscillator>();
            InitializeParameters();
        }

        protected override void InitializeParameters(ParameterFactory factory)
        {
            PitchSemiManager = factory.CreateParameterManager(
                name: "SEMI",
                label: "Oct",
                minValue: -36,
                maxValue: 36,
                defaultValue: 0,
                valueChangedHandler: SetPitchSemi);
            CreateRedirection(PitchSemiManager, nameof(PitchSemiManager));

            PitchFineManager = factory.CreateParameterManager(
                name: "FINE",
                label: "semitones",
                minValue: -100,
                maxValue: 100,
                defaultValue: 0,
                valueChangedHandler: SetPitchFine);
            CreateRedirection(PitchFineManager, nameof(PitchFineManager));
            pitchMultiplierFilter = new ParameterFilter(UpdatePitchMultiplier, 0);

            WaveTableManager = factory.CreateParameterManager(
                name: "TYPE",
                valueChangedHandler: SetWaveTable);
            CreateRedirection(WaveTableManager, nameof(WaveTableManager));
        }

        private void SetPitchSemi(float value)
        {
            pitchSemi = (float)Math.Pow(2, value / 12);
            var target = pitchSemi * pitchFine;
            pitchMultiplierFilter.SetTarget(target);
        }

        private void SetPitchFine(float value)
        {
            pitchFine = (float)Math.Pow(2, value / 1200);
            var target = pitchSemi * pitchFine;
            pitchMultiplierFilter.SetTarget(target);
        }

        private void UpdatePitchMultiplier(float value)
        {
            pitchMultiplier = value;
            foreach (var oscillator in oscillators)
                oscillator.SetPitchMultiplier(pitchMultiplier);
        }

        private void SetWaveTable(float value)
        {
            var waveTables = Utilities.WaveTables;

            int index = (int)(value * waveTables.Length);
            if (index == waveTables.Length)
                index -= 1;
            var newWaveTable = waveTables[index];

            if (waveTable != newWaveTable)
            {
                waveTable = newWaveTable;
                foreach (var oscillator in oscillators)
                    oscillator.SetWaveTable(waveTable.Clone());
            }
        }

        public Oscillator CreateNewOscillator()
        {
            var res = new Oscillator();
            res.SetPitchMultiplier(pitchMultiplier);
            res.SetWaveTable(waveTable.Clone());
            oscillators.Add(res);
            return res;
        }

        public void RemoveOscillator(Oscillator oscillator)
        {
            oscillators.Remove(oscillator);
        }

        public void Process()
        {
            pitchMultiplierFilter.Process();
        }
    }
}