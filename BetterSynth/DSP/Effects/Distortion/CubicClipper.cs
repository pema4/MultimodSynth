namespace BetterSynth
{
    /// <summary>
    /// Представляет собой вариант эффекта сатурации.
    /// </summary>
    /// <seealso cref="https://ccrma.stanford.edu/realsimple/faust_strings/Cubic_Nonlinear_Distortion.html"/>
    class CubicClipper : IDistortion
    {
        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="input">Входной сигнал.</param>
        /// <returns>Выходной сигнал.</returns>
        public float Process(float input)
        {
            if (input < -1)
                return -1;
            else if (input > 1)
                return 1;
            else
                return 1.5f * input - 0.5f * input * input * input;
        }

        /// <summary>
        /// Устанавливает новое значение "силы" эффекта (в диапазоне [0, 1]).
        /// У этого варианта сатурации нет "силы".
        /// </summary>
        /// <param name="value">Значение "силы" эффекта.</param>
        public void SetAmount(float value)
        {
        }
    }
}
