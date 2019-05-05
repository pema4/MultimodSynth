using System;

namespace BetterSynth
{
    /// <summary>
    /// Представляет собой эффект дилэй с предварительной задержкой в левом или правом канале.
    /// </summary>
    class StereoOffsetDelay : AudioComponent, IDelay
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
        /// Предварительная линия задержки для левого канала.
        /// </summary>
        private DelayLine offsetBufferL;

        /// <summary>
        /// Предварительная линия задержки для правого канала.
        /// </summary>
        private DelayLine offsetBufferR;

        /// <summary>
        /// Инициализирует новый объект типа StereoOffsetDelay.
        /// </summary>
        public StereoOffsetDelay()
        {
            delayL = new DelayLine();
            delayR = new DelayLine();
            offsetBufferL = new DelayLine();
            offsetBufferR = new DelayLine();
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
            inputL = offsetBufferL.Process(inputL);
            inputR = offsetBufferR.Process(inputR);
            outputL = delayL.Process(inputL);
            outputR = delayR.Process(inputR);
        }

        /// <summary>
        /// Устанавливает новое значение времени задержки (в сэмплах).
        /// </summary>
        /// <param name="value">Время задержки.</param>
        public void SetDelay(float value)
        {
            delayL.SetDelay(value);
            delayR.SetDelay(value);
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
            value *= SampleRate;
            var leftDelay = -Math.Min(value, 0);
            var rightDelay = value + leftDelay;
            offsetBufferL.SetDelay(leftDelay);
            offsetBufferR.SetDelay(rightDelay);
        }


        /// <summary>
        /// Очищает буфер дилэя.
        /// </summary>
        public void Reset()
        {
            offsetBufferL.Reset();
            offsetBufferR.Reset();
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
            offsetBufferL.SampleRate = newSampleRate;
            offsetBufferR.SampleRate = newSampleRate;
        }
    }
}
