using System;

namespace MultimodSynth
{
    /// <summary>
    /// Реализация SVF (State Variable Filter).
    /// </summary>
    /// <seealso cref="https://cytomic.com/files/dsp/SvfLinearTrapOptimised2.pdf"/>
    class SvfFilter : AudioComponent
    {
        /// <summary>
        /// Указывает тип фильтра.
        /// </summary>
        public enum FilterType
        {
            None,
            Low,
            Band,
            High,
            Notch,
            Peak,
            All,
            Bell,
            LowShelf,
            HighShelf,
        };

        /// <summary>
        /// Текущий тип фильтра.
        /// </summary>
        private FilterType type;

        /// <summary>
        /// Частота среза фильтра.
        /// </summary>
        private float cutoff;

        /// <summary>
        /// "Ширина" фильтра.
        /// </summary>
        private float q = 1;

        /// <summary>
        /// Первый аккумулятор.
        /// </summary>
        private float ic1eq = 0;

        /// <summary>
        /// Второй аккумулятор.
        /// </summary>
        private float ic2eq = 0;

        /// <summary>
        /// Коэффициент фильтра.
        /// </summary>
        private float a1;

        /// <summary>
        /// Коэффициент фильтра.
        /// </summary>
        private float a2;

        /// <summary>
        /// Коэффициент фильтра.
        /// </summary>
        private float a3;

        /// <summary>
        /// Коэффициент фильтра.
        /// </summary>
        private float m0;

        /// <summary>
        /// Коэффициент фильтра.
        /// </summary>
        private float m1;

        /// <summary>
        /// Коэффициент фильтра.
        /// </summary>
        private float m2;

        /// <summary>
        /// Коэффициент фильтра.
        /// </summary>
        private float gain;

        /// <summary>
        /// Инициализирует новый объект типа SvfFilter c заданными параметрами.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cutoff"></param>
        /// <param name="q"></param>
        /// <param name="gain"></param>
        public SvfFilter(
            FilterType type = FilterType.None,
            float cutoff = 20000,
            float q = 1,
            float gain = 0)
        {
            SetType(type);
            SetCutoff(cutoff);
            SetQ(q);
            SetGain(gain);
        }

        /// <summary>
        /// Устанавливает новое значение типа фильтра.
        /// </summary>
        /// <param name="value"></param>
        public void SetType(FilterType value)
        {
            type = value;
            UpdateCoefficients();
            Reset();
        }

        /// <summary>
        /// Устанавливает новое значение частоты среза фильтра.
        /// </summary>
        /// <param name="value"></param>
        public void SetCutoff(float value)
        {
            if (cutoff != value)
            {
                cutoff = value;
                UpdateCoefficients();
            }
        }

        /// <summary>
        /// Устанавливает новое значение "ширины" фильтра.
        /// </summary>
        /// <param name="value"></param>
        public void SetQ(float value)
        {
            q = value;
            UpdateCoefficients();
        }

        /// <summary>
        /// Устанавливает новое значение увеличения уровня громкости.
        /// </summary>
        /// <param name="value"></param>
        public void SetGain(float value)
        {
            gain = value;
            UpdateCoefficients();
        }

        /// <summary>
        /// Выполняет обновление всех коэффициентов.
        /// </summary>
        private void UpdateCoefficients()
        {
            switch (type)
            {
                case FilterType.Low:
                    var g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    var k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 0;
                    m1 = 0;
                    m2 = 1;
                    break;

                case FilterType.Band:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 0;
                    m1 = 1;
                    m2 = 0;
                    break;

                case FilterType.High:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = -k;
                    m2 = -1;
                    break;

                case FilterType.Notch:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = -k;
                    m2 = 0;
                    break;

                case FilterType.Peak:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = -k;
                    m2 = -2;
                    break;

                case FilterType.All:
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = -2 * k;
                    m2 = 0;
                    break;

                case FilterType.Bell:
                    var A = (float)Math.Pow(10, gain / 40);
                    g = (float)Math.Tan(Math.PI * cutoff / SampleRate);
                    k = 1 / (q * A);
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = k * (A * A - 1);
                    m2 = 0;
                    break;

                case FilterType.LowShelf:
                    A = (float)Math.Pow(10, gain / 40);
                    g = (float)(Math.Tan(Math.PI * cutoff / SampleRate) / Math.Sqrt(A));
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = 1;
                    m1 = k * (A - 1);
                    m2 = A * A - 1;
                    break;

                case FilterType.HighShelf:
                    A = (float)Math.Pow(10, gain / 40);
                    g = (float)(Math.Tan(Math.PI * cutoff / SampleRate) * Math.Sqrt(A));
                    k = 1 / q;
                    a1 = 1 / (1 + g * (g + k));
                    a2 = g * a1;
                    a3 = g * a2;
                    m0 = A * A;
                    m1 = k * (1 - A) * A;
                    m2 = 1 - A * A;
                    break;
            }
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="v0">Входной сигнал.</param>
        /// <returns>Выходной сигнал.</returns>
        public float Process(float v0)
        {
            float v3 = v0 - ic2eq;
            float v1 = a1 * ic1eq + a2 * v3;
            float v2 = ic2eq + a2 * ic1eq + a3 * v3;
            ic1eq = 2 * v1 - ic1eq;
            ic2eq = 2 * v2 - ic2eq;
            return m0 * v0 + m1 * v1 + m2 * v2;
        }

        /// <summary>
        /// Сбрасывает текущее состояние фильтра.
        /// </summary>
        public void Reset()
        {
            ic1eq = 0;
            ic2eq = 0;
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            UpdateCoefficients();
        }
    }
}
