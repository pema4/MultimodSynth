using System;

namespace BetterSynth
{
    /// <summary>
    /// Фильтр низких частот, используемый для сглаживания изменения параметров.
    /// </summary>
    /// <seealso cref="http://www.musicdsp.org/en/latest/Filters/257-1-pole-lpf-for-smooth-parameter-changes.html"/>
    class ParameterFilter : AudioComponent
    {
        /// <summary>
        /// Частота дискретизации, для которой вычислен коэффициент DefaultExp.
        /// </summary>
        private const double BaseSampleRate = 44100;

        /// <summary>
        /// Коэффициент для генерации коэффициента скорости отклика фильтра.
        /// </summary>
        private const double DefaultExp = 0.0015995606308184566;

        /// <summary>
        /// Коэффициенты фильтра.
        /// </summary>
        private float a, b;

        /// <summary>
        /// Текущее сглаженное значение.
        /// </summary>
        private float value;

        /// <summary>
        /// Целевое значение фильтра.
        /// </summary>
        private float target;

        /// <summary>
        /// Коэффициент скорости отклика.
        /// </summary>
        private float responseTimeCoefficient;

        /// <summary>
        /// Показывает, активен ли фильтр в данный момент.
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Действие, выполняемое при изменении текущего значения.
        /// </summary>
        private Action<float> valueChangedAction;

        /// <summary>
        /// Инициализирует новый объект класса ParameterFilter, выполняющий заданное действие
        /// при каждом изменении сглаженного значения.
        /// </summary>
        /// <param name="valueChangedAction">Действие, выполняемое при изменении сглаженного значения.</param>
        /// <param name="initialValue">Начальное значение фильтра.</param>
        /// <param name="responseTimeCoefficient">Коэффициент скорости отклика фильтра</param>
        public ParameterFilter(
            Action<float> valueChangedAction, 
            float initialValue = 0, 
            float responseTimeCoefficient = 1)
        {
            a = 0.99f;
            b = 1f - a;
            value = initialValue;
            this.valueChangedAction = valueChangedAction;
            this.responseTimeCoefficient = responseTimeCoefficient;
        }

        /// <summary>
        /// Устанавливает целевое значение фильтра.
        /// </summary>
        /// <param name="value"></param>
        public void SetTarget(float value)
        {
            target = value;
            isActive = true;
        }

        /// <summary>
        /// Устанавливает коэффициент времени отклика фильтра.
        /// </summary>
        /// <param name="value"></param>
        public void SetResponseTimeCoefficient(float value)
        {
            responseTimeCoefficient = value;
        }

        /// <summary>
        /// Выполняет очередной ход сглаживания.
        /// </summary>
        public void Process()
        {
            if (isActive)
            {
                var newValue = (target * b) + (value * a);
                valueChangedAction(newValue);
                if (value != newValue)
                    value = newValue;
                else
                    isActive = false;
            }
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            var coeff = BaseSampleRate / newSampleRate / responseTimeCoefficient;
            a = (float)Math.Exp(-2 * Math.PI * DefaultExp * coeff);
            b = 1 - a;
        }
    }
}
