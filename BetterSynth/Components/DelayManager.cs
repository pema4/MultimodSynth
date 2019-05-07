using Jacobi.Vst.Framework;
using System;

namespace MultimodSynth
{
    /// <summary>
    /// Компонент плагина, отвечающий за эффект "дилэй".
    /// </summary>
    class DelayManager : AudioComponentWithParameters
    {
        /// <summary>
        /// Указывает тип эффекта дилэй.
        /// </summary>
        public enum StereoMode
        {
            None,
            StereoOffset,
            VariousTime,
            PingPong,
        }

        /// <summary>
        /// Максимальная амплитуда генератора низких частот.
        /// </summary>
        private const float MaxLfoDepth = 0.05f;

        /// <summary>
        /// Максимальное время задержки в секундах.
        /// </summary>
        private const float MaxTime = 1f;

        /// <summary>
        /// Текущее время задержки (в секундах).
        /// </summary>
        private float delay;

        /// <summary>
        /// Текущий коэффициент "чистого" сигнала без эффекта.
        /// </summary>
        private float dryCoeff = 1;

        /// <summary>
        /// Текущий коэффициент обратной связи.
        /// </summary>
        private float feedback;
        
        /// <summary>
        /// Текущая амплитуда генератора низких частот
        /// (в диапазоне [0, 1]).
        /// </summary>
        private float lfoDepth;

        /// <summary>
        /// Максимальное значение времени задержки (в сэмплах).
        /// </summary>
        private float maxDelay;

        /// <summary>
        /// Текущий тип эффекта дилэй.
        /// </summary>
        private StereoMode mode;

        /// <summary>
        /// Текущее значение коэффициента, показывающего, как сильно
        /// отличаются левый и правый канал выходного сигнала (значение в диапазоне [-1, 1]).
        /// </summary>
        private float stereoAmount;

        /// <summary>
        /// Текущий коффициент сигнала с применённым к нему эффектом.
        /// </summary>
        private float wetCoeff;

        /// <summary>
        /// Знак сигнала с применённым к нему эффектом.
        /// </summary>
        private int wetSign = 1;

        /// <summary>
        /// Текущий объект эффекта дилэй.
        /// </summary>
        private IDelay currentDelay;

        /// <summary>
        /// Объект класа PingPongDelay.
        /// </summary>
        private readonly PingPongDelay pingPongDelay;

        /// <summary>
        /// Объект класса VariousTimeDelay.
        /// </summary>
        private readonly VariousTimeDelay variousTimeDelay;

        /// <summary>
        /// Объект класса StereoOffsetDelay.
        /// </summary>
        private readonly StereoOffsetDelay stereoOffsetDelay;

        /// <summary>
        /// Генератор низких частот.
        /// </summary>
        private SineLFO lfo;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания параметра времени задержки.
        /// </summary>
        private ParameterFilter timeFilter;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания параметра стерео.
        /// </summary>
        private ParameterFilter stereoAmountFilter;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания параметра количество входного сигнала и выходного.
        /// </summary>
        private ParameterFilter mixFilter;

        /// <summary>
        /// Объект, управляющий параметром типа эффекта.
        /// </summary>
        public VstParameterManager ModeManager { get; set; }

        /// <summary>
        /// Объект, управляющий параметром времени задержки.
        /// </summary>
        public VstParameterManager TimeManager { get; set; }
        
        /// <summary>
        /// Объект, управляющий параметром коэффициента обратной связи.
        /// </summary>
        public VstParameterManager FeedbackManager { get; set; }

        /// <summary>
        /// Объект, управляющий параметром стерео.
        /// </summary>
        public VstParameterManager StereoAmountManager { get; set; }

        /// <summary>
        /// Объект, управляющий параметром количества входного и выходного сигналов.
        /// </summary>
        public VstParameterManager MixManager { get; set; }

        /// <summary>
        /// Объект, управляющий параметром инвертирования выходного сигнала.
        /// </summary>
        public VstParameterManager InvertManager { get; set; }

