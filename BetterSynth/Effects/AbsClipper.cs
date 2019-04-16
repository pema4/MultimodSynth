using System;

namespace BetterSynth
{
    /// <summary>
    /// http://www.earlevel.com/main/2017/05/26/guitar-amp-simulation/
    /// </summary>
    class AbsClipper
    {
        public float Process(float input)
        {
            if (input < -1)
                return -1;
            else if (input > 1)
                return 1;
            else
                return input * (2 - Math.Abs(input));
        }
    }
}
