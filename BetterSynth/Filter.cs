using System;

namespace BetterSynth
{
    class Filter
    {
        private const float BaseCutoff = 65.41f;
        
        private Plugin plugin;
        private SvfFilter filter;
        private float curve;
        private float noteFrequency;
        private float cutoffMultiplier;
        private float trackingCoeff;
        private float cutoff;

        public Filter(Plugin plugin)
        {
            this.plugin = plugin;

            filter = new SvfFilter(plugin)
            {
                Cutoff = 20000,
                Gain = 0,
                Q = 1,
                Type = SvfFilterType.Low,
            };
        }
        
        public float SampleRate
        {
            get => filter.SampleRate;
            set => filter.SampleRate = value;
        }

        public SvfFilterType FilterType
        {
            get => filter.Type;
            set => filter.Type = value;
        }

        public float CutoffMultiplier
        {
            get => cutoffMultiplier;
            set
            {
                if (cutoffMultiplier != value)
                {
                    cutoffMultiplier = value;
                    CalculateCutoff();
                }
            }
        }

        public float NoteFrequency
        {
            get => noteFrequency;
            set
            {
                if (noteFrequency != value)
                {
                    noteFrequency = value;
                    CalculateCutoff();
                }
            }
        }

        public float TrackingCoeff
        {
            get => trackingCoeff;
            set
            {
                if (trackingCoeff != value)
                {
                    trackingCoeff = value;
                    CalculateCutoff();
                }
            }
        }

        private void CalculateCutoff()
        {
            var trackedCutoff = BaseCutoff + (noteFrequency - BaseCutoff) * trackingCoeff;
            cutoff = cutoffMultiplier * trackedCutoff;
        }

        public float Curve
        {
            get => curve;
            set
            {
                curve = value;
                float q;
                if (value >= 0.5f)
                    q = (float)Math.Pow(16, 2 * value - 1);
                else
                    q = (float)Math.Pow(4, 2 * value - 1);
                
                filter.Q = q;
            }
        }

        public float Process(float input, float cutoffModulation = 0)
        {
            var modulatedCutoff = cutoff * (1 + cutoffModulation * 305);
            if (modulatedCutoff > 20000)
                modulatedCutoff = 20000;
            filter.Cutoff = modulatedCutoff;

            return filter.Process(input);
        }

        public void Reset() => filter.Reset();
    }
}