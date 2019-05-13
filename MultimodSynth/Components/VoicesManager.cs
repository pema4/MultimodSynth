using System.Linq;
using System.Collections.Generic;
using Jacobi.Vst.Framework;

namespace MultimodSynth
{
    /// <summary>
    /// Компонент плагина, управляющий всем голосами.
    /// </summary>
    class VoicesManager : AudioComponentWithParameters
    {
        /// <summary>
        /// Максимальное количество голосов.
        /// </summary>
        private const int MaxVoicesCount = 32;
        
        /// <summary>
        /// Список ссылок на все используемые и неиспользуемые голоса.
        /// </summary>
        private List<Voice> voicesPool;

        /// <summary>
        /// Список активных голосов.
        /// </summary>
        private List<Voice> activeVoices;

        /// <summary>
        /// Отображение номера ноты в список голосов, играющих эту ноту.
        /// </summary>
        private Dictionary<byte, List<Voice>> noteToVoicesMapping;

        /// <summary>
        /// Отсортированное множество номеров свободных голосов.
        /// </summary>
        private SortedSet<int> freeVoicesIndices;

        /// <summary>
        /// Текущий тип модуляции.
        /// </summary>
        private Voice.ModulationType modulationType;

        /// <summary>
        /// Менеджер всех осцилляторов A во всех голосах.
        /// </summary>
        public OscillatorsManager OscAManager { get; set; }

        /// <summary>
        /// Менеджер всех осцилляторов B во всех голосах.
        /// </summary>
        public OscillatorsManager OscBManager { get; set; }

        /// <summary>
        /// Менеджер фильра во всех голосах.
        /// </summary>
        public FiltersManager FilterManager { get; set; }

        /// <summary>
        /// Менеджер всех огибающих уровня осциллятора A во всех голосах.
        /// </summary>
        public EnvelopesManager OscAVolumeEnvelopeManager { get; set; }

        /// <summary>
        /// Менеджер всех огибающих уровня осциллятора B во всех голосах.
        /// </summary>
        public EnvelopesManager OscBVolumeEnvelopeManager { get; set; }

        /// <summary>
        /// Менеджер всех огибающих частоты среза фильтра во всех голосах.
        /// </summary>
        public EnvelopesManager FilterCutoffEnvelopeManager { get; set; }

        /// <summary>
        /// Объект, управляющий параметром типа модуляции.
        /// </summary>
        public VstParameterManager ModulationTypeManager { get; private set; }

        /// <summary>
        /// Инициализирует новый объект класса VoicesManager, принадлежащий переданному плагину
        /// и имеющий переданный префикс названия параметров.
        /// </summary>
        /// <param name="plugin">Плагин, которому принадлежит создаваемый объект.</param>
        /// <param name="parameterPrefix">Префикс названия параметров.</param>
        public VoicesManager(Plugin plugin, string parameterPrefix)
            : base(plugin, parameterPrefix)
        {
            OscAManager = new OscillatorsManager(plugin, "A_");
            OscAVolumeEnvelopeManager = new EnvelopesManager(plugin, "A_");
            OscBManager = new OscillatorsManager(plugin, "B_");
            OscBVolumeEnvelopeManager = new EnvelopesManager(plugin, "B_");
            FilterManager = new FiltersManager(plugin, "F_");
            FilterCutoffEnvelopeManager = new EnvelopesManager(plugin, "F_");

            freeVoicesIndices = new SortedSet<int>(Enumerable.Range(0, MaxVoicesCount));
            noteToVoicesMapping = new Dictionary<byte, List<Voice>>();
            activeVoices = new List<Voice>();
            voicesPool = new List<Voice>();
            for (int i = 0; i < MaxVoicesCount; ++i)
                voicesPool.Add(CreateVoice());

            InitializeParameters();
        }

        /// <summary>
        /// Инициализирует параметры с помощью переданной фабрики параметров.
        /// </summary>
        /// <param name="factory">Фабрика параметров</param>
        protected override void InitializeParameters(ParameterFactory factory)
        {
            ModulationTypeManager = factory.CreateParameterManager(
                name: "_MT",
                valueChangedHandler: SetModulationType);
        }

