using System;

namespace BetterSynth
{
    static class Utilities
    {
        public static double ConvertDbToAmp(float db)
        {
            return Math.Pow(10, db / 10);
        }

        public static double MidiNoteToFrequency(int note)
        {
            return 440 * Math.Pow(2, (note - 69) / 12.0);
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static WaveTable[] WaveTables =
        {
            new WaveTable((exp, x) => Math.Sign(Math.Sin(x)) * Math.Pow(Math.Abs(Math.Sin(x)), 1 / exp), 0.001, 1),
            new WaveTable(TruncatedSawGenerator(20, 20000), 1, 11, wavesAmount: 10),
        };

        private static Func<double, double> SawGenerator(double initFrequency, double maxFrequency)
        {
            return freq =>
            {
                double res = 0;
                for (int i = 1; i < maxFrequency / initFrequency; ++i)
                    res += Math.Sin(i * freq) / i;
                return res;
            };
        }
        
        private static Func<double, double, double> TruncatedSawGenerator(double initFrequency, double maxFrequency)
        {
            return (octaves, freq) =>
            {
                var sawGenerator = SawGenerator(initFrequency, Math.Min(maxFrequency, initFrequency * Math.Pow(2, octaves)));
                return sawGenerator(freq);
            };
        }
    }
}