        /// <summary>
        /// Объект, управляющий параметром частоты генератора низких частот.
        /// </summary>
        public VstParameterManager LfoRateManager { get; set; }

        /// <summary>
        /// Объект, управляющий параметром амплитуды генератора низких частот.
        /// </summary>
        public VstParameterManager LfoDepthManager { get; set; }

        /// <summary>
        /// Инициализирует новый объект класса DelayManager, принадлежащий переданному плагину
        /// и имеющий переданный префикс названия параметров.
        /// </summary>
        /// <param name="plugin">Плагин, которому принадлежит создаваемый объект.</param>
        /// <param name="parameterPrefix">Префикс названия параметров.</param>
        public DelayManager(Plugin plugin, string parameterPrefix = "DL")
            : base(plugin, parameterPrefix)
        {
            stereoOffsetDelay = new StereoOffsetDelay();
            variousTimeDelay = new VariousTimeDelay();
            pingPongDelay = new PingPongDelay();
            lfo = new SineLFO();

            InitializeParameters();
        }

        /// <summary>
        /// Инициализирует параметры с помощью переданной фабрики параметров.
        /// </summary>
        /// <param name="factory">Фабрика параметров</param>
        protected override void InitializeParameters(ParameterFactory factory)
        {
            // Параметр типа дилэя.
            ModeManager = factory.CreateParameterManager(
                name: "MODE",
                valueChangedHandler: SetMode);

            // Параметр времени задержки.
            TimeManager = factory.CreateParameterManager(
                name: "TIME",
                defaultValue: 0.8f,
                valueChangedHandler: SetTimeTarget);
            timeFilter = new ParameterFilter(UpdateTime, 0, 100);
            TimeManager.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "ActiveParameter")
                    currentDelay?.Reset();
            };

            // Параметр коэффициента обратной связи.
            FeedbackManager = factory.CreateParameterManager(
                name: "FB",
                defaultValue: 0.5f,
                valueChangedHandler: SetFeedback);

            // Параметр стерео-эффекта.
            StereoAmountManager = factory.CreateParameterManager(
                name: "STER",
                defaultValue: 0.5f,
                valueChangedHandler: SetStereoAmountTarget);
            stereoAmountFilter = new ParameterFilter(UpdateStereoAmount, 0, 100);

            // Параметр количества выходного и входного сигналов.
            MixManager = factory.CreateParameterManager(
                name: "MIX",
                defaultValue: 0.5f,
                valueChangedHandler: SetMixTarget);
            mixFilter = new ParameterFilter(UpdateMix, 0);

            // Параметр инвертирования выходного сигнала.
            InvertManager = factory.CreateParameterManager(
                name: "INV",
                valueChangedHandler: SetInvert);

            // Параметр частоты генератора низких частот.
            LfoRateManager = factory.CreateParameterManager(
                name: "RATE",
                valueChangedHandler: SetLfoRate);

