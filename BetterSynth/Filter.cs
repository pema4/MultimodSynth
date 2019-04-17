using System;

namespace BetterSynth
{
    class Filter : AudioComponent
    {
        private const float BaseCutoff = 65.41f;
        
        private SvfFilter filter;
        private float noteFrequency;
        private float cutoffMultiplier;
        private float trackingCoeff;
        private float cutoff;

        public Filter()
        {
            filter = new SvfFilter(type: SvfFilterType.Low);
        }

        public void SetFilterType(SvfFilterType value)
        {
            filter.SetType(value);
        }

        public void SetCutoffMultiplier(float value)
        {
            cutoffMultiplier = value;
            CalculateCutoff();
        }

        public void SetNoteFrequency(float value)
        {
            noteFrequency = value;
            CalculateCutoff();
        }

        public void SetTrackingCoeff(float value)
        {
            trackingCoeff = value;
            CalculateCutoff();
        }

        private void CalculateCutoff()
        {
            var trackedCutoff = BaseCutoff + (noteFrequency - BaseCutoff) * trackingCoeff;
            cutoff = cutoffMultiplier * trackedCutoff;
        }

        public void SetCurve(float value)
        {
            float q;
            if (value >= 0.5f)
                q = (float)Math.Pow(16, 2 * value - 1);
            else
                q = (float)Math.Pow(4, 2 * value - 1);
                
            filter.SetQ(q);
        }

        public float Process(float input, float cutoffModulation = 0)
        {
            var modulatedCutoff = cutoff * (1 + (float)Math.Pow(2, 10 * cutoffModulation));
            if (modulatedCutoff > 20000)
                modulatedCutoff = 20000;
            filter.SetCutoff(modulatedCutoff);

            return filter.Process(input);
        }

        public void Reset() => filter.Reset();

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            filter.SampleRate = newSampleRate;
        }
    }
}