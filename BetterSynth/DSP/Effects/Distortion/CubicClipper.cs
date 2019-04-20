namespace BetterSynth
{
    /// <summary>
    /// https://ccrma.stanford.edu/realsimple/faust_strings/Cubic_Nonlinear_Distortion.html
    /// </summary>
    class CubicClipper : IDistortion
    {
        public float Process(float input)
        {
            if (input < -1)
                return -1;
            else if (input > 1)
                return 1;
            else
                return 1.5f * input - 0.5f * input * input * input;
        }

        public void SetAmount(float value)
        {
        }
    }
}
