using System.Linq;
using System.Collections.Generic;
using Jacobi.Vst.Framework;

namespace BetterSynth
{
    class VoicesManager : AudioComponentWithParameters
    {
        private const int MaxVoicesCount = 32;
        
        private List<Voice> voicesPool;
        private List<Voice> activeVoices;
        private Dictionary<byte, List<Voice>> noteToVoicesMapping;
        private SortedSet<int> freeVoicesIndices;
        private Voice.ModulationType modulationType;

        public OscillatorsManager OscAManager { get; set; }

        public OscillatorsManager OscBManager { get; set; }

        public FiltersManager FilterManager { get; set; }

        public EnvelopesManager OscAVolumeEnvelopeManager { get; set; }

        public EnvelopesManager OscBVolumeEnvelopeManager { get; set; }

        public EnvelopesManager FilterCutoffEnvelopeManager { get; set; }

        public VstParameterManager ModulationTypeManager { get; private set; }

        public VoicesManager(Plugin plugin, string parameterPrefix, string parameterCategory = "voices")
            : base(plugin, parameterPrefix, parameterCategory)
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

        protected override void InitializeParameters(ParameterFactory factory)
        {
            ModulationTypeManager = factory.CreateParameterManager(
                name: "_MT",
                valueChangedHandler: SetModulationType);
            CreateRedirection(ModulationTypeManager, nameof(ModulationTypeManager));
        }

        private void SetModulationType(float value)
        {
            if (value < 0.2f)
                modulationType = Voice.ModulationType.None;
            else if (value < 0.4f)
                modulationType = Voice.ModulationType.AmplitudeModulationA;
            else if (value < 0.6f)
                modulationType = Voice.ModulationType.AmplitudeModulationB;
            else if (value < 0.8f)
                modulationType = Voice.ModulationType.FrequencyModulationA;
            else
                modulationType = Voice.ModulationType.FrequencyModulationB;

            foreach (var voice in voicesPool)
                voice.Modulation = modulationType;
        }

        protected override void OnSampleRateChanged(float newSampleRate)
        {
            foreach (var voice in voicesPool)
                voice.SampleRate = newSampleRate;
        }

        private Voice CreateVoice()
        {
            var voiceOscA = OscAManager.CreateNewOscillator();
            var voiceOscB = OscBManager.CreateNewOscillator();
            var voiceFilter = FilterManager.CreateNewFilter();
            var oscAEnvelope = OscAVolumeEnvelopeManager.CreateNewEnvelope();
            var oscBEnvelope = OscBVolumeEnvelopeManager.CreateNewEnvelope();
            var filterEnvelope = FilterCutoffEnvelopeManager.CreateNewEnvelope();

            var voice = new Voice(Plugin, voiceOscA, voiceOscB, voiceFilter,
                oscAEnvelope, oscBEnvelope, filterEnvelope);

            voice.Modulation = modulationType;
            voice.SoundStop += (sender, e) => StopVoice(voice);

            return voice;
        }

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

        public void ReleaseNote(MidiNote note)
        {
            byte noteNo = note.NoteNo;

            if (noteToVoicesMapping.ContainsKey(noteNo))
            {
                foreach (var voice in noteToVoicesMapping[noteNo])
                    voice.TriggerRelease();
            }
        }

        private void StopVoice(Voice voice)
        {
            activeVoices.Remove(voice);

            byte noteNo = voice.Note.NoteNo;
            noteToVoicesMapping[noteNo].Remove(voice);

            int voiceIndex = voicesPool.IndexOf(voice);
            freeVoicesIndices.Add(voiceIndex);
        }

        public float Process()
        {
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
                    StopVoice(voice);
                else
                    i += 1;
            }
            
            return sum;
        }
    }
}
