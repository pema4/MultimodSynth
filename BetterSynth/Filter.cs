using System;

namespace BetterSynth
{
    class Filter
    {
        private Plugin plugin;
        private SvfFilter filter;
        private float curve;
        private float noteFrequency;
        private float cutoff;
        private float trackingCoeff;
        private float overallCutoff;

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

        public float Cutoff
        {
            get => cutoff;
            set
            {
                if (cutoff != value)
                {
                    cutoff = value;
                    overallCutoff = Cutoff + NoteFrequency * TrackingCoeff;
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
                    overallCutoff = Cutoff + NoteFrequency * TrackingCoeff;
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
                    overallCutoff = Cutoff + NoteFrequency * TrackingCoeff;
                }
            }
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
            filter.Cutoff = Math.Min(overallCutoff * (1 + cutoffModulation * 10), 20000);
            return filter.Process(input);
        }

        public void Reset() => filter.Reset();
    }
}