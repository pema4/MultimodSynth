using System;

namespace BetterSynth
{
    class SineLFO : AudioComponent
    {
        private float frequency;
        private float initialPhase;
        private float coeff;
        private float sin;
        private float cos;

        public SineLFO()
        {
            ResetPhase();
        }

        public void SetFrequency(float value)
        {
            frequency = value;
            UpdateCoefficients();
        }

        public void SetInitialPhase(float phase)
        {
            initialPhase = phase;
            sin = (float)(Math.Sin(phase) * cos + Math.Cos(phase) * sin);
            cos = (float)(Math.Cos(phase) * cos - Math.Sin(phase) * sin);
        }

        public void ResetPhase()
        {
            sin = (float)Math.Sin(initialPhase);
            cos = (float)Math.Cos(initialPhase);
        }

        private void UpdateCoefficients()
        {
            coeff = (float)(2 * Math.PI * frequency / SampleRate);
        }

        public float Process()
        {
            sin = sin + cos * coeff;
            cos = cos - sin * coeff;
            return sin;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            UpdateCoefficients();
        }
    }
}
