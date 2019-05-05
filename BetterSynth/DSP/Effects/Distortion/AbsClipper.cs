using System;

namespace BetterSynth
{
    /// <summary>
    /// Представляет собой вариант эффект сатурации.
    /// </summary>
    /// <seealso cref=" http://www.earlevel.com/main/2017/05/26/guitar-amp-simulation/"/>
    class AbsClipper : IDistortion
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
                return input * (2 - Math.Abs(input));
        }

        /// <summary>
        /// Устанавливает новое значение "силы" эффекта (в диапазоне [0, 1]).
        /// У данного варианта сатурации нет силы.
        /// </summary>
        /// <param name="value">Значение "силы" эффекта.</param>
        public void SetAmount(float value)
        {
        }
    }
}
