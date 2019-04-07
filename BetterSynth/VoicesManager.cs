using System;
using System.Linq;
using System.Collections.Generic;
using Jacobi.Vst.Framework;
using WavesData;

namespace BetterSynth
{
    class VoicesManager
    {
        private const int MaxVoicesCount = 32;

        private Plugin plugin;
        private List<Voice> voices;
        private OscillatorsManager oscAManager;
        private OscillatorsManager oscBManager;
        private FiltersManager filtersManager;
        private EnvelopesManager oscAVolumeEnvelopeManager;
        private EnvelopesManager oscBVolumeEnvelopeManager;
        private EnvelopesManager filterCutoffEnvelopeManager;
        private SortedSet<int> freeVoices = new SortedSet<int>();
        private Dictionary<byte, int> noteToVoiceMapping = new Dictionary<byte, int>();
        private List<byte> pressedNotes = new List<byte>();

        public VoicesManager(Plugin plugin)
        {
            this.plugin = plugin;

            oscAManager = new OscillatorsManager(plugin, "A");
            oscBManager = new OscillatorsManager(plugin, "B");
            filtersManager = new FiltersManager(plugin, "F");
            oscAVolumeEnvelopeManager = new EnvelopesManager(plugin, "A");
            oscBVolumeEnvelopeManager = new EnvelopesManager(plugin, "B");
            filterCutoffEnvelopeManager = new EnvelopesManager(plugin, "F");

            freeVoices = new SortedSet<int>(Enumerable.Range(0, MaxVoicesCount));
            noteToVoiceMapping = new Dictionary<byte, int>();
            pressedNotes = new List<byte>();

            voices = new List<Voice>();

            for (int i = 0; i < MaxVoicesCount; ++i)
                voices.Add(CreateVoice());
        }

        private Voice CreateVoice()
        {
            var voiceOscA = oscAManager.CreateNewOscillator();
            var voiceOscB = oscBManager.CreateNewOscillator();
            var voiceFilter = filtersManager.CreateNewFilter();
            var oscAEnvelope = oscAVolumeEnvelopeManager.CreateNewEnvelope();
            var oscBEnvelope = oscBVolumeEnvelopeManager.CreateNewEnvelope();
            var filterEnvelope = filterCutoffEnvelopeManager.CreateNewEnvelope();

            var voice = new Voice(plugin, voiceOscA, voiceOscB, voiceFilter,
                oscAEnvelope, oscBEnvelope, filterEnvelope);
            voice.SoundStop += (sender, e) => RemoveNote(voice.Note.NoteNo);

            return voice;
        }

        internal void PlayNote(MidiNote note)
        {
            RemoveNote(note.NoteNo);

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

        internal void Process(out float output)
        {
            output = voices.Select(voice => voice.Process())
                           .Sum();
        }
    }
}
