namespace MultimodSynth
{
    /// <summary>
    /// Интерфейс, представляющий собой эффект дилэй.
    /// </summary>
    interface IDelay
    {
        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="inputL">Левый канал входного сигнала</param>
        /// <param name="inputR">Правый канал входного сигнала</param>
        /// <param name="outputL">Левый канал выходного сигнала.</param>
        /// <param name="outputR">Правый канал выходного сигнала.</param>
        void Process(float inputL, float inputR, out float outputL, out float outputR);

        /// <summary>
        /// Устанавливает новое значение времени задержки (в сэмплах).
        /// </summary>
        /// <param name="value">Время задержки.</param>
        void SetDelay(float value);

        /// <summary>
        /// Устанавливает новое значение коэффициента обратной связи.
        /// </summary>
        /// <param name="value">Коэффициент обратной связи.</param>
        void SetFeedback(float value);

        /// <summary>
        /// Устанавливает новое значение коэффициента стерео-эффекта.
        /// </summary>
        /// <param name="value">Коэффициент стерео-эффекта.</param>
        void SetStereo(float value);

        /// <summary>
        /// Очищает буфер дилэя.
        /// </summary>
        void Reset();
    }
}
