using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WavesData;

namespace BetterSynth
{
    /// <summary>
    /// Provides static methods to compute approximate values of some mathematical functions.
    /// </summary>
    public static class SampledMath
    {
        private static SampledFunction sqrt = new SampledFunction(x => Math.Sqrt(x), 0, 1, mode: InterpolationMode.Normal);

        /// <summary>
        /// Returns the square root of a specified number from interval of [0; 1].
        /// </summary>
        /// <param name="value">A number from interval of [0; 1]</param>
        /// <returns>The square root of the argument.</returns>
        public static float Sqrt(float value) => sqrt[value];

        private static WaveTable exponentialEnvelopeTable = new WaveTable((x, y) =>
            x != 0 ? (Math.Pow(Math.Exp(x), y) - 1) / (Math.Exp(x) - 1) : y,
            tableStartPoint: -10, tableEndPoint: 10, wavesAmount: 128,
            waveStartPoint: 0, waveEndPoint: 1, samplesAmount: 256,
            mode: InterpolationMode.Normal);

        public static float ExponentialEnvelope(float value, float curve) => exponentialEnvelopeTable[curve][value];
    }
}
