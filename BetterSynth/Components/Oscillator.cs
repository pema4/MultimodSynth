using System;

namespace BetterSynth
{
    class Oscillator : AudioComponent
    {
        private float noteFrequency;
        private float overallFrequency;
        private float phaseIncrement;
        private float pitchMultiplier;
        private float phasor;
        private WaveTableOscillator waveTable;

        public Oscillator()
        {
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            phaseIncrement = overallFrequency / newSampleRate;
        }

        public void SetWaveTable(WaveTableOscillator waveTable)
        {
            this.waveTable = waveTable;
            this.waveTable.SetPhaseIncrement(phaseIncrement);
        }

        public void SetNoteFrequency(float value)
        {
            noteFrequency = value;
            UpdateCoefficients();
        }

        public void SetPitchMultiplier(float value)
        {
            pitchMultiplier = value;
            UpdateCoefficients();
        }

        private void UpdateCoefficients()
        {
            overallFrequency = noteFrequency * pitchMultiplier;
            phaseIncrement = overallFrequency / SampleRate;
            waveTable?.SetPhaseIncrement(phaseIncrement);
        }

        public float Process(float phaseModulation = 0)
        {
            var phase = phasor + phaseModulation;
            phase -= (float)Math.Floor(phase);
            var waveTable = this.waveTable;
            float result;
            if (waveTable == null)
                result = 0;
            else
                result = waveTable.Process(phase);

            phasor += phaseIncrement;
            if (phasor >= 1)
                phasor -= 1;

            return result;
        }

        public void ResetPhase() => phasor = 0;
    }
}
