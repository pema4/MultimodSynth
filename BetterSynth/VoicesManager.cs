using System;
using System.Linq;
using System.Collections.Generic;
using Jacobi.Vst.Framework;
using WavesData;

namespace BetterSynth
{
    class VoicesManager : ManagerOfManagers
    {
        private const int MaxVoicesCount = 32;

        private Plugin plugin;
        private string parameterPrefix;
        private List<Voice> voices;
        private SortedSet<int> freeVoices = new SortedSet<int>();
        private Dictionary<byte, int> noteToVoiceMapping = new Dictionary<byte, int>();
        private List<byte> pressedNotes = new List<byte>();
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
                    OscAManager.SampleRate = value;
                    OscBManager.SampleRate = value;
                    FiltersManager.SampleRate = value;
                    OscAVolumeEnvelopeManager.SampleRate = value;
                    OscBVolumeEnvelopeManager.SampleRate = value;
                    FilterCutoffEnvelopeManager.SampleRate = value;
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

            freeVoices = new SortedSet<int>(Enumerable.Range(0, MaxVoicesCount));
            noteToVoiceMapping = new Dictionary<byte, int>();
            pressedNotes = new List<byte>();

            voices = new List<Voice>();

            for (int i = 0; i < MaxVoicesCount; ++i)
                voices.Add(CreateVoice());

            InitializaParameters();
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
            voice.SoundStop += (sender, e) => RemoveNote(voice.Note.NoteNo);

            return voice;
        }

        private void InitializaParameters()
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

            foreach (var voice in voices)
                voice.ModulationType = modulationType;
        }

        internal void PlayNote(MidiNote note)
        {
            //RemoveNote(note.NoteNo);

            int voiceIndex;

            if (freeVoices.Count == 0)
            {
                voiceIndex = noteToVoiceMapping[pressedNotes[0]];
                RemoveNote(pressedNotes[0]);
            }
            else
            {
                voiceIndex = freeVoices.Min;
                freeVoices.Remove(voiceIndex);
            }

            noteToVoiceMapping[note.NoteNo] = voiceIndex;
            pressedNotes.Add(note.NoteNo);
            voices[voiceIndex].PlayNote(note);
        }

        private void RemoveNote(byte noteNo)
        {
            if (noteToVoiceMapping.ContainsKey(noteNo))
            {
                int voiceIndex = noteToVoiceMapping[noteNo];
                freeVoices.Add(voiceIndex);
                noteToVoiceMapping.Remove(noteNo);
                pressedNotes.Remove(noteNo);
            }
        }

        internal void ReleaseNote(MidiNote note)
        {
            byte noteNo = note.NoteNo;
            if (noteToVoiceMapping.ContainsKey(noteNo))
            {
                int voiceIndex = noteToVoiceMapping[noteNo];
                voices[voiceIndex].TriggerRelease();
            }
        }

        internal float Process()
        {
            return voices.Select(voice => voice.Process()).Sum();
        }
    }
}
