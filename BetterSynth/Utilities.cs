using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WavesData;

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
            new WaveTable((exp, x) => Math.Sign(Math.Sin(x)) * Math.Pow(Math.Abs(Math.Sin(x)), 1 / exp), 0.001, 1,
                mode: InterpolationMode.Normal),
        };
    }
}
