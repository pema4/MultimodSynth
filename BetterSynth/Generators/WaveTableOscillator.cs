using System;

namespace BetterSynth
{
    [Serializable]
    public class WaveTableOscillator
    {
        private const double DefaultSampleRate = 44100;

        [Serializable]
        private class WaveTable
        {
            public int Length;
            public float[] Samples;
            public float PhaseIncrement;
        }

        private WaveTable[] waveTables;
        private WaveTable waveTable;
        private int waveTablesAmount;
        private float phaseIncrement;

        public delegate double GeneratorFunction(double phase, double freq, double maxFreq);

        public WaveTableOscillator(
            GeneratorFunction generator,
            double startFrequency,
            double endFrequency)
        {
            waveTablesAmount = (int)(Math.Floor(Math.Log(endFrequency / startFrequency, 2)) + 1);
            waveTables = new WaveTable[waveTablesAmount];
            for (int i = 0; i < waveTables.Length; ++i)
            {
                var frequency = startFrequency * (1 << i);
                var samples = PrepareSamples(generator, frequency, DefaultSampleRate / 2);
                waveTables[i] = new WaveTable
                {
                    Length = samples.Length,
                    PhaseIncrement = (float)(frequency / DefaultSampleRate),
                    Samples = samples,
                };
            }
        }

        private static float[] PrepareSamples(GeneratorFunction generator, double freq, double maxFreq)
        {
            var result = new float[(int)(2 * DefaultSampleRate / freq)];
            for (int i = 0; i < result.Length; ++i)
                result[i] = (float)generator((double)i / result.Length, freq, maxFreq);

            return result;
        }

        private WaveTableOscillator(WaveTable[] waveTables)
        {
            this.waveTables = waveTables;
            waveTablesAmount = waveTables.Length;
        }

        public WaveTableOscillator Clone()
        {
            return new WaveTableOscillator(waveTables);
        }
        
        public void SetPhaseIncrement(double phaseIncrement)
        {
            this.phaseIncrement = (float)phaseIncrement;

            int wtIndex = 0;
            while (wtIndex < waveTablesAmount - 1 && phaseIncrement > waveTables[wtIndex].PhaseIncrement)
                wtIndex += 1;

            waveTable = waveTables[wtIndex];
        }

        public float Process(float phase)
        {
            if (phase >= 1)
                phase -= 1;

            float temp = phase * waveTable.Length;
            int leftIndex = (int)temp;
            int rightIndex = leftIndex + 1;
            if (rightIndex == waveTable.Length)
                rightIndex = 0;
            float rightCoeff = temp - leftIndex;
            float leftCoeff = 1 - rightCoeff;
            return leftCoeff * waveTable.Samples[leftIndex] + rightCoeff * waveTable.Samples[rightIndex];
        }
    }
}