        /// <summary>
        /// Обработчик изменения типа модуляции.
        /// </summary>
        /// <param name="value">Нормированное новое значение параметра.</param>
        private void SetModulationType(float value)
        {
            modulationType = Converters.ToModulationType(value);

            foreach (var voice in voicesPool)
                voice.Modulation = modulationType;
        }

        /// <summary>
        /// Возвращает новый объект голоса, связанный с этим объектом.
        /// </summary>
        /// <returns></returns>
        private Voice CreateVoice()
        {
            var voiceOscA = OscAManager.CreateNewOscillator();
            var voiceOscB = OscBManager.CreateNewOscillator();
            var voiceFilter = FilterManager.CreateNewFilter();
            var oscAEnvelope = OscAVolumeEnvelopeManager.CreateNewEnvelope();
            var oscBEnvelope = OscBVolumeEnvelopeManager.CreateNewEnvelope();
            var filterEnvelope = FilterCutoffEnvelopeManager.CreateNewEnvelope();
            filterEnvelope.SetAmplitude(0);

            var voice = new Voice(Plugin, voiceOscA, voiceOscB, voiceFilter,
                oscAEnvelope, oscBEnvelope, filterEnvelope);

            voice.Modulation = modulationType;

            return voice;
        }

        /// <summary>
        /// Играет переданную ноту.
        /// </summary>
        /// <param name="note">Нота, которую необходимо проиграть.</param>
        public void PlayNote(MidiNote note)
        {
            Voice voice;

            if (freeVoicesIndices.Count == 0)
            {
                voice = activeVoices[0];
                StopVoice(voice);
            }
            else
            {
                voice = voicesPool[freeVoicesIndices.Min];
            }

            byte noteNo = note.NoteNo;
            
            if (!noteToVoicesMapping.ContainsKey(noteNo))
                noteToVoicesMapping[noteNo] = new List<Voice>();

            noteToVoicesMapping[noteNo].Add(voice);
            activeVoices.Add(voice);
            freeVoicesIndices.Remove(voicesPool.IndexOf(voice));
            voice.PlayNote(note);
        }

        /// <summary>
        /// Отпускает переданную ноту.
        /// </summary>
        /// <param name="note">Нота, которую необходимо отпустить.</param>
        public void ReleaseNote(MidiNote note)
        {
            byte noteNo = note.NoteNo;

            if (noteToVoicesMapping.ContainsKey(noteNo))
            {
                foreach (var voice in noteToVoicesMapping[noteNo])
                    voice.TriggerRelease();
            }
        }

        /// <summary>
        /// Останавливает переданный голос и помечает его как неиспользуемый.
        /// </summary>
        /// <param name="note">Голос, который необходимо остановить.</param>
        private void StopVoice(Voice voice)
        {
            activeVoices.Remove(voice);

            byte noteNo = voice.Note.NoteNo;
            noteToVoicesMapping[noteNo].Remove(voice);

            int voiceIndex = voicesPool.IndexOf(voice);
            freeVoicesIndices.Add(voiceIndex);
        }

        /// <summary>
        /// Генерация новых выходных данных.
        /// </summary>
        /// <returns>Выходной сигнал.</returns>
        public float Process()
        {
            // Вызовы этих методов обновляют значение параметров, которые подвержены сглаживанию.
            OscAManager.Process();
            OscBManager.Process();
            FilterManager.Process();
            OscAVolumeEnvelopeManager.Process();
            OscBVolumeEnvelopeManager.Process();
            FilterCutoffEnvelopeManager.Process();

            float sum = 0;
            for (int i = 0; i < activeVoices.Count;)
            {
                var voice = activeVoices[i];
                sum += voice.Process();
                if (!voice.IsActive)
                    // StopVoice помечает голос как свободный 
                    // и удаляет его из массива activeVoices.
                    // При этом инкрементировать значение i не нужно, потому что
                    // все элементы activeVoices сдвигаются влево
                    StopVoice(voice);
                else
                    i += 1;
            }
            return sum;
        }

        /// <summary>
        /// Обработчик изменения частоты дискретизации.
        /// </summary>
        /// <param name="newSampleRate">Новая частота дискретизации.</param>
        protected override void OnSampleRateChanged(float newSampleRate)
        {
            foreach (var voice in voicesPool)
                voice.SampleRate = newSampleRate;
        }
    }
}
