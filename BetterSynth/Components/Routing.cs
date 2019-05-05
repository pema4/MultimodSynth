using Jacobi.Vst.Framework;

namespace BetterSynth
{
    /// <summary>
    /// Компонент плагина, предстающий собой всю цепочку создания обработки звука.
    /// </summary>
    class Routing : AudioComponentWithParameters
    {
        /// <summary>
        /// Буфер для хранения последних n семплов (для оверсэмплинга).
        /// </summary>
        private double[] samplesForOversampling = new double[8];

        /// <summary>
        /// Уровень громкости выходного сигнала.
        /// </summary>
        private float masterVolume;

        /// <summary>
        /// Фильтр низких частот, используемый для сглаживания параметра
        /// уровня громкости выходного сигнала.
        /// </summary>
        private ParameterFilter masterVolumeFilter;

        /// <summary>
        /// Менеджер всех голосов.
        /// </summary>
        public VoicesManager VoicesManager { get; private set; }

        /// <summary>
        /// Фильтр для снижения частоты дискретизации.
        /// </summary>
        public Downsampler Downsampler { get; private set; }

        /// <summary>
        /// Эффект дисторшн.
        /// </summary>
        public DistortionManager DistortionManager { get; private set; }

        /// <summary>
        /// Эффект дилэй.
        /// </summary>
        public DelayManager DelayManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром коэффициента повышения частоты дискретизации.
        /// </summary>
        public VstParameterManager OversamplingOrderManager { get; private set; }

        /// <summary>
        /// Объект, управляющий параметром уровня громкости выходного сигнала.
        /// </summary>
        public VstParameterManager MasterVolumeManager { get; private set; }

        /// <summary>
        /// Инициализирует новый объект класса Routing, принадлежащий переданному плагину
        /// и имеющий переданный префикс названия параметров.
        /// </summary>
        /// <param name="plugin">Плагин, которому принадлежит создаваемый объект.</param>
        /// <param name="parameterPrefix">Префикс названия параметров.</param>
        public Routing(Plugin plugin, string parameterPrefix = "M_") 
            : base(plugin, parameterPrefix)
        {
            VoicesManager = new VoicesManager(plugin, "M_");
            Downsampler = new Downsampler();
            DistortionManager = new DistortionManager(plugin, "DS_");
            DelayManager = new DelayManager(plugin, "DL_");

            plugin.MidiProcessor.NoteOn += MidiProcessor_NoteOn;
            plugin.MidiProcessor.NoteOff += MidiProcessor_NoteOff;

            InitializeParameters();
        }

        /// <summary>
        /// Инициализирует параметры с помощью переданной фабрики параметров.
        /// </summary>
        /// <param name="factory">Фабрика параметров</param>
        protected override void InitializeParameters(ParameterFactory factory)
        {
            OversamplingOrderManager = factory.CreateParameterManager(
                name: "OVSMP",
                valueChangedHandler: SetOversamplingOrder);

            MasterVolumeManager = factory.CreateParameterManager(
                name: "VOL",
                defaultValue: 0.5f,
                valueChangedHandler: x => masterVolumeFilter.SetTarget(x));
            masterVolumeFilter = new ParameterFilter(UpdateMasterVolume, 1);
        }

        /// <summary>
        /// Обработчик изменения коэффициента повышения частоты дискретизации.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetOversamplingOrder(float value)
        {
            int newOrder = Converters.ToOversamplingOrder(value);

            if (newOrder != Downsampler.Order)
            {
                Downsampler.Order = newOrder;
                UpdateSampleRates();
            }
        }

        /// <summary>
        /// Обработчик изменения "сглаженного" значения уровня громкости выходного сигнала.
        /// </summary>
        /// <param name="value">Новое значение уровня громкости.</param>
        private void UpdateMasterVolume(float value) => masterVolume = value;

        /// <summary>
        /// Обработчик события нажатия на клавишу.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MidiProcessor_NoteOn(object sender, MidiNoteEventArgs e)
        {
            VoicesManager.PlayNote(e.Note);
        }

        /// <summary>
        /// Обработчик события отпускания клавиши.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MidiProcessor_NoteOff(object sender, MidiNoteEventArgs e)
        {
            VoicesManager.ReleaseNote(e.Note);
        }

        /// <summary>
        /// Генерация новых выходных данных.
        /// </summary>
        /// <param name="left">Левый канал выходного сигнала.</param>
        /// <param name="right">Правый канал выходного сигнала.</param>
        public void Process(out float left, out float right)
        {
            // Сглаживание значений параметров.
            masterVolumeFilter.Process();

            for (int i = 0; i < Downsampler.Order; ++i)
            {
                var voicesOutput = VoicesManager.Process();
                var saturationOutput = DistortionManager.Process(voicesOutput);
                samplesForOversampling[i] = saturationOutput;
            }
            
            var output = (float)Downsampler.Process(samplesForOversampling);
            DelayManager.Process(output, output, out left, out right);
            left *= masterVolume;
            right *= masterVolume;
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            UpdateSampleRates();
            DelayManager.SampleRate = newSampleRate;
        }

        /// <summary>
        /// Обновляет частоты дискретизации компонентов, подверженных оверсэмплингу.
        /// </summary>
        private void UpdateSampleRates()
        {
            var scaledSampleRate = SampleRate * Downsampler.Order;
            VoicesManager.SampleRate = scaledSampleRate;
            DistortionManager.SampleRate = scaledSampleRate;
        }
    }
}