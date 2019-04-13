using System.Linq;
using System.Collections.Generic;
using Jacobi.Vst.Framework;

namespace BetterSynth
{
    class VoicesManager : ManagerOfManagers
    {
        private const int MaxVoicesCount = 32;

        private Plugin plugin;
        private string parameterPrefix;
        private List<Voice> voicesPool;
        private List<Voice> usedVoices;
        private Dictionary<byte, List<Voice>> noteToVoicesMapping;
        private SortedSet<int> freeVoicesIndices;
        private float sampleRate;
        private ModulationType modulationType;

        public OscillatorsManager OscAManager { get; set; }

        public OscillatorsManager OscBManager { get; set; }

        public FiltersManager FiltersManager { get; set; }

        public EnvelopesManager OscAVolumeEnvelopeManager { get; set; }

        public EnvelopesManager OscBVolumeEnvelopeManager { get; set; }

        public EnvelopesManager FilterCutoffEnvelopeManager { get; set; }

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                if (sampleRate != value)
                {
                    sampleRate = value;

                    foreach (var voice in voicesPool)
                        voice.SampleRate = sampleRate;
                }
            }
        }

        public VoicesManager(Plugin plugin, string parameterPrefix)
        {
            this.plugin = plugin;
            this.parameterPrefix = parameterPrefix;

            OscAManager = new OscillatorsManager(plugin, "A");
            OscBManager = new OscillatorsManager(plugin, "B");
            FiltersManager = new FiltersManager(plugin, "F");
            OscAVolumeEnvelopeManager = new EnvelopesManager(plugin, "A");
            OscBVolumeEnvelopeManager = new EnvelopesManager(plugin, "B");
            FilterCutoffEnvelopeManager = new EnvelopesManager(plugin, "F");

            freeVoicesIndices = new SortedSet<int>(Enumerable.Range(0, MaxVoicesCount));
            noteToVoicesMapping = new Dictionary<byte, List<Voice>>();
            usedVoices = new List<Voice>();

            voicesPool = new List<Voice>();

            for (int i = 0; i < MaxVoicesCount; ++i)
                voicesPool.Add(CreateVoice());

            InitializeParameters();
        }

        private Voice CreateVoice()
        {
            var voiceOscA = OscAManager.CreateNewOscillator();
            var voiceOscB = OscBManager.CreateNewOscillator();
            var voiceFilter = FiltersManager.CreateNewFilter();
            var oscAEnvelope = OscAVolumeEnvelopeManager.CreateNewEnvelope();
            var oscBEnvelope = OscBVolumeEnvelopeManager.CreateNewEnvelope();
            var filterEnvelope = FilterCutoffEnvelopeManager.CreateNewEnvelope();

            var voice = new Voice(plugin, voiceOscA, voiceOscB, voiceFilter,
                oscAEnvelope, oscBEnvelope, filterEnvelope);

            voice.ModulationType = modulationType;
            voice.SoundStop += (sender, e) => StopVoice(voice);

            return voice;
        }

        private void InitializeParameters()
        {
            var factory = new ParameterFactory(plugin, "voice");

            ModulationTypeManager = factory.CreateParameterManager(
                name: "_MT",
                valueChangedHandler: SetModulationType);
            CreateRedirection(ModulationTypeManager, nameof(ModulationTypeManager));
        }

        public VstParameterManager ModulationTypeManager { get; private set; }

        private void SetModulationType(float value)
        {
            if (value < 0.2f)
                modulationType = ModulationType.None;
            else if (value < 0.4f)
                modulationType = ModulationType.AmplitudeModulationA;
            else if (value < 0.6f)
                modulationType = ModulationType.AmplitudeModulationB;
            else if (value < 0.8f)
                modulationType = ModulationType.FrequencyModulationA;
            else
                modulationType = ModulationType.FrequencyModulationB;

            foreach (var voice in voicesPool)
                voice.ModulationType = modulationType;
        }

        public void PlayNote(MidiNote note)
        {
            Voice voice;

            if (freeVoicesIndices.Count == 0)
            {
                voice = usedVoices[0];
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
            usedVoices.Add(voice);
            freeVoicesIndices.Remove(voicesPool.IndexOf(voice));
            voice.PlayNote(note);
        }

        private void StopVoice(Voice voice)
        {
            usedVoices.Remove(voice);

            byte noteNo = voice.Note.NoteNo;
            noteToVoicesMapping[noteNo].Remove(voice);

            int voiceIndex = voicesPool.IndexOf(voice);
            freeVoicesIndices.Add(voiceIndex);
        }

        public void ReleaseNote(MidiNote note)
        {
            byte noteNo = note.NoteNo;

            if (noteToVoicesMapping.ContainsKey(noteNo))
            {
                foreach (var voice in noteToVoicesMapping[noteNo])
                    voice.TriggerRelease();
            }
        }

        public float Process()
        {
            OscAManager.Process();
            OscBManager.Process();
            OscAVolumeEnvelopeManager.Process();
            OscBVolumeEnvelopeManager.Process();

            float sum = 0;

            foreach (var voice in usedVoices.ToArray())
                sum += voice.Process();
            
            return sum;
        }
    }
}
