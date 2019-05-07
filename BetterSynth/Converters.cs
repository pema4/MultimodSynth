using System;
using System.Windows;

namespace MultimodSynth
{
    /// <summary>
    /// Contains various method for convertation values from
    /// normalized forms to unnormalized forms and strings.
    /// </summary>
    static class Converters
    {
        public static int ToSemitones(double value) =>
            (int)(-36 + value * 72);

        public static string SemitonesToString(double value) =>
            $"{ToSemitones(value):+0;-0} semitones";

        public static int ToCents(double value) =>
            (int)(-100 + value * 200);

        public static string CentsToString(double value) =>
            $"{ToCents(value):+0;-0} cents";

        public static WaveTableOscillator ToWaveTable(double value)
        {
            var waveTables = Utilities.WaveTables;
            int index = (int)(value * (waveTables.Length - 1));
            return waveTables[index];
        }

        public static string WaveTableToString(double value)
        {
            if (value < 0.2)
                return "sine";
            else if (value < 0.4)
                return "triangle";
            else if (value < 0.6)
                return "saw";
            else if (value < 0.8)
                return "square (no pwm)";
            else if (value < 1)
                return "square (50% pwm)";
            else
                return "square (75% pwm)";
        }

        public static double ToSeconds(double value) =>
            10 * value;

        public static string SecondsToString(double value) =>
            $"{ToSeconds(value):F2} s";

        public static DelayManager.StereoMode ToDelayMode(double value)
        {
            if (value < 1f / 3)
                return DelayManager.StereoMode.None;
            else if (value < 2f / 3)
                return DelayManager.StereoMode.StereoOffset;
            else if (value < 1)
                return DelayManager.StereoMode.VariousTime;
            else
                return DelayManager.StereoMode.PingPong;
        }

        public static string DelayModeToString(double value)
        {
            if (value < 1f / 3)
                return "no delay";
            else if (value < 2f / 3)
                return "stereo offset";
            else if (value < 1)
                return "var time";
            else
                return "ping pong";
        }

        public static double ToDelayTime(double value) =>
            Math.Exp(11.512925464970229 * (value - 1));

        public static string DelayTimeToString(double value) =>
            $"{ToDelayTime(value) * 1000:F2} ms";

        public static double ToStereoAmount(double value) =>
            -1 + 2 * value;

        public static string StereoAmountToString(double value)
        {
            value = ToStereoAmount(value);
            if (value < 0)
                return $"{100 * value:F1}% left";
            else if (value == 0)
                return $"centered";
            else
                return $"{100 * -value:F1}% right";
        }

        public static string PercentsToString(double value) =>
            $"{100 * value:F1}%";

        public static string InvertToString(double value)
        {
            if (value < 0.5)
                return "normal";
            else
                return "inverted";
        }

        public static double ToDelayLfoRate(double value) =>
            5 * value;

        public static string DelayLfoRateToString(double value) =>
            $"{ToDelayLfoRate(value):F2} Hz";

        public static DistortionManager.DistortionMode ToDistortionMode(double value)
        {
            if (value < 1f / 5)
                return DistortionManager.DistortionMode.None;
            else if (value < 2f / 5)
                return DistortionManager.DistortionMode.AbsClipping;
            else if (value < 3f / 5)
                return DistortionManager.DistortionMode.SoftClipping;
            else if (value < 4f / 5)
                return DistortionManager.DistortionMode.CubicClipping;
            else if (value < 1)
                return DistortionManager.DistortionMode.BitCrush;
            else
                return DistortionManager.DistortionMode.SampleRateReduction;
        }

        public static string DistortionModeToString(double value)
        {
            if (value < 1f / 5)
                return "no distortion";
            else if (value < 2f / 5)
                return "abs saturation";
            else if (value < 3f / 5)
                return "mod saturation";
            else if (value < 4f / 5)
                return "cubic saturation";
            else if (value < 1)
                return "bitcrush";
            else
                return "rate reduction";
        }

        public static double ToAsymmetry(double value) =>
            -1 + 2 * value;

        public static string AsymmetryToString(double value)
        {
            var asym = ToAsymmetry(value);
            if (asym == 0)
                return "centered";
            else
                return asym.ToString("+0.00;-0.00");
        }

        public static double ToDistortionAmp(double value) =>
            4 * value;

        public static string DistortionAmpToString(double value) =>
            $"{ToDistortionAmp(value):F2}x";

        public static double ToDistortionLowpassCutoff(double value) =>
            Math.Exp(2.9957322735539909 + 6.9077552789821368 * value);

        public static string DistortionLowpassCutoffToString(double value) =>
            $"{ToDistortionLowpassCutoff(value):F1} Hz";


        public static double ToEnvelopeTime(double value) =>
            value * 10;

        public static string EnvelopeTimeToString(double value) =>
            $"{ToEnvelopeTime(value):F2} s";

        public static string EnvelopeCurveToString(double value) =>
            $"{value * 100:F1}% linear";

        public static SvfFilter.FilterType ToFilterType(double value)
        {
            if (value < 1f / 3)
                return SvfFilter.FilterType.Low;
            else if (value < 2f / 3)
                return SvfFilter.FilterType.Band;
            else if (value < 1)
                return SvfFilter.FilterType.Notch;
            else
                return SvfFilter.FilterType.High;
        }

        public static string FilterTypeToString(double value)
        {
            if (value < 1f / 3)
                return "lowpass";
            else if (value < 2f / 3)
                return "bandpass";
            else if (value < 1)
                return "notch";
            else
                return "highpass";
        }

        public static double ToFilterCutoffMultiplier(double value) => 
            Math.Pow(2, 13 * value - 2);

        public static string FilterCutoffMultiplierToString(double value) =>
            $"{13 * value:+0.0;-0.0} oct";

        public static int ToOversamplingOrder(double value)
        {
            if (value < 1f / 3)
                return 1;
            else if (value < 2f / 3)
                return 2;
            else if (value < 1)
                return 4;
            else
                return 8;
        }

        public static string OversamplingOrderToString(double value) =>
            $"{ToOversamplingOrder(value)}x";

        public static Voice.ModulationType ToModulationType(double value)
        {
            if (value < 0.25f)
                return Voice.ModulationType.None;
            else if (value < 0.5f)
                return Voice.ModulationType.AmplitudeModulationA;
            else if (value < 0.75f)
                return Voice.ModulationType.AmplitudeModulationB;
            else if (value < 1)
                return Voice.ModulationType.FrequencyModulationA;
            else
                return Voice.ModulationType.FrequencyModulationB;
        }

        public static string ModulationTypeToString(double value)
        {
            if (value < 0.25f)
                return "No modulation";
            else if (value < 0.5f)
                return "AM B -> A";
            else if (value < 0.75f)
                return "AM A -> B";
            else if (value < 1)
                return "FM B -> A";
            else
                return "FM A -> B";
        }
    }
}
