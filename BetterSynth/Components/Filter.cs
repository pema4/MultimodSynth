using System;

namespace BetterSynth
{
    /// <summary>
    /// Компонент голоса плагина, представляющий собой SVF-фильтр.
    /// </summary>
    class Filter : AudioComponent
    {
        /// <summary>
        /// Базовая частота среза фильтра.
        /// </summary>
        private const float BaseCutoff = 65.41f;

        /// <summary>
        /// Массив заранее сгенерированных коэффициентов. Используется для ускорения вычислений.
        /// </summary>
        private static float[] FilterModulationLookup;
        
        static Filter()
        {
            var length = 1000;
            FilterModulationLookup = new float[length];
            for (int i = 0; i < length; ++i)
                FilterModulationLookup[i] = (float)Math.Pow(2, 10.0 * i / (length - 1));
        }

        /// <summary>
        /// Объект фильтра.
        /// </summary>
        private SvfFilter filter;

        /// <summary>
        /// Частота играемой ноты.
        /// </summary>
        private float noteFrequency;

        /// <summary>
        /// Множитель частоты срезы фильтра.
        /// </summary>
        private float cutoffMultiplier;

        /// <summary>
        /// Коэффициент отслеживания частоты играемой ноты.
        /// </summary>
        private float trackingCoeff;

        /// <summary>
        /// Текущая частота среза фильтра.
        /// </summary>
        private float cutoff;

        /// <summary>
        /// Инициализирует новый объект класса Filter.
        /// </summary>
        public Filter()
        {
            filter = new SvfFilter(type: SvfFilter.FilterType.Low);
        }

        /// <summary>
        /// Устанавливает новое значение типа фильтра.
        /// </summary>
        /// <param name="value"></param>
        public void SetFilterType(SvfFilter.FilterType value)
        {
            filter.SetType(value);
        }

        /// <summary>
        /// Устанавливает новое значение множителя частоты среза.
        /// </summary>
        /// <param name="value"></param>
        public void SetCutoffMultiplier(float value)
        {
            cutoffMultiplier = value;
            CalculateCutoff();
        }

        /// <summary>
        /// Устанавливает новое значение частоты играемой ноты.
        /// </summary>
        /// <param name="value"></param>
        public void SetNoteFrequency(float value)
        {
            noteFrequency = value;
            CalculateCutoff();
        }

        /// <summary>
        /// Устанавливает новое значение коэффициента отслеживания частоты играемой ноты.
        /// </summary>
        /// <param name="value"></param>
        public void SetTrackingCoeff(float value)
        {
            trackingCoeff = value;
            CalculateCutoff();
        }

        /// <summary>
        /// Обновляет текущую частоту среза.
        /// </summary>
        private void CalculateCutoff()
        {
            var trackedCutoff = BaseCutoff + (noteFrequency - BaseCutoff) * trackingCoeff;
            cutoff = cutoffMultiplier * trackedCutoff;
        }

        /// <summary>
        /// Устанавливает новое значение "ширины" фильра.
        /// </summary>
        /// <param name="value"></param>
        public void SetCurve(float value)
        {
            float q;
            if (value >= 0.5f)
                q = (float)Math.Pow(16, 2 * value - 1);
            else
                q = (float)Math.Pow(4, 2 * value - 1);
                
            filter.SetQ(q);
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="input">Входной сигнал.</param>
        /// <param name="cutoffModulation">Модуляция частоты среза фильтра.</param>
        /// <returns>Выходной сигнал.</returns>
        public float Process(float input, float cutoffModulation = 0)
        {
            var modulatedCutoff = cutoff;
            modulatedCutoff *= 1 + FilterModulationLookup[(int)(999 * cutoffModulation)];
            if (modulatedCutoff > 20000)
                modulatedCutoff = 20000;
            filter.SetCutoff(modulatedCutoff);

            return filter.Process(input);
        }

        /// <summary>
        /// Сбрасывает текущее состояние фильтра.
        /// </summary>
        public void Reset() => filter.Reset();

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            filter.SampleRate = newSampleRate;
        }
    }
}