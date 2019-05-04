namespace BetterSynth
{
    /// <summary>
    /// Базовый класс, предоставляющий своим наследникам информацию о частоте дискретизации.
    /// </summary>
    abstract class AudioComponent
    {
        private float sampleRate;

        /// <summary>
        /// Частота дискретизации, на которой работает данный компонент
        /// </summary>
        public float SampleRate
        {
            get => sampleRate;
            set
            {
                if (sampleRate != value)
                {
                    sampleRate = value;
                    OnSampleRateChanged(sampleRate);
                }
            }
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected virtual void OnSampleRateChanged(float newSampleRate)
        {
        }
    }
}
