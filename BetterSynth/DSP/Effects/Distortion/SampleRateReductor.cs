using System;

namespace BetterSynth
{
    class SampleRateReductor : AudioComponent, IDistortion
    {
        private float holdTime;
        private float phasor;
        private float phaseIncrement;
        private float sample;

        public void SetAmount(float value)
        {
            holdTime = (float)Math.Pow(44100, 1 - value);
            UpdateCoefficients();
        }

        private void UpdateCoefficients()
        {
            phaseIncrement = holdTime / SampleRate;
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

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            UpdateCoefficients();
        }
    }
}
