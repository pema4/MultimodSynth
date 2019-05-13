using Jacobi.Vst.Framework;

namespace MultimodSynth
{
    /// <summary>
    /// Компонент плагина, отвечающий за эффект "дисторшн".
    /// </summary>
    class DistortionManager : AudioComponentWithParameters
    {
        /// <summary>
        /// Указывает тип эффекта дисторшн.
        /// </summary>
        public enum DistortionMode
        {
            None,
            AbsClipping,
            SoftClipping,
            CubicClipping,
            BitCrush,
            SampleRateReduction,
        }
        
        /// <summary>
        /// Текущая сила эффекта дисторшн.
        /// </summary>
        private float amount;

        /// <summary>
        /// Текущий уровень громкости входного сигнала.
        /// </summary>
        private float amp = 1;

        /// <summary>
        /// Текущее значение постоянного амплитудного смещения.
        /// </summary>
        private float dcOffset;

        /// <summary>
        /// Текущий коэффициент "чистого" сигнала без эффекта.
        /// </summary>
        private float dryCoeff = 1;

        /// <summary>
        /// Текущий тип эффекта дисторшн.
        /// </summary>
        private DistortionMode mode;

        /// <summary>
        /// Текущий коэффициент сигнала с применённым к нему эффектом.
        /// </summary>
        private float wetCoeff;

        /// <summary>
        /// Текущий объект эффекта дисторшн.
        /// </summary>
        private IDistortion currentDistortion;

        /// <summary>
        /// Объект класса SoftClipper.
        /// </summary>
        private readonly SoftClipper softClipper;

        /// <summary>
        /// Объект класса AbsClipper.
        /// </summary>
        private readonly AbsClipper absClipper;

        /// <summary>
        /// Объект класса CubicClipper.
        /// </summary>
        private readonly CubicClipper cubicClipper;

        /// <summary>
        /// Объект класса BitCrusher.
        /// </summary>
        private readonly BitCrusher bitCrusher;

        /// <summary>
        /// Объект класса SampleRateReductor.
        /// </summary>
        private readonly SampleRateReductor sampleRateReductor;

        /// <summary>
        /// Фильтр высоких частот для устранения постоянного амплитудного смещения.
        /// </summary>
        private DCBlocker dcBlocker;

        /// <summary>
        /// Фильтр низких частот, применяющийся к входному сигналу.
        /// </summary>
        private SvfFilter lowPass;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания параметра
        /// уровня громкости входного сигнала.
        /// </summary>
        private ParameterFilter ampFilter;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания параметра количества входного и выходного сигналов.
        /// </summary>
        private ParameterFilter mixFilter;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания параметра постоянного амплитудного сдвига.
        /// </summary>
        private ParameterFilter asymmetryFilter;

        /// <summary>
        /// Объект, управляющий параметром типа эффекта.
        /// </summary>
        public VstParameterManager ModeManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром силы эффекта.
        /// </summary>
        public VstParameterManager AmountManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром постоянного амплитудного сдвига.
        /// </summary>
        public VstParameterManager AsymmetryManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром громкости входного сигнала.
        /// </summary>
        public VstParameterManager AmpManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром частоты среза фильтра низких частот.
        /// </summary>
        public VstParameterManager LowPassCutoffManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром количества выходного и входного сигналов.
        /// </summary>
        public VstParameterManager MixManager { get; private set; }

        /// <summary>
        /// Инициализирует новый объект класса DistortionManager, принадлежащий переданному плагину
        /// и имеющий переданный префикс названия параметров.
        /// </summary>
        /// <param name="plugin">Плагин, которому принадлежит создаваемый объект.</param>
        /// <param name="parameterPrefix">Префикс названия параметров.</param>
        public DistortionManager(
            Plugin plugin,
            string parameterPrefix = "D")
            : base(plugin, parameterPrefix)
        {
            dcBlocker = new DCBlocker(10);
            lowPass = new SvfFilter(type: SvfFilter.FilterType.Low);
            absClipper = new AbsClipper();
            softClipper = new SoftClipper();
            cubicClipper = new CubicClipper();
            bitCrusher = new BitCrusher();
            sampleRateReductor = new SampleRateReductor();

            InitializeParameters();
        }

