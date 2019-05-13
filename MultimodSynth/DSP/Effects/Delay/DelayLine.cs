namespace MultimodSynth
{
    /// <summary>
    /// Представляет собой линию задержки.
    /// </summary>
    /// <seealso cref="https://ccrma.stanford.edu/~jos/pasp/Variable_Delay_Lines.html"/>
    class DelayLine : AudioComponent
    {
        /// <summary>
        /// Максимальное время задержки (в секундах).
        /// </summary>
        private const float MaxTime = 1f;

        /// <summary>
        /// Буфер, используемый для хранения предыдущих сэмплов.
        /// </summary>
        private float[] buffer;

        /// <summary>
        /// Длина буфера.
        /// </summary>
        private int bufferLength;

        /// <summary>
        /// Указатель на ячейку буфера для записи.
        /// </summary>
        private int writePoint;

        /// <summary>
        /// Указатель на ячейку буфера для чтения.
        /// </summary>
        private int readPoint;

        /// <summary>
        /// Сдвиг точки чтения относительно указателя для чтения.
        /// Используется для нецелочисленного времени задержки.
        /// </summary>
        private float readOffset;

        /// <summary>
        /// Коэффициент обратной связи.
        /// </summary>
        private float feedback;

        /// <summary>
        /// Время задержки (в сэмплах).
        /// </summary>
        private float delay;

        /// <summary>
        /// Устанавливает новое значение времени задержки (в сэмплах).
        /// </summary>
        /// <param name="delay">Время задержки.</param>
        public void SetDelay(float delay)
        {
            if (this.delay == delay)
                return;

            this.delay = delay;
            double delayedPoint = (double)writePoint - delay;
            if (delayedPoint < 0)
                delayedPoint += bufferLength;

            readPoint = (int)delayedPoint;
            if (readPoint == bufferLength)
                readPoint -= 1;
            readOffset = (float)(delayedPoint - readPoint);
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
        /// Возвращает значение в точке чтения (используется линейная интерполяция).
        /// </summary>
        /// <returns>Значение в точке чтения.</returns>
        public float CalculateOutput()
        {
            float res = buffer[readPoint] * (1 - readOffset);
            var nextPoint = readPoint + 1;
            if (nextPoint < bufferLength)
                res += buffer[nextPoint] * readOffset;
            else
                res += buffer[0] * readOffset;
            return res;
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="input">Входной сигнал.</param>
        /// <returns>Выходной сигнал.</returns>
        public float Process(float input)
        {
            buffer[writePoint] = input;

            float delayedSample = CalculateOutput();
            readPoint += 1;
            if (readPoint == bufferLength)
                readPoint = 0;

            buffer[writePoint] += delayedSample * feedback;
            writePoint += 1;
            if (writePoint == bufferLength)
                writePoint = 0;

            return delayedSample;
        }

        /// <summary>
        /// Очищает буфер линии задержки.
        /// </summary>
        public void Reset()
        {
            buffer.Initialize();
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            bufferLength = (int)(newSampleRate * MaxTime);
            buffer = new float[bufferLength];
        }
    }
}
