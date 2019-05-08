using System;

namespace MultimodSynth
{
    /// <summary>
    /// Реализация низкочастотного генератора синусоиды.
    /// </summary>
    class SineLFO : AudioComponent
    {
        /// <summary>
        /// Частота генератора.
        /// </summary>
        private float frequency;

        /// <summary>
        /// Коэффициент для генерации.
        /// </summary>
        private float coeff;

        /// <summary>
        /// Текущее значение синусоиды.
        /// </summary>
        private float sin;

        /// <summary>
        /// Текущее значение косинусоиды.
        /// </summary>
        private float cos;

        /// <summary>
        /// Инициализирует новый объект типа SineLFO.
        /// </summary>
        public SineLFO()
        {
            sin = 0;
            cos = 1;
        }

        /// <summary>
        /// Устанавливает новое значение частоты.
        /// </summary>
        /// <param name="value"></param>
        public void SetFrequency(float value)
        {
            frequency = value;
            UpdateCoefficients();
        }

        /// <summary>
        /// Выполняет обновление коэффициента.
        /// </summary>
        private void UpdateCoefficients()
        {
            coeff = (float)(2 * Math.PI * frequency / SampleRate);
        }

        /// <summary>
        /// Генерация нового значения.
        /// </summary>
        /// <returns></returns>
        public float Process()
        {
            sin = sin + cos * coeff;
            cos = cos - sin * coeff;
            return sin;
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
