using System;

namespace WavesData
{
    [Serializable]
    public class WaveTable
    {
        private WaveLookup[] waves;

        /// <summary>
        /// Initializes a new instance of the WaveTable class that is generated using given function of two variables.
        /// </summary>
        /// <param name="generatorFunction">Function used to generate waves and samples. First arg (x) is iterated in wave table, second (y) is iterated in individual lookup table.</param>
        /// <param name="tableStartPoint">Value from which sampling of wave table starts.</param>
        /// <param name="tableEndPoint">Value at which sampling of wave table end.</param>
        /// <param name="waveStartPoint">Value from which sampling of lookup table starts.</param>
        /// <param name="waveEndPoint">Value at which sampling of lookup table end.</param>
        /// <param name="wavesAmount">Amount of waves in wave table.</param>
        /// <param name="samplesAmount">Amount of samples in lookup table.</param>
        public WaveTable(
            Func<double, double, double> generatorFunction,
            double tableStartPoint,
            double tableEndPoint,
            double waveStartPoint = 0,
            double waveEndPoint = 2 * Math.PI,
            int wavesAmount = 128,
            int samplesAmount = 4096)
        {
            waves = new WaveLookup[wavesAmount];

            for (int i = 0; i < wavesAmount; ++i)
            {
                double x = tableStartPoint + (tableEndPoint - tableStartPoint) * i / wavesAmount;
                double function(double y) => generatorFunction(x, y);
                waves[i] = new WaveLookup(function, waveStartPoint, waveEndPoint, samplesAmount);
            }
        }
        
        public WaveTableLookup this[float point]
        {
            get
            {
                float unnormalizedLength = point * (waves.Length - 1);
                int leftIndex = (int)unnormalizedLength;
                int rightIndex = (leftIndex != waves.Length - 1) ? leftIndex + 1 : leftIndex;
                float rightCoeff = unnormalizedLength - leftIndex;
                float leftCoeff = 1 - rightCoeff;
                return new WaveTableLookup(waves[leftIndex], waves[rightIndex], leftCoeff, rightCoeff);
            }
        }
    }
}
