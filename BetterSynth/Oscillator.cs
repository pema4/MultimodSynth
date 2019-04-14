namespace BetterSynth
{
    class Oscillator : AudioComponent
    {
        private float noteFrequency;
        private float overallFrequency;
        private float phaseIncrement;
        private float pitchMultiplier;
        private float phasor;

        public Oscillator()
        {
            // PitchMultiplier = 1;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            phaseIncrement = overallFrequency / newSampleRate;
        }

        public WaveTable WaveTable { get; set; }

        public float NoteFrequency
        {
            get => noteFrequency;
            set
            {
                noteFrequency = value;
                UpdateCoefficients();
            }
        }

        public float PitchMultiplier
        {
            get => pitchMultiplier;
            set
            {
                pitchMultiplier = value;
                UpdateCoefficients();
            }
        }

        private void UpdateCoefficients()
        {
            overallFrequency = noteFrequency * pitchMultiplier;
            phaseIncrement = overallFrequency / SampleRate;
        }

        public float Process(float phaseModulation = 0)
        {
            float output = WaveTable.Process(phasor);

            phasor = phasor + phaseIncrement + phaseModulation;
            if (phasor >= 2)
                phasor -= 2;
            else if (phasor >= 1)
                phasor -= 1;
            else if (phasor < -1)
                phasor += 2;
            else if (phasor < 0)
                phasor += 1;

            return output;
        }

        public void ResetPhase() => phasor = 0;
    }
}