            // Параметр амплитуды генератора низких частот.
            LfoDepthManager = factory.CreateParameterManager(
                name: "DEPTH",
                valueChangedHandler: SetLfoDepth);
        }

        /// <summary>
        /// Обработчик изменения типа дилэя.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetMode(float value)
        {
            var newMode = Converters.ToDelayMode(value);
            if (newMode != mode)
            {
                mode = newMode;
                switch (mode)
                {
                    case StereoMode.StereoOffset:
                        ChangeDelay(stereoOffsetDelay);
                        break;
                    case StereoMode.VariousTime:
                        ChangeDelay(variousTimeDelay);
                        break;
                    case StereoMode.PingPong:
                        ChangeDelay(pingPongDelay);
                        break;
                }
            }
        }

        /// <summary>
        /// Изменяет текущий объект дилэя на новый.
        /// </summary>
        /// <param name="newDelay">Новый объект дилэя.</param>
        private void ChangeDelay(IDelay newDelay)
        {
            currentDelay = newDelay;
            currentDelay?.Reset();
            currentDelay?.SetFeedback(feedback);
            currentDelay?.SetStereo(stereoAmount);
        }

        /// <summary>
        /// Обработчик изменения времени задержки.
        /// </summary>
        /// <param name="target">Нормированное новое значение параметра.</param>
        private void SetTimeTarget(float target)
        {
            timeFilter.SetTarget((float)Converters.ToDelayTime(target));
        }

        /// <summary>
        /// Обработчик изменения "сглаженного" значения времени задержки.
        /// </summary>
        /// <param name="value">Новое значение времени задержки.</param>
        private void UpdateTime(float value)
        {
            delay = value * SampleRate;
        }

        /// <summary>
        /// Обработчик изменения коэффициента обратной связи.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetFeedback(float value)
        {
            feedback = value;
            currentDelay?.SetFeedback(feedback);
        }

        /// <summary>
        /// Обработчик изменения параметра стерео-эффекта.
        /// </summary>
        /// <param name="target">Нормированное новое значение параметра.</param>
        private void SetStereoAmountTarget(float target)
        {
            stereoAmountFilter.SetTarget((float)Converters.ToStereoAmount(target));
        }

        /// <summary>
        /// Обработчик изменения "сглаженного" значения параметра стерео-эффекта.
        /// </summary>
        /// <param name="value">Новое значение стерео-эффекта.</param>
        private void UpdateStereoAmount(float value)
        {
            stereoAmount = value;
            currentDelay?.SetStereo(stereoAmount);
        }

        /// <summary>
        /// Обработчик изменения параметра количества выходного и входного сигнала.
        /// </summary>
        /// <param name="target">Нормированное новое значение параметра.</param>
        private void SetMixTarget(float target)
        {
            mixFilter.SetTarget(target);
        }

        /// <summary>
        /// Обработчик изменения "сглаженного" значения количества выходного и входного сигнала.
        /// </summary>
        /// <param name="value">Новое значение.</param>
        private void UpdateMix(float value)
        {
            wetCoeff = value;
            dryCoeff = 1 - value;
        }

        /// <summary>
        /// Обработчик изменения параметра инвертирования выходного сигнала.
        /// </summary>
        /// <param name="target">Нормированное новое значение параметра.</param>
        private void SetInvert(float value)
        {
            if (value < 0.5)
                wetSign = 1;
            else
                wetSign = -1;
        }

        /// <summary>
        /// Обработчик изменения частоты генератора низких частот.
        /// </summary>
        /// <param name="target">Нормированное новое значение параметра.</param>
        private void SetLfoRate(float value)
        {
            lfo.SetFrequency((float)Converters.ToDelayLfoRate(value));
        }

        /// <summary>
        /// Обработчик изменения амплитуды генератора низких частот.
        /// </summary>
        /// <param name="target">Нормированное новое значение параметра.</param>
        private void SetLfoDepth(float value)
        {
            lfoDepth = value;
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
            // Сглаживание значений параметров.
            timeFilter.Process();
            mixFilter.Process();
            stereoAmountFilter.Process();

            if (mode == StereoMode.None)
            {
                outputL = inputL;
                outputR = inputR;
            }
            else
            {
                var lfoCoeff = 1 + MaxLfoDepth * lfoDepth * lfo.Process();
                currentDelay.SetDelay(Math.Min(maxDelay, delay * lfoCoeff));
                currentDelay.Process(inputL, inputR, out var wetL, out var wetR);
                outputL = dryCoeff * inputL + wetSign * wetCoeff * wetL;
                outputR = dryCoeff * inputL + wetSign * wetCoeff * wetR;
            }
        }
        
        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            timeFilter.SampleRate = newSampleRate;
            stereoAmountFilter.SampleRate = newSampleRate;
            mixFilter.SampleRate = newSampleRate;
            stereoOffsetDelay.SampleRate = newSampleRate;
            variousTimeDelay.SampleRate = newSampleRate;
            pingPongDelay.SampleRate = newSampleRate;
            lfo.SampleRate = newSampleRate;

            maxDelay = MaxTime * newSampleRate;
        }
    }
}
