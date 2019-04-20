using System;

namespace BetterSynth
{
    class BitCrusher : IDistortion
    {
        private float bits;
        
        public void SetAmount(float value)
        {
            bits = (float)Math.Pow(1 << 16, 1 - value);
        }

        public float Process(float input)
        {
            return (float)Math.Round(bits * input) / bits;
        }
    }
}
