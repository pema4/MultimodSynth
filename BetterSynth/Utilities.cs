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
            new WaveTable(TruncatedSawGenerator(20, 15000), 1, 11, wavesAmount: 10),
            new WaveTable(TruncatedSquareGenerator(20, 15000), 1, 11, wavesAmount: 10),
            new WaveTable(PwmGenerator(20, 15000), 2 * Math.PI / 11, 2 * Math.PI, wavesAmount: 10),
        };
        
        private static Func<double, double, double> TruncatedSawGenerator(double initFreq, double maxFreq)
        {
            Func<double, double> SawGenerator(int harmonicsCount)
            {
                return freq =>
                {
                    double res = 0;
                    for (int i = 1; i < harmonicsCount; ++i)
                        res += Math.Sin(i * freq) / i;
                    return res;
                };
            }

            return (octaves, x) =>
            {
                int harmonicsCount = (int)(Math.Min(maxFreq, initFreq * Math.Pow(2, octaves)) / initFreq);
                var sawGenerator = SawGenerator(harmonicsCount);
                return sawGenerator(x);
            };
        }

        private static Func<double, double, double> TruncatedSquareGenerator(double initFreq, double maxFreq)
        {
            Func<double, double> SquareGenerator(int harmonicsCount)
            {
                return freq =>
                {
                    double res = 0;
                    for (int i = 1; i < harmonicsCount; i += 2)
                        res += Math.Sin(i * freq) / i;
                    return res;
                };
            }

            return (octaves, x) =>
            {
                int harmonicsCount = (int)(Math.Min(maxFreq, initFreq * Math.Pow(2, octaves)) / initFreq);
                var sawGenerator = SquareGenerator(harmonicsCount);
                return sawGenerator(x);
            };
        }

        private static Func<double, double, double> PwmGenerator(double initFreq, double maxFreq)
        {
            return (pwm, x) =>
            {
                double res = 0;
                for (int i = 1; i < (int)(maxFreq / initFreq); ++i)
                    res += (Math.Sin(i * x) - Math.Sin(i *(x + pwm))) / i;
                return res / 2;
            };
        }
    }
}
