using System;

namespace BetterSynth
{
    class BitCrusher
    {
        private float bits;
        
        public float Bits
        {
            get => bits;
            set => bits = value;
        }

        public float Process(float input)
        {
            return (float)Math.Round(bits * input) / bits;
        }
    }
}
