using System;
using System.Windows.Forms;

namespace WavesData
{
    [Serializable]
    public class WaveTable
    {
        private float[] samples;
        private int wavesAmount;
        private int waveSamplesAmount;
        private float position;
        private int leftOffset;
        private int rightOffset;
        private float leftWaveCoeff;
        private float rightWaveCoeff;

        /// <summary>
        /// Initializes a new instance of the WaveTable class that is generated using given function of two variables.
        /// </summary>
        /// <param name="generatorFunction">Function used to generate waves and samples. First arg (x) is iterated in wave table, second (y) is iterated in individual lookup table.</param>
        /// <param name="tableStartPoint">Value from which sampling of wave table starts.</param>
        /// <param name="tableEndPoint">Value at which sampling of wave table end.</param>
        /// <param name="waveStartPoint">Value from which sampling of lookup table starts.</param>
        /// <param name="waveEndPoint">Value at which sampling of lookup table end.</param>
        /// <param name="wavesAmount">Amount of waves in wave table.</param>
        /// <param name="waveSamplesAmount">Amount of samples in lookup table.</param>
        public WaveTable(
            Func<double, double, double> function,
            double tableStartPoint,
            double tableEndPoint,
            double waveStartPoint = 0,
            double waveEndPoint = 2 * Math.PI,
            int wavesAmount = 128,
            int waveSamplesAmount = 4096)
        {
            this.wavesAmount = wavesAmount;
            this.waveSamplesAmount = waveSamplesAmount;
            samples = new float[wavesAmount * waveSamplesAmount];

            for (int i = 0; i < wavesAmount; ++i)
            {
                double x = tableStartPoint + (tableEndPoint - tableStartPoint) * i / wavesAmount;
                
                for (int j = 0; j < waveSamplesAmount; ++j)
                {
                    double y = waveStartPoint + (waveEndPoint - waveStartPoint) * j / waveSamplesAmount;
                    double sample = function(x, y);
                    samples[i * waveSamplesAmount + j] = (float)sample;
                }
            }

            CalculateCoefficients();
        }

        public float Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    CalculateCoefficients();
                }
            }
        }

        private void CalculateCoefficients()
        {
            float unnormalizedPosition = position * (wavesAmount - 1);
            int integerPart = (int)unnormalizedPosition;

            leftOffset = integerPart * waveSamplesAmount;
            rightOffset = leftOffset + waveSamplesAmount;
            if (rightOffset == samples.Length)
                rightOffset = leftOffset;

            rightWaveCoeff = unnormalizedPosition - integerPart;
            leftWaveCoeff = 1 - rightWaveCoeff;
        }

        public float Process(float phaseOffset)
        {
            float unnormalized = phaseOffset * waveSamplesAmount;
            int leftIndex = (int)unnormalized;
            int rightIndex = leftIndex + 1;
            if (rightIndex == waveSamplesAmount)
                rightIndex = 0;
            float rightSampleCoeff = unnormalized - leftIndex;
            float leftSampleCoeff = 1 - rightSampleCoeff;

            float sum = 0;
            sum += (samples[leftOffset + leftIndex] * leftSampleCoeff +
                    samples[leftOffset + rightIndex] * rightSampleCoeff) * leftWaveCoeff;

            sum += (samples[rightOffset + leftIndex] * leftSampleCoeff +
                    samples[rightOffset + rightIndex] * rightSampleCoeff) * rightWaveCoeff;
            return sum;
        }
    }
}
