using System;

namespace MultimodSynth
{
    /// <summary>
    /// Содержит методы для преобразования нормализованных значений параметра
    /// в реальные и в отображаемые значения.
    /// </summary>
    static class Converters
    {
        /// <summary>
        /// Конвертирует нормализованное значение параметра в подстройку по частоте (в полутонах).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToSemitones(double value) =>
            (int)(-36 + value * 72);

        /// <summary>
        /// Возвращает строковое представление подстройки по частоте (в полутонах).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SemitonesToString(double value) =>
            $"{ToSemitones(value):+0;-0} semitones";

        /// <summary>
        /// Конвертирует нормализованное значение параметра в подстройку по частоте (в центах).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToCents(double value) =>
            (int)(-100 + value * 200);

        /// <summary>
        /// Возвращает строковое представление подстройки по частоте (в центах).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CentsToString(double value) =>
            $"{ToCents(value):+0;-0} cents";

        /// <summary>
        /// Конвертирует нормализованное значение параметра в объект WaveTableOscillator.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static WaveTableOscillator ToWaveTable(double value)
        {
            var waveTables = Utilities.WaveTables;
            int index = (int)(value * (waveTables.Length - 1));
            return waveTables[index];
        }

        /// <summary>
        /// Возвращает строковое представление выбранного объекта WaveTableOscillator.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Конвертирует нормализованное значение параметра в режим эффекта дилэй.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает строковое представление режима эффекта дилэй.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Конвертирует нормализованное значение параметра в время задержки эффекта дилэй.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToDelayTime(double value) =>
            Math.Exp(11.512925464970229 * (value - 1));

        /// <summary>
        /// Возвращает строковое представление времени задержки эффекта дилэй.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DelayTimeToString(double value) =>
            $"{ToDelayTime(value) * 1000:F2} ms";

        /// <summary>
        /// Конвертирует нормализованное значение параметра в стерео-эффект эффекта дилэй.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToStereoAmount(double value) =>
            -1 + 2 * value;

        /// <summary>
        /// Возвращает строковое представление стерео-эффекта эффекта дилэй.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string StereoAmountToString(double value)
        {
            value = ToStereoAmount(value);
            if (value < 0)
                return $"{100 * -value:F1}% left";
            else if (value == 0)
                return $"centered";
            else
                return $"{100 * +value:F1}% right";
        }

        /// <summary>
        /// Возвращает строковое представление значения в процентах.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string PercentsToString(double value) =>
            $"{100 * value:F1}%";

        /// <summary>
        /// Возвращает строковое представление параметра инверсии эффета дилэй.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string InvertToString(double value)
        {
            if (value < 0.5)
                return "normal";
            else
                return "inverted";
        }

        /// <summary>
        /// Конвертирует нормализованное значение параметра в частоту генератора низких частот эффекта дилэй.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToDelayLfoRate(double value) =>
            5 * value;

        /// <summary>
        /// Возвращает строковое представление частоты генератора низких частот эффекта дилэй.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DelayLfoRateToString(double value) =>
            $"{ToDelayLfoRate(value):F2} Hz";

        /// <summary>
        /// Конвертирует нормализованное значение параметра в режим эффекта дисторшн.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает строковое представление режима эффекта дисторшн.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Конвертирует нормализованное значение параметра в количество асимметрии эффекта дисторшн.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToAsymmetry(double value) =>
            -1 + 2 * value;

        /// <summary>
        /// Возвращает строковое представление количества ассиметрии эффекта дисторшн.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsymmetryToString(double value)
        {
            var asym = ToAsymmetry(value);
            if (asym == 0)
                return "centered";
            else
                return asym.ToString("+0.00;-0.00");
        }

        /// <summary>
        /// Конвертирует нормализованное значение параметра в увеличение уровня громкости эффекта дисторшн.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToDistortionAmp(double value) =>
            4 * value;

        /// <summary>
        /// Возвращает строковое представление увеличения громкости эффекта дисторшн.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DistortionAmpToString(double value) =>
            $"{ToDistortionAmp(value):F2}x";

        /// <summary>
        /// Конвертирует нормализованное значение параметра в частоту среза фильтра эффекта дисторшн.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToDistortionLowpassCutoff(double value) =>
            Math.Exp(2.9957322735539909 + 6.9077552789821368 * value);

        /// <summary>
        /// Возвращает строковое представление частоты среза фильтра эффекта дисторшн.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DistortionLowpassCutoffToString(double value) =>
            $"{ToDistortionLowpassCutoff(value):F1} Hz";

        /// <summary>
        /// Конвертирует нормализованное значение параметра в длительность стадии огибающей.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToEnvelopeTime(double value) =>
            value * 10;

        /// <summary>
        /// Возвращает строковое представление длительности стадии огибающей.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EnvelopeTimeToString(double value) =>
            $"{ToEnvelopeTime(value):F2} s";

        /// <summary>
        /// Возвращает строковое представление изгиба стадии огибающей.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EnvelopeCurveToString(double value) =>
            $"{value * 100:F1}% linear";

        /// <summary>
        /// Конвертирует нормализованное значение параметра в тип фильтра.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает строковое представление типа фильтра.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Конвертирует нормализованное значение параметра в множитель частоты среза фильтра.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToFilterCutoffMultiplier(double value) => 
            Math.Pow(2, 13 * value - 2);

        /// <summary>
        /// Возвращает строковое представление множителя частоты среза фильтра.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FilterCutoffMultiplierToString(double value) =>
            $"{13 * value:+0.0;-0.0} oct";

        /// <summary>
        /// Конвертирует нормализованное значение параметра в множитель частоты дискретизации.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает строковое представление множителя частоты дискретизации.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string OversamplingOrderToString(double value) =>
            $"{ToOversamplingOrder(value)}x";

        /// <summary>
        /// Конвертирует нормализованное значение параметра в тип модуляции.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Возвращает строковое представление типа модуляции.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
