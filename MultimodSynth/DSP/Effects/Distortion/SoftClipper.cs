namespace MultimodSynth
{
    /// <summary>
    /// Представляет собой эффект сатурации с изменяемым порогом.
    /// </summary>
    /// <seealso cref="http://www.musicdsp.org/en/latest/Effects/42-soft-saturation.html"/>
    class SoftClipper : IDistortion
    {
        /// <summary>
        /// Порог, начиная с которого начинает действовать эффект.
        /// </summary>
        private float treshold;

        /// <summary>
        /// Временная переменная, использующаяся для ускорения вычислений.
        /// </summary>
        private float denominator;

        /// <summary>
        /// Коэффициент нормализации.
        /// </summary>
        private float normalizationCoeff;

        /// <summary>
        /// Устанавливает новое значение "силы" эффекта (в диапазоне [0, 1]).
        /// </summary>
        /// <param name="value">Значение "силы" эффекта.</param>
        public void SetAmount(float value)
        {
            treshold = 1 - value;
            var temp = 1 - treshold;
            denominator = temp * temp;
            normalizationCoeff = 1 / ((treshold + 1) / 2);
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="input">Входной сигнал.</param>
        /// <returns>Выходной сигнал.</returns>
        public float Process(float input)
        {
            if (input < -1)
                return -1;
            else if (input < -treshold)
            {
                var temp = input + treshold;
                return normalizationCoeff * (-treshold + temp / (1 + temp * temp / denominator));
            }
            else if (input < treshold)
                return normalizationCoeff * input;
            else if (input < 1)
            {
                var temp = input - treshold;
                return normalizationCoeff * (treshold + temp / (1 + temp * temp / denominator));
            }
            else
                return 1;
        }
    }
}
