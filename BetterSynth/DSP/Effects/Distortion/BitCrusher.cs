using System;

namespace MultimodSynth
{
    /// <summary>
    /// Представляет собой эффект ухудшения качества звука
    /// путём уменьшения глубины квантования.
    /// </summary>
    class BitCrusher : IDistortion
    {
        /// <summary>
        /// Количество возможных уровней входного сигнала.
        /// </summary>
        private float steps;
        
        /// <summary>
        /// Устанавливает новое значение "силы" эффекта (в диапазоне [0, 1]).
        /// Чем больше сила, тем меньше глубина квантования.
        /// </summary>
        /// <param name="value">Значение "силы" эффекта.</param>
        public void SetAmount(float value)
        {
            steps = (float)Math.Pow(1 << 16, 1 - value);
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="input">Входной сигнал.</param>
        /// <returns>Выходной сигнал.</returns>
        public float Process(float input)
        {
            return (float)Math.Round(steps * input) / steps;
        }
    }
}
