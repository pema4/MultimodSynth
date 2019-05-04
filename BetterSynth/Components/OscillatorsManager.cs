using Jacobi.Vst.Framework;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BetterSynth
{
    internal class OscillatorsManager : AudioComponentWithParameters
    {        
        private float pitchFine;
        private float pitchSemi;
        private float pitchMultiplier;
        private WaveTableOscillator waveTable = Utilities.WaveTables[0];
        private List<Oscillator> oscillators;
        private ParameterFilter pitchMultiplierFilter;

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
                defaultValue: 0.5f,
                valueChangedHandler: SetPitchSemi);

            PitchFineManager = factory.CreateParameterManager(
                name: "FINE",
                defaultValue: 0.5f,
                valueChangedHandler: SetPitchFine);
            pitchMultiplierFilter = new ParameterFilter(UpdatePitchMultiplier, 0);

            WaveTableManager = factory.CreateParameterManager(
                name: "TYPE",
                defaultValue: 0,
                valueChangedHandler: SetWaveTable);
        }

        private void SetPitchSemi(float value)
        {
            pitchSemi = (float)Math.Pow(2, (int)Converters.ToSemitones(value) / 12.0);
            var target = pitchSemi * pitchFine;
            pitchMultiplierFilter.SetTarget(target);
        }

        private void SetPitchFine(float value)
        {
            pitchFine = (float)Math.Pow(2, (int)Converters.ToCents(value) / 1200.0);
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
            var newWaveTable = Converters.ToWaveTable(value);

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
        
        public void Process()
        {
            pitchMultiplierFilter.Process();
        }
    }
}