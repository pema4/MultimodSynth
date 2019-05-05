using System;

namespace BetterSynth
{
    /// <summary>
    /// Представляет собой эффект ухудшения качества звука
    /// путём уменьшения частоты дискретизации.
    /// </summary>
    class SampleRateReductor : AudioComponent, IDistortion
    {
        /// <summary>
        /// Время, в течении которого проигрывается записанный сэмпл.
        /// </summary>
        private float holdTime;

        /// <summary>
        /// Фазовый аккумулятор.
        /// </summary>
        private float phasor;

        /// <summary>
        /// Инкремент фазы.
        /// </summary>
        private float phaseIncrement;

        /// <summary>
        /// Записанный сэмпл, который будет повторяться некоторое время.
        /// </summary>
        private float sample;

        /// <summary>
        /// Устанавливает новое значение "силы" эффекта (в диапазоне [0, 1]).
        /// Чем больше сила, тем меньше частота дискретизации.
        /// </summary>
        /// <param name="value">Значение "силы" эффекта.</param>
        public void SetAmount(float value)
        {
            holdTime = (float)Math.Pow(44100, 1 - value);
            UpdateCoefficients();
        }

        /// <summary>
        /// Обновление инкремента фазы при изменении некоторых параметров.
        /// </summary>
        private void UpdateCoefficients()
        {
            phaseIncrement = holdTime / SampleRate;
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="input">Входной сигнал.</param>
        /// <returns>Выходной сигнал.</returns>
        public float Process(float input)
        {
            var output = sample;

            phasor += phaseIncrement;
            if (phasor >= 1)
            {
                phasor -= 1;
                sample = input;
            }

            return output;
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
