using System;

namespace MultimodSynth
{
    /// <summary>
    /// Компонент голоса плагина, представляющий осциллятор.
    /// </summary>
    class Oscillator : AudioComponent
    {
        /// <summary>
        /// Частота играемой ноты.
        /// </summary>
        private float noteFrequency;

        /// <summary>
        /// Играемая частота.
        /// </summary>
        private float frequency;

        /// <summary>
        /// Изменение фазы за 1 сэмпл.
        /// </summary>
        private float phaseIncrement;

        /// <summary>
        /// Множитель частоты.
        /// </summary>
        private float pitchMultiplier;

        /// <summary>
        /// Текущая фаза осциллятора.
        /// </summary>
        private float phasor;

        /// <summary>
        /// Ссылка на используемый объект класса WaveTable.
        /// </summary>
        private WaveTableOscillator waveTable;

        /// <summary>
        /// Устанавливает новый объект класса WaveTable.
        /// </summary>
        /// <param name="waveTable"></param>
        public void SetWaveTable(WaveTableOscillator waveTable)
        {
            this.waveTable = waveTable;
            this.waveTable.SetPhaseIncrement(phaseIncrement);
        }

        /// <summary>
        /// Устанавливает новое значение частоты играемой ноты.
        /// </summary>
        /// <param name="value"></param>
        public void SetNoteFrequency(float value)
        {
            noteFrequency = value;
            UpdateCoefficients();
        }

        /// <summary>
        /// Устанавливает новое значение множителя частоты.
        /// </summary>
        /// <param name="value"></param>
        public void SetPitchMultiplier(float value)
        {
            pitchMultiplier = value;
            UpdateCoefficients();
        }

        /// <summary>
        /// Обновляет все коэффициенты.
        /// </summary>
        private void UpdateCoefficients()
        {
            frequency = noteFrequency * pitchMultiplier;
            phaseIncrement = frequency / SampleRate;
            waveTable?.SetPhaseIncrement(phaseIncrement);
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="phaseModulation">Фазовая модуляция.</param>
        /// <returns>Выходной сигнал.</returns>
        public float Process(float phaseModulation = 0)
        {
            var phase = phasor + phaseModulation;
            phase -= (float)Math.Floor(phase);
            var waveTable = this.waveTable;
            float result;
            if (waveTable == null)
                result = 0;
            else
                result = waveTable.Process(phase);

            phasor += phaseIncrement;
            if (phasor >= 1)
                phasor -= 1;

            return result;
        }

        /// <summary>
        /// Сбрасывает текущее состояние осциллятора.
        /// </summary>
        public void Reset() => phasor = 0;

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            phaseIncrement = frequency / newSampleRate;
        }
    }
}
