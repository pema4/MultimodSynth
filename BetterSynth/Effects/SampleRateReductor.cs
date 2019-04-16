namespace BetterSynth
{
    class SampleRateReductor : AudioComponent
    {
        private float holdTime;
        private float phasor;
        private float phaseIncrement;
        private float sample;

        public SampleRateReductor()
        {
        }

        public float HoldTime
        {
            get => holdTime;
            set
            {
                holdTime = value;
                UpdateCoefficients();
            }
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            UpdateCoefficients();
        }

        private void UpdateCoefficients()
        {
            phaseIncrement = HoldTime / SampleRate;
        }

        public float Process(float input)
        {
            var output = sample;

            phasor += phaseIncrement;
            if (phasor >= 1)
            {
                phasor -= 1;
                sample = input;
            }

            return output;
        }
    }
}
