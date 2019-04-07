namespace BetterSynth
{
    class OnePoleLowpassFilter
    {
        private float a, b, z;

        OnePoleLowpassFilter()
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
