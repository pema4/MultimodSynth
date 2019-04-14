using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    /// <summary>
    /// https://ccrma.stanford.edu/~jos/filters/DC_Blocker_Software_Implementations.html
    /// </summary>
    class DCBlocker : AudioComponent
    {
        private double DefaultExp = 0.00079777080867193622;
        private const float BaseSampleRate = 441000f;
        private float xm1, ym1;
        private float r;
        private float normalizationCoeff;

        public float Process(float x)
        {
            var y = normalizationCoeff * (x - xm1) + r * ym1;
            xm1 = x;
            ym1 = y;
            return y;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            var ratio = newSampleRate / BaseSampleRate;
            r = (float)Math.Exp(-2 * Math.PI * DefaultExp * ratio);
            normalizationCoeff = (1 + r) / 2;
            xm1 = 0;
            ym1 = 0;
        }
    }
}