        /// <summary>
        /// Инициализирует параметры с помощью переданной фабрики параметров.
        /// </summary>
        /// <param name="factory">Фабрика параметров</param>
        protected override void InitializeParameters(ParameterFactory factory)
        {
            // Параметр типа дисторшна.
            ModeManager = factory.CreateParameterManager(
                name: "TYPE",
                valueChangedHandler: SetMode);

            // Параметр силы эффекта.
            AmountManager = factory.CreateParameterManager(
                name: "AMNT",
                defaultValue: 0.5f,
                valueChangedHandler: SetAmount);

            // Параметр постоянного амплитудного сдвига.
            AsymmetryManager = factory.CreateParameterManager(
                name: "ASYM",
                defaultValue: 0.5f,
                valueChangedHandler: SetAsymmetryTarget);
            asymmetryFilter = new ParameterFilter(UpdateAsymmetry, 0);

            // Параметр уровня громкости входного сигнала.
            AmpManager = factory.CreateParameterManager(
                name: "AMP",
                defaultValue: 0.25f,
                valueChangedHandler: SetAmpTarget);
            ampFilter = new ParameterFilter(UpdateAmp, 1);

            // Параметр частоты среза фильтра низких частот для входного сигнала.
            LowPassCutoffManager = factory.CreateParameterManager(
                name: "LP",
                defaultValue: 1,
                valueChangedHandler: SetLowPassCutoff);

            // Параметр количества выходного и входного сигналов.
            MixManager = factory.CreateParameterManager(
                name: "MIX",
                defaultValue: 0.5f,
                valueChangedHandler: x => mixFilter.SetTarget(x));
            mixFilter = new ParameterFilter(UpdateMix, 0);
        }

        /// <summary>
        /// Обработчик изменения типа дисторшна.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetMode(float value)
        {
            var newMode = Converters.ToDistortionMode(value);
            if (newMode != mode)
            {
                mode = newMode;
                switch (mode)
                {
                    case DistortionMode.AbsClipping:
                        ChangeDistortion(absClipper);
                        break;
                    case DistortionMode.SoftClipping:
                        ChangeDistortion(softClipper);
                        break;
                    case DistortionMode.CubicClipping:
                        ChangeDistortion(cubicClipper);
                        break;
                    case DistortionMode.BitCrush:
                        ChangeDistortion(bitCrusher);
                        break;
                    case DistortionMode.SampleRateReduction:
                        ChangeDistortion(sampleRateReductor);
                        break;
                }
            }
        }

        /// <summary>
        /// Изменяет текущий объект дисторшна на новый.
        /// </summary>
        /// <param name="newDistortion">Новый объект дисторшна.</param>
        private void ChangeDistortion(IDistortion newDistortion)
        {
            currentDistortion = newDistortion;
            currentDistortion?.SetAmount(amount);
        }

        /// <summary>
        /// Обработчик изменения силы эффекта.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetAmount(float value)
        {
            amount = value;
            currentDistortion?.SetAmount(amount);
        }

        /// <summary>
        /// Обработчик изменения коэффициента постоянного амплитудного сдвига.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetAsymmetryTarget(float value)
        {
            asymmetryFilter.SetTarget(value);
        }

        /// <summary>
        /// Обработчик изменения "сглаженного" значения постоянного амплитудного сдвига.
        /// </summary>
        /// <param name="value">Новое значение постоянного амплитудного сдвига.</param>
        private void UpdateAsymmetry(float value)
        {
            dcOffset = (float)Converters.ToAsymmetry(value);
        }

        /// <summary>
        /// Обработчик изменения частоты среза фильтра низких частот.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetLowPassCutoff(float value)
        {
            var cutoff = (float)Converters.ToDistortionLowpassCutoff(value);
            lowPass.SetCutoff(cutoff);
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
        /// Обработчик изменения уровня входного сигнала.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetAmpTarget(float value)
        {
            ampFilter.SetTarget((float)Converters.ToDistortionAmp(value));
        }

        /// <summary>
        /// Обработчик изменения "сглаженного" значения уровня входного сигнала.
        /// </summary>
        /// <param name="value">Новое значение уровня входного сигнала.</param>
        private void UpdateAmp(float value)
        {
            amp = value;
        }

        /// <summary>
        /// Обработка новых входных данных.
        /// </summary>
        /// <param name="input">Входящий сигнал</param>
        /// <returns>Выходящий сигнал</returns>
        public float Process(float input)
        {
            ampFilter.Process();
            mixFilter.Process();
            asymmetryFilter.Process();

            input *= amp;
            input = lowPass.Process(input);
            if (mode == DistortionMode.None)
                return dcBlocker.Process(input);
            else
            {
                var output = currentDistortion.Process(input + dcOffset);
                output = dcBlocker.Process(output);
                return dryCoeff * input + wetCoeff * output;
            }
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            dcBlocker.SampleRate = newSampleRate;
            lowPass.SampleRate = newSampleRate;
            sampleRateReductor.SampleRate = newSampleRate;
        }
    }
}
