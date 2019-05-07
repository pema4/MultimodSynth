using System;

namespace MultimodSynth
{
    /// <summary>
    /// Представляет собой фильтр, убирающий постоянное амплитудное смещение.
    /// </summary>
    /// <seealso cref="https://ccrma.stanford.edu/~jos/filters/DC_Blocker_Software_Implementations.html"/>
    class DCBlocker : AudioComponent
    {
        /// <summary>
        /// Частота дискретизации, для которой вычислен коэффициент DefaultExp.
        /// </summary>
        private const float BaseSampleRate = 44100f;

        /// <summary>
        /// Коэффициент для генерации коэффициента скорости отклика фильтра.
        /// </summary>
        private double DefaultExp = 0.00079777080867193622;

        /// <summary>
        /// Предыдущие значения входного и выходного сигнала.
        /// </summary>
        private float xm1, ym1;

        /// <summary>
        /// Коэффициент фильтра.
        /// </summary>
        private float r;

        /// <summary>
        /// Коэффициент для нормализации выходного сигнала. 
        /// </summary>
        private float normalizationCoeff;

        /// <summary>
        /// Коэффициент скорости отклика.
        /// </summary>
        private float responseTimeCoefficient;

        /// <summary>
        /// Инициализирует новый объект класса DCBlocker.
        /// </summary>
        /// <param name="responseTimeCoefficient">Коэффициент скорости отклика фильтра</param>
        public DCBlocker(float responseTimeCoefficient = 1)
        {
            this.responseTimeCoefficient = responseTimeCoefficient;
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="x">Входной сигнал.</param>
        /// <returns>Выходной сигнал.</returns>
        public float Process(float x)
        {
            var y = normalizationCoeff * (x - xm1) + r * ym1;
            xm1 = x;
            ym1 = y;
            return y;
        }

        /// <summary>
        /// Устанавливает коэффициент времени отклика фильтра.
        /// </summary>
        /// <param name="value"></param>
        public void SetResponseTimeCoefficient(float value)
        {
            responseTimeCoefficient = value;
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            var coeff = BaseSampleRate / newSampleRate / responseTimeCoefficient;
            r = (float)Math.Exp(-2 * Math.PI * DefaultExp * coeff);
            normalizationCoeff = (1 + r) / 2;
        }
    }
}
