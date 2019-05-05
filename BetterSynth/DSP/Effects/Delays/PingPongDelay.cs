namespace BetterSynth
{
    /// <summary>
    /// Представляет собой эффект пинг-понг дилэй.
    /// </summary>
    class PingPongDelay : AudioComponent, IDelay
    {
        /// <summary>
        /// Линия задержки для левого канала.
        /// </summary>
        private DelayLine delayL;

        /// <summary>
        /// Линия задержки для правого канала.
        /// </summary>
        private DelayLine delayR;

        /// <summary>
        /// Коэффициент обратной связи.
        /// </summary>
        private float feedback;

        /// <summary>
        /// Коэффициент начальной громкости левого канала.
        /// </summary>
        private float leftCoeff;

        /// <summary>
        /// Коэффициент начальной громкости правого канала.
        /// </summary>
        private float rightCoeff;

        /// <summary>
        /// Инициализирует новый объект типа PingPongDelay.
        /// </summary>
        public PingPongDelay()
        {
            delayL = new DelayLine();
            delayR = new DelayLine();
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="inputL">Левый канал входного сигнала</param>
        /// <param name="inputR">Правый канал входного сигнала</param>
        /// <param name="outputL">Левый канал выходного сигнала.</param>
        /// <param name="outputR">Правый канал выходного сигнала.</param>
        public void Process(float inputL, float inputR, out float outputL, out float outputR)
        {
            outputL = delayL.CalculateOutput();
            outputR = delayR.CalculateOutput();
            delayL.Process(inputL * leftCoeff + outputR * feedback);
            delayR.Process(inputR * rightCoeff + outputL * feedback);
        }

        /// <summary>
        /// Устанавливает новое значение времени задержки (в сэмплах).
        /// </summary>
        /// <param name="value">Время задержки.</param>
        public void SetDelay(float delay)
        {
            delayL.SetDelay(delay);
            delayR.SetDelay(delay);
        }

        /// <summary>
        /// Устанавливает новое значение коэффициента обратной связи.
        /// </summary>
        /// <param name="value">Коэффициент обратной связи.</param>
        public void SetFeedback(float value)
        {
            feedback = value;
        }

        /// <summary>
        /// Устанавливает новое значение коэффициента стерео-эффекта.
        /// </summary>
        /// <param name="value">Коэффициент стерео-эффекта.</param>
        public void SetStereo(float value)
        {
            if (value < 0)
            {
                leftCoeff = 1;
                rightCoeff = 1 + value;
            }
            else
            {
                rightCoeff = 1;
                leftCoeff = 1 - value;
            }
        }

        /// <summary>
        /// Очищает буфер дилэя.
        /// </summary>
        public void Reset()
        {
            delayL.Reset();
            delayR.Reset();
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            delayL.SampleRate = newSampleRate;
            delayR.SampleRate = newSampleRate;
        }
    }
}
