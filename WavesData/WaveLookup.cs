using System;

namespace WavesData
{
    /// <summary>
    /// Represents a lookup table for selected function.
    /// </summary>
    public class WaveLookup
    {
        private float[] samples;
        
        public WaveLookup(
            Func<double, double> generatorFunction,
            double startPoint = 0,
            double endPoint = 2 * Math.PI,
            int samplesAmount = 4096)
        {
            samples = new float[samplesAmount];
            for (int i = 0; i < samples.Length; ++i)
            {
                double arg = startPoint + (endPoint - startPoint) / samplesAmount * i;
                samples[i] = (float)generatorFunction(arg);
            }
        }

        public float this[float point]
        {
            get
            {
                float unnormalizedLength = point * samples.Length;
                int leftIndex = (int)unnormalizedLength;
                int rightIndex = leftIndex == samples.Length - 1 ? 0 : leftIndex + 1;
                float rightCoeff = unnormalizedLength - leftIndex;
                float leftCoeff = 1 - rightCoeff;
                return samples[leftIndex] * leftCoeff + samples[rightIndex] * rightCoeff;
            }
        }
    }
}
