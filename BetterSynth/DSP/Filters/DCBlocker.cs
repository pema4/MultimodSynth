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
        private const float BaseSampleRate = 44100f;
        private float xm1, ym1;
        private float r;
        private float normalizationCoeff;
        private float responseTimeCoefficient;

        public DCBlocker(float responseTimeCoefficient = 1)
        {
            this.responseTimeCoefficient = responseTimeCoefficient;
        }

        public float Process(float x)
        {
            var y = normalizationCoeff * (x - xm1) + r * ym1;
            xm1 = x;
            ym1 = y;
            return y;
        }
        
        public void SetResponseTimeCoefficient(float value)
        {
            responseTimeCoefficient = value;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            var coeff = BaseSampleRate / newSampleRate / responseTimeCoefficient;
            r = (float)Math.Exp(-2 * Math.PI * DefaultExp * coeff);
            normalizationCoeff = (1 + r) / 2;
        }
    }
}
