using System;

namespace WavesData
{
    [Serializable]
    public class WaveTable
    {
        public SampledFunction[] Waves { get; set; }
        private double tableStartPoint;
        private double tableEndPoint;
        private double waveStartPoint;
        private double waveEndPoint;
        private int samplesAmount;

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
            int samplesAmount = 1 << 12,
            InterpolationMode mode = InterpolationMode.Cycled)
        {
            this.tableStartPoint = tableStartPoint;
            this.tableEndPoint = tableEndPoint;
            this.waveStartPoint = waveStartPoint;
            this.waveEndPoint = waveEndPoint;
            this.samplesAmount = samplesAmount;
            InterpolationMode = mode;

            Waves = new SampledFunction[wavesAmount];
            double xDelta;
            //if (mode == InterpolationMode.Cycled)
                xDelta = (tableEndPoint - tableStartPoint) / wavesAmount;
            //else
              //  xDelta = (tableEndPoint - tableStartPoint) / (wavesAmount - 1);
            double x = tableStartPoint;
            for (int i = 0; i < wavesAmount; ++i)
            {
                double xCopy = x;
                double function(double y) => generatorFunction(xCopy, y);
                Waves[i] = new SampledFunction(function, waveStartPoint, waveEndPoint, samplesAmount, mode: mode);
                x += xDelta;
            }
        }
        
        public SampledFunction this[int idx] => Waves[idx];

        public int Count => Waves.Length;

        public InterpolationMode InterpolationMode { get; set; }
        
        public WaveTableLookup this[float point]
        {
            get
            {
                float unnormalizedLength;
                if (InterpolationMode == InterpolationMode.Normal)
                    unnormalizedLength = point * (Waves.Length - 1);
                else
                    unnormalizedLength = point * Waves.Length;
                int leftIndex = (int)unnormalizedLength;
                int rightIndex = leftIndex == Waves.Length - 1 ? 0 : leftIndex + 1;
                float rightCoeff = unnormalizedLength - leftIndex;
                float leftCoeff = 1 - rightCoeff;
                return new WaveTableLookup(this, leftIndex, rightIndex, leftCoeff, rightCoeff);
            }
        }
    }
}
