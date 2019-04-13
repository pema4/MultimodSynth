using System;

namespace BetterSynth
{
    /// <summary>
    /// http://www.musicdsp.org/en/latest/Filters/257-1-pole-lpf-for-smooth-parameter-changes.html
    /// </summary>
    class OnePoleLowpassFilter
    {
        private float a, b, z;

        public OnePoleLowpassFilter()
        {
            a = 0.99f;
            b = 1f - a;
            z = 0;
        }

        public float Process(float input)
        {
            z = (input * b) + (z * a);
            return z;
        }
    }
}
