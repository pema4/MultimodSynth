namespace BetterSynth
{
    /// <summary>
    /// http://www.musicdsp.org/en/latest/Effects/42-soft-saturation.html
    /// </summary>
    class SoftClipper
    {
        private float treshold;
        private float denominator;
        private float normalizationCoeff;

        public float Treshold
        {
            get => treshold;
            set
            {
                treshold = value;
                var temp = 1 - treshold;
                denominator = temp * temp;
                normalizationCoeff = 1 / ((treshold + 1) / 2);
            }
        }

        public float Process(float input)
        {
            if (input < -1)
                return -1;
            else if (input < -treshold)
            {
                var temp = input + treshold;
                return normalizationCoeff * (-treshold + temp / (1 + temp * temp / denominator));
            }
            else if (input < treshold)
                return normalizationCoeff * input;
            else if (input < 1)
            {
                var temp = input - treshold;
                return normalizationCoeff * (treshold + temp / (1 + temp * temp / denominator));
            }
            else
                return 1;
        }
    }
}
