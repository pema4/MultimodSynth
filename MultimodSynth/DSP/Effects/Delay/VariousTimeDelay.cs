using System;

namespace MultimodSynth
{
    /// <summary>
    /// Представляет собой эффект дилэй с отличающимся временем задержки в левом и правом канале.
    /// </summary>
    class VariousTimeDelay : AudioComponent, IDelay
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
        /// Базовое время задержки (в сэмплах).
        /// </summary>
        private float delay;

        /// <summary>
        /// Коэффициент времени задержки для левого канала.
        /// </summary>
        private float leftDelayCoeff;

        /// <summary>
        /// Коэффициент времени задержки для правого канала.
        /// </summary>
        private float rightDelayCoeff;

        /// <summary>
        /// Инициализирует новый объект типа VariousTimeDelay.
        /// </summary>
        public VariousTimeDelay()
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
            outputL = delayL.Process(inputL);
            outputR = delayR.Process(inputR);
        }


        /// <summary>
        /// Устанавливает новое значение времени задержки (в сэмплах).
        /// </summary>
        /// <param name="value">Время задержки.</param>
        public void SetDelay(float value)
        {
            delay = value;
            delayL.SetDelay(delay * leftDelayCoeff);
            delayR.SetDelay(delay * rightDelayCoeff);
        }

        /// <summary>
        /// Устанавливает новое значение коэффициента обратной связи.
        /// </summary>
        /// <param name="value">Коэффициент обратной связи.</param>
        public void SetFeedback(float value)
        {
            delayL.SetFeedback(value);
            delayR.SetFeedback(value);
        }

        /// <summary>
        /// Устанавливает новое значение коэффициента стерео-эффекта.
        /// </summary>
        /// <param name="value">Коэффициент стерео-эффекта.</param>
        public void SetStereo(float value)
        {
            leftDelayCoeff = 1 + Math.Min(value, 0) * 0.99f;
            rightDelayCoeff = 1 - Math.Max(value, 0) * 0.99f;
            delayL.SetDelay(delay * leftDelayCoeff);
            delayR.SetDelay(delay * rightDelayCoeff);
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
