using System;
using System.Linq;
using System.Collections.Generic;
using Jacobi.Vst.Framework;
using WavesData;

namespace BetterSynth
{
    class VoicesManager
    {
        #region vst parameters initialization

        public VstParameterManager AttackTime { get; private set; }

        public VstParameterManager DecayTime { get; private set; }

        public VstParameterManager SustainLevel { get; private set; }
        
        public VstParameterManager ReleaseTime { get; private set; }

        public VstParameterManager AttackCurve { get; private set; }

        public VstParameterManager DecayReleaseCurve { get; private set; }

        public VstParameterManager WaveTablePosition { get; private set; }

        private void InitializeParameters()
        {
            var factory = new ParameterFactory(plugin, "voice");

            AttackTime = factory.CreateParameterManager(
                name: "VOL_ATT",
                minValue: 0,
                maxValue: 10,
                defaultValue: 0.5f,
                valueChangedHandler: setAttackTime);

            DecayTime = factory.CreateParameterManager(
                name: "VOL_DEC",
                minValue: 0,
                maxValue: 10,
                defaultValue: 0,
                valueChangedHandler: setDecayTime);

            SustainLevel = factory.CreateParameterManager(
                name: "VOL_SUS",
                minValue: 0,
                maxValue: 1,
                defaultValue: 1,
                valueChangedHandler: setSustainLevel);

            ReleaseTime = factory.CreateParameterManager(
                name: "VOL_REL",
                minValue: 0,
                maxValue: 10,
                defaultValue: 0.5f,
                valueChangedHandler: setReleaseTime);

            AttackCurve = factory.CreateParameterManager(
                name: "VOL_A_C",
                minValue: 0,
                maxValue: 1,
                defaultValue: 1,
                valueChangedHandler: setAttackCurve);

            DecayReleaseCurve = factory.CreateParameterManager(
                name: "VOL_DR_C",
                minValue: 0,
                maxValue: 1,
                defaultValue: 1,
                valueChangedHandler: setDecayReleaseCurve);

            WaveTablePosition = factory.CreateParameterManager(
                name: "WT_POS",
                minValue: 0,
                maxValue: 1,
                defaultValue: 0.7f,
                valueChangedHandler: setWaveTablePosition);
        }

        private void setAttackTime(float value)
        {
            if (voices == null)
                return;
            float rate = plugin.AudioProcessor.SampleRate * value;
            foreach (var voice in voices)
                voice.VolumeEnvelope.AttackRate = rate;
        }

        private void setDecayTime(float value)
        {
            if (voices == null)
                return;
            float rate = plugin.AudioProcessor.SampleRate * value;
            foreach (var voice in voices)
                voice.VolumeEnvelope.DecayRate = rate;
        }

        private void setSustainLevel(float level)
        {
            if (voices == null)
                return;
            foreach (var voice in voices)
                voice.VolumeEnvelope.SustainLevel = level;
        }

        private void setReleaseTime(float value)
        {
            if (voices == null)
                return;
            float rate = plugin.AudioProcessor.SampleRate * value;
            foreach (var voice in voices)
                voice.VolumeEnvelope.ReleaseRate = rate;
        }

        private void setAttackCurve(float value)
        {
            if (voices == null)
                return;
            float targetRatio = 0.001f * ((float)Math.Exp(12 * (0.001f + 0.999f * value)) - 1);
            foreach (var voice in voices)
                voice.VolumeEnvelope.AttackTargetRatio = targetRatio;
        }

        private void setDecayReleaseCurve(float value)
        {
            if (voices == null)
                return;
            float targetRatio = 0.001f * ((float)Math.Exp(12 * (0.001f + 0.999f * value)) - 1);
            foreach (var voice in voices)
                voice.VolumeEnvelope.DecayReleaseTargetRatio = targetRatio;
        }

        private void setWaveTablePosition(float pos)
        {
            if (voices == null)
                return;
            foreach (var voice in voices)
                voice.Osc.WaveTablePos = pos;
        }

        private void setAllVoicesParameters()
        {
            setAttackTime(AttackTime.CurrentValue);
            setDecayTime(DecayTime.CurrentValue);
            setSustainLevel(SustainLevel.CurrentValue);
            setReleaseTime(ReleaseTime.CurrentValue);
            setAttackCurve(AttackCurve.CurrentValue);
            setDecayReleaseCurve(DecayReleaseCurve.CurrentValue);
            setWaveTablePosition(WaveTablePosition.CurrentValue);
        }

        #endregion vst parameters initialization

        private Plugin plugin;
        private Voice[] voices;
        private int voicesCount;

        public VoicesManager(Plugin plugin)
        {
            this.plugin = plugin;
            InitializeParameters();
            WaveTable wt = new WaveTable((x, y) => x + y, 0, 1);

            plugin.Opened += (sender, e) =>
            {
                VoicesCount = 32;
            };
        }

        public int VoicesCount
        {
            get => voicesCount;
            set
            {
                voicesCount = value;
                voices = new Voice[value];
                for (int i = 0; i < voices.Length; ++i)
                    voices[i] = CreateVoice();
                setAllVoicesParameters();
                freeVoices = new SortedSet<int>(Enumerable.Range(0, value));
                noteToVoiceMapping = new Dictionary<byte, int>();
                pressedNotes = new List<byte>();
            }
        }

        private Voice CreateVoice()
        {
            var voice = new Voice(plugin, x => RemoveNote(x.Note.NoteNo));
            return voice;
        }

        private SortedSet<int> freeVoices = new SortedSet<int>();
        private Dictionary<byte, int> noteToVoiceMapping = new Dictionary<byte, int>();
        private List<byte> pressedNotes = new List<byte>();

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
                voices[voiceIndex].Release();
            }
        }

        internal void Process(out float output)
        {
            output = voices.Select(voice => { voice.Process(out float x); return x; })
                           .Sum();
        }
    }
}
