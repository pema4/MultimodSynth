using System;
using System.Collections.Generic;

namespace WavesData
{
    /// <summary>
    /// Represents a lookup table for selected function.
    /// </summary>
    public class SampledFunction
    {
        private float[] samples;
        private InterpolationMode InterpolationMode { get; set; }
        private InterpolationMethod InterpolationMethod { get; set; } 

        public SampledFunction(
            float[] samples,
            InterpolationMode mode = InterpolationMode.Cycled,
            InterpolationMethod method = InterpolationMethod.Linear)
        {
            InterpolationMode = mode;
            InterpolationMethod = method;
            this.samples = (float[])samples.Clone();
        }
        
        public SampledFunction(
            Func<double, double> generatorFunction,
            double samplingStartPoint = 0,
            double samplingEndPoint = 2 * Math.PI,
            int samplesAmount = 2048,
            InterpolationMode mode = InterpolationMode.Cycled,
            InterpolationMethod method = InterpolationMethod.Linear)
        {
            InterpolationMode = mode;
            InterpolationMethod = method;
            
            samples = new float[samplesAmount];
            for (int i = 0; i < samples.Length; ++i)
            {
                double arg = samplingStartPoint + (samplingEndPoint - samplingStartPoint) / samplesAmount * i;
                samples[i] = (float)generatorFunction(arg);
            }
        }
        
        public float InterpolateLinear(float point)
        {
            /*
            if (InterpolationMode == InterpolationMode.Normal)
                point = point / samples.Length * (samples.Length - 1);
            float distanceBetweenSamples = 1.0f / samples.Length;
            float offset = point % distanceBetweenSamples;
            int leftIdx = (int)((point - offset) * samples.Length);
            int rightIdx = (leftIdx + 1) % samples.Length;
            float rightCoeff = offset / distanceBetweenSamples;
            float leftCoeff = 1 - rightCoeff;
            return samples[leftIdx] * leftCoeff + samples[rightIdx] * rightCoeff;
            */

            float unnormalizedLength;
            if (InterpolationMode == InterpolationMode.Normal)
                unnormalizedLength = point * (samples.Length - 1);
            else
                unnormalizedLength = point * samples.Length;
            int leftIndex = (int)unnormalizedLength;
            int rightIndex = leftIndex == samples.Length - 1 ? 0 : leftIndex + 1;
            float rightCoeff = unnormalizedLength - leftIndex;
            float leftCoeff = 1 - rightCoeff;
            return samples[leftIndex] * leftCoeff + samples[rightIndex] * rightCoeff;
        }

        public float InterpolateSinc(float point)
        {
            throw new NotImplementedException();
        }

        public float this[float point]
        {
            get
            {
                point %= 1;
                switch (InterpolationMethod)
                {
                    case InterpolationMethod.Linear:
                        return InterpolateLinear(point);
                    case InterpolationMethod.Sinc:
                        return InterpolateSinc(point);
                    default:
                        return samples[(int)(point * samples.Length)];
                }
            }
        }

        public float this[int idx] => samples[idx];

        public int Count => samples.Length;
    }
}
