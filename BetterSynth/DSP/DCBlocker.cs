using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterSynth
{
    /// <summary>
    /// https://ccrma.stanford.edu/~jos/filters/DC_Blocker_Software_Implementations.html
    /// </summary>
    class DCBlocker
    {
        private float xm1, ym1;

        public float Process(float x)
        {
            var y = x - xm1 + 0.995f * ym1;
            xm1 = x;
            ym1 = y;
            return y;
        }
    }
}
