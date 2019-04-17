using System;

namespace BetterSynth
{
    public enum SvfFilterType
    {
        None,
        Low,
        Band,
        High,
        Notch,
        Peak,
        All,
        Bell,
        LowShelf,
        HighShelf,
    };

    class SvfFilter : AudioComponent
    {
        private SvfFilterType type;
        private float cutoff;
        private float q = 1;
        private float ic1eq = 0;
        private float ic2eq = 0;
        private float g;
        private float k;
        private float a1;
        private float a2;
        private float a3;
        private float m0;
        private float m1;
        private float m2;
        private float A;
        private float gain;

        public SvfFilter(
            SvfFilterType type = SvfFilterType.None,
            float cutoff = 20000,
            float q = 1,
            float gain = 0)
        {
            SetType(type);
            SetCutoff(cutoff);
            SetQ(q);
            SetGain(gain);
        }

        public void SetType(SvfFilterType value)
        {
            type = value;
            UpdateCoefficients();
            Reset();
        }

        public void SetCutoff(float value)
        {
            if (cutoff != value)
            {
                cutoff = value;
                UpdateCoefficients();
            }
        }

        public void SetQ(float value)
        {
            q = value;
            UpdateCoefficients();
        }

        public void SetGain(float value)
        {
            gain = value;
            UpdateCoefficients();
        }

        private void UpdateCoefficients()
        {
            switch (type)
            {
                case SvfFilterType.Low:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 0;
                    m1 = 0;
                    m2 = 1;
                    break;

                case SvfFilterType.Band:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 0;
                    m1 = 1;
                    m2 = 0;
                    break;

                case SvfFilterType.High:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = -k;
                    m2 = -1;
                    break;

                case SvfFilterType.Notch:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = -k;
                    m2 = 0;
                    break;

                case SvfFilterType.Peak:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = -k;
                    m2 = -2;
                    break;

                case SvfFilterType.All:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = -2 * k;
                    m2 = 0;
                    break;

                case SvfFilterType.Bell:
                    A = (float)Math.Pow(10, gain / 40);
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / (q * A);
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = k * (A * A - 1);
                    m2 = 0;
                    break;

                case SvfFilterType.LowShelf:
                    A = (float)Math.Pow(10, gain / 40);
                    g = (float)(Math.Tan(Math.PI * cutoff / SampleRate) / Math.Sqrt(A));
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = k * (A - 1);
                    m2 = A * A - 1;
                    break;

                case SvfFilterType.HighShelf:
                    A = (float)Math.Pow(10, gain / 40);
                    g = (float)(Math.Tan(Math.PI * cutoff / SampleRate) * Math.Sqrt(A));
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = A * A;
                    m1 = k * (1 - A) * A;
                    m2 = 1 - A * A;
                    break;
            }
        }

        public float Process(float v0)
        {
            float v3 = v0 - ic2eq;
            float v1 = a1 * ic1eq + a2 * v3;
            float v2 = ic2eq + a2 * ic1eq + a3 * v3;
            ic1eq = 2 * v1 - ic1eq;
            ic2eq = 2 * v2 - ic2eq;

            return m0 * v0 + m1 * v1 + m2 * v2;
        }

        public void Reset()
        {
            ic1eq = 0;
            ic2eq = 0;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            UpdateCoefficients();
        }
    }
}
