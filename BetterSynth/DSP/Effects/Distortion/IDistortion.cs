namespace BetterSynth
{
    /// <summary>
    /// Интерфейс, представляющий собой эффект дисторшн.
    /// </summary>
    interface IDistortion
    {
        /// <summary>
        /// Устанавливает новое значение "силы" эффекта (в диапазоне [0, 1]).
        /// </summary>
        /// <param name="value">Значение "силы" эффекта.</param>
        void SetAmount(float value);

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="input">Входной сигнал.</param>
        /// <returns>Выходной сигнал.</returns>
        float Process(float input);
    }
}
